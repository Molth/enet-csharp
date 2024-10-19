using enet_uint32 = uint;

// ReSharper disable ALL

namespace enet
{
    public static partial class ENet
    {
        public const enet_uint32 ENET_TIME_OVERFLOW = 86400000;

        public static bool ENET_TIME_LESS(enet_uint32 a, enet_uint32 b) => ((a) - (b) >= ENET_TIME_OVERFLOW);
        public static bool ENET_TIME_GREATER(enet_uint32 a, enet_uint32 b) => ((b) - (a) >= ENET_TIME_OVERFLOW);
        public static bool ENET_TIME_LESS_EQUAL(enet_uint32 a, enet_uint32 b) => (!ENET_TIME_GREATER(a, b));
        public static bool ENET_TIME_GREATER_EQUAL(enet_uint32 a, enet_uint32 b) => (!ENET_TIME_LESS(a, b));

        public static enet_uint32 ENET_TIME_DIFFERENCE(enet_uint32 a, enet_uint32 b) => ((a) - (b) >= ENET_TIME_OVERFLOW ? (b) - (a) : (a) - (b));
    }
}