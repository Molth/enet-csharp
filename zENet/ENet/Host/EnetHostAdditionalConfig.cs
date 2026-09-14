using enet;
using Enet;

// ReSharper disable ALL

namespace ThreadedEnet
{
    /// <summary>
    ///     Additional configuration applied to an <see cref="EnetHost" /> after it is created.
    /// </summary>
    /// <remarks>
    ///     Each field maps to the corresponding setter invoked by
    ///     <see cref="ThreadedManagedEnetHost.Start(ThreadedEnetHostConfig)" /> after the underlying host is created.
    /// </remarks>
    public struct EnetHostAdditionalConfig
    {
        /// <summary>
        ///     The packet compressor callbacks used to compress and decompress packets;
        ///     the default disables compression.
        /// </summary>
        public ENetCompressor Compressor;

        /// <summary>
        ///     A callback used to compute packet checksums,
        ///     or <see langword="null" /> to use the default checksum behavior.
        /// </summary>
        public unsafe delegate* managed<ENetBuffer*, nuint, uint> ChecksumCallback;

        /// <summary>
        ///     A callback invoked to intercept incoming events before they are processed,
        ///     or <see langword="null" /> to disable interception.
        /// </summary>
        public unsafe delegate* managed<ENetHost*, ENetEvent*, int> InterceptCallback;

        /// <summary>
        ///     When non-zero, the host ignores incoming connection requests instead of accepting them.
        /// </summary>
        public bool IgnoreConnectRequests;

        /// <summary>
        ///     The maximum transmission unit (MTU) used by the host;
        ///     zero uses <see cref="ENet.ENET_HOST_DEFAULT_MTU" />.
        /// </summary>
        public uint Mtu;

        /// <summary>
        ///     Number of allowed peers from duplicate IPs.
        ///     zero uses <see cref="ENet.ENET_PROTOCOL_MAXIMUM_PEER_ID" />.
        /// </summary>
        public nuint MaximumDuplicatePeers;

        /// <summary>
        ///     The maximum allowable packet size that may be sent or received on a peer;
        ///     zero uses <see cref="ENet.ENET_HOST_DEFAULT_MAXIMUM_PACKET_SIZE" />.
        /// </summary>
        public nuint MaximumPacketSize;

        /// <summary>
        ///     The maximum aggregate amount of buffer space a peer may use waiting for packets to be delivered;
        ///     zero uses <see cref="ENet.ENET_HOST_DEFAULT_MAXIMUM_WAITING_DATA" />.
        /// </summary>
        public nuint MaximumWaitingData;
    }
}