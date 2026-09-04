using System;
using System.Threading;
using enet;

// ReSharper disable ALL

namespace Enet
{
    /// <summary>
    ///     An ENet host for communicating with peers.
    /// </summary>
    public sealed unsafe class ManagedEnetHost : IIsCreated, IDisposable
    {
        /// <summary>
        ///     Indicates whether the instance has been disposed.
        /// </summary>
        private int _disposed;

        /// <summary>
        ///     Gets the handle to the underlying object.
        /// </summary>
        private EnetHost _handle;

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="handle" /> is null.</exception>
        private ManagedEnetHost(EnetHost handle)
        {
            ThrowHelpers.ThrowIfNull(handle.GetInner(), ExceptionArgument.handle);
            _disposed = 0;
            _handle = handle;
        }

        /// <summary>
        ///     Gets the underlying socket descriptor used by the host.
        /// </summary>
        public ENetSocket Socket => _handle.Socket;

        /// <summary>
        ///     Gets the Internet address to which the host is bound.
        /// </summary>
        public ENetAddress Address => _handle.Address;

        /// <summary>
        ///     Gets the downstream bandwidth limit (in bytes per second) of the host.
        /// </summary>
        public uint IncomingBandwidth => _handle.IncomingBandwidth;

        /// <summary>
        ///     Gets the upstream bandwidth limit (in bytes per second) of the host.
        /// </summary>
        public uint OutgoingBandwidth => _handle.OutgoingBandwidth;

        /// <summary>
        ///     Gets the maximum transmission unit (MTU) used by the host.
        /// </summary>
        public uint Mtu => _handle.Mtu;

        /// <summary>
        ///     Gets the number of peers allocated for this host.
        /// </summary>
        public nuint PeerCount => _handle.PeerCount;

        /// <summary>
        ///     Gets the maximum number of channels allowed per peer.
        /// </summary>
        public nuint ChannelLimit => _handle.ChannelLimit;

        /// <summary>
        ///     Gets the current service time of the host in milliseconds.
        /// </summary>
        public uint ServiceTime => _handle.ServiceTime;

        /// <summary>
        ///     Gets the total number of packets currently queued for sending.
        /// </summary>
        public uint TotalQueued => _handle.TotalQueued;

        /// <summary>
        ///     Gets the size of packets used by the host.
        /// </summary>
        public nuint PacketSize => _handle.PacketSize;

        /// <summary>
        ///     Gets a function pointer to the checksum callback used by the host, or <c>null</c> if none.
        /// </summary>
        public delegate* managed<ENetBuffer*, nuint, uint> ChecksumCallback => _handle.ChecksumCallback;

        /// <summary>
        ///     Gets the compressor used by the host for packet compression.
        /// </summary>
        public ENetCompressor Compressor => _handle.Compressor;

        /// <summary>
        ///     Gets the total number of bytes sent by the host.
        /// </summary>
        public uint TotalSentData => _handle.TotalSentData;

        /// <summary>
        ///     Gets the total number of packets sent by the host.
        /// </summary>
        public uint TotalSentPackets => _handle.TotalSentPackets;

        /// <summary>
        ///     Gets the total number of bytes received by the host.
        /// </summary>
        public uint TotalReceivedData => _handle.TotalReceivedData;

        /// <summary>
        ///     Gets the total number of packets received by the host.
        /// </summary>
        public uint TotalReceivedPackets => _handle.TotalReceivedPackets;

        /// <summary>
        ///     Gets a function pointer to the intercept callback, or <c>null</c> if none.
        /// </summary>
        public delegate* managed<ENetHost*, ENetEvent*, int> InterceptCallback => _handle.InterceptCallback;

        /// <summary>
        ///     Gets the number of peers currently connected to the host.
        /// </summary>
        public nuint ConnectedPeers => _handle.ConnectedPeers;

        /// <summary>
        ///     Gets the number of peers whose bandwidth is currently being limited.
        /// </summary>
        public nuint BandwidthLimitedPeers => _handle.BandwidthLimitedPeers;

        /// <summary>
        ///     Gets the number of duplicate peers currently tracked by the host.
        /// </summary>
        public nuint DuplicatePeers => _handle.DuplicatePeers;

