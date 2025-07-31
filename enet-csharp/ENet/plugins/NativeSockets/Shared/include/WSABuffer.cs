#pragma warning disable CS1591
#pragma warning disable CS8981

// ReSharper disable ALL

namespace NativeSockets
{
    public unsafe struct WSABuffer
    {
        public nuint Length;
        public void* Pointer;
    }
}