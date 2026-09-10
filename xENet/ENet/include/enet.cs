using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using NativeSockets;
using static enet.ENet;

// ReSharper disable ALL

namespace enet
{
    /// <summary>
    ///     ENet reliable UDP networking library
    /// </summary>
    public static partial class ENet
    {
        /// <summary>
        ///     The major component of the ENet version.
        /// </summary>
        public const uint ENET_VERSION_MAJOR = 1;

        /// <summary>
        ///     The minor component of the ENet version.
        /// </summary>
        public const uint ENET_VERSION_MINOR = 3;

        /// <summary>
        ///     The patch component of the ENet version.
        /// </summary>
        public const uint ENET_VERSION_PATCH = 18;

        /// <summary>
        ///     The packed ENet version value.
        /// </summary>
        public const uint ENET_VERSION = 66322;

        /// <summary>
        ///     Packs a major, minor and patch version into a single version value.
        /// </summary>
        /// <param name="major">The major version component.</param>
        /// <param name="minor">The minor version component.</param>
        /// <param name="patch">The patch version component.</param>
        /// <returns>The packed version value.</returns>
        public static uint ENET_VERSION_CREATE(uint major, uint minor, uint patch) => (((major) << 16) | ((minor) << 8) | (patch));

        /// <summary>
        ///     Extracts the major version component from a packed version value.
        /// </summary>
        /// <param name="version">The packed version value.</param>
        /// <returns>The major version component.</returns>
        public static uint ENET_VERSION_GET_MAJOR(uint version) => (((version) >> 16) & 0xFF);

        /// <summary>
        ///     Extracts the minor version component from a packed version value.
        /// </summary>
        /// <param name="version">The packed version value.</param>
        /// <returns>The minor version component.</returns>
        public static uint ENET_VERSION_GET_MINOR(uint version) => (((version) >> 8) & 0xFF);

        /// <summary>
        ///     Extracts the patch version component from a packed version value.
        /// </summary>
        /// <param name="version">The packed version value.</param>
        /// <returns>The patch version component.</returns>
        public static uint ENET_VERSION_GET_PATCH(uint version) => ((version) & 0xFF);
    }

    /// <summary>
    ///     The type of a native socket.
    /// </summary>
    public enum ENetSocketType
    {
        /// <summary>
        ///     A connectionless datagram socket.
        /// </summary>
        ENET_SOCKET_TYPE_DATAGRAM = 2
    }

    /// <summary>
    ///     The wait states that can be requested on a socket.
    /// </summary>
    public enum ENetSocketWait
    {
        /// <summary>
        ///     Wait for no specific state.
        /// </summary>
        ENET_SOCKET_WAIT_NONE = 0,

        /// <summary>
        ///     Wait until the socket can send data.
        /// </summary>
        ENET_SOCKET_WAIT_SEND = (1 << 0),

        /// <summary>
        ///     Wait until the socket can receive data.
        /// </summary>
        ENET_SOCKET_WAIT_RECEIVE = (1 << 1),

        /// <summary>
        ///     Wait until the wait is interrupted.
        /// </summary>
        ENET_SOCKET_WAIT_INTERRUPT = (1 << 2)
    }

    /// <summary>
    ///     The addressing mode used when creating a host.
    /// </summary>
    public enum ENetHostOption
    {
        /// <summary>
        ///     Use Ipv4 addressing only.
        /// </summary>
        ENET_HOSTOPT_IPV4 = 0,

        /// <summary>
        ///     Use Ipv6 addressing only.
        /// </summary>
        ENET_HOSTOPT_IPV6_ONLY = 1,

        /// <summary>
        ///     Use Ipv6 dual stack addressing, accepting both Ipv4 and Ipv6.
        /// </summary>
        ENET_HOSTOPT_IPV6_DUALMODE = 2
    }

    /// <summary>
    ///     The socket options that can be configured on a native socket.
    /// </summary>
    public enum ENetSocketOption
    {
        /// <summary>
        ///     Toggles non-blocking mode on the socket.
        /// </summary>
        ENET_SOCKOPT_NONBLOCK = 1,

        /// <summary>
        ///     Enables or disables broadcast on the socket.
        /// </summary>
        ENET_SOCKOPT_BROADCAST = 2,

        /// <summary>
        ///     Sets the receive buffer size of the socket.
        /// </summary>
        ENET_SOCKOPT_RCVBUF = 3,

        /// <summary>
        ///     Sets the send buffer size of the socket.
        /// </summary>
        ENET_SOCKOPT_SNDBUF = 4,

        /// <summary>
        ///     Allows the socket to reuse a bound address.
        /// </summary>
        ENET_SOCKOPT_REUSEADDR = 5,

        /// <summary>
        ///     Sets the receive timeout of the socket.
        /// </summary>
        ENET_SOCKOPT_RCVTIMEO = 6,

        /// <summary>
        ///     Sets the send timeout of the socket.
        /// </summary>
        ENET_SOCKOPT_SNDTIMEO = 7,

        /// <summary>
        ///     Retrieves the last error of the socket.
        /// </summary>
        ENET_SOCKOPT_ERROR = 8,

        /// <summary>
        ///     Disables the Nagle algorithm on the socket.
        /// </summary>
        ENET_SOCKOPT_NODELAY = 9,

        /// <summary>
        ///     Sets the time to live of packets sent on the socket.
        /// </summary>
        ENET_SOCKOPT_TTL = 10,

        /// <summary>
        ///     Restricts the socket to Ipv6 only.
        /// </summary>
        ENET_SOCKOPT_IPV6_ONLY = 11
    }

    /// <summary>
    ///     The shutdown directions that can be applied to a socket.
    /// </summary>
    public enum ENetSocketShutdown
    {
        /// <summary>
        ///     Shut down reading from the socket.
        /// </summary>
        ENET_SOCKET_SHUTDOWN_READ = 0,

        /// <summary>
        ///     Shut down writing to the socket.
        /// </summary>
        ENET_SOCKET_SHUTDOWN_WRITE = 1,

        /// <summary>
        ///     Shut down both reading and writing to the socket.
        /// </summary>
        ENET_SOCKET_SHUTDOWN_READ_WRITE = 2
    }

    public static partial class ENet
    {
        /// <summary>
        ///     A port value indicating that the operating system should choose an available port.
        /// </summary>
        public const ushort ENET_PORT_ANY = 0;

        /// <summary>
        ///     Initializes the well-known addresses used by the ENet runtime.
        /// </summary>
        static ENet()
        {
            ENET_HOST_ANY_V4.GetInner().FromIpAddress(IPAddress.Any, ENET_PORT_ANY);
            ENET_HOST_ANY_V6.GetInner().FromIpAddress(IPAddress.IPv6Any, ENET_PORT_ANY);
            ENET_HOST_BROADCAST.GetInner().FromIpAddress(IPAddress.Broadcast, ENET_PORT_ANY);
        }

        /// <summary>
        ///     The well-known address representing any Ipv4 host.
        /// </summary>
        public static ENetAddress ENET_HOST_ANY_V4 { get; }

        /// <summary>
        ///     The well-known address representing any Ipv6 host.
        /// </summary>
        public static ENetAddress ENET_HOST_ANY_V6 { get; }

        /// <summary>
        ///     The well-known broadcast address.
        /// </summary>
        public static ENetAddress ENET_HOST_BROADCAST { get; }

