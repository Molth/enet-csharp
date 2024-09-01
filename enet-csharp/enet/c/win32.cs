using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security;
using static enet.ENetSocketType;
using static enet.ENetSocketWait;

#pragma warning disable CA1401
#pragma warning disable CA2101
#pragma warning disable CA2211
#pragma warning disable SYSLIB1054

// ReSharper disable ALL

namespace enet
{
    [SuppressUnmanagedCodeSecurity]
    public static unsafe partial class ENet
    {
#if __IOS__ || (UNITY_IOS && !UNITY_EDITOR)
        private const string NATIVE_LIBRARY = "__Internal";
#else
        private const string NATIVE_LIBRARY = "nanosockets";
#endif

        public const int SOCKET_ERROR = -1;

        public static uint timeBase = 0;

        [DllImport(NATIVE_LIBRARY, EntryPoint = "nanosockets_initialize", CallingConvention = CallingConvention.Cdecl)]
        public static extern int enet_initialize();

        [DllImport(NATIVE_LIBRARY, EntryPoint = "nanosockets_deinitialize", CallingConvention = CallingConvention.Cdecl)]
        public static extern void enet_deinitialize();

        public static uint enet_host_random_seed() => (uint)timeGetTime();

        public static uint enet_time_get() => (uint)timeGetTime() - timeBase;

        [DllImport(NATIVE_LIBRARY, EntryPoint = "nanosockets_bind", CallingConvention = CallingConvention.Cdecl)]
        public static extern int enet_socket_bind(long socket, ENetAddress* address);

        [DllImport(NATIVE_LIBRARY, EntryPoint = "nanosockets_address_get", CallingConvention = CallingConvention.Cdecl)]
        public static extern int enet_socket_get_address(long socket, ENetAddress* address);

        public static long enet_socket_create(ENetSocketType type)
        {
            if (type == ENET_SOCKET_TYPE_DATAGRAM)
                return enet_socket_create(0, 0);

            return INVALID_SOCKET;
        }

        [DllImport(NATIVE_LIBRARY, EntryPoint = "nanosockets_create", CallingConvention = CallingConvention.Cdecl)]
        public static extern long enet_socket_create(int sendBufferSize, int receiveBufferSize);

        public static int enet_socket_set_option(long socket, ENetSocketOption option, int value)
        {
            int result = SOCKET_ERROR;
            switch (option)
            {
                case ENetSocketOption.ENET_SOCKOPT_NONBLOCK:
                    result = enet_socket_set_nonblocking(socket, (byte)value);
                    break;
                case ENetSocketOption.ENET_SOCKOPT_BROADCAST:
                    result = enet_socket_set_option(socket, SocketOptionLevel.Socket, SocketOptionName.Broadcast, &value, 4);
                    break;
                case ENetSocketOption.ENET_SOCKOPT_RCVBUF:
                    result = enet_socket_set_option(socket, SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, &value, 4);
                    break;
                case ENetSocketOption.ENET_SOCKOPT_SNDBUF:
                    result = enet_socket_set_option(socket, SocketOptionLevel.Socket, SocketOptionName.SendBuffer, &value, 4);
                    break;
                case ENetSocketOption.ENET_SOCKOPT_REUSEADDR:
                    result = enet_socket_set_option(socket, SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, &value, 4);
                    break;
                case ENetSocketOption.ENET_SOCKOPT_RCVTIMEO:
                    result = enet_socket_set_option(socket, SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, &value, 4);
                    break;
                case ENetSocketOption.ENET_SOCKOPT_SNDTIMEO:
                    result = enet_socket_set_option(socket, SocketOptionLevel.Socket, SocketOptionName.SendTimeout, &value, 4);
                    break;
                case ENetSocketOption.ENET_SOCKOPT_NODELAY:
                    result = enet_socket_set_option(socket, SocketOptionLevel.Socket, SocketOptionName.NoDelay, &value, 4);
                    break;
                case ENetSocketOption.ENET_SOCKOPT_TTL:
                    result = enet_socket_set_option(socket, SocketOptionLevel.IP, SocketOptionName.IpTimeToLive, &value, 4);
                    break;
                default:
                    break;
            }

            return result == SOCKET_ERROR ? -1 : 0;
        }

