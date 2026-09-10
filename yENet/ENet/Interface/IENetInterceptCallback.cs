#if NET7_0_OR_GREATER
using enet;

// ReSharper disable ALL

namespace Enet
{
    /// <summary>
    ///     Intercepts received raw UDP packets.
    /// </summary>
    public unsafe interface IENetInterceptCallback
    {
        /// <summary>
        ///     Intercepts a received raw UDP packet.
        /// </summary>
        /// <param name="host">The host that received the packet.</param>
        /// <param name="event">The event describing the received packet.</param>
        /// <returns>1 to intercept, 0 to ignore, or -1 to propagate an error.</returns>
        static abstract int Intercept(ENetHost* host, ENetEvent* @event);
    }
}
#endif