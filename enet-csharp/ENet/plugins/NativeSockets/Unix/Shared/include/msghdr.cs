#pragma warning disable CS1591
#pragma warning disable CS8981

// ReSharper disable ALL

namespace NativeSockets
{
    public unsafe struct msghdr
    {
        public void* msg_name;
        public nuint msg_namelen;
        public iovec* msg_iov;
        public int msg_iovlen;
        public void* msg_control;
        public nuint msg_controllen;
        public int msg_flags;
    }
}