using enet;

// ReSharper disable ALL

namespace Enet
{
    /// <summary>
    ///     The addressing mode used when creating a host.
    /// </summary>
    public enum EnetHostOption
    {
        /// <summary>
        ///     Use Ipv4 addressing only.
        /// </summary>
        Ipv4 = ENetHostOption.ENET_HOSTOPT_IPV4,

        /// <summary>
        ///     Use Ipv6 addressing only.
        /// </summary>
        Ipv6Only = ENetHostOption.ENET_HOSTOPT_IPV6_ONLY,

        /// <summary>
        ///     Use Ipv6 dual stack addressing, accepting both Ipv4 and Ipv6.
        /// </summary>
        Ipv6DualMode = ENetHostOption.ENET_HOSTOPT_IPV6_DUALMODE
    }
}