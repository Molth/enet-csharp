using Enet;
using NativeCollections;
using NativeSockets;

// ReSharper disable ALL

namespace ThreadedEnet
{
    /// <summary>
    ///     Implements the background worker loop that services the host, processes queued commands,
    ///     and forwards host events to the incoming event queue.
    /// </summary>
    internal static class EnetHostRunner
    {
        /// <summary>
        ///     Attempts to increment the active thread count for the given host state.
        /// </summary>
        /// <param name="states">The host state that tracks the active thread count.</param>
        /// <returns>
        ///     <see langword="true" /> if the thread count was successfully incremented;
        ///     otherwise, <see langword="false" /> if the host is already shutting down (thread count is zero).
        /// </returns>
        /// <remarks>
        ///     This method uses a spin‑wait loop to atomically increment the thread counter.
        ///     If the current count is zero, it returns <see langword="false" /> to indicate that
        ///     no further work should be performed (shutdown in progress).
        /// </remarks>
        public static bool TryEnter(EnetHostStates states)
        {
            var spinWait = new UnsafeSpinWait();
            while (true)
            {
                var threads = states.Threads.Load(Ordering.Acquire);
                if (threads == 0)
                    return false;

                if (states.Threads.CompareExchange(threads + 1, threads) == threads)
                    return true;

                spinWait.SpinOnce(-1);
            }
        }

        /// <summary>
        ///     Decrements the active thread count for the given host state.
        /// </summary>
        /// <param name="states">The host state that tracks the active thread count.</param>
        /// <remarks>
        ///     This method should be called when a background thread exits to release its reference
        ///     on the host state. It atomically decreases the thread counter.
        /// </remarks>
        public static void Exit(EnetHostStates states) => states.Threads.Sub(1);

