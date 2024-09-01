using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

#pragma warning disable CS8600
#pragma warning disable CS8603

// ReSharper disable ALL

namespace enet
{
    [StructLayout(LayoutKind.Explicit, Size = 8)]
    public struct ENetSocket : IEquatable<ENetSocket>
    {
        [FieldOffset(0)] public GCHandle handle;

        [NotNull] public Socket socket => (Socket)handle.Target;

        public bool Equals(ENetSocket other) => handle.Equals(other.handle);
        public override bool Equals(object? obj) => obj is ENetSocket other && Equals(other);
        public override int GetHashCode() => handle.GetHashCode();

        public override string ToString() => !handle.IsAllocated ? null : socket.ToString();

        public static bool operator ==(ENetSocket left, ENetSocket right) => left.Equals(right);
        public static bool operator !=(ENetSocket left, ENetSocket right) => !left.Equals(right);
    }

    [StructLayout(LayoutKind.Explicit, Size = 8)]
    public struct ENetAddress : IEquatable<ENetAddress>
    {
        [FieldOffset(0)] public GCHandle handle;

        [NotNull] public IPEndPoint ipEndPoint => (IPEndPoint)handle.Target;

        public bool Equals(ENetAddress other) => (!handle.IsAllocated && !other.handle.IsAllocated) || (handle.IsAllocated && other.handle.IsAllocated && ipEndPoint.Equals(other.ipEndPoint));
        public override bool Equals(object? obj) => obj is ENetAddress other && Equals(other);
        public override int GetHashCode() => !handle.IsAllocated ? 0 : ipEndPoint.GetHashCode();

        public override string ToString() => !handle.IsAllocated ? null : ipEndPoint.ToString();

        public static bool operator ==(ENetAddress left, ENetAddress right) => left.Equals(right);
        public static bool operator !=(ENetAddress left, ENetAddress right) => !left.Equals(right);
    }
}