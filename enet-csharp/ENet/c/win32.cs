using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using static enet.ENetSocketOption;
using static enet.ENetSocketType;
using static enet.ENetSocketWait;
using static NanoSockets.UDP;

#pragma warning disable CA1401
#pragma warning disable CA2101
#pragma warning disable CA2211
#pragma warning disable SYSLIB1054
#pragma warning disable CS1591

// ReSharper disable ALL

namespace enet
{
    public static unsafe partial class ENet
    {
        public const int SOCKET_ERROR = -1;

        public static uint timeBase = 0;

        public static int enet_initialize() => nanosockets_initialize();

        public static void enet_deinitialize() => nanosockets_deinitialize();

        public static uint enet_host_random_seed() => (uint)timeGetTime();

        public static uint enet_time_get() => (uint)timeGetTime() - timeBase;

        public static void enet_time_set(uint newTimeBase) => timeBase = (uint)timeGetTime() - newTimeBase;

        public static int enet_socket_bind(long socket, ENetAddress* address) => nanosockets_bind(socket, address);

        public static int enet_socket_get_address(long socket, ENetAddress* address) => nanosockets_address_get(socket, address);

        public static long enet_socket_create(ENetSocketType type)
        {
            if (type == ENET_SOCKET_TYPE_DATAGRAM)
                return nanosockets_create(0, 0);

            return INVALID_SOCKET;
        }

        public static int enet_socket_set_option(long socket, ENetSocketOption option, int value)
        {
            int result = SOCKET_ERROR;
            switch (option)
            {
                case ENET_SOCKOPT_NONBLOCK:
                    result = enet_socket_set_nonblocking(socket, (byte)value);
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

        public static int enet_socket_set_nonblocking(long socket, byte nonBlocking) => nanosockets_set_nonblocking(socket, nonBlocking);

        public static void enet_socket_destroy(long* socket) => nanosockets_destroy(socket);

        public static int enet_socket_send(long socket, ENetAddress* address, ENetBuffer* buffers, nuint bufferCount)
        {
            if (bufferCount == 0)
                return 0;
            else if (bufferCount == 1)
                return nanosockets_send(socket, address, buffers[0].data, (int)buffers[0].dataLength);
            else
            {
                nuint totalLength = 0;
                for (nuint i = 0; i < bufferCount; ++i)
                    totalLength += buffers[i].dataLength;
                byte* buffer = stackalloc byte[(int)totalLength];
                nuint offset = 0;
                for (nuint i = 0; i < bufferCount; ++i)
                {
                    memcpy(buffer + offset, buffers[i].data, buffers[i].dataLength);
                    offset += buffers[i].dataLength;
                }

                return nanosockets_send(socket, address, buffer, (int)totalLength);
            }
        }

        public static int enet_socket_receive(long socket, ENetAddress* address, ENetBuffer* buffers, nuint bufferCount)
        {
            if (bufferCount == 0)
                return 0;
            else if (bufferCount == 1)
                return nanosockets_receive(socket, address, buffers->data, (int)buffers->dataLength);
            else
            {
                nuint totalLength = 0;
                for (nuint i = 0; i < bufferCount; ++i)
                    totalLength += buffers[i].dataLength;
                byte* buffer = stackalloc byte[(int)totalLength];
                int result = nanosockets_receive(socket, address, buffer, (int)totalLength);
                if (result <= 0)
                    return result;
                int offset = 0;
                int length;
                for (nuint i = 0; i < bufferCount; ++i)
                {
                    length = result - offset;
                    if (length < (int)buffers[i].dataLength)
                    {
                        if (length > 0)
                            memcpy(buffers[i].data, buffer + offset, (nuint)length);
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

        public static int enet_socket_wait(long socket, uint* condition, uint timeout)
        {
            if ((*condition & (uint)ENET_SOCKET_WAIT_SEND) != 0)
                return 0;
            if ((*condition & (uint)ENET_SOCKET_WAIT_RECEIVE) != 0)
                return enet_socket_poll(socket, timeout) > 0 ? 0 : -1;
            return 0;
        }

        public static int enet_socket_poll(long socket, uint timeout) => nanosockets_poll(socket, timeout);

        public static int enet_address_set_host_ip(ENetAddress* address, ReadOnlySpan<char> hostName)
        {
            int byteCount = Encoding.ASCII.GetByteCount(hostName);
            byte* buffer = stackalloc byte[byteCount];
            Encoding.ASCII.GetBytes(hostName, MemoryMarshal.CreateSpan(ref *buffer, byteCount));
            return nanosockets_address_set_ip(address, buffer);
        }

        public static int enet_address_set_host(ENetAddress* address, ReadOnlySpan<char> hostName)
        {
            int byteCount = Encoding.ASCII.GetByteCount(hostName);
            byte* buffer = stackalloc byte[byteCount];
            Encoding.ASCII.GetBytes(hostName, MemoryMarshal.CreateSpan(ref *buffer, byteCount));
            return nanosockets_address_set_hostname(address, buffer);
        }

        public static int enet_address_get_host_ip(ENetAddress* address, byte* hostName, nuint nameLength) => nanosockets_address_get_ip(address, hostName, (int)nameLength);

        public static int enet_address_get_host(ENetAddress* address, byte* hostName, nuint nameLength) => nanosockets_address_get_hostname(address, hostName, (int)nameLength);
    }
}