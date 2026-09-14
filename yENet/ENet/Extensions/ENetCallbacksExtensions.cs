#if NET7_0_OR_GREATER
using enet;

// ReSharper disable ALL

namespace Enet
{
    /// <summary>
    ///     Provides extension methods for wiring an <see cref="ENetCallbacks" /> to a
    ///     callbacks implementation supplied as a type parameter.
    /// </summary>
    /// <remarks>
    ///     The callback fields of an <see cref="ENetCallbacks" /> are function pointers, so
    ///     implementations are provided as static abstract members of an
    ///     <see cref="IENetCallbacks" /> type. This extension binds all three callbacks
    ///     (malloc, free and no_memory) of an instance in a single call.
    /// </remarks>
    public static unsafe class ENetCallbacksExtensions
    {
        /// <summary>
        ///     Binds the callback function pointers of this <see cref="ENetCallbacks" /> to the
        ///     static abstract operations of <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">
        ///     The <see cref="IENetCallbacks" /> implementation whose operations are bound as callbacks.
        /// </typeparam>
        /// <param name="callbacks">The callbacks whose function pointers are populated.</param>
        public static void From<T>(ref this ENetCallbacks callbacks) where T : IENetCallbacks
        {
            callbacks.malloc = &T.Malloc;
            callbacks.free = &T.Free;
            callbacks.no_memory = &T.NoMemory;
        }
    }
}
#endif