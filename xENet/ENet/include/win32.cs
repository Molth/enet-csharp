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
    public readonly struct ENetSocket : IEquatable<ENetSocket>, IComparable<ENetSocket>
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
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public bool Equals(ENetSocket other) => _handle.Equals(other._handle);

        /// <summary>
        ///     Compares the current instance with another object of the same type and returns an integer that indicates
        ///     whether the current instance precedes, follows, or occurs in the same position in the sort order as the other
        ///     object.
        /// </summary>
        /// <param name="other">An object to compare with this instance.</param>
        /// <returns>
        ///     A value that indicates the relative order of the objects being compared. The return value has these meanings:
        ///     <list type="table">
        ///         <listheader>
        ///             <term> Value</term><description> Meaning</description>
        ///         </listheader>
        ///         <item>
        ///             <term> Less than zero</term>
        ///             <description> This instance precedes <paramref name="other" /> in the sort order.</description>
        ///         </item>
        ///         <item>
        ///             <term> Zero</term>
        ///             <description> This instance occurs in the same position in the sort order as <paramref name="other" />.</description>
        ///         </item>
        ///         <item>
        ///             <term> Greater than zero</term>
        ///             <description> This instance follows <paramref name="other" /> in the sort order.</description>
        ///         </item>
        ///     </list>
        /// </returns>
        public int CompareTo(ENetSocket other) => _handle.CompareTo(other._handle);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public override bool Equals(object? obj) => obj is ENetSocket other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public override int GetHashCode() => _handle.GetHashCode();

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(ENetSocket left, ENetSocket right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(ENetSocket left, ENetSocket right) => !left.Equals(right);

        /// <summary>
        ///     Gets the handle to the underlying object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal NativeSocket GetInner() => _handle;
    }
}