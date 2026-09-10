using System;
using System.Runtime.CompilerServices;
using enet;

// ReSharper disable ALL

namespace Enet
{
    /// <summary>
    ///     An ENet peer which data packets may be sent or received from.
    /// </summary>
    /// <remarks>
    ///     No fields should be modified unless otherwise specified.
    /// </remarks>
    public readonly unsafe struct EnetPeer : IIsCreated
    {
        /// <summary>
        ///     Gets the handle to the underlying object.
        /// </summary>
        private readonly ENetPeer* _handle;

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        public EnetPeer(ENetPeer* handle) => _handle = handle;

        /// <summary>
        ///     Gets the handle to the underlying object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ENetPeer* GetInner() => _handle;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => _handle != null;

        /// <summary>
        ///     Validates that the instance has been properly allocated and initialized.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if the instance is not created
        ///     (i.e., the underlying native handle is <see langword="null" />).
        /// </exception>
        public void Validate() => ThrowHelpers.ThrowIfNotCreated(IsCreated, ExceptionArgument._dummy);

        /// <summary>
        ///     Gets the host that manages this peer.
        /// </summary>
        public EnetHost Host => new(_handle->host);

        /// <summary>
        ///     Gets the remote peer identifier used for outgoing communication.
        /// </summary>
        public ushort OutgoingPeerId => _handle->outgoingPeerID;

        /// <summary>
        ///     Gets the local identifier assigned to this peer by the host.
        ///     This ID is unique within the host's peer list.
        /// </summary>
        public ushort IncomingPeerId => _handle->incomingPeerID;

        /// <summary>
        ///     Gets the unique connection identifier for this peer.
        /// </summary>
        public uint ConnectId => _handle->connectID;

        /// <summary>
        ///     Gets the session identifier used for outgoing communication.
        /// </summary>
        public byte OutgoingSessionId => _handle->outgoingSessionID;

        /// <summary>
        ///     Gets the session identifier used for incoming communication.
        /// </summary>
        public byte IncomingSessionId => _handle->incomingSessionID;

        /// <summary>
        ///     Gets the Internet address of the peer.
        /// </summary>
        public ENetAddress Address => _handle->address;

        /// <summary>
        ///     Gets the application private data pointer, which may be freely modified.
        /// </summary>
        public void* Data => _handle->data;

        /// <summary>
        ///     Gets the current state of the peer.
        /// </summary>
        public EnetPeerState State => (EnetPeerState)_handle->state;

        /// <summary>
        ///     Gets the number of channels allocated for communication with the peer.
        /// </summary>
        public nuint ChannelCount => _handle->channelCount;

        /// <summary>
        ///     Gets the downstream bandwidth of the client in bytes/second.
        /// </summary>
        public uint IncomingBandwidth => _handle->incomingBandwidth;

        /// <summary>
        ///     Gets the upstream bandwidth of the client in bytes/second.
        /// </summary>
        public uint OutgoingBandwidth => _handle->outgoingBandwidth;

        /// <summary>
        ///     Gets the time at which incoming bandwidth throttling was last updated.
        /// </summary>
        public uint IncomingBandwidthThrottleEpoch => _handle->incomingBandwidthThrottleEpoch;

        /// <summary>
        ///     Gets the time at which outgoing bandwidth throttling was last updated.
        /// </summary>
        public uint OutgoingBandwidthThrottleEpoch => _handle->outgoingBandwidthThrottleEpoch;

        /// <summary>
        ///     Gets the total amount of incoming data received from the peer.
        /// </summary>
        public uint IncomingDataTotal => _handle->incomingDataTotal;

        /// <summary>
        ///     Gets the total amount of outgoing data sent to the peer.
        /// </summary>
        public uint OutgoingDataTotal => _handle->outgoingDataTotal;

        /// <summary>
        ///     Gets the time (in milliseconds) at which the last packet was sent to the peer.
        /// </summary>
        public uint LastSendTime => _handle->lastSendTime;

        /// <summary>
        ///     Gets the time (in milliseconds) at which the last packet was received from the peer.
        /// </summary>
        public uint LastReceiveTime => _handle->lastReceiveTime;

        /// <summary>
        ///     Gets the next scheduled timeout time for the peer.
        /// </summary>
        public uint NextTimeout => _handle->nextTimeout;

        /// <summary>
        ///     Gets the earliest timeout time among pending commands.
        /// </summary>
        public uint EarliestTimeout => _handle->earliestTimeout;

        /// <summary>
        ///     Gets the epoch at which packet loss statistics were last updated.
        /// </summary>
        public uint PacketLossEpoch => _handle->packetLossEpoch;

        /// <summary>
        ///     Gets the total number of packets sent to the peer.
        /// </summary>
        public uint PacketsSent => _handle->packetsSent;

        /// <summary>
        ///     Gets the total number of packets lost from the peer.
        /// </summary>
        public uint PacketsLost => _handle->packetsLost;

        /// <summary>
        ///     Gets the mean packet loss of reliable packets as a ratio with respect to
        ///     the constant <c>ENET_PEER_PACKET_LOSS_SCALE</c>.
        /// </summary>
        public uint PacketLoss => _handle->packetLoss;

        /// <summary>
        ///     Gets the variance of the packet loss.
        /// </summary>
        public uint PacketLossVariance => _handle->packetLossVariance;

        /// <summary>
        ///     Gets the current packet throttle value, representing the probability that
        ///     an unreliable packet should be sent.
        /// </summary>
        public uint PacketThrottle => _handle->packetThrottle;

        /// <summary>
        ///     Gets the maximum packet throttle limit.
        /// </summary>
        public uint PacketThrottleLimit => _handle->packetThrottleLimit;

        /// <summary>
        ///     Gets the current packet throttle counter.
        /// </summary>
        public uint PacketThrottleCounter => _handle->packetThrottleCounter;

        /// <summary>
        ///     Gets the epoch at which packet throttle statistics were last updated.
        /// </summary>
        public uint PacketThrottleEpoch => _handle->packetThrottleEpoch;

        /// <summary>
        ///     Gets the packet throttle acceleration factor.
        /// </summary>
        public uint PacketThrottleAcceleration => _handle->packetThrottleAcceleration;

        /// <summary>
        ///     Gets the packet throttle deceleration factor.
        /// </summary>
        public uint PacketThrottleDeceleration => _handle->packetThrottleDeceleration;

        /// <summary>
        ///     Gets the interval over which throttle statistics are measured, in milliseconds.
        /// </summary>
        public uint PacketThrottleInterval => _handle->packetThrottleInterval;

        /// <summary>
        ///     Gets the interval between ping probes, in milliseconds.
        /// </summary>
        public uint PingInterval => _handle->pingInterval;

        /// <summary>
        ///     Gets the timeout limit for the peer.
        /// </summary>
        public uint TimeoutLimit => _handle->timeoutLimit;

        /// <summary>
        ///     Gets the minimum timeout value.
        /// </summary>
        public uint TimeoutMinimum => _handle->timeoutMinimum;

        /// <summary>
        ///     Gets the maximum timeout value.
        /// </summary>
        public uint TimeoutMaximum => _handle->timeoutMaximum;

        /// <summary>
        ///     Gets the last recorded round trip time (RTT) in milliseconds.
        /// </summary>
        public uint LastRoundTripTime => _handle->lastRoundTripTime;

        /// <summary>
        ///     Gets the lowest recorded round trip time (RTT) in milliseconds.
        /// </summary>
        public uint LowestRoundTripTime => _handle->lowestRoundTripTime;

        /// <summary>
        ///     Gets the variance of the last recorded round trip time.
        /// </summary>
        public uint LastRoundTripTimeVariance => _handle->lastRoundTripTimeVariance;

        /// <summary>
        ///     Gets the highest recorded round trip time variance.
        /// </summary>
        public uint HighestRoundTripTimeVariance => _handle->highestRoundTripTimeVariance;

        /// <summary>
        ///     Gets the mean round trip time (RTT) in milliseconds between sending a reliable
        ///     packet and receiving its acknowledgement.
        /// </summary>
        public uint RoundTripTime => _handle->roundTripTime;

        /// <summary>
        ///     Gets the variance of the mean round trip time.
        /// </summary>
        public uint RoundTripTimeVariance => _handle->roundTripTimeVariance;

        /// <summary>
        ///     Gets the maximum transmission unit for the peer.
        /// </summary>
        public uint Mtu => _handle->mtu;

        /// <summary>
        ///     Gets the window size for reliable data transmission.
        /// </summary>
        public uint WindowSize => _handle->windowSize;

        /// <summary>
        ///     Gets the amount of reliable data currently in transit (sent but not yet acknowledged).
        /// </summary>
        public uint ReliableDataInTransit => _handle->reliableDataInTransit;

        /// <summary>
        ///     Gets the event data associated with the peer.
        /// </summary>
        public uint EventData => _handle->eventData;

        /// <summary>
        ///     Gets the total amount of data waiting to be sent to the peer.
        /// </summary>
        public nuint TotalWaitingData => _handle->totalWaitingData;

        /// <summary>
        ///     Queues a packet to be sent to this peer.
        /// </summary>
        /// <param name="channelId">Channel on which to send.</param>
        /// <param name="packet">
        ///     The packet to send.
        ///     <para>
        ///         <b>On success</b>: ENet assumes ownership of the underlying native handle; the <paramref name="packet" />
        ///         reference
        ///         will be reset to a default (invalid) state, and the caller must not use it further.
        ///     </para>
        ///     <para>
        ///         <b>On failure</b>: the caller retains full ownership and is responsible for destroying the packet (e.g., via
        ///         <c>Dispose</c>).
        ///     </para>
        /// </param>
        /// <returns>true on success, false on failure.</returns>
        /// <remarks>
        ///     On success, ENet will assume ownership of the packet.
        ///     On failure, the caller must still destroy the packet.
        /// </remarks>
        public bool TrySend(byte channelId, ref EnetPacket packet)
        {
            var result = ENET_API.enet_peer_send(_handle, channelId, packet.GetInner()) == 0;
            if (result)
                packet = default;
            return result;
        }

        /// <summary>
        ///     Attempts to dequeue any incoming queued packet from this peer.
        /// </summary>
        /// <param name="channelId">When successful, holds the channel ID of the channel the packet was received on.</param>
        /// <param name="packet">
        ///     A pointer to the packet, or <see langword="null" /> if there are no available incoming queued
        ///     packets.
        /// </param>
        /// <returns>There are available incoming queued packets.</returns>
        public bool TryReceive(out byte channelId, out EnetPacket packet)
        {
            Unsafe.SkipInit(out byte internalChannelId);
            var internalPacket = ENET_API.enet_peer_receive(_handle, &internalChannelId);
            if (internalPacket == null)
            {
                channelId = 0;
                packet = default;
                return false;
            }

            channelId = internalChannelId;
            packet = new EnetPacket(internalPacket);
            return true;
        }

        /// <summary>
        ///     Sends a ping request to this peer.
        /// </summary>
        /// <remarks>
        ///     Ping requests factor into the mean round trip time. ENet automatically pings all connected peers at regular
        ///     intervals,
        ///     but this function may be called to ensure more frequent ping requests.
        /// </remarks>
        public void Ping() => ENET_API.enet_peer_ping(_handle);

        /// <summary>
        ///     Sets the interval at which pings will be sent to this peer.
        /// </summary>
        /// <param name="pingInterval">The interval in milliseconds; defaults to ENET_PEER_PING_INTERVAL if 0.</param>
        public void SetPingInterval(uint pingInterval) => ENET_API.enet_peer_ping_interval(_handle, pingInterval);

        /// <summary>
        ///     Sets the timeout parameters for this peer.
        /// </summary>
        /// <param name="timeoutLimit">The timeout limit; defaults to ENET_PEER_TIMEOUT_LIMIT if 0.</param>
        /// <param name="timeoutMinimum">The timeout minimum; defaults to ENET_PEER_TIMEOUT_MINIMUM if 0.</param>
        /// <param name="timeoutMaximum">The timeout maximum; defaults to ENET_PEER_TIMEOUT_MAXIMUM if 0.</param>
        public void SetTimeout(uint timeoutLimit, uint timeoutMinimum, uint timeoutMaximum) => ENET_API.enet_peer_timeout(_handle, timeoutLimit, timeoutMinimum, timeoutMaximum);

        /// <summary>
        ///     Forcefully disconnects this peer. The foreign host is not notified and will timeout on its connection.
        /// </summary>
        public void Reset() => ENET_API.enet_peer_reset(_handle);

        /// <summary>
        ///     Requests a disconnection from this peer.
        /// </summary>
        /// <param name="data">Data describing the disconnection.</param>
        /// <remarks>
        ///     An ENET_EVENT_DISCONNECT event will be generated by enet_host_service() once the disconnection is complete.
        /// </remarks>
        public void Disconnect(uint data) => ENET_API.enet_peer_disconnect(_handle, data);

        /// <summary>
        ///     Forces an immediate disconnection from this peer. No disconnect event will be generated.
        /// </summary>
        /// <param name="data">Data describing the disconnection.</param>
        public void DisconnectNow(uint data) => ENET_API.enet_peer_disconnect_now(_handle, data);

        /// <summary>
        ///     Requests a disconnection from this peer, but only after all queued outgoing packets are sent.
        /// </summary>
        /// <param name="data">Data describing the disconnection.</param>
        /// <remarks>
        ///     An ENET_EVENT_DISCONNECT event will be generated by enet_host_service() once the disconnection is complete.
        /// </remarks>
        public void DisconnectLater(uint data) => ENET_API.enet_peer_disconnect_later(_handle, data);

        /// <summary>
        ///     Configures the throttle parameters for this peer.
        /// </summary>
        /// <param name="interval">
        ///     Interval in milliseconds over which to measure lowest mean RTT; default is
        ///     ENET_PEER_PACKET_THROTTLE_INTERVAL.
        /// </param>
        /// <param name="acceleration">Rate at which to increase the throttle probability as mean RTT declines.</param>
        /// <param name="deceleration">Rate at which to decrease the throttle probability as mean RTT increases.</param>
        public void ConfigureThrottle(uint interval, uint acceleration, uint deceleration) => ENET_API.enet_peer_throttle_configure(_handle, interval, acceleration, deceleration);

        /// <summary>
        ///     Sets the application private data pointer associated with this peer.
        ///     This pointer can be used to store arbitrary user data and may be freely modified.
        /// </summary>
        /// <param name="data">The user data pointer to set.</param>
        public void SetData(void* data) => ENET_API.enet_peer_data(_handle, data);
    }
}