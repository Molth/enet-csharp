using System.Runtime.InteropServices;

#pragma warning disable CS1591
#pragma warning disable CS8981

// ReSharper disable ALL

namespace NativeSockets
{
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    public unsafe struct sockaddr
    {
        [FieldOffset(0)] public sa_family_t sa_family;
        [FieldOffset(2)] public fixed byte sa_data[14];
    }
}