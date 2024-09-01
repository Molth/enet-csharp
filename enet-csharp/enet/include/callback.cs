// ReSharper disable ALL

namespace enet
{
    public static unsafe partial class ENet
    {
        public struct ENetCallbacks
        {
            public delegate* managed<nint, void*> malloc;
            public delegate* managed<void*, void> free;

            public ENetCallbacks(delegate*<nint, void*> malloc, delegate*<void*, void> free)
            {
                this.malloc = malloc;
                this.free = free;
            }
        }
    }
}