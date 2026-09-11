using System;
using System.Net.Sockets;
using System.Threading;
using enet;
using Enet;
using NativeCollections;

// ReSharper disable ALL

namespace ThreadedEnet
{
    /// <summary>
    ///     A thread-safe ENet host that runs the network servicing loop on a dedicated background thread
    ///     and communicates with the calling thread through lock-free event queues.
    /// </summary>
    public sealed class ThreadedManagedEnetHost : IDisposable
    {
        /// <summary>
        ///     Represents the callback invoked when a peer connects to the host.
        /// </summary>
        /// <param name="host">The host that raised the event.</param>
        /// <param name="uid">The unique identifier of the newly connected peer.</param>
        /// <param name="address">The network address of the newly connected peer.</param>
        /// <param name="command">The payload of the connect event.</param>
        /// <param name="events">The number of events dispatched so far during this polling pass.</param>
        /// <returns>
        ///     <see langword="true" /> to continue dispatching the remaining events;
        ///     otherwise, <see langword="false" /> to stop polling.
        /// </returns>
        public delegate bool OnConnected(ThreadedManagedEnetHost host, EnetUid uid, ENetAddress address, EnetIncomingCommandConnect command, nuint events);

        /// <summary>
        ///     Represents the callback invoked when a peer disconnects from the host.
        /// </summary>
        /// <param name="host">The host that raised the event.</param>
        /// <param name="uid">The unique identifier of the disconnected peer.</param>
        /// <param name="address">The network address of the disconnected peer.</param>
        /// <param name="command">The payload of the disconnect event.</param>
        /// <param name="events">The number of events dispatched so far during this polling pass.</param>
        /// <returns>
        ///     <see langword="true" /> to continue dispatching the remaining events;
        ///     otherwise, <see langword="false" /> to stop polling.
        /// </returns>
        public delegate bool OnDisconnected(ThreadedManagedEnetHost host, EnetUid uid, ENetAddress address, EnetIncomingCommandDisconnect command, nuint events);

        /// <summary>
        ///     Represents the callback invoked when a packet is received from a peer.
        ///     The callback owns the packet carried by <paramref name="command" /> and must dispose of it
        ///     when it is no longer needed.
        /// </summary>
        /// <param name="host">The host that raised the event.</param>
        /// <param name="uid">The unique identifier of the peer that sent the packet.</param>
        /// <param name="address">The network address of the peer that sent the packet.</param>
        /// <param name="command">The payload of the receive event, including the received packet.</param>
        /// <param name="events">The number of events dispatched so far during this polling pass.</param>
        /// <returns>
        ///     <see langword="true" /> to continue dispatching the remaining events;
        ///     otherwise, <see langword="false" /> to stop polling.
        /// </returns>
        public delegate bool OnReceived(ThreadedManagedEnetHost host, EnetUid uid, ENetAddress address, EnetIncomingCommandReceive command, nuint events);

        /// <summary>
        ///     The current host state shared with the background thread, or <see langword="null" /> when the host is not started.
        /// </summary>
        private UnsafeAtomicRef<EnetHostStates> _states;

        /// <summary>
        ///     Gets a value indicating whether the host has been started and has not been shut down yet.
        /// </summary>
        public bool IsStarted => _states.Load(Ordering.Acquire) != null;

        /// <summary>
        ///     Releases the resources used by the host by shutting it down.
        /// </summary>
#pragma warning disable CA1816 // Call GC.SuppressFinalize correctly
        public void Dispose() => Shutdown(uint.MaxValue);
#pragma warning restore CA1816 // Call GC.SuppressFinalize correctly

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        ~ThreadedManagedEnetHost() => Dispose();

        /// <summary>
        ///     Starts the host on a new background thread using the specified configuration.
        /// </summary>
        /// <param name="config">The configuration used to create and run the host.</param>
        /// <exception cref="ArgumentException">Thrown when host creation fails.</exception>
        /// <exception cref="SocketException">Thrown when host creation fails.</exception>
        public void Start(EnetHostConfig config)
        {
            var states = _states.Load(Ordering.Acquire);
            if (states != null)
                ThrowHelpers.ThrowHostAlreadyStartedException();

            var host = ManagedEnetHost.Create(config.LocalAddress, config.PeerCount, config.ChannelLimit, config.IncomingBandwidth, config.OutgoingBandwidth, config.Option);
            host.SetCompressor(config.Compressor);
            unsafe
            {
                host.SetChecksumCallback(config.ChecksumCallback);
                host.SetInterceptCallback(config.InterceptCallback);
            }

            host.SetMaxDuplicatePeers(config.MaxDuplicatePeers);
            host.SetIgnoreConnectRequests(config.IgnoreConnectRequests);

            states = new EnetHostStates();
            states.Host = host;
            states.Config = config;
            states.Threads.Store(1, Ordering.Relaxed);
            states.IncomingEvents = NativeSegQueue<EnetIncomingEvent>.Create();
            states.OutgoingEvents = NativeSegQueue<EnetOutgoingEvent>.Create();

            if (_states.CompareExchange(states, null) != null)
            {
                states.Host.Dispose();
                states.IncomingEvents.Dispose();
                states.OutgoingEvents.Dispose();
                ThrowHelpers.ThrowHostAlreadyStartedException();
            }

            new Thread(EnetHostRunner.ThreadedRunning) { IsBackground = true }.Start(states);
        }

