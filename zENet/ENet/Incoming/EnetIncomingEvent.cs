using enet;
using Enet;

// ReSharper disable ALL

namespace ThreadedEnet
{
    /// <summary>
    ///     An ENet event as returned by enet_host_service().
    /// </summary>
    internal struct EnetIncomingEvent
    {
        /// <summary>
        ///     type of the event
        /// </summary>
        public EnetEventType Type;

        /// <summary>
        ///     The unique identifier of the peer that generated the event.
        /// </summary>
        public EnetUid Uid;

        /// <summary>
        ///     The network address of the peer that generated the event.
        /// </summary>
        public ENetAddress Address;

        /// <summary>
        ///     The command payload associated with the event;
        ///     the active member depends on <see cref="Type" />.
        /// </summary>
        public EnetIncomingCommand Command;
    }
}