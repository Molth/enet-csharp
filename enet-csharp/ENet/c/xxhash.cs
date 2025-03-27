#pragma warning disable CS1591

// ReSharper disable ALL

namespace enet
{
    public static unsafe partial class ENet
    {
        public const uint ENET_XXHASH_SEED = 0;

        public static int enet_xxhash_32(void* buffer, nuint byteCount) => xxhash_32(buffer, byteCount, ENET_XXHASH_SEED);
    }
}