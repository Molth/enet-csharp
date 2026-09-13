using System;
using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using NativeSockets;
using static enet.ENetSocketOption;
using static enet.ENetSocketType;
using static enet.ENetSocketWait;
using static enet.ENetHostOption;

// ReSharper disable ALL

namespace enet
{
    public static unsafe partial class ENet
    {
        /// <summary>
        ///     The wall-time base used to make <c>enet_time_get</c> values start at zero.
        /// </summary>
#pragma warning disable CA2211 // Non-constant fields should not be visible
        public static uint timeBase;
#pragma warning restore CA2211 // Non-constant fields should not be visible

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

        /// <summary>
        ///     Returns a random seed derived from the current time for host initialization.
        /// </summary>
        /// <returns>A random seed value.</returns>
        public static uint enet_host_random_seed() => (uint)timeGetTime();

        /// <summary>
        ///     Returns the time in milliseconds elapsed since the time base was set.
        /// </summary>
        /// <returns>
        ///     the wall-time in milliseconds.  Its initial value is unspecified
        ///     unless otherwise set.
        /// </returns>
        public static uint enet_time_get() => (uint)timeGetTime() - timeBase;

        /// <summary>
        ///     Sets the current wall-time in milliseconds.
        /// </summary>
        public static void enet_time_set(uint newTimeBase) => timeBase = (uint)timeGetTime() - newTimeBase;

        /// <summary>
        ///     Binds the socket to the specified local address.
        /// </summary>
        /// <param name="socket">The socket to bind.</param>
        /// <param name="address">The local address to bind to.</param>
        /// <returns>0 on success, SOCKET_ERROR on failure.</returns>
        public static int enet_socket_bind(ENetSocket socket, ENetAddress* address) => (int)socket.GetInner().Bind(address->GetInner());

        /// <summary>
        ///     Retrieves the local address the socket is bound to.
        /// </summary>
        /// <param name="socket">The socket to query.</param>
        /// <param name="address">Receives the local address.</param>
        /// <returns>0 on success, SOCKET_ERROR on failure.</returns>
        public static int enet_socket_get_address(ENetSocket socket, ENetAddress* address) => (int)socket.GetInner().GetName(ref address->GetInner());

        /// <summary>
        ///     Creates a native socket of the requested type and addressing mode.
        /// </summary>
        /// <param name="type">The type of socket to create.</param>
        /// <param name="option">The addressing mode to use.</param>
        /// <returns>The created socket, or an invalid socket on failure.</returns>
        public static ENetSocket enet_socket_create(ENetSocketType type, ENetHostOption option)
        {
            if (type == ENET_SOCKET_TYPE_DATAGRAM)
            {
                bool ipv6 = option == ENET_HOSTOPT_IPV6_ONLY || option == ENET_HOSTOPT_IPV6_DUALMODE;
                SocketError error = NativeSocket.Create(ipv6, out NativeSocket socket);

                if (error != SocketError.Success)
                    goto error;

                if (option == ENET_HOSTOPT_IPV6_ONLY && socket.SetDualMode(false) != SocketError.Success)
                {
                    socket.Dispose();
                    goto error;
                }

                if (option == ENET_HOSTOPT_IPV6_DUALMODE && socket.SetDualMode(true) != SocketError.Success)
                {
                    socket.Dispose();
                    goto error;
                }

                return new ENetSocket(socket);
            }

            error:
            return new ENetSocket(new NativeSocket(INVALID_SOCKET, AddressFamily.Unspecified));
        }