        /// <summary>
        ///     The Ipv4 broadcast address bytes.
        /// </summary>
        private static ReadOnlySpan<byte> ENET_ADDRESS_BROADCAST => new byte[4] { 255, 255, 255, 255 };
    }

    /// <summary>
    ///     Portable internet address structure.
    /// </summary>
    /// <remarks>
    ///     The port must be host byte-order.
    ///     <br />
    ///     The constant <see cref="ENET_HOST_ANY_V4" /> or <see cref="ENET_HOST_ANY_V6" /> may be used to specify the default
    ///     server host. The constant <see cref="ENET_HOST_BROADCAST" /> may be used to specify the
    ///     broadcast address (255.255.255.255).  This makes sense for enet_host_connect,
    ///     but not for enet_host_create.  Once a server responds to a broadcast, the
    ///     address is updated from <see cref="ENET_HOST_BROADCAST" /> to the server's actual IP address.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ENetAddress : IEquatable<ENetAddress>, IComparable<ENetAddress>
#if NET6_0_OR_GREATER
        , ISpanFormattable
#else
        , IFormattable
#endif
    {
        /// <summary>
        ///     Gets the handle to the underlying object.
        /// </summary>
        private NativeSocketAddress _handle;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ENetAddress" /> structure.
        /// </summary>
        /// <param name="handle">The native socket address.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ENetAddress(NativeSocketAddress handle) => _handle = handle;

        /// <summary>
        ///     Gets whether the address is an Ipv4 address.
        /// </summary>
        public readonly bool IsIpv4 => _handle.IsIpv4;

        /// <summary>
        ///     Gets whether the address is an Ipv6 address.
        /// </summary>
        public readonly bool IsIpv6 => _handle.IsIpv6;

        /// <summary>
        ///     Gets the address family of the socket address.
        /// </summary>
        public AddressFamily Family
        {
            readonly get => _handle.Family;
            set => _handle.Family = value;
        }

        /// <summary>
        ///     Gets or sets the port number of the socket address.
        /// </summary>
        /// <returns>An unsigned integer value indicating the port number of the socket address.</returns>
        public ushort Port
        {
            readonly get => _handle.Port;
            set => _handle.Port = value;
        }

        /// <summary>
        ///     Gets or sets the Ipv6 address scope identifier.
        /// </summary>
        /// <returns>An unsigned integer that specifies the scope of the address.</returns>
        public uint ScopeId
        {
            readonly get => _handle.ScopeId;
            set => _handle.ScopeId = value;
        }

        /// <summary>
        ///     Gets whether the socket address is an Ipv4-mapped Ipv6 address.
        /// </summary>
        /// <returns>
        ///     Returns true if the socket address is an Ipv4-mapped Ipv6 address;
        ///     otherwise, false.
        /// </returns>
        public readonly bool IsIpv4MappedToIpv6 => _handle.IsIpv4MappedToIpv6;

        /// <summary>
        ///     Gets the underlying buffer size of this.
        /// </summary>
        /// <returns>The underlying buffer size of this.</returns>
        public readonly int Size => _handle.Size;

        /// <summary>
        ///     Gets or sets the specified index element in the underlying buffer.
        /// </summary>
        /// <param name="index">The array index element of the desired information.</param>
        /// <returns>The value of the specified index element in the underlying buffer.</returns>
        /// <exception cref="T:System.ArgumentOutOfRangeException">The specified index does not exist in the buffer.</exception>
        public byte this[int index]
        {
            readonly get => _handle[index];
            set => _handle[index] = value;
        }

        /// <summary>
        ///     Maps the socket address object to an Ipv6 address.
        /// </summary>
        /// <returns>Returns socket address. An Ipv6 address.</returns>
        public readonly ENetAddress MapToIpv6() => new(_handle.MapToIpv6());

        /// <summary>
        ///     Maps the socket address object to an Ipv4 address.
        /// </summary>
        /// <returns>Returns socket address. An Ipv4 address.</returns>
        public readonly ENetAddress MapToIpv4() => new(_handle.MapToIpv4());

        /// <summary>
        ///     Gets the underlying memory that can be passed to native OS calls.
        /// </summary>
        public Span<byte> Buffer => _handle.Buffer;

        /// <summary>
        ///     Gets the ip address of the endpoint.
        /// </summary>
        public Span<byte> Address => _handle.Address;

        /// <summary>
        ///     Returns a span that represents the raw (28 bytes) buffer of the address.
        /// </summary>
        /// <returns>A span of bytes.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<byte> AsSpan() => _handle.AsSpan();

        /// <summary>
        ///     Returns a read-only span that represents the raw (28 bytes) buffer of the address.
        /// </summary>
        /// <returns>A read-only span of bytes.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly ReadOnlySpan<byte> AsReadOnlySpan() => _handle.AsReadOnlySpan();

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(ENetAddress other) => _handle.Equals(other._handle);

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
        public readonly int CompareTo(ENetAddress other) => _handle.CompareTo(other._handle);

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public readonly override bool Equals(object? obj) => obj is ENetAddress other && other.Equals(this);

        /// <summary>
        ///     Returns the hash code for this instance.
        /// </summary>
        public readonly override int GetHashCode() => _handle.GetHashCode();

        /// <summary>
        ///     Indicates whether the current object is equal to another object.
        /// </summary>
        public static bool operator ==(ENetAddress left, ENetAddress right) => left.Equals(right);

        /// <summary>
        ///     Indicates whether the current object is not equal to another object.
        /// </summary>
        public static bool operator !=(ENetAddress left, ENetAddress right) => !left.Equals(right);

        /// <summary>
        ///     Returns information about the socket address.
        /// </summary>
        /// <returns>A string that contains information about this.</returns>
        public readonly override string ToString() => _handle.ToString();

        /// <summary>
        ///     Tries to format the current socket address into the provided span.
        /// </summary>
        /// <param name="destination">When this method returns, the socket address as a span of characters.</param>
        /// <param name="charsWritten">When this method returns, the number of characters written into the span.</param>
        /// <returns>
        ///     <see langword="true" /> if the formatting was successful;
        ///     otherwise, <see langword="false" />.
        /// </returns>
        public readonly bool TryFormat(Span<char> destination, out int charsWritten) => _handle.TryFormat(destination, out charsWritten);

        /// <summary>
        ///     Returns the string representation of the current socket address.
        /// </summary>
        /// <param name="_">The format specifier (ignored).</param>
        /// <param name="__">The format provider (ignored).</param>
        /// <returns>A string representation of the socket address.</returns>
        public readonly string ToString(string? _, IFormatProvider? __) => _handle.ToString(_, __);

        /// <summary>
        ///     Tries to format the current socket address into the provided span.
        /// </summary>
        /// <param name="destination">The span to receive the formatted characters.</param>
        /// <param name="charsWritten">When this method returns, the number of characters written.</param>
        /// <param name="_">The format specifier (ignored).</param>
        /// <param name="__">The format provider (ignored).</param>
        /// <returns><see langword="true" /> if the formatting succeeded; otherwise, <see langword="false" />.</returns>
        public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> _, IFormatProvider? __) => _handle.TryFormat(destination, out charsWritten, _, __);

