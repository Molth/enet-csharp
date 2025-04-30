using System.Runtime.InteropServices;

#pragma warning disable CS1591
#pragma warning disable CS8981

// ReSharper disable ALL

namespace winsock
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct sockaddr_in
    {
        public ushort sin_family;
        public ushort sin_port;
        public in_addr sin_addr;
        public fixed byte sin_zero[8];
    }
}