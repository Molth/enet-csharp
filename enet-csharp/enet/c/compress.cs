using enet_uint8 = byte;
using enet_uint16 = ushort;
using enet_uint32 = uint;

// ReSharper disable NegativeEqualityExpression
// ReSharper disable InconsistentNaming
// ReSharper disable SuggestVarOrType_Elsewhere

namespace enet
{
    public struct ENetSymbol
    {
        public enet_uint8 value;
        public enet_uint8 count;
        public enet_uint16 under;
        public enet_uint16 left, right;
        public enet_uint16 symbols;
        public enet_uint16 escapes;
        public enet_uint16 total;
        public enet_uint16 parent;
    }

    public static partial class ENet
    {
        public const enet_uint32 ENET_RANGE_CODER_TOP = 1 << 24;
        public const enet_uint32 ENET_RANGE_CODER_BOTTOM = 1 << 16;

        public const enet_uint32 ENET_CONTEXT_SYMBOL_DELTA = 3;
        public const enet_uint32 ENET_CONTEXT_SYMBOL_MINIMUM = 1;
        public const enet_uint32 ENET_CONTEXT_ESCAPE_MINIMUM = 1;

        public const enet_uint32 ENET_SUBCONTEXT_ORDER = 2;
        public const enet_uint32 ENET_SUBCONTEXT_SYMBOL_DELTA = 2;
        public const enet_uint32 ENET_SUBCONTEXT_ESCAPE_DELTA = 5;
    }

    public unsafe struct ENetRangeCoder
    {
        public ENetSymbol* symbols;
    }

    public static unsafe partial class ENet
    {
        public static void* enet_range_coder_create()
        {
            ENetRangeCoder* rangeCoder = (ENetRangeCoder*)enet_malloc(sizeof(ENetRangeCoder));
            rangeCoder->symbols = (ENetSymbol*)enet_malloc(4096 * sizeof(ENetSymbol));
            return rangeCoder;
        }

        public static void enet_range_coder_destroy(void* context)
        {
            ENetRangeCoder* rangeCoder = (ENetRangeCoder*)context;
            if (rangeCoder == null)
                return;
            enet_free(rangeCoder->symbols);
            enet_free(rangeCoder);
        }

        public static ENetSymbol* ENET_SYMBOL_CREATE(ENetRangeCoder* rangeCoder, enet_uint32* nextSymbol, enet_uint8 value_, enet_uint8 count_)
        {
            ENetSymbol* symbol = &rangeCoder->symbols[(*nextSymbol)++];
            symbol->value = value_;
            symbol->count = count_;
            symbol->under = count_;
            symbol->left = 0;
            symbol->right = 0;
            symbol->symbols = 0;
            symbol->escapes = 0;
            symbol->total = 0;
            symbol->parent = 0;
            return symbol;
        }

        public static ENetSymbol* ENET_CONTEXT_CREATE(ENetRangeCoder* rangeCoder, enet_uint32* nextSymbol, enet_uint16 escapes_, enet_uint16 minimum)
        {
            ENetSymbol* context = ENET_SYMBOL_CREATE(rangeCoder, nextSymbol, 0, 0);
            context->escapes = escapes_;
            context->total = (enet_uint16)(escapes_ + 256 * minimum);
            context->symbols = 0;
            return context;
        }

        public static enet_uint16 enet_symbol_rescale(ENetSymbol* symbol)
        {
            enet_uint16 total = 0;
            for (;;)
            {
                symbol->count -= (enet_uint8)(symbol->count >> 1);
                symbol->under = symbol->count;
                if (symbol->left != 0)
                    symbol->under += enet_symbol_rescale(symbol + symbol->left);
                total += symbol->under;
                if (!(symbol->right != 0)) break;
                symbol += symbol->right;
            }

            return total;
        }

        //TODO: Complete compress next
    }
}