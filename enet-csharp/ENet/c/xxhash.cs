using enet_uint32 = uint;

// ReSharper disable ALL

namespace enet
{
    public static unsafe partial class ENet
    {
        public const enet_uint32 ENET_XXHASH_SEED = 0;

        public static int enet_xxhash_32(void* buffer, enet_uint32 byteCount) => xxhash_32(buffer, byteCount, ENET_XXHASH_SEED);
    }
}