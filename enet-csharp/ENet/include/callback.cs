#pragma warning disable CS1591

// ReSharper disable ALL

namespace enet
{
    public unsafe struct ENetCallbacks
    {
        public delegate* managed<nuint, void*> malloc;
        public delegate* managed<void*, void> free;

        public ENetCallbacks(delegate* managed<nuint, void*> malloc, delegate* managed<void*, void> free)
        {
            this.malloc = malloc;
            this.free = free;
        }
    }
}