using enet;

// ReSharper disable ALL

namespace Enet
{
    /// <summary>
    ///     Represents the possible states of a peer in the ENet library.
    ///     These states reflect the lifecycle of a connection.
    /// </summary>
    public enum EnetPeerState
    {
        /// <summary>
        ///     The peer has no active connection.
        /// </summary>
        Disconnected = ENetPeerState.ENET_PEER_STATE_DISCONNECTED,

        /// <summary>
        ///     A connection request is in progress towards the foreign host.
        /// </summary>
        Connecting = ENetPeerState.ENET_PEER_STATE_CONNECTING,

        /// <summary>
        ///     The connect request has been acknowledged and the peer is awaiting final confirmation.
        /// </summary>
        AcknowledgingConnect = ENetPeerState.ENET_PEER_STATE_ACKNOWLEDGING_CONNECT,

        /// <summary>
        ///     The connection has been established and is pending dispatch of the connect event.
        /// </summary>
        ConnectionPending = ENetPeerState.ENET_PEER_STATE_CONNECTION_PENDING,

        /// <summary>
        ///     The connection attempt completed successfully.
        /// </summary>
        ConnectionSucceeded = ENetPeerState.ENET_PEER_STATE_CONNECTION_SUCCEEDED,

        /// <summary>
        ///     The peer is fully connected and can send and receive data.
        /// </summary>
        Connected = ENetPeerState.ENET_PEER_STATE_CONNECTED,

        /// <summary>
        ///     A disconnect is requested after all queued outgoing packets have been sent.
        /// </summary>
        DisconnectLater = ENetPeerState.ENET_PEER_STATE_DISCONNECT_LATER,

        /// <summary>
        ///     A disconnect request has been sent and the peer is waiting for it to complete.
        /// </summary>
        Disconnecting = ENetPeerState.ENET_PEER_STATE_DISCONNECTING,

        /// <summary>
        ///     The disconnect request has been acknowledged and the peer is awaiting final confirmation.
        /// </summary>
        AcknowledgingDisconnect = ENetPeerState.ENET_PEER_STATE_ACKNOWLEDGING_DISCONNECT,

        /// <summary>
        ///     The peer has disconnected and is waiting to be reclaimed after the disconnect event is dispatched.
        /// </summary>
        Zombie = ENetPeerState.ENET_PEER_STATE_ZOMBIE
    }
}