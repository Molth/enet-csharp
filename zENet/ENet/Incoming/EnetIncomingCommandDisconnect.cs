// ReSharper disable ALL

namespace ENet
{
    /// <summary>
    ///     The payload of an incoming disconnect event.
    /// </summary>
    public struct EnetIncomingCommandDisconnect
    {
        /// <summary>
        ///     The user data attached to the disconnection, or zero when none was supplied.
        /// </summary>
        public uint EventData;
    }
}