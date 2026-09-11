// ReSharper disable ALL

namespace ThreadedEnet
{
    /// <summary>
    ///     Identifies the kind of operation requested through the outgoing event queue.
    /// </summary>
    internal enum EnetOutgoingEventType
    {
        /// <summary>
        ///     Requests a connection to a remote host.
        /// </summary>
        Connect,

        /// <summary>
        ///     Requests a graceful disconnection of a peer.
        /// </summary>
        Disconnect,

        /// <summary>
        ///     Requests sending a packet to a specific peer.
        /// </summary>
        Send,

        /// <summary>
        ///     Requests broadcasting a packet to all connected peers.
        /// </summary>
        Broadcast,

        /// <summary>
        ///     Requests broadcasting a packet to a selected set of peers.
        /// </summary>
        BroadcastSelected,

        /// <summary>
        ///     Requests sending a ping packet to an address.
        /// </summary>
        Ping,

        /// <summary>
        ///     Requests setting the ping interval for a peer.
        /// </summary>
        SetPingInterval,

        /// <summary>
        ///     Requests setting the timeout parameters for a peer.
        /// </summary>
        SetTimeout,

        /// <summary>
        ///     Requests configuring throttle parameters for a peer.
        /// </summary>
        ConfigureThrottle
    }
}