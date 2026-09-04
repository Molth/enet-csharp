#pragma warning disable CS1591

// ReSharper disable ALL

namespace enet
{
    public unsafe struct ENetCallbacks
    {
        public delegate* managed<nuint, void*> malloc;
        public delegate* managed<void*, void> free;
        public delegate* managed<void> no_memory;

        public ENetCallbacks(delegate* managed<nuint, void*> malloc, delegate* managed<void*, void> free, delegate* managed<void> no_memory)
        {
            this.malloc = malloc;
            this.free = free;
            this.no_memory = no_memory;
        }
    }
}