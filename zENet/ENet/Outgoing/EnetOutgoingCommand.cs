using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace ENet
{
    /// <summary>
    ///     A discriminated union of the command payloads carried by outgoing (user-to-host) events.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    internal struct EnetOutgoingCommand
    {
        /// <summary>
        ///     The payload of an outgoing connect command.
        /// </summary>
        [FieldOffset(0)] public EnetOutgoingCommandConnect Connect;

        /// <summary>
        ///     The payload of an outgoing disconnect command.
        /// </summary>
        [FieldOffset(0)] public EnetOutgoingCommandDisconnect Disconnect;

        /// <summary>
        ///     The payload of an outgoing send command.
        /// </summary>
        [FieldOffset(0)] public EnetOutgoingCommandSend Send;

        /// <summary>
        ///     The payload of an outgoing broadcast command.
        /// </summary>
        [FieldOffset(0)] public EnetOutgoingCommandBroadcast Broadcast;

        /// <summary>
        ///     The payload of an outgoing ping command.
        /// </summary>
        [FieldOffset(0)] public EnetOutgoingCommandPing Ping;

        /// <summary>
        ///     The payload of an outgoing set ping interval command.
        /// </summary>
        [FieldOffset(0)] public EnetOutgoingCommandSetPingInterval SetPingInterval;

        /// <summary>
        ///     The payload of an outgoing set timeout command.
        /// </summary>
        [FieldOffset(0)] public EnetOutgoingCommandSetTimeout SetTimeout;

        /// <summary>
        ///     The payload of an outgoing configure throttle command.
        /// </summary>
        [FieldOffset(0)] public EnetOutgoingCommandConfigureThrottle ConfigureThrottle;
    }
}