#if NET7_0_OR_GREATER
using enet;

// ReSharper disable ALL

namespace Enet
{
    /// <summary>
    ///     Computes the checksum of the data held in buffers[0:bufferCount-1].
    /// </summary>
    public unsafe interface IENetChecksumCallback
    {
        /// <summary>
        ///     Computes the checksum of the given buffers and returns it.
        /// </summary>
        /// <param name="buffers">The buffers to checksum.</param>
        /// <param name="bufferCount">The number of buffers.</param>
        /// <returns>The computed checksum.</returns>
        static abstract uint Checksum(ENetBuffer* buffers, nuint bufferCount);
    }
}
#endif