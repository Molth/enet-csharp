using System.Net;
using size_t = nint;
using enet_uint16 = ushort;
using enet_uint32 = uint;

// ReSharper disable RedundantCast

namespace enet
{
    public static partial class ENet
    {
        public const long INVALID_SOCKET = ~0;

        public const long ENET_SOCKET_NULL = INVALID_SOCKET;

        public static enet_uint16 ENET_HOST_TO_NET_16(enet_uint16 value) => (enet_uint16)IPAddress.HostToNetworkOrder((short)value);
        public static enet_uint32 ENET_HOST_TO_NET_32(enet_uint32 value) => (enet_uint32)IPAddress.HostToNetworkOrder((long)value);

        public static enet_uint16 ENET_NET_TO_HOST_16(enet_uint16 value) => (enet_uint16)IPAddress.NetworkToHostOrder((short)value);
        public static enet_uint32 ENET_NET_TO_HOST_32(enet_uint32 value) => (enet_uint32)IPAddress.NetworkToHostOrder((long)value);
    }

    public unsafe struct ENetBuffer
    {
        public size_t dataLength;
        public void* data;
    }
}