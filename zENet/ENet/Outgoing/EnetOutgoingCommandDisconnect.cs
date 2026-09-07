// ReSharper disable ALL

namespace ENet
{
    /// <summary>
    ///     The payload of an outgoing disconnect command.
    /// </summary>
    internal struct EnetOutgoingCommandDisconnect
    {
        /// <summary>
        ///     The unique identifier of the peer to disconnect.
        /// </summary>
        public EnetUid Uid;

        /// <summary>
        ///     data associated with the event, if appropriate
        /// </summary>
        public uint EventData;
    }
}