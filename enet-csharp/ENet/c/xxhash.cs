using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

#pragma warning disable CA2211
#pragma warning disable CS1591

// ReSharper disable ALL

namespace enet
{
    public static unsafe partial class ENet
    {
        public static uint ENET_XXHASH_32_SEED = enet_xxhash_32_generate_seed();

        public static uint enet_xxhash_32_generate_seed()
        {
            uint seed;

            Span<byte> data = MemoryMarshal.CreateSpan(ref *(byte*)&seed, 4);
            do
            {
                RandomNumberGenerator.Fill(data);
            } while (seed == 0);

            return seed;
        }

        public static void enet_xxhash_32_set_seed(uint newSeed) => ENET_XXHASH_32_SEED = newSeed;

        public static int enet_xxhash_32(void* buffer, nuint byteCount) => xxhash_32(buffer, byteCount, ENET_XXHASH_32_SEED);
    }
}