        /// <summary>
        ///     The entry point of the background thread.
        ///     Services the host until the associated state is shut down, then disconnects all peers
        ///     and releases the host and the event queues.
        /// </summary>
        /// <param name="obj">The <see cref="EnetHostStates" /> instance that drives this worker.</param>
        public static void ThreadedRunning(object? obj)
        {
            if (obj is not EnetHostStates states || states.Host == null)
            {
                ThrowHelpers.ThrowArgumentNullException(ExceptionArgument._dummy);
                return;
            }

            var host = states.Host;
            ref readonly var config = ref states.Config;

            var uids = new NativeArray<EnetUid>((int)config.PeerCount);
            for (var i = 0; i < (int)config.PeerCount; ++i)
            {
                ref var uid = ref uids[i];
                uid = new EnetUid((ulong)i);
            }

            EnetPeer peer;

            var spinWait = new UnsafeSpinWait();

            while (states.Threads.Load(Ordering.Acquire) > 0)
            {
                SelectModeFlags shouldSpinOnce = 0;

                while (states.OutgoingEvents.TryDequeue(out var outgoing))
                {
                    shouldSpinOnce |= SelectModeFlags.SelectWrite;

                    switch (outgoing.Type)
                    {
                        case EnetOutgoingEventType.Connect:
                            ref var connect = ref outgoing.Command.Connect;
                            var tryConnected = host.TryConnect(connect.Address, connect.ChannelCount, connect.EventData, out peer);
                            if (!tryConnected)
                            {
                                var incoming = new EnetIncomingEvent();
                                incoming.Type = EnetEventType.Disconnect;
                                incoming.Address = connect.Address;
                                incoming.Command.Disconnect.EventData = connect.EventData;
                                states.IncomingEvents.Enqueue(incoming);
                            }

                            break;

                        case EnetOutgoingEventType.Disconnect:
                            ref var disconnect = ref outgoing.Command.Disconnect;
                            if (uids.Validate(disconnect.Uid))
                            {
                                host.TryGetPeer(disconnect.Uid.IncomingPeerId, out peer);
                                peer.Disconnect(disconnect.EventData);
                            }

                            break;

                        case EnetOutgoingEventType.Send:
                            ref var send = ref outgoing.Command.Send;
                            if (uids.Validate(send.Uid))
                            {
                                host.TryGetPeer(send.Uid.IncomingPeerId, out peer);
                                if (!peer.TrySend(send.ChannelId, ref send.Packet))
                                    send.Packet.Dispose();
                            }
                            else
                            {
                                send.Packet.Dispose();
                            }

                            break;

                        case EnetOutgoingEventType.Broadcast:
                            ref var broadcast = ref outgoing.Command.Broadcast;
                            host.Broadcast(broadcast.ChannelId, ref broadcast.Packet);
                            break;

                        case EnetOutgoingEventType.Ping:
                            ref var ping = ref outgoing.Command.Ping;
                            host.TryPing(ping.Address);
                            break;

                        case EnetOutgoingEventType.SetPingInterval:
                            ref var setPingInterval = ref outgoing.Command.SetPingInterval;
                            if (uids.Validate(setPingInterval.Uid))
                            {
                                host.TryGetPeer(setPingInterval.Uid.IncomingPeerId, out peer);
                                peer.SetPingInterval(setPingInterval.PingInterval);
                            }

                            break;

                        case EnetOutgoingEventType.SetTimeout:
                            ref var setTimeout = ref outgoing.Command.SetTimeout;
                            if (uids.Validate(setTimeout.Uid))
                            {
                                host.TryGetPeer(setTimeout.Uid.IncomingPeerId, out peer);
                                peer.SetTimeout(setTimeout.TimeoutLimit, setTimeout.TimeoutMinimum, setTimeout.TimeoutMaximum);
                            }

                            break;

                        case EnetOutgoingEventType.ConfigureThrottle:
                            ref var configureThrottle = ref outgoing.Command.ConfigureThrottle;
                            if (uids.Validate(configureThrottle.Uid))
                            {
                                host.TryGetPeer(configureThrottle.Uid.IncomingPeerId, out peer);
                                peer.ConfigureThrottle(configureThrottle.Interval, configureThrottle.Acceleration, configureThrottle.Deceleration);
                            }

                            break;
                    }
                }

                if (host.Service(config.ServiceTimeout, out var @event) > 0)
                {
                    shouldSpinOnce |= SelectModeFlags.SelectRead;

                    while (true)
                    {
                        peer = @event.Peer;
                        ref var uid = ref uids[peer.IncomingPeerId];
                        var incoming = new EnetIncomingEvent();
                        incoming.Address = peer.Address;
                        switch (@event.Type)
                        {
                            case EnetEventType.Connect:
                                uid = uid.Next();
                                incoming.Type = EnetEventType.Connect;
                                incoming.Uid = uid;
                                ref var connect = ref incoming.Command.Connect;
                                connect.ChannelCount = peer.ChannelCount;
                                connect.EventData = peer.EventData;
                                states.IncomingEvents.Enqueue(incoming);
                                break;

                            case EnetEventType.Disconnect:
                                incoming.Type = EnetEventType.Disconnect;
                                incoming.Uid = uid;
                                ref var disconnect = ref incoming.Command.Disconnect;
                                disconnect.EventData = peer.EventData;
                                states.IncomingEvents.Enqueue(incoming);
                                break;

                            case EnetEventType.Receive:
                                incoming.Type = EnetEventType.Receive;
                                incoming.Uid = uid;
                                ref var receive = ref incoming.Command.Receive;
                                receive.Packet = @event.Packet;
                                states.IncomingEvents.Enqueue(incoming);
                                break;
                        }

                        if (host.CheckEvents(out @event) <= 0)
                            break;
                    }
                }

                if (shouldSpinOnce == 0)
                    spinWait.SpinOnce();
                else if ((shouldSpinOnce & SelectModeFlags.SelectRead) != 0)
                    spinWait.Reset();
                else if ((shouldSpinOnce & SelectModeFlags.SelectWrite) != 0)
                    spinWait.SpinOnce(-1);
            }

            uids.Dispose();

            host.Flush();

            for (nuint i = 0; i < config.PeerCount; ++i)
            {
                host.TryGetPeer((ushort)i, out peer);
                peer.DisconnectNow(states.ShutdownEventData);
            }

            host.Dispose();

            while (states.OutgoingEvents.TryDequeue(out var outgoing))
            {
                if (outgoing.Type == EnetOutgoingEventType.Send)
                    outgoing.Command.Send.Packet.Dispose();
            }

            states.OutgoingEvents.Dispose();

            while (states.IncomingEvents.TryDequeue(out var incoming))
            {
                if (incoming.Type == EnetEventType.Receive)
                    incoming.Command.Receive.Packet.Dispose();
            }

            states.IncomingEvents.Dispose();
        }
    }
}