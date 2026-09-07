using enet;

// ReSharper disable ALL

namespace ENet
{
    /// <summary>
    ///     The payload of an outgoing ping command.
    /// </summary>
    internal struct EnetOutgoingCommandPing
    {
        /// <summary>
        ///     The destination address to ping.
        /// </summary>
        public ENetAddress Address;
    }
}