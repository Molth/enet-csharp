using System;
using System.Net;
using System.Runtime.CompilerServices;

// ReSharper disable ALL

namespace enet
{
    /// <summary>
    ///     ENet reliable UDP networking library
    /// </summary>
    /// <remarks>生きていたいと</remarks>
    public static unsafe class ENET_API
    {
        /// <summary>
        ///     Initializes ENet globally. Must be called prior to using any functions in
        ///     ENet.
        /// </summary>
        /// <returns>0 on success, &lt; 0 on failure</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int enet_initialize() => ENet.enet_initialize();

        /// <summary>
        ///     Initializes ENet globally and supplies user-overridden callbacks. Must be called prior to using any functions in
        ///     ENet.
        ///     Do not use <see cref="enet_initialize()" /> if you use this variant. Make sure the <see cref="ENetCallbacks" />
        ///     structure
        ///     is zeroed out so that any additional callbacks added in future versions will be properly ignored.
        /// </summary>
        /// <param name="version">
        ///     the constant <see cref="ENet.ENET_VERSION" /> should be supplied so ENet knows which version of
        ///     <see cref="ENetCallbacks" /> struct to use
        /// </param>
        /// <param name="inits">user-overridden callbacks where any NULL callbacks will use ENet's defaults</param>
        /// <returns>0 on success, &lt; 0 on failure</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int enet_initialize_with_callbacks(uint version, ENetCallbacks* inits) => ENet.enet_initialize_with_callbacks(version, inits);

        /// <summary>
        ///     Shuts down ENet globally.  Should be called when a program that has
        ///     initialized ENet exits.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void enet_deinitialize() => ENet.enet_deinitialize();

        /// <summary>
        ///     Gives the linked version of the ENet library.
        /// </summary>
        /// <returns>the version number</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint enet_linked_version() => ENet.enet_linked_version();

        /// <summary>
        ///     Gets the current wall-time in milliseconds.
        /// </summary>
        /// <returns>
        ///     the wall-time in milliseconds.  Its initial value is unspecified
        ///     unless otherwise set.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint enet_time_get() => ENet.enet_time_get();

        /// <summary>
        ///     Sets the current wall-time in milliseconds.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void enet_time_set(uint newTimeBase) => ENet.enet_time_set(newTimeBase);

        /// <summary>
        ///     Creates a native socket of the requested type and addressing mode.
        /// </summary>
        /// <param name="type">The type of socket to create.</param>
        /// <param name="option">The addressing mode to use.</param>
        /// <returns>The created socket, or an invalid socket on failure.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ENetSocket enet_socket_create(ENetSocketType type, ENetHostOption option) => ENet.enet_socket_create(type, option);

        /// <summary>
        ///     Binds the socket to the specified local address.
        /// </summary>
        /// <param name="socket">The socket to bind.</param>
        /// <param name="address">The local address to bind to.</param>
        /// <returns>0 on success, SOCKET_ERROR on failure.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int enet_socket_bind(ENetSocket socket, ENetAddress* address) => ENet.enet_socket_bind(socket, address);

        /// <summary>
        ///     Retrieves the local address the socket is bound to.
        /// </summary>
        /// <param name="socket">The socket to query.</param>
        /// <param name="address">Receives the local address.</param>
        /// <returns>0 on success, SOCKET_ERROR on failure.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int enet_socket_get_address(ENetSocket socket, ENetAddress* address) => ENet.enet_socket_get_address(socket, address);

        /// <summary>
        ///     Sends a vectored payload to the specified address on the socket.
        /// </summary>
        /// <param name="socket">The socket to send on.</param>
        /// <param name="address">The destination address.</param>
        /// <param name="buffers">The buffers holding the payload.</param>
        /// <param name="bufferCount">The number of buffers.</param>
        /// <returns>The number of bytes sent, 0 when the send would block, -1 on failure.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int enet_socket_send(ENetSocket socket, ENetAddress* address, ENetBuffer* buffers, nuint bufferCount) => ENet.enet_socket_send(socket, address, buffers, bufferCount);

        /// <summary>
        ///     Receives a vectored payload on the socket, reporting the sender address.
        /// </summary>
        /// <param name="socket">The socket to receive on.</param>
        /// <param name="address">Receives the source address.</param>
        /// <param name="buffers">The buffers receiving the payload.</param>
        /// <param name="bufferCount">The number of buffers.</param>
        /// <returns>The number of bytes received, 0 when no data is available, -1 on failure.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int enet_socket_receive(ENetSocket socket, ENetAddress* address, ENetBuffer* buffers, nuint bufferCount) => ENet.enet_socket_receive(socket, address, buffers, bufferCount);

