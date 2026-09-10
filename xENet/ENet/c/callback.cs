// ReSharper disable ALL

namespace enet
{
    public static unsafe partial class ENet
    {
        /// <summary>
        ///     The global allocation callbacks used by the ENet runtime, initialized to the built-in defaults.
        /// </summary>
#pragma warning disable CA2211 // Non-constant fields should not be visible
        public static ENetCallbacks callbacks = new ENetCallbacks(&malloc, &free, &abort);
#pragma warning restore CA2211 // Non-constant fields should not be visible

        /// <summary>
        ///     Initializes ENet globally and supplies user-overridden callbacks. Must be called prior to using any functions in
        ///     ENet.
        ///     Do not use <see cref="enet_initialize()" /> if you use this variant. Make sure the <see cref="ENetCallbacks" />
        ///     structure
        ///     is zeroed out so that any additional callbacks added in future versions will be properly ignored.
        /// </summary>
        /// <param name="version">
        ///     the constant <see cref="ENet.ENET_VERSION" /> should be supplied so ENet knows which version of
        ///     <see cref="ENetCallbacks" /> struct to use
        /// </param>
        /// <param name="inits">user-overridden callbacks where any NULL callbacks will use ENet's defaults</param>
        /// <returns>0 on success, &lt; 0 on failure</returns>
        public static int enet_initialize_with_callbacks(uint version, ENetCallbacks* inits)
        {
            if (version < ENET_VERSION_CREATE(1, 3, 0))
                return -1;

            if (inits->malloc != null || inits->free != null)
            {
                if (inits->malloc == null || inits->free == null)
                    return -1;

                callbacks.malloc = inits->malloc;
                callbacks.free = inits->free;
            }

            if (inits->no_memory != null)
                callbacks.no_memory = inits->no_memory;

            return enet_initialize();
        }

        /// <summary>
        ///     Gives the linked version of the ENet library.
        /// </summary>
        /// <returns>the version number</returns>
        public static uint enet_linked_version() => ENET_VERSION;

        /// <summary>
        ///     Allocates a block of memory through the configured allocation callbacks, notifying the no-memory handler on
        ///     failure.
        /// </summary>
        /// <param name="size">The number of bytes to allocate.</param>
        /// <returns>A pointer to the allocated block, or <see langword="null" /> if the no-memory handler returns.</returns>
        public static void* enet_malloc(nuint size)
        {
            void* memory = callbacks.malloc(size);

            if (memory == null)
                callbacks.no_memory();

            return memory;
        }

        /// <summary>
        ///     Releases a block of memory through the configured deallocation callback.
        /// </summary>
        /// <param name="memory">The pointer to the block to free.</param>
        public static void enet_free(void* memory) => callbacks.free(memory);
    }
}