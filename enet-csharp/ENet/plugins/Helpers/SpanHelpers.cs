using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace NativeSockets
{
    internal static class SpanHelpers
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Compare(ref byte left, ref byte right, nuint byteCount)
        {
            nuint quotient = byteCount >> 30;
            nuint remainder = byteCount & 1073741823;
            for (nuint i = 0; i < quotient; ++i)
            {
                if (!MemoryMarshal.CreateReadOnlySpan(ref left, 1073741824).SequenceEqual(MemoryMarshal.CreateReadOnlySpan(ref right, 1073741824)))
                    return false;
                left = ref Unsafe.AddByteOffset(ref left, new IntPtr(1073741824));
                right = ref Unsafe.AddByteOffset(ref right, new IntPtr(1073741824));
            }

            return MemoryMarshal.CreateReadOnlySpan(ref left, (int)remainder).SequenceEqual(MemoryMarshal.CreateReadOnlySpan(ref right, (int)remainder));
        }
    }
}