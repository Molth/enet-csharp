// ReSharper disable ALL

namespace enet
{
    public static partial class ENet
    {
        public const uint ENET_TIME_OVERFLOW = 86400000;

        public static bool ENET_TIME_LESS(uint a, uint b) => ((a) - (b) >= ENET_TIME_OVERFLOW);
        public static bool ENET_TIME_GREATER(uint a, uint b) => ((b) - (a) >= ENET_TIME_OVERFLOW);
        public static bool ENET_TIME_LESS_EQUAL(uint a, uint b) => (!ENET_TIME_GREATER(a, b));
        public static bool ENET_TIME_GREATER_EQUAL(uint a, uint b) => (!ENET_TIME_LESS(a, b));

        public static uint ENET_TIME_DIFFERENCE(uint a, uint b) => ((a) - (b) >= ENET_TIME_OVERFLOW ? (b) - (a) : (a) - (b));
    }
}