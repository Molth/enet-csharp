using System.Runtime.InteropServices;

#pragma warning disable CS1591
#pragma warning disable CS8981

// ReSharper disable ALL

namespace NativeSockets
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct pollfd
    {
        public int fd;
        public short events;
        public short revents;
    }
}