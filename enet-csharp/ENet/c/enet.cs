using System;
using System.Runtime.CompilerServices;
using System.Security;

#pragma warning disable CS1591

// ReSharper disable ALL

namespace enet
{
    [SuppressUnmanagedCodeSecurity]
    public static unsafe class ENET_API
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int enet_initialize() => ENet.enet_initialize();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int enet_initialize_with_callbacks(uint version, ENetCallbacks* inits) => ENet.enet_initialize_with_callbacks(version, inits);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void enet_deinitialize() => ENet.enet_deinitialize();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint enet_linked_version() => ENet.enet_linked_version();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint enet_time_get() => ENet.enet_time_get();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void enet_time_set(uint newTimeBase) => ENet.enet_time_set(newTimeBase);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long enet_socket_create(ENetSocketType type) => ENet.enet_socket_create(type);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int enet_socket_bind(long socket, ENetAddress* address) => ENet.enet_socket_bind(socket, address);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int enet_socket_get_address(long socket, ENetAddress* address) => ENet.enet_socket_get_address(socket, address);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int enet_socket_send(long socket, ENetAddress* address, ENetBuffer* buffers, nuint bufferCount) => ENet.enet_socket_send(socket, address, buffers, bufferCount);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int enet_socket_receive(long socket, ENetAddress* address, ENetBuffer* buffers, nuint bufferCount) => ENet.enet_socket_receive(socket, address, buffers, bufferCount);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int enet_socket_wait(long socket, uint* condition, uint timeout) => ENet.enet_socket_wait(socket, condition, timeout);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int enet_socket_set_option(long socket, ENetSocketOption option, int value) => ENet.enet_socket_set_option(socket, option, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void enet_socket_destroy(long* socket) => ENet.enet_socket_destroy(socket);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int enet_address_set_host_ip(ENetAddress* address, ReadOnlySpan<char> hostName) => ENet.enet_address_set_host_ip(address, hostName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int enet_address_set_host(ENetAddress* address, ReadOnlySpan<char> hostName) => ENet.enet_address_set_host(address, hostName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int enet_address_get_host_ip(ENetAddress* address, byte* hostName, nuint nameLength) => ENet.enet_address_get_host_ip(address, hostName, nameLength);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int enet_address_get_host(ENetAddress* address, byte* hostName, nuint nameLength) => ENet.enet_address_get_host(address, hostName, nameLength);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ENetPacket* enet_packet_create(void* data, nuint dataLength, uint flags) => ENet.enet_packet_create(data, dataLength, flags);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void enet_packet_destroy(ENetPacket* packet) => ENet.enet_packet_destroy(packet);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int enet_packet_resize(ENetPacket* packet, nuint dataLength) => ENet.enet_packet_resize(packet, dataLength);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint enet_crc32(ENetBuffer* buffers, nuint bufferCount) => ENet.enet_crc32(buffers, bufferCount);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int enet_host_ping(ENetHost* host, ENetAddress* address) => ENet.enet_host_ping(host, address);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ENetHost* enet_host_create(ENetAddress* address, nuint peerCount, nuint channelLimit, uint incomingBandwidth, uint outgoingBandwidth) => ENet.enet_host_create(address, peerCount, channelLimit, incomingBandwidth, outgoingBandwidth);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void enet_host_destroy(ENetHost* host) => ENet.enet_host_destroy(host);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ENetPeer* enet_host_connect(ENetHost* host, ENetAddress* address, nuint channelCount, uint data) => ENet.enet_host_connect(host, address, channelCount, data);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int enet_host_check_events(ENetHost* host, ENetEvent* @event) => ENet.enet_host_check_events(host, @event);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int enet_host_service(ENetHost* host, ENetEvent* @event, uint timeout) => ENet.enet_host_service(host, @event, timeout);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void enet_host_flush(ENetHost* host) => ENet.enet_host_flush(host);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void enet_host_broadcast(ENetHost* host, byte channelID, ENetPacket* packet) => ENet.enet_host_broadcast(host, channelID, packet);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void enet_host_compress(ENetHost* host, ENetCompressor* compressor) => ENet.enet_host_compress(host, compressor);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void enet_host_channel_limit(ENetHost* host, nuint channelLimit) => ENet.enet_host_channel_limit(host, channelLimit);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void enet_host_bandwidth_limit(ENetHost* host, uint incomingBandwidth, uint outgoingBandwidth) => ENet.enet_host_bandwidth_limit(host, incomingBandwidth, outgoingBandwidth);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int enet_peer_send(ENetPeer* peer, byte channelID, ENetPacket* packet) => ENet.enet_peer_send(peer, channelID, packet);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ENetPacket* enet_peer_receive(ENetPeer* peer, byte* channelID) => ENet.enet_peer_receive(peer, channelID);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void enet_peer_ping(ENetPeer* peer) => ENet.enet_peer_ping(peer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void enet_peer_ping_interval(ENetPeer* peer, uint pingInterval) => ENet.enet_peer_ping_interval(peer, pingInterval);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void enet_peer_timeout(ENetPeer* peer, uint timeoutLimit, uint timeoutMinimum, uint timeoutMaximum) => ENet.enet_peer_timeout(peer, timeoutLimit, timeoutMinimum, timeoutMaximum);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void enet_peer_reset(ENetPeer* peer) => ENet.enet_peer_reset(peer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void enet_peer_disconnect(ENetPeer* peer, uint data) => ENet.enet_peer_disconnect(peer, data);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void enet_peer_disconnect_now(ENetPeer* peer, uint data) => ENet.enet_peer_disconnect_now(peer, data);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void enet_peer_disconnect_later(ENetPeer* peer, uint data) => ENet.enet_peer_disconnect_later(peer, data);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void enet_peer_throttle_configure(ENetPeer* peer, uint interval, uint acceleration, uint deceleration) => ENet.enet_peer_throttle_configure(peer, interval, acceleration, deceleration);
    }
}