// ReSharper disable ALL

namespace ENet
{
    /// <summary>
    ///     The payload of an outgoing configure throttle command.
    /// </summary>
    internal struct EnetOutgoingCommandConfigureThrottle
    {
        /// <summary>
        ///     The unique identifier of the target peer.
        /// </summary>
        public EnetUid Uid;

        /// <summary>
        ///     The interval over which throttle statistics are measured, in milliseconds.
        /// </summary>
        public uint Interval;

        /// <summary>
        ///     The rate at which to increase the throttle probability as mean RTT declines.
        /// </summary>
        public uint Acceleration;

        /// <summary>
        ///     The rate at which to decrease the throttle probability as mean RTT increases.
        /// </summary>
        public uint Deceleration;
    }
}