// ReSharper disable ALL

using Enet;

namespace ThreadedEnet
{
    /// <summary>
    ///     The payload of an outgoing send command.
    /// </summary>
    internal struct EnetOutgoingCommandSend
    {
        /// <summary>
        ///     The unique identifier of the target peer.
        /// </summary>
        public EnetUid Uid;

        /// <summary>
        ///     channel on the peer that generated the event, if appropriate
        /// </summary>
        public byte ChannelId;

        /// <summary>
        ///     packet associated with the event, if appropriate
        /// </summary>
        public EnetPacket Packet;
    }
}