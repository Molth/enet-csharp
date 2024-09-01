using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static enet.ENet;

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
        public static readonly ENetIP ENET_HOST_ANY = new ENetIP(0, 0);
        public static readonly ENetIP ENET_HOST_BROADCAST = new ENetIP(0, 0xFFFFFFFFFFFF0000);
        public const uint ENET_PORT_ANY = 0;
    }

    [StructLayout(LayoutKind.Explicit, Size = 16)]
    public unsafe struct ENetIP : IEquatable<ENetIP>
    {
        [FieldOffset(0)] public ulong high;
        [FieldOffset(8)] public ulong low;

        public ENetIP(ulong high, ulong low)
        {
            this.high = high;
            this.low = low;
        }

        public void CopyTo(byte* buffer)
        {
            *(ulong*)buffer = high;
            *(ulong*)(buffer + 8) = low;
        }

        public bool Equals(ENetIP other) => high == other.high && low == other.low;
        public override bool Equals(object? obj) => obj is ENetIP other && Equals(other);
        public override int GetHashCode() => ((16337 + (int)high) ^ ((int)(high >> 32) * 31 + (int)low) ^ (int)(low >> 32)) * 31;

        public override string ToString()
        {
            var buffer = stackalloc byte[64];
            _ = enet_get_ip((ENetIP*)Unsafe.AsPointer(ref this), buffer, 64);
            return new string((sbyte*)buffer);
        }

        public static bool operator ==(ENetIP left, ENetIP right) => left.high == right.high && left.low == right.low;
        public static bool operator !=(ENetIP left, ENetIP right) => left.high != right.high || left.low != right.low;
    }

    [StructLayout(LayoutKind.Explicit, Size = 24)]
    public unsafe struct ENetAddress : IEquatable<ENetAddress>
    {
        [FieldOffset(0)] public ENetIP host;
        [FieldOffset(16)] public UInt16 port;

        public bool Equals(ENetAddress other) => host == other.host && port == other.port;
        public override bool Equals(object? obj) => obj is ENetAddress other && Equals(other);
        public override int GetHashCode() => host.GetHashCode() + port;

        public override string ToString()
        {
            var buffer = stackalloc byte[64];
            _ = enet_get_ip((ENetAddress*)Unsafe.AsPointer(ref this), buffer, 64);
            return new string((sbyte*)buffer) + ":" + port;
        }

        public static bool operator ==(ENetAddress left, ENetAddress right) => left.host == right.host && left.port == right.port;
        public static bool operator !=(ENetAddress left, ENetAddress right) => left.host != right.host || left.port != right.port;
    }

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
        public nint referenceCount;
        public uint flags;
        public byte* data;
        public nint dataLength;
        public delegate* managed<ENetPacket*, void> freeCallback;
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

    public unsafe struct ENetPeer
    {
        public ENetListNode dispatchList;
        public ENetHost* host;
        public ushort outgoingPeerID;
        public ushort incomingPeerID;
        public uint connectID;
        public byte outgoingSessionID;
        public byte incomingSessionID;
        public ENetAddress address;
        public void* data;
        public ENetPeerState state;
        public ENetChannel* channels;
        public nint channelCount;
        public uint incomingBandwidth;
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
        public nint totalWaitingData;
    }

    public unsafe struct ENetCompressor
    {
        public void* context;
        public delegate* managed<void*, ENetBuffer*, nint, nint, byte*, nint, nint> compress;
        public delegate* managed<void*, byte*, nint, byte*, nint, nint> decompress;
        public delegate* managed<void*, void> destroy;
    }

    public unsafe struct ENetHost
    {
        public long socket;
        public ENetAddress address;
        public uint incomingBandwidth;
        public uint outgoingBandwidth;
        public uint bandwidthThrottleEpoch;
        public uint mtu;
        public uint randomSeed;
        public int recalculateBandwidthLimits;
        public ENetPeer* peers;
        public nint peerCount;
        public nint channelLimit;
        public uint serviceTime;
        public ENetList dispatchQueue;
        public uint totalQueued;
        public nint packetSize;
        public ushort headerFlags;
        public ENetProtocol* commands;
        public nint commandCount;
        public ENetBuffer* buffers;
        public nint bufferCount;
        public delegate* managed<ENetBuffer*, nint, uint> checksum;
        public ENetCompressor compressor;
        public byte** packetData;
        public ENetAddress receivedAddress;
        public byte* receivedData;
        public nint receivedDataLength;
        public uint totalSentData;
        public uint totalSentPackets;
        public uint totalReceivedData;
        public uint totalReceivedPackets;
        public delegate* managed<ENetHost*, ENetEvent*, int> intercept;
        public nint connectedPeers;
        public nint bandwidthLimitedPeers;
        public nint duplicatePeers;
        public nint maximumPacketSize;
        public nint maximumWaitingData;
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
        public byte channelID;
        public uint data;
        public ENetPacket* packet;
    }
}