using System.Runtime.CompilerServices;
using enet;

// ReSharper disable ALL

namespace Enet
{
    /// <summary>
    ///     An ENet event as returned by enet_host_service().
    /// </summary>
    public readonly unsafe struct EnetEvent
    {
        /// <summary>
        ///     Gets the handle to the underlying object.
        /// </summary>
        private readonly ENetEvent _handle;

        /// <summary>
        ///     Initializes a new instance of this class.
        /// </summary>
        public EnetEvent(ENetEvent handle) => _handle = handle;

        /// <summary>
        ///     Gets the handle to the underlying object.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ENetEvent GetInner() => _handle;

        /// <summary>
        ///     type of the event
        /// </summary>
        public EnetEventType Type => (EnetEventType)_handle.type;

        /// <summary>
        ///     peer that generated a connect, disconnect or receive event
        /// </summary>
        public EnetPeer Peer => new(_handle.peer);

        /// <summary>
        ///     channel on the peer that generated the event, if appropriate
        /// </summary>
        public byte ChannelId => _handle.channelID;

        /// <summary>
        ///     data associated with the event, if appropriate
        /// </summary>
        public uint Data => _handle.data;

        /// <summary>
        ///     packet associated with the event, if appropriate
        /// </summary>
        public EnetPacket Packet => new(_handle.packet);
    }
}