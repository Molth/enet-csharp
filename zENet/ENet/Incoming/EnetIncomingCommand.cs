using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace ThreadedEnet
{
    /// <summary>
    ///     A discriminated union of the command payloads carried by incoming (host-to-user) events.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    internal struct EnetIncomingCommand
    {
        /// <summary>
        ///     The payload of an incoming connect event.
        /// </summary>
        [FieldOffset(0)] public EnetIncomingCommandConnect Connect;

        /// <summary>
        ///     The payload of an incoming disconnect event.
        /// </summary>
        [FieldOffset(0)] public EnetIncomingCommandDisconnect Disconnect;

        /// <summary>
        ///     The payload of an incoming receive event.
        /// </summary>
        [FieldOffset(0)] public EnetIncomingCommandReceive Receive;
    }
}