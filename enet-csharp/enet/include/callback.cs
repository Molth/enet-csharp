using size_t = nint;

// ReSharper disable ALL

namespace enet
{
    public static unsafe partial class ENet
    {
        public struct ENetCallbacks
        {
            public delegate* managed<size_t, void*> malloc;
            public delegate* managed<void*, void> free;

            public ENetCallbacks(delegate*<size_t, void*> malloc, delegate*<void*, void> free)
            {
                this.malloc = malloc;
                this.free = free;
            }
        }
    }
}