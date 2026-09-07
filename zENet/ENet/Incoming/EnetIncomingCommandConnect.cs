// ReSharper disable ALL

namespace ENet
{
    /// <summary>
    ///     The payload of an incoming connect event.
    /// </summary>
    public struct EnetIncomingCommandConnect
    {
        /// <summary>
        ///     The number of channels allocated by the connecting peer.
        /// </summary>
        public nuint ChannelCount;

        /// <summary>
        ///     The user data supplied by the remote host when the connection was initiated.
        /// </summary>
        public uint EventData;
    }
}