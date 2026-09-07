using Enet;

// ReSharper disable ALL

namespace ENet
{
    /// <summary>
    ///     The payload of an incoming receive event.
    /// </summary>
    public struct EnetIncomingCommandReceive
    {
        /// <summary>
        ///     The received packet. The receiver owns the packet and must dispose of it
        ///     when it is no longer needed.
        /// </summary>
        public EnetPacket Packet;
    }
}