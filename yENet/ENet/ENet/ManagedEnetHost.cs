using System;
using System.Net.Sockets;
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
        ///     When non-zero, the host ignores incoming connection requests instead of accepting them.
        /// </summary>
        public bool IgnoreConnectRequests => _handle.IgnoreConnectRequests;

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
        ///     Number of allowed peers from duplicate IPs.
        /// </summary>
        public nuint MaximumDuplicatePeers => _handle.MaximumDuplicatePeers;

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
            _handle.Dispose();
        }

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => Volatile.Read(ref _disposed) == 0 && _handle.IsCreated;

        /// <summary>
        ///     Performs application-defined tasks associated with freeing,
        ///     releasing, or resetting unmanaged resources.
        /// </summary>
        ~ManagedEnetHost() => Dispose();

        /// <summary>
        ///     Validates that the instance has been properly allocated and initialized.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        ///     Thrown if the instance is not created
        ///     (i.e., the underlying native handle is <see langword="null" />).
        /// </exception>
        public void Validate() => ThrowHelpers.ThrowIfNotCreated(IsCreated, ExceptionArgument._dummy);

        /// <summary>
        ///     Gets the handle to the underlying object.
        /// </summary>
        public EnetHost GetInner() => _handle;

        /// <summary>
        ///     Sends a 1‑byte dummy packet directly to the specified address without queuing.
        ///     This is typically used for NAT hole‑punching or to elicit a response from a remote host.
        /// </summary>
        /// <param name="address">The destination address to ping.</param>
        /// <returns>
        ///     <see langword="true" /> if the packet was successfully sent;
        ///     otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///     The packet contains a single byte of arbitrary data and is sent immediately via the host's socket,
        ///     bypassing the usual ENet queuing and reliability mechanisms.
        ///     This function does not affect the peer's state or round‑trip time statistics.
        /// </remarks>
        public bool TryPing(ENetAddress address) => _handle.TryPing(address);

        /// <summary>
        ///     Sets whether the host ignores incoming connection requests.
        /// </summary>
        /// <param name="ignoreConnectRequests">
        ///     <see langword="true" /> to ignore incoming connection requests,
        ///     or <see langword="false" /> to accept them.
        /// </param>
        public void SetIgnoreConnectRequests(bool ignoreConnectRequests) => _handle.SetIgnoreConnectRequests(ignoreConnectRequests);

        /// <summary>
        ///     Sets the MTU of the host.
        /// </summary>
        /// <param name="mtu">The MTU to set, in bytes. if 0, the default is used.</param>
        /// <returns>0 on success, or -1 if the MTU exceeds ENET_PROTOCOL_MAXIMUM_MTU.</returns>
        public bool SetMtu(uint mtu) => _handle.SetMtu(mtu);

        /// <summary>
        ///     Attempts to retrieve a peer by its incoming peer identifier.
        /// </summary>
        /// <param name="incomingPeerId">The local identifier assigned to this peer slot within the host.</param>
        /// <param name="peer">
        ///     When this method returns, contains the <see cref="EnetPeer" /> corresponding to the specified ID,
        ///     or a default (invalid) peer if the ID is out of range.
        /// </param>
        /// <returns>
        ///     <see langword="true" /> if the peer slot exists and was successfully retrieved;
        ///     otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///     <para>
        ///         The <paramref name="incomingPeerId" /> corresponds to a fixed slot in the host's internal peers array,
        ///         which is allocated at host creation time based on the <c>peerCount</c> parameter passed to
        ///         <see cref="Create" />. The ID is not assigned dynamically during connection; it is the index into
        ///         that pre-allocated array and remains constant for the lifetime of the host.
        ///     </para>
        ///     <para>
        ///         This method does not verify whether the peer is currently connected; it merely checks that the
        ///         ID is within the valid range of the pre-allocated array.
        ///     </para>
        /// </remarks>
        public bool TryGetPeer(ushort incomingPeerId, out EnetPeer peer) => _handle.TryGetPeer(incomingPeerId, out peer);

        /// <summary>
        ///     Sets the checksum callback function used by the host to compute packet checksums.
        /// </summary>
        /// <param name="checksum">
        ///     A function pointer to a custom checksum calculation routine, or <c>null</c> to disable custom checksums
        ///     and revert to the default checksum behavior.
        /// </param>
        public void SetChecksumCallback(delegate* managed<ENetBuffer*, nuint, uint> checksum) => _handle.SetChecksumCallback(checksum);

#if NET7_0_OR_GREATER
        /// <summary>
        ///     Sets the checksum callback of the host using the static abstract checksum strategy
        ///     <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">The checksum type implementing <see cref="IENetChecksumCallback" />.</typeparam>
        public void SetChecksumCallback<T>() where T : IENetChecksumCallback => _handle.SetChecksumCallback<T>();
#endif

        /// <summary>
        ///     Sets the checksum callback of the host to the default CRC-32 implementation.
        /// </summary>
        public void SetChecksumCallbackWithCrc32() => _handle.SetChecksumCallbackWithCrc32();

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

#if NET7_0_OR_GREATER
        /// <summary>
        ///     Sets the intercept callback of the host using the static abstract intercept strategy
        ///     <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">The intercept type implementing <see cref="IENetInterceptCallback" />.</typeparam>
        public void SetInterceptCallback<T>() where T : IENetInterceptCallback => _handle.SetInterceptCallback<T>();
