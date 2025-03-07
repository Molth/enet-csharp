using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security;

#pragma warning disable CA1401
#pragma warning disable CA2101
#pragma warning disable CA2211
#pragma warning disable SYSLIB1054

// ReSharper disable ALL

namespace NanoSockets
{
    [StructLayout(LayoutKind.Explicit, Size = 4)]
    public unsafe struct in_addr
    {
        [FieldOffset(0)] public fixed byte S_un_b[4];
        [FieldOffset(0)] public fixed ushort S_un_w[2];
        [FieldOffset(0)] public ulong S_addr;
    }

    [StructLayout(LayoutKind.Explicit, Size = 16)]
    public unsafe struct in6_addr
    {
        [FieldOffset(0)] public fixed byte Byte[16];
        [FieldOffset(0)] public fixed ushort Word[8];
    }

    [StructLayout(LayoutKind.Explicit, Size = 16)]
    public unsafe struct in4_addr
    {
        [FieldOffset(0)] public fixed byte zeros[10];
        [FieldOffset(10)] public ushort ffff;
        [FieldOffset(12)] public in_addr ip;
    }

    [StructLayout(LayoutKind.Explicit, Size = 20)]
    public struct NanoAddress
    {
        [FieldOffset(0)] public in6_addr ipv6;
        [FieldOffset(0)] public in4_addr ipv4;
        [FieldOffset(16)] public ushort port;
    }

    [SuppressUnmanagedCodeSecurity]
    public static unsafe class NanoSockets
    {
#if __IOS__ || (UNITY_IOS && !UNITY_EDITOR)
        private const string NATIVE_LIBRARY = "__Internal";
#else
        private const string NATIVE_LIBRARY = "nanosockets";
#endif

        [DllImport(NATIVE_LIBRARY, EntryPoint = "nanosockets_initialize", CallingConvention = CallingConvention.Cdecl)]
        public static extern int nanosockets_initialize();

        [DllImport(NATIVE_LIBRARY, EntryPoint = "nanosockets_deinitialize", CallingConvention = CallingConvention.Cdecl)]
        public static extern void nanosockets_deinitialize();

        [DllImport(NATIVE_LIBRARY, EntryPoint = "nanosockets_bind", CallingConvention = CallingConvention.Cdecl)]
        public static extern int nanosockets_bind(long socket, void* address);

        [DllImport(NATIVE_LIBRARY, EntryPoint = "nanosockets_address_get", CallingConvention = CallingConvention.Cdecl)]
        public static extern int nanosockets_address_get(long socket, void* address);

        [DllImport(NATIVE_LIBRARY, EntryPoint = "nanosockets_create", CallingConvention = CallingConvention.Cdecl)]
        public static extern long nanosockets_create(int sendBufferSize, int receiveBufferSize);

        [DllImport(NATIVE_LIBRARY, EntryPoint = "nanosockets_set_nonblocking", CallingConvention = CallingConvention.Cdecl)]
        public static extern int nanosockets_set_nonblocking(long socket, byte nonBlocking);

        [DllImport(NATIVE_LIBRARY, EntryPoint = "nanosockets_set_option", CallingConvention = CallingConvention.Cdecl)]
        public static extern int nanosockets_set_option(long socket, SocketOptionLevel level, SocketOptionName optionName, int* optionValue, int optionLength);

        [DllImport(NATIVE_LIBRARY, EntryPoint = "nanosockets_destroy", CallingConvention = CallingConvention.Cdecl)]
        public static extern void nanosockets_destroy(long* socket);

        [DllImport(NATIVE_LIBRARY, EntryPoint = "nanosockets_send", CallingConvention = CallingConvention.Cdecl)]
        public static extern int nanosockets_send(long socket, void* address, void* buffer, int bufferLength);

        [DllImport(NATIVE_LIBRARY, EntryPoint = "nanosockets_receive", CallingConvention = CallingConvention.Cdecl)]
        public static extern int nanosockets_receive(long socket, void* address, void* buffer, int bufferLength);

        [DllImport(NATIVE_LIBRARY, EntryPoint = "nanosockets_poll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int nanosockets_poll(long socket, uint timeout);

        [DllImport(NATIVE_LIBRARY, EntryPoint = "nanosockets_address_set_ip", CallingConvention = CallingConvention.Cdecl)]
        public static extern int nanosockets_address_set_ip(void* address, void* hostName);

        [DllImport(NATIVE_LIBRARY, EntryPoint = "nanosockets_address_set_hostname", CallingConvention = CallingConvention.Cdecl)]
        public static extern int nanosockets_address_set_hostname(void* address, byte* hostName);

        [DllImport(NATIVE_LIBRARY, EntryPoint = "nanosockets_address_get_ip", CallingConvention = CallingConvention.Cdecl)]
        public static extern int nanosockets_address_get_ip(void* address, byte* hostName, int nameLength);

        [DllImport(NATIVE_LIBRARY, EntryPoint = "nanosockets_address_get_hostname", CallingConvention = CallingConvention.Cdecl)]
        public static extern int nanosockets_address_get_hostname(void* address, byte* hostName, int nameLength);
    }
}