        /// <summary>
        ///     Gets the maximum packet size allowed by the host.
        /// </summary>
        public nuint MaximumPacketSize => _handle.MaximumPacketSize;

        /// <summary>
        ///     Gets the maximum amount of waiting data allowed by the host.
        /// </summary>
        public nuint MaximumWaitingData => _handle.MaximumWaitingData;

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;

            GC.SuppressFinalize(this);
            var handle = _handle;
            _handle = default;
            handle.Dispose();
        }

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => Volatile.Read(ref _disposed) == 0 && _handle.IsCreated;

        /// <summary>
        ///     Gets the handle to the underlying object.
        /// </summary>
        public EnetHost GetInner() => _handle;

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        ~ManagedEnetHost() => Dispose();

        /// <summary>
        ///     Sends a ping request to an address.
        /// </summary>
        /// <param name="address">destination for the ping request</param>
        public bool Ping(ENetAddress address) => _handle.Ping(address);

        /// <summary>
        ///     Initiates a connection to a foreign host.
        /// </summary>
        /// <param name="address">destination for the connection</param>
        /// <param name="channelCount">number of channels to allocate</param>
        /// <param name="data">user data supplied to the receiving host</param>
        /// <param name="peer">a peer representing the foreign host on success, NULL on failure</param>
        /// <returns>a peer representing the foreign host on success, NULL on failure</returns>
        /// <remarks>
        ///     The peer returned will have not completed the connection until enet_host_service()
        ///     notifies of an ENET_EVENT_TYPE_CONNECT event for the peer.
        /// </remarks>
        public bool TryConnect(ENetAddress address, nuint channelCount, uint data, out EnetPeer peer) => _handle.TryConnect(address, channelCount, data, out peer);

        /// <summary>
        ///     Checks for any queued events on the host and dispatches one if available.
        /// </summary>
        /// <param name="event">an event structure where event details will be placed if available</param>
        /// <returns>
        ///     <list type="bullet">
        ///         <item>
        ///             <description>&gt; 0 if an event was dispatched</description>
        ///         </item>
        ///         <item>
        ///             <description>0 if no events are available</description>
        ///         </item>
        ///         <item>
        ///             <description>&lt; 0 on failure</description>
        ///         </item>
        ///     </list>
        /// </returns>
        public int CheckEvents(out EnetEvent @event) => _handle.CheckEvents(out @event);

        /// <summary>
        ///     Waits for events on the host specified and shuttles packets between
        ///     the host and its peers.
        /// </summary>
        /// <param name="event">
        ///     an event structure where event details will be placed if one occurs
        ///     if event == NULL then no events will be delivered
        /// </param>
        /// <param name="timeout">number of milliseconds that ENet should wait for events</param>
        /// <returns>
        ///     <list type="bullet">
        ///         <item>
        ///             <description>&gt; 0 if an event occurred within the specified time limit</description>
        ///         </item>
        ///         <item>
        ///             <description>0 if no event occurred</description>
        ///         </item>
        ///         <item>
        ///             <description>&lt; 0 on failure</description>
        ///         </item>
        ///     </list>
        /// </returns>
        /// <remarks>
        ///     enet_host_service should be called fairly regularly for adequate performance
        /// </remarks>
        public int Service(uint timeout, out EnetEvent @event) => _handle.Service(timeout, out @event);

        /// <summary>
        ///     Sends any queued packets on the host specified to its designated peers.
        /// </summary>
        /// <remarks>
        ///     This function need only be used in circumstances where one wishes to send queued packets earlier than in a call to
        ///     enet_host_service().
        /// </remarks>
        public void Flush() => _handle.Flush();

        /// <summary>
        ///     Queues a packet to be sent to all peers associated with the host.
        /// </summary>
        /// <param name="channelId">channel on which to broadcast</param>
        /// <param name="packet">packet to broadcast</param>
        public void Broadcast(byte channelId, ref EnetPacket packet) => _handle.Broadcast(channelId, ref packet);

        /// <summary>
        ///     Sets the packet compressor the host should use to compress and decompress packets.
        /// </summary>
        /// <param name="compressor">callbacks for for the packet compressor; if NULL, then compression is disabled</param>
        public void SetCompressor(ENetCompressor compressor) => _handle.SetCompressor(compressor);

