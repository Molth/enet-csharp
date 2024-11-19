using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if NET7_0_OR_GREATER
using System.Runtime.Intrinsics;
#endif
using size_t = nuint;
using enet_uint8 = byte;
using enet_uint16 = ushort;
using enet_uint32 = uint;
using ENetSocket = long;
using static enet.ENet;

// ReSharper disable ALL

namespace enet
{
    public static partial class ENet
    {
        public const enet_uint32 ENET_VERSION_MAJOR = 1;
        public const enet_uint32 ENET_VERSION_MINOR = 3;
        public const enet_uint32 ENET_VERSION_PATCH = 18;
        public static enet_uint32 ENET_VERSION_CREATE(enet_uint32 major, enet_uint32 minor, enet_uint32 patch) => (((major) << 16) | ((minor) << 8) | (patch));
        public static enet_uint32 ENET_VERSION_GET_MAJOR(enet_uint32 version) => (((version) >> 16) & 0xFF);
        public static enet_uint32 ENET_VERSION_GET_MINOR(enet_uint32 version) => (((version) >> 8) & 0xFF);
        public static enet_uint32 ENET_VERSION_GET_PATCH(enet_uint32 version) => ((version) & 0xFF);
        public static readonly enet_uint32 ENET_VERSION = ENET_VERSION_CREATE(ENET_VERSION_MAJOR, ENET_VERSION_MINOR, ENET_VERSION_PATCH);
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
        public static readonly ENetIP ENET_HOST_BROADCAST = new ENetIP(stackalloc enet_uint8[16] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 255, 255, 255, 255, 255, 255 });
        public const enet_uint32 ENET_PORT_ANY = 0;
    }

    [StructLayout(LayoutKind.Explicit, Size = 16)]
    public unsafe struct ENetIP : IEquatable<ENetIP>
    {
        [FieldOffset(0)] public fixed enet_uint8 ipv6[16];
        [FieldOffset(12)] public fixed enet_uint8 ipv4[4];

        public ENetIP(ReadOnlySpan<enet_uint8> buffer) => Unsafe.CopyBlockUnaligned(ref Unsafe.As<ENetIP, enet_uint8>(ref this), ref MemoryMarshal.GetReference(buffer), (enet_uint32)buffer.Length);

        public bool Equals(ENetIP other)
        {
#if NET7_0_OR_GREATER
            if (Vector128.IsHardwareAccelerated)
                return Vector128.LoadUnsafe<enet_uint8>(ref Unsafe.As<ENetIP, enet_uint8>(ref this)) == Vector128.LoadUnsafe<enet_uint8>(ref Unsafe.As<ENetIP, enet_uint8>(ref other));
#endif
            ref int left = ref Unsafe.As<ENetIP, int>(ref this);
            ref int right = ref Unsafe.As<ENetIP, int>(ref other);
            return left == right && Unsafe.Add<int>(ref left, 1) == Unsafe.Add<int>(ref right, 1) && Unsafe.Add<int>(ref left, 2) == Unsafe.Add<int>(ref right, 2) && Unsafe.Add<int>(ref left, 3) == Unsafe.Add<int>(ref right, 3);
        }

        public override bool Equals(object? obj) => obj is ENetIP other && Equals(other);

        public override int GetHashCode() => enet_xxhash_32(Unsafe.AsPointer(ref this), 16);

        public override string ToString()
        {
            enet_uint8* buffer = stackalloc enet_uint8[64];
            _ = enet_get_ip((ENetIP*)Unsafe.AsPointer(ref this), buffer, 64);
            return new string((sbyte*)buffer);
        }

        public static bool operator ==(ENetIP left, ENetIP right) => left.Equals(right);
        public static bool operator !=(ENetIP left, ENetIP right) => !left.Equals(right);

        public static implicit operator Span<enet_uint8>(ENetIP ip) => MemoryMarshal.CreateSpan(ref Unsafe.As<ENetIP, enet_uint8>(ref ip), 16);
        public static implicit operator ReadOnlySpan<enet_uint8>(ENetIP ip) => MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<ENetIP, enet_uint8>(ref ip), 16);
    }

    [StructLayout(LayoutKind.Explicit, Size = 20)]
    public unsafe struct ENetAddress : IEquatable<ENetAddress>
    {
        [FieldOffset(0)] public ENetIP host;
        [FieldOffset(16)] public enet_uint16 port;

        public bool Equals(ENetAddress other) => this.host == other.host && this.port == other.port;
        public override bool Equals(object? obj) => obj is ENetAddress other && Equals(other);

        public override int GetHashCode() => host.GetHashCode() ^ (int)port;

        public override string ToString()
        {
            enet_uint8* buffer = stackalloc enet_uint8[64];
            _ = enet_get_ip((ENetAddress*)Unsafe.AsPointer(ref this.host), buffer, 64);
            return new string((sbyte*)buffer) + ":" + port;
        }

        public static bool operator ==(ENetAddress left, ENetAddress right) => left.Equals(right);
        public static bool operator !=(ENetAddress left, ENetAddress right) => !left.Equals(right);
    }

    [Flags]
    public enum ENetPacketFlag
    {
        ENET_PACKET_FLAG_RELIABLE = (1 << 0),
        ENET_PACKET_FLAG_UNSEQUENCED = (1 << 1),
        ENET_PACKET_FLAG_NO_ALLOCATE = (1 << 2),
        ENET_PACKET_FLAG_UNRELIABLE_FRAGMENT = (1 << 3),
        ENET_PACKET_FLAG_SENT = (1 << 8)
    }

    public unsafe struct ENetPacket
    {
        public size_t referenceCount;
        public enet_uint32 flags;
        public enet_uint8* data;
        public size_t dataLength;
        public delegate* managed<ENetPacket*, void> freeCallback;
        public void* userData;
    }

    public struct ENetAcknowledgement
    {
        public ENetListNode acknowledgementList;
        public enet_uint32 sentTime;
        public ENetProtocol command;
    }

    public unsafe struct ENetOutgoingCommand
    {
        public ENetListNode outgoingCommandList;
        public enet_uint16 reliableSequenceNumber;
        public enet_uint16 unreliableSequenceNumber;
        public enet_uint32 sentTime;
        public enet_uint32 roundTripTimeout;
        public enet_uint32 queueTime;
        public enet_uint32 fragmentOffset;
        public enet_uint16 fragmentLength;
        public enet_uint16 sendAttempts;
        public ENetProtocol command;
        public ENetPacket* packet;
    }

    public unsafe struct ENetIncomingCommand
    {
        public ENetListNode incomingCommandList;
        public enet_uint16 reliableSequenceNumber;
        public enet_uint16 unreliableSequenceNumber;
        public ENetProtocol command;
        public enet_uint32 fragmentCount;
        public enet_uint32 fragmentsRemaining;
        public enet_uint32* fragments;
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
        public const enet_uint32 ENET_BUFFER_MAXIMUM = (1 + 2 * ENET_PROTOCOL_MAXIMUM_PACKET_COMMANDS);

        public const enet_uint32 ENET_HOST_RECEIVE_BUFFER_SIZE = 256 * 1024;
        public const enet_uint32 ENET_HOST_SEND_BUFFER_SIZE = 256 * 1024;
        public const enet_uint32 ENET_HOST_BANDWIDTH_THROTTLE_INTERVAL = 1000;
        public const enet_uint32 ENET_HOST_DEFAULT_MTU = 1392;
        public const enet_uint32 ENET_HOST_DEFAULT_MAXIMUM_PACKET_SIZE = 32 * 1024 * 1024;
        public const enet_uint32 ENET_HOST_DEFAULT_MAXIMUM_WAITING_DATA = 32 * 1024 * 1024;
        public const enet_uint32 ENET_PEER_DEFAULT_ROUND_TRIP_TIME = 500;
        public const enet_uint32 ENET_PEER_DEFAULT_PACKET_THROTTLE = 32;
        public const enet_uint32 ENET_PEER_PACKET_THROTTLE_SCALE = 32;
        public const enet_uint32 ENET_PEER_PACKET_THROTTLE_COUNTER = 7;
        public const enet_uint32 ENET_PEER_PACKET_THROTTLE_ACCELERATION = 2;
        public const enet_uint32 ENET_PEER_PACKET_THROTTLE_DECELERATION = 2;
        public const enet_uint32 ENET_PEER_PACKET_THROTTLE_INTERVAL = 5000;
        public const enet_uint32 ENET_PEER_PACKET_LOSS_SCALE = (1 << 16);
        public const enet_uint32 ENET_PEER_PACKET_LOSS_INTERVAL = 10000;
        public const enet_uint32 ENET_PEER_WINDOW_SIZE_SCALE = 64 * 1024;
        public const enet_uint32 ENET_PEER_TIMEOUT_LIMIT = 32;
        public const enet_uint32 ENET_PEER_TIMEOUT_MINIMUM = 5000;
        public const enet_uint32 ENET_PEER_TIMEOUT_MAXIMUM = 30000;
        public const enet_uint32 ENET_PEER_PING_INTERVAL = 500;
        public const enet_uint32 ENET_PEER_UNSEQUENCED_WINDOWS = 64;
        public const enet_uint32 ENET_PEER_UNSEQUENCED_WINDOW_SIZE = 1024;
        public const enet_uint32 ENET_PEER_FREE_UNSEQUENCED_WINDOWS = 32;
        public const enet_uint32 ENET_PEER_RELIABLE_WINDOWS = 16;
        public const enet_uint32 ENET_PEER_RELIABLE_WINDOW_SIZE = 0x1000;
        public const enet_uint32 ENET_PEER_FREE_RELIABLE_WINDOWS = 8;
    }

    public unsafe struct ENetChannel
    {
        public enet_uint16 outgoingReliableSequenceNumber;
        public enet_uint16 outgoingUnreliableSequenceNumber;
        public enet_uint16 usedReliableWindows;
        public fixed enet_uint16 reliableWindows[(int)ENET_PEER_RELIABLE_WINDOWS];
        public enet_uint16 incomingReliableSequenceNumber;
        public enet_uint16 incomingUnreliableSequenceNumber;
        public ENetList incomingReliableCommands;
        public ENetList incomingUnreliableCommands;
    }

    public enum ENetPeerFlag
    {
        ENET_PEER_FLAG_NEEDS_DISPATCH = (1 << 0),
        ENET_PEER_FLAG_CONTINUE_SENDING = (1 << 1)
    }

    public unsafe struct ENetPeer
    {
        public ENetListNode dispatchList;
        public ENetHost* host;
        public enet_uint16 outgoingPeerID;
        public enet_uint16 incomingPeerID;
        public enet_uint32 connectID;
        public enet_uint8 outgoingSessionID;
        public enet_uint8 incomingSessionID;
        public ENetAddress address;
        public void* data;
        public ENetPeerState state;
        public ENetChannel* channels;
        public size_t channelCount;
        public enet_uint32 incomingBandwidth;
        public enet_uint32 outgoingBandwidth;
        public enet_uint32 incomingBandwidthThrottleEpoch;
        public enet_uint32 outgoingBandwidthThrottleEpoch;
        public enet_uint32 incomingDataTotal;
        public enet_uint32 outgoingDataTotal;
        public enet_uint32 lastSendTime;
        public enet_uint32 lastReceiveTime;
        public enet_uint32 nextTimeout;
        public enet_uint32 earliestTimeout;
        public enet_uint32 packetLossEpoch;
        public enet_uint32 packetsSent;
        public enet_uint32 packetsLost;
        public enet_uint32 packetLoss;
        public enet_uint32 packetLossVariance;
        public enet_uint32 packetThrottle;
        public enet_uint32 packetThrottleLimit;
        public enet_uint32 packetThrottleCounter;
        public enet_uint32 packetThrottleEpoch;
        public enet_uint32 packetThrottleAcceleration;
        public enet_uint32 packetThrottleDeceleration;
        public enet_uint32 packetThrottleInterval;
        public enet_uint32 pingInterval;
        public enet_uint32 timeoutLimit;
        public enet_uint32 timeoutMinimum;
        public enet_uint32 timeoutMaximum;
        public enet_uint32 lastRoundTripTime;
        public enet_uint32 lowestRoundTripTime;
        public enet_uint32 lastRoundTripTimeVariance;
        public enet_uint32 highestRoundTripTimeVariance;
        public enet_uint32 roundTripTime;
        public enet_uint32 roundTripTimeVariance;
        public enet_uint32 mtu;
        public enet_uint32 windowSize;
        public enet_uint32 reliableDataInTransit;
        public enet_uint16 outgoingReliableSequenceNumber;
        public ENetList acknowledgements;
        public ENetList sentReliableCommands;
        public ENetList outgoingSendReliableCommands;
        public ENetList outgoingCommands;
        public ENetList dispatchedCommands;
        public enet_uint16 flags;
        public enet_uint16 reserved;
        public enet_uint16 incomingUnsequencedGroup;
        public enet_uint16 outgoingUnsequencedGroup;
        public fixed enet_uint32 unsequencedWindow[(int)ENET_PEER_UNSEQUENCED_WINDOW_SIZE / 32];
        public enet_uint32 eventData;
        public size_t totalWaitingData;
    }

    public unsafe struct ENetCompressor
    {
        public void* context;
        public delegate* managed<void*, ENetBuffer*, size_t, size_t, enet_uint8*, size_t, size_t> compress;
        public delegate* managed<void*, enet_uint8*, size_t, enet_uint8*, size_t, size_t> decompress;
        public delegate* managed<void*, void> destroy;
    }

    public unsafe struct ENetHost
    {
        public ENetSocket socket;
        public ENetAddress address;
        public enet_uint32 incomingBandwidth;
        public enet_uint32 outgoingBandwidth;
        public enet_uint32 bandwidthThrottleEpoch;
        public enet_uint32 mtu;
        public enet_uint32 randomSeed;
        public int recalculateBandwidthLimits;
        public ENetPeer* peers;
        public size_t peerCount;
        public size_t channelLimit;
        public enet_uint32 serviceTime;
        public ENetList dispatchQueue;
        public enet_uint32 totalQueued;
        public size_t packetSize;
        public enet_uint16 headerFlags;
        public ENetProtocols commands_t;
        public ENetProtocol* commands => (ENetProtocol*)Unsafe.AsPointer(ref commands_t);
        public size_t commandCount;
        public ENetBuffers buffers_t;
        public ENetBuffer* buffers => (ENetBuffer*)Unsafe.AsPointer(ref buffers_t);
        public size_t bufferCount;
        public delegate* managed<ENetBuffer*, size_t, enet_uint32> checksum;
        public ENetCompressor compressor;
        public ENetPacketData packetData;
        public ENetAddress receivedAddress;
        public enet_uint8* receivedData;
        public size_t receivedDataLength;
        public enet_uint32 totalSentData;
        public enet_uint32 totalSentPackets;
        public enet_uint32 totalReceivedData;
        public enet_uint32 totalReceivedPackets;
        public delegate* managed<ENetHost*, ENetEvent*, int> intercept;
        public size_t connectedPeers;
        public size_t bandwidthLimitedPeers;
        public size_t duplicatePeers;
        public size_t maximumPacketSize;
        public size_t maximumWaitingData;
    }

    [AttributeUsage(AttributeTargets.Struct)]
    public sealed class ENetArrayAttribute : Attribute
    {
        public ENetArrayAttribute(enet_uint32 length)
        {
        }
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

        public enet_uint8* this[int i] => ((ENetPacketDataBuffer*)Unsafe.AsPointer(ref this))[i].data;
    }

    public unsafe struct ENetPacketDataBuffer
    {
        public fixed enet_uint8 data[(int)ENET_PROTOCOL_MAXIMUM_MTU];
    }

    public enum ENetEventType
    {
        ENET_EVENT_TYPE_NONE = 0,
        ENET_EVENT_TYPE_CONNECT = 1,
        ENET_EVENT_TYPE_DISCONNECT = 2,
        ENET_EVENT_TYPE_RECEIVE = 3
    }

    public unsafe struct ENetEvent
    {
        public ENetEventType type;
        public ENetPeer* peer;
        public enet_uint8 channelID;
        public enet_uint32 data;
        public ENetPacket* packet;
    }
}