#endif

        /// <summary>
        ///     Sets the maximum number of allowed peers from duplicate IPs.
        /// </summary>
        /// <param name="duplicatePeers">The maximum number of duplicate peers to maintain. if 0, the default is used.</param>
        public void SetMaximumDuplicatePeers(nuint duplicatePeers) => _handle.SetMaximumDuplicatePeers(duplicatePeers);

        /// <summary>
        ///     Sets the maximum allowable packet size that may be sent or received on a peer.
        /// </summary>
        /// <param name="maximumPacketSize">The maximum allowable packet size; if 0, the default is used.</param>
        public void SetMaximumPacketSize(nuint maximumPacketSize) => _handle.SetMaximumPacketSize(maximumPacketSize);

        /// <summary>
        ///     Sets the maximum aggregate amount of buffer space a peer may use waiting for packets to be delivered.
        /// </summary>
        /// <param name="maximumWaitingData">The maximum aggregate waiting data; if 0, the default is used.</param>
        public void SetMaximumWaitingData(nuint maximumWaitingData) => _handle.SetMaximumWaitingData(maximumWaitingData);

        /// <summary>
        ///     Initiates a connection to a foreign host.
        /// </summary>
        /// <param name="address">destination for the connection</param>
        /// <param name="channelCount">number of channels to allocate</param>
        /// <param name="data">user data supplied to the receiving host</param>
        /// <param name="peer">a peer representing the foreign host on success, NULL on failure</param>
        /// <returns>
        ///     <see langword="true" /> if the connection attempt was initiated and the peer object is valid;
        ///     otherwise, <see langword="false" />.
        /// </returns>
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
        /// <param name="packet">
        ///     The packet to broadcast.
        ///     <para>
        ///         <b>Ownership transfer</b>: After calling this method, ENet assumes ownership of the underlying native handle
        ///         regardless of whether the broadcast is fully successful (e.g., even if some peers cannot accept the packet).
        ///         The <paramref name="packet" /> reference will be reset to a default (invalid) state, and the caller must not
        ///         use or destroy it afterwards.
        ///     </para>
        /// </param>
        /// <remarks>
        ///     This method always transfers ownership of the packet to the host.
        /// </remarks>
        public void Broadcast(byte channelId, ref EnetPacket packet) => _handle.Broadcast(channelId, ref packet);

        /// <summary>
        ///     Queues a packet to be sent to the connected peers selected by the supplied bit array.
        /// </summary>
        /// <param name="channelId">channel on which to broadcast</param>
        /// <param name="bitArray">a bit array selecting the peers to receive the packet</param>
        /// <param name="packet">packet to broadcast</param>
        public void BroadcastSelected(byte channelId, ReadOnlySpan<byte> bitArray, ref EnetPacket packet) => _handle.BroadcastSelected(channelId, bitArray, ref packet);

        /// <summary>
        ///     Sets the packet compressor the host should use to compress and decompress packets.
        /// </summary>
        /// <param name="compressor">callbacks for for the packet compressor; if NULL, then compression is disabled</param>
        public void SetCompressor(ENetCompressor compressor) => _handle.SetCompressor(compressor);

#if NET7_0_OR_GREATER
        /// <summary>
        ///     Sets the packet compressor the host should use to compress and decompress packets
        ///     using the static abstract compressor strategy <typeparamref name="T" />.
        /// </summary>
        /// <param name="context">The compressor context data.</param>
        /// <typeparam name="T">The compressor type implementing <see cref="IENetCompressor" />.</typeparam>
        public void SetCompressor<T>(void* context) where T : IENetCompressor => _handle.SetCompressor<T>(context);
#endif

        /// <summary>
        ///     Sets the packet compressor the host should use to the default range coder.
        /// </summary>
        /// <returns>
        ///     <see langword="true" /> if the host compressor was set to the range coder;
        ///     otherwise, <see langword="false" />.
        /// </returns>
        public bool TrySetCompressorWithRangeCoder() => _handle.TrySetCompressorWithRangeCoder();

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
        ///     Recomputes the packet throttle limits of all connected peers to respect the host bandwidth constraints.
        /// </summary>
        /// <remarks>
        ///     The library invokes this automatically at regular intervals when bandwidth limits are configured;
        ///     calling it explicitly forces an immediate recalculation.
        /// </remarks>
        public void ThrottleBandwidth() => _handle.ThrottleBandwidth();

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
        ///     Specifies the IP addressing mode to use (Ipv4, Ipv6-only, or Ipv6 dual‑stack).
        /// </param>
        /// <exception cref="ArgumentException">Thrown when host creation fails.</exception>
        /// <exception cref="SocketException">Thrown when host creation fails.</exception>
        public static ManagedEnetHost Create(ENetAddress address, nuint peerCount, nuint channelLimit, uint incomingBandwidth, uint outgoingBandwidth, EnetHostOption option)
        {
            var handle = EnetHost.Create(address, peerCount, channelLimit, incomingBandwidth, outgoingBandwidth, option);
            if (!handle.IsCreated)
                return default!;

            return new ManagedEnetHost(handle);
        }
    }
}