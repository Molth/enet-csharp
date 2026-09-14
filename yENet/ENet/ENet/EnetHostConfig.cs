using enet;

// ReSharper disable ALL

namespace Enet
{
    /// <summary>
    ///     Specifies the configuration parameters used to create an <see cref="EnetHost" />.
    /// </summary>
    /// <remarks>
    ///     Groups the parameters of
    ///     <see cref="EnetHost.Create(ENetAddress, nuint, nuint, uint, uint, EnetHostOption)" /> into a single
    ///     value so a host can be created from a reusable configuration.
    /// </remarks>
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
        ///     zero uses <see cref="ENet.ENET_PROTOCOL_MAXIMUM_CHANNEL_COUNT" />.
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
        ///     The IP addressing mode to use (Ipv4, Ipv6-only, or Ipv6 dual-stack).
        /// </summary>
        public EnetHostOption Option;

        /// <summary>
        ///     Initializes a new instance of the <see cref="EnetHostConfig" /> struct with the specified values.
        /// </summary>
        /// <param name="localAddress">
        ///     The address to bind the host to.
        /// </param>
        /// <param name="peerCount">
        ///     The maximum number of peers that can be connected to this host simultaneously.
        /// </param>
        /// <param name="channelLimit">
        ///     The maximum number of channels allowed per peer. Pass <c>0</c> to use the default limit (
        ///     <see cref="ENet.ENET_PROTOCOL_MAXIMUM_CHANNEL_COUNT" />).
        /// </param>
        /// <param name="incomingBandwidth">
        ///     The downstream bandwidth limit in bytes per second. Pass <c>0</c> for unlimited bandwidth.
        /// </param>
        /// <param name="outgoingBandwidth">
        ///     The upstream bandwidth limit in bytes per second. Pass <c>0</c> for unlimited bandwidth.
        /// </param>
        /// <param name="option">
        ///     Specifies the IP addressing mode to use (Ipv4, Ipv6-only, or Ipv6 dual‑stack).
        /// </param>
        public EnetHostConfig(ENetAddress localAddress, nuint peerCount, nuint channelLimit, uint incomingBandwidth, uint outgoingBandwidth, EnetHostOption option)
        {
            LocalAddress = localAddress;
            PeerCount = peerCount;
            ChannelLimit = channelLimit;
            IncomingBandwidth = incomingBandwidth;
            OutgoingBandwidth = outgoingBandwidth;
            Option = option;
        }
    }
}