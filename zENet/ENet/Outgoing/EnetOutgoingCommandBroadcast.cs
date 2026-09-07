// ReSharper disable ALL

using Enet;

namespace ThreadedEnet
{
    /// <summary>
    ///     The payload of an outgoing broadcast command.
    /// </summary>
    internal struct EnetOutgoingCommandBroadcast
    {
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