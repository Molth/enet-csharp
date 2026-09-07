// ReSharper disable ALL

namespace ThreadedEnet
{
    /// <summary>
    ///     The payload of an outgoing set ping interval command.
    /// </summary>
    internal struct EnetOutgoingCommandSetPingInterval
    {
        /// <summary>
        ///     The unique identifier of the target peer.
        /// </summary>
        public EnetUid Uid;

        /// <summary>
        ///     The interval between ping probes, in milliseconds.
        /// </summary>
        public uint PingInterval;
    }
}