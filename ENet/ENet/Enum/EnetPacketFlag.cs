using System;
using enet;

// ReSharper disable ALL

namespace Enet
{
    /// <summary>
    ///     Packet flag bit constants.
    /// </summary>
    [Flags]
    public enum EnetPacketFlag
    {
        /// <summary>
        ///     packet must be received by the target peer and resend attempts should be
        ///     made until the packet is delivered
        /// </summary>
        Reliable = ENetPacketFlag.ENET_PACKET_FLAG_RELIABLE,

        /// <summary>
        ///     packet will not be sequenced with other packets
        /// </summary>
        Unsequenced = ENetPacketFlag.ENET_PACKET_FLAG_UNSEQUENCED,

        /// <summary>
        ///     packet will not allocate data, and user must supply it instead
        /// </summary>
        NoAllocate = ENetPacketFlag.ENET_PACKET_FLAG_NO_ALLOCATE,

        /// <summary>
        ///     packet will be fragmented using unreliable (instead of reliable) sends
        ///     if it exceeds the MTU
        /// </summary>
        UnreliableFragment = ENetPacketFlag.ENET_PACKET_FLAG_UNRELIABLE_FRAGMENT,

        /// <summary>
        ///     whether the packet has been sent from all queues it has been entered into
        /// </summary>
        Sent = ENetPacketFlag.ENET_PACKET_FLAG_SENT
    }
}