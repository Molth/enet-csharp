#if NET10_0_OR_GREATER
using System.Runtime.CompilerServices;
using enet;

// ReSharper disable ALL

namespace Enet
{
    /// <summary>
    ///     Provides a static factory method for wiring an <see cref="ENetCompressor" /> to a
    ///     compressor implementation supplied as a type parameter.
    /// </summary>
    /// <remarks>
    ///     The callback fields of an <see cref="ENetCompressor" /> are function pointers, so
    ///     implementations are provided as static abstract members of an
    ///     <see cref="IENetCompressor" /> type. This factory binds all three callbacks
    ///     (compress, decompress and destroy) in a single call.
    /// </remarks>
    public static unsafe class ENetCompressorExtensions
    {
        extension(ENetCompressor)
        {
            /// <summary>
            ///     Creates an <see cref="ENetCompressor" /> whose context and compress, decompress and
            ///     destroy function pointers are bound to the static abstract operations of
            ///     <typeparamref name="T" />.
            /// </summary>
            /// <param name="context">The context data passed to each callback; must be non-NULL.</param>
            /// <typeparam name="T">
            ///     The <see cref="IENetCompressor" /> implementation whose operations are bound as callbacks.
            /// </typeparam>
            /// <returns>An <see cref="ENetCompressor" /> with context and all function pointers populated.</returns>
            public static ENetCompressor From<T>(void* context) where T : IENetCompressor
            {
                Unsafe.SkipInit(out ENetCompressor result);
                result.context = context;
                result.compress = &T.Compress;
                result.decompress = &T.Decompress;
                result.destroy = &T.Destroy;
                return result;
            }
        }
    }
}
#endif