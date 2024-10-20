using enet_uint32 = uint;

// ReSharper disable ALL

namespace enet
{
    public static partial class ENet
    {
        public static enet_uint32 ENET_MAX(enet_uint32 x, enet_uint32 y) => ((x) > (y) ? (x) : (y));
        public static enet_uint32 ENET_MIN(enet_uint32 x, enet_uint32 y) => ((x) < (y) ? (x) : (y));
        public static enet_uint32 ENET_DIFFERENCE(enet_uint32 x, enet_uint32 y) => ((x) < (y) ? (y) - (x) : (x) - (y));
    }
}