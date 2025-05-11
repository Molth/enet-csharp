using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if NET7_0_OR_GREATER
using System.Runtime.Intrinsics;
#endif
using static enet.ENet;

#pragma warning disable CS1591
#pragma warning disable CS8632

// ReSharper disable ALL

namespace enet
{
    public static partial class ENet
    {
        public const uint ENET_VERSION_MAJOR = 1;
        public const uint ENET_VERSION_MINOR = 3;
        public const uint ENET_VERSION_PATCH = 18;
        public static uint ENET_VERSION_CREATE(uint major, uint minor, uint patch) => (((major) << 16) | ((minor) << 8) | (patch));
        public static uint ENET_VERSION_GET_MAJOR(uint version) => (((version) >> 16) & 0xFF);
        public static uint ENET_VERSION_GET_MINOR(uint version) => (((version) >> 8) & 0xFF);
        public static uint ENET_VERSION_GET_PATCH(uint version) => ((version) & 0xFF);
        public static readonly uint ENET_VERSION = ENET_VERSION_CREATE(ENET_VERSION_MAJOR, ENET_VERSION_MINOR, ENET_VERSION_PATCH);
    }

    public enum ENetSocketType
    {
        ENET_SOCKET_TYPE_STREAM = 1,
        ENET_SOCKET_TYPE_DATAGRAM = 2
    }

    public enum ENetSocketWait
    {
        ENET_SOCKET_WAIT_NONE = 0,
        ENET_SOCKET_WAIT_SEND = (1 << 0),
        ENET_SOCKET_WAIT_RECEIVE = (1 << 1),
        ENET_SOCKET_WAIT_INTERRUPT = (1 << 2)
    }

    public enum ENetSocketOption
    {
        ENET_SOCKOPT_NONBLOCK = 1,
        ENET_SOCKOPT_BROADCAST = 2,
        ENET_SOCKOPT_RCVBUF = 3,
        ENET_SOCKOPT_SNDBUF = 4,
        ENET_SOCKOPT_REUSEADDR = 5,
        ENET_SOCKOPT_RCVTIMEO = 6,
        ENET_SOCKOPT_SNDTIMEO = 7,
        ENET_SOCKOPT_ERROR = 8,
        ENET_SOCKOPT_NODELAY = 9,
        ENET_SOCKOPT_TTL = 10
    }

    public enum ENetSocketShutdown
    {
        ENET_SOCKET_SHUTDOWN_READ = 0,
        ENET_SOCKET_SHUTDOWN_WRITE = 1,
        ENET_SOCKET_SHUTDOWN_READ_WRITE = 2
    }

    public static partial class ENet
    {
        public static readonly ENetIP ENET_HOST_ANY = new ENetIP();
        public static readonly ENetIP ENET_HOST_BROADCAST = new ENetIP(stackalloc byte[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 255, 255, 255, 255 });
        public const uint ENET_PORT_ANY = 0;
    }

    [StructLayout(LayoutKind.Explicit, Size = 16)]
    public unsafe struct ENetIP : IEquatable<ENetIP>
    {
        [FieldOffset(0)] public fixed Byte ipv6[16];
        [FieldOffset(12)] public fixed Byte ipv4[4];

        public ENetIP(ReadOnlySpan<byte> buffer) => Unsafe.CopyBlockUnaligned(ref Unsafe.As<ENetIP, byte>(ref Unsafe.AsRef(in this)), ref MemoryMarshal.GetReference(buffer), (uint)buffer.Length);

        public bool Equals(ENetIP other)
        {
#if NET7_0_OR_GREATER
            if (Vector128.IsHardwareAccelerated)
                return Vector128.LoadUnsafe<byte>(ref Unsafe.As<ENetIP, byte>(ref Unsafe.AsRef(in this))) == Vector128.LoadUnsafe<byte>(ref Unsafe.As<ENetIP, byte>(ref other));
#endif
            ref int left = ref Unsafe.As<ENetIP, int>(ref Unsafe.AsRef(in this));
            ref int right = ref Unsafe.As<ENetIP, int>(ref other);
            return left == right && Unsafe.Add<int>(ref left, 1) == Unsafe.Add<int>(ref right, 1) && Unsafe.Add<int>(ref left, 2) == Unsafe.Add<int>(ref right, 2) && Unsafe.Add<int>(ref left, 3) == Unsafe.Add<int>(ref right, 3);
        }

        public override bool Equals(object? obj) => obj is ENetIP other && Equals(other);

