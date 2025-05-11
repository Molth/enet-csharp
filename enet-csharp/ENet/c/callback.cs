#pragma warning disable CS1591
#pragma warning disable CA2211

// ReSharper disable ALL

namespace enet
{
    public static unsafe partial class ENet
    {
        public static ENetCallbacks callbacks = new ENetCallbacks(&malloc, &free);

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

            return enet_initialize();
        }

        /// <summary>
        ///     Gives the linked version of the ENet library.
        /// </summary>
        /// <returns>the version number</returns>
        public static uint enet_linked_version() => ENET_VERSION;

        public static void* enet_malloc(nuint size) => callbacks.malloc(size);

        public static void enet_free(void* memory) => callbacks.free(memory);
    }
}