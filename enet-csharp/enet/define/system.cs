using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace enet
{
    public static unsafe partial class ENet
    {
        public static void* malloc(nint size)
        {
#if NET6_0_OR_GREATER
            return NativeMemory.Alloc((nuint)size);
#else
            return (void*)Marshal.AllocHGlobal(size);
#endif
        }

        public static void free(void* memory)
        {
#if NET6_0_OR_GREATER
            NativeMemory.Free(memory);
#else
            Marshal.FreeHGlobal((nint)memory);
#endif
        }

        public static void memcpy(void* dst, void* src, nint size) => Unsafe.CopyBlock(dst, src, (uint)size);

        public static void memset(void* dst, byte val, nint size) => Unsafe.InitBlock(dst, val, (uint)size);

        public static long timeGetTime() => Stopwatch.GetTimestamp() * 1000L / Stopwatch.Frequency;
    }
}