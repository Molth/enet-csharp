using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using NativeSockets;
using static enet.ENetSocketOption;
using static enet.ENetSocketType;
using static enet.ENetSocketWait;
using static enet.ENetHostOption;

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
        ///     Initializes ENet globally.
        ///     Must be called prior to using any functions in ENet.
        /// </summary>
        /// <returns>0 on success, &lt; 0 on failure</returns>
        public static int enet_initialize() => (int)NativeSocketPal.Startup();

        /// <summary>
        ///     Shuts down ENet globally.
        ///     Should be called when a program that has initialized ENet exits.
        /// </summary>
        public static void enet_deinitialize() => NativeSocketPal.Cleanup();

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

        public static int enet_socket_bind(ENetSocket socket, ENetAddress* address) => (int)socket.GetInner().Bind(address->GetInner());

        public static int enet_socket_get_address(ENetSocket socket, ENetAddress* address) => (int)socket.GetInner().GetName(ref address->GetInner());

        public static ENetSocket enet_socket_create(ENetSocketType type, ENetHostOption option = 0)
        {
            if (type == ENET_SOCKET_TYPE_DATAGRAM)
            {
                bool ipv6 = option == ENET_HOSTOPT_IPV6_ONLY || option == ENET_HOSTOPT_IPV6_DUALMODE;
                SocketError error = NativeSocket.Create(ipv6, out NativeSocket socket);
                if (error == SocketError.Success && option == ENET_HOSTOPT_IPV6_DUALMODE && enet_socket_set_option(new ENetSocket(socket), ENET_SOCKOPT_IPV6_ONLY, 0) < 0)
                {
                    socket.Dispose();
                    goto error;
                }

                return new ENetSocket(socket);
            }

            error:
            return new ENetSocket(new NativeSocket(INVALID_SOCKET, AddressFamily.Unspecified));
        }

        public static int enet_socket_set_option(ENetSocket socket, ENetSocketOption option, int value)
        {
            int result = SOCKET_ERROR;
            var optionValue = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<int, byte>(ref value), 4);
            switch (option)
            {
                case ENET_SOCKOPT_NONBLOCK:
                    result = enet_socket_set_nonblocking(socket, value);
                    break;
                case ENET_SOCKOPT_BROADCAST:
                    result = (int)socket.GetInner().SetOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, optionValue);
                    break;
                case ENET_SOCKOPT_RCVBUF:
                    result = (int)socket.GetInner().SetOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, optionValue);
                    break;
                case ENET_SOCKOPT_SNDBUF:
                    result = (int)socket.GetInner().SetOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, optionValue);
                    break;
                case ENET_SOCKOPT_REUSEADDR:
                    result = (int)socket.GetInner().SetOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, optionValue);
                    break;
                case ENET_SOCKOPT_RCVTIMEO:
                    result = (int)socket.GetInner().SetOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, optionValue);
                    break;
                case ENET_SOCKOPT_SNDTIMEO:
                    result = (int)socket.GetInner().SetOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, optionValue);
                    break;
                case ENET_SOCKOPT_NODELAY:
                    result = (int)socket.GetInner().SetOption(SocketOptionLevel.Socket, SocketOptionName.NoDelay, optionValue);
                    break;
                case ENET_SOCKOPT_TTL:
                    result = (int)socket.GetInner().SetOption(SocketOptionLevel.IP, SocketOptionName.IpTimeToLive, optionValue);
                    break;
                case ENET_SOCKOPT_IPV6_ONLY:
                    result = (int)socket.GetInner().SetOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, optionValue);
                    break;
                default:
                    break;
            }

            return result == 0 ? 0 : -1;
        }

        public static int enet_socket_set_nonblocking(ENetSocket socket, int nonBlocking) => (int)socket.GetInner().SetBlocking(nonBlocking == 0);

        public static void enet_socket_destroy(ENetSocket* socket)
        {
            socket->GetInner().Dispose();
            *socket = new ENetSocket(new NativeSocket(INVALID_SOCKET, AddressFamily.Unspecified));
        }

        public static int enet_socket_send(ENetSocket socket, ENetAddress* address, ENetBuffer* buffers, nuint bufferCount)
        {
            Debug.Assert(bufferCount <= 16);

            Span<NativeIoSlice> __buffers = stackalloc NativeIoSlice[(int)bufferCount];
            for (int i = 0; i < (int)bufferCount; ++i)
                __buffers[i] = new NativeIoSlice(buffers[i].data, (int)buffers[i].dataLength);

            return socket.GetInner().SendMessageTo(__buffers, address->GetInner());
        }

        public static int enet_socket_receive(ENetSocket socket, ENetAddress* address, ENetBuffer* buffers, nuint bufferCount)
        {
            Debug.Assert(bufferCount <= 16);

            Span<NativeIoSlice> __buffers = stackalloc NativeIoSlice[(int)bufferCount];
            for (int i = 0; i < (int)bufferCount; ++i)
                __buffers[i] = new NativeIoSlice(buffers[i].data, (int)buffers[i].dataLength);

            return socket.GetInner().ReceiveMessageFrom(__buffers, ref address->GetInner());
        }

        public static int enet_socket_wait(ENetSocket socket, uint* condition, uint milliseconds)
        {
            int error;
            bool status;

            if ((*condition & (uint)ENET_SOCKET_WAIT_SEND) != 0)
            {
                error = (int)socket.GetInner().Poll((int)(milliseconds * 1000), SelectMode.SelectWrite, out status);
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
                error = (int)socket.GetInner().Poll((int)(milliseconds * 1000), SelectMode.SelectRead, out status);
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

        public static int enet_address_set_from_endpoint(ENetAddress* address, IPEndPoint ip) => address->GetInner().FromIpEndPoint(ip) == SocketError.Success ? 0 : -1;

        public static int enet_address_set_from_address(ENetAddress* address, IPAddress ip, ushort port) => address->GetInner().FromIpAddress(ip, port) == SocketError.Success ? 0 : -1;

        public static int enet_address_set_ip_ipv4(ENetAddress* address, ReadOnlySpan<char> ip, ushort port) => address->GetInner().SetIpIpv4(ip, port) == SocketError.Success ? 0 : -1;

        public static int enet_address_set_ip_ipv6(ENetAddress* address, ReadOnlySpan<char> ip, ushort port, uint scopeId) => address->GetInner().SetIpIpv6(ip, port, scopeId) == SocketError.Success ? 0 : -1;

        public static int enet_address_set_host_ipv4(ENetAddress* address, ReadOnlySpan<char> hostName, ushort port) => address->GetInner().SetHostNameIpv4(hostName, port) == SocketError.Success ? 0 : -1;

        public static int enet_address_set_host_ipv6(ENetAddress* address, ReadOnlySpan<char> hostName, ushort port, uint scopeId) => address->GetInner().SetHostNameIpv6(hostName, port, scopeId) == SocketError.Success ? 0 : -1;

        public static int enet_address_get_ip(ENetAddress* address, ref Span<char> ip) => address->GetInner().GetIp(ref ip) == SocketError.Success ? 0 : -1;

        public static int enet_address_get_host(ENetAddress* address, ref Span<char> hostName) => address->GetInner().GetHostName(ref hostName) == SocketError.Success ? 0 : -1;
    }
}