using Enet;
using NativeCollections;

// ReSharper disable ALL

namespace ThreadedEnet
{
    /// <summary>
    ///     The payload of an outgoing broadcast selected command.
    /// </summary>
    internal struct EnetOutgoingCommandBroadcastSelected
    {
        /// <summary>
        ///     channel on the peer that generated the event, if appropriate
        /// </summary>
        public byte ChannelId;

        /// TODO
        public NativeArray<byte> IncomingPeerIds;

        /// <summary>
        ///     packet associated with the event, if appropriate
        /// </summary>
        public EnetPacket Packet;
    }
}