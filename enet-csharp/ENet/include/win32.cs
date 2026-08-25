using System;
using System.Buffers.Binary;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using NativeSockets;

#pragma warning disable CS1591

// ReSharper disable ALL

namespace enet
{
    public static partial class ENet
    {
        public const nint INVALID_SOCKET = ~0;

        public const nint ENET_SOCKET_NULL = INVALID_SOCKET;

        public static ushort ENET_HOST_TO_NET_16(ushort host) => BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(host) : host;

        public static uint ENET_HOST_TO_NET_32(uint host) => BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(host) : host;

        public static ushort ENET_NET_TO_HOST_16(ushort network) => BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(network) : network;

        public static uint ENET_NET_TO_HOST_32(uint network) => BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(network) : network;
    }

    public unsafe struct ENetBuffer
    {
        public nuint dataLength;
        public void* data;
    }

    /// <summary>
    ///     Represents a native socket handle with its associated address family.
    /// </summary>
    public readonly struct ENetSocket
    {
        /// <summary>
        ///     The native socket handle.
        /// </summary>
        private readonly NativeSocket _handle;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ENetSocket" /> structure.
        /// </summary>
        /// <param name="handle">The native socket handle.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal ENetSocket(NativeSocket handle) => _handle = handle;

        /// <summary>
        ///     Gets a value that indicates whether this has been allocated or initialized.
        /// </summary>
        public bool IsCreated => _handle.IsCreated;

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
        ///     Gets the native socket handle.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal NativeSocket GetInner() => _handle;
    }
}