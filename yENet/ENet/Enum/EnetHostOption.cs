using enet;

// ReSharper disable ALL

namespace Enet
{
    /// <summary>
    ///     Specifies the IP addressing options for an ENet host.
    /// </summary>
    public enum EnetHostOption
    {
        /// <summary>
        ///     The host listens using IPv4 addressing.
        /// </summary>
        Ipv4 = ENetHostOption.ENET_HOSTOPT_IPV4,

        /// <summary>
        ///     The host listens using IPv6 addressing only.
        /// </summary>
        Ipv6Only = ENetHostOption.ENET_HOSTOPT_IPV6_ONLY,

        /// <summary>
        ///     The host listens using IPv6 dual-stack mode, accepting both IPv4 and IPv6.
        /// </summary>
        Ipv6DualMode = ENetHostOption.ENET_HOSTOPT_IPV6_DUALMODE
    }
}