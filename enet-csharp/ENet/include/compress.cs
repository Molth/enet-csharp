using System.Runtime.InteropServices;

#pragma warning disable CS1591

// ReSharper disable ALL

namespace enet
{
    public struct ENetSymbol
    {
        /* binary indexed tree of symbols */
        public byte value;
        public byte count;
        public ushort under;
        public ushort left;
        public ushort right;

        /* context defined by this symbol */
        public ushort symbols;
        public ushort escapes;
        public ushort total;
        public ushort parent;
    }

    [ENetArray(4096)]
    [StructLayout(LayoutKind.Sequential)]
    internal struct ENetSymbols
    {
        private ENetSymbols2048 _element0;
        private ENetSymbols2048 _element1;

        [StructLayout(LayoutKind.Sequential)]
        private struct ENetSymbols2
        {
            private ENetSymbol _element0;
            private ENetSymbol _element1;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ENetSymbols4
        {
            private ENetSymbols2 _element0;
            private ENetSymbols2 _element1;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ENetSymbols8
        {
            private ENetSymbols4 _element0;
            private ENetSymbols4 _element1;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ENetSymbols16
        {
            private ENetSymbols8 _element0;
            private ENetSymbols8 _element1;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ENetSymbols32
        {
            private ENetSymbols16 _element0;
            private ENetSymbols16 _element1;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ENetSymbols64
        {
            private ENetSymbols32 _element0;
            private ENetSymbols32 _element1;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ENetSymbols128
        {
            private ENetSymbols64 _element0;
            private ENetSymbols64 _element1;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ENetSymbols256
        {
            private ENetSymbols128 _element0;
            private ENetSymbols128 _element1;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ENetSymbols512
        {
            private ENetSymbols256 _element0;
            private ENetSymbols256 _element1;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ENetSymbols1024
        {
            private ENetSymbols512 _element0;
            private ENetSymbols512 _element1;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct ENetSymbols2048
        {
            private ENetSymbols1024 _element0;
            private ENetSymbols1024 _element1;
        }
    }
}