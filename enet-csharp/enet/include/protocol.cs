using System.Runtime.InteropServices;
using enet_uint8 = byte;
using enet_uint16 = ushort;
using enet_uint32 = uint;

// ReSharper disable ALL

namespace enet
{
    public static partial class ENet
    {
        public const enet_uint32 ENET_PROTOCOL_MINIMUM_MTU = 576;
        public const enet_uint32 ENET_PROTOCOL_MAXIMUM_MTU = 4096;
        public const enet_uint32 ENET_PROTOCOL_MAXIMUM_PACKET_COMMANDS = 32;
        public const enet_uint32 ENET_PROTOCOL_MINIMUM_WINDOW_SIZE = 4096;
        public const enet_uint32 ENET_PROTOCOL_MAXIMUM_WINDOW_SIZE = 65536;
        public const enet_uint32 ENET_PROTOCOL_MINIMUM_CHANNEL_COUNT = 1;
        public const enet_uint32 ENET_PROTOCOL_MAXIMUM_CHANNEL_COUNT = 255;
        public const enet_uint32 ENET_PROTOCOL_MAXIMUM_PEER_ID = 0xFFF;
        public const enet_uint32 ENET_PROTOCOL_MAXIMUM_FRAGMENT_COUNT = 1024 * 1024;
    }

    public enum ENetProtocolCommand
    {
        ENET_PROTOCOL_COMMAND_NONE = 0,
        ENET_PROTOCOL_COMMAND_ACKNOWLEDGE = 1,
        ENET_PROTOCOL_COMMAND_CONNECT = 2,
        ENET_PROTOCOL_COMMAND_VERIFY_CONNECT = 3,
        ENET_PROTOCOL_COMMAND_DISCONNECT = 4,
        ENET_PROTOCOL_COMMAND_PING = 5,
        ENET_PROTOCOL_COMMAND_SEND_RELIABLE = 6,
        ENET_PROTOCOL_COMMAND_SEND_UNRELIABLE = 7,
        ENET_PROTOCOL_COMMAND_SEND_FRAGMENT = 8,
        ENET_PROTOCOL_COMMAND_SEND_UNSEQUENCED = 9,
        ENET_PROTOCOL_COMMAND_BANDWIDTH_LIMIT = 10,
        ENET_PROTOCOL_COMMAND_THROTTLE_CONFIGURE = 11,
        ENET_PROTOCOL_COMMAND_SEND_UNRELIABLE_FRAGMENT = 12,
        ENET_PROTOCOL_COMMAND_COUNT = 13,

        ENET_PROTOCOL_COMMAND_MASK = 0x0F
    }

    public enum ENetProtocolFlag
    {
        ENET_PROTOCOL_COMMAND_FLAG_ACKNOWLEDGE = (1 << 7),
        ENET_PROTOCOL_COMMAND_FLAG_UNSEQUENCED = (1 << 6),

        ENET_PROTOCOL_HEADER_FLAG_COMPRESSED = (1 << 14),
        ENET_PROTOCOL_HEADER_FLAG_SENT_TIME = (1 << 15),
        ENET_PROTOCOL_HEADER_FLAG_MASK = ENET_PROTOCOL_HEADER_FLAG_COMPRESSED | ENET_PROTOCOL_HEADER_FLAG_SENT_TIME,

        ENET_PROTOCOL_HEADER_SESSION_MASK = (3 << 12),
        ENET_PROTOCOL_HEADER_SESSION_SHIFT = 12
    }

    public struct ENetProtocolHeader
    {
        public enet_uint16 peerID;
        public enet_uint16 sentTime;
    }

    public struct ENetProtocolCommandHeader
    {
        public enet_uint8 command;
        public enet_uint8 channelID;
        public enet_uint16 reliableSequenceNumber;
    }

    public struct ENetProtocolAcknowledge
    {
        public ENetProtocolCommandHeader header;
        public enet_uint16 receivedReliableSequenceNumber;
        public enet_uint16 receivedSentTime;
    }

