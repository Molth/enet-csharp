using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

#pragma warning disable CS1591

// ReSharper disable ALL

namespace NativeSockets
{
    public static unsafe class SocketPal
    {
        public static ushort ADDRESS_FAMILY_INTER_NETWORK_V6 { get; }

        public static bool IsWindows => WindowsSocketPal.IsSupported;
        public static bool IsLinux => LinuxSocketPal.IsSupported;
        public static bool IsBSD => BSDSocketPal.IsSupported;

        private static readonly delegate* managed<SocketError> _GetLastSocketError;
        private static readonly delegate* managed<SocketError> _Initialize;
        private static readonly delegate* managed<SocketError> _Cleanup;
        private static readonly delegate* managed<bool, nint> _Create;
        private static readonly delegate* managed<nint, SocketError> _Close;
        private static readonly delegate* managed<nint, bool, SocketError> _SetDualMode6;
        private static readonly delegate* managed<nint, sockaddr_in*, SocketError> _Bind4;
        private static readonly delegate* managed<nint, sockaddr_in6*, SocketError> _Bind6;
        private static readonly delegate* managed<nint, sockaddr_in*, SocketError> _Connect4;
        private static readonly delegate* managed<nint, sockaddr_in6*, SocketError> _Connect6;
        private static readonly delegate* managed<nint, SocketOptionLevel, SocketOptionName, int*, int, SocketError> _SetOption;
        private static readonly delegate* managed<nint, SocketOptionLevel, SocketOptionName, int*, int*, SocketError> _GetOption;
        private static readonly delegate* managed<nint, bool, SocketError> _SetBlocking;
        private static readonly delegate* managed<nint, int, SelectMode, out bool, SocketError> _Poll;
        private static readonly delegate* managed<nint, void*, int, int> _Send;
        private static readonly delegate* managed<nint, void*, int, sockaddr_in*, int> _SendTo4;
        private static readonly delegate* managed<nint, void*, int, sockaddr_in6*, int> _SendTo6;
        private static readonly delegate* managed<nint, void*, int, int> _Receive;
        private static readonly delegate* managed<nint, void*, int, sockaddr_in*, int> _ReceiveFrom4;
        private static readonly delegate* managed<nint, void*, int, sockaddr_in6*, int> _ReceiveFrom6;
        private static readonly delegate* managed<nint, WSABuffer*, int, int> _WSASend;
        private static readonly delegate* managed<nint, WSABuffer*, int, sockaddr_in*, int> _WSASendTo4;
        private static readonly delegate* managed<nint, WSABuffer*, int, sockaddr_in6*, int> _WSASendTo6;
        private static readonly delegate* managed<nint, WSABuffer*, int, int> _WSAReceive;
        private static readonly delegate* managed<nint, WSABuffer*, int, sockaddr_in*, int> _WSAReceiveFrom4;
        private static readonly delegate* managed<nint, WSABuffer*, int, sockaddr_in6*, int> _WSAReceiveFrom6;
        private static readonly delegate* managed<nint, sockaddr_in*, SocketError> _GetName4;
        private static readonly delegate* managed<nint, sockaddr_in6*, SocketError> _GetName6;
        private static readonly delegate* managed<sockaddr_in*, ReadOnlySpan<char>, SocketError> _SetIP4;
        private static readonly delegate* managed<sockaddr_in6*, ReadOnlySpan<char>, SocketError> _SetIP6;
        private static readonly delegate* managed<sockaddr_in*, Span<byte>, SocketError> _GetIP4;
        private static readonly delegate* managed<sockaddr_in6*, Span<byte>, SocketError> _GetIP6;
        private static readonly delegate* managed<sockaddr_in*, ReadOnlySpan<char>, SocketError> _SetHostName4;
        private static readonly delegate* managed<sockaddr_in6*, ReadOnlySpan<char>, SocketError> _SetHostName6;
        private static readonly delegate* managed<sockaddr_in*, Span<byte>, SocketError> _GetHostName4;
        private static readonly delegate* managed<sockaddr_in6*, Span<byte>, SocketError> _GetHostName6;