        /// <summary>
        ///     Stops the host and releases its resources.
        ///     The background thread is signaled to terminate, and this method returns immediately
        ///     without waiting for the thread to finish.
        /// </summary>
        /// <param name="eventData">
        ///     The user data attached to the disconnect requests
        ///     sent to all peers when the host shuts down.
        /// </param>
        public void Shutdown(uint eventData)
        {
            var states = _states.Exchange(null);
            if (states == null)
                return;

            states.ShutdownEventData = eventData;
            EnetHostRunner.Exit(states);
        }

        /// <summary>
        ///     Polls the host for incoming events and dispatches them to the supplied callbacks.
        /// </summary>
        /// <param name="onConnected">The callback invoked for connect events.</param>
        /// <param name="onDisconnected">The callback invoked for disconnect events.</param>
        /// <param name="onReceived">The callback invoked for receive events.</param>
        /// <param name="maxEvents">The maximum number of events to dispatch in this call, or zero for no limit.</param>
        public void PollEvents(OnConnected onConnected, OnDisconnected onDisconnected, OnReceived onReceived, nuint maxEvents)
        {
            var states = _states.Load(Ordering.Acquire);
            if (states == null)
                return;

            if (!EnetHostRunner.TryEnter(states))
                return;

            try
            {
                nuint events = 0;
                var moveNext = true;
                while (moveNext && (maxEvents == 0 || events < maxEvents) && states.IncomingEvents.TryDequeue(out var @event))
                {
                    switch (@event.Type)
                    {
                        case EnetEventType.Connect:
                            if (!onConnected(this, @event.Uid, @event.Address, @event.Command.Connect, events))
                                moveNext = false;
                            break;

                        case EnetEventType.Disconnect:
                            if (!onDisconnected(this, @event.Uid, @event.Address, @event.Command.Disconnect, events))
                                moveNext = false;
                            break;

                        case EnetEventType.Receive:
                            if (!onReceived(this, @event.Uid, @event.Address, @event.Command.Receive, events))
                                moveNext = false;
                            break;
                    }

                    events += 1;
                }
            }
            finally
            {
                EnetHostRunner.Exit(states);
            }
        }

        /// <summary>
        ///     Queues a connection request to the specified remote address.
        /// </summary>
        /// <param name="address">The remote address to connect to.</param>
        /// <param name="channelCount">The number of channels to allocate for the connection.</param>
        /// <param name="eventData">The user data supplied to the receiving host.</param>
        public void Connect(ENetAddress address, nuint channelCount, uint eventData)
        {
            var states = _states.Load(Ordering.Acquire);
            if (states == null)
                return;

            var outgoing = new EnetOutgoingEvent();
            outgoing.Type = EnetOutgoingEventType.Connect;
            ref var connect = ref outgoing.Command.Connect;
            connect = new EnetOutgoingCommandConnect();
            connect.Address = address;
            connect.ChannelCount = channelCount;
            connect.EventData = eventData;

            if (!EnetHostRunner.TryEnter(states))
                return;

            states.OutgoingEvents.Enqueue(outgoing);
            EnetHostRunner.Exit(states);
        }

        /// <summary>
        ///     Queues a graceful disconnection request for the specified peer.
        /// </summary>
        /// <param name="uid">The unique identifier of the peer to disconnect.</param>
        /// <param name="eventData">The user data attached to the disconnection.</param>
        public void Disconnect(EnetUid uid, uint eventData)
        {
            var states = _states.Load(Ordering.Acquire);
            if (states == null)
                return;

            var outgoing = new EnetOutgoingEvent();
            outgoing.Type = EnetOutgoingEventType.Disconnect;
            ref var disconnect = ref outgoing.Command.Disconnect;
            disconnect.Uid = uid;
            disconnect.EventData = eventData;

            if (!EnetHostRunner.TryEnter(states))
                return;

            states.OutgoingEvents.Enqueue(outgoing);
            EnetHostRunner.Exit(states);
        }

