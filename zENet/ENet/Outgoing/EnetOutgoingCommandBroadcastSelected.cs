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

        /// <summary>
        ///     a bit array in which bit <c>i</c> (i.e. the bit at byte <c>i / 8</c>, bit offset <c>i % 8</c>)
        ///     selects the peer whose incoming peer identifier is <c>i</c>
        /// </summary>
        public NativeArray<byte> BitArray;

        /// <summary>
        ///     packet associated with the event, if appropriate
        /// </summary>
        public EnetPacket Packet;
    }
}