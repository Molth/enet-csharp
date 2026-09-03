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
        ///     The host will use Ipv4 only.
        /// </summary>
        Ipv4 = ENetHostOption.ENET_HOSTOPT_IPV4,

        /// <summary>
        ///     The host will use Ipv6 only (no Ipv4 mapping or dual‑stack).
        /// </summary>
        Ipv6Only = ENetHostOption.ENET_HOSTOPT_IPV6_ONLY,

        /// <summary>
        ///     The host will use Ipv6 in dual‑mode, allowing both Ipv6 and Ipv4 connections.
        /// </summary>
        Ipv6DualMode = ENetHostOption.ENET_HOSTOPT_IPV6_DUALMODE
    }
}