        /// <summary>
        ///     Waits until the socket becomes ready for the requested conditions or the timeout elapses.
        /// </summary>
        /// <param name="socket">The socket to wait on.</param>
        /// <param name="condition">On input the conditions to wait for; on output the conditions that became ready.</param>
        /// <param name="milliseconds">The maximum time to wait in milliseconds.</param>
        /// <returns>0 on success, -1 on failure.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int enet_socket_wait(ENetSocket socket, uint* condition, uint milliseconds) => ENet.enet_socket_wait(socket, condition, milliseconds);

        /// <summary>
        ///     Applies a socket option to the given socket.
        /// </summary>
        /// <param name="socket">The socket to configure.</param>
        /// <param name="option">The option to apply.</param>
        /// <param name="value">The option value.</param>
        /// <returns>0 on success, -1 on failure or for unsupported options.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int enet_socket_set_option(ENetSocket socket, ENetSocketOption option, int value) => ENet.enet_socket_set_option(socket, option, value);

        /// <summary>
        ///     Closes and invalidates the given socket.
        /// </summary>
        /// <param name="socket">The socket to destroy.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void enet_socket_destroy(ENetSocket* socket) => ENet.enet_socket_destroy(socket);

        /// <summary>
        ///     Attempts to parse the printable form of the IP address in the parameter hostName
        ///     and sets the host field in the address parameter if successful.
        /// </summary>
        /// <param name="address">destination to store the parsed IP address</param>
        /// <param name="ipEndPoint">IP address to parse</param>
        /// <returns>
        ///     <list type="bullet">
        ///         <item>
        ///             <description>0 on success</description>
        ///         </item>
        ///         <item>
        ///             <description>&lt; 0 on failure</description>
        ///         </item>
        ///     </list>
        ///     the address of the given hostName in address on success
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int enet_address_set_from_ipendpoint(ENetAddress* address, IPEndPoint ipEndPoint) => ENet.enet_address_set_from_ipendpoint(address, ipEndPoint);

        /// <summary>
        ///     Attempts to parse the printable form of the IP address in the parameter hostName
        ///     and sets the host field in the address parameter if successful.
        /// </summary>
        /// <param name="address">destination to store the parsed IP address</param>
        /// <param name="ipAddress">IP address to set</param>
        /// <param name="port">port number in host byte order</param>
        /// <returns>
        ///     <list type="bullet">
        ///         <item>
        ///             <description>0 on success</description>
        ///         </item>
        ///         <item>
        ///             <description>&lt; 0 on failure</description>
        ///         </item>
        ///     </list>
        ///     the address of the given hostName in address on success
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int enet_address_set_from_ipaddress(ENetAddress* address, IPAddress ipAddress, ushort port) => ENet.enet_address_set_from_ipaddress(address, ipAddress, port);

        /// <summary>
        ///     Attempts to parse the printable form of the IP address in the parameter hostName
        ///     and sets the host field in the address parameter if successful.
        /// </summary>
        /// <param name="address">destination to store the parsed IP address</param>
        /// <param name="ip">IP address to parse</param>
        /// <param name="port">port number in host byte order</param>
        /// <returns>
        ///     <list type="bullet">
        ///         <item>
        ///             <description>0 on success</description>
        ///         </item>
        ///         <item>
        ///             <description>&lt; 0 on failure</description>
        ///         </item>
        ///     </list>
        ///     the address of the given hostName in address on success
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int enet_address_set_ip_ipv4(ENetAddress* address, ReadOnlySpan<char> ip, ushort port) => ENet.enet_address_set_ip_ipv4(address, ip, port);

        /// <summary>
        ///     Attempts to parse the printable form of the IP address in the parameter hostName
        ///     and sets the host field in the address parameter if successful.
        /// </summary>
        /// <param name="address">destination to store the parsed IP address</param>
        /// <param name="ip">IP address to parse</param>
        /// <param name="port">port number in host byte order</param>
        /// <param name="scopeId">Ipv6 scope identifier for link‑local or site‑local addresses</param>
        /// <returns>
        ///     <list type="bullet">
        ///         <item>
        ///             <description>0 on success</description>
        ///         </item>
        ///         <item>
        ///             <description>&lt; 0 on failure</description>
        ///         </item>
        ///     </list>
        ///     the address of the given hostName in address on success
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int enet_address_set_ip_ipv6(ENetAddress* address, ReadOnlySpan<char> ip, ushort port, uint scopeId = 0) => ENet.enet_address_set_ip_ipv6(address, ip, port, scopeId);

