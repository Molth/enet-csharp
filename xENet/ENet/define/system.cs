using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace enet
{
    public static unsafe partial class ENet
    {
        /// <summary>
        ///     Allocates a block of memory of the specified size, in bytes.
        /// </summary>
        /// <param name="byteCount">The size, in bytes, of the block to allocate.</param>
        /// <returns>A pointer to the allocated block of memory.</returns>
        /// <remarks>
        ///     <para>
        ///         This method allows <paramref name="byteCount" /> to be <c>0</c> and will return a valid pointer that should
        ///         not be dereferenced and that should be passed to free to avoid memory leaks.
        ///     </para>
        ///     <para>This method is a thin wrapper over the C <c>malloc</c> API.</para>
        /// </remarks>
        public static void* malloc(nuint byteCount)
        {
            try
            {
#if NET6_0_OR_GREATER
                return NativeMemory.Alloc(byteCount);
#else
                return (void*)Marshal.AllocHGlobal((nint)byteCount);
#endif
            }
            catch (OutOfMemoryException)
            {
                return null;
            }
        }

        /// <summary>
        ///     Frees a block of memory.
        /// </summary>
        /// <param name="ptr">A pointer to the block of memory that should be freed.</param>
        /// <remarks>
        ///     <para>This method does nothing if <paramref name="ptr" /> is <c>null</c>.</para>
        ///     <para>This method is a thin wrapper over the C <c>free</c> API.</para>
        /// </remarks>
        public static void free(void* ptr)
        {
#if NET6_0_OR_GREATER
            NativeMemory.Free(ptr);
#else
            Marshal.FreeHGlobal((nint)ptr);
#endif
        }

        /// <summary>
        ///     Copies bytes from the source address to the destination address
        ///     without assuming architecture dependent alignment of the addresses.
        /// </summary>
        /// <param name="destination">The destination address to copy to.</param>
        /// <param name="source">The source address to copy from.</param>
        /// <param name="byteCount">The number of bytes to copy.</param>
        public static void memcpy(void* destination, void* source, nuint byteCount)
        {
            if (!Environment.Is64BitProcess)
            {
                Unsafe.CopyBlockUnaligned(destination, source, (uint)byteCount);
                return;
            }

#if NET7_0_OR_GREATER
            NativeMemory.Copy(source, destination, byteCount);
#else
            Buffer.MemoryCopy(source, destination, byteCount, byteCount);
#endif
        }

        /// <summary>
        ///     Copies the byte <paramref name="value" /> to the first <paramref name="byteCount" /> bytes
        ///     of the memory located at <paramref name="startAddress" />.
        /// </summary>
        /// <param name="startAddress">A pointer to the block of memory to fill.</param>
        /// <param name="byteCount">The number of bytes to be set to <paramref name="value" />.</param>
        /// <param name="value">The value to be set.</param>
        public static void memset(void* startAddress, byte value, nuint byteCount)
        {
            if (!Environment.Is64BitProcess)
            {
                Unsafe.InitBlockUnaligned(startAddress, value, (uint)byteCount);
                return;
            }

#if NET7_0_OR_GREATER
            NativeMemory.Fill(startAddress, byteCount, value);
#else
            for (uint count; byteCount > 0; byteCount -= count, startAddress = (byte*)startAddress + count)
            {
                count = byteCount > uint.MaxValue ? uint.MaxValue : (uint)byteCount;
                Unsafe.InitBlockUnaligned(startAddress, value, count);
            }
#endif
        }

        /// <summary>
        ///     Terminates the current process with a failure exit code, mirroring the C <c>abort</c> behavior.
        /// </summary>
        public static void abort() => Environment.Exit(-1);

        /// <summary>
        ///     Returns the current time in milliseconds, derived from the high-resolution stopwatch.
        /// </summary>
        /// <returns>The current time in milliseconds.</returns>
        public static long timeGetTime() => Stopwatch.GetTimestamp() * 1000L / Stopwatch.Frequency;
    }
}