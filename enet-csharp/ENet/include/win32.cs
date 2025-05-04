using System;
using System.Buffers.Binary;

#pragma warning disable CS1591

// ReSharper disable ALL

namespace enet
{
    public static partial class ENet
    {
        public const nint INVALID_SOCKET = ~0;

        public const nint ENET_SOCKET_NULL = INVALID_SOCKET;

        public static ushort ENET_HOST_TO_NET_16(ushort host) => BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(host) : host;

        public static uint ENET_HOST_TO_NET_32(uint host) => BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(host) : host;

        public static ushort ENET_NET_TO_HOST_16(ushort network) => BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(network) : network;

        public static uint ENET_NET_TO_HOST_32(uint network) => BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(network) : network;
    }

    public unsafe struct ENetBuffer
    {
        public nuint dataLength;
        public void* data;
    }
}