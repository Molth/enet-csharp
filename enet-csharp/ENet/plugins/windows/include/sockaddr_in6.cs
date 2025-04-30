using System.Runtime.InteropServices;

#pragma warning disable CS1591
#pragma warning disable CS8981

// ReSharper disable ALL

namespace winsock
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct sockaddr_in6
    {
        public ushort sin6_family;
        public ushort sin6_port;
        public uint sin6_flowinfo;
        public fixed byte sin6_addr[16];
        public uint sin6_scope_id;
    }
}