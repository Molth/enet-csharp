using System;
using System.Buffers.Binary;

// ReSharper disable ALL

namespace enet
{
    public static partial class ENet
    {
        public const long INVALID_SOCKET = ~0;

        public const long ENET_SOCKET_NULL = INVALID_SOCKET;

        public static ushort ENET_HOST_TO_NET_16(ushort host) => BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(host) : host;

        public static uint ENET_HOST_TO_NET_32(uint host) => BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(host) : host;

        public static ushort ENET_NET_TO_HOST_16(ushort host) => BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(host) : host;

        public static uint ENET_NET_TO_HOST_32(uint host) => BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(host) : host;
    }

    public unsafe struct ENetBuffer
    {
        public nint dataLength;
        public void* data;
    }
}