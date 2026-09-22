#if NET10_0_OR_GREATER
using System.Runtime.CompilerServices;
using enet;

// ReSharper disable ALL

namespace Enet
{
    /// <summary>
    ///     Provides a static factory method for wiring an <see cref="ENetCallbacks" /> to a
    ///     callbacks implementation supplied as a type parameter.
    /// </summary>
    /// <remarks>
    ///     The callback fields of an <see cref="ENetCallbacks" /> are function pointers, so
    ///     implementations are provided as static abstract members of an
    ///     <see cref="IENetCallbacks" /> type. This factory binds all three callbacks
    ///     (malloc, free and no_memory) in a single call.
    /// </remarks>
    public static unsafe class ENetCallbacksExtensions
    {
        extension(ENetCallbacks)
        {
            /// <summary>
            ///     Creates an <see cref="ENetCallbacks" /> whose malloc, free and no_memory function
            ///     pointers are bound to the static abstract operations of <typeparamref name="T" />.
            /// </summary>
            /// <typeparam name="T">
            ///     The <see cref="IENetCallbacks" /> implementation whose operations are bound as callbacks.
            /// </typeparam>
            /// <returns>An <see cref="ENetCallbacks" /> with all function pointers populated.</returns>
            public static ENetCallbacks From<T>() where T : IENetCallbacks
            {
                Unsafe.SkipInit(out ENetCallbacks result);
                result.malloc = &T.Malloc;
                result.free = &T.Free;
                result.no_memory = &T.NoMemory;
                return result;
            }
        }
    }
}
#endif