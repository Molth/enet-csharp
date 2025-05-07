using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

#pragma warning disable CS1591
#pragma warning disable CS8981
#pragma warning disable SYSLIB1054

// ReSharper disable ALL

namespace winsock
{
    public static unsafe class WinSock
    {
        private const string NATIVE_LIBRARY = "ws2_32.dll";
        public const ushort ADDRESS_FAMILY_INTER_NETWORK_V6 = 23;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError GetLastSocketError() => (SocketError)WSAGetLastError();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError Initialize()
        {
            WSAData wsaData;
            SocketError errorCode = WSAStartup(514, &wsaData);
            return errorCode;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError Cleanup() => WSACleanup();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint Create()
        {
            nint socket = WSASocketW((AddressFamily)ADDRESS_FAMILY_INTER_NETWORK_V6, SocketType.Dgram, ProtocolType.Udp, 0, 0, 1 | 128);
            if (socket != -1)
            {
                int optionValue = 0;
                SocketError errorCode = SetOption(socket, SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, &optionValue);
                if (errorCode != SocketError.Success)
                {
                    Close(socket);
                    socket = -1;
                }

                byte* __inBuffer_native = stackalloc byte[1] { 0x00 };
                int __bytesTransferred_native;
                errorCode = WSAIoctl(socket, -1744830452, __inBuffer_native, 1, null, 0, &__bytesTransferred_native, 0, 0);
                if (errorCode != SocketError.Success)
                {
                    Close(socket);
                    socket = -1;
                }
            }

            return socket;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError Close(nint socket)
        {
            SocketError errorCode = closesocket(socket);
            return errorCode;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError Bind(nint socket, sockaddr_in6* socketAddress)
        {
            sockaddr_in6 __socketAddress_native;
            if (socketAddress == null)
            {
                __socketAddress_native = new sockaddr_in6();
                SetIP(&__socketAddress_native, "::");
            }
            else
            {
                if (socketAddress->sin6_family != (int)ADDRESS_FAMILY_INTER_NETWORK_V6)
                    return SocketError.InvalidArgument;

                __socketAddress_native = *socketAddress;
                __socketAddress_native.sin6_port = WinSock2.HOST_TO_NET_16(socketAddress->sin6_port);
            }

            SocketError errorCode = bind(socket, (sockaddr*)&__socketAddress_native, 28);
            return errorCode;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError Connect(nint socket, sockaddr_in6* socketAddress)
        {
            sockaddr_in6 __socketAddress_native = *socketAddress;
            __socketAddress_native.sin6_port = WinSock2.HOST_TO_NET_16(socketAddress->sin6_port);

            SocketError errorCode = connect(socket, (sockaddr*)&__socketAddress_native, 28);
            return errorCode;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError SetOption(nint socket, SocketOptionLevel optionLevel, SocketOptionName optionName, int* optionValue, int optionLength = sizeof(int))
        {
            SocketError errorCode = setsockopt(socket, optionLevel, optionName, optionValue, optionLength);
            return errorCode == SocketError.SocketError ? GetLastSocketError() : SocketError.Success;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError GetOption(nint socket, SocketOptionLevel level, SocketOptionName optionName, int* optionValue, int* optionLength = null)
        {
            int num = sizeof(int);
            if (optionLength == null)
                optionLength = &num;

            SocketError errorCode = getsockopt(socket, (int)level, (int)optionName, (byte*)optionValue, optionLength);
            return errorCode == SocketError.SocketError ? GetLastSocketError() : SocketError.Success;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError SetBlocking(nint socket, bool shouldBlock)
        {
            int intBlocking = shouldBlock ? 0 : -1;
            SocketError errorCode = ioctlsocket(socket, unchecked((int)0x8004667E), &intBlocking);

            if (errorCode == SocketError.SocketError)
                errorCode = GetLastSocketError();

            return errorCode;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError Poll(nint socket, int microseconds, SelectMode mode, out bool status)
        {
            nint* fileDescriptorSet = stackalloc nint[2] { 1, socket };
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
            {
                sockaddr_in6 __socketAddress_native = *socketAddress;
                __socketAddress_native.sin6_port = WinSock2.HOST_TO_NET_16(socketAddress->sin6_port);
                return sendto(socket, (byte*)buffer, length, SocketFlags.None, (byte*)&__socketAddress_native, 28);
            }

            int num = sendto(socket, (byte*)buffer, length, SocketFlags.None, null, 28);
            return num;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReceiveFrom(nint socket, void* buffer, int length, sockaddr_in6* socketAddress)
        {
            sockaddr_storage addressStorage = new sockaddr_storage();
            int socketAddressSize = sizeof(sockaddr_storage);

            int num = recvfrom(socket, (byte*)buffer, length, SocketFlags.None, (byte*)&addressStorage, &socketAddressSize);

            if (num > 0 && socketAddress != null)
            {
                if (addressStorage.ss_family == (int)AddressFamily.InterNetwork)
                {
                    sockaddr_in* __socketAddress_native = (sockaddr_in*)&addressStorage;
                    Unsafe.InitBlockUnaligned(socketAddress->sin6_addr, 0, 8);
                    Unsafe.WriteUnaligned(socketAddress->sin6_addr + 8, -0x10000);
                    Unsafe.CopyBlockUnaligned(socketAddress->sin6_addr + 12, &__socketAddress_native->sin_addr, 4);
                    socketAddress->sin6_port = WinSock2.NET_TO_HOST_16(__socketAddress_native->sin_port);
                }
                else if (addressStorage.ss_family == (int)ADDRESS_FAMILY_INTER_NETWORK_V6)
                {
                    sockaddr_in6* __socketAddress_native = (sockaddr_in6*)&addressStorage;
                    Unsafe.CopyBlockUnaligned(socketAddress->sin6_addr, __socketAddress_native->sin6_addr, 16);
                    socketAddress->sin6_port = WinSock2.NET_TO_HOST_16(__socketAddress_native->sin6_port);
                }
            }

            return num;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError GetName(nint socket, sockaddr_in6* socketAddress)
        {
            sockaddr_storage addressStorage = new sockaddr_storage();
            int socketAddressSize = sizeof(sockaddr_storage);
            SocketError errorCode = getsockname(socket, (sockaddr*)&addressStorage, &socketAddressSize);
            if (errorCode == SocketError.Success)
            {
                if (addressStorage.ss_family == (int)AddressFamily.InterNetwork)
                {
                    sockaddr_in* __socketAddress_native = (sockaddr_in*)&addressStorage;
                    Unsafe.InitBlockUnaligned(socketAddress->sin6_addr, 0, 8);
                    Unsafe.WriteUnaligned(socketAddress->sin6_addr + 8, -0x10000);
                    Unsafe.CopyBlockUnaligned(socketAddress->sin6_addr + 12, &__socketAddress_native->sin_addr, 4);
                    socketAddress->sin6_port = WinSock2.NET_TO_HOST_16(__socketAddress_native->sin_port);
                }
                else if (addressStorage.ss_family == (int)ADDRESS_FAMILY_INTER_NETWORK_V6)
                {
                    sockaddr_in6* __socketAddress_native = (sockaddr_in6*)&addressStorage;
                    Unsafe.CopyBlockUnaligned(socketAddress->sin6_addr, __socketAddress_native->sin6_addr, 16);
                    socketAddress->sin6_port = WinSock2.NET_TO_HOST_16(__socketAddress_native->sin6_port);
                }
            }

            return errorCode;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError SetIP(void* pAddrBuf, ReadOnlySpan<char> ip)
        {
            int byteCount = Encoding.ASCII.GetByteCount(ip);
            Span<byte> buffer = stackalloc byte[byteCount];
            Encoding.ASCII.GetBytes(ip, buffer);

            int addressFamily = (int)ADDRESS_FAMILY_INTER_NETWORK_V6;
            if (ip.IndexOf(':') < 0)
            {
                addressFamily = (int)AddressFamily.InterNetwork;
                Unsafe.InitBlockUnaligned(pAddrBuf, 0, 8);
                Unsafe.WriteUnaligned((byte*)pAddrBuf + 8, -0x10000);
                pAddrBuf = (byte*)pAddrBuf + 12;
            }

            int error = inet_pton(addressFamily, Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer)), (byte*)pAddrBuf);

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
            ref int reference = ref Unsafe.AsRef<int>(pAddrBuf);
            if (Unsafe.Add<int>(ref reference, 2) == -0x10000 && reference == 0 && Unsafe.Add(ref reference, 1) == 0)
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
        public static SocketError SetHostName(void* pAddrBuf, ReadOnlySpan<char> hostName)
        {
            int byteCount = Encoding.ASCII.GetByteCount(hostName);
            Span<byte> buffer = stackalloc byte[byteCount];
            Encoding.ASCII.GetBytes(hostName, buffer);

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
                        sockaddr_in* socketAddress = (sockaddr_in*)hint->ai_addr;

                        Unsafe.InitBlockUnaligned(pAddrBuf, 0, 8);
                        Unsafe.WriteUnaligned((byte*)pAddrBuf + 8, -0x10000);
                        Unsafe.WriteUnaligned((byte*)pAddrBuf + 12, socketAddress->sin_addr.S_addr);

                        freeaddrinfo(results);

                        return 0;
                    }

                    if (hint->ai_family == (int)ADDRESS_FAMILY_INTER_NETWORK_V6)
                    {
                        sockaddr_in6* socketAddress = (sockaddr_in6*)hint->ai_addr;

                        Unsafe.CopyBlockUnaligned(pAddrBuf, socketAddress->sin6_addr, 16);

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
                Unsafe.WriteUnaligned((byte*)pAddrBuf + 8, -0x10000);
            }

            int error = inet_pton(addressFamily, Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer)), (byte*)pAddrBuf + 12);

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
        public static SocketError GetHostName(sockaddr_in6* socketAddress, Span<byte> buffer)
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

            return GetIP(socketAddress->sin6_addr, buffer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void MicrosecondsToTimeValue(long microseconds, ref TimeValue socketTime)
        {
            const int microcnv = 1000000;
            socketTime.Seconds = (int)(microseconds / microcnv);
            socketTime.Microseconds = (int)(microseconds % microcnv);
        }

        [DllImport(NATIVE_LIBRARY, CallingConvention = CallingConvention.StdCall)]
        private static extern SocketError WSAStartup(short wVersionRequested, WSAData* lpWSAData);

        [DllImport(NATIVE_LIBRARY, CallingConvention = CallingConvention.StdCall)]
        private static extern SocketError WSACleanup();

        [DllImport(NATIVE_LIBRARY, CallingConvention = CallingConvention.StdCall)]
        private static extern SocketError bind(nint __socketHandle_native, sockaddr* __socketAddress_native, int __socketAddressSize_native);

        [DllImport(NATIVE_LIBRARY, CallingConvention = CallingConvention.StdCall)]
        private static extern SocketError getsockname(nint __socketHandle_native, sockaddr* __socketAddress_native, int* __socketAddressSize_native);

        [DllImport(NATIVE_LIBRARY, CallingConvention = CallingConvention.StdCall)]
        private static extern nint WSASocketW(AddressFamily __addressFamily_native, SocketType __socketType_native, ProtocolType __protocolType_native, nint __protocolInfo_native, uint __group_native, int __flags_native);

        [DllImport(NATIVE_LIBRARY, CallingConvention = CallingConvention.StdCall)]
        private static extern SocketError ioctlsocket(nint __socketHandle_native, int __cmd_native, int* __argp_native);

        [DllImport(NATIVE_LIBRARY, CallingConvention = CallingConvention.StdCall)]
        private static extern SocketError setsockopt(nint __socketHandle_native, SocketOptionLevel __optionLevel_native, SocketOptionName __optionName_native, int* __optionValue_native, int __optionLength_native);

        [DllImport(NATIVE_LIBRARY, CallingConvention = CallingConvention.StdCall)]
        private static extern SocketError getsockopt(nint s, int level, int optname, byte* optval, int* optlen);

        [DllImport(NATIVE_LIBRARY, CallingConvention = CallingConvention.StdCall)]
        private static extern SocketError connect(nint s, sockaddr* name, int namelen);

        [DllImport(NATIVE_LIBRARY, CallingConvention = CallingConvention.StdCall)]
        private static extern SocketError closesocket(nint __socketHandle_native);

        [DllImport(NATIVE_LIBRARY, CallingConvention = CallingConvention.StdCall)]
        private static extern int sendto(nint __socketHandle_native, byte* __pinnedBuffer_native, int __len_native, SocketFlags __socketFlags_native, byte* __socketAddress_native, int __socketAddressSize_native);

        [DllImport(NATIVE_LIBRARY, CallingConvention = CallingConvention.StdCall)]
        private static extern int recvfrom(nint __socketHandle_native, byte* __pinnedBuffer_native, int __len_native, SocketFlags __socketFlags_native, byte* __socketAddress_native, int* __socketAddressSize_native);

        [DllImport(NATIVE_LIBRARY, CallingConvention = CallingConvention.StdCall)]
        private static extern int select(int __ignoredParameter_native, nint* __readfds_native, nint* __writefds_native, nint* __exceptfds_native, TimeValue* __timeout_native);

        [DllImport(NATIVE_LIBRARY, CallingConvention = CallingConvention.StdCall)]
        private static extern int WSAGetLastError();

        [DllImport(NATIVE_LIBRARY, CallingConvention = CallingConvention.StdCall)]
        private static extern int inet_pton(int Family, void* pszAddrString, void* pAddrBuf);

        [DllImport(NATIVE_LIBRARY, CallingConvention = CallingConvention.StdCall)]
        private static extern int getaddrinfo(byte* pNodeName, byte* pServiceName, addrinfo* pHints, addrinfo** ppResult);

        [DllImport(NATIVE_LIBRARY, CallingConvention = CallingConvention.StdCall)]
        private static extern void freeaddrinfo(addrinfo* pAddrInfo);

        [DllImport(NATIVE_LIBRARY, CallingConvention = CallingConvention.StdCall)]
        private static extern byte* inet_ntop(int Family, void* pAddr, ref byte pStringBuf, nuint StringBufSize);

        [DllImport(NATIVE_LIBRARY, CallingConvention = CallingConvention.StdCall)]
        private static extern int getnameinfo(sockaddr* pSockaddr, int SockaddrLength, ref byte pNodeBuffer, ulong NodeBufferSize, byte* pServiceBuffer, ulong ServiceBufferSize, int Flags);

        [DllImport(NATIVE_LIBRARY, CallingConvention = CallingConvention.StdCall)]
        private static extern SocketError WSAIoctl(nint __socketHandle_native, int __ioControlCode_native, byte* __inBuffer_native, int __inBufferSize_native, byte* __outBuffer_native, int __outBufferSize_native, int* __bytesTransferred_native, nint __overlapped_native, nint __completionRoutine_native);

        [StructLayout(LayoutKind.Sequential, Size = 408)]
        private struct WSAData
        {
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct TimeValue
        {
            public int Seconds;
            public int Microseconds;
        }
    }
}