        /// <summary>
        ///     Attempts to resolve the host named by the parameter hostName and sets
        ///     the host field in the address parameter if successful.
        /// </summary>
        /// <param name="address">destination to store resolved address</param>
        /// <param name="hostName">host name to lookup</param>
        /// <param name="port">port number in host byte order</param>
        /// <returns>
        ///     <list type="bullet">
        ///         <item>
        ///             <description>0 on success</description>
        ///         </item>
        ///         <item>
        ///             <description>&lt; 0 on failure</description>
        ///         </item>
        ///     </list>
        ///     the address of the given hostName in address on success
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int enet_address_set_hostname_ipv4(ENetAddress* address, ReadOnlySpan<char> hostName, ushort port) => ENet.enet_address_set_hostname_ipv4(address, hostName, port);

        /// <summary>
        ///     Attempts to resolve the host named by the parameter hostName and sets
        ///     the host field in the address parameter if successful.
        /// </summary>
        /// <param name="address">destination to store resolved address</param>
        /// <param name="hostName">host name to lookup</param>
        /// <param name="port">port number in host byte order</param>
        /// <param name="scopeId">Ipv6 scope identifier for link‑local or site‑local addresses</param>
        /// <returns>
        ///     <list type="bullet">
        ///         <item>
        ///             <description>0 on success</description>
        ///         </item>
        ///         <item>
        ///             <description>&lt; 0 on failure</description>
        ///         </item>
        ///     </list>
        ///     the address of the given hostName in address on success
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int enet_address_set_hostname_ipv6(ENetAddress* address, ReadOnlySpan<char> hostName, ushort port, uint scopeId = 0) => ENet.enet_address_set_hostname_ipv6(address, hostName, port, scopeId);

        /// <summary>
        ///     Gives the printable form of the IP address specified in the <b>address</b> parameter.
        /// </summary>
        /// <param name="address">address printed</param>
        /// <param name="ip">destination for name, must not be NULL</param>
        /// <returns>
        ///     <list type="bullet">
        ///         <item>
        ///             <description>0 on success</description>
        ///         </item>
        ///         <item>
        ///             <description>&lt; 0 on failure</description>
        ///         </item>
        ///     </list>
        ///     the null-terminated name of the host in hostName on success
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int enet_address_get_ip(ENetAddress* address, ref Span<char> ip) => ENet.enet_address_get_ip(address, ref ip);

        /// <summary>
        ///     Attempts to do a reverse lookup of the host field in the address parameter.
        /// </summary>
        /// <param name="address">address used for reverse lookup</param>
        /// <param name="hostName">destination for name, must not be NULL</param>
        /// <returns>
        ///     <list type="bullet">
        ///         <item>
        ///             <description>0 on success</description>
        ///         </item>
        ///         <item>
        ///             <description>&lt; 0 on failure</description>
        ///         </item>
        ///     </list>
        ///     the null-terminated name of the host in hostName on success
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int enet_address_get_hostname(ENetAddress* address, ref Span<char> hostName) => ENet.enet_address_get_hostname(address, ref hostName);

        /// <summary>
        ///     Creates a packet that may be sent to a peer.
        /// </summary>
        /// <param name="data">initial contents of the packet's data; the packet's data will remain uninitialized if data is NULL.</param>
        /// <param name="dataLength">size of the data allocated for this packet</param>
        /// <param name="flags">flags for this packet as described for the ENetPacket structure.</param>
        /// <returns>the packet on success, NULL on failure</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ENetPacket* enet_packet_create(void* data, nuint dataLength, uint flags) => ENet.enet_packet_create(data, dataLength, flags);

        /// <summary>
        ///     Destroys the packet and deallocates its data.
        /// </summary>
        /// <param name="packet">packet to be destroyed</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void enet_packet_destroy(ENetPacket* packet) => ENet.enet_packet_destroy(packet);

        /// <summary>
        ///     Attempts to resize the data in the packet to length specified in the
        ///     dataLength parameter.
        /// </summary>
        /// <param name="packet">packet to resize</param>
        /// <param name="dataLength">new size for the packet data</param>
        /// <returns>0 on success, &lt; 0 on failure</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int enet_packet_resize(ENetPacket* packet, nuint dataLength) => ENet.enet_packet_resize(packet, dataLength);

