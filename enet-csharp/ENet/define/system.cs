﻿using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using size_t = nuint;
using enet_uint8 = byte;
#if !NET6_0_OR_GREATER
using ssize_t = nint;
#endif

#pragma warning disable CS1591

// ReSharper disable ALL

namespace enet
{
    public static unsafe partial class ENet
    {
        public static void* malloc(size_t size)
        {
#if NET6_0_OR_GREATER
            return NativeMemory.Alloc((nuint)size);
#else
            return (void*)Marshal.AllocHGlobal((ssize_t)size);
#endif
        }

        public static void free(void* memory)
        {
#if NET6_0_OR_GREATER
            NativeMemory.Free(memory);
#else
            Marshal.FreeHGlobal((ssize_t)memory);
#endif
        }

        public static void memcpy(void* dst, void* src, size_t size) => Unsafe.CopyBlockUnaligned(dst, src, (uint)size);

        public static void memset(void* dst, enet_uint8 val, size_t size) => Unsafe.InitBlockUnaligned(dst, val, (uint)size);

        public static long timeGetTime() => Stopwatch.GetTimestamp() * 1000L / Stopwatch.Frequency;
    }
}