        /// <summary>
        ///     Applies a socket option to the given socket.
        /// </summary>
        /// <param name="socket">The socket to configure.</param>
        /// <param name="option">The option to apply.</param>
        /// <param name="value">The option value.</param>
        /// <returns>0 on success, -1 on failure or for unsupported options.</returns>
        public static int enet_socket_set_option(ENetSocket socket, ENetSocketOption option, int value)
        {
            int result = SOCKET_ERROR;
            ReadOnlySpan<byte> optionValue = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.As<int, byte>(ref value), 4);
            switch (option)
            {
                case ENET_SOCKOPT_NONBLOCK:
                    result = (int)socket.GetInner().SetBlocking(value == 0);
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
                case ENET_SOCKOPT_SNDTIMEO:
                    if (!IsWindows())
                    {
                        nint* timeval = stackalloc nint[2];
                        timeval[0] = value / 1000;
                        timeval[1] = value % 1000 * 1000;
                        optionValue = MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef<byte>(timeval), 2 * sizeof(nint));
                    }

                    result = (int)socket.GetInner().SetOption(SocketOptionLevel.Socket, option == ENET_SOCKOPT_RCVTIMEO ? SocketOptionName.ReceiveTimeout : SocketOptionName.SendTimeout, optionValue);
                    break;
                case ENET_SOCKOPT_ERROR:
                    result = (int)socket.GetInner().SetOption(SocketOptionLevel.Socket, SocketOptionName.Error, optionValue);
                    break;
                case ENET_SOCKOPT_TTL:
                    result = (int)socket.GetInner().SetOption(SocketOptionLevel.IP, SocketOptionName.IpTimeToLive, optionValue);
                    break;
                case ENET_SOCKOPT_IPV6_ONLY:
                    result = (int)socket.GetInner().SetOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, optionValue);
                    break;
            }

            return result == 0 ? 0 : -1;

            static bool IsWindows() =>
#if NET5_0_OR_GREATER
                OperatingSystem.IsWindows();