        /// <summary>
        ///     Computes the CRC-32 checksum of the given buffers and returns it in network byte order.
        /// </summary>
        /// <param name="buffers">The buffers to checksum.</param>
        /// <param name="bufferCount">The number of buffers.</param>
        /// <returns>The CRC-32 checksum in network byte order.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint enet_crc32(ENetBuffer* buffers, nuint bufferCount) => ENet.enet_crc32(buffers, bufferCount);

        /// <summary>
        ///     Sends a 1‑byte dummy packet directly to the specified address without queuing.
        ///     This is typically used for NAT hole‑punching or to elicit a response from a remote host.
        /// </summary>
        /// <param name="host">host ping the address</param>
        /// <param name="address">The destination address to ping.</param>
        /// <returns>
        ///     <see langword="0" /> if the packet was successfully sent;
        ///     otherwise, <see langword="false" />.
        /// </returns>
        /// <remarks>
        ///     The packet contains a single byte of arbitrary data and is sent immediately via the host's socket,
        ///     bypassing the usual ENet queuing and reliability mechanisms.
        ///     This function does not affect the peer's state or round‑trip time statistics.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int enet_host_ping(ENetHost* host, ENetAddress* address) => ENet.enet_host_ping(host, address);

        /// <summary>
        ///     Sets whether the host ignores incoming connection requests.
        /// </summary>
        /// <param name="host">The host on which to set the ignore-connection-requests behavior.</param>
        /// <param name="ignoreConnectRequests">Non-zero to ignore incoming connection requests, or zero to accept them.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void enet_host_ignore_connect_requests(ENetHost* host, int ignoreConnectRequests) => ENet.enet_host_ignore_connect_requests(host, ignoreConnectRequests);

        /// <summary>
        ///     Sets the MTU of the host.
        /// </summary>
        /// <param name="host">The host whose MTU is being set.</param>
        /// <param name="mtu">The MTU to set, in bytes. If 0, the host default MTU is used.</param>
        /// <returns>0 on success, or -1 if the MTU exceeds ENET_PROTOCOL_MAXIMUM_MTU.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int enet_host_mtu(ENetHost* host, uint mtu) => ENet.enet_host_mtu(host, mtu);

        /// <summary>
        ///     Gets the peer associated with the specified incoming peer identifier.
        /// </summary>
        /// <param name="host">The host whose peer is being retrieved.</param>
        /// <param name="incomingPeerID">The local identifier of the peer slot to retrieve within the host.</param>
        /// <returns>
        ///     A pointer to the peer at the specified slot, or <see langword="null" /> if
        ///     <paramref name="incomingPeerID" /> is out of range of the host's pre-allocated peers array.
        /// </returns>
        /// <remarks>
        ///     The identifier corresponds to a fixed slot in the host's internal peers array, which is allocated
        ///     at host creation time based on the <c>peerCount</c> parameter. It is the index into that array and
        ///     does not verify whether the peer is currently connected.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ENetPeer* enet_host_get_peer(ENetHost* host, ushort incomingPeerID) => ENet.enet_host_get_peer(host, incomingPeerID);

        /// <summary>
        ///     Sets the checksum callback used by the host.
        /// </summary>
        /// <param name="host">The host whose checksum callback is being set.</param>
        /// <param name="checksum">The checksum callback to use, or null to disable checksums.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void enet_host_checksum(ENetHost* host, delegate* managed<ENetBuffer*, nuint, uint> checksum) => ENet.enet_host_checksum(host, checksum);

        /// <summary>
        ///     Enables packet checksums for the host by setting its checksum callback to the built-in CRC-32 implementation.
        /// </summary>
        /// <param name="host">The host on which to enable CRC-32 packet checksums.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void enet_host_checksum_with_crc32(ENetHost* host) => ENet.enet_host_checksum_with_crc32(host);

        /// <summary>
        ///     Sets the intercept callback used by the host.
        /// </summary>
        /// <param name="host">The host whose intercept callback is being set.</param>
        /// <param name="intercept">The intercept callback to use, or null to disable interception.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void enet_host_intercept(ENetHost* host, delegate* managed<ENetHost*, ENetEvent*, int> intercept) => ENet.enet_host_intercept(host, intercept);

        /// <summary>
        ///     Sets the maximum number of duplicate peers that the host will track.
        /// </summary>
        /// <param name="host">The host whose duplicate peer limit is being set.</param>
        /// <param name="duplicatePeers">The maximum number of duplicate peers to maintain. if 0, the default is used.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void enet_host_duplicate_peers(ENetHost* host, nuint duplicatePeers) => ENet.enet_host_duplicate_peers(host, duplicatePeers);

