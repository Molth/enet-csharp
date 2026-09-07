using enet;
using Enet;

// ReSharper disable ALL

namespace ThreadedEnet
{
    /// <summary>
    ///     Configuration options used to create and run a threaded ENet host.
    /// </summary>
    public struct EnetHostConfig
    {
        /// <summary>
        ///     The local address to bind the host socket to.
        /// </summary>
        public ENetAddress LocalAddress;

        /// <summary>
        ///     The maximum number of peers that can be connected to the host simultaneously.
        /// </summary>
        public nuint PeerCount;

        /// <summary>
        ///     The maximum number of channels allowed per peer;
        ///     zero uses the protocol default.
        /// </summary>
        public nuint ChannelLimit;

        /// <summary>
        ///     The downstream bandwidth limit in bytes per second;
        ///     zero means unlimited.
        /// </summary>
        public uint IncomingBandwidth;

        /// <summary>
        ///     The upstream bandwidth limit in bytes per second;
        ///     zero means unlimited.
        /// </summary>
        public uint OutgoingBandwidth;

        /// <summary>
        ///     The IP addressing mode to use (IPv4, IPv6-only, or IPv6 dual-stack).
        /// </summary>
        public EnetHostOption Option;

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
        ///     The maximum number of duplicate peers the host will track;
        ///     zero uses the internal default.
        /// </summary>
        public nuint MaxDuplicatePeers;

        /// <summary>
        ///     The maximum time in milliseconds the background thread waits for network events during each service pass.
        /// </summary>
        public uint ServiceTimeout;
    }
}