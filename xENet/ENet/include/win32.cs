using System;
using System.Buffers.Binary;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using NativeSockets;

// ReSharper disable ALL

namespace enet
{
    public static partial class ENet
    {
        /// <summary>
        ///     The value returned by socket operations to indicate a failure.
        /// </summary>
        public const int SOCKET_ERROR = -1;

        /// <summary>
        ///     The sentinel value representing an invalid native socket handle.
        /// </summary>
        public const nint INVALID_SOCKET = ~0;

        /// <summary>
        ///     The sentinel value used to indicate the absence of a socket.
        /// </summary>
        public const nint ENET_SOCKET_NULL = INVALID_SOCKET;

        /// <summary>
        ///     Converts a 16-bit host-order value to network byte order (big endian).
        /// </summary>
        /// <param name="host">The host-order value to convert.</param>
        /// <returns>The value in network byte order.</returns>
        public static ushort ENET_HOST_TO_NET_16(ushort host) => BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(host) : host;

        /// <summary>
        ///     Converts a 32-bit host-order value to network byte order (big endian).
        /// </summary>
        /// <param name="host">The host-order value to convert.</param>
        /// <returns>The value in network byte order.</returns>
        public static uint ENET_HOST_TO_NET_32(uint host) => BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(host) : host;

        /// <summary>
        ///     Converts a 16-bit network-order value (big endian) to host byte order.
        /// </summary>
        /// <param name="network">The network-order value to convert.</param>
        /// <returns>The value in host byte order.</returns>
        public static ushort ENET_NET_TO_HOST_16(ushort network) => BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(network) : network;

        /// <summary>
        ///     Converts a 32-bit network-order value (big endian) to host byte order.
        /// </summary>
        /// <param name="network">The network-order value to convert.</param>
        /// <returns>The value in host byte order.</returns>
        public static uint ENET_NET_TO_HOST_32(uint network) => BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(network) : network;
    }

    /// <summary>
    ///     Describes a contiguous block of packet data with its length.
    /// </summary>
    public unsafe struct ENetBuffer
    {
        /// <summary>
        ///     The number of valid bytes in the buffer.
        /// </summary>
        public nuint dataLength;

        /// <summary>
        ///     Pointer to the start of the data.
        /// </summary>
        public void* data;
    }

    /// <summary>
    ///     Represents a native socket handle with its associated address family.
    /// </summary>
    public readonly struct ENetSocket
    {
        /// <summary>
        ///     Gets the handle to the underlying object.
        /// </summary>
        private readonly NativeSocket _handle;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ENetSocket" /> structure.
        /// </summary>
        /// <param name="handle">The native socket handle.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ENetSocket(NativeSocket handle) => _handle = handle;

        /// <summary>
        ///     Gets the native socket handle.
        /// </summary>
        public nint Handle => _handle.Handle;

        /// <summary>
        ///     Gets the address family of the socket.
        /// </summary>
        public AddressFamily Family => _handle.Family;

        /// <summary>
        ///     Gets a value indicating whether the socket uses Ipv4.
        /// </summary>
        public bool IsIpv4 => _handle.IsIpv4;

        /// <summary>
        ///     Gets a value indicating whether the socket uses Ipv6.
        /// </summary>
        public bool IsIpv6 => _handle.IsIpv6;

        /// <summary>
        ///     Implicitly converts a <see cref="T:NativeSockets.NativeSocket" /> to its native handle.
        /// </summary>
        /// <param name="socket">The socket to convert.</param>
        /// <returns>The native socket handle.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator nint(ENetSocket socket) => socket.Handle;

        /// <summary>
        ///     Gets the handle to the underlying object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal NativeSocket GetInner() => _handle;
    }
}