        /// <summary>
        ///     Sets the maximum allowable packet size that may be sent or received on a peer.
        /// </summary>
        /// <param name="host">The host whose maximum packet size is being set.</param>
        /// <param name="maximumPacketSize">The maximum allowable packet size; if 0, the default is used.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void enet_host_maximum_packet_size(ENetHost* host, nuint maximumPacketSize) => ENet.enet_host_maximum_packet_size(host, maximumPacketSize);

        /// <summary>
        ///     Sets the maximum aggregate amount of buffer space a peer may use waiting for packets to be delivered.
        /// </summary>
        /// <param name="host">The host whose maximum waiting data is being set.</param>
        /// <param name="maximumWaitingData">The maximum aggregate waiting data; if 0, the default is used.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void enet_host_maximum_waiting_data(ENetHost* host, nuint maximumWaitingData) => ENet.enet_host_maximum_waiting_data(host, maximumWaitingData);

        /// <summary>
        ///     Queues a packet to be sent to the connected peers selected by the supplied bit array.
        /// </summary>
        /// <param name="host">host on which to broadcast the packet</param>
        /// <param name="bitArray">
        ///     a bit array in which bit <c>i</c> (i.e. the bit at byte <c>i / 8</c>, bit offset <c>i % 8</c>)
        ///     selects the peer whose incoming peer identifier is <c>i</c>
        /// </param>
        /// <param name="channelID">channel on which to broadcast</param>
        /// <param name="packet">packet to broadcast</param>
        /// <remarks>
        ///     <para>
        ///         Only peers that are both selected by <paramref name="bitArray" /> and currently in the
        ///         connected state receive the packet. Bits beyond <c>host->peerCount</c> are ignored.
        ///     </para>
        ///     <para>
        ///         This function always transfers ownership of the packet to the host. If no selected peer is
        ///         connected (and thus the packet is never queued), the packet is destroyed.
        ///     </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void enet_host_broadcast_selected(ENetHost* host, byte channelID, ReadOnlySpan<byte> bitArray, ENetPacket* packet) => ENet.enet_host_broadcast_selected(host, channelID, bitArray, packet);

        /// <summary>
        ///     Creates a host for communicating to peers.
        /// </summary>
        /// <param name="address">
        ///     The address at which other peers may connect to this host. If NULL, then no peers may connect to
        ///     the host.
        /// </param>
        /// <param name="peerCount">The maximum number of peers that should be allocated for the host.</param>
        /// <param name="channelLimit">
        ///     The maximum number of channels allowed; if 0, then this is equivalent to
        ///     ENET_PROTOCOL_MAXIMUM_CHANNEL_COUNT
        /// </param>
        /// <param name="incomingBandwidth">
        ///     Downstream bandwidth of the host in bytes/second; if 0, ENet will assume unlimited
        ///     bandwidth.
        /// </param>
        /// <param name="outgoingBandwidth">
        ///     Upstream bandwidth of the host in bytes/second; if 0, ENet will assume unlimited
        ///     bandwidth.
        /// </param>
        /// <param name="option">
        ///     <list type="bullet">
        ///         <item>
        ///             <description>ENET_HOSTOPT_IPV4 (default): Ipv4</description>
        ///         </item>
        ///         <item>
        ///             <description>ENET_HOSTOPT_IPV6_ONLY: Ipv6-only</description>
        ///         </item>
        ///         <item>
        ///             <description>ENET_HOSTOPT_IPV6_DUALMODE: both Ipv4 and Ipv6</description>
        ///         </item>
        ///     </list>
        /// </param>
        /// <returns>The host on success and NULL on failure</returns>
        /// <remarks>
        ///     ENet will strategically drop packets on specific sides of a connection between hosts
        ///     to ensure the host's bandwidth is not overwhelmed. The bandwidth parameters also determine
        ///     the window size of a connection which limits the amount of reliable packets that may be in transit
        ///     at any given time.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ENetHost* enet_host_create(ENetAddress* address, nuint peerCount, nuint channelLimit, uint incomingBandwidth, uint outgoingBandwidth, ENetHostOption option) => ENet.enet_host_create(address, peerCount, channelLimit, incomingBandwidth, outgoingBandwidth, option);

