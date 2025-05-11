﻿using System;
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
        public nuint referenceCount;
        public uint flags;
        public byte* data;
        public nuint dataLength;
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
        public nuint channelCount;
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
        public nuint totalWaitingData;
    }

    public unsafe struct ENetCompressor
    {
        public void* context;
        public delegate* managed<void*, ENetBuffer*, nuint, nuint, byte*, nuint, nuint> compress;
        public delegate* managed<void*, byte*, nuint, byte*, nuint, nuint> decompress;
        public delegate* managed<void*, void> destroy;
    }

    public unsafe struct ENetHost
    {
        public nint socket;
        public ENetAddress address;
        public uint incomingBandwidth;
        public uint outgoingBandwidth;
        public uint bandwidthThrottleEpoch;
        public uint mtu;
        public uint randomSeed;
        public int recalculateBandwidthLimits;
        public ENetPeer* peers;
        public nuint peerCount;
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
        public delegate* managed<ENetBuffer*, nuint, uint> checksum;
        public ENetCompressor compressor;
        public ENetPacketData packetData;
        public ENetAddress receivedAddress;
        public byte* receivedData;
        public nuint receivedDataLength;
        public uint totalSentData;
        public uint totalSentPackets;
        public uint totalReceivedData;
        public uint totalReceivedPackets;
        public delegate* managed<ENetHost*, ENetEvent*, int> intercept;
        public nuint connectedPeers;
        public nuint bandwidthLimitedPeers;
        public nuint duplicatePeers;
        public nuint maximumPacketSize;
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