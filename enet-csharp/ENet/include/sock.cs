using System;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
#if !NET5_0_OR_GREATER
using System.Runtime.InteropServices;
#endif
using unixsock;
using winsock;

#pragma warning disable CS1591

// ReSharper disable ALL

namespace enet
{
    public static unsafe class ENetSock
    {
        public static readonly ushort ADDRESS_FAMILY_INTER_NETWORK_V6;
        private static readonly delegate* managed<SocketError> _GetLastSocketError;
        private static readonly delegate* managed<SocketError> _Initialize;
        private static readonly delegate* managed<SocketError> _Cleanup;
        private static readonly delegate* managed<nint> _Create;
        private static readonly delegate* managed<nint, SocketError> _Close;
        private static readonly delegate* managed<nint, sockaddr_in6*, SocketError> _Bind;
        private static readonly delegate* managed<nint, sockaddr_in6*, SocketError> _Connect;
        private static readonly delegate* managed<nint, SocketOptionLevel, SocketOptionName, int*, SocketError> _SetOption;
        private static readonly delegate* managed<nint, SocketOptionLevel, SocketOptionName, int*, SocketError> _GetOption;
        private static readonly delegate* managed<nint, bool, SocketError> _SetBlocking;
        private static readonly delegate* managed<nint, int, SelectMode, out bool, SocketError> _Poll;
        private static readonly delegate* managed<nint, void*, int, sockaddr_in6*, int> _SendTo;
        private static readonly delegate* managed<nint, void*, int, sockaddr_in6*, int> _ReceiveFrom;
        private static readonly delegate* managed<nint, sockaddr_in6*, SocketError> _GetName;
        private static readonly delegate* managed<void*, ReadOnlySpan<char>, SocketError> _SetIP;
        private static readonly delegate* managed<void*, Span<byte>, SocketError> _GetIP;
        private static readonly delegate* managed<void*, ReadOnlySpan<char>, SocketError> _SetHostName;
        private static readonly delegate* managed<sockaddr_in6*, Span<byte>, SocketError> _GetHostName;

        static ENetSock()
        {
            var isWindows =
#if NET5_0_OR_GREATER
                OperatingSystem.IsWindows();
#else
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
#endif
            if (isWindows)
            {
                ADDRESS_FAMILY_INTER_NETWORK_V6 = WinSock.ADDRESS_FAMILY_INTER_NETWORK_V6;
                _GetLastSocketError = &WinSock.GetLastSocketError;
                _Initialize = &WinSock.Initialize;
                _Cleanup = &WinSock.Cleanup;
                _Create = &WinSock.Create;
                _Close = &WinSock.Close;
                _Bind = &WinSock.Bind;
                _Connect = &WinSock.Connect;
                _SetOption = &WinSock.SetOption;
                _GetOption = &WinSock.GetOption;
                _SetBlocking = &WinSock.SetBlocking;
                _Poll = &WinSock.Poll;
                _SendTo = &WinSock.SendTo;
                _ReceiveFrom = &WinSock.ReceiveFrom;
                _GetName = &WinSock.GetName;
                _SetIP = &WinSock.SetIP;
                _GetIP = &WinSock.GetIP;
                _SetHostName = &WinSock.SetHostName;
                _GetHostName = &WinSock.GetHostName;
            }
            else
            {
                ADDRESS_FAMILY_INTER_NETWORK_V6 = UnixSock.ADDRESS_FAMILY_INTER_NETWORK_V6;
                _GetLastSocketError = &UnixSock.GetLastSocketError;
                _Initialize = &UnixSock.Initialize;
                _Cleanup = &UnixSock.Cleanup;
                _Create = &UnixSock.Create;
                _Close = &UnixSock.Close;
                _Bind = &UnixSock.Bind;
                _Connect = &UnixSock.Connect;
                _SetOption = &UnixSock.SetOption;
                _GetOption = &UnixSock.GetOption;
                _SetBlocking = &UnixSock.SetBlocking;
                _Poll = &UnixSock.Poll;
                _SendTo = &UnixSock.SendTo;
                _ReceiveFrom = &UnixSock.ReceiveFrom;
                _GetName = &UnixSock.GetName;
                _SetIP = &UnixSock.SetIP;
                _GetIP = &UnixSock.GetIP;
                _SetHostName = &UnixSock.SetHostName;
                _GetHostName = &UnixSock.GetHostName;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError GetLastSocketError() => _GetLastSocketError();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError Initialize() => _Initialize();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError Cleanup() => _Cleanup();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static nint Create() => _Create();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError Close(nint handle) => _Close(handle);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError Bind(nint handle, sockaddr_in6* address) => _Bind(handle, address);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError Connect(nint handle, sockaddr_in6* address) => _Connect(handle, address);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError SetOption(nint handle, SocketOptionLevel level, SocketOptionName name, int* value) => _SetOption(handle, level, name, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError GetOption(nint handle, SocketOptionLevel level, SocketOptionName name, int* value) => _GetOption(handle, level, name, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError SetBlocking(nint handle, bool blocking) => _SetBlocking(handle, blocking);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError Poll(nint handle, int microseconds, SelectMode mode, out bool status) => _Poll(handle, microseconds, mode, out status);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SendTo(nint handle, void* buffer, int length, sockaddr_in6* address) => _SendTo(handle, buffer, length, address);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReceiveFrom(nint handle, void* buffer, int length, sockaddr_in6* address) => _ReceiveFrom(handle, buffer, length, address);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError GetName(nint handle, sockaddr_in6* address) => _GetName(handle, address);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError SetIP(void* context, ReadOnlySpan<char> ip) => _SetIP(context, ip);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError GetIP(void* context, Span<byte> ip) => _GetIP(context, ip);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError SetHostName(void* context, ReadOnlySpan<char> hostName) => _SetHostName(context, hostName);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SocketError GetHostName(sockaddr_in6* address, Span<byte> hostName) => _GetHostName(address, hostName);
    }
}