        /// <summary>
        ///     Destroys the host and all resources associated with it.
        /// </summary>
        /// <param name="host">pointer to the host to destroy</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void enet_host_destroy(ENetHost* host) => ENet.enet_host_destroy(host);

        /// <summary>
        ///     Initiates a connection to a foreign host.
        /// </summary>
        /// <param name="host">host seeking the connection</param>
        /// <param name="address">destination for the connection</param>
        /// <param name="channelCount">number of channels to allocate</param>
        /// <param name="data">user data supplied to the receiving host</param>
        /// <returns>a peer representing the foreign host on success, NULL on failure</returns>
        /// <remarks>
        ///     The peer returned will have not completed the connection until enet_host_service()
        ///     notifies of an ENET_EVENT_TYPE_CONNECT event for the peer.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ENetPeer* enet_host_connect(ENetHost* host, ENetAddress* address, nuint channelCount, uint data) => ENet.enet_host_connect(host, address, channelCount, data);

        /// <summary>
        ///     Checks for any queued events on the host and dispatches one if available.
        /// </summary>
        /// <param name="host">host to check for events</param>
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int enet_host_check_events(ENetHost* host, ENetEvent* @event) => ENet.enet_host_check_events(host, @event);

        /// <summary>
        ///     Waits for events on the host specified and shuttles packets between
        ///     the host and its peers.
        /// </summary>
        /// <param name="host">host to service</param>
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int enet_host_service(ENetHost* host, ENetEvent* @event, uint timeout) => ENet.enet_host_service(host, @event, timeout);

        /// <summary>
        ///     Sends any queued packets on the host specified to its designated peers.
        /// </summary>
        /// <param name="host">host to flush</param>
        /// <remarks>
        ///     This function need only be used in circumstances where one wishes to send queued packets earlier than in a call to
        ///     enet_host_service().
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void enet_host_flush(ENetHost* host) => ENet.enet_host_flush(host);

        /// <summary>
        ///     Queues a packet to be sent to all peers associated with the host.
        /// </summary>
        /// <param name="host">host on which to broadcast the packet</param>
        /// <param name="channelID">channel on which to broadcast</param>
        /// <param name="packet">packet to broadcast</param>
        /// <remarks>
        ///     This function always transfers ownership of the packet to the host. If no peer is
        ///     connected (and thus the packet is never queued), the packet is destroyed.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void enet_host_broadcast(ENetHost* host, byte channelID, ENetPacket* packet) => ENet.enet_host_broadcast(host, channelID, packet);

        /// <summary>
        ///     Sets the packet compressor the host should use to compress and decompress packets.
        /// </summary>
        /// <param name="host">host to enable or disable compression for</param>
        /// <param name="compressor">callbacks for for the packet compressor; if NULL, then compression is disabled</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void enet_host_compress(ENetHost* host, ENetCompressor* compressor) => ENet.enet_host_compress(host, compressor);

        /// <summary>
        ///     Sets the packet compressor the host should use to the default range coder.
        /// </summary>
        /// <param name="host">host to enable the range coder for</param>
        /// <returns>0 on success, &lt; 0 on failure</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int enet_host_compress_with_range_coder(ENetHost* host) => ENet.enet_host_compress_with_range_coder(host);

        /// <summary>
        ///     Limits the maximum allowed channels of future incoming connections.
        /// </summary>
        /// <param name="host">host to limit</param>
        /// <param name="channelLimit">
        ///     the maximum number of channels allowed; if 0, then this is equivalent to
        ///     ENET_PROTOCOL_MAXIMUM_CHANNEL_COUNT
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void enet_host_channel_limit(ENetHost* host, nuint channelLimit) => ENet.enet_host_channel_limit(host, channelLimit);

        /// <summary>
        ///     Adjusts the bandwidth limits of a host.
        /// </summary>
        /// <param name="host">host to adjust</param>
        /// <param name="incomingBandwidth">new incoming bandwidth</param>
        /// <param name="outgoingBandwidth">new outgoing bandwidth</param>
        /// <remarks>
        ///     the incoming and outgoing bandwidth parameters are identical in function to those
        ///     specified in <see cref="enet_host_create(ENetAddress*, nuint, nuint, uint, uint, ENetHostOption)" />.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void enet_host_bandwidth_limit(ENetHost* host, uint incomingBandwidth, uint outgoingBandwidth) => ENet.enet_host_bandwidth_limit(host, incomingBandwidth, outgoingBandwidth);