        [DllImport(NATIVE_LIBRARY, EntryPoint = "nanosockets_set_nonblocking", CallingConvention = CallingConvention.Cdecl)]
        public static extern int enet_socket_set_nonblocking(long socket, byte nonBlocking);

        [DllImport(NATIVE_LIBRARY, EntryPoint = "nanosockets_set_option", CallingConvention = CallingConvention.Cdecl)]
        public static extern int enet_socket_set_option(long socket, SocketOptionLevel level, SocketOptionName optionName, int* optionValue, int optionLength);

        [DllImport(NATIVE_LIBRARY, EntryPoint = "nanosockets_destroy", CallingConvention = CallingConvention.Cdecl)]
        public static extern void enet_socket_destroy(long* socket);

        public static int enet_socket_send(long socket, ENetAddress* address, ENetBuffer* buffers, nint bufferCount)
        {
            if (bufferCount == 0)
                return 0;
            else if (bufferCount == 1)
                return enet_socket_send(socket, address, buffers[0].data, buffers[0].dataLength);
            else
            {
                int totalLength = 0;
                for (int i = 0; i < bufferCount; ++i)
                    totalLength += (int)buffers[i].dataLength;
                byte* buffer = stackalloc byte[totalLength];
                int offset = 0;
                for (int i = 0; i < bufferCount; ++i)
                {
                    memcpy(buffer + offset, buffers[i].data, buffers[i].dataLength);
                    offset += (int)buffers[i].dataLength;
                }

                return enet_socket_send(socket, address, buffer, totalLength);
            }
        }

        [DllImport(NATIVE_LIBRARY, EntryPoint = "nanosockets_send", CallingConvention = CallingConvention.Cdecl)]
        public static extern int enet_socket_send(long socket, ENetAddress* address, void* buffer, nint bufferLength);

        public static int enet_socket_receive(long socket, ENetAddress* address, ENetBuffer* buffer) => enet_socket_receive(socket, address, buffer->data, buffer->dataLength);

        [DllImport(NATIVE_LIBRARY, EntryPoint = "nanosockets_receive", CallingConvention = CallingConvention.Cdecl)]
        public static extern int enet_socket_receive(long socket, ENetAddress* address, void* buffer, nint bufferLength);

        public static int enet_socket_wait(long socket, uint* condition, uint timeout)
        {
            if ((*condition & (uint)ENET_SOCKET_WAIT_SEND) != 0)
                return 0;
            if ((*condition & (uint)ENET_SOCKET_WAIT_RECEIVE) != 0)
                return enet_socket_poll(socket, timeout) > 0 ? 0 : -1;
            return 0;
        }

        [DllImport(NATIVE_LIBRARY, EntryPoint = "nanosockets_poll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int enet_socket_poll(long socket, uint timeout);

        [DllImport(NATIVE_LIBRARY, EntryPoint = "nanosockets_address_set_ip", CallingConvention = CallingConvention.Cdecl)]
        public static extern int enet_set_ip(ENetAddress* address, string ip);

        [DllImport(NATIVE_LIBRARY, EntryPoint = "nanosockets_address_get_ip", CallingConvention = CallingConvention.Cdecl)]
        public static extern int enet_get_ip(ENetAddress* address, void* buffer, int length);

        [DllImport(NATIVE_LIBRARY, EntryPoint = "nanosockets_address_set_ip", CallingConvention = CallingConvention.Cdecl)]
        public static extern int enet_set_ip(ENetIP* address, string ip);

        [DllImport(NATIVE_LIBRARY, EntryPoint = "nanosockets_address_get_ip", CallingConvention = CallingConvention.Cdecl)]
        public static extern int enet_get_ip(ENetIP* address, void* buffer, int length);
    }
}