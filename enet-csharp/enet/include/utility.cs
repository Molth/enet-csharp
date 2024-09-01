// ReSharper disable ALL

namespace enet
{
    public static partial class ENet
    {
        public static uint ENET_MAX(uint x, uint y) => ((x) > (y) ? (x) : (y));
        public static uint ENET_MIN(uint x, uint y) => ((x) < (y) ? (x) : (y));
        public static uint ENET_DIFFERENCE(uint x, uint y) => ((x) < (y) ? (y) - (x) : (x) - (y));
    }
}