using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace enet
{
    public partial class ENet
    {
        /* adaptation constants tuned aggressively for small packet sizes rather than large file compression */

        /// <summary>
        ///     The top of the range coder interval, at which output bytes begin to be emitted.
        /// </summary>
        public const uint ENET_RANGE_CODER_TOP = 1 << 24;

        /// <summary>
        ///     The bottom of the range coder interval, below which renormalization emits bytes.
        /// </summary>
        public const uint ENET_RANGE_CODER_BOTTOM = 1 << 16;

        /// <summary>
        ///     The amount by which a context symbol count is incremented on each occurrence.
        /// </summary>
        public const uint ENET_CONTEXT_SYMBOL_DELTA = 3;

        /// <summary>
        ///     The minimum symbol weight assigned to a newly created context symbol.
        /// </summary>
        public const uint ENET_CONTEXT_SYMBOL_MINIMUM = 1;

        /// <summary>
        ///     The initial escape weight assigned to a newly created context.
        /// </summary>
        public const uint ENET_CONTEXT_ESCAPE_MINIMUM = 1;

        /// <summary>
        ///     The order of the context model, i.e. how many preceding bytes form a context.
        /// </summary>
        public const uint ENET_SUBCONTEXT_ORDER = 2;

        /// <summary>
        ///     The amount by which a subcontext symbol count is incremented on each occurrence.
        /// </summary>
        public const uint ENET_SUBCONTEXT_SYMBOL_DELTA = 2;

        /// <summary>
        ///     The amount by which a subcontext escape weight is incremented on an escape.
        /// </summary>
        public const uint ENET_SUBCONTEXT_ESCAPE_DELTA = 5;
    }

    /// <summary>
    ///     A range coder context holding the symbol table used by the ENet compression scheme.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ENetRangeCoder
    {
        /* only allocate enough symbols for reasonable MTUs, would need to be larger for large file compression */

        /// <summary>
        ///     The backing storage for the range coder symbol table.
        /// </summary>
        private ENetSymbols symbols_t;

        /// <summary>
        ///     The range coder symbol table as an array of <see cref="ENetSymbol" /> entries.
        /// </summary>
        public ENetSymbol* symbols => (ENetSymbol*)Unsafe.AsPointer(ref symbols_t);
    }

    /// <summary>
    ///     A single node of the range coder symbol tree used by the ENet compression scheme.
    /// </summary>
    public struct ENetSymbol
    {
        /// <summary>
        ///     The symbol value represented by this node.
        /// </summary>
        public byte value;

        /// <summary>
        ///     The occurrence count of the symbol, used to update the binary indexed tree.
        /// </summary>
        public byte count;

        /// <summary>
        ///     The count of symbols strictly below this node in the tree.
        /// </summary>
        public ushort under;

        /// <summary>
        ///     The index of the left child of this node in the symbol table.
        /// </summary>
        public ushort left;

        /// <summary>
        ///     The index of the right child of this node in the symbol table.
        /// </summary>
        public ushort right;

        /// <summary>
        ///     The number of symbols defined by the context associated with this node.
        /// </summary>
        /// <remarks>context defined by this symbol</remarks>
        public ushort symbols;

        /// <summary>
        ///     The escape count for the context defined by this node.
        /// </summary>
        public ushort escapes;

        /// <summary>
        ///     The total symbol count for the context defined by this node.
        /// </summary>
        public ushort total;

        /// <summary>
        ///     The index of the parent of this node in the symbol table.
        /// </summary>
        public ushort parent;
    }

    /// <summary>
    ///     A fixed-size array of <see cref="ENetSymbol" /> nodes laid out as a contiguous block of 4096 entries.
    /// </summary>
    [ENetArray(4096)]
    [StructLayout(LayoutKind.Sequential)]
    internal struct ENetSymbols
    {
        private ENetSymbols2048 _element0;
        private ENetSymbols2048 _element1;

        /// <summary>
        ///     A fixed-size array of two <see cref="ENetSymbol" /> elements.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct ENetSymbols2
        {
            private ENetSymbol _element0;
            private ENetSymbol _element1;
        }

        /// <summary>
        ///     A fixed-size array of four <see cref="ENetSymbol" /> elements.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct ENetSymbols4
        {
            private ENetSymbols2 _element0;
            private ENetSymbols2 _element1;
        }

        /// <summary>
        ///     A fixed-size array of eight <see cref="ENetSymbol" /> elements.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct ENetSymbols8
        {
            private ENetSymbols4 _element0;
            private ENetSymbols4 _element1;
        }

        /// <summary>
        ///     A fixed-size array of sixteen <see cref="ENetSymbol" /> elements.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct ENetSymbols16
        {
            private ENetSymbols8 _element0;
            private ENetSymbols8 _element1;
        }

        /// <summary>
        ///     A fixed-size array of thirty-two <see cref="ENetSymbol" /> elements.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct ENetSymbols32
        {
            private ENetSymbols16 _element0;
            private ENetSymbols16 _element1;
        }

        /// <summary>
        ///     A fixed-size array of sixty-four <see cref="ENetSymbol" /> elements.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct ENetSymbols64
        {
            private ENetSymbols32 _element0;
            private ENetSymbols32 _element1;
        }

        /// <summary>
        ///     A fixed-size array of one hundred and twenty-eight <see cref="ENetSymbol" /> elements.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct ENetSymbols128
        {
            private ENetSymbols64 _element0;
            private ENetSymbols64 _element1;
        }

        /// <summary>
        ///     A fixed-size array of two hundred and fifty-six <see cref="ENetSymbol" /> elements.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct ENetSymbols256
        {
            private ENetSymbols128 _element0;
            private ENetSymbols128 _element1;
        }

        /// <summary>
        ///     A fixed-size array of five hundred and twelve <see cref="ENetSymbol" /> elements.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct ENetSymbols512
        {
            private ENetSymbols256 _element0;
            private ENetSymbols256 _element1;
        }

        /// <summary>
        ///     A fixed-size array of one thousand and twenty-four <see cref="ENetSymbol" /> elements.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct ENetSymbols1024
        {
            private ENetSymbols512 _element0;
            private ENetSymbols512 _element1;
        }

        /// <summary>
        ///     A fixed-size array of two thousand and forty-eight <see cref="ENetSymbol" /> elements.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct ENetSymbols2048
        {
            private ENetSymbols1024 _element0;
            private ENetSymbols1024 _element1;
        }
    }
}