    public struct ENetProtocolConnect
    {
        public ENetProtocolCommandHeader header;
        public enet_uint16 outgoingPeerID;
        public enet_uint8 incomingSessionID;
        public enet_uint8 outgoingSessionID;
        public enet_uint32 mtu;
        public enet_uint32 windowSize;
        public enet_uint32 channelCount;
        public enet_uint32 incomingBandwidth;
        public enet_uint32 outgoingBandwidth;
        public enet_uint32 packetThrottleInterval;
        public enet_uint32 packetThrottleAcceleration;
        public enet_uint32 packetThrottleDeceleration;
        public enet_uint32 connectID;
        public enet_uint32 data;
    }

    public struct ENetProtocolVerifyConnect
    {
        public ENetProtocolCommandHeader header;
        public enet_uint16 outgoingPeerID;
        public enet_uint8 incomingSessionID;
        public enet_uint8 outgoingSessionID;
        public enet_uint32 mtu;
        public enet_uint32 windowSize;
        public enet_uint32 channelCount;
        public enet_uint32 incomingBandwidth;
        public enet_uint32 outgoingBandwidth;
        public enet_uint32 packetThrottleInterval;
        public enet_uint32 packetThrottleAcceleration;
        public enet_uint32 packetThrottleDeceleration;
        public enet_uint32 connectID;
    }

    public struct ENetProtocolBandwidthLimit
    {
        public ENetProtocolCommandHeader header;
        public enet_uint32 incomingBandwidth;
        public enet_uint32 outgoingBandwidth;
    }

    public struct ENetProtocolThrottleConfigure
    {
        public ENetProtocolCommandHeader header;
        public enet_uint32 packetThrottleInterval;
        public enet_uint32 packetThrottleAcceleration;
        public enet_uint32 packetThrottleDeceleration;
    }

    public struct ENetProtocolDisconnect
    {
        public ENetProtocolCommandHeader header;
        public enet_uint32 data;
    }

    public struct ENetProtocolPing
    {
        public ENetProtocolCommandHeader header;
    }

    public struct ENetProtocolSendReliable
    {
        public ENetProtocolCommandHeader header;
        public enet_uint16 dataLength;
    }

    public struct ENetProtocolSendUnreliable
    {
        public ENetProtocolCommandHeader header;
        public enet_uint16 unreliableSequenceNumber;
        public enet_uint16 dataLength;
    }

    public struct ENetProtocolSendUnsequenced
    {
        public ENetProtocolCommandHeader header;
        public enet_uint16 unsequencedGroup;
        public enet_uint16 dataLength;
    }

    public struct ENetProtocolSendFragment
    {
        public ENetProtocolCommandHeader header;
        public enet_uint16 startSequenceNumber;
        public enet_uint16 dataLength;
        public enet_uint32 fragmentCount;
        public enet_uint32 fragmentNumber;
        public enet_uint32 totalLength;
        public enet_uint32 fragmentOffset;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct ENetProtocol
    {
        [FieldOffset(0)] public ENetProtocolCommandHeader header;
        [FieldOffset(0)] public ENetProtocolAcknowledge acknowledge;
        [FieldOffset(0)] public ENetProtocolConnect connect;
        [FieldOffset(0)] public ENetProtocolVerifyConnect verifyConnect;
        [FieldOffset(0)] public ENetProtocolDisconnect disconnect;
        [FieldOffset(0)] public ENetProtocolPing ping;
        [FieldOffset(0)] public ENetProtocolSendReliable sendReliable;
        [FieldOffset(0)] public ENetProtocolSendUnreliable sendUnreliable;
        [FieldOffset(0)] public ENetProtocolSendUnsequenced sendUnsequenced;
        [FieldOffset(0)] public ENetProtocolSendFragment sendFragment;
        [FieldOffset(0)] public ENetProtocolBandwidthLimit bandwidthLimit;
        [FieldOffset(0)] public ENetProtocolThrottleConfigure throttleConfigure;
    }
}