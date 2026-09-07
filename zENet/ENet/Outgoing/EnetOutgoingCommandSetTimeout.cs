// ReSharper disable ALL

namespace ThreadedEnet
{
    /// <summary>
    ///     The payload of an outgoing set timeout command.
    /// </summary>
    internal struct EnetOutgoingCommandSetTimeout
    {
        /// <summary>
        ///     The unique identifier of the target peer.
        /// </summary>
        public EnetUid Uid;

        /// <summary>
        ///     The timeout limit for the peer.
        /// </summary>
        public uint TimeoutLimit;

        /// <summary>
        ///     The minimum timeout value.
        /// </summary>
        public uint TimeoutMinimum;

        /// <summary>
        ///     The maximum timeout value.
        /// </summary>
        public uint TimeoutMaximum;
    }
}