using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CS1591
#pragma warning disable CS8981

// ReSharper disable ALL

namespace NativeSockets
{
    [StructLayout(LayoutKind.Explicit, Size = 2)]
    public struct sa_family_t
    {
        [FieldOffset(0)] public byte bsd_len;
        [FieldOffset(1)] public byte bsd_family;
        [FieldOffset(0)] public ushort family;

        public bool IsIPv4
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => SocketPal.IsBSD ? bsd_family == (int)AddressFamily.InterNetwork : family == (int)AddressFamily.InterNetwork;
        }

        public bool IsIPv6
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => SocketPal.IsBSD ? bsd_family == BSDSocketPal.ADDRESS_FAMILY_INTER_NETWORK_V6 : family == SocketPal.ADDRESS_FAMILY_INTER_NETWORK_V6;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sa_family_t FromBSD(ushort value)
        {
            Unsafe.SkipInit(out sa_family_t result);
            result.bsd_len = value == BSDSocketPal.ADDRESS_FAMILY_INTER_NETWORK_V6 ? (byte)28 : (byte)16;
            result.bsd_family = (byte)value;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator sa_family_t(ushort value)
        {
            Unsafe.SkipInit(out sa_family_t result);
            if (SocketPal.IsBSD)
            {
                result.bsd_len = value == BSDSocketPal.ADDRESS_FAMILY_INTER_NETWORK_V6 ? (byte)28 : (byte)16;
                result.bsd_family = (byte)value;
                return result;
            }

            result.family = value;
            return result;
        }
    }
}