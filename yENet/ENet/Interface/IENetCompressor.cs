#if NET7_0_OR_GREATER
using enet;

// ReSharper disable ALL

namespace Enet
{
    /// <summary>
    ///     Defines the operations of an ENet packet compressor as static abstract members.
    ///     Implementations are supplied as type parameters to provide a compressor strategy.
    /// </summary>
    public unsafe interface IENetCompressor
    {
        /// <summary>
        ///     Compresses from inBuffers[0:inBufferCount-1], containing inLimit bytes, to outData,
        ///     outputting at most outLimit bytes. Should return 0 on failure.
        /// </summary>
        /// <param name="context">The compressor context data.</param>
        /// <param name="inBuffers">The input buffers holding the data to compress.</param>
        /// <param name="inBufferCount">The number of input buffers.</param>
        /// <param name="inLimit">The maximum number of input bytes to consume.</param>
        /// <param name="outData">The output buffer receiving the compressed data.</param>
        /// <param name="outLimit">The maximum number of output bytes available.</param>
        /// <returns>The number of compressed bytes written, or 0 on failure.</returns>
        static abstract nuint Compress(void* context, ENetBuffer* inBuffers, nuint inBufferCount, nuint inLimit, byte* outData, nuint outLimit);

        /// <summary>
        ///     Decompresses from inData, containing inLimit bytes, to outData,
        ///     outputting at most outLimit bytes. Should return 0 on failure.
        /// </summary>
        /// <param name="context">The compressor context data.</param>
        /// <param name="inData">The input buffer holding the compressed data.</param>
        /// <param name="inLimit">The number of compressed input bytes available.</param>
        /// <param name="outData">The output buffer receiving the decompressed data.</param>
        /// <param name="outLimit">The maximum number of output bytes available.</param>
        /// <returns>The number of decompressed bytes written, or 0 on failure.</returns>
        static abstract nuint Decompress(void* context, byte* inData, nuint inLimit, byte* outData, nuint outLimit);

        /// <summary>
        ///     Destroys the context when compression is disabled or the host is destroyed. Maybe a no-op.
        /// </summary>
        /// <param name="context">The compressor context data to destroy.</param>
        static abstract void Destroy(void* context);
    }
}
#endif