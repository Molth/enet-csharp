// ReSharper disable ALL

namespace enet
{
    /// <summary>
    ///     Custom allocation callbacks used by the ENet runtime in place of the default allocator.
    /// </summary>
    public unsafe struct ENetCallbacks
    {
        /// <summary>
        ///     Function pointer invoked to allocate a block of the given size in bytes.
        /// </summary>
        public delegate* managed<nuint, void*> malloc;

        /// <summary>
        ///     Function pointer invoked to release a previously allocated block.
        /// </summary>
        public delegate* managed<void*, void> free;

        /// <summary>
        ///     Function pointer invoked when an allocation attempt fails due to insufficient memory.
        /// </summary>
        public delegate* managed<void> no_memory;

        /// <summary>
        ///     Initializes the callback set with the specified allocation functions.
        /// </summary>
        /// <param name="malloc">The allocation function.</param>
        /// <param name="free">The deallocation function.</param>
        /// <param name="no_memory">The out-of-memory notification function.</param>
        public ENetCallbacks(delegate* managed<nuint, void*> malloc, delegate* managed<void*, void> free, delegate* managed<void> no_memory)
        {
            this.malloc = malloc;
            this.free = free;
            this.no_memory = no_memory;
        }
    }
}