using System;
using System.Buffers.Binary;
using size_t = nint;
using enet_uint16 = ushort;
using enet_uint32 = uint;

// ReSharper disable ALL

namespace enet
{
    public static partial class ENet
    {
        public const long INVALID_SOCKET = ~0;

        public const long ENET_SOCKET_NULL = INVALID_SOCKET;

        public static enet_uint16 ENET_HOST_TO_NET_16(enet_uint16 host) => BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(host) : host;

        public static enet_uint32 ENET_HOST_TO_NET_32(enet_uint32 host) => BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(host) : host;

        public static enet_uint16 ENET_NET_TO_HOST_16(enet_uint16 host) => BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(host) : host;

        public static enet_uint32 ENET_NET_TO_HOST_32(enet_uint32 host) => BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(host) : host;
    }

    public unsafe struct ENetBuffer
    {
        public size_t dataLength;
        public void* data;
    }
}