#else
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
#endif
        }

        /// <summary>
        ///     Closes and invalidates the given socket.
        /// </summary>
        /// <param name="socket">The socket to destroy.</param>
        public static void enet_socket_destroy(ENetSocket* socket)
        {
            socket->GetInner().Dispose();
            *socket = new ENetSocket(new NativeSocket(INVALID_SOCKET, AddressFamily.Unspecified));
        }

        /// <summary>
        ///     Sends a vectored payload to the specified address on the socket.
        /// </summary>
        /// <param name="socket">The socket to send on.</param>
        /// <param name="address">The destination address.</param>
        /// <param name="buffers">The buffers holding the payload.</param>
        /// <param name="bufferCount">The number of buffers.</param>
        /// <returns>The number of bytes sent, 0 when the send would block, -1 on failure.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="bufferCount" /> or any of the <paramref name="buffers" /> has a <c>dataLength</c>
        ///     greater than <see cref="int.MaxValue" />.
        /// </exception>
        public static int enet_socket_send(ENetSocket socket, ENetAddress* address, ENetBuffer* buffers, nuint bufferCount)
        {
            int num;

            NativeIoSlice[]? array = null;
            Span<NativeIoSlice> __buffers = bufferCount <= 32 ? stackalloc NativeIoSlice[(int)bufferCount] : (array = ArrayPool<NativeIoSlice>.Shared.Rent((int)bufferCount)).AsSpan(0, (int)bufferCount);

            try
            {
                for (int i = 0; i < (int)bufferCount; ++i)
                    __buffers[i] = new NativeIoSlice(buffers[i].data, (int)buffers[i].dataLength);

                num = address != null ? socket.GetInner().SendToVectored(__buffers, address->GetInner()) : socket.GetInner().SendVectored(__buffers);
            }
            finally
            {
                if (array != null)
                    ArrayPool<NativeIoSlice>.Shared.Return(array);
            }

            if (num == -1)
            {
                if (NativeSocketPal.GetLastSocketError() == SocketError.WouldBlock)
                    return 0;

                return -1;
            }

            return num;
        }

        /// <summary>
        ///     Receives a vectored payload on the socket, reporting the sender address.
        /// </summary>
        /// <param name="socket">The socket to receive on.</param>
        /// <param name="address">Receives the source address.</param>
        /// <param name="buffers">The buffers receiving the payload.</param>
        /// <param name="bufferCount">The number of buffers.</param>
        /// <returns>
        ///     The number of bytes received, 0 when no data is available,
        ///     -2 when the receive was interrupted or truncated, -1 on failure.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown when <paramref name="bufferCount" /> or any of the <paramref name="buffers" /> has a <c>dataLength</c>
        ///     greater than <see cref="int.MaxValue" />.
        /// </exception>
        public static int enet_socket_receive(ENetSocket socket, ENetAddress* address, ENetBuffer* buffers, nuint bufferCount)
        {
            int num;
            SocketFlags flags = 0;

            NativeIoSlice[]? array = null;
            Span<NativeIoSlice> __buffers = bufferCount <= 32 ? stackalloc NativeIoSlice[(int)bufferCount] : (array = ArrayPool<NativeIoSlice>.Shared.Rent((int)bufferCount)).AsSpan(0, (int)bufferCount);

            try
            {
                for (int i = 0; i < (int)bufferCount; ++i)
                    __buffers[i] = new NativeIoSlice(buffers[i].data, (int)buffers[i].dataLength);

                num = address != null ? socket.GetInner().ReceiveFromVectored(__buffers, ref flags, ref address->GetInner()) : socket.GetInner().ReceiveVectored(__buffers, ref flags);
            }
            finally
            {
                if (array != null)
                    ArrayPool<NativeIoSlice>.Shared.Return(array);
            }

            if (num == -1)
            {
                switch (NativeSocketPal.GetLastSocketError())
                {
                    case SocketError.WouldBlock:
                    case SocketError.ConnectionReset:
                        return 0;
                    case SocketError.Interrupted:
                    case SocketError.MessageSize:
                    case SocketError.Success when (flags & SocketFlags.Partial) != 0:
                        return -2;
                    default:
                        return -1;
                }
            }

            return num;
        }

        /// <summary>
        ///     Waits until the socket becomes ready for the requested conditions or the timeout elapses.
        /// </summary>
        /// <param name="socket">The socket to wait on.</param>
        /// <param name="condition">
        ///     On input the conditions to wait for; on output the conditions that became ready.
        /// </param>
        /// <param name="milliseconds">The maximum time to wait in milliseconds.</param>
        /// <returns>0 on success, -1 on failure.</returns>
        public static int enet_socket_wait(ENetSocket socket, uint* condition, uint milliseconds)
        {
            SelectModeFlags inFlags = 0;

            if ((*condition & (uint)ENET_SOCKET_WAIT_SEND) != 0)
                inFlags |= SelectModeFlags.SelectWrite;

            if ((*condition & (uint)ENET_SOCKET_WAIT_RECEIVE) != 0)
                inFlags |= SelectModeFlags.SelectRead;

            if ((*condition & (uint)ENET_SOCKET_WAIT_INTERRUPT) != 0)
                inFlags |= SelectModeFlags.SelectError;

            *condition = 0;

            int error = (int)socket.GetInner().PollFlags((int)(milliseconds * 1000), inFlags, out SelectModeFlags outFlags);
            if (error == 0)
            {
                if ((outFlags & SelectModeFlags.SelectWrite) != 0)
                    *condition |= (uint)ENET_SOCKET_WAIT_SEND;

                if ((outFlags & SelectModeFlags.SelectRead) != 0)
                    *condition |= (uint)ENET_SOCKET_WAIT_RECEIVE;

                if ((outFlags & SelectModeFlags.SelectError) != 0)
                    *condition |= (uint)ENET_SOCKET_WAIT_INTERRUPT;

                return 0;
            }

            return -1;
        }

        /// <summary>
        ///     Populates an ENet address from an <see cref="IPEndPoint" />.
        /// </summary>
        /// <param name="address">The address to populate.</param>
        /// <param name="ipEndPoint">The endpoint containing the address and port.</param>
        /// <returns>0 on success, -1 on failure.</returns>
        public static int enet_address_set_from_ipendpoint(ENetAddress* address, IPEndPoint ipEndPoint) => address->FromIpEndPoint(ipEndPoint) == SocketError.Success ? 0 : -1;

        /// <summary>
        ///     Populates an ENet address from an <see cref="IPAddress" /> and port.
        /// </summary>
        /// <param name="address">The address to populate.</param>
        /// <param name="ipAddress">The IP address to set.</param>
        /// <param name="port">The port number.</param>
        /// <returns>0 on success, -1 on failure.</returns>
        public static int enet_address_set_from_ipaddress(ENetAddress* address, IPAddress ipAddress, ushort port) => address->FromIpAddress(ipAddress, port) == SocketError.Success ? 0 : -1;

        /// <summary>
        ///     Populates an ENet address by parsing an Ipv4 address string and port.
        /// </summary>
        /// <param name="address">The address to populate.</param>
        /// <param name="ip">The Ipv4 address string.</param>
        /// <param name="port">The port number.</param>
        /// <returns>0 on success, -1 on failure.</returns>
        public static int enet_address_set_ip_ipv4(ENetAddress* address, ReadOnlySpan<char> ip, ushort port) => address->SetIpIpv4(ip, port) == SocketError.Success ? 0 : -1;

        /// <summary>
        ///     Populates an ENet address by parsing an Ipv6 address string, port and scope.
        /// </summary>
        /// <param name="address">The address to populate.</param>
        /// <param name="ip">The Ipv6 address string.</param>
        /// <param name="port">The port number.</param>
        /// <param name="scopeId">The Ipv6 scope identifier.</param>
        /// <returns>0 on success, -1 on failure.</returns>
        public static int enet_address_set_ip_ipv6(ENetAddress* address, ReadOnlySpan<char> ip, ushort port, uint scopeId = 0) => address->SetIpIpv6(ip, port, scopeId) == SocketError.Success ? 0 : -1;

        /// <summary>
        ///     Populates an ENet address by resolving a host name to an Ipv4 address.
        /// </summary>
        /// <param name="address">The address to populate.</param>
        /// <param name="hostName">The host name to resolve.</param>
        /// <param name="port">The port number.</param>
        /// <returns>0 on success, -1 on failure.</returns>
        public static int enet_address_set_hostname_ipv4(ENetAddress* address, ReadOnlySpan<char> hostName, ushort port) => address->SetHostNameIpv4(hostName, port) == SocketError.Success ? 0 : -1;

        /// <summary>
        ///     Populates an ENet address by resolving a host name to an Ipv6 address.
        /// </summary>
        /// <param name="address">The address to populate.</param>
        /// <param name="hostName">The host name to resolve.</param>
        /// <param name="port">The port number.</param>
        /// <param name="scopeId">The Ipv6 scope identifier.</param>
        /// <returns>0 on success, -1 on failure.</returns>
        public static int enet_address_set_hostname_ipv6(ENetAddress* address, ReadOnlySpan<char> hostName, ushort port, uint scopeId = 0) => address->SetHostNameIpv6(hostName, port, scopeId) == SocketError.Success ? 0 : -1;

        /// <summary>
        ///     Retrieves the IP address of an ENet address as a character span.
        /// </summary>
        /// <param name="address">The address to query.</param>
        /// <param name="ip">Receives the address characters.</param>
        /// <returns>0 on success, -1 on failure.</returns>
        public static int enet_address_get_ip(ENetAddress* address, ref Span<char> ip) => address->GetIp(ref ip) == SocketError.Success ? 0 : -1;

        /// <summary>
        ///     Retrieves the host name (reverse DNS) of an ENet address.
        /// </summary>
        /// <param name="address">The address to query.</param>
        /// <param name="hostName">Receives the host name characters.</param>
        /// <returns>0 on success, -1 on failure.</returns>
        public static int enet_address_get_hostname(ENetAddress* address, ref Span<char> hostName) => address->GetHostName(ref hostName) == SocketError.Success ? 0 : -1;
    }
}