        static SocketPal()
        {
            if (IsWindows)
            {
                ADDRESS_FAMILY_INTER_NETWORK_V6 = WindowsSocketPal.ADDRESS_FAMILY_INTER_NETWORK_V6;

                _GetLastSocketError = &WindowsSocketPal.GetLastSocketError;
                _Initialize = &WindowsSocketPal.Initialize;
                _Cleanup = &WindowsSocketPal.Cleanup;
                _Create = &WindowsSocketPal.Create;
                _Close = &WindowsSocketPal.Close;
                _SetDualMode6 = &WindowsSocketPal.SetDualMode6;
                _Bind4 = &WindowsSocketPal.Bind4;
                _Bind6 = &WindowsSocketPal.Bind6;
                _Connect4 = &WindowsSocketPal.Connect4;
                _Connect6 = &WindowsSocketPal.Connect6;
                _SetOption = &WindowsSocketPal.SetOption;
                _GetOption = &WindowsSocketPal.GetOption;
                _SetBlocking = &WindowsSocketPal.SetBlocking;
                _Poll = &WindowsSocketPal.Poll;
                _Send = &WindowsSocketPal.Send;
                _SendTo4 = &WindowsSocketPal.SendTo4;
                _SendTo6 = &WindowsSocketPal.SendTo6;
                _Receive = &WindowsSocketPal.Receive;
                _ReceiveFrom4 = &WindowsSocketPal.ReceiveFrom4;
                _ReceiveFrom6 = &WindowsSocketPal.ReceiveFrom6;
                _WSASend = &WindowsSocketPal.WSASend;
                _WSASendTo4 = &WindowsSocketPal.WSASendTo4;
                _WSASendTo6 = &WindowsSocketPal.WSASendTo6;
                _WSAReceive = &WindowsSocketPal.WSAReceive;
                _WSAReceiveFrom4 = &WindowsSocketPal.WSAReceiveFrom4;
                _WSAReceiveFrom6 = &WindowsSocketPal.WSAReceiveFrom6;
                _GetName4 = &WindowsSocketPal.GetName4;
                _GetName6 = &WindowsSocketPal.GetName6;
                _SetIP4 = &WindowsSocketPal.SetIP4;
                _SetIP6 = &WindowsSocketPal.SetIP6;
                _GetIP4 = &WindowsSocketPal.GetIP4;
                _GetIP6 = &WindowsSocketPal.GetIP6;
                _SetHostName4 = &WindowsSocketPal.SetHostName4;
                _SetHostName6 = &WindowsSocketPal.SetHostName6;
                _GetHostName4 = &WindowsSocketPal.GetHostName4;
                _GetHostName6 = &WindowsSocketPal.GetHostName6;

                return;
            }

            if (IsLinux)
            {
                ADDRESS_FAMILY_INTER_NETWORK_V6 = LinuxSocketPal.ADDRESS_FAMILY_INTER_NETWORK_V6;

                _GetLastSocketError = &LinuxSocketPal.GetLastSocketError;
                _Initialize = &LinuxSocketPal.Initialize;
                _Cleanup = &LinuxSocketPal.Cleanup;
                _Create = &LinuxSocketPal.Create;
                _Close = &LinuxSocketPal.Close;
                _SetDualMode6 = &LinuxSocketPal.SetDualMode6;
                _Bind4 = &LinuxSocketPal.Bind4;
                _Bind6 = &LinuxSocketPal.Bind6;
                _Connect4 = &LinuxSocketPal.Connect4;
                _Connect6 = &LinuxSocketPal.Connect6;
                _SetOption = &LinuxSocketPal.SetOption;
                _GetOption = &LinuxSocketPal.GetOption;
                _SetBlocking = &LinuxSocketPal.SetBlocking;
                _Poll = &LinuxSocketPal.Poll;
                _Send = &LinuxSocketPal.Send;
                _SendTo4 = &LinuxSocketPal.SendTo4;
                _SendTo6 = &LinuxSocketPal.SendTo6;
                _Receive = &LinuxSocketPal.Receive;
                _ReceiveFrom4 = &LinuxSocketPal.ReceiveFrom4;
                _ReceiveFrom6 = &LinuxSocketPal.ReceiveFrom6;
                _WSASend = &LinuxSocketPal.WSASend;
                _WSASendTo4 = &LinuxSocketPal.WSASendTo4;
                _WSASendTo6 = &LinuxSocketPal.WSASendTo6;
                _WSAReceive = &LinuxSocketPal.WSAReceive;
                _WSAReceiveFrom4 = &LinuxSocketPal.WSAReceiveFrom4;
                _WSAReceiveFrom6 = &LinuxSocketPal.WSAReceiveFrom6;
                _GetName4 = &LinuxSocketPal.GetName4;
                _GetName6 = &LinuxSocketPal.GetName6;
                _SetIP4 = &LinuxSocketPal.SetIP4;
                _SetIP6 = &LinuxSocketPal.SetIP6;
                _GetIP4 = &LinuxSocketPal.GetIP4;
                _GetIP6 = &LinuxSocketPal.GetIP6;
                _SetHostName4 = &LinuxSocketPal.SetHostName4;
                _SetHostName6 = &LinuxSocketPal.SetHostName6;
                _GetHostName4 = &LinuxSocketPal.GetHostName4;
                _GetHostName6 = &LinuxSocketPal.GetHostName6;

                return;
            }

            ADDRESS_FAMILY_INTER_NETWORK_V6 = BSDSocketPal.ADDRESS_FAMILY_INTER_NETWORK_V6;

            _GetLastSocketError = &BSDSocketPal.GetLastSocketError;
            _Initialize = &BSDSocketPal.Initialize;
            _Cleanup = &BSDSocketPal.Cleanup;
            _Create = &BSDSocketPal.Create;
            _Close = &BSDSocketPal.Close;
            _SetDualMode6 = &BSDSocketPal.SetDualMode6;
            _Bind4 = &BSDSocketPal.Bind4;
            _Bind6 = &BSDSocketPal.Bind6;
            _Connect4 = &BSDSocketPal.Connect4;
            _Connect6 = &BSDSocketPal.Connect6;
            _SetOption = &BSDSocketPal.SetOption;
            _GetOption = &BSDSocketPal.GetOption;
            _SetBlocking = &BSDSocketPal.SetBlocking;
            _Poll = &BSDSocketPal.Poll;
            _Send = &BSDSocketPal.Send;
            _SendTo4 = &BSDSocketPal.SendTo4;
            _SendTo6 = &BSDSocketPal.SendTo6;
            _Receive = &BSDSocketPal.Receive;
            _ReceiveFrom4 = &BSDSocketPal.ReceiveFrom4;
            _ReceiveFrom6 = &BSDSocketPal.ReceiveFrom6;
            _WSASend = &BSDSocketPal.WSASend;
            _WSASendTo4 = &BSDSocketPal.WSASendTo4;
            _WSASendTo6 = &BSDSocketPal.WSASendTo6;
            _WSAReceive = &BSDSocketPal.WSAReceive;
            _WSAReceiveFrom4 = &BSDSocketPal.WSAReceiveFrom4;
            _WSAReceiveFrom6 = &BSDSocketPal.WSAReceiveFrom6;
            _GetName4 = &BSDSocketPal.GetName4;
            _GetName6 = &BSDSocketPal.GetName6;
            _SetIP4 = &BSDSocketPal.SetIP4;
            _SetIP6 = &BSDSocketPal.SetIP6;
            _GetIP4 = &BSDSocketPal.GetIP4;
            _GetIP6 = &BSDSocketPal.GetIP6;
            _SetHostName4 = &BSDSocketPal.SetHostName4;
            _SetHostName6 = &BSDSocketPal.SetHostName6;
            _GetHostName4 = &BSDSocketPal.GetHostName4;
            _GetHostName6 = &BSDSocketPal.GetHostName6;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError GetLastSocketError() => _GetLastSocketError();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError Initialize() => _Initialize();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError Cleanup() => _Cleanup();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint Create(bool ipv6) => _Create(ipv6);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError Close(nint socket) => _Close(socket);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError SetDualMode6(nint socket, bool dualMode) => _SetDualMode6(socket, dualMode);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError Bind4(nint socket, sockaddr_in* socketAddress) => _Bind4(socket, socketAddress);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError Bind6(nint socket, sockaddr_in6* socketAddress) => _Bind6(socket, socketAddress);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError Connect4(nint socket, sockaddr_in* socketAddress) => _Connect4(socket, socketAddress);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError Connect6(nint socket, sockaddr_in6* socketAddress) => _Connect6(socket, socketAddress);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError SetOption(nint socket, SocketOptionLevel level, SocketOptionName name, int* value, int length = sizeof(int)) => _SetOption(socket, level, name, value, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError GetOption(nint socket, SocketOptionLevel level, SocketOptionName name, int* value, int* length = null) => _GetOption(socket, level, name, value, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError SetBlocking(nint socket, bool blocking) => _SetBlocking(socket, blocking);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError Poll(nint socket, int microseconds, SelectMode mode, out bool status) => _Poll(socket, microseconds, mode, out status);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Send(nint socket, void* buffer, int length) => _Send(socket, buffer, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SendTo4(nint socket, void* buffer, int length, sockaddr_in* socketAddress) => _SendTo4(socket, buffer, length, socketAddress);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SendTo6(nint socket, void* buffer, int length, sockaddr_in6* socketAddress) => _SendTo6(socket, buffer, length, socketAddress);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Receive(nint socket, void* buffer, int length) => _Receive(socket, buffer, length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReceiveFrom4(nint socket, void* buffer, int length, sockaddr_in* socketAddress) => _ReceiveFrom4(socket, buffer, length, socketAddress);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReceiveFrom6(nint socket, void* buffer, int length, sockaddr_in6* socketAddress) => _ReceiveFrom6(socket, buffer, length, socketAddress);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WSASend(nint socket, WSABuffer* buffers, int bufferCount) => _WSASend(socket, buffers, bufferCount);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WSASendTo4(nint socket, WSABuffer* buffers, int bufferCount, sockaddr_in* socketAddress) => _WSASendTo4(socket, buffers, bufferCount, socketAddress);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WSASendTo6(nint socket, WSABuffer* buffers, int bufferCount, sockaddr_in6* socketAddress) => _WSASendTo6(socket, buffers, bufferCount, socketAddress);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WSAReceive(nint socket, WSABuffer* buffers, int bufferCount) => _WSAReceive(socket, buffers, bufferCount);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WSAReceiveFrom4(nint socket, WSABuffer* buffers, int bufferCount, sockaddr_in* socketAddress) => _WSAReceiveFrom4(socket, buffers, bufferCount, socketAddress);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int WSAReceiveFrom6(nint socket, WSABuffer* buffers, int bufferCount, sockaddr_in6* socketAddress) => _WSAReceiveFrom6(socket, buffers, bufferCount, socketAddress);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError GetName4(nint socket, sockaddr_in* socketAddress) => _GetName4(socket, socketAddress);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError GetName6(nint socket, sockaddr_in6* socketAddress) => _GetName6(socket, socketAddress);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError SetIP4(sockaddr_in* pAddrBuf, ReadOnlySpan<char> ip) => _SetIP4(pAddrBuf, ip);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError SetIP6(sockaddr_in6* pAddrBuf, ReadOnlySpan<char> ip) => _SetIP6(pAddrBuf, ip);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError GetIP4(sockaddr_in* pAddrBuf, Span<byte> ip) => _GetIP4(pAddrBuf, ip);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError GetIP6(sockaddr_in6* pAddrBuf, Span<byte> ip) => _GetIP6(pAddrBuf, ip);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError SetHostName4(sockaddr_in* pAddrBuf, ReadOnlySpan<char> hostName) => _SetHostName4(pAddrBuf, hostName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError SetHostName6(sockaddr_in6* pAddrBuf, ReadOnlySpan<char> hostName) => _SetHostName6(pAddrBuf, hostName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError GetHostName4(sockaddr_in* socketAddress, Span<byte> hostName) => _GetHostName4(socketAddress, hostName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError GetHostName6(sockaddr_in6* socketAddress, Span<byte> hostName) => _GetHostName6(socketAddress, hostName);
    }
}