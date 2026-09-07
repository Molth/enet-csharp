// ReSharper disable ALL

namespace ENet
{
    /// <summary>
    ///     An event describing an operation requested through the public API and queued for the background thread.
    /// </summary>
    internal struct EnetOutgoingEvent
    {
        /// <summary>
        ///     type of the event
        /// </summary>
        public EnetOutgoingEventType Type;

        /// <summary>
        ///     The command payload; the active member depends on <see cref="Type" />.
        /// </summary>
        public EnetOutgoingCommand Command;
    }
}