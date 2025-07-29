using System.Runtime.InteropServices;

#pragma warning disable CS1591
#pragma warning disable CS8981

// ReSharper disable ALL

namespace NativeSockets
{
    [StructLayout(LayoutKind.Explicit, Size = 128)]
    public struct sockaddr_storage
    {
        [FieldOffset(0)] public sa_family_t ss_family;
        [FieldOffset(8)] public long __ss_align;
    }
}