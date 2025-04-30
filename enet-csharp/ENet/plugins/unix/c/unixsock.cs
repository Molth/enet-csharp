using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using winsock;

#pragma warning disable CS1591
#pragma warning disable CS8981
#pragma warning disable SYSLIB1054

// ReSharper disable ALL

namespace unixsock
{
    public static unsafe class UnixSock
    {
        private const string NATIVE_LIBRARY = "libc";
        public const ushort ADDRESS_FAMILY_INTER_NETWORK_V6 = 10;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError GetLastSocketError() => (SocketError)Marshal.GetLastWin32Error();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError Initialize() => SocketError.Success;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError Cleanup() => SocketError.Success;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint Create()
        {
            var _socket = socket((int)ADDRESS_FAMILY_INTER_NETWORK_V6, (int)SocketType.Dgram, 0);
            if (_socket != -1)
            {
                var optionValue = 0;
                var errorCode = SetOption(_socket, SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, &optionValue);
                if (errorCode != SocketError.Success)
                {
                    Close(_socket);
                    _socket = -1;
                }
            }

            return _socket;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError Close(nint socket)
        {
            var errorCode = close(socket);
            return errorCode;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError Bind(nint socket, sockaddr_in6* socketAddress)
        {
            var buffer = stackalloc byte[28];
            var __socketAddress_native = (sockaddr_in6*)buffer;

            if (socketAddress == null)
                SetIP(__socketAddress_native, "::");
            else
            {
                if (socketAddress->sin6_family != (int)ADDRESS_FAMILY_INTER_NETWORK_V6)
                    return SocketError.InvalidArgument;

                Unsafe.CopyBlockUnaligned(__socketAddress_native, socketAddress, 28);
                __socketAddress_native->sin6_port = WinSock2.HOST_TO_NET_16(socketAddress->sin6_port);
            }

            var errorCode = bind(socket, (sockaddr*)__socketAddress_native, 28);
            return errorCode;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError Connect(nint socket, sockaddr_in6* socketAddress)
        {
            var errorCode = connect(socket, (sockaddr*)socketAddress, 28);
            return errorCode;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError SetOption(nint socket, SocketOptionLevel optionLevel, SocketOptionName optionName, int* optionValue)
        {
            var errorCode = setsockopt(socket, optionLevel, optionName, optionValue, sizeof(int));
            return errorCode == SocketError.SocketError ? GetLastSocketError() : SocketError.Success;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError GetOption(nint socket, SocketOptionLevel level, SocketOptionName optionName, int* optionValue)
        {
            var optionLength = 4;
            var errorCode = getsockopt(socket, (int)level, (int)optionName, (byte*)optionValue, &optionLength);
            return errorCode == SocketError.SocketError ? GetLastSocketError() : SocketError.Success;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError SetBlocking(nint socket, bool shouldBlock)
        {
            SocketError errorCode;
            int flags = fcntl(socket, 4, 0);
            if (flags == -1)
            {
                errorCode = (SocketError)Marshal.GetLastWin32Error();
            }
            else
            {
                flags = shouldBlock ? flags & ~2048 : flags | 2048;
                errorCode = (SocketError)fcntl(socket, 4, flags);
            }

            return errorCode;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError Poll(nint socket, int microseconds, SelectMode mode, out bool status)
        {
            var fileDescriptorSet = stackalloc nint[2] { 1, socket };
            TimeValue timeout = default;
            int socketCount;
            if (microseconds != -1)
            {
                MicrosecondsToTimeValue((uint)microseconds, ref timeout);
                socketCount = select(0, mode == SelectMode.SelectRead ? fileDescriptorSet : null, mode == SelectMode.SelectWrite ? fileDescriptorSet : null, mode == SelectMode.SelectError ? fileDescriptorSet : null, &timeout);
            }
            else
            {
                socketCount = select(0, mode == SelectMode.SelectRead ? fileDescriptorSet : null, mode == SelectMode.SelectWrite ? fileDescriptorSet : null, mode == SelectMode.SelectError ? fileDescriptorSet : null, null);
            }

            if ((SocketError)socketCount == SocketError.SocketError)
            {
                status = false;
                return GetLastSocketError();
            }

            status = (int)fileDescriptorSet[0] != 0 && fileDescriptorSet[1] == socket;
            return SocketError.Success;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SendTo(nint socket, void* buffer, int length, sockaddr_in6* socketAddress)
        {
            if (socketAddress != null)
                socketAddress->sin6_port = WinSock2.HOST_TO_NET_16(socketAddress->sin6_port);

            var num = sendto(socket, (byte*)buffer, length, SocketFlags.None, (byte*)socketAddress, socketAddress != null ? 28 : 0);
            return num;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReceiveFrom(nint socket, void* buffer, int length, sockaddr_in6* socketAddress)
        {
            var socketAddressSize = 28;
            var num = recvfrom(socket, (byte*)buffer, length, SocketFlags.None, (byte*)socketAddress, &socketAddressSize);
            if (socketAddress != null)
                socketAddress->sin6_port = WinSock2.NET_TO_HOST_16(socketAddress->sin6_port);

            return num;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError GetName(nint socket, sockaddr_in6* socketAddress)
        {
            var sockaddr = new sockaddr_storage();
            var byteCount = 128;
            var errorCode = getsockname(socket, (sockaddr*)&sockaddr, &byteCount);
            if (errorCode == SocketError.Success)
            {
                if (sockaddr.ss_family == (int)AddressFamily.InterNetwork)
                {
                    Unsafe.WriteUnaligned(socketAddress->sin6_addr + 10, (ushort)0xFFFF);
                    Unsafe.CopyBlockUnaligned(socketAddress->sin6_addr + 12, &sockaddr, 4);
                    socketAddress->sin6_port = WinSock2.NET_TO_HOST_16(((sockaddr_in*)&sockaddr)->sin_port);
                }
                else if (sockaddr.ss_family == (int)ADDRESS_FAMILY_INTER_NETWORK_V6)
                {
                    Unsafe.CopyBlockUnaligned(socketAddress->sin6_addr, &sockaddr, 16);
                    socketAddress->sin6_port = WinSock2.NET_TO_HOST_16(((sockaddr_in6*)&sockaddr)->sin6_port);
                }
            }

            return errorCode;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError SetIP(void* pAddrBuf, ReadOnlySpan<char> ip)
        {
            var byteCount = Encoding.ASCII.GetByteCount(ip);
            Span<byte> buffer = stackalloc byte[byteCount];
            Encoding.ASCII.GetBytes(ip, buffer);

            var addressFamily = (int)ADDRESS_FAMILY_INTER_NETWORK_V6;
            if (ip.IndexOf(':') < 0)
            {
                addressFamily = (int)AddressFamily.InterNetwork;
                Unsafe.InitBlockUnaligned(pAddrBuf, 0, 10);
                Unsafe.WriteUnaligned((byte*)pAddrBuf + 10, (ushort)0xFFFF);
                pAddrBuf = (byte*)pAddrBuf + 12;
            }

            var error = inet_pton(addressFamily, Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer)), (byte*)pAddrBuf);

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
        public static SocketError GetIP(void* pAddrBuf, Span<byte> buffer)
        {
            ref var reference = ref Unsafe.As<byte, int>(ref MemoryMarshal.GetReference(buffer));
            if (Unsafe.ReadUnaligned<ushort>((byte*)pAddrBuf + 10) == 0xFFFF && reference == 0 && Unsafe.Add(ref reference, 1) == 0 && Unsafe.Add(ref reference, 2) == 0)
            {
                if (inet_ntop((int)AddressFamily.InterNetwork, (byte*)pAddrBuf + 12, (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer)), (nuint)buffer.Length) == null)
                    return SocketError.Fault;
            }
            else if (inet_ntop((int)ADDRESS_FAMILY_INTER_NETWORK_V6, pAddrBuf, (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer)), (nuint)buffer.Length) == null)
            {
                return SocketError.Fault;
            }

            return SocketError.Success;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError SetHostName(void* pAddrBuf, ReadOnlySpan<char> hostName)
        {
            var byteCount = Encoding.ASCII.GetByteCount(hostName);
            Span<byte> buffer = stackalloc byte[byteCount];
            Encoding.ASCII.GetBytes(hostName, buffer);

            var addressInfo = new addrinfo();
            addrinfo* result, resultList = null;

            if (getaddrinfo((byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer)), null, &addressInfo, &resultList) != 0)
                return SocketError.Fault;

            for (result = resultList; result != null; result = result->ai_next)
            {
                if (result->ai_addr != null && result->ai_addrlen >= (nuint)sizeof(sockaddr_in))
                {
                    if (result->ai_family == (int)AddressFamily.InterNetwork)
                    {
                        var socketAddress = (sockaddr_in*)result->ai_addr;

                        Unsafe.InitBlockUnaligned(pAddrBuf, 0, 10);
                        Unsafe.WriteUnaligned((byte*)pAddrBuf + 10, (ushort)0xFFFF);
                        Unsafe.WriteUnaligned((byte*)pAddrBuf + 12, socketAddress->sin_addr.S_addr);

                        freeaddrinfo(resultList);

                        return 0;
                    }

                    if (result->ai_family == (int)ADDRESS_FAMILY_INTER_NETWORK_V6)
                    {
                        var socketAddress = (sockaddr_in6*)result->ai_addr;

                        Unsafe.CopyBlockUnaligned(pAddrBuf, socketAddress->sin6_addr, 16);

                        freeaddrinfo(resultList);

                        return 0;
                    }
                }
            }

            if (resultList != null)
                freeaddrinfo(resultList);

            var addressFamily = (int)ADDRESS_FAMILY_INTER_NETWORK_V6;
            if (buffer.IndexOf((byte)':') == -1)
            {
                addressFamily = (int)AddressFamily.InterNetwork;
                Unsafe.InitBlockUnaligned(pAddrBuf, 0, 10);
                Unsafe.WriteUnaligned((byte*)pAddrBuf + 10, (ushort)0xFFFF);
            }

            var error = inet_pton(addressFamily, Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer)), (byte*)pAddrBuf + 12);

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
        public static SocketError GetHostName(sockaddr_in6* address, Span<byte> buffer)
        {
            sockaddr_in6 socketAddress;

            socketAddress.sin6_family = (ushort)ADDRESS_FAMILY_INTER_NETWORK_V6;
            socketAddress.sin6_port = WinSock2.HOST_TO_NET_16(address->sin6_port);
            socketAddress.sin6_flowinfo = 0;
            Unsafe.CopyBlockUnaligned(socketAddress.sin6_addr, address->sin6_addr, 16);
            socketAddress.sin6_scope_id = 0;

            var error = getnameinfo((sockaddr*)&socketAddress, sizeof(sockaddr_in6), (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer)), (ulong)buffer.Length, null, 0, 0x4);

            if (error == 0)
            {
                if (Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer)) != null && buffer.Length > 0 && buffer.IndexOf((byte)'\0') < 0)
                    return SocketError.Fault;

                return SocketError.Success;
            }

            if (error != 0x2AF9L)
                return SocketError.Fault;

            return GetIP(address->sin6_addr, buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void MicrosecondsToTimeValue(long microseconds, ref TimeValue socketTime)
        {
            const int microcnv = 1000000;
            socketTime.Seconds = (int)(microseconds / microcnv);
            socketTime.Microseconds = (int)(microseconds % microcnv);
        }

        [DllImport(NATIVE_LIBRARY)]
        private static extern SocketError bind(nint __socketHandle_native, sockaddr* __socketAddress_native, int __socketAddressSize_native);

        [DllImport(NATIVE_LIBRARY)]
        private static extern SocketError getsockname(nint __socketHandle_native, sockaddr* __socketAddress_native, int* __socketAddressSize_native);

        [DllImport(NATIVE_LIBRARY)]
        private static extern nint socket(int af, int type, int protocol);

        [DllImport(NATIVE_LIBRARY)]
        private static extern int fcntl(nint fd, int cmd, int arg);

        [DllImport(NATIVE_LIBRARY)]
        private static extern SocketError setsockopt(nint __socketHandle_native, SocketOptionLevel __optionLevel_native, SocketOptionName __optionName_native, int* __optionValue_native, int __optionLength_native);

        [DllImport(NATIVE_LIBRARY)]
        private static extern SocketError getsockopt(nint s, int level, int optname, byte* optval, int* optlen);

        [DllImport(NATIVE_LIBRARY)]
        private static extern SocketError connect(nint s, sockaddr* name, int namelen);

        [DllImport(NATIVE_LIBRARY)]
        private static extern SocketError close(nint __socketHandle_native);

        [DllImport(NATIVE_LIBRARY)]
        private static extern int sendto(nint __socketHandle_native, byte* __pinnedBuffer_native, int __len_native, SocketFlags __socketFlags_native, byte* __socketAddress_native, int __socketAddressSize_native);

        [DllImport(NATIVE_LIBRARY)]
        private static extern int recvfrom(nint __socketHandle_native, byte* __pinnedBuffer_native, int __len_native, SocketFlags __socketFlags_native, byte* __socketAddress_native, int* __socketAddressSize_native);

        [DllImport(NATIVE_LIBRARY)]
        private static extern int select(int __ignoredParameter_native, nint* __readfds_native, nint* __writefds_native, nint* __exceptfds_native, TimeValue* __timeout_native);

        [DllImport(NATIVE_LIBRARY)]
        private static extern int inet_pton(int Family, void* pszAddrString, void* pAddrBuf);

        [DllImport(NATIVE_LIBRARY)]
        private static extern int getaddrinfo(byte* pNodeName, byte* pServiceName, addrinfo* pHints, addrinfo** ppResult);

        [DllImport(NATIVE_LIBRARY)]
        private static extern void freeaddrinfo(addrinfo* pAddrInfo);

        [DllImport(NATIVE_LIBRARY)]
        private static extern byte* inet_ntop(int Family, void* pAddr, byte* pStringBuf, nuint StringBufSize);

        [DllImport(NATIVE_LIBRARY)]
        private static extern int getnameinfo(sockaddr* pSockaddr, int SockaddrLength, byte* pNodeBuffer, ulong NodeBufferSize, byte* pServiceBuffer, ulong ServiceBufferSize, int Flags);

        [StructLayout(LayoutKind.Sequential)]
        private struct TimeValue
        {
            public int Seconds;
            public int Microseconds;
        }
    }
}