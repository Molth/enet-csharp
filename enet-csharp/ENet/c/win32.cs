﻿using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using ENetSocket = long;
using size_t = nuint;
using enet_uint8 = byte;
using enet_uint32 = uint;
using static enet.ENetSocketOption;
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

        public static enet_uint32 timeBase = 0;

        [DllImport(NATIVE_LIBRARY, EntryPoint = "nanosockets_initialize", CallingConvention = CallingConvention.Cdecl)]
        public static extern int enet_initialize();

        [DllImport(NATIVE_LIBRARY, EntryPoint = "nanosockets_deinitialize", CallingConvention = CallingConvention.Cdecl)]
        public static extern void enet_deinitialize();

        public static enet_uint32 enet_host_random_seed() => (enet_uint32)timeGetTime();

        public static enet_uint32 enet_time_get() => (enet_uint32)timeGetTime() - timeBase;

        public static void enet_time_set(enet_uint32 newTimeBase) => timeBase = (enet_uint32)timeGetTime() - newTimeBase;

        [DllImport(NATIVE_LIBRARY, EntryPoint = "nanosockets_bind", CallingConvention = CallingConvention.Cdecl)]
        public static extern int enet_socket_bind(ENetSocket socket, ENetAddress* address);

        [DllImport(NATIVE_LIBRARY, EntryPoint = "nanosockets_address_get", CallingConvention = CallingConvention.Cdecl)]
        public static extern int enet_socket_get_address(ENetSocket socket, ENetAddress* address);

        public static ENetSocket enet_socket_create(ENetSocketType type)
        {
            if (type == ENET_SOCKET_TYPE_DATAGRAM)
                return nanosockets_create(0, 0);

            return INVALID_SOCKET;
        }

        [DllImport(NATIVE_LIBRARY, EntryPoint = "nanosockets_create", CallingConvention = CallingConvention.Cdecl)]
        public static extern ENetSocket nanosockets_create(int sendBufferSize, int receiveBufferSize);

        public static int enet_socket_set_option(ENetSocket socket, ENetSocketOption option, int value)
        {
            int result = SOCKET_ERROR;
            switch (option)
            {
                case ENET_SOCKOPT_NONBLOCK:
                    result = enet_socket_set_nonblocking(socket, (enet_uint8)value);
                    break;
                case ENET_SOCKOPT_BROADCAST:
                    result = nanosockets_set_option(socket, SocketOptionLevel.Socket, SocketOptionName.Broadcast, &value, 4);
                    break;
                case ENET_SOCKOPT_RCVBUF:
                    result = nanosockets_set_option(socket, SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, &value, 4);
                    break;
                case ENET_SOCKOPT_SNDBUF:
                    result = nanosockets_set_option(socket, SocketOptionLevel.Socket, SocketOptionName.SendBuffer, &value, 4);
                    break;
                case ENET_SOCKOPT_REUSEADDR:
                    result = nanosockets_set_option(socket, SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, &value, 4);
                    break;
                case ENET_SOCKOPT_RCVTIMEO:
                    result = nanosockets_set_option(socket, SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, &value, 4);
                    break;
                case ENET_SOCKOPT_SNDTIMEO:
                    result = nanosockets_set_option(socket, SocketOptionLevel.Socket, SocketOptionName.SendTimeout, &value, 4);
                    break;
                case ENET_SOCKOPT_NODELAY:
                    result = nanosockets_set_option(socket, SocketOptionLevel.Socket, SocketOptionName.NoDelay, &value, 4);
                    break;
                case ENET_SOCKOPT_TTL:
                    result = nanosockets_set_option(socket, SocketOptionLevel.IP, SocketOptionName.IpTimeToLive, &value, 4);
                    break;
                default:
                    break;
            }

            return result == SOCKET_ERROR ? -1 : 0;
        }

        [DllImport(NATIVE_LIBRARY, EntryPoint = "nanosockets_set_nonblocking", CallingConvention = CallingConvention.Cdecl)]
        public static extern int enet_socket_set_nonblocking(ENetSocket socket, enet_uint8 nonBlocking);

        [DllImport(NATIVE_LIBRARY, EntryPoint = "nanosockets_set_option", CallingConvention = CallingConvention.Cdecl)]
        public static extern int nanosockets_set_option(ENetSocket socket, SocketOptionLevel level, SocketOptionName optionName, int* optionValue, int optionLength);

        [DllImport(NATIVE_LIBRARY, EntryPoint = "nanosockets_destroy", CallingConvention = CallingConvention.Cdecl)]
        public static extern void enet_socket_destroy(ENetSocket* socket);

        public static int enet_socket_send(ENetSocket socket, ENetAddress* address, ENetBuffer* buffers, size_t bufferCount)
        {
            if (bufferCount == 0)
                return 0;
            else if (bufferCount == 1)
                return nanosockets_send(socket, address, buffers[0].data, buffers[0].dataLength);
            else
            {
                size_t totalLength = 0;
                for (size_t i = 0; i < bufferCount; ++i)
                    totalLength += buffers[i].dataLength;
                enet_uint8* buffer = stackalloc enet_uint8[(int)totalLength];
                size_t offset = 0;
                for (size_t i = 0; i < bufferCount; ++i)
                {
                    memcpy(buffer + offset, buffers[i].data, buffers[i].dataLength);
                    offset += buffers[i].dataLength;
                }

                return nanosockets_send(socket, address, buffer, totalLength);
            }
        }

        [DllImport(NATIVE_LIBRARY, EntryPoint = "nanosockets_send", CallingConvention = CallingConvention.Cdecl)]
        public static extern int nanosockets_send(ENetSocket socket, ENetAddress* address, void* buffer, size_t bufferLength);

        public static int enet_socket_receive(ENetSocket socket, ENetAddress* address, ENetBuffer* buffers, size_t bufferCount)
        {
            if (bufferCount == 0)
                return 0;
            else if (bufferCount == 1)
                return nanosockets_receive(socket, address, buffers->data, buffers->dataLength);
            else
            {
                size_t totalLength = 0;
                for (size_t i = 0; i < bufferCount; ++i)
                    totalLength += buffers[i].dataLength;
                enet_uint8* buffer = stackalloc enet_uint8[(int)totalLength];
                int result = nanosockets_receive(socket, address, buffer, totalLength);
                if (result <= 0)
                    return result;
                int offset = 0;
                int length;
                for (size_t i = 0; i < bufferCount; ++i)
                {
                    length = result - offset;
                    if (length < (int)buffers[i].dataLength)
                    {
                        if (length > 0)
                            memcpy(buffers[i].data, buffer + offset, (size_t)length);
                        break;
                    }
                    else
                    {
                        memcpy(buffers[i].data, buffer + offset, buffers[i].dataLength);
                        offset += (int)buffers[i].dataLength;
                    }
                }

                return result;
            }
        }

        [DllImport(NATIVE_LIBRARY, EntryPoint = "nanosockets_receive", CallingConvention = CallingConvention.Cdecl)]
        public static extern int nanosockets_receive(ENetSocket socket, ENetAddress* address, void* buffer, size_t bufferLength);

        public static int enet_socket_wait(ENetSocket socket, enet_uint32* condition, enet_uint32 timeout)
        {
            if ((*condition & (enet_uint32)ENET_SOCKET_WAIT_SEND) != 0)
                return 0;
            if ((*condition & (enet_uint32)ENET_SOCKET_WAIT_RECEIVE) != 0)
                return enet_socket_poll(socket, timeout) > 0 ? 0 : -1;
            return 0;
        }

        [DllImport(NATIVE_LIBRARY, EntryPoint = "nanosockets_poll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int enet_socket_poll(ENetSocket socket, enet_uint32 timeout);

        public static int enet_address_set_host_ip(ENetIP* address, ReadOnlySpan<char> hostName)
        {
            int byteCount = Encoding.ASCII.GetByteCount(hostName);
            enet_uint8* buffer = stackalloc enet_uint8[byteCount];
            Encoding.ASCII.GetBytes(hostName, MemoryMarshal.CreateSpan(ref *buffer, byteCount));
            return nanosockets_address_set_ip(address, buffer);
        }

        [DllImport(NATIVE_LIBRARY, EntryPoint = "nanosockets_address_set_ip", CallingConvention = CallingConvention.Cdecl)]
        public static extern int nanosockets_address_set_ip(ENetIP* address, enet_uint8* hostName);

        public static int enet_address_set_host(ENetIP* address, ReadOnlySpan<char> hostName)
        {
            int byteCount = Encoding.ASCII.GetByteCount(hostName);
            enet_uint8* buffer = stackalloc enet_uint8[byteCount];
            Encoding.ASCII.GetBytes(hostName, MemoryMarshal.CreateSpan(ref *buffer, byteCount));
            return nanosockets_address_set_hostname(address, buffer);
        }

        [DllImport(NATIVE_LIBRARY, EntryPoint = "nanosockets_address_set_hostname", CallingConvention = CallingConvention.Cdecl)]
        public static extern int nanosockets_address_set_hostname(ENetIP* address, enet_uint8* hostName);

        [DllImport(NATIVE_LIBRARY, EntryPoint = "nanosockets_address_get_ip", CallingConvention = CallingConvention.Cdecl)]
        public static extern int enet_address_get_host_ip(ENetIP* address, enet_uint8* hostName, size_t nameLength);

        [DllImport(NATIVE_LIBRARY, EntryPoint = "nanosockets_address_get_hostname", CallingConvention = CallingConvention.Cdecl)]
        public static extern int enet_address_get_host(ENetIP* address, enet_uint8* hostName, size_t nameLength);
    }
}