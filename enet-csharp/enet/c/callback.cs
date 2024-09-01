#pragma warning disable CA2211

// ReSharper disable ALL

namespace enet
{
    public static unsafe partial class ENet
    {
        public static ENetCallbacks callbacks = new(&malloc, &free);

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

        public static uint enet_linked_version() => ENET_VERSION;

        public static void* enet_malloc(nint size) => callbacks.malloc(size);

        public static void enet_free(void* memory) => callbacks.free(memory);
    }
}