        /// <summary>
        ///     Limits the maximum allowed channels of future incoming connections.
        /// </summary>
        /// <param name="channelLimit">
        ///     the maximum number of channels allowed; if 0, then this is equivalent to
        ///     <see cref="ENet.ENET_PROTOCOL_MAXIMUM_CHANNEL_COUNT" />.
        /// </param>
        public void SetChannelLimit(nuint channelLimit) => _handle.SetChannelLimit(channelLimit);

        /// <summary>
        ///     Adjusts the bandwidth limits of the host.
        /// </summary>
        /// <param name="incomingBandwidth">new incoming bandwidth in bytes/second.</param>
        /// <param name="outgoingBandwidth">new outgoing bandwidth in bytes/second.</param>
        public void SetBandwidthLimit(uint incomingBandwidth, uint outgoingBandwidth) => _handle.SetBandwidthLimit(incomingBandwidth, outgoingBandwidth);

        /// <summary>
        ///     Sets the checksum callback function used by the host to compute packet checksums.
        /// </summary>
        /// <param name="checksum">
        ///     A function pointer to a custom checksum calculation routine, or <c>null</c> to disable custom checksums
        ///     and revert to the default checksum behavior.
        /// </param>
        public void SetChecksumCallback(delegate* managed<ENetBuffer*, nuint, uint> checksum) => _handle.SetChecksumCallback(checksum);

        /// <summary>
        ///     Sets the intercept callback function used by the host to intercept incoming events before they are processed.
        /// </summary>
        /// <param name="intercept">
        ///     A function pointer to an intercept routine, or <c>null</c> to disable interception.
        /// </param>
        /// <remarks>
        ///     The intercept callback receives the host and a pointer to the event structure. It can modify or suppress
        ///     the event by returning a non‑zero value.
        /// </remarks>
        public void SetInterceptCallback(delegate* managed<ENetHost*, ENetEvent*, int> intercept) => _handle.SetInterceptCallback(intercept);

        /// <summary>
        ///     Sets the maximum number of duplicate peers that the host will track.
        /// </summary>
        /// <param name="duplicatePeers">
        ///     The maximum number of duplicate peers to maintain. A value of <c>0</c> may indicate no explicit limit,
        ///     causing the host to use its internal default.
        /// </param>
        public void SetMaxDuplicatePeers(nuint duplicatePeers) => _handle.SetMaxDuplicatePeers(duplicatePeers);

        /// <summary>
        ///     Initializes a new instance of the <see cref="ManagedEnetHost" /> class with the specified address, peer count,
        ///     channel
        ///     limit, bandwidth, and IP option.
        /// </summary>
        /// <param name="address">
        ///     The address to bind the host to.
        /// </param>
        /// <param name="peerCount">
        ///     The maximum number of peers that can be connected to this host simultaneously.
        /// </param>
        /// <param name="channelLimit">
        ///     The maximum number of channels allowed per peer. Pass <c>0</c> to use the default limit (
        ///     <c>ENET_PROTOCOL_MAXIMUM_CHANNEL_COUNT</c>).
        /// </param>
        /// <param name="incomingBandwidth">
        ///     The downstream bandwidth limit in bytes per second. Pass <c>0</c> for unlimited bandwidth.
        /// </param>
        /// <param name="outgoingBandwidth">
        ///     The upstream bandwidth limit in bytes per second. Pass <c>0</c> for unlimited bandwidth.
        /// </param>
        /// <param name="option">
        ///     Specifies the IP addressing mode to use (IPv4, IPv6-only, or IPv6 dual‑stack).
        /// </param>
        /// <exception cref="ArgumentException">
        ///     Thrown when host creation fails.
        /// </exception>
        public static ManagedEnetHost Create(ENetAddress address, nuint peerCount, nuint channelLimit, uint incomingBandwidth, uint outgoingBandwidth, EnetHostOption option)
        {
            var handle = EnetHost.Create(address, peerCount, channelLimit, incomingBandwidth, outgoingBandwidth, option);
            if (!handle.IsCreated)
                return default!;

            return new ManagedEnetHost(handle);
        }
    }
}