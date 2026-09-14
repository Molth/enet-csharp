#if NET7_0_OR_GREATER

// ReSharper disable ALL

namespace Enet
{
    /// <summary>
    ///     Defines the custom allocation callbacks used by the ENet runtime as static abstract members.
    ///     Implementations are supplied as type parameters to provide an allocator strategy.
    /// </summary>
    public unsafe interface IENetCallbacks
    {
        /// <summary>
        ///     Allocates a block of the given size in bytes.
        /// </summary>
        /// <param name="byteCount">The size of the block to allocate, in bytes.</param>
        /// <returns>A pointer to the newly allocated block.</returns>
        static abstract void* Malloc(nuint byteCount);

        /// <summary>
        ///     Releases a previously allocated block.
        /// </summary>
        /// <param name="ptr">A pointer to the block to release.</param>
        static abstract void Free(void* ptr);

        /// <summary>
        ///     Invoked when an allocation attempt fails due to insufficient memory.
        /// </summary>
        static abstract void NoMemory();
    }
}
#endif