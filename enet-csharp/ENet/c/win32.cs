﻿using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using NativeSockets;
using static enet.ENetSocketOption;
using static enet.ENetSocketType;
using static enet.ENetSocketWait;
using static enet.ENetHostOption;
using static NativeSockets.SocketPal;

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

        /// <summary>
        ///     Initializes ENet globally. Must be called prior to using any functions in
        ///     ENet.
        /// </summary>
        /// <returns>0 on success, &lt; 0 on failure</returns>
        public static int enet_initialize() => (int)Initialize();

        /// <summary>
        ///     Shuts down ENet globally.  Should be called when a program that has
        ///     initialized ENet exits.
        /// </summary>
        public static void enet_deinitialize() => Cleanup();

        public static uint enet_host_random_seed() => (uint)timeGetTime();

        /// <returns>
        ///     the wall-time in milliseconds.  Its initial value is unspecified
        ///     unless otherwise set.
        /// </returns>
        public static uint enet_time_get() => (uint)timeGetTime() - timeBase;

        /// <summary>
        ///     Sets the current wall-time in milliseconds.
        /// </summary>
        public static void enet_time_set(uint newTimeBase) => timeBase = (uint)timeGetTime() - newTimeBase;

        public static int enet_socket_bind(ENetSocket socket, ENetAddress* address)
        {
            if (address == null)
                return socket.IsIPv6 ? (int)Bind6(socket, null) : (int)Bind4(socket, null);

            if (socket.IsIPv6)
            {
                sockaddr_in6 socketAddress;
                socketAddress.sin6_family = (ushort)ADDRESS_FAMILY_INTER_NETWORK_V6;
                socketAddress.sin6_port = address->port;
                socketAddress.sin6_flowinfo = 0;
                memcpy(socketAddress.sin6_addr, &address->host, 16);
                socketAddress.sin6_scope_id = address->scopeID;

                return (int)Bind6(socket, &socketAddress);
            }
            else
            {
                if (address->IsIPv6)
                    return -1;

                sockaddr_in socketAddress;
                socketAddress.sin_family = (ushort)AddressFamily.InterNetwork;
                socketAddress.sin_port = address->port;
                Unsafe.WriteUnaligned(&socketAddress.sin_addr, address->address);
                memset(socketAddress.sin_zero, 0, 8);

                return (int)Bind4(socket, &socketAddress);
            }
        }

        public static int enet_socket_get_address(nint socket, ENetAddress* address)
        {
            sockaddr_in6 socketAddress;
            int result = (int)GetName6(socket, &socketAddress);
            if (result == 0)
            {
                memcpy(&address->host, socketAddress.sin6_addr, 16);
                address->port = socketAddress.sin6_port;
                address->scopeID = socketAddress.sin6_scope_id;
            }

            return result;
        }

        public static ENetSocket enet_socket_create(ENetSocketType type, ENetHostOption option = 0)
        {
            if (type == ENET_SOCKET_TYPE_DATAGRAM)
            {
                bool ipv6 = option == ENET_HOSTOPT_IPV6_ONLY || option == ENET_HOSTOPT_IPV6_DUALMODE;
                nint socket = Create(ipv6);
                if (socket != ENET_SOCKET_NULL && option == ENET_HOSTOPT_IPV6_DUALMODE && enet_socket_set_option(socket, ENET_SOCKOPT_IPV6_ONLY, 0) < 0)
                {
                    Close(socket);
                    goto error;
                }

                return new ENetSocket { handle = socket, IsIPv6 = ipv6 };
            }

            error:
            return new ENetSocket { handle = INVALID_SOCKET };
        }

        public static int enet_socket_set_option(nint socket, ENetSocketOption option, int value)
        {
            int result = SOCKET_ERROR;
            switch (option)
            {
                case ENET_SOCKOPT_NONBLOCK:
                    result = enet_socket_set_nonblocking(socket, value);
                    break;
                case ENET_SOCKOPT_BROADCAST:
                    result = (int)SetOption(socket, SocketOptionLevel.Socket, SocketOptionName.Broadcast, &value);
                    break;
                case ENET_SOCKOPT_RCVBUF:
                    result = (int)SetOption(socket, SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, &value);
                    break;
                case ENET_SOCKOPT_SNDBUF:
                    result = (int)SetOption(socket, SocketOptionLevel.Socket, SocketOptionName.SendBuffer, &value);
                    break;
                case ENET_SOCKOPT_REUSEADDR:
                    result = (int)SetOption(socket, SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, &value);
                    break;
                case ENET_SOCKOPT_RCVTIMEO:
                    result = (int)SetOption(socket, SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, &value);
                    break;
                case ENET_SOCKOPT_SNDTIMEO:
                    result = (int)SetOption(socket, SocketOptionLevel.Socket, SocketOptionName.SendTimeout, &value);
                    break;
                case ENET_SOCKOPT_NODELAY:
                    result = (int)SetOption(socket, SocketOptionLevel.Socket, SocketOptionName.NoDelay, &value);
                    break;
                case ENET_SOCKOPT_TTL:
                    result = (int)SetOption(socket, SocketOptionLevel.IP, SocketOptionName.IpTimeToLive, &value);
                    break;
                case ENET_SOCKOPT_IPV6_ONLY:
                    result = (int)SetOption(socket, SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, &value);
                    break;
                default:
                    break;
            }

            return result == SOCKET_ERROR ? -1 : 0;
        }

        public static int enet_socket_set_nonblocking(nint socket, int nonBlocking) => (int)SetBlocking(socket, nonBlocking == 0);

        public static void enet_socket_destroy(ENetSocket* socket)
        {
            Close(socket->handle);
            socket->handle = -1;
        }

        public static int enet_socket_send(ENetSocket socket, ENetAddress* address, ENetBuffer* buffers, nuint bufferCount)
        {
            if (bufferCount == 0)
                return 0;

            if (socket.IsIPv6)
            {
                sockaddr_in6 socketAddress;
                socketAddress.sin6_family = (ushort)ADDRESS_FAMILY_INTER_NETWORK_V6;
                socketAddress.sin6_port = address->port;
                socketAddress.sin6_flowinfo = 0;
                memcpy(socketAddress.sin6_addr, &address->host, 16);
                socketAddress.sin6_scope_id = address->scopeID;

                if (bufferCount == 1)
                    return SendTo6(socket, buffers[0].data, (int)buffers[0].dataLength, &socketAddress);
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

                    return SendTo6(socket, buffer, (int)totalLength, &socketAddress);
                }
            }
            else
            {
                if (address->IsIPv6)
                    return -1;

                sockaddr_in socketAddress;
                socketAddress.sin_family = (ushort)AddressFamily.InterNetwork;
                socketAddress.sin_port = address->port;
                Unsafe.WriteUnaligned(&socketAddress.sin_addr, address->address);
                memset(socketAddress.sin_zero, 0, 8);

                if (bufferCount == 1)
                    return SendTo4(socket, buffers[0].data, (int)buffers[0].dataLength, &socketAddress);
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

                    return SendTo4(socket, buffer, (int)totalLength, &socketAddress);
                }
            }
        }

        public static int enet_socket_receive(ENetSocket socket, ENetAddress* address, ENetBuffer* buffers, nuint bufferCount)
        {
            if (bufferCount == 0)
                return 0;

            int result;
            if (socket.IsIPv6)
            {
                sockaddr_in6 socketAddress;
                if (bufferCount == 1)
                {
                    result = ReceiveFrom6(socket, buffers->data, (int)buffers->dataLength, &socketAddress);
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
                    result = ReceiveFrom6(socket, buffer, (int)totalLength, &socketAddress);
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
                address->scopeID = socketAddress.sin6_scope_id;
                return result;
            }
            else
            {
                sockaddr_in socketAddress;
                if (bufferCount == 1)
                {
                    result = ReceiveFrom4(socket, buffers->data, (int)buffers->dataLength, &socketAddress);
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
                    result = ReceiveFrom4(socket, buffer, (int)totalLength, &socketAddress);
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
                memset(address, 0, 8);
                Unsafe.WriteUnaligned((byte*)address + 8, WinSock2.ADDRESS_FAMILY_INTER_NETWORK_V4_MAPPED_V6);
                Unsafe.WriteUnaligned(&address->address, socketAddress.sin_addr);
                address->port = socketAddress.sin_port;
                return result;
            }
        }

        public static int enet_socket_wait(nint socket, uint* condition, uint timeout)
        {
            int error;
            bool status;

            if ((*condition & (uint)ENET_SOCKET_WAIT_SEND) != 0)
            {
                error = (int)Poll(socket, (int)(timeout * 1000), SelectMode.SelectWrite, out status);
                if (error == 0)
                {
                    *condition = (uint)ENET_SOCKET_WAIT_NONE;
                    if (status)
                    {
                        *condition |= (uint)ENET_SOCKET_WAIT_SEND;
                        return 0;
                    }
                }

                return -1;
            }

            if ((*condition & (uint)ENET_SOCKET_WAIT_RECEIVE) != 0)
            {
                error = (int)Poll(socket, (int)(timeout * 1000), SelectMode.SelectRead, out status);
                if (error == 0)
                {
                    *condition = (uint)ENET_SOCKET_WAIT_NONE;
                    if (status)
                    {
                        *condition |= (uint)ENET_SOCKET_WAIT_RECEIVE;
                        return 0;
                    }
                }

                return -1;
            }

            return 0;
        }

        /// <summary>
        ///     Attempts to parse the printable form of the IP address in the parameter hostName
        ///     and sets the host field in the address parameter if successful.
        /// </summary>
        /// <param name="address">destination to store the parsed IP address</param>
        /// <param name="ip">IP address to parse</param>
        /// <returns>
        ///     <list type="bullet">
        ///         <item>
        ///             <description>0 on success</description>
        ///         </item>
        ///         <item>
        ///             <description>&lt; 0 on failure</description>
        ///         </item>
        ///     </list>
        ///     the address of the given hostName in address on success
        /// </returns>
        public static int enet_address_set_host_ip(ENetAddress* address, ReadOnlySpan<char> ip)
        {
            sockaddr_in6 __socketAddress_native;
            SocketError error = SetIP6(&__socketAddress_native, ip);
            if (error == 0)
            {
                memcpy(address, __socketAddress_native.sin6_addr, 16);
                address->scopeID = __socketAddress_native.sin6_scope_id;
            }

            return (int)error;
        }

        /// <summary>
        ///     Attempts to resolve the host named by the parameter hostName and sets
        ///     the host field in the address parameter if successful.
        /// </summary>
        /// <param name="address">destination to store resolved address</param>
        /// <param name="hostName">host name to lookup</param>
        /// <returns>
        ///     <list type="bullet">
        ///         <item>
        ///             <description>0 on success</description>
        ///         </item>
        ///         <item>
        ///             <description>&lt; 0 on failure</description>
        ///         </item>
        ///     </list>
        ///     the address of the given hostName in address on success
        /// </returns>
        public static int enet_address_set_host(ENetAddress* address, ReadOnlySpan<char> hostName)
        {
            sockaddr_in6 __socketAddress_native;
            SocketError error = SetHostName6(&__socketAddress_native, hostName);
            if (error == 0)
            {
                memcpy(address, __socketAddress_native.sin6_addr, 16);
                address->scopeID = __socketAddress_native.sin6_scope_id;
            }

            return (int)error;
        }

        /// <summary>
        ///     Gives the printable form of the IP address specified in the <b>address</b> parameter.
        /// </summary>
        /// <param name="address">address printed</param>
        /// <param name="ip">destination for name, must not be NULL</param>
        /// <param name="nameLength">maximum length of hostName.</param>
        /// <returns>
        ///     <list type="bullet">
        ///         <item>
        ///             <description>0 on success</description>
        ///         </item>
        ///         <item>
        ///             <description>&lt; 0 on failure</description>
        ///         </item>
        ///     </list>
        ///     the null-terminated name of the host in hostName on success
        /// </returns>
        public static int enet_address_get_host_ip(ENetAddress* address, byte* ip, nuint nameLength)
        {
            sockaddr_in6 __socketAddress_native;
            memcpy(__socketAddress_native.sin6_addr, address, 16);
            __socketAddress_native.sin6_scope_id = address->scopeID;

            SocketError error = GetIP6(&__socketAddress_native, MemoryMarshal.CreateSpan(ref *ip, (int)nameLength));

            return (int)error;
        }

        /// <summary>
        ///     Attempts to do a reverse lookup of the host field in the address parameter.
        /// </summary>
        /// <param name="address">address used for reverse lookup</param>
        /// <param name="hostName">destination for name, must not be NULL</param>
        /// <param name="nameLength">maximum length of hostName.</param>
        /// <returns>
        ///     <list type="bullet">
        ///         <item>
        ///             <description>0 on success</description>
        ///         </item>
        ///         <item>
        ///             <description>&lt; 0 on failure</description>
        ///         </item>
        ///     </list>
        ///     the null-terminated name of the host in hostName on success
        /// </returns>
        public static int enet_address_get_host(ENetAddress* address, byte* hostName, nuint nameLength)
        {
            sockaddr_in6 __socketAddress_native;

            __socketAddress_native.sin6_family = (ushort)ADDRESS_FAMILY_INTER_NETWORK_V6;
            __socketAddress_native.sin6_port = address->port;
            __socketAddress_native.sin6_flowinfo = 0;
            memcpy(__socketAddress_native.sin6_addr, address, 16);
            __socketAddress_native.sin6_scope_id = address->scopeID;

            SocketError error = GetHostName6(&__socketAddress_native, MemoryMarshal.CreateSpan(ref *hostName, (int)nameLength));

            return (int)error;
        }
    }
}