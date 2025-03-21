using System.Runtime.CompilerServices;
using size_t = nuint;
using ssize_t = nint;
using enet_uint8 = byte;
using enet_uint32 = uint;

#pragma warning disable CS1591

// ReSharper disable ALL

namespace enet
{
    public static unsafe partial class ENet
    {
        public static int xxhash_32(void* buffer, size_t byteCount, enet_uint32 seed)
        {
            enet_uint32 num1 = 0;
            enet_uint32 num2 = 0;
            enet_uint32 num3 = 0;
            enet_uint32 num4 = 0;
            enet_uint32 num5 = 0;
            enet_uint32 num6 = 0;
            enet_uint32 num7 = 0;
            enet_uint32 num8 = 0;
            ref enet_uint8 local1 = ref *(enet_uint8*)buffer;
#if NET6_0_OR_GREATER
            ref enet_uint8 local2 = ref Unsafe.Add(ref local1, byteCount);
#else
            ref enet_uint8 local2 = ref Unsafe.Add(ref local1, (ssize_t)byteCount);
#endif
            if (byteCount >= 16)
            {
                num1 = (enet_uint32)((int)seed - 1640531535 - 2048144777);
                num2 = seed + 2246822519U;
                num3 = seed;
                num4 = seed - 2654435761U;
                const ssize_t elementOffset1 = 16;
                for (ref enet_uint8 local3 = ref Unsafe.Subtract(ref local2, Unsafe.ByteOffset(ref local1, ref local2) % elementOffset1); Unsafe.IsAddressLessThan(ref local1, ref local3); local1 = ref Unsafe.Add(ref local1, elementOffset1))
                {
                    enet_uint32 num9 = num1 + Unsafe.ReadUnaligned<enet_uint32>(ref local1) * 2246822519U;
                    num1 = (enet_uint32)((((int)num9 << 13) | (int)(num9 >> 19)) * -1640531535);
                    enet_uint32 num10 = num2 + Unsafe.ReadUnaligned<enet_uint32>(ref Unsafe.Add(ref local1, 4)) * 2246822519U;
                    num2 = (enet_uint32)((((int)num10 << 13) | (int)(num10 >> 19)) * -1640531535);
                    enet_uint32 num11 = num3 + Unsafe.ReadUnaligned<enet_uint32>(ref Unsafe.Add(ref local1, 8)) * 2246822519U;
                    num3 = (enet_uint32)((((int)num11 << 13) | (int)(num11 >> 19)) * -1640531535);
                    enet_uint32 num12 = num4 + Unsafe.ReadUnaligned<enet_uint32>(ref Unsafe.Add(ref local1, 12)) * 2246822519U;
                    num4 = (enet_uint32)((((int)num12 << 13) | (int)(num12 >> 19)) * -1640531535);
                    num8 += 4U;
                }
            }

            const ssize_t elementOffset2 = 4;
            for (; Unsafe.ByteOffset(ref local1, ref local2) >= elementOffset2; local1 = ref Unsafe.Add(ref local1, elementOffset2))
            {
                enet_uint32 num13 = (enet_uint32)Unsafe.ReadUnaligned<int>(ref local1);
                enet_uint32 num14 = num8++;
                switch (num14 % 4U)
                {
                    case 0:
                        num5 = num13;
                        break;
                    case 1:
                        num6 = num13;
                        break;
                    case 2:
                        num7 = num13;
                        break;
                    default:
                        if (num14 == 3U)
                        {
                            num1 = (enet_uint32)((int)seed - 1640531535 - 2048144777);
                            num2 = seed + 2246822519U;
                            num3 = seed;
                            num4 = seed - 2654435761U;
                        }

                        enet_uint32 num15 = num1 + num5 * 2246822519U;
                        num1 = (enet_uint32)((((int)num15 << 13) | (int)(num15 >> 19)) * -1640531535);
                        enet_uint32 num16 = num2 + num6 * 2246822519U;
                        num2 = (enet_uint32)((((int)num16 << 13) | (int)(num16 >> 19)) * -1640531535);
                        enet_uint32 num17 = num3 + num7 * 2246822519U;
                        num3 = (enet_uint32)((((int)num17 << 13) | (int)(num17 >> 19)) * -1640531535);
                        enet_uint32 num18 = num4 + num13 * 2246822519U;
                        num4 = (enet_uint32)((((int)num18 << 13) | (int)(num18 >> 19)) * -1640531535);
                        break;
                }
            }

            for (; Unsafe.IsAddressLessThan(ref local1, ref local2); local1 = ref Unsafe.Add(ref local1, 1))
            {
                enet_uint32 num19 = local1;
                enet_uint32 num20 = num8++;
                switch (num20 % 4U)
                {
                    case 0:
                        num5 = num19;
                        break;
                    case 1:
                        num6 = num19;
                        break;
                    case 2:
                        num7 = num19;
                        break;
                    default:
                        if (num20 == 3U)
                        {
                            num1 = (enet_uint32)((int)seed - 1640531535 - 2048144777);
                            num2 = seed + 2246822519U;
                            num3 = seed;
                            num4 = seed - 2654435761U;
                        }

                        enet_uint32 num21 = num1 + num5 * 2246822519U;
                        num1 = (enet_uint32)((((int)num21 << 13) | (int)(num21 >> 19)) * -1640531535);
                        enet_uint32 num22 = num2 + num6 * 2246822519U;
                        num2 = (enet_uint32)((((int)num22 << 13) | (int)(num22 >> 19)) * -1640531535);
                        enet_uint32 num23 = num3 + num7 * 2246822519U;
                        num3 = (enet_uint32)((((int)num23 << 13) | (int)(num23 >> 19)) * -1640531535);
                        enet_uint32 num24 = num4 + num19 * 2246822519U;
                        num4 = (enet_uint32)((((int)num24 << 13) | (int)(num24 >> 19)) * -1640531535);
                        break;
                }
            }

            enet_uint32 num25 = num8;
            enet_uint32 num26 = num25 % 4U;
            enet_uint32 num27 = (enet_uint32)((num25 < 4U ? (int)seed + 374761393 : (((int)num1 << 1) | (int)(num1 >> 31)) + (((int)num2 << 7) | (int)(num2 >> 25)) + (((int)num3 << 12) | (int)(num3 >> 20)) + (((int)num4 << 18) | (int)(num4 >> 14))) + (int)num25 * 4);
            if (num26 > 0U)
            {
                enet_uint32 num28 = num27 + num5 * 3266489917U;
                num27 = (enet_uint32)((((int)num28 << 17) | (int)(num28 >> 15)) * 668265263);
                if (num26 > 1U)
                {
                    enet_uint32 num29 = num27 + num6 * 3266489917U;
                    num27 = (enet_uint32)((((int)num29 << 17) | (int)(num29 >> 15)) * 668265263);
                    if (num26 > 2U)
                    {
                        enet_uint32 num30 = num27 + num7 * 3266489917U;
                        num27 = (enet_uint32)((((int)num30 << 17) | (int)(num30 >> 15)) * 668265263);
                    }
                }
            }

            int num31 = (int)num27;
#if NET7_0_OR_GREATER
            int num32 = (num31 ^ (num31 >>> 15)) * -2048144777;
            int num33 = (num32 ^ (num32 >>> 13)) * -1028477379;
            return num33 ^ (num33 >>> 16);
#else
            int num32 = (num31 ^ (int)((enet_uint32)num31 >> 15)) * -2048144777;
            int num33 = (num32 ^ (int)((enet_uint32)num32 >> 13)) * -1028477379;
            return num33 ^ (int)((enet_uint32)num33 >> 16);
#endif
        }
    }
}