        /// <summary>
        ///     Initializes a new instance of the <see cref="IPEndPoint" /> class with the specified address and port number.
        /// </summary>
        /// <param name="result">A new instance of the <see cref="IPEndPoint" /> class.</param>
        /// <returns>
        ///     <see cref="SocketError.Success" /> if successful;
        ///     <see cref="SocketError.AddressFamilyNotSupported" /> if the address family is not Ipv4 or Ipv6.
        /// </returns>
        public readonly SocketError ToIpEndPoint(out IPEndPoint? result) => _handle.ToIpEndPoint(out result);

        /// <summary>
        ///     Initializes a new instance of the <see cref="IPAddress" /> class with the specified address.
        /// </summary>
        /// <param name="result">A new instance of the <see cref="IPAddress" /> class.</param>
        /// <returns>
        ///     <see cref="SocketError.Success" /> if successful;
        ///     <see cref="SocketError.AddressFamilyNotSupported" /> if the address family is not Ipv4 or Ipv6.
        /// </returns>
        public readonly SocketError ToIpAddress(out IPAddress? result) => _handle.ToIpAddress(out result);

        /// <summary>
        ///     Initializes a new instance of the <see cref="SocketAddress" /> class with the specified address.
        /// </summary>
        /// <param name="result">A new instance of the <see cref="SocketAddress" /> class.</param>
        /// <returns>
        ///     <see cref="SocketError.Success" /> if successful;
        ///     <see cref="SocketError.AddressFamilyNotSupported" /> if the address family is not Ipv4 or Ipv6.
        /// </returns>
        public readonly SocketError ToSocketAddress(out SocketAddress? result) => _handle.ToSocketAddress(out result);

        /// <summary>
        ///     Populates this address from the specified <see cref="IPEndPoint" />.
        /// </summary>
        /// <param name="source">The <see cref="IPEndPoint" /> containing the ip address and port.</param>
        /// <returns>
        ///     <see cref="SocketError.Success" /> if successful;
        ///     <see cref="SocketError.AddressFamilyNotSupported" /> if the address family is not Ipv4 or Ipv6.
        /// </returns>
        /// <exception cref="NullReferenceException">Thrown if <paramref name="source" /> is null.</exception>
        public SocketError FromIpEndPoint(IPEndPoint source) => _handle.FromIpEndPoint(source);

        /// <summary>
        ///     Populates this address from the specified <see cref="IPAddress" /> and port.
        /// </summary>
        /// <param name="source">The <see cref="IPAddress" /> to set.</param>
        /// <param name="port">The port number.</param>
        /// <returns>
        ///     <see cref="SocketError.Success" /> if successful;
        ///     <see cref="SocketError.AddressFamilyNotSupported" /> if the address family is not Ipv4 or Ipv6.
        /// </returns>
        /// <exception cref="NullReferenceException">Thrown if <paramref name="source" /> is null.</exception>
        public SocketError FromIpAddress(IPAddress source, ushort port) => _handle.FromIpAddress(source, port);

        /// <summary>
        ///     Populates this address from the specified <see cref="SocketAddress" />.
        /// </summary>
        /// <param name="source">The source <see cref="SocketAddress" /> to copy from.</param>
        /// <returns>
        ///     <see cref="SocketError.Success" /> if the address is valid and copied successfully;
        ///     <see cref="SocketError.AddressFamilyNotSupported" /> if the address family is not Ipv4 or Ipv6,
        ///     or the buffer size is insufficient.
        /// </returns>
        /// <exception cref="NullReferenceException">Thrown if <paramref name="source" /> is null.</exception>
        public SocketError FromSocketAddress(SocketAddress source) => _handle.FromSocketAddress(source);

        /// <summary>
        ///     Converts an Ipv4 address and port into this address.
        /// </summary>
        /// <param name="ip">The ip address as a span of characters.</param>
        /// <param name="port">The port number.</param>
        /// <returns><see cref="SocketError.Success" /> if successful; otherwise an error code.</returns>
        public SocketError SetIpIpv4(ReadOnlySpan<char> ip, ushort port) => _handle.SetIpIpv4(ip, port);

        /// <summary>
        ///     Converts an Ipv6 address, port, and scope id into this address.
        /// </summary>
        /// <param name="ip">The ip address as a span of characters.</param>
        /// <param name="port">The port number.</param>
        /// <param name="scopeId">The scope id for the Ipv6 address.</param>
        /// <returns><see cref="SocketError.Success" /> if successful; otherwise an error code.</returns>
        public SocketError SetIpIpv6(ReadOnlySpan<char> ip, ushort port, uint scopeId = 0) => _handle.SetIpIpv6(ip, port, scopeId);

        /// <summary>
        ///     Populates this address by resolving the specified host name to an Ipv4 address.
        /// </summary>
        /// <param name="hostName">The host name to resolve (e.g., "localhost", "example.com").</param>
        /// <param name="port">The port number.</param>
        /// <returns><see cref="SocketError.Success" /> on success; otherwise an error code.</returns>
        public SocketError SetHostNameIpv4(ReadOnlySpan<char> hostName, ushort port) => _handle.SetHostNameIpv4(hostName, port);

        /// <summary>
        ///     Populates this address by resolving the specified host name to an Ipv6 address.
        /// </summary>
        /// <param name="hostName">The host name to resolve (e.g., "localhost", "example.com").</param>
        /// <param name="port">The port number.</param>
        /// <param name="scopeId">The Ipv6 scope identifier (used for link-local or site-local addresses).</param>
        /// <returns><see cref="SocketError.Success" /> on success; otherwise an error code.</returns>
        public SocketError SetHostNameIpv6(ReadOnlySpan<char> hostName, ushort port, uint scopeId = 0) => _handle.SetHostNameIpv6(hostName, port, scopeId);

        /// <summary>
        ///     Retrieves the address from this socket address as a character span.
        /// </summary>
        /// <param name="ip">A span to receive the address chars. On success, it is resized to the actual character count.</param>
        /// <returns><see cref="SocketError.Success" /> if successful; otherwise an error code.</returns>
        public readonly SocketError GetIp(ref Span<char> ip) => _handle.GetIp(ref ip);

        /// <summary>
        ///     Retrieves the host name (reverse DNS) from this address.
        /// </summary>
        /// <param name="hostName">A span to receive the host name chars. On success, it is resized to the actual character count.</param>
        /// <returns><see cref="SocketError.Success" /> if successful; otherwise an error code.</returns>
        public readonly SocketError GetHostName(ref Span<char> hostName) => _handle.GetHostName(ref hostName);

