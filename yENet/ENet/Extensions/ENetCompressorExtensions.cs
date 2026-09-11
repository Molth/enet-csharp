#if NET7_0_OR_GREATER
using enet;

// ReSharper disable ALL

namespace Enet
{
    /// <summary>
    ///     Provides extension methods for wiring an <see cref="ENetCompressor" /> to a
    ///     compressor implementation supplied as a type parameter.
    /// </summary>
    /// <remarks>
    ///     The callback fields of an <see cref="ENetCompressor" /> are function pointers, so
    ///     implementations are provided as static abstract members of an
    ///     <see cref="IENetCompressor" /> type. This extension binds all three callbacks
    ///     (compress, decompress and destroy) of an instance in a single call.
    /// </remarks>
    public static unsafe class ENetCompressorExtensions
    {
        /// <summary>
        ///     Binds the compressor context and callback function pointers of this
        ///     <see cref="ENetCompressor" /> to the static abstract operations of <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">
        ///     The <see cref="IENetCompressor" /> implementation whose operations are bound as callbacks.
        /// </typeparam>
        /// <param name="compressor">The compressor whose context and callbacks are populated.</param>
        /// <param name="context">The context data passed to each callback; Must be non-NULL.</param>
        public static void From<T>(ref this ENetCompressor compressor, void* context) where T : IENetCompressor
        {
            compressor.context = context;
            compressor.compress = &T.Compress;
            compressor.decompress = &T.Decompress;
            compressor.destroy = &T.Destroy;
        }
    }
}
#endif