        /// <summary>
        ///     Queues a packet to be sent to the specified peer.
        ///     Ownership of <paramref name="packet" /> is transferred to the host: the reference is reset to the
        ///     default value, and the caller must not use or dispose of the packet afterwards.
        /// </summary>
        /// <param name="uid">The unique identifier of the destination peer.</param>
        /// <param name="channelId">The channel on which to send the packet.</param>
        /// <param name="packet">The packet to send. The reference is reset to the default value on return.</param>
        public void Send(EnetUid uid, byte channelId, ref EnetPacket packet)
        {
            var internalPacket = packet;
            packet = default;
            var states = _states.Load(Ordering.Acquire);
            if (states == null)
            {
                internalPacket.Dispose();
                return;
            }

            var outgoing = new EnetOutgoingEvent();
            outgoing.Type = EnetOutgoingEventType.Send;
            ref var send = ref outgoing.Command.Send;
            send.Uid = uid;
            send.ChannelId = channelId;
            send.Packet = internalPacket;

            if (!EnetHostRunner.TryEnter(states))
            {
                internalPacket.Dispose();
                return;
            }

            states.OutgoingEvents.Enqueue(outgoing);
            EnetHostRunner.Exit(states);
        }

        /// <summary>
        ///     Queues a packet to be sent to all connected peers.
        ///     Ownership of <paramref name="packet" /> is transferred to the host: the reference is reset to the
        ///     default value, and the caller must not use or dispose of the packet afterwards.
        /// </summary>
        /// <param name="channelId">The channel on which to broadcast the packet.</param>
        /// <param name="packet">The packet to broadcast. The reference is reset to the default value on return.</param>
        public void Broadcast(byte channelId, ref EnetPacket packet)
        {
            var internalPacket = packet;
            packet = default;
            var states = _states.Load(Ordering.Acquire);
            if (states == null)
            {
                internalPacket.Dispose();
                return;
            }

            var outgoing = new EnetOutgoingEvent();
            outgoing.Type = EnetOutgoingEventType.Broadcast;
            ref var broadcast = ref outgoing.Command.Broadcast;
            broadcast.ChannelId = channelId;
            broadcast.Packet = internalPacket;

            if (!EnetHostRunner.TryEnter(states))
            {
                internalPacket.Dispose();
                return;
            }

            states.OutgoingEvents.Enqueue(outgoing);
            EnetHostRunner.Exit(states);
        }

        /// <summary>
        ///     Queues a packet to be sent to the selected peers identified by their incoming peer IDs.
        ///     Ownership of <paramref name="packet" /> is transferred to the host: the reference is reset to the
        ///     default value, and the caller must not use or dispose of the packet afterwards.
        /// </summary>
        /// <param name="channelId">The channel on which to broadcast the packet.</param>
        /// <param name="bitArray">
        ///     a bit array in which bit <c>i</c> (i.e. the bit at byte <c>i / 8</c>, bit offset <c>i % 8</c>)
        ///     selects the peer whose incoming peer identifier is <c>i</c>
        /// </param>
        /// <param name="packet">The packet to broadcast. The reference is reset to the default value on return.</param>
        public void BroadcastSelected(byte channelId, ReadOnlySpan<byte> bitArray, ref EnetPacket packet)
        {
            var internalPacket = packet;
            packet = default;
            var states = _states.Load(Ordering.Acquire);
            if (states == null)
            {
                internalPacket.Dispose();
                return;
            }

            var internalBitArray = new NativeArray<byte>(bitArray.Length);
            bitArray.CopyTo(internalBitArray);

            var outgoing = new EnetOutgoingEvent();
            outgoing.Type = EnetOutgoingEventType.BroadcastSelected;
            ref var broadcastSelected = ref outgoing.Command.BroadcastSelected;
            broadcastSelected.ChannelId = channelId;
            broadcastSelected.BitArray = internalBitArray;
            broadcastSelected.Packet = internalPacket;

            if (!EnetHostRunner.TryEnter(states))
            {
                internalBitArray.Dispose();
                internalPacket.Dispose();
                return;
            }

            states.OutgoingEvents.Enqueue(outgoing);
            EnetHostRunner.Exit(states);
        }

        /// <summary>
        ///     Queues a ping request to the specified address.
        ///     Pings are typically used for NAT hole punching or to elicit a response from a remote host.
        /// </summary>
        /// <param name="address">The destination address to ping.</param>
        public void Ping(ENetAddress address)
        {
            var states = _states.Load(Ordering.Acquire);
            if (states == null)
                return;

            var outgoing = new EnetOutgoingEvent();
            outgoing.Type = EnetOutgoingEventType.Ping;
            ref var ping = ref outgoing.Command.Ping;
            ping.Address = address;

            if (!EnetHostRunner.TryEnter(states))
                return;

            states.OutgoingEvents.Enqueue(outgoing);
            EnetHostRunner.Exit(states);
        }

