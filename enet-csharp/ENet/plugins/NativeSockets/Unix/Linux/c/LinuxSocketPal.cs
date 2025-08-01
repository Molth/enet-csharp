using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using static NativeSockets.UnixSocketPal;

#pragma warning disable CA1401
#pragma warning disable CS1591
#pragma warning disable CS8981
#pragma warning disable SYSLIB1054

// ReSharper disable ALL

namespace NativeSockets
{
    [SuppressUnmanagedCodeSecurity]
    public static unsafe class LinuxSocketPal
    {
        public const ushort ADDRESS_FAMILY_INTER_NETWORK_V6 = 10;

        public static bool IsSupported { get; } = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError GetLastSocketError() => UnixSocketPal.GetLastSocketError();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError Initialize() => SocketError.Success;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError Cleanup() => SocketError.Success;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint Create(bool ipv6)
        {
            int family = ipv6 ? ADDRESS_FAMILY_INTER_NETWORK_V6 : (int)AddressFamily.InterNetwork;
            nint _socket = socket(family, (int)SocketType.Dgram, 0);
            return _socket;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError Close(nint socket)
        {
            SocketError errorCode = close((int)socket);
            return errorCode;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError SetDualMode6(nint socket, bool dualMode)
        {
            int optionValue = dualMode ? 0 : 1;
            SocketError errorCode = SetOption(socket, SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, &optionValue);
            return errorCode;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError Bind4(nint socket, sockaddr_in* socketAddress)
        {
            sockaddr_in __socketAddress_native;
            if (socketAddress == null)
            {
                __socketAddress_native = new sockaddr_in();
                __socketAddress_native.sin_family = (ushort)AddressFamily.InterNetwork;
                SetIP4(&__socketAddress_native, "0.0.0.0");
            }
            else
            {
                __socketAddress_native = *socketAddress;
                __socketAddress_native.sin_port = WinSock2.HOST_TO_NET_16(socketAddress->sin_port);
            }

            SocketError errorCode = bind((int)socket, (sockaddr*)&__socketAddress_native, sizeof(sockaddr_in));
            return errorCode;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError Bind6(nint socket, sockaddr_in6* socketAddress)
        {
            sockaddr_in6 __socketAddress_native;
            if (socketAddress == null)
            {
                __socketAddress_native = new sockaddr_in6();
                __socketAddress_native.sin6_family = ADDRESS_FAMILY_INTER_NETWORK_V6;
                SetIP6(&__socketAddress_native, "::");
            }
            else
            {
                __socketAddress_native = *socketAddress;
                __socketAddress_native.sin6_port = WinSock2.HOST_TO_NET_16(socketAddress->sin6_port);
            }

            SocketError errorCode = bind((int)socket, (sockaddr*)&__socketAddress_native, sizeof(sockaddr_in6));
            return errorCode;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError Connect4(nint socket, sockaddr_in* socketAddress)
        {
            sockaddr_in __socketAddress_native = *socketAddress;
            __socketAddress_native.sin_port = WinSock2.HOST_TO_NET_16(socketAddress->sin_port);

            SocketError errorCode = connect((int)socket, (sockaddr*)&__socketAddress_native, sizeof(sockaddr_in));
            return errorCode;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError Connect6(nint socket, sockaddr_in6* socketAddress)
        {
            sockaddr_in6 __socketAddress_native = *socketAddress;
            __socketAddress_native.sin6_port = WinSock2.HOST_TO_NET_16(socketAddress->sin6_port);

            SocketError errorCode = connect((int)socket, (sockaddr*)&__socketAddress_native, sizeof(sockaddr_in6));
            return errorCode;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError SetOption(nint socket, SocketOptionLevel optionLevel, SocketOptionName optionName, int* optionValue, int optionLength = sizeof(int))
        {
            SocketError errorCode = setsockopt((int)socket, optionLevel, optionName, optionValue, optionLength);
            return errorCode == SocketError.SocketError ? GetLastSocketError() : SocketError.Success;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError GetOption(nint socket, SocketOptionLevel level, SocketOptionName optionName, int* optionValue, int* optionLength = null)
        {
            int num = sizeof(int);
            if (optionLength == null)
                optionLength = &num;

            SocketError errorCode = getsockopt((int)socket, (int)level, (int)optionName, (byte*)optionValue, optionLength);
            return errorCode == SocketError.SocketError ? GetLastSocketError() : SocketError.Success;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError SetBlocking(nint socket, bool shouldBlock)
        {
            int flags = fcntl((int)socket, 3, 0);
            if (flags == -1)
                return GetLastSocketError();

            flags = shouldBlock ? flags & ~2048 : flags | 2048;
            if (fcntl((int)socket, 4, flags) == -1)
                return GetLastSocketError();

            return SocketError.Success;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError Poll(nint socket, int microseconds, SelectMode mode, out bool status)
        {
            PollEvents inEvent = 0;
            switch (mode)
            {
                case SelectMode.SelectRead:
                    inEvent = PollEvents.POLLIN;
                    break;
                case SelectMode.SelectWrite:
                    inEvent = PollEvents.POLLOUT;
                    break;
                case SelectMode.SelectError:
                    inEvent = PollEvents.POLLPRI;
                    break;
            }

            int milliseconds = microseconds == -1 ? -1 : microseconds / 1000;

            pollfd fd;
            fd.fd = (int)socket;
            fd.events = (short)inEvent;
            fd.revents = 0;

            int errno = poll(&fd, 1, milliseconds);
            if (errno == -1)
            {
                status = false;
                return GetLastSocketError();
            }

            PollEvents outEvents = (PollEvents)fd.revents;
            switch (mode)
            {
                case SelectMode.SelectRead:
                    status = (outEvents & (PollEvents.POLLIN | PollEvents.POLLHUP)) != 0;
                    break;
                case SelectMode.SelectWrite:
                    status = (outEvents & PollEvents.POLLOUT) != 0;
                    break;
                case SelectMode.SelectError:
                    status = (outEvents & (PollEvents.POLLERR | PollEvents.POLLPRI)) != 0;
                    break;
                default:
                    status = false;
                    break;
            }

            return SocketError.Success;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Send(nint socket, void* buffer, int length)
        {
            int num = send((int)socket, (byte*)buffer, length, SocketFlags.None);
            return num;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SendTo4(nint socket, void* buffer, int length, sockaddr_in* socketAddress)
        {
            if (socketAddress != null)
            {
                sockaddr_in __socketAddress_native = *socketAddress;
                __socketAddress_native.sin_port = WinSock2.HOST_TO_NET_16(socketAddress->sin_port);
                return sendto((int)socket, (byte*)buffer, length, SocketFlags.None, (byte*)&__socketAddress_native, sizeof(sockaddr_in));
            }

            int num = send((int)socket, (byte*)buffer, length, SocketFlags.None);
            return num;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SendTo6(nint socket, void* buffer, int length, sockaddr_in6* socketAddress)
        {
            if (socketAddress != null)
            {
                sockaddr_in6 __socketAddress_native = *socketAddress;
                __socketAddress_native.sin6_port = WinSock2.HOST_TO_NET_16(socketAddress->sin6_port);
                return sendto((int)socket, (byte*)buffer, length, SocketFlags.None, (byte*)&__socketAddress_native, sizeof(sockaddr_in6));
            }

            int num = send((int)socket, (byte*)buffer, length, SocketFlags.None);
            return num;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Receive(nint socket, void* buffer, int length)
        {
            int num = recv((int)socket, (byte*)buffer, length, SocketFlags.None);
            return num;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReceiveFrom4(nint socket, void* buffer, int length, sockaddr_in* socketAddress)
        {
            sockaddr_storage addressStorage = new sockaddr_storage();
            int socketAddressSize = sizeof(sockaddr_storage);

            int num = recvfrom((int)socket, (byte*)buffer, length, SocketFlags.None, (byte*)&addressStorage, &socketAddressSize);

            if (num > 0 && socketAddress != null)
            {
                sockaddr_in* __socketAddress_native = (sockaddr_in*)&addressStorage;
                *socketAddress = *__socketAddress_native;
                socketAddress->sin_port = WinSock2.NET_TO_HOST_16(__socketAddress_native->sin_port);
            }

            return num;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReceiveFrom6(nint socket, void* buffer, int length, sockaddr_in6* socketAddress)
        {
            sockaddr_storage addressStorage = new sockaddr_storage();
            int socketAddressSize = sizeof(sockaddr_storage);

            int num = recvfrom((int)socket, (byte*)buffer, length, SocketFlags.None, (byte*)&addressStorage, &socketAddressSize);

            if (num > 0 && socketAddress != null)
            {
                socketAddress->sin6_family.family = ADDRESS_FAMILY_INTER_NETWORK_V6;
                if (addressStorage.ss_family.family == (int)AddressFamily.InterNetwork)
                {
                    sockaddr_in* __socketAddress_native = (sockaddr_in*)&addressStorage;
                    Unsafe.InitBlockUnaligned(socketAddress->sin6_addr, 0, 8);
                    Unsafe.WriteUnaligned(socketAddress->sin6_addr + 8, WinSock2.ADDRESS_FAMILY_INTER_NETWORK_V4_MAPPED_V6);
                    Unsafe.WriteUnaligned(socketAddress->sin6_addr + 12, __socketAddress_native->sin_addr);
                    socketAddress->sin6_port = WinSock2.NET_TO_HOST_16(__socketAddress_native->sin_port);
                    socketAddress->sin6_scope_id = 0;
                }
                else if (addressStorage.ss_family.family == (int)ADDRESS_FAMILY_INTER_NETWORK_V6)
                {
                    sockaddr_in6* __socketAddress_native = (sockaddr_in6*)&addressStorage;
                    Unsafe.CopyBlockUnaligned(socketAddress->sin6_addr, __socketAddress_native->sin6_addr, 20);
                    socketAddress->sin6_port = WinSock2.NET_TO_HOST_16(__socketAddress_native->sin6_port);
                }
            }

            return num;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WSASend(nint socket, WSABuffer* buffers, int bufferCount)
        {
            iovec* msg_iov = stackalloc iovec[bufferCount];
            for (int i = 0; i < bufferCount; ++i)
            {
                msg_iov[i].iov_base = buffers[i].Pointer;
                msg_iov[i].iov_len = buffers[i].Length;
            }

            msghdr msg;
            msg.msg_name = null;
            msg.msg_namelen = 0;
            msg.msg_iov = msg_iov;
            msg.msg_iovlen = bufferCount;
            msg.msg_control = null;
            msg.msg_controllen = 0;
            msg.msg_flags = 0;

            int num = (int)sendmsg((int)socket, &msg, 0);
            return num;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WSASendTo4(nint socket, WSABuffer* buffers, int bufferCount, sockaddr_in* socketAddress)
        {
            iovec* msg_iov = stackalloc iovec[bufferCount];
            for (int i = 0; i < bufferCount; ++i)
            {
                msg_iov[i].iov_base = buffers[i].Pointer;
                msg_iov[i].iov_len = buffers[i].Length;
            }

            sockaddr_in __socketAddress_native;

            msghdr msg;
            msg.msg_iov = msg_iov;
            msg.msg_iovlen = bufferCount;
            msg.msg_control = null;
            msg.msg_controllen = 0;
            msg.msg_flags = 0;

            if (socketAddress != null)
            {
                __socketAddress_native = *socketAddress;
                __socketAddress_native.sin_port = WinSock2.HOST_TO_NET_16(socketAddress->sin_port);

                msg.msg_name = &__socketAddress_native;
                msg.msg_namelen = 16;
            }
            else
            {
                msg.msg_name = null;
                msg.msg_namelen = 0;
            }

            int num = (int)sendmsg((int)socket, &msg, 0);
            return num;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WSASendTo6(nint socket, WSABuffer* buffers, int bufferCount, sockaddr_in6* socketAddress)
        {
            iovec* msg_iov = stackalloc iovec[bufferCount];
            for (int i = 0; i < bufferCount; ++i)
            {
                msg_iov[i].iov_base = buffers[i].Pointer;
                msg_iov[i].iov_len = buffers[i].Length;
            }

            sockaddr_in6 __socketAddress_native;

            msghdr msg;
            msg.msg_iov = msg_iov;
            msg.msg_iovlen = bufferCount;
            msg.msg_control = null;
            msg.msg_controllen = 0;
            msg.msg_flags = 0;

            if (socketAddress != null)
            {
                __socketAddress_native = *socketAddress;
                __socketAddress_native.sin6_port = WinSock2.HOST_TO_NET_16(socketAddress->sin6_port);

                msg.msg_name = &__socketAddress_native;
                msg.msg_namelen = 28;
            }
            else
            {
                msg.msg_name = null;
                msg.msg_namelen = 0;
            }

            int num = (int)sendmsg((int)socket, &msg, 0);
            return num;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WSAReceive(nint socket, WSABuffer* buffers, int bufferCount)
        {
            iovec* msg_iov = stackalloc iovec[bufferCount];
            for (int i = 0; i < bufferCount; ++i)
            {
                msg_iov[i].iov_base = buffers[i].Pointer;
                msg_iov[i].iov_len = buffers[i].Length;
            }

            msghdr msg;
            msg.msg_name = null;
            msg.msg_namelen = 0;
            msg.msg_iov = msg_iov;
            msg.msg_iovlen = bufferCount;
            msg.msg_control = null;
            msg.msg_controllen = 0;
            msg.msg_flags = 0;

            int num = (int)recvmsg((int)socket, &msg, 0);

            if (msg.msg_flags != 0)
                return -1;

            return num;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WSAReceiveFrom4(nint socket, WSABuffer* buffers, int bufferCount, sockaddr_in* socketAddress)
        {
            iovec* msg_iov = stackalloc iovec[bufferCount];
            for (int i = 0; i < bufferCount; ++i)
            {
                msg_iov[i].iov_base = buffers[i].Pointer;
                msg_iov[i].iov_len = buffers[i].Length;
            }

            sockaddr_storage addressStorage = new sockaddr_storage();

            msghdr msg;
            msg.msg_name = &addressStorage;
            msg.msg_namelen = (nuint)sizeof(sockaddr_storage);
            msg.msg_iov = msg_iov;
            msg.msg_iovlen = bufferCount;
            msg.msg_control = null;
            msg.msg_controllen = 0;
            msg.msg_flags = 0;

            int num = (int)recvmsg((int)socket, &msg, 0);

            if (msg.msg_flags != 0)
                return -1;

            if (num > 0 && socketAddress != null)
            {
                sockaddr_in* __socketAddress_native = (sockaddr_in*)&addressStorage;
                *socketAddress = *__socketAddress_native;
                socketAddress->sin_port = WinSock2.NET_TO_HOST_16(__socketAddress_native->sin_port);
            }

            return num;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WSAReceiveFrom6(nint socket, WSABuffer* buffers, int bufferCount, sockaddr_in6* socketAddress)
        {
            iovec* msg_iov = stackalloc iovec[bufferCount];
            for (int i = 0; i < bufferCount; ++i)
            {
                msg_iov[i].iov_base = buffers[i].Pointer;
                msg_iov[i].iov_len = buffers[i].Length;
            }

            sockaddr_storage addressStorage = new sockaddr_storage();

            msghdr msg;
            msg.msg_name = &addressStorage;
            msg.msg_namelen = (nuint)sizeof(sockaddr_storage);
            msg.msg_iov = msg_iov;
            msg.msg_iovlen = bufferCount;
            msg.msg_control = null;
            msg.msg_controllen = 0;
            msg.msg_flags = 0;

            int num = (int)recvmsg((int)socket, &msg, 0);

            if (msg.msg_flags != 0)
                return -1;

            if (num > 0 && socketAddress != null)
            {
                socketAddress->sin6_family.family = ADDRESS_FAMILY_INTER_NETWORK_V6;
                if (addressStorage.ss_family.family == (int)AddressFamily.InterNetwork)
                {
                    sockaddr_in* __socketAddress_native = (sockaddr_in*)&addressStorage;
                    Unsafe.InitBlockUnaligned(socketAddress->sin6_addr, 0, 8);
                    Unsafe.WriteUnaligned(socketAddress->sin6_addr + 8, WinSock2.ADDRESS_FAMILY_INTER_NETWORK_V4_MAPPED_V6);
                    Unsafe.WriteUnaligned(socketAddress->sin6_addr + 12, __socketAddress_native->sin_addr);
                    socketAddress->sin6_port = WinSock2.NET_TO_HOST_16(__socketAddress_native->sin_port);
                    socketAddress->sin6_scope_id = 0;
                }
                else if (addressStorage.ss_family.family == (int)ADDRESS_FAMILY_INTER_NETWORK_V6)
                {
                    sockaddr_in6* __socketAddress_native = (sockaddr_in6*)&addressStorage;
                    Unsafe.CopyBlockUnaligned(socketAddress->sin6_addr, __socketAddress_native->sin6_addr, 20);
                    socketAddress->sin6_port = WinSock2.NET_TO_HOST_16(__socketAddress_native->sin6_port);
                }
            }

            return num;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError GetName4(nint socket, sockaddr_in* socketAddress)
        {
            sockaddr_storage addressStorage = new sockaddr_storage();
            int socketAddressSize = sizeof(sockaddr_storage);
            SocketError errorCode = getsockname((int)socket, (sockaddr*)&addressStorage, &socketAddressSize);
            if (errorCode == SocketError.Success)
            {
                socketAddress->sin_family = addressStorage.ss_family;
                sockaddr_in* __socketAddress_native = (sockaddr_in*)&addressStorage;
                *socketAddress = *__socketAddress_native;
                socketAddress->sin_port = WinSock2.NET_TO_HOST_16(__socketAddress_native->sin_port);
            }

            return errorCode;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError GetName6(nint socket, sockaddr_in6* socketAddress)
        {
            sockaddr_storage addressStorage = new sockaddr_storage();
            int socketAddressSize = sizeof(sockaddr_storage);
            SocketError errorCode = getsockname((int)socket, (sockaddr*)&addressStorage, &socketAddressSize);
            if (errorCode == SocketError.Success)
            {
                socketAddress->sin6_family = addressStorage.ss_family;
                if (addressStorage.ss_family.family == (int)AddressFamily.InterNetwork)
                {
                    sockaddr_in* __socketAddress_native = (sockaddr_in*)&addressStorage;
                    Unsafe.InitBlockUnaligned(socketAddress->sin6_addr, 0, 8);
                    Unsafe.WriteUnaligned(socketAddress->sin6_addr + 8, WinSock2.ADDRESS_FAMILY_INTER_NETWORK_V4_MAPPED_V6);
                    Unsafe.WriteUnaligned(socketAddress->sin6_addr + 12, __socketAddress_native->sin_addr);
                    socketAddress->sin6_port = WinSock2.NET_TO_HOST_16(__socketAddress_native->sin_port);
                }
                else if (addressStorage.ss_family.family == (int)ADDRESS_FAMILY_INTER_NETWORK_V6)
                {
                    sockaddr_in6* __socketAddress_native = (sockaddr_in6*)&addressStorage;
                    Unsafe.CopyBlockUnaligned(socketAddress->sin6_addr, __socketAddress_native->sin6_addr, 16);
                    socketAddress->sin6_port = WinSock2.NET_TO_HOST_16(__socketAddress_native->sin6_port);
                }
            }

            return errorCode;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError SetIP4(sockaddr_in* socketAddress, ReadOnlySpan<char> ip)
        {
            void* pAddrBuf = &socketAddress->sin_addr;

            int byteCount = Encoding.ASCII.GetByteCount(ip);
            Span<byte> buffer = stackalloc byte[byteCount + 1];
            Encoding.ASCII.GetBytes(ip, buffer);
            buffer[byteCount] = 0;

            int addressFamily = (int)AddressFamily.InterNetwork;

            int error = inet_pton(addressFamily, Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer)), pAddrBuf);

            switch (error)
            {
                case 1:
                    return SocketError.Success;
                case 0:
                    return SocketError.InvalidArgument;
                default:
                    return SocketError.Fault;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError SetIP6(sockaddr_in6* socketAddress, ReadOnlySpan<char> ip)
        {
            void* pAddrBuf = socketAddress->sin6_addr;

            int byteCount = Encoding.ASCII.GetByteCount(ip);
            Span<byte> buffer = stackalloc byte[byteCount + 1];
            Encoding.ASCII.GetBytes(ip, buffer);
            buffer[byteCount] = 0;

            int addressFamily = (int)ADDRESS_FAMILY_INTER_NETWORK_V6;
            if (ip.IndexOf(':') < 0)
            {
                addressFamily = (int)AddressFamily.InterNetwork;
                Unsafe.InitBlockUnaligned(pAddrBuf, 0, 8);
                Unsafe.WriteUnaligned((byte*)pAddrBuf + 8, WinSock2.ADDRESS_FAMILY_INTER_NETWORK_V4_MAPPED_V6);
                pAddrBuf = (byte*)pAddrBuf + 12;
            }

            int error = inet_pton(addressFamily, Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer)), pAddrBuf);

            switch (error)
            {
                case 1:
                    return SocketError.Success;
                case 0:
                    return SocketError.InvalidArgument;
                default:
                    return SocketError.Fault;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError GetIP4(sockaddr_in* socketAddress, Span<byte> buffer)
        {
            void* pAddrBuf = &socketAddress->sin_addr;

            if (inet_ntop((int)AddressFamily.InterNetwork, pAddrBuf, ref MemoryMarshal.GetReference(buffer), (nuint)buffer.Length) == null)
                return SocketError.Fault;

            return SocketError.Success;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError GetIP6(sockaddr_in6* socketAddress, Span<byte> buffer)
        {
            void* pAddrBuf = socketAddress->sin6_addr;

            ref int reference = ref Unsafe.AsRef<int>(pAddrBuf);
            if (Unsafe.Add<int>(ref reference, 2) == WinSock2.ADDRESS_FAMILY_INTER_NETWORK_V4_MAPPED_V6 && reference == 0 && Unsafe.Add(ref reference, 1) == 0)
            {
                if (inet_ntop((int)AddressFamily.InterNetwork, (byte*)pAddrBuf + 12, ref MemoryMarshal.GetReference(buffer), (nuint)buffer.Length) == null)
                    return SocketError.Fault;
            }
            else if (inet_ntop((int)ADDRESS_FAMILY_INTER_NETWORK_V6, pAddrBuf, ref MemoryMarshal.GetReference(buffer), (nuint)buffer.Length) == null)
            {
                return SocketError.Fault;
            }

            return SocketError.Success;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError SetHostName4(sockaddr_in* socketAddress, ReadOnlySpan<char> hostName)
        {
            void* pAddrBuf = &socketAddress->sin_addr;

            int byteCount = Encoding.ASCII.GetByteCount(hostName);
            Span<byte> buffer = stackalloc byte[byteCount + 1];
            Encoding.ASCII.GetBytes(hostName, buffer);
            buffer[byteCount] = 0;

            addrinfo addressInfo = new addrinfo();
            addrinfo* hint, results = null;

            if (getaddrinfo((byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer)), null, &addressInfo, &results) != 0)
                return SocketError.Fault;

            for (hint = results; hint != null; hint = hint->ai_next)
            {
                if (hint->ai_addr != null && hint->ai_addrlen >= (nuint)sizeof(sockaddr_in))
                {
                    if (hint->ai_family == (int)AddressFamily.InterNetwork)
                    {
                        sockaddr_in* __socketAddress_native = (sockaddr_in*)hint->ai_addr;

                        *socketAddress = *__socketAddress_native;
                        socketAddress->sin_port = WinSock2.NET_TO_HOST_16(__socketAddress_native->sin_port);

                        freeaddrinfo(results);

                        return 0;
                    }
                }
            }

            if (results != null)
                freeaddrinfo(results);

            const int addressFamily = (int)AddressFamily.InterNetwork;

            int error = inet_pton(addressFamily, Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer)), pAddrBuf);

            switch (error)
            {
                case 1:
                    return SocketError.Success;
                case 0:
                    return SocketError.InvalidArgument;
                default:
                    return SocketError.Fault;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError SetHostName6(sockaddr_in6* socketAddress, ReadOnlySpan<char> hostName)
        {
            void* pAddrBuf = socketAddress->sin6_addr;

            int byteCount = Encoding.ASCII.GetByteCount(hostName);
            Span<byte> buffer = stackalloc byte[byteCount + 1];
            Encoding.ASCII.GetBytes(hostName, buffer);
            buffer[byteCount] = 0;

            addrinfo addressInfo = new addrinfo();
            addrinfo* hint, results = null;

            if (getaddrinfo((byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer)), null, &addressInfo, &results) != 0)
                return SocketError.Fault;

            for (hint = results; hint != null; hint = hint->ai_next)
            {
                if (hint->ai_addr != null && hint->ai_addrlen >= (nuint)sizeof(sockaddr_in))
                {
                    if (hint->ai_family == (int)AddressFamily.InterNetwork)
                    {
                        sockaddr_in* __socketAddress_native = (sockaddr_in*)hint->ai_addr;

                        Unsafe.InitBlockUnaligned(pAddrBuf, 0, 8);
                        Unsafe.WriteUnaligned((byte*)pAddrBuf + 8, WinSock2.ADDRESS_FAMILY_INTER_NETWORK_V4_MAPPED_V6);
                        Unsafe.WriteUnaligned((byte*)pAddrBuf + 12, __socketAddress_native->sin_addr.S_addr);

                        freeaddrinfo(results);

                        return 0;
                    }

                    if (hint->ai_family == (int)ADDRESS_FAMILY_INTER_NETWORK_V6)
                    {
                        sockaddr_in6* __socketAddress_native = (sockaddr_in6*)hint->ai_addr;

                        Unsafe.CopyBlockUnaligned(pAddrBuf, __socketAddress_native->sin6_addr, 16);

                        freeaddrinfo(results);

                        return 0;
                    }
                }
            }

            if (results != null)
                freeaddrinfo(results);

            int addressFamily = (int)ADDRESS_FAMILY_INTER_NETWORK_V6;
            if (buffer.IndexOf((byte)':') == -1)
            {
                addressFamily = (int)AddressFamily.InterNetwork;
                Unsafe.InitBlockUnaligned(pAddrBuf, 0, 8);
                Unsafe.WriteUnaligned((byte*)pAddrBuf + 8, WinSock2.ADDRESS_FAMILY_INTER_NETWORK_V4_MAPPED_V6);
                pAddrBuf = (byte*)pAddrBuf + 12;
            }

            int error = inet_pton(addressFamily, Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer)), pAddrBuf);

            switch (error)
            {
                case 1:
                    return SocketError.Success;
                case 0:
                    return SocketError.InvalidArgument;
                default:
                    return SocketError.Fault;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError GetHostName4(sockaddr_in* socketAddress, Span<byte> buffer)
        {
            sockaddr_in __socketAddress_native = *socketAddress;

            __socketAddress_native.sin_port = WinSock2.HOST_TO_NET_16(socketAddress->sin_port);

            int error = getnameinfo((sockaddr*)&__socketAddress_native, sizeof(sockaddr_in), ref MemoryMarshal.GetReference(buffer), (ulong)buffer.Length, null, 0, 0x4);

            if (error == 0)
            {
                if (Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer)) != null && buffer.Length > 0 && buffer.IndexOf((byte)'\0') < 0)
                    return SocketError.Fault;

                return SocketError.Success;
            }

            if (error != 0x2AF9L)
                return SocketError.Fault;

            return GetIP4(socketAddress, buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError GetHostName6(sockaddr_in6* socketAddress, Span<byte> buffer)
        {
            sockaddr_in6 __socketAddress_native = *socketAddress;

            __socketAddress_native.sin6_port = WinSock2.HOST_TO_NET_16(socketAddress->sin6_port);

            int error = getnameinfo((sockaddr*)&__socketAddress_native, sizeof(sockaddr_in6), ref MemoryMarshal.GetReference(buffer), (ulong)buffer.Length, null, 0, 0x4);

            if (error == 0)
            {
                if (Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer)) != null && buffer.Length > 0 && buffer.IndexOf((byte)'\0') < 0)
                    return SocketError.Fault;

                return SocketError.Success;
            }

            if (error != 0x2AF9L)
                return SocketError.Fault;

            return GetIP6(socketAddress, buffer);
        }
    }
}