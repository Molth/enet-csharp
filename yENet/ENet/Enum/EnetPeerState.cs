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
        ///     The peer is not connected or has been disconnected.
        /// </summary>
        Disconnected = ENetPeerState.ENET_PEER_STATE_DISCONNECTED,

        /// <summary>
        ///     The peer is attempting to establish a connection.
        /// </summary>
        Connecting = ENetPeerState.ENET_PEER_STATE_CONNECTING,

        /// <summary>
        ///     The peer has received a connect request and is acknowledging it.
        /// </summary>
        AcknowledgingConnect = ENetPeerState.ENET_PEER_STATE_ACKNOWLEDGING_CONNECT,

        /// <summary>
        ///     The peer has sent a connect request and is waiting for the acknowledgment.
        /// </summary>
        ConnectionPending = ENetPeerState.ENET_PEER_STATE_CONNECTION_PENDING,

        /// <summary>
        ///     The peer has successfully completed the connection handshake.
        /// </summary>
        ConnectionSucceeded = ENetPeerState.ENET_PEER_STATE_CONNECTION_SUCCEEDED,

        /// <summary>
        ///     The peer is fully connected and can send and receive data.
        /// </summary>
        Connected = ENetPeerState.ENET_PEER_STATE_CONNECTED,

        /// <summary>
        ///     The peer has been requested to disconnect, but is still sending queued data.
        /// </summary>
        DisconnectLater = ENetPeerState.ENET_PEER_STATE_DISCONNECT_LATER,

        /// <summary>
        ///     The peer is in the process of disconnecting.
        /// </summary>
        Disconnecting = ENetPeerState.ENET_PEER_STATE_DISCONNECTING,

        /// <summary>
        ///     The peer has received a disconnect request and is acknowledging it.
        /// </summary>
        AcknowledgingDisconnect = ENetPeerState.ENET_PEER_STATE_ACKNOWLEDGING_DISCONNECT,

        /// <summary>
        ///     The peer has been fully disconnected but the object is not yet destroyed.
        /// </summary>
        Zombie = ENetPeerState.ENET_PEER_STATE_ZOMBIE
    }
}