        /// <summary>
        ///     Queues a command to set the ping interval for the specified peer.
        ///     The actual change is applied on the background thread.
        /// </summary>
        /// <param name="uid">The unique identifier of the target peer.</param>
        /// <param name="pingInterval">
        ///     The desired ping interval in milliseconds. A value of <c>0</c> instructs ENet to use its default interval
        ///     (<c>ENET_PEER_PING_INTERVAL</c>).
        /// </param>
        public void SetPingInterval(EnetUid uid, uint pingInterval)
        {
            var states = _states.Load(Ordering.Acquire);
            if (states == null)
                return;

            var outgoing = new EnetOutgoingEvent();
            outgoing.Type = EnetOutgoingEventType.SetPingInterval;
            ref var setPingInterval = ref outgoing.Command.SetPingInterval;
            setPingInterval.Uid = uid;
            setPingInterval.PingInterval = pingInterval;

            if (!EnetHostRunner.TryEnter(states))
                return;

            states.OutgoingEvents.Enqueue(outgoing);
            EnetHostRunner.Exit(states);
        }

        /// <summary>
        ///     Queues a command to set the timeout parameters for the specified peer.
        ///     The actual change is applied on the background thread.
        /// </summary>
        /// <param name="uid">The unique identifier of the target peer.</param>
        /// <param name="timeoutLimit">
        ///     The timeout limit in milliseconds. A value of <c>0</c> uses the ENet default
        ///     (<c>ENET_PEER_TIMEOUT_LIMIT</c>).
        /// </param>
        /// <param name="timeoutMinimum">
        ///     The minimum timeout in milliseconds. A value of <c>0</c> uses the ENet default
        ///     (<c>ENET_PEER_TIMEOUT_MINIMUM</c>).
        /// </param>
        /// <param name="timeoutMaximum">
        ///     The maximum timeout in milliseconds. A value of <c>0</c> uses the ENet default
        ///     (<c>ENET_PEER_TIMEOUT_MAXIMUM</c>).
        /// </param>
        public void SetTimeout(EnetUid uid, uint timeoutLimit, uint timeoutMinimum, uint timeoutMaximum)
        {
            var states = _states.Load(Ordering.Acquire);
            if (states == null)
                return;

            var outgoing = new EnetOutgoingEvent();
            outgoing.Type = EnetOutgoingEventType.SetTimeout;
            ref var setTimeout = ref outgoing.Command.SetTimeout;
            setTimeout.Uid = uid;
            setTimeout.TimeoutLimit = timeoutLimit;
            setTimeout.TimeoutMinimum = timeoutMinimum;
            setTimeout.TimeoutMaximum = timeoutMaximum;

            if (!EnetHostRunner.TryEnter(states))
                return;

            states.OutgoingEvents.Enqueue(outgoing);
            EnetHostRunner.Exit(states);
        }

        /// <summary>
        ///     Queues a command to configure the throttle parameters for the specified peer.
        ///     The actual change is applied on the background thread.
        /// </summary>
        /// <param name="uid">The unique identifier of the target peer.</param>
        /// <param name="interval">
        ///     The measurement interval in milliseconds over which the lowest mean RTT is tracked.
        ///     A value of <c>0</c> uses the ENet default (<c>ENET_PEER_PACKET_THROTTLE_INTERVAL</c>).
        /// </param>
        /// <param name="acceleration">
        ///     The rate at which the throttle probability increases as the mean RTT declines.
        /// </param>
        /// <param name="deceleration">
        ///     The rate at which the throttle probability decreases as the mean RTT rises.
        /// </param>
        public void ConfigureThrottle(EnetUid uid, uint interval, uint acceleration, uint deceleration)
        {
            var states = _states.Load(Ordering.Acquire);
            if (states == null)
                return;

            var outgoing = new EnetOutgoingEvent();
            outgoing.Type = EnetOutgoingEventType.ConfigureThrottle;
            ref var configureThrottle = ref outgoing.Command.ConfigureThrottle;
            configureThrottle.Uid = uid;
            configureThrottle.Interval = interval;
            configureThrottle.Acceleration = acceleration;
            configureThrottle.Deceleration = deceleration;

            if (!EnetHostRunner.TryEnter(states))
                return;

            states.OutgoingEvents.Enqueue(outgoing);
            EnetHostRunner.Exit(states);
        }
    }
}