        /// <summary>
        ///     Gets the handle to the underlying object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference
        internal ref NativeSocketAddress GetInner() => ref _handle;
#pragma warning restore CS9084 // Struct member returns 'this' or other instance members by reference
    }

    /// <summary>
    ///     Packet flag bit constants.
    /// </summary>
    /// <seealso cref="ENetPacket" />
    [Flags]
    public enum ENetPacketFlag
    {
        /// <summary>
        ///     packet must be received by the target peer and resend attempts should be
        ///     made until the packet is delivered
        /// </summary>
        ENET_PACKET_FLAG_RELIABLE = (1 << 0),

        /// <summary>
        ///     packet will not be sequenced with other packets
        /// </summary>
        ENET_PACKET_FLAG_UNSEQUENCED = (1 << 1),

        /// <summary>
        ///     packet will not allocate data, and user must supply it instead
        /// </summary>
        ENET_PACKET_FLAG_NO_ALLOCATE = (1 << 2),

        /// <summary>
        ///     packet will be fragmented using unreliable (instead of reliable) sends
        ///     if it exceeds the MTU
        /// </summary>
        ENET_PACKET_FLAG_UNRELIABLE_FRAGMENT = (1 << 3),

        /// <summary>
        ///     whether the packet has been sent from all queues it has been entered into
        /// </summary>
        ENET_PACKET_FLAG_SENT = (1 << 8)
    }

    /// <summary>
    ///     ENet packet structure.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         An ENet data packet that may be sent to or received from a peer. The shown
    ///         fields should only be read and never modified. The data field contains the
    ///         allocated data for the packet. The dataLength fields specifies the length
    ///         of the allocated data. The flags field is either 0 (specifying no flags),
    ///         or a bitwise-or of any combination of the following flags:
    ///     </para>
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 <c>ENET_PACKET_FLAG_RELIABLE</c> - packet must be received by the target peer
    ///                 and resend attempts should be made until the packet is delivered
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <c>ENET_PACKET_FLAG_UNSEQUENCED</c> - packet will not be sequenced with other packets
    ///                 (not supported for reliable packets)
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <c>ENET_PACKET_FLAG_NO_ALLOCATE</c> - packet will not allocate data, and user must supply it
    ///                 instead
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <c>ENET_PACKET_FLAG_UNRELIABLE_FRAGMENT</c> - packet will be fragmented using unreliable
    ///                 (instead of reliable) sends if it exceeds the MTU
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <c>ENET_PACKET_FLAG_SENT</c> - whether the packet has been sent from all queues it has been
    ///                 entered into
    ///             </description>
    ///         </item>
    ///     </list>
    /// </remarks>
    /// <seealso cref="ENetPacketFlag" />
    public unsafe struct ENetPacket
    {
        /// <summary>
        ///     internal use only
        /// </summary>
        public nuint referenceCount;

        /// <summary>
        ///     bitwise-or of ENetPacketFlag constants
        /// </summary>
        public uint flags;

        /// <summary>
        ///     allocated data for packet
        /// </summary>
        public byte* data;

        /// <summary>
        ///     length of data
        /// </summary>
        public nuint dataLength;

        /// <summary>
        ///     function to be called when the packet is no longer in use
        /// </summary>
        public delegate* managed<ENetPacket*, void> freeCallback;

        /// <summary>
        ///     application private data, may be freely modified
        /// </summary>
        public void* userData;
    }

    /// <summary>
    ///     An acknowledgement tracking a reliably sent command waiting for confirmation.
    /// </summary>
    public struct ENetAcknowledgement
    {
        /// <summary>
        ///     The list node linking this acknowledgement into the peer acknowledgement queue.
        /// </summary>
        public ENetListNode acknowledgementList;

        /// <summary>
        ///     The time the acknowledged command was sent.
        /// </summary>
        public uint sentTime;

        /// <summary>
        ///     The protocol command being acknowledged.
        /// </summary>
        public ENetProtocol command;
    }

    /// <summary>
    ///     A command queued for transmission to a peer.
    /// </summary>
    public unsafe struct ENetOutgoingCommand
    {
        /// <summary>
        ///     The list node linking this command into the peer outgoing command queue.
        /// </summary>
        public ENetListNode outgoingCommandList;

        /// <summary>
        ///     The reliable sequence number assigned to this command.
        /// </summary>
        public ushort reliableSequenceNumber;

        /// <summary>
        ///     The unreliable sequence number assigned to this command.
        /// </summary>
        public ushort unreliableSequenceNumber;

        /// <summary>
        ///     The time this command was last sent.
        /// </summary>
        public uint sentTime;

        /// <summary>
        ///     The timeout in milliseconds after which the command is resent.
        /// </summary>
        public uint roundTripTimeout;

        /// <summary>
        ///     The time this command was enqueued.
        /// </summary>
        public uint queueTime;

        /// <summary>
        ///     The byte offset of the current fragment within the packet.
        /// </summary>
        public uint fragmentOffset;

        /// <summary>
        ///     The length in bytes of the current fragment payload.
        /// </summary>
        public ushort fragmentLength;

        /// <summary>
        ///     The number of times this command has been sent.
        /// </summary>
        public ushort sendAttempts;

        /// <summary>
        ///     The protocol command data to transmit.
        /// </summary>
        public ENetProtocol command;

        /// <summary>
        ///     The packet associated with this command, or <see langword="null" /> for control commands.
        /// </summary>
        public ENetPacket* packet;
    }

    /// <summary>
    ///     A command received from a peer awaiting processing or reassembly.
    /// </summary>
    public unsafe struct ENetIncomingCommand
    {
        /// <summary>
        ///     The list node linking this command into the peer incoming command queue.
        /// </summary>
        public ENetListNode incomingCommandList;

        /// <summary>
        ///     The reliable sequence number of this command.
        /// </summary>
        public ushort reliableSequenceNumber;

        /// <summary>
        ///     The unreliable sequence number of this command.
        /// </summary>
        public ushort unreliableSequenceNumber;

        /// <summary>
        ///     The protocol command data received.
        /// </summary>
        public ENetProtocol command;

        /// <summary>
        ///     The total number of fragments expected for the fragmented packet.
        /// </summary>
        public uint fragmentCount;

        /// <summary>
        ///     The number of fragments still awaiting arrival.
        /// </summary>
        public uint fragmentsRemaining;

        /// <summary>
        ///     The bit field tracking which fragments have been received.
        /// </summary>
        public uint* fragments;

        /// <summary>
        ///     The packet being assembled from this command.
        /// </summary>
        public ENetPacket* packet;
    }

    /// <summary>
    ///     The connection states a peer can occupy.
    /// </summary>
    public enum ENetPeerState
    {
        /// <summary>
        ///     The peer is not connected.
        /// </summary>
        ENET_PEER_STATE_DISCONNECTED = 0,

        /// <summary>
        ///     A connection attempt is in progress.
        /// </summary>
        ENET_PEER_STATE_CONNECTING = 1,

        /// <summary>
        ///     The connection request has been acknowledged and awaits the final confirmation.
        /// </summary>
        ENET_PEER_STATE_ACKNOWLEDGING_CONNECT = 2,

        /// <summary>
        ///     The connection is pending but has not yet been established.
        /// </summary>
        ENET_PEER_STATE_CONNECTION_PENDING = 3,

        /// <summary>
        ///     The connection attempt has succeeded.
        /// </summary>
        ENET_PEER_STATE_CONNECTION_SUCCEEDED = 4,

        /// <summary>
        ///     The peer is connected and ready for communication.
        /// </summary>
        ENET_PEER_STATE_CONNECTED = 5,

        /// <summary>
        ///     The peer will be disconnected after pending commands are flushed.
        /// </summary>
        ENET_PEER_STATE_DISCONNECT_LATER = 6,

        /// <summary>
        ///     A disconnect request is in progress.
        /// </summary>
        ENET_PEER_STATE_DISCONNECTING = 7,

        /// <summary>
        ///     The disconnect request has been acknowledged and awaits the final confirmation.
        /// </summary>
        ENET_PEER_STATE_ACKNOWLEDGING_DISCONNECT = 8,

        /// <summary>
        ///     The peer connection has been terminated and only awaits cleanup.
        /// </summary>
        ENET_PEER_STATE_ZOMBIE = 9
    }

    public static partial class ENet
    {
        /// <summary>
        ///     The maximum number of buffers used when assembling a packet.
        /// </summary>
        public const uint ENET_BUFFER_MAXIMUM = (1 + 2 * ENET_PROTOCOL_MAXIMUM_PACKET_COMMANDS);

        /// <summary>
        ///     The default size of the host receive buffer in bytes.
        /// </summary>
        public const uint ENET_HOST_RECEIVE_BUFFER_SIZE = 256 * 1024;

        /// <summary>
        ///     The default size of the host send buffer in bytes.
        /// </summary>
        public const uint ENET_HOST_SEND_BUFFER_SIZE = 256 * 1024;

        /// <summary>
        ///     The interval in milliseconds between bandwidth throttle computations.
        /// </summary>
        public const uint ENET_HOST_BANDWIDTH_THROTTLE_INTERVAL = 1000;

        /// <summary>
        ///     The default maximum transmission unit used when the peer does not negotiate one.
        /// </summary>
        public const uint ENET_HOST_DEFAULT_MTU = 1392;

        /// <summary>
        ///     The default maximum packet size that may be sent or received on a peer.
        /// </summary>
        public const uint ENET_HOST_DEFAULT_MAXIMUM_PACKET_SIZE = 32 * 1024 * 1024;

        /// <summary>
        ///     The default maximum aggregate amount of buffer space a peer may use waiting for delivery.
        /// </summary>
        public const uint ENET_HOST_DEFAULT_MAXIMUM_WAITING_DATA = 32 * 1024 * 1024;

        /// <summary>
        ///     The default round trip time in milliseconds assumed for a new peer.
        /// </summary>
        public const uint ENET_PEER_DEFAULT_ROUND_TRIP_TIME = 500;

        /// <summary>
        ///     The default packet throttle value assumed for a new peer.
        /// </summary>
        public const uint ENET_PEER_DEFAULT_PACKET_THROTTLE = 32;

        /// <summary>
        ///     The scale factor applied to the packet throttle value.
        /// </summary>
        public const uint ENET_PEER_PACKET_THROTTLE_SCALE = 32;

        /// <summary>
        ///     The number of throttle samples collected before the throttle value is adjusted.
        /// </summary>
        public const uint ENET_PEER_PACKET_THROTTLE_COUNTER = 7;

        /// <summary>
        ///     The rate at which the packet throttle value is increased on successful samples.
        /// </summary>
        public const uint ENET_PEER_PACKET_THROTTLE_ACCELERATION = 2;

        /// <summary>
        ///     The rate at which the packet throttle value is decreased on failed samples.
        /// </summary>
        public const uint ENET_PEER_PACKET_THROTTLE_DECELERATION = 2;

        /// <summary>
        ///     The default interval in milliseconds between packet throttle measurements.
        /// </summary>
        public const uint ENET_PEER_PACKET_THROTTLE_INTERVAL = 5000;

        /// <summary>
        ///     The scale factor applied to the packet loss ratio.
        /// </summary>
        public const uint ENET_PEER_PACKET_LOSS_SCALE = (1 << 16);

        /// <summary>
        ///     The interval in milliseconds over which packet loss is measured.
        /// </summary>
        public const uint ENET_PEER_PACKET_LOSS_INTERVAL = 10000;

        /// <summary>
        ///     The scale factor applied to the window size.
        /// </summary>
        public const uint ENET_PEER_WINDOW_SIZE_SCALE = 64 * 1024;

        /// <summary>
        ///     The default timeout limit in milliseconds before a connection is considered timed out.
        /// </summary>
        public const uint ENET_PEER_TIMEOUT_LIMIT = 32;

        /// <summary>
        ///     The default minimum timeout in milliseconds before a connection is considered timed out.
        /// </summary>
        public const uint ENET_PEER_TIMEOUT_MINIMUM = 5000;

        /// <summary>
        ///     The default maximum timeout in milliseconds before a connection is considered timed out.
        /// </summary>
        public const uint ENET_PEER_TIMEOUT_MAXIMUM = 30000;

        /// <summary>
        ///     The default interval in milliseconds between keep-alive pings.
        /// </summary>
        public const uint ENET_PEER_PING_INTERVAL = 500;

        /// <summary>
        ///     The number of windows used to track unsequenced packets.
        /// </summary>
        public const uint ENET_PEER_UNSEQUENCED_WINDOWS = 64;

        /// <summary>
        ///     The number of unsequenced packets tracked per window.
        /// </summary>
        public const uint ENET_PEER_UNSEQUENCED_WINDOW_SIZE = 1024;

        /// <summary>
        ///     The number of free unsequenced windows a peer may shift.
        /// </summary>
        public const uint ENET_PEER_FREE_UNSEQUENCED_WINDOWS = 32;

        /// <summary>
        ///     The number of windows used to track reliable packets.
        /// </summary>
        public const uint ENET_PEER_RELIABLE_WINDOWS = 16;

        /// <summary>
        ///     The number of reliable packets tracked per window.
        /// </summary>
        public const uint ENET_PEER_RELIABLE_WINDOW_SIZE = 0x1000;

        /// <summary>
        ///     The number of free reliable windows a peer may shift.
        /// </summary>
        public const uint ENET_PEER_FREE_RELIABLE_WINDOWS = 8;
    }

    /// <summary>
    ///     The per-channel state tracking reliable and unreliable sequencing for a peer.
    /// </summary>
    public unsafe struct ENetChannel
    {
        /// <summary>
        ///     The next reliable sequence number to assign on this channel.
        /// </summary>
        public ushort outgoingReliableSequenceNumber;

        /// <summary>
        ///     The next unreliable sequence number to assign on this channel.
        /// </summary>
        public ushort outgoingUnreliableSequenceNumber;

        /// <summary>
        ///     The number of reliable windows currently in use.
        /// </summary>
        public ushort usedReliableWindows;

        /// <summary>
        ///     The sliding windows tracking which reliable packets have been sent on this channel.
        /// </summary>
        public fixed ushort reliableWindows[(int)ENET_PEER_RELIABLE_WINDOWS];

        /// <summary>
        ///     The highest reliable sequence number received on this channel.
        /// </summary>
        public ushort incomingReliableSequenceNumber;

        /// <summary>
        ///     The highest unreliable sequence number received on this channel.
        /// </summary>
        public ushort incomingUnreliableSequenceNumber;

        /// <summary>
        ///     The queue of reliable commands received on this channel.
        /// </summary>
        public ENetList incomingReliableCommands;

        /// <summary>
        ///     The queue of unreliable commands received on this channel.
        /// </summary>
        public ENetList incomingUnreliableCommands;
    }

    /// <summary>
    ///     The flags that can be set on a peer.
    /// </summary>
    public enum ENetPeerFlag
    {
        /// <summary>
        ///     Indicates the peer has events waiting to be dispatched.
        /// </summary>
        ENET_PEER_FLAG_NEEDS_DISPATCH = (1 << 0),

        /// <summary>
        ///     Indicates the peer still has commands queued for sending.
        /// </summary>
        ENET_PEER_FLAG_CONTINUE_SENDING = (1 << 1)
    }

    /// <summary>
    ///     An ENet peer which data packets may be sent or received from.
    /// </summary>
    /// <remarks>
    ///     No fields should be modified unless otherwise specified.
    /// </remarks>
    public unsafe struct ENetPeer
    {
        /// <summary>
        ///     The list node linking this peer into the host dispatch queue.
        /// </summary>
        public ENetListNode dispatchList;

        /// <summary>
        ///     The host this peer belongs to.
        /// </summary>
        public ENetHost* host;

        /// <summary>
        ///     The peer identifier by which the remote side knows this peer.
        /// </summary>
        public ushort outgoingPeerID;

        /// <summary>
        ///     The peer identifier by which this host knows the remote peer.
        /// </summary>
        public ushort incomingPeerID;

        /// <summary>
        ///     A unique value identifying the connection attempt.
        /// </summary>
        public uint connectID;

        /// <summary>
        ///     The session identifier of the outgoing connection.
        /// </summary>
        public byte outgoingSessionID;

        /// <summary>
        ///     The session identifier of the incoming connection.
        /// </summary>
        public byte incomingSessionID;

        /// <summary>
        ///     Internet address of the peer
        /// </summary>
        public ENetAddress address;

        /// <summary>
        ///     Application private data, may be freely modified
        /// </summary>
        public void* data;

        /// <summary>
        ///     The current connection state of the peer.
        /// </summary>
        public ENetPeerState state;

        /// <summary>
        ///     The array of channels allocated for the peer.
        /// </summary>
        public ENetChannel* channels;

        /// <summary>
        ///     Number of channels allocated for communication with peer
        /// </summary>
        public nuint channelCount;

        /// <summary>
        ///     Downstream bandwidth of the client in bytes/second
        /// </summary>
        public uint incomingBandwidth;

        /// <summary>
        ///     Upstream bandwidth of the client in bytes/second
        /// </summary>
        public uint outgoingBandwidth;

        /// <summary>
        ///     The time of the next incoming bandwidth throttle sample.
        /// </summary>
        public uint incomingBandwidthThrottleEpoch;

        /// <summary>
        ///     The time of the next outgoing bandwidth throttle sample.
        /// </summary>
        public uint outgoingBandwidthThrottleEpoch;

        /// <summary>
        ///     The total number of bytes received from the peer.
        /// </summary>
        public uint incomingDataTotal;

        /// <summary>
        ///     The total number of bytes sent to the peer.
        /// </summary>
        public uint outgoingDataTotal;

        /// <summary>
        ///     The last time data was sent to the peer.
        /// </summary>
        public uint lastSendTime;

        /// <summary>
        ///     The last time data was received from the peer.
        /// </summary>
        public uint lastReceiveTime;

        /// <summary>
        ///     The time at which the current timeout period expires.
        /// </summary>
        public uint nextTimeout;

        /// <summary>
        ///     The earliest time at which a pending command times out.
        /// </summary>
        public uint earliestTimeout;

        /// <summary>
        ///     The time of the next packet loss sample.
        /// </summary>
        public uint packetLossEpoch;

        /// <summary>
        ///     The total number of reliable packets sent to the peer.
        /// </summary>
        public uint packetsSent;

        /// <summary>
        ///     The total number of reliable packets lost to the peer.
        /// </summary>
        public uint packetsLost;

        /// <summary>
        ///     mean packet loss of reliable packets as a ratio with respect to the constant ENET_PEER_PACKET_LOSS_SCALE
        /// </summary>
        public uint packetLoss;

        /// <summary>
        ///     The variance of the packet loss ratio.
        /// </summary>
        public uint packetLossVariance;

        /// <summary>
        ///     The current packet throttle value in the range zero to ENET_PEER_PACKET_THROTTLE_SCALE.
        /// </summary>
        public uint packetThrottle;

        /// <summary>
        ///     The maximum packet throttle value allowed for the peer.
        /// </summary>
        public uint packetThrottleLimit;

        /// <summary>
        ///     The number of samples collected within the current throttle interval.
        /// </summary>
        public uint packetThrottleCounter;

        /// <summary>
        ///     The time of the next packet throttle sample.
        /// </summary>
        public uint packetThrottleEpoch;

        /// <summary>
        ///     The packet throttle acceleration rate.
        /// </summary>
        public uint packetThrottleAcceleration;

        /// <summary>
        ///     The packet throttle deceleration rate.
        /// </summary>
        public uint packetThrottleDeceleration;

        /// <summary>
        ///     The packet throttle measurement interval in milliseconds.
        /// </summary>
        public uint packetThrottleInterval;

        /// <summary>
        ///     The interval in milliseconds between keep-alive pings.
        /// </summary>
        public uint pingInterval;

        /// <summary>
        ///     The timeout limit in milliseconds before the connection is considered timed out.
        /// </summary>
        public uint timeoutLimit;

        /// <summary>
        ///     The minimum timeout in milliseconds before the connection is considered timed out.
        /// </summary>
        public uint timeoutMinimum;

        /// <summary>
        ///     The maximum timeout in milliseconds before the connection is considered timed out.
        /// </summary>
        public uint timeoutMaximum;

        /// <summary>
        ///     The most recently measured round trip time in milliseconds.
        /// </summary>
        public uint lastRoundTripTime;

        /// <summary>
        ///     The lowest round trip time ever measured for the peer.
        /// </summary>
        public uint lowestRoundTripTime;

        /// <summary>
        ///     The variance of the most recent round trip time measurement.
        /// </summary>
        public uint lastRoundTripTimeVariance;

        /// <summary>
        ///     The highest round trip time variance ever measured for the peer.
        /// </summary>
        public uint highestRoundTripTimeVariance;

        /// <summary>
        ///     mean round trip time (RTT), in milliseconds, between sending a reliable packet and receiving its acknowledgement
        /// </summary>
        public uint roundTripTime;

        /// <summary>
        ///     The variance of the round trip time.
        /// </summary>
        public uint roundTripTimeVariance;

        /// <summary>
        ///     The maximum transmission unit negotiated for the peer.
        /// </summary>
        public uint mtu;

        /// <summary>
        ///     The receive window size negotiated for the peer.
        /// </summary>
        public uint windowSize;

        /// <summary>
        ///     The number of reliable bytes currently awaiting acknowledgement.
        /// </summary>
        public uint reliableDataInTransit;

        /// <summary>
        ///     The next reliable sequence number to assign on the peer.
        /// </summary>
        public ushort outgoingReliableSequenceNumber;

        /// <summary>
        ///     The queue of acknowledgements awaiting transmission.
        /// </summary>
        public ENetList acknowledgements;

        /// <summary>
        ///     The queue of reliable commands sent but not yet acknowledged.
        /// </summary>
        public ENetList sentReliableCommands;

        /// <summary>
        ///     The queue of reliable commands waiting to be sent.
        /// </summary>
        public ENetList outgoingSendReliableCommands;

        /// <summary>
        ///     The queue of commands waiting to be sent.
        /// </summary>
        public ENetList outgoingCommands;

        /// <summary>
        ///     The queue of commands awaiting dispatch to the application.
        /// </summary>
        public ENetList dispatchedCommands;

        /// <summary>
        ///     The flags set on this peer.
        /// </summary>
        public ushort flags;

        /// <summary>
        ///     Reserved for internal use.
        /// </summary>
        public ushort reserved;

        /// <summary>
        ///     The highest unsequenced group received from the peer.
        /// </summary>
        public ushort incomingUnsequencedGroup;

        /// <summary>
        ///     The next unsequenced group to assign on the peer.
        /// </summary>
        public ushort outgoingUnsequencedGroup;

        /// <summary>
        ///     The sliding windows tracking which unsequenced packets have been received.
        /// </summary>
        public fixed uint unsequencedWindow[(int)ENET_PEER_UNSEQUENCED_WINDOW_SIZE / 32];

        /// <summary>
        ///     Application defined data carried with the last disconnect event.
        /// </summary>
        public uint eventData;

        /// <summary>
        ///     The total number of bytes queued waiting to be delivered to the peer.
        /// </summary>
        public nuint totalWaitingData;
    }

    /// <summary>
    ///     An ENet packet compressor for compressing UDP packets before socket sends or receives.
    /// </summary>
    public unsafe struct ENetCompressor
    {
        /// <summary>
        ///     Context data for the compressor. Must be non-NULL.
        /// </summary>
        public void* context;

        /// <summary>
        ///     Compresses from inBuffers[0:inBufferCount-1], containing inLimit bytes, to outData, outputting at most outLimit
        ///     bytes. Should return 0 on failure.
        /// </summary>
        public delegate* managed<void*, ENetBuffer*, nuint, nuint, byte*, nuint, nuint> compress;

        /// <summary>
        ///     Decompresses from inData, containing inLimit bytes, to outData, outputting at most outLimit bytes. Should return 0
        ///     on failure.
        /// </summary>
        public delegate* managed<void*, byte*, nuint, byte*, nuint, nuint> decompress;

        /// <summary>
        ///     Destroys the context when compression is disabled or the host is destroyed. May be NULL.
        /// </summary>
        public delegate* managed<void*, void> destroy;

        /// <summary>
        ///     Initializes the compressor with the specified context and callbacks.
        /// </summary>
        /// <param name="context">The compressor context data.</param>
        /// <param name="compress">The compression callback.</param>
        /// <param name="decompress">The decompression callback.</param>
        /// <param name="destroy">The context destruction callback, or <see langword="null" />.</param>
        public ENetCompressor(void* context, delegate* managed<void*, ENetBuffer*, nuint, nuint, byte*, nuint, nuint> compress, delegate* managed<void*, byte*, nuint, byte*, nuint, nuint> decompress, delegate* managed<void*, void> destroy)
        {
            this.context = context;
            this.compress = compress;
            this.decompress = decompress;
            this.destroy = destroy;
        }
    }

    /// <summary>
    ///     An ENet host for communicating with peers.
    /// </summary>
    /// <remarks>
    ///     No fields should be modified unless otherwise stated.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ENetHost
    {
        /// <summary>
        ///     The native socket the host receives and sends data on.
        /// </summary>
        public ENetSocket socket;

        /// <summary>
        ///     Internet address of the host
        /// </summary>
        public ENetAddress address;

        /// <summary>
        ///     downstream bandwidth of the host
        /// </summary>
        public uint incomingBandwidth;

        /// <summary>
        ///     upstream bandwidth of the host
        /// </summary>
        public uint outgoingBandwidth;

        /// <summary>
        ///     The time of the next bandwidth throttle computation.
        /// </summary>
        public uint bandwidthThrottleEpoch;

        /// <summary>
        ///     The maximum transmission unit of the host socket.
        /// </summary>
        public uint mtu;

        /// <summary>
        ///     The random seed used to generate connect identifiers.
        /// </summary>
        public uint randomSeed;

        /// <summary>
        ///     Non-zero when the bandwidth limits must be recalculated for all peers.
        /// </summary>
        public int recalculateBandwidthLimits;

        /// <summary>
        ///     array of peers allocated for this host
        /// </summary>
        public ENetPeer* peers;

        /// <summary>
        ///     number of peers allocated for this host
        /// </summary>
        public nuint peerCount;

        /// <summary>
        ///     maximum number of channels allowed for connected peers
        /// </summary>
        public nuint channelLimit;

        /// <summary>
        ///     The time of the last service pass.
        /// </summary>
        public uint serviceTime;

        /// <summary>
        ///     The queue of peers with events waiting to be dispatched.
        /// </summary>
        public ENetList dispatchQueue;

        /// <summary>
        ///     The total number of bytes queued waiting for delivery across all peers.
        /// </summary>
        public uint totalQueued;

        /// <summary>
        ///     The size in bytes of the packets being assembled for transmission.
        /// </summary>
        public nuint packetSize;

        /// <summary>
        ///     The flags applied to the header of outgoing packets.
        /// </summary>
        public ushort headerFlags;

        /// <summary>
        ///     When non-zero, the host ignores incoming connection requests instead of accepting them.
        /// </summary>
        public ushort ignoreConnectRequests;

        /// <summary>
        ///     The backing storage for the outgoing command array.
        /// </summary>
        private ENetProtocols commands_t;

        /// <summary>
        ///     The array of protocol commands being assembled for the next packet.
        /// </summary>
        public ENetProtocol* commands => (ENetProtocol*)Unsafe.AsPointer(ref commands_t);

        /// <summary>
        ///     The number of commands currently assembled for the next packet.
        /// </summary>
        public nuint commandCount;

        /// <summary>
        ///     The backing storage for the outgoing buffer array.
        /// </summary>
        private ENetBuffers buffers_t;

        /// <summary>
        ///     The array of buffers being assembled for the next packet.
        /// </summary>
        public ENetBuffer* buffers => (ENetBuffer*)Unsafe.AsPointer(ref buffers_t);

        /// <summary>
        ///     The number of buffers currently assembled for the next packet.
        /// </summary>
        public nuint bufferCount;

        /// <summary>
        ///     callback the user can set to enable packet checksums for this host
        /// </summary>
        public delegate* managed<ENetBuffer*, nuint, uint> checksum;

        /// <summary>
        ///     The compressor applied to packets, if any.
        /// </summary>
        public ENetCompressor compressor;

        /// <summary>
        ///     The fixed buffers used to store the payload data of outgoing packets.
        /// </summary>
        public ENetPacketData packetData;

        /// <summary>
        ///     The address the currently received packet came from.
        /// </summary>
        public ENetAddress receivedAddress;

        /// <summary>
        ///     The buffer holding the currently received packet payload.
        /// </summary>
        public byte* receivedData;

        /// <summary>
        ///     The length in bytes of the currently received packet payload.
        /// </summary>
        public nuint receivedDataLength;

        /// <summary>
        ///     total data sent, user should reset to 0 as needed to prevent overflow
        /// </summary>
        public uint totalSentData;

        /// <summary>
        ///     total UDP packets sent, user should reset to 0 as needed to prevent overflow
        /// </summary>
        public uint totalSentPackets;

        /// <summary>
        ///     total data received, user should reset to 0 as needed to prevent overflow
        /// </summary>
        public uint totalReceivedData;

        /// <summary>
        ///     total UDP packets received, user should reset to 0 as needed to prevent overflow
        /// </summary>
        public uint totalReceivedPackets;

        /// <summary>
        ///     callback the user can set to intercept received raw UDP packets
        /// </summary>
        public delegate* managed<ENetHost*, ENetEvent*, int> intercept;

        /// <summary>
        ///     The number of peers currently in the connected state.
        /// </summary>
        public nuint connectedPeers;

        /// <summary>
        ///     The number of peers currently limited by bandwidth throttling.
        /// </summary>
        public nuint bandwidthLimitedPeers;

        /// <summary>
        ///     optional number of allowed peers from duplicate IPs, defaults to ENET_PROTOCOL_MAXIMUM_PEER_ID
        /// </summary>
        public nuint duplicatePeers;

        /// <summary>
        ///     the maximum allowable packet size that may be sent or received on a peer
        /// </summary>
        public nuint maximumPacketSize;

        /// <summary>
        ///     the maximum aggregate amount of buffer space a peer may use waiting for packets to be delivered
        /// </summary>
        public nuint maximumWaitingData;
    }

    /// <summary>
    ///     Marks a structure as a fixed-size array of the given length.
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct)]
    internal sealed class ENetArrayAttribute : Attribute
    {
        /// <summary>
        ///     The number of elements in the fixed-size array.
        /// </summary>
        public readonly uint Length;

        /// <summary>
        ///     Initializes the attribute with the specified array length.
        /// </summary>
        /// <param name="length">The number of elements in the array.</param>
        public ENetArrayAttribute(uint length) => Length = length;
    }

    /// <summary>
    ///     A fixed-size array of <see cref="ENetProtocol" /> commands laid out as a contiguous block.
    /// </summary>
    [ENetArray(ENET_PROTOCOL_MAXIMUM_PACKET_COMMANDS)]
    [StructLayout(LayoutKind.Sequential)]
    internal struct ENetProtocols
    {
        private ENetProtocols16 _element0;
        private ENetProtocols16 _element1;

        /// <summary>
        ///     A fixed-size array of two <see cref="ENetProtocol" /> elements.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct ENetProtocols2
        {
            private ENetProtocol _element0;
            private ENetProtocol _element1;
        }

        /// <summary>
        ///     A fixed-size array of four <see cref="ENetProtocol" /> elements.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct ENetProtocols4
        {
            private ENetProtocols2 _element0;
            private ENetProtocols2 _element1;
        }

        /// <summary>
        ///     A fixed-size array of eight <see cref="ENetProtocol" /> elements.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct ENetProtocols8
        {
            private ENetProtocols4 _element0;
            private ENetProtocols4 _element1;
        }

        /// <summary>
        ///     A fixed-size array of sixteen <see cref="ENetProtocol" /> elements.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct ENetProtocols16
        {
            private ENetProtocols8 _element0;
            private ENetProtocols8 _element1;
        }

        /// <summary>
        ///     A fixed-size array of thirty-two <see cref="ENetProtocol" /> elements.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct ENetProtocols32
        {
            private ENetProtocols16 _element0;
            private ENetProtocols16 _element1;
        }
    }

    /// <summary>
    ///     A fixed-size array of <see cref="ENetBuffer" /> entries laid out as a contiguous block.
    /// </summary>
    [ENetArray(ENET_BUFFER_MAXIMUM)]
    [StructLayout(LayoutKind.Sequential)]
    internal struct ENetBuffers
    {
        private ENetBuffers64 _element0;
        private ENetBuffer _element1;

        /// <summary>
        ///     A fixed-size array of two <see cref="ENetBuffer" /> elements.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct ENetBuffers2
        {
            private ENetBuffer _element0;
            private ENetBuffer _element1;
        }

        /// <summary>
        ///     A fixed-size array of four <see cref="ENetBuffer" /> elements.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct ENetBuffers4
        {
            private ENetBuffers2 _element0;
            private ENetBuffers2 _element1;
        }

        /// <summary>
        ///     A fixed-size array of eight <see cref="ENetBuffer" /> elements.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct ENetBuffers8
        {
            private ENetBuffers4 _element0;
            private ENetBuffers4 _element1;
        }

        /// <summary>
        ///     A fixed-size array of sixteen <see cref="ENetBuffer" /> elements.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct ENetBuffers16
        {
            private ENetBuffers8 _element0;
            private ENetBuffers8 _element1;
        }

        /// <summary>
        ///     A fixed-size array of thirty-two <see cref="ENetBuffer" /> elements.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct ENetBuffers32
        {
            private ENetBuffers16 _element0;
            private ENetBuffers16 _element1;
        }

        /// <summary>
        ///     A fixed-size array of sixty-four <see cref="ENetBuffer" /> elements.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct ENetBuffers64
        {
            private ENetBuffers32 _element0;
            private ENetBuffers32 _element1;
        }
    }

    /// <summary>
    ///     A fixed-size array of packet payload buffers used to store outgoing packet data.
    /// </summary>
    [ENetArray(2)]
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ENetPacketData
    {
        private ENetPacketDataBuffer _element0;
        private ENetPacketDataBuffer _element1;

        /// <summary>
        ///     Gets a pointer to the payload buffer at the specified index.
        /// </summary>
        /// <param name="i">The zero based index of the buffer.</param>
        /// <returns>A pointer to the start of the payload buffer.</returns>
        public byte* this[int i] => (byte*)Unsafe.AsPointer(ref Unsafe.Add(ref _element0, i));
    }

    /// <summary>
    ///     A fixed-size payload buffer large enough to hold the maximum MTU.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = (int)ENET_PROTOCOL_MAXIMUM_MTU)]
    internal unsafe struct ENetPacketDataBuffer
    {
        /// <summary>
        ///     Alignment padding to ensure the structure is properly aligned in memory.
        /// </summary>
        private nuint _padding;
    }

    /// <summary>
    ///     An ENet event type, as specified in <see cref="ENetEvent" />.
    /// </summary>
    public enum ENetEventType
    {
        /// <summary>
        ///     no event occurred within the specified time limit
        /// </summary>
        ENET_EVENT_TYPE_NONE = 0,

        /// <summary>
        ///     a connection request initiated by enet_host_connect has completed.
        ///     The peer field contains the peer which successfully connected.
        /// </summary>
        ENET_EVENT_TYPE_CONNECT = 1,

        /// <summary>
        ///     a peer has disconnected. This event is generated on a successful
        ///     completion of a disconnect initiated by enet_peer_disconnect, if
        ///     a peer has timed out, or if a connection request intialized by
        ///     enet_host_connect has timed out. The peer field contains the peer
        ///     which disconnected. The data field contains user supplied data
        ///     describing the disconnection, or 0, if none is available.
        /// </summary>
        ENET_EVENT_TYPE_DISCONNECT = 2,

        /// <summary>
        ///     a packet has been received from a peer. The peer field specifies the
        ///     peer which sent the packet. The channelID field specifies the channel
        ///     number upon which the packet was received. The packet field contains
        ///     the packet that was received; this packet must be destroyed with
        ///     enet_packet_destroy after use.
        /// </summary>
        ENET_EVENT_TYPE_RECEIVE = 3
    }

    /// <summary>
    ///     An ENet event as returned by enet_host_service().
    /// </summary>
    /// <seealso cref="enet_host_service(ENetHost*, ENetEvent*, uint)" />
    public unsafe struct ENetEvent
    {
        /// <summary>
        ///     type of the event
        /// </summary>
        public ENetEventType type;

        /// <summary>
        ///     peer that generated a connect, disconnect or receive event
        /// </summary>
        public ENetPeer* peer;

        /// <summary>
        ///     channel on the peer that generated the event, if appropriate
        /// </summary>
        public byte channelID;

        /// <summary>
        ///     data associated with the event, if appropriate
        /// </summary>
        public uint data;

        /// <summary>
        ///     packet associated with the event, if appropriate
        /// </summary>
        public ENetPacket* packet;
    }
}