        /// <summary>
        ///     Recomputes the packet throttle limits of all connected peers to respect the host bandwidth constraints.
        /// </summary>
        /// <param name="host">The host to throttle.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void enet_host_bandwidth_throttle(ENetHost* host) => ENet.enet_host_bandwidth_throttle(host);

        /// <summary>
        ///     Sets the application private data associated with a peer.
        /// </summary>
        /// <param name="peer">peer whose data is being set</param>
        /// <param name="data">the application private data to associate with the peer</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void enet_peer_data(ENetPeer* peer, void* data) => ENet.enet_peer_data(peer, data);

        /// <summary>
        ///     Queues a packet to be sent.
        /// </summary>
        /// <remarks>
        ///     On success, ENet will assume ownership of the packet, and so enet_packet_destroy
        ///     should not be called on it thereafter. On failure, the caller still must destroy
        ///     the packet on its own as ENet has not queued the packet. The caller can also
        ///     check the packet's referenceCount field after sending to check if ENet queued
        ///     the packet and thus incremented the referenceCount.
        /// </remarks>
        /// <param name="peer">destination for the packet</param>
        /// <param name="channelID">channel on which to send</param>
        /// <param name="packet">packet to send</param>
        /// <returns>
        ///     <list type="bullet">
        ///         <item>
        ///             <description>0 on success</description>
        ///         </item>
        ///         <item>
        ///             <description>&lt; 0 on failure</description>
        ///         </item>
        ///     </list>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int enet_peer_send(ENetPeer* peer, byte channelID, ENetPacket* packet) => ENet.enet_peer_send(peer, channelID, packet);

        /// <summary>
        ///     Attempts to dequeue any incoming queued packet.
        /// </summary>
        /// <param name="peer">peer to dequeue packets from</param>
        /// <param name="channelID">holds the channel ID of the channel the packet was received on success</param>
        /// <returns>a pointer to the packet, or NULL if there are no available incoming queued packets</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ENetPacket* enet_peer_receive(ENetPeer* peer, byte* channelID) => ENet.enet_peer_receive(peer, channelID);

        /// <summary>
        ///     Sends a ping request to a peer.
        /// </summary>
        /// <param name="peer">destination for the ping request</param>
        /// <remarks>
        ///     ping requests factor into the mean round trip time as designated by the
        ///     roundTripTime field in the ENetPeer structure. ENet automatically pings all connected
        ///     peers at regular intervals, however, this function may be called to ensure more
        ///     frequent ping requests.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void enet_peer_ping(ENetPeer* peer) => ENet.enet_peer_ping(peer);

        /// <summary>
        ///     Sets the interval at which pings will be sent to a peer.
        /// </summary>
        /// <remarks>
        ///     Pings are used both to monitor the liveness of the connection and also to dynamically
        ///     adjust the throttle during periods of low traffic so that the throttle has reasonable
        ///     responsiveness during traffic spikes.
        /// </remarks>
        /// <param name="peer">the peer to adjust</param>
        /// <param name="pingInterval">the interval at which to send pings; defaults to ENET_PEER_PING_INTERVAL if 0</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void enet_peer_ping_interval(ENetPeer* peer, uint pingInterval) => ENet.enet_peer_ping_interval(peer, pingInterval);

        /// <summary>
        ///     Sets the timeout parameters for a peer.
        /// </summary>
        /// <remarks>
        ///     The timeout parameter control how and when a peer will timeout from a failure to acknowledge
        ///     reliable traffic. Timeout values use an exponential backoff mechanism, where if a reliable
        ///     packet is not acknowledge within some multiple of the average RTT plus a variance tolerance,
        ///     the timeout will be doubled until it reaches a set limit. If the timeout is thus at this
        ///     limit and reliable packets have been sent but not acknowledged within a certain minimum time
        ///     period, the peer will be disconnected. Alternatively, if reliable packets have been sent
        ///     but not acknowledged for a certain maximum time period, the peer will be disconnected regardless
        ///     of the current timeout limit value.
        /// </remarks>
        /// <param name="peer">the peer to adjust</param>
        /// <param name="timeoutLimit">the timeout limit; defaults to ENET_PEER_TIMEOUT_LIMIT if 0</param>
        /// <param name="timeoutMinimum">the timeout minimum; defaults to ENET_PEER_TIMEOUT_MINIMUM if 0</param>
        /// <param name="timeoutMaximum">the timeout maximum; defaults to ENET_PEER_TIMEOUT_MAXIMUM if 0</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void enet_peer_timeout(ENetPeer* peer, uint timeoutLimit, uint timeoutMinimum, uint timeoutMaximum) => ENet.enet_peer_timeout(peer, timeoutLimit, timeoutMinimum, timeoutMaximum);