        public override int GetHashCode()
        {
            HashCode hashCode = new HashCode();
#if NET6_0_OR_GREATER
            hashCode.AddBytes(MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ENetIP, byte>(ref Unsafe.AsRef(in this)), 16));
#else
            ref int reference = ref Unsafe.As<ENetIP, int>(ref Unsafe.AsRef(in this));
            for (int i = 0; i < 4; i++)
                hashCode.Add(Unsafe.Add(ref reference, i));
#endif
            return hashCode.ToHashCode();
        }

        public override string ToString()
        {
            byte* buffer = stackalloc byte[64];
            _ = enet_address_get_host_ip((ENetAddress*)Unsafe.AsPointer(ref Unsafe.AsRef(in this)), buffer, 64);
            return new string((sbyte*)buffer);
        }

        public static bool operator ==(ENetIP left, ENetIP right) => left.Equals(right);
        public static bool operator !=(ENetIP left, ENetIP right) => !left.Equals(right);

        public static implicit operator Span<byte>(ENetIP ip) => MemoryMarshal.CreateSpan(ref Unsafe.As<ENetIP, byte>(ref ip), 16);
        public static implicit operator ReadOnlySpan<byte>(ENetIP ip) => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ENetIP, byte>(ref ip), 16);
    }

    /// <summary>
    ///     Portable internet address structure.
    /// </summary>
    /// <remarks>
    ///     The host must be specified in <b>network byte-order</b>, and the port must be in host
    ///     byte-order. The constant ENET_HOST_ANY may be used to specify the default
    ///     server host. The constant ENET_HOST_BROADCAST may be used to specify the
    ///     broadcast address (255.255.255.255).  This makes sense for enet_host_connect,
    ///     but not for enet_host_create.  Once a server responds to a broadcast, the
    ///     address is updated from ENET_HOST_BROADCAST to the server's actual IP address.
    /// </remarks>
    [StructLayout(LayoutKind.Explicit, Size = 20)]
    public unsafe struct ENetAddress : IEquatable<ENetAddress>
    {
        [FieldOffset(0)] public ENetIP host;
        [FieldOffset(16)] public ushort port;

        public bool Equals(ENetAddress other) => this.host == other.host && this.port == other.port;
        public override bool Equals(object? obj) => obj is ENetAddress other && Equals(other);

        public override int GetHashCode()
        {
            HashCode hashCode = new HashCode();
#if NET6_0_OR_GREATER
            hashCode.AddBytes(MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ENetAddress, byte>(ref Unsafe.AsRef(in this)), 20));
#else
            ref int reference = ref Unsafe.As<ENetAddress, int>(ref Unsafe.AsRef(in this));
            for (int i = 0; i < 5; i++)
                hashCode.Add(Unsafe.Add(ref reference, i));
#endif
            return hashCode.ToHashCode();
        }

        public override string ToString()
        {
            byte* buffer = stackalloc byte[64];
            _ = enet_address_get_host_ip((ENetAddress*)Unsafe.AsPointer(ref Unsafe.AsRef(in this).host), buffer, 64);
            return new string((sbyte*)buffer) + ":" + port;
        }

        public static bool operator ==(ENetAddress left, ENetAddress right) => left.Equals(right);
        public static bool operator !=(ENetAddress left, ENetAddress right) => !left.Equals(right);
    }

    /// <summary>
    ///     Packet flag bit constants.
    /// </summary>
    /// <remarks>
    ///     The host must be specified in <b>network byte-order</b>, and the port must be in
    ///     host byte-order. The constant ENET_HOST_ANY may be used to specify the
    ///     default server host.
    /// </remarks>
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

    public struct ENetAcknowledgement
    {
        public ENetListNode acknowledgementList;
        public uint sentTime;
        public ENetProtocol command;
    }

    public unsafe struct ENetOutgoingCommand
    {
        public ENetListNode outgoingCommandList;
        public ushort reliableSequenceNumber;
        public ushort unreliableSequenceNumber;
        public uint sentTime;
        public uint roundTripTimeout;
        public uint queueTime;
        public uint fragmentOffset;
        public ushort fragmentLength;
        public ushort sendAttempts;
        public ENetProtocol command;
        public ENetPacket* packet;
    }

    public unsafe struct ENetIncomingCommand
    {
        public ENetListNode incomingCommandList;
        public ushort reliableSequenceNumber;
        public ushort unreliableSequenceNumber;
        public ENetProtocol command;
        public uint fragmentCount;
        public uint fragmentsRemaining;
        public uint* fragments;
        public ENetPacket* packet;
    }

    public enum ENetPeerState
    {
        ENET_PEER_STATE_DISCONNECTED = 0,
        ENET_PEER_STATE_CONNECTING = 1,
        ENET_PEER_STATE_ACKNOWLEDGING_CONNECT = 2,
        ENET_PEER_STATE_CONNECTION_PENDING = 3,
        ENET_PEER_STATE_CONNECTION_SUCCEEDED = 4,
        ENET_PEER_STATE_CONNECTED = 5,
        ENET_PEER_STATE_DISCONNECT_LATER = 6,
        ENET_PEER_STATE_DISCONNECTING = 7,
        ENET_PEER_STATE_ACKNOWLEDGING_DISCONNECT = 8,
        ENET_PEER_STATE_ZOMBIE = 9
    }

    public static partial class ENet
    {
        public const uint ENET_BUFFER_MAXIMUM = (1 + 2 * ENET_PROTOCOL_MAXIMUM_PACKET_COMMANDS);

        public const uint ENET_HOST_RECEIVE_BUFFER_SIZE = 256 * 1024;
        public const uint ENET_HOST_SEND_BUFFER_SIZE = 256 * 1024;
        public const uint ENET_HOST_BANDWIDTH_THROTTLE_INTERVAL = 1000;
        public const uint ENET_HOST_DEFAULT_MTU = 1392;
        public const uint ENET_HOST_DEFAULT_MAXIMUM_PACKET_SIZE = 32 * 1024 * 1024;
        public const uint ENET_HOST_DEFAULT_MAXIMUM_WAITING_DATA = 32 * 1024 * 1024;
        public const uint ENET_PEER_DEFAULT_ROUND_TRIP_TIME = 500;
        public const uint ENET_PEER_DEFAULT_PACKET_THROTTLE = 32;
        public const uint ENET_PEER_PACKET_THROTTLE_SCALE = 32;
        public const uint ENET_PEER_PACKET_THROTTLE_COUNTER = 7;
        public const uint ENET_PEER_PACKET_THROTTLE_ACCELERATION = 2;
        public const uint ENET_PEER_PACKET_THROTTLE_DECELERATION = 2;
        public const uint ENET_PEER_PACKET_THROTTLE_INTERVAL = 5000;
        public const uint ENET_PEER_PACKET_LOSS_SCALE = (1 << 16);
        public const uint ENET_PEER_PACKET_LOSS_INTERVAL = 10000;
        public const uint ENET_PEER_WINDOW_SIZE_SCALE = 64 * 1024;
        public const uint ENET_PEER_TIMEOUT_LIMIT = 32;
        public const uint ENET_PEER_TIMEOUT_MINIMUM = 5000;
        public const uint ENET_PEER_TIMEOUT_MAXIMUM = 30000;
        public const uint ENET_PEER_PING_INTERVAL = 500;
        public const uint ENET_PEER_UNSEQUENCED_WINDOWS = 64;
        public const uint ENET_PEER_UNSEQUENCED_WINDOW_SIZE = 1024;
        public const uint ENET_PEER_FREE_UNSEQUENCED_WINDOWS = 32;
        public const uint ENET_PEER_RELIABLE_WINDOWS = 16;
        public const uint ENET_PEER_RELIABLE_WINDOW_SIZE = 0x1000;
        public const uint ENET_PEER_FREE_RELIABLE_WINDOWS = 8;
    }

    public unsafe struct ENetChannel
    {
        public ushort outgoingReliableSequenceNumber;
        public ushort outgoingUnreliableSequenceNumber;
        public ushort usedReliableWindows;
        public fixed ushort reliableWindows[(int)ENET_PEER_RELIABLE_WINDOWS];
        public ushort incomingReliableSequenceNumber;
        public ushort incomingUnreliableSequenceNumber;
        public ENetList incomingReliableCommands;
        public ENetList incomingUnreliableCommands;
    }

    public enum ENetPeerFlag
    {
        ENET_PEER_FLAG_NEEDS_DISPATCH = (1 << 0),
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
        public ENetListNode dispatchList;
        public ENetHost* host;
        public ushort outgoingPeerID;
        public ushort incomingPeerID;
        public uint connectID;
        public byte outgoingSessionID;
        public byte incomingSessionID;

        /// <summary>
        ///     Internet address of the peer
        /// </summary>
        public ENetAddress address;

        /// <summary>
        ///     Application private data, may be freely modified
        /// </summary>
        public void* data;

        public ENetPeerState state;
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

        public uint incomingBandwidthThrottleEpoch;
        public uint outgoingBandwidthThrottleEpoch;
        public uint incomingDataTotal;
        public uint outgoingDataTotal;
        public uint lastSendTime;
        public uint lastReceiveTime;
        public uint nextTimeout;
        public uint earliestTimeout;
        public uint packetLossEpoch;
        public uint packetsSent;
        public uint packetsLost;

        /// <summary>
        ///     mean packet loss of reliable packets as a ratio with respect to the constant ENET_PEER_PACKET_LOSS_SCALE
        /// </summary>
        public uint packetLoss;

        public uint packetLossVariance;
        public uint packetThrottle;
        public uint packetThrottleLimit;
        public uint packetThrottleCounter;
        public uint packetThrottleEpoch;
        public uint packetThrottleAcceleration;
        public uint packetThrottleDeceleration;
        public uint packetThrottleInterval;
        public uint pingInterval;
        public uint timeoutLimit;
        public uint timeoutMinimum;
        public uint timeoutMaximum;
        public uint lastRoundTripTime;
        public uint lowestRoundTripTime;
        public uint lastRoundTripTimeVariance;
        public uint highestRoundTripTimeVariance;

        /// <summary>
        ///     mean round trip time (RTT), in milliseconds, between sending a reliable packet and receiving its acknowledgement
        /// </summary>
        public uint roundTripTime;

        public uint roundTripTimeVariance;
        public uint mtu;
        public uint windowSize;
        public uint reliableDataInTransit;
        public ushort outgoingReliableSequenceNumber;
        public ENetList acknowledgements;
        public ENetList sentReliableCommands;
        public ENetList outgoingSendReliableCommands;
        public ENetList outgoingCommands;
        public ENetList dispatchedCommands;
        public ushort flags;
        public ushort reserved;
        public ushort incomingUnsequencedGroup;
        public ushort outgoingUnsequencedGroup;
        public fixed uint unsequencedWindow[(int)ENET_PEER_UNSEQUENCED_WINDOW_SIZE / 32];
        public uint eventData;
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
    }

    /// <summary>
    ///     An ENet host for communicating with peers.
    /// </summary>
    /// <remarks>
    ///     No fields should be modified unless otherwise stated.
    /// </remarks>
    /// <seealso cref="enet_host_create(ENetAddress*, nuint, nuint, uint, uint)" />
    /// <seealso cref="enet_host_destroy(ENetHost*)" />
    /// <seealso cref="enet_host_connect(ENetHost*, ENetAddress*, nuint, uint)" />
    /// <seealso cref="enet_host_service(ENetHost*, ENetEvent*, uint)" />
    /// <seealso cref="enet_host_flush(ENetHost*)" />
    /// <seealso cref="enet_host_broadcast(ENetHost*, byte, ENetPacket*)" />
    /// <seealso cref="enet_host_compress(ENetHost*, ENetCompressor*)" />
    /// <seealso cref="enet_host_compress_with_range_coder(ENetHost*)" />
    /// <seealso cref="enet_host_channel_limit(ENetHost*, nuint)" />
    /// <seealso cref="enet_host_bandwidth_limit(ENetHost*, uint, uint)" />
    /// <seealso cref="enet_host_bandwidth_throttle(ENetHost*)" />
    public unsafe struct ENetHost
    {
        public nint socket;

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

        public uint bandwidthThrottleEpoch;
        public uint mtu;
        public uint randomSeed;
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

        public uint serviceTime;
        public ENetList dispatchQueue;
        public uint totalQueued;
        public nuint packetSize;
        public ushort headerFlags;
        public ENetProtocols commands_t;
        public ENetProtocol* commands => (ENetProtocol*)Unsafe.AsPointer(ref commands_t);
        public nuint commandCount;
        public ENetBuffers buffers_t;
        public ENetBuffer* buffers => (ENetBuffer*)Unsafe.AsPointer(ref buffers_t);
        public nuint bufferCount;

        /// <summary>
        ///     callback the user can set to enable packet checksums for this host
        /// </summary>
        public delegate* managed<ENetBuffer*, nuint, uint> checksum;

        public ENetCompressor compressor;
        public ENetPacketData packetData;
        public ENetAddress receivedAddress;
        public byte* receivedData;
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

        public nuint connectedPeers;
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

    [AttributeUsage(AttributeTargets.Struct)]
    public sealed class ENetArrayAttribute : Attribute
    {
        public readonly uint Length;

        public ENetArrayAttribute(uint length) => Length = length;
    }

    [ENetArray(ENET_PROTOCOL_MAXIMUM_PACKET_COMMANDS)]
    public struct ENetProtocols
    {
        public ENetProtocol command0;
        public ENetProtocol command1;
        public ENetProtocol command2;
        public ENetProtocol command3;
        public ENetProtocol command4;
        public ENetProtocol command5;
        public ENetProtocol command6;
        public ENetProtocol command7;
        public ENetProtocol command8;
        public ENetProtocol command9;
        public ENetProtocol command10;
        public ENetProtocol command11;
        public ENetProtocol command12;
        public ENetProtocol command13;
        public ENetProtocol command14;
        public ENetProtocol command15;
        public ENetProtocol command16;
        public ENetProtocol command17;
        public ENetProtocol command18;
        public ENetProtocol command19;
        public ENetProtocol command20;
        public ENetProtocol command21;
        public ENetProtocol command22;
        public ENetProtocol command23;
        public ENetProtocol command24;
        public ENetProtocol command25;
        public ENetProtocol command26;
        public ENetProtocol command27;
        public ENetProtocol command28;
        public ENetProtocol command29;
        public ENetProtocol command30;
        public ENetProtocol command31;
    }

    [ENetArray(ENET_BUFFER_MAXIMUM)]
    public struct ENetBuffers
    {
        public ENetBuffer buffer0;
        public ENetBuffer buffer1;
        public ENetBuffer buffer2;
        public ENetBuffer buffer3;
        public ENetBuffer buffer4;
        public ENetBuffer buffer5;
        public ENetBuffer buffer6;
        public ENetBuffer buffer7;
        public ENetBuffer buffer8;
        public ENetBuffer buffer9;
        public ENetBuffer buffer10;
        public ENetBuffer buffer11;
        public ENetBuffer buffer12;
        public ENetBuffer buffer13;
        public ENetBuffer buffer14;
        public ENetBuffer buffer15;
        public ENetBuffer buffer16;
        public ENetBuffer buffer17;
        public ENetBuffer buffer18;
        public ENetBuffer buffer19;
        public ENetBuffer buffer20;
        public ENetBuffer buffer21;
        public ENetBuffer buffer22;
        public ENetBuffer buffer23;
        public ENetBuffer buffer24;
        public ENetBuffer buffer25;
        public ENetBuffer buffer26;
        public ENetBuffer buffer27;
        public ENetBuffer buffer28;
        public ENetBuffer buffer29;
        public ENetBuffer buffer30;
        public ENetBuffer buffer31;
        public ENetBuffer buffer32;
        public ENetBuffer buffer33;
        public ENetBuffer buffer34;
        public ENetBuffer buffer35;
        public ENetBuffer buffer36;
        public ENetBuffer buffer37;
        public ENetBuffer buffer38;
        public ENetBuffer buffer39;
        public ENetBuffer buffer40;
        public ENetBuffer buffer41;
        public ENetBuffer buffer42;
        public ENetBuffer buffer43;
        public ENetBuffer buffer44;
        public ENetBuffer buffer45;
        public ENetBuffer buffer46;
        public ENetBuffer buffer47;
        public ENetBuffer buffer48;
        public ENetBuffer buffer49;
        public ENetBuffer buffer50;
        public ENetBuffer buffer51;
        public ENetBuffer buffer52;
        public ENetBuffer buffer53;
        public ENetBuffer buffer54;
        public ENetBuffer buffer55;
        public ENetBuffer buffer56;
        public ENetBuffer buffer57;
        public ENetBuffer buffer58;
        public ENetBuffer buffer59;
        public ENetBuffer buffer60;
        public ENetBuffer buffer61;
        public ENetBuffer buffer62;
        public ENetBuffer buffer63;
        public ENetBuffer buffer64;
    }

    [ENetArray(2)]
    public unsafe struct ENetPacketData
    {
        public ENetPacketDataBuffer buffer0;
        public ENetPacketDataBuffer buffer1;

        public byte* this[int i] => (((ENetPacketDataBuffer*)Unsafe.AsPointer(ref Unsafe.AsRef(in this))) + i)->data;
    }

    public unsafe struct ENetPacketDataBuffer
    {
        public fixed byte data[(int)ENET_PROTOCOL_MAXIMUM_MTU];
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