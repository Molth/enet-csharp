using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using winsock;
using static enet.ENetSocketOption;
using static enet.ENetSocketType;
using static enet.ENetSocketWait;
using static enet.ENetSock;

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

        public static int enet_initialize() => (int)Initialize();

        public static void enet_deinitialize() => Cleanup();

        public static uint enet_host_random_seed() => (uint)timeGetTime();

        public static uint enet_time_get() => (uint)timeGetTime() - timeBase;

        public static void enet_time_set(uint newTimeBase) => timeBase = (uint)timeGetTime() - newTimeBase;

        public static int enet_socket_bind(long socket, ENetAddress* address)
        {
            sockaddr_in6 socketAddress;
            socketAddress.sin6_family = (ushort)ADDRESS_FAMILY_INTER_NETWORK_V6;
            socketAddress.sin6_port = address->port;
            socketAddress.sin6_flowinfo = 0;
            memcpy(socketAddress.sin6_addr, &address->host, 16);
            socketAddress.sin6_scope_id = 0;
            return (int)Bind((nint)socket, &socketAddress);
        }

        public static int enet_socket_get_address(long socket, ENetAddress* address)
        {
            sockaddr_in6 socketAddress;
            int result = (int)GetName((nint)socket, &socketAddress);
            if (result == 0)
            {
                memcpy(&address->host, socketAddress.sin6_addr, 16);
                address->port = socketAddress.sin6_port;
            }

            return result;
        }

        public static long enet_socket_create(ENetSocketType type)
        {
            if (type == ENET_SOCKET_TYPE_DATAGRAM)
                return Create();

            return INVALID_SOCKET;
        }

        public static int enet_socket_set_option(long socket, ENetSocketOption option, int value)
        {
            int result = SOCKET_ERROR;
            switch (option)
            {
                case ENET_SOCKOPT_NONBLOCK:
                    result = enet_socket_set_nonblocking(socket, value);
                    break;
                case ENET_SOCKOPT_BROADCAST:
                    result = (int)SetOption((nint)socket, SocketOptionLevel.Socket, SocketOptionName.Broadcast, &value);
                    break;
                case ENET_SOCKOPT_RCVBUF:
                    result = (int)SetOption((nint)socket, SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, &value);
                    break;
                case ENET_SOCKOPT_SNDBUF:
                    result = (int)SetOption((nint)socket, SocketOptionLevel.Socket, SocketOptionName.SendBuffer, &value);
                    break;
                case ENET_SOCKOPT_REUSEADDR:
                    result = (int)SetOption((nint)socket, SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, &value);
                    break;
                case ENET_SOCKOPT_RCVTIMEO:
                    result = (int)SetOption((nint)socket, SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, &value);
                    break;
                case ENET_SOCKOPT_SNDTIMEO:
                    result = (int)SetOption((nint)socket, SocketOptionLevel.Socket, SocketOptionName.SendTimeout, &value);
                    break;
                case ENET_SOCKOPT_NODELAY:
                    result = (int)SetOption((nint)socket, SocketOptionLevel.Socket, SocketOptionName.NoDelay, &value);
                    break;
                case ENET_SOCKOPT_TTL:
                    result = (int)SetOption((nint)socket, SocketOptionLevel.IP, SocketOptionName.IpTimeToLive, &value);
                    break;
                default:
                    break;
            }

            return result == SOCKET_ERROR ? -1 : 0;
        }

        public static int enet_socket_set_nonblocking(long socket, int nonBlocking) => (int)SetBlocking((nint)socket, nonBlocking == 0);

        public static void enet_socket_destroy(long* socket)
        {
            Close((nint)socket);
            *socket = -1;
        }

        public static int enet_socket_send(long socket, ENetAddress* address, ENetBuffer* buffers, nuint bufferCount)
        {
            sockaddr_in6 socketAddress;
            socketAddress.sin6_family = (ushort)ADDRESS_FAMILY_INTER_NETWORK_V6;
            socketAddress.sin6_port = address->port;
            socketAddress.sin6_flowinfo = 0;
            memcpy(socketAddress.sin6_addr, &address->host, 16);
            socketAddress.sin6_scope_id = 0;

            if (bufferCount == 0)
                return 0;
            else if (bufferCount == 1)
                return SendTo((nint)socket, buffers[0].data, (int)buffers[0].dataLength, &socketAddress);
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

                return SendTo((nint)socket, buffer, (int)totalLength, &socketAddress);
            }
        }

        public static int enet_socket_receive(long socket, ENetAddress* address, ENetBuffer* buffers, nuint bufferCount)
        {
            sockaddr_in6 socketAddress;
            int result;
            if (bufferCount == 0)
                return 0;
            else if (bufferCount == 1)
            {
                result = ReceiveFrom((nint)socket, buffers->data, (int)buffers->dataLength, &socketAddress);
                if (result <= 0)
                    return result;
                goto label;
            }
            else
            {
                nuint totalLength = 0;
                for (nuint i = 0; i < bufferCount; ++i)
                    totalLength += buffers[i].dataLength;
                byte* buffer = stackalloc byte[(int)totalLength];
                result = ReceiveFrom((nint)socket, buffer, (int)totalLength, &socketAddress);
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

                goto label;
            }

            label:
            memcpy(&address->host, socketAddress.sin6_addr, 16);
            address->port = socketAddress.sin6_port;
            return result;
        }

        public static int enet_socket_wait(long socket, uint* condition, uint timeout)
        {
            int error;
            bool status;

            if ((*condition & (uint)ENET_SOCKET_WAIT_SEND) != 0)
            {
                error = (int)Poll((nint)socket, (int)(timeout / 1000), SelectMode.SelectWrite, out status);
                return (error == 0 && status) ? 0 : -1;
            }

            if ((*condition & (uint)ENET_SOCKET_WAIT_RECEIVE) != 0)
            {
                error = (int)Poll((nint)socket, (int)(timeout / 1000), SelectMode.SelectRead, out status);
                return (error == 0 && status) ? 0 : -1;
            }

            return 0;
        }

        public static int enet_address_set_host_ip(ENetAddress* address, ReadOnlySpan<char> hostName)
        {
            return (int)SetIP(&address->host, hostName);
        }

        public static int enet_address_set_host(ENetAddress* address, ReadOnlySpan<char> hostName)
        {
            return (int)SetHostName(&address->host, hostName);
        }

        public static int enet_address_get_host_ip(ENetAddress* address, byte* hostName, nuint nameLength)
        {
            return (int)GetIP(&address->host, MemoryMarshal.CreateSpan(ref *hostName, (int)nameLength));
        }

        public static int enet_address_get_host(ENetAddress* address, byte* hostName, nuint nameLength)
        {
            sockaddr_in6 socketAddress;
            int result = (int)GetHostName(&socketAddress, MemoryMarshal.CreateSpan(ref *hostName, (int)nameLength));
            if (result == 0)
            {
                memcpy(&address->host, socketAddress.sin6_addr, 16);
                address->port = socketAddress.sin6_port;
            }

            return result;
        }
    }
}