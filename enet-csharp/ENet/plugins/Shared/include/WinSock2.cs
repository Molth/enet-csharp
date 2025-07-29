using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

#pragma warning disable CS1591

// ReSharper disable ALL

namespace NativeSockets
{
    public static class WinSock2
    {
        public static int ADDRESS_FAMILY_INTER_NETWORK_V4_MAPPED_V6 { get; } = (BitConverter.IsLittleEndian ? -65536 : 65535);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort HOST_TO_NET_16(ushort host) => BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(host) : host;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint HOST_TO_NET_32(uint host) => BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(host) : host;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort NET_TO_HOST_16(ushort network) => BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(network) : network;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint NET_TO_HOST_32(uint network) => BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(network) : network;
    }
}