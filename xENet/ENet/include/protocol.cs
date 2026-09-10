using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace enet
{
    public static partial class ENet
    {
        /// <summary>
        ///     The smallest MTU accepted by the protocol.
        /// </summary>
        public const uint ENET_PROTOCOL_MINIMUM_MTU = 576;

        /// <summary>
        ///     The largest MTU accepted by the protocol.
        /// </summary>
        public const uint ENET_PROTOCOL_MAXIMUM_MTU = 4096;

        /// <summary>
        ///     The maximum number of commands that can be packed into a single packet.
        /// </summary>
        public const uint ENET_PROTOCOL_MAXIMUM_PACKET_COMMANDS = 32;

        /// <summary>
        ///     The smallest receive window size accepted by the protocol.
        /// </summary>
        public const uint ENET_PROTOCOL_MINIMUM_WINDOW_SIZE = 4096;

        /// <summary>
        ///     The largest receive window size accepted by the protocol.
        /// </summary>
        public const uint ENET_PROTOCOL_MAXIMUM_WINDOW_SIZE = 65536;

        /// <summary>
        ///     The smallest number of channels accepted by the protocol.
        /// </summary>
        public const uint ENET_PROTOCOL_MINIMUM_CHANNEL_COUNT = 1;

        /// <summary>
        ///     The largest number of channels accepted by the protocol.
        /// </summary>
        public const uint ENET_PROTOCOL_MAXIMUM_CHANNEL_COUNT = 255;

        /// <summary>
        ///     The largest peer identifier that can be encoded in the packet header.
        /// </summary>
        public const uint ENET_PROTOCOL_MAXIMUM_PEER_ID = 0xFFF;

        /// <summary>
        ///     The maximum number of fragments allowed for a single packet.
        /// </summary>
        public const uint ENET_PROTOCOL_MAXIMUM_FRAGMENT_COUNT = 1024 * 1024;
    }

    /// <summary>
    ///     The command types that can appear in a protocol packet.
    /// </summary>
    public enum ENetProtocolCommand
    {
        /// <summary>
        ///     An empty command with no meaning.
        /// </summary>
        ENET_PROTOCOL_COMMAND_NONE = 0,

        /// <summary>
        ///     Acknowledges receipt of a reliable command.
        /// </summary>
        ENET_PROTOCOL_COMMAND_ACKNOWLEDGE = 1,

        /// <summary>
        ///     Initiates a connection from a peer to a host.
        /// </summary>
        ENET_PROTOCOL_COMMAND_CONNECT = 2,

        /// <summary>
        ///     Confirms a connection request from a host to a peer.
        /// </summary>
        ENET_PROTOCOL_COMMAND_VERIFY_CONNECT = 3,

        /// <summary>
        ///     Requests termination of a connection.
        /// </summary>
        ENET_PROTOCOL_COMMAND_DISCONNECT = 4,

        /// <summary>
        ///     A keep-alive probe sent to maintain a connection.
        /// </summary>
        ENET_PROTOCOL_COMMAND_PING = 5,

        /// <summary>
        ///     Carries a reliably delivered packet.
        /// </summary>
        ENET_PROTOCOL_COMMAND_SEND_RELIABLE = 6,

        /// <summary>
        ///     Carries a packet delivered without acknowledgement.
        /// </summary>
        ENET_PROTOCOL_COMMAND_SEND_UNRELIABLE = 7,

        /// <summary>
        ///     Carries a fragment of a fragmented reliable packet.
        /// </summary>
        ENET_PROTOCOL_COMMAND_SEND_FRAGMENT = 8,

        /// <summary>
        ///     Carries a packet delivered without sequence ordering.
        /// </summary>
        ENET_PROTOCOL_COMMAND_SEND_UNSEQUENCED = 9,

        /// <summary>
        ///     Adjusts the bandwidth limits of the remote peer.
        /// </summary>
        ENET_PROTOCOL_COMMAND_BANDWIDTH_LIMIT = 10,

        /// <summary>
        ///     Adjusts the packet throttle parameters of the remote peer.
        /// </summary>
        ENET_PROTOCOL_COMMAND_THROTTLE_CONFIGURE = 11,

        /// <summary>
        ///     Carries a fragment of an unreliable packet.
        /// </summary>
        ENET_PROTOCOL_COMMAND_SEND_UNRELIABLE_FRAGMENT = 12,

        /// <summary>
        ///     The number of defined command types.
        /// </summary>
        ENET_PROTOCOL_COMMAND_COUNT = 13,

        /// <summary>
        ///     Bit mask used to isolate the command type from the command flags.
        /// </summary>
        ENET_PROTOCOL_COMMAND_MASK = 0x0F
    }

    /// <summary>
    ///     Flags and header bit masks used by the protocol.
    /// </summary>
    public enum ENetProtocolFlag
    {
        /// <summary>
        ///     Marks a command as requiring an acknowledgement.
        /// </summary>
        ENET_PROTOCOL_COMMAND_FLAG_ACKNOWLEDGE = (1 << 7),

        /// <summary>
        ///     Marks a command as unsequenced.
        /// </summary>
        ENET_PROTOCOL_COMMAND_FLAG_UNSEQUENCED = (1 << 6),

        /// <summary>
        ///     Header flag indicating the payload is compressed.
        /// </summary>
        ENET_PROTOCOL_HEADER_FLAG_COMPRESSED = (1 << 14),

        /// <summary>
        ///     Header flag indicating the packet carries a sent time.
        /// </summary>
        ENET_PROTOCOL_HEADER_FLAG_SENT_TIME = (1 << 15),

        /// <summary>
        ///     Bit mask covering all header flags.
        /// </summary>
        ENET_PROTOCOL_HEADER_FLAG_MASK = ENET_PROTOCOL_HEADER_FLAG_COMPRESSED | ENET_PROTOCOL_HEADER_FLAG_SENT_TIME,

        /// <summary>
        ///     Bit mask isolating the session field in the packet header.
        /// </summary>
        ENET_PROTOCOL_HEADER_SESSION_MASK = (3 << 12),

        /// <summary>
        ///     Bit offset of the session field in the packet header.
        /// </summary>
        ENET_PROTOCOL_HEADER_SESSION_SHIFT = 12
    }

    /// <summary>
    ///     The fixed header that precedes every protocol packet.
    /// </summary>
    public struct ENetProtocolHeader
    {
        /// <summary>
        ///     The identifier of the sending peer, with session bits in the high nibble.
        /// </summary>
        public ushort peerID;

        /// <summary>
        ///     The time the packet was sent, present only when the sent time flag is set.
        /// </summary>
        public ushort sentTime;
    }

    /// <summary>
    ///     The header shared by every protocol command.
    /// </summary>
    public struct ENetProtocolCommandHeader
    {
        /// <summary>
        ///     The command type combined with its flags.
        /// </summary>
        public byte command;

        /// <summary>
        ///     The channel the command is delivered on.
        /// </summary>
        public byte channelID;

        /// <summary>
        ///     The reliable sequence number of the command.
        /// </summary>
        public ushort reliableSequenceNumber;
    }

    /// <summary>
    ///     The acknowledgement command used to confirm receipt of reliable commands.
    /// </summary>
    public struct ENetProtocolAcknowledge
    {
        /// <summary>
        ///     The common command header.
        /// </summary>
        public ENetProtocolCommandHeader header;

        /// <summary>
        ///     The reliable sequence number being acknowledged.
        /// </summary>
        public ushort receivedReliableSequenceNumber;

        /// <summary>
        ///     The sent time reported by the acknowledged command.
        /// </summary>
        public ushort receivedSentTime;
    }

    /// <summary>
    ///     The connection request command sent when a peer initiates a connection.
    /// </summary>
    public struct ENetProtocolConnect
    {
        /// <summary>
        ///     The common command header.
        /// </summary>
        public ENetProtocolCommandHeader header;

        /// <summary>
        ///     The peer identifier assigned to the connecting peer by the initiator.
        /// </summary>
        public ushort outgoingPeerID;

        /// <summary>
        ///     The session identifier of the incoming connection.
        /// </summary>
        public byte incomingSessionID;

        /// <summary>
        ///     The session identifier of the outgoing connection.
        /// </summary>
        public byte outgoingSessionID;

        /// <summary>
        ///     The maximum transmission unit negotiated for the connection.
        /// </summary>
        public uint mtu;

        /// <summary>
        ///     The receive window size requested by the initiator.
        /// </summary>
        public uint windowSize;

        /// <summary>
        ///     The number of channels negotiated for the connection.
        /// </summary>
        public uint channelCount;

        /// <summary>
        ///     The inbound bandwidth limit in bytes per second.
        /// </summary>
        public uint incomingBandwidth;

        /// <summary>
        ///     The outbound bandwidth limit in bytes per second.
        /// </summary>
        public uint outgoingBandwidth;

        /// <summary>
        ///     The packet throttle measurement interval in milliseconds.
        /// </summary>
        public uint packetThrottleInterval;

        /// <summary>
        ///     The packet throttle acceleration rate.
        /// </summary>
        public uint packetThrottleAcceleration;

        /// <summary>
        ///     The packet throttle deceleration rate.
        /// </summary>
        public uint packetThrottleDeceleration;

        /// <summary>
        ///     A unique identifier used to match the connection with its verification.
        /// </summary>
        public uint connectID;

        /// <summary>
        ///     Application defined data carried with the connection request.
        /// </summary>
        public uint data;
    }

    /// <summary>
    ///     The connection verification command sent in reply to a connection request.
    /// </summary>
    public struct ENetProtocolVerifyConnect
    {
        /// <summary>
        ///     The common command header.
        /// </summary>
        public ENetProtocolCommandHeader header;

        /// <summary>
        ///     The peer identifier assigned to the connecting peer by the responder.
        /// </summary>
        public ushort outgoingPeerID;

        /// <summary>
        ///     The session identifier of the incoming connection.
        /// </summary>
        public byte incomingSessionID;

        /// <summary>
        ///     The session identifier of the outgoing connection.
        /// </summary>
        public byte outgoingSessionID;

        /// <summary>
        ///     The maximum transmission unit negotiated for the connection.
        /// </summary>
        public uint mtu;

        /// <summary>
        ///     The receive window size accepted by the responder.
        /// </summary>
        public uint windowSize;

        /// <summary>
        ///     The number of channels negotiated for the connection.
        /// </summary>
        public uint channelCount;

        /// <summary>
        ///     The inbound bandwidth limit in bytes per second.
        /// </summary>
        public uint incomingBandwidth;

        /// <summary>
        ///     The outbound bandwidth limit in bytes per second.
        /// </summary>
        public uint outgoingBandwidth;

        /// <summary>
        ///     The packet throttle measurement interval in milliseconds.
        /// </summary>
        public uint packetThrottleInterval;

        /// <summary>
        ///     The packet throttle acceleration rate.
        /// </summary>
        public uint packetThrottleAcceleration;

        /// <summary>
        ///     The packet throttle deceleration rate.
        /// </summary>
        public uint packetThrottleDeceleration;

        /// <summary>
        ///     The connection identifier echoed from the connection request.
        /// </summary>
        public uint connectID;
    }

    /// <summary>
    ///     The bandwidth limit command used to adjust the remote peer bandwidth.
    /// </summary>
    public struct ENetProtocolBandwidthLimit
    {
        /// <summary>
        ///     The common command header.
        /// </summary>
        public ENetProtocolCommandHeader header;

        /// <summary>
        ///     The inbound bandwidth limit in bytes per second.
        /// </summary>
        public uint incomingBandwidth;

        /// <summary>
        ///     The outbound bandwidth limit in bytes per second.
        /// </summary>
        public uint outgoingBandwidth;
    }

    /// <summary>
    ///     The throttle configuration command used to adjust the remote peer throttle parameters.
    /// </summary>
    public struct ENetProtocolThrottleConfigure
    {
        /// <summary>
        ///     The common command header.
        /// </summary>
        public ENetProtocolCommandHeader header;

        /// <summary>
        ///     The packet throttle measurement interval in milliseconds.
        /// </summary>
        public uint packetThrottleInterval;

        /// <summary>
        ///     The packet throttle acceleration rate.
        /// </summary>
        public uint packetThrottleAcceleration;

        /// <summary>
        ///     The packet throttle deceleration rate.
        /// </summary>
        public uint packetThrottleDeceleration;
    }

    /// <summary>
    ///     The disconnect command used to terminate a connection.
    /// </summary>
    public struct ENetProtocolDisconnect
    {
        /// <summary>
        ///     The common command header.
        /// </summary>
        public ENetProtocolCommandHeader header;

        /// <summary>
        ///     Application defined data carried with the disconnect request.
        /// </summary>
        public uint data;
    }

    /// <summary>
    ///     The keep-alive ping command.
    /// </summary>
    public struct ENetProtocolPing
    {
        /// <summary>
        ///     The common command header.
        /// </summary>
        public ENetProtocolCommandHeader header;
    }

    /// <summary>
    ///     The reliable send command carrying an acknowledged payload.
    /// </summary>
    public struct ENetProtocolSendReliable
    {
        /// <summary>
        ///     The common command header.
        /// </summary>
        public ENetProtocolCommandHeader header;

        /// <summary>
        ///     The length in bytes of the payload that follows the command.
        /// </summary>
        public ushort dataLength;
    }

    /// <summary>
    ///     The unreliable send command carrying an unacknowledged payload.
    /// </summary>
    public struct ENetProtocolSendUnreliable
    {
        /// <summary>
        ///     The common command header.
        /// </summary>
        public ENetProtocolCommandHeader header;

        /// <summary>
        ///     The unreliable sequence number of the command.
        /// </summary>
        public ushort unreliableSequenceNumber;

        /// <summary>
        ///     The length in bytes of the payload that follows the command.
        /// </summary>
        public ushort dataLength;
    }

    /// <summary>
    ///     The unsequenced send command carrying an out-of-order payload.
    /// </summary>
    public struct ENetProtocolSendUnsequenced
    {
        /// <summary>
        ///     The common command header.
        /// </summary>
        public ENetProtocolCommandHeader header;

        /// <summary>
        ///     The group used to track the unsequenced payload.
        /// </summary>
        public ushort unsequencedGroup;

        /// <summary>
        ///     The length in bytes of the payload that follows the command.
        /// </summary>
        public ushort dataLength;
    }

    /// <summary>
    ///     The fragment send command carrying one piece of a fragmented packet.
    /// </summary>
    public struct ENetProtocolSendFragment
    {
        /// <summary>
        ///     The common command header.
        /// </summary>
        public ENetProtocolCommandHeader header;

        /// <summary>
        ///     The reliable sequence number of the first fragment of the packet.
        /// </summary>
        public ushort startSequenceNumber;

        /// <summary>
        ///     The length in bytes of the payload carried by this fragment.
        /// </summary>
        public ushort dataLength;

        /// <summary>
        ///     The total number of fragments the packet was split into.
        /// </summary>
        public uint fragmentCount;

        /// <summary>
        ///     The zero based index of this fragment.
        /// </summary>
        public uint fragmentNumber;

        /// <summary>
        ///     The total length in bytes of the original unfragmented packet.
        /// </summary>
        public uint totalLength;

        /// <summary>
        ///     The byte offset of this fragment within the original packet.
        /// </summary>
        public uint fragmentOffset;
    }

    /// <summary>
    ///     A union of all protocol commands overlaid at offset zero so the active command is read through the common header.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct ENetProtocol
    {
        /// <summary>
        ///     The common command header shared by all command variants.
        /// </summary>
        [FieldOffset(0)] public ENetProtocolCommandHeader header;

        /// <summary>
        ///     The acknowledgement command view.
        /// </summary>
        [FieldOffset(0)] public ENetProtocolAcknowledge acknowledge;

        /// <summary>
        ///     The connection request command view.
        /// </summary>
        [FieldOffset(0)] public ENetProtocolConnect connect;

        /// <summary>
        ///     The connection verification command view.
        /// </summary>
        [FieldOffset(0)] public ENetProtocolVerifyConnect verifyConnect;

        /// <summary>
        ///     The disconnect command view.
        /// </summary>
        [FieldOffset(0)] public ENetProtocolDisconnect disconnect;

        /// <summary>
        ///     The ping command view.
        /// </summary>
        [FieldOffset(0)] public ENetProtocolPing ping;

        /// <summary>
        ///     The reliable send command view.
        /// </summary>
        [FieldOffset(0)] public ENetProtocolSendReliable sendReliable;

        /// <summary>
        ///     The unreliable send command view.
        /// </summary>
        [FieldOffset(0)] public ENetProtocolSendUnreliable sendUnreliable;

        /// <summary>
        ///     The unsequenced send command view.
        /// </summary>
        [FieldOffset(0)] public ENetProtocolSendUnsequenced sendUnsequenced;

        /// <summary>
        ///     The fragment send command view.
        /// </summary>
        [FieldOffset(0)] public ENetProtocolSendFragment sendFragment;

        /// <summary>
        ///     The bandwidth limit command view.
        /// </summary>
        [FieldOffset(0)] public ENetProtocolBandwidthLimit bandwidthLimit;

        /// <summary>
        ///     The throttle configuration command view.
        /// </summary>
        [FieldOffset(0)] public ENetProtocolThrottleConfigure throttleConfigure;
    }
}