        /// <summary>
        ///     Forcefully disconnects a peer.
        /// </summary>
        /// <param name="peer">peer to forcefully disconnect</param>
        /// <remarks>
        ///     The foreign host represented by the peer is not notified of the disconnection and will timeout
        ///     on its connection to the local host.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void enet_peer_reset(ENetPeer* peer) => ENet.enet_peer_reset(peer);

        /// <summary>
        ///     Request a disconnection from a peer.
        /// </summary>
        /// <param name="peer">peer to request a disconnection</param>
        /// <param name="data">data describing the disconnection</param>
        /// <remarks>
        ///     An ENET_EVENT_DISCONNECT event will be generated by enet_host_service()
        ///     once the disconnection is complete.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void enet_peer_disconnect(ENetPeer* peer, uint data) => ENet.enet_peer_disconnect(peer, data);

        /// <summary>
        ///     Force an immediate disconnection from a peer.
        /// </summary>
        /// <param name="peer">peer to disconnect</param>
        /// <param name="data">data describing the disconnection</param>
        /// <remarks>
        ///     No ENET_EVENT_DISCONNECT event will be generated. The foreign peer is not
        ///     guaranteed to receive the disconnect notification, and is reset immediately upon
        ///     return from this function.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void enet_peer_disconnect_now(ENetPeer* peer, uint data) => ENet.enet_peer_disconnect_now(peer, data);

        /// <summary>
        ///     Request a disconnection from a peer, but only after all queued outgoing packets are sent.
        /// </summary>
        /// <param name="peer">peer to request a disconnection</param>
        /// <param name="data">data describing the disconnection</param>
        /// <remarks>
        ///     An ENET_EVENT_DISCONNECT event will be generated by enet_host_service()
        ///     once the disconnection is complete.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void enet_peer_disconnect_later(ENetPeer* peer, uint data) => ENet.enet_peer_disconnect_later(peer, data);

        /// <summary>
        ///     Configures throttle parameter for a peer.
        /// </summary>
        /// <remarks>
        ///     Unreliable packets are dropped by ENet in response to the varying conditions
        ///     of the Internet connection to the peer. The throttle represents a probability
        ///     that an unreliable packet should not be dropped and thus sent by ENet to the peer.
        ///     The lowest mean round trip time from the sending of a reliable packet to the
        ///     receipt of its acknowledgement is measured over an amount of time specified by
        ///     the interval parameter in milliseconds. If a measured round trip time happens to
        ///     be significantly less than the mean round trip time measured over the interval,
        ///     then the throttle probability is increased to allow more traffic by an amount
        ///     specified in the acceleration parameter, which is a ratio to the ENET_PEER_PACKET_THROTTLE_SCALE
        ///     constant. If a measured round trip time happens to be significantly greater than
        ///     the mean round trip time measured over the interval, then the throttle probability
        ///     is decreased to limit traffic by an amount specified in the deceleration parameter, which
        ///     is a ratio to the ENET_PEER_PACKET_THROTTLE_SCALE constant. When the throttle has
        ///     a value of ENET_PEER_PACKET_THROTTLE_SCALE, no unreliable packets are dropped by
        ///     ENet, and so 100% of all unreliable packets will be sent. When the throttle has a
        ///     value of 0, all unreliable packets are dropped by ENet, and so 0% of all unreliable
        ///     packets will be sent. Intermediate values for the throttle represent intermediate
        ///     probabilities between 0% and 100% of unreliable packets being sent. The bandwidth
        ///     limits of the local and foreign hosts are taken into account to determine a
        ///     sensible limit for the throttle probability above which it should not raise even in
        ///     the best of conditions.
        /// </remarks>
        /// <param name="peer">peer to configure</param>
        /// <param name="interval">
        ///     interval, in milliseconds, over which to measure lowest mean RTT; the default value is
        ///     ENET_PEER_PACKET_THROTTLE_INTERVAL.
        /// </param>
        /// <param name="acceleration">rate at which to increase the throttle probability as mean RTT declines</param>
        /// <param name="deceleration">rate at which to decrease the throttle probability as mean RTT increases</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void enet_peer_throttle_configure(ENetPeer* peer, uint interval, uint acceleration, uint deceleration) => ENet.enet_peer_throttle_configure(peer, interval, acceleration, deceleration);
    }
}