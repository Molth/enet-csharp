using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

#pragma warning disable CS1591

// ReSharper disable ALL

namespace NativeSockets
{
    internal static class XxHash
    {
        public static uint XXHASH_32_SEED { get; }

        static XxHash()
        {
            Span<byte> buffer = stackalloc byte[4];
            RandomNumberGenerator.Fill(buffer);
            XXHASH_32_SEED = Unsafe.ReadUnaligned<uint>(ref MemoryMarshal.GetReference(buffer));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Hash32<T>(in T obj) where T : unmanaged => Hash32(MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<T, byte>(ref Unsafe.AsRef(in obj)), Unsafe.SizeOf<T>()), XXHASH_32_SEED);

        public static int Hash32(ReadOnlySpan<byte> buffer, uint seed)
        {
            int length = buffer.Length;
            ref byte local1 = ref MemoryMarshal.GetReference(buffer);
            uint num1;
            if (buffer.Length >= 16)
            {
                uint num2 = seed + 606290984U;
                uint num3 = seed + 2246822519U;
                uint num4 = seed;
                uint num5 = seed - 2654435761U;
                for (; length >= 16; length -= 16)
                {
                    ref byte local2 = ref Unsafe.AddByteOffset(ref local1, new IntPtr(buffer.Length - length));
                    uint num6 = num2 + Unsafe.ReadUnaligned<uint>(ref local2) * 2246822519U;
                    num2 = (uint)((((int)num6 << 13) | (int)(num6 >> 19)) * -1640531535);
                    uint num7 = num3 + Unsafe.ReadUnaligned<uint>(ref Unsafe.AddByteOffset(ref local2, new IntPtr(4))) * 2246822519U;
                    num3 = (uint)((((int)num7 << 13) | (int)(num7 >> 19)) * -1640531535);
                    uint num8 = num4 + Unsafe.ReadUnaligned<uint>(ref Unsafe.AddByteOffset(ref local2, new IntPtr(8))) * 2246822519U;
                    num4 = (uint)((((int)num8 << 13) | (int)(num8 >> 19)) * -1640531535);
                    uint num9 = num5 + Unsafe.ReadUnaligned<uint>(ref Unsafe.AddByteOffset(ref local2, new IntPtr(12))) * 2246822519U;
                    num5 = (uint)((((int)num9 << 13) | (int)(num9 >> 19)) * -1640531535);
                }

                num1 = (uint)((((int)num2 << 1) | (int)(num2 >> 31)) + (((int)num3 << 7) | (int)(num3 >> 25)) + (((int)num4 << 12) | (int)(num4 >> 20)) + (((int)num5 << 18) | (int)(num5 >> 14)) + buffer.Length);
            }
            else
                num1 = (uint)((int)seed + 374761393 + buffer.Length);

            for (; length >= 4; length -= 4)
            {
                uint num10 = Unsafe.ReadUnaligned<uint>(ref Unsafe.AddByteOffset(ref local1, new IntPtr(buffer.Length - length)));
                uint num11 = num1 + num10 * 3266489917U;
                num1 = (uint)((((int)num11 << 17) | (int)(num11 >> 15)) * 668265263);
            }

            ref byte local3 = ref Unsafe.AddByteOffset(ref local1, new IntPtr(buffer.Length - length));
            for (int index = 0; index < length; ++index)
            {
                uint num12 = Unsafe.AddByteOffset(ref local3, new IntPtr(index));
                uint num13 = num1 + num12 * 374761393U;
                num1 = (uint)((((int)num13 << 11) | (int)(num13 >> 21)) * -1640531535);
            }

#if NET7_0_OR_GREATER
            int num14 = ((int)num1 ^ (int)(num1 >> 15)) * -2048144777;
            int num15 = (num14 ^ (num14 >>> 13)) * -1028477379;
            return num15 ^ (num15 >>> 16);
#else
            int num14 = ((int)num1 ^ (int)(num1 >> 15)) * -2048144777;
            int num15 = (num14 ^ (int)((uint)num14 >> 13)) * -1028477379;
            return num15 ^ (int)((uint)num15 >> 16);
#endif
        }
    }
}