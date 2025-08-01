#pragma warning disable CS1591
#pragma warning disable CS8981

// ReSharper disable ALL

namespace NativeSockets
{
    public unsafe struct iovec
    {
        public void* iov_base;
        public nuint iov_len;
    }
}