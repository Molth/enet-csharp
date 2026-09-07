using enet;

// ReSharper disable ALL

namespace ThreadedEnet
{
    /// <summary>
    ///     The payload of an outgoing connect command.
    /// </summary>
    internal struct EnetOutgoingCommandConnect
    {
        /// <summary>
        ///     The remote address to connect to.
        /// </summary>
        public ENetAddress Address;

        /// <summary>
        ///     The number of channels to allocate for the connection.
        /// </summary>
        public nuint ChannelCount;

        /// <summary>
        ///     The user data supplied to the receiving host.
        /// </summary>
        public uint EventData;
    }
}