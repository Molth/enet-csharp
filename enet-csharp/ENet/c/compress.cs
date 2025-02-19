using System.Runtime.CompilerServices;
using enet_uint8 = byte;
using enet_uint16 = ushort;
using enet_uint32 = uint;
using size_t = nuint;
using ssize_t = nint;

// ReSharper disable ALL

namespace enet
{
    public partial class ENet
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
        public ENetSymbols symbols_t;
        public ENetSymbol* symbols => (ENetSymbol*)Unsafe.AsPointer(ref symbols_t);
    }

    public unsafe partial class ENet
    {
        public static void* enet_range_coder_create()
        {
            ENetRangeCoder* rangeCoder = (ENetRangeCoder*)enet_malloc((size_t)sizeof(ENetRangeCoder));
            if (rangeCoder == null)
                return null;

            return rangeCoder;
        }

        public static void enet_range_coder_destroy(void* context)
        {
            ENetRangeCoder* rangeCoder = (ENetRangeCoder*)context;
            if (rangeCoder == null)
                return;

            enet_free(rangeCoder);
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

        public static size_t enet_range_coder_compress(void* context, ENetBuffer* inBuffers, size_t inBufferCount, size_t inLimit, enet_uint8* outData, size_t outLimit)
        {
            ENetRangeCoder* rangeCoder = (ENetRangeCoder*)context;
            enet_uint8* outStart = outData, outEnd = &outData[outLimit];
            enet_uint8* inData, inEnd;
            enet_uint32 encodeLow = 0, encodeRange = unchecked((enet_uint32)(~0));
            ENetSymbol* root;
            enet_uint16 predicted = 0;
            size_t order = 0, nextSymbol = 0;

            if (rangeCoder == null || inBufferCount <= 0 || inLimit <= 0)
                return 0;

            inData = (enet_uint8*)inBuffers->data;
            inEnd = &inData[inBuffers->dataLength];
            inBuffers++;
            inBufferCount--;

            {
                {
                    root = &rangeCoder->symbols[(ssize_t)(nextSymbol++)];
                    root->value = 0;
                    root->count = 0;
                    root->under = 0;
                    root->left = 0;
                    root->right = 0;
                    root->symbols = 0;
                    root->escapes = 0;
                    root->total = 0;
                    root->parent = 0;
                }
                ;
                (root)->escapes = (enet_uint16)ENET_CONTEXT_ESCAPE_MINIMUM;
                (root)->total = (enet_uint16)(ENET_CONTEXT_ESCAPE_MINIMUM + 256 * ENET_CONTEXT_SYMBOL_MINIMUM);
                (root)->symbols = 0;
            }
            ;

            for (;;)
            {
                ENetSymbol* subcontext, symbol;

                enet_uint8 value;
                enet_uint16 count, under, total;
                enet_uint16* parent = &predicted;
                if (inData >= inEnd)
                {
                    if (inBufferCount <= 0)
                        break;
                    inData = (enet_uint8*)inBuffers->data;
                    inEnd = &inData[inBuffers->dataLength];
                    inBuffers++;
                    inBufferCount--;
                }

                value = *inData++;

                for (subcontext = &rangeCoder->symbols[predicted];
                     subcontext != root;
                     subcontext = &rangeCoder->symbols[subcontext->parent])
                {
                    {
                        under = (enet_uint16)(value * 0);
                        count = 0;
                        if (!((subcontext)->symbols != 0))
                        {
                            {
                                symbol = &rangeCoder->symbols[(ssize_t)(nextSymbol++)];
                                symbol->value = value;
                                symbol->count = (enet_uint8)ENET_SUBCONTEXT_SYMBOL_DELTA;
                                symbol->under = (enet_uint16)ENET_SUBCONTEXT_SYMBOL_DELTA;
                                symbol->left = 0;
                                symbol->right = 0;
                                symbol->symbols = 0;
                                symbol->escapes = 0;
                                symbol->total = 0;
                                symbol->parent = 0;
                            }
                            ;
                            (subcontext)->symbols = (enet_uint16)(symbol - (subcontext));
                        }
                        else
                        {
                            ENetSymbol* node = (subcontext) + (subcontext)->symbols;
                            for (;;)
                            {
                                if (value < node->value)
                                {
                                    node->under += (enet_uint16)ENET_SUBCONTEXT_SYMBOL_DELTA;
                                    if (node->left != 0)
                                    {
                                        node += node->left;
                                        continue;
                                    }

                                    {
                                        symbol = &rangeCoder->symbols[(ssize_t)(nextSymbol++)];
                                        symbol->value = value;
                                        symbol->count = (enet_uint8)ENET_SUBCONTEXT_SYMBOL_DELTA;
                                        symbol->under = (enet_uint16)ENET_SUBCONTEXT_SYMBOL_DELTA;
                                        symbol->left = 0;
                                        symbol->right = 0;
                                        symbol->symbols = 0;
                                        symbol->escapes = 0;
                                        symbol->total = 0;
                                        symbol->parent = 0;
                                    }
                                    ;
                                    node->left = (enet_uint16)(symbol - node);
                                }
                                else if (value > node->value)
                                {
                                    under += node->under;
                                    if (node->right != 0)
                                    {
                                        node += node->right;
                                        continue;
                                    }

                                    {
                                        symbol = &rangeCoder->symbols[(ssize_t)(nextSymbol++)];
                                        symbol->value = value;
                                        symbol->count = (enet_uint8)ENET_SUBCONTEXT_SYMBOL_DELTA;
                                        symbol->under = (enet_uint16)ENET_SUBCONTEXT_SYMBOL_DELTA;
                                        symbol->left = 0;
                                        symbol->right = 0;
                                        symbol->symbols = 0;
                                        symbol->escapes = 0;
                                        symbol->total = 0;
                                        symbol->parent = 0;
                                    }
                                    ;
                                    node->right = (enet_uint16)(symbol - node);
                                }
                                else
                                {
                                    count += node->count;
                                    under += (enet_uint16)(node->under - node->count);
                                    node->under += (enet_uint16)ENET_SUBCONTEXT_SYMBOL_DELTA;
                                    node->count += (enet_uint8)ENET_SUBCONTEXT_SYMBOL_DELTA;
                                    symbol = node;
                                }

                                break;
                            }
                        }
                    }
                    ;
                    *parent = (enet_uint16)(symbol - &rangeCoder->symbols[0]);
                    parent = &symbol->parent;
                    total = subcontext->total;

                    if (count > 0)
                    {
                        {
                            encodeRange /= (total);
                            encodeLow += (enet_uint32)((subcontext->escapes + under) * encodeRange);
                            encodeRange *= (count);
                            for (;;)
                            {
                                if ((encodeLow ^ (encodeLow + encodeRange)) >= ENET_RANGE_CODER_TOP)
                                {
                                    if (encodeRange >= ENET_RANGE_CODER_BOTTOM) break;
                                    encodeRange = (enet_uint32)(-encodeLow & (ENET_RANGE_CODER_BOTTOM - 1));
                                }

                                {
                                    if (outData >= outEnd) return 0;
                                    *outData++ = (enet_uint8)(encodeLow >> 24);
                                }
                                ;
                                encodeRange <<= 8;
                                encodeLow <<= 8;
                            }
                        }
                        ;
                    }
                    else
                    {
                        if (subcontext->escapes > 0 && subcontext->escapes < total)
                        {
                            encodeRange /= (total);
                            encodeLow += (0) * encodeRange;
                            encodeRange *= (subcontext->escapes);
                            for (;;)
                            {
                                if ((encodeLow ^ (encodeLow + encodeRange)) >= ENET_RANGE_CODER_TOP)
                                {
                                    if (encodeRange >= ENET_RANGE_CODER_BOTTOM) break;
                                    encodeRange = (enet_uint32)(-encodeLow & (ENET_RANGE_CODER_BOTTOM - 1));
                                }

                                {
                                    if (outData >= outEnd) return 0;
                                    *outData++ = (enet_uint8)(encodeLow >> 24);
                                }
                                ;
                                encodeRange <<= 8;
                                encodeLow <<= 8;
                            }
                        }

                        ;
                        subcontext->escapes += (enet_uint16)ENET_SUBCONTEXT_ESCAPE_DELTA;
                        subcontext->total += (enet_uint16)ENET_SUBCONTEXT_ESCAPE_DELTA;
                    }

                    subcontext->total += (enet_uint16)ENET_SUBCONTEXT_SYMBOL_DELTA;
                    if (count > 0xFF - 2 * ENET_SUBCONTEXT_SYMBOL_DELTA || subcontext->total > ENET_RANGE_CODER_BOTTOM - 0x100)
                    {
                        (subcontext)->total = ((subcontext)->symbols != 0) ? ((enet_uint16)enet_symbol_rescale((subcontext) + (subcontext)->symbols)) : (enet_uint16)0;
                        (subcontext)->escapes -= (enet_uint16)((subcontext)->escapes >> 1);
                        (subcontext)->total += (enet_uint16)((subcontext)->escapes + 256 * 0);
                    }

                    ;
                    if (count > 0) goto nextInput;
                }

                {
                    under = (enet_uint16)(value * ENET_CONTEXT_SYMBOL_MINIMUM);
                    count = (enet_uint16)ENET_CONTEXT_SYMBOL_MINIMUM;
                    if (!((root)->symbols != 0))
                    {
                        {
                            symbol = &rangeCoder->symbols[(ssize_t)(nextSymbol++)];
                            symbol->value = value;
                            symbol->count = (enet_uint8)ENET_CONTEXT_SYMBOL_DELTA;
                            symbol->under = (enet_uint16)ENET_CONTEXT_SYMBOL_DELTA;
                            symbol->left = 0;
                            symbol->right = 0;
                            symbol->symbols = 0;
                            symbol->escapes = 0;
                            symbol->total = 0;
                            symbol->parent = 0;
                        }
                        ;
                        (root)->symbols = (enet_uint16)(symbol - (root));
                    }
                    else
                    {
                        ENetSymbol* node = (root) + (root)->symbols;
                        for (;;)
                        {
                            if (value < node->value)
                            {
                                node->under += (enet_uint16)ENET_CONTEXT_SYMBOL_DELTA;
                                if (node->left != 0)
                                {
                                    node += node->left;
                                    continue;
                                }

                                {
                                    symbol = &rangeCoder->symbols[(ssize_t)(nextSymbol++)];
                                    symbol->value = value;
                                    symbol->count = (enet_uint8)ENET_CONTEXT_SYMBOL_DELTA;
                                    symbol->under = (enet_uint16)ENET_CONTEXT_SYMBOL_DELTA;
                                    symbol->left = 0;
                                    symbol->right = 0;
                                    symbol->symbols = 0;
                                    symbol->escapes = 0;
                                    symbol->total = 0;
                                    symbol->parent = 0;
                                }
                                ;
                                node->left = (enet_uint16)(symbol - node);
                            }
                            else if (value > node->value)
                            {
                                under += node->under;
                                if (node->right != 0)
                                {
                                    node += node->right;
                                    continue;
                                }

                                {
                                    symbol = &rangeCoder->symbols[(ssize_t)(nextSymbol++)];
                                    symbol->value = value;
                                    symbol->count = (enet_uint8)ENET_CONTEXT_SYMBOL_DELTA;
                                    symbol->under = (enet_uint16)ENET_CONTEXT_SYMBOL_DELTA;
                                    symbol->left = 0;
                                    symbol->right = 0;
                                    symbol->symbols = 0;
                                    symbol->escapes = 0;
                                    symbol->total = 0;
                                    symbol->parent = 0;
                                }
                                ;
                                node->right = (enet_uint16)(symbol - node);
                            }
                            else
                            {
                                count += node->count;
                                under += (enet_uint16)(node->under - node->count);
                                node->under += (enet_uint16)ENET_CONTEXT_SYMBOL_DELTA;
                                node->count += (enet_uint8)ENET_CONTEXT_SYMBOL_DELTA;
                                symbol = node;
                            }

                            break;
                        }
                    }
                }
                ;
                *parent = (enet_uint16)(symbol - &rangeCoder->symbols[0]);
                parent = &symbol->parent;
                total = root->total;

                {
                    encodeRange /= (total);
                    encodeLow += (enet_uint32)((root->escapes + under) * encodeRange);
                    encodeRange *= (count);
                    for (;;)
                    {
                        if ((encodeLow ^ (encodeLow + encodeRange)) >= ENET_RANGE_CODER_TOP)
                        {
                            if (encodeRange >= ENET_RANGE_CODER_BOTTOM) break;
                            encodeRange = (enet_uint32)(-encodeLow & (ENET_RANGE_CODER_BOTTOM - 1));
                        }

                        {
                            if (outData >= outEnd) return 0;
                            *outData++ = (enet_uint8)(encodeLow >> 24);
                        }
                        ;
                        encodeRange <<= 8;
                        encodeLow <<= 8;
                    }
                }
                ;
                root->total += (enet_uint16)ENET_CONTEXT_SYMBOL_DELTA;
                if (count > 0xFF - 2 * ENET_CONTEXT_SYMBOL_DELTA + ENET_CONTEXT_SYMBOL_MINIMUM || root->total > ENET_RANGE_CODER_BOTTOM - 0x100)
                {
                    (root)->total = ((root)->symbols != 0) ? enet_symbol_rescale((root) + (root)->symbols) : (enet_uint16)0;
                    (root)->escapes -= (enet_uint16)((root)->escapes >> 1);
                    (root)->total += (enet_uint16)((root)->escapes + 256 * ENET_CONTEXT_SYMBOL_MINIMUM);
                }

                ;

                nextInput:
                if (order >= ENET_SUBCONTEXT_ORDER)
                    predicted = rangeCoder->symbols[predicted].parent;
                else
                    order++;
                {
                    if ((enet_uint32)nextSymbol >= sizeof(ENetSymbols) / sizeof(ENetSymbol) - ENET_SUBCONTEXT_ORDER)
                    {
                        nextSymbol = 0;
                        {
                            {
                                root = &rangeCoder->symbols[(ssize_t)(nextSymbol++)];
                                root->value = 0;
                                root->count = 0;
                                root->under = 0;
                                root->left = 0;
                                root->right = 0;
                                root->symbols = 0;
                                root->escapes = 0;
                                root->total = 0;
                                root->parent = 0;
                            }
                            ;
                            (root)->escapes = (enet_uint16)ENET_CONTEXT_ESCAPE_MINIMUM;
                            (root)->total = (enet_uint16)(ENET_CONTEXT_ESCAPE_MINIMUM + 256 * ENET_CONTEXT_SYMBOL_MINIMUM);
                            (root)->symbols = 0;
                        }
                        ;
                        predicted = 0;
                        order = 0;
                    }
                }
                ;
            }

            {
                while (encodeLow != 0)
                {
                    {
                        if (outData >= outEnd) return 0;
                        *outData++ = (enet_uint8)(encodeLow >> 24);
                    }
                    ;
                    encodeLow <<= 8;
                }
            }
            ;

            return (size_t)(outData - outStart);
        }

        public static size_t enet_range_coder_decompress(void* context, enet_uint8* inData, size_t inLimit, enet_uint8* outData, size_t outLimit)
        {
            ENetRangeCoder* rangeCoder = (ENetRangeCoder*)context;
            enet_uint8* outStart = outData, outEnd = &outData[outLimit];
            enet_uint8* inEnd = &inData[inLimit];
            enet_uint32 decodeLow = 0, decodeCode = 0, decodeRange = unchecked((enet_uint32)(~0));
            ENetSymbol* root;
            enet_uint16 predicted = 0;
            size_t order = 0, nextSymbol = 0;
            if (rangeCoder == null || inLimit <= 0)
                return 0;

            {
                {
                    root = &rangeCoder->symbols[(ssize_t)(nextSymbol++)];
                    root->value = 0;
                    root->count = 0;
                    root->under = 0;
                    root->left = 0;
                    root->right = 0;
                    root->symbols = 0;
                    root->escapes = 0;
                    root->total = 0;
                    root->parent = 0;
                }
                ;
                (root)->escapes = (enet_uint16)ENET_CONTEXT_ESCAPE_MINIMUM;
                (root)->total = (enet_uint16)(ENET_CONTEXT_ESCAPE_MINIMUM + 256 * ENET_CONTEXT_SYMBOL_MINIMUM);
                (root)->symbols = 0;
            }
            ;

            {
                if (inData < inEnd) decodeCode |= (enet_uint32)(*inData++ << 24);
                if (inData < inEnd) decodeCode |= (enet_uint32)(*inData++ << 16);
                if (inData < inEnd) decodeCode |= (enet_uint32)(*inData++ << 8);
                if (inData < inEnd) decodeCode |= *inData++;
            }
            ;

            for (;;)
            {
                ENetSymbol* subcontext, symbol, patch;

                enet_uint8 value = 0;
                enet_uint16 code, under, count, bottom, total;
                enet_uint16* parent = &predicted;
                for (subcontext = &rangeCoder->symbols[predicted];
                     subcontext != root;
                     subcontext = &rangeCoder->symbols[subcontext->parent])
                {
                    if (subcontext->escapes <= 0)
                        continue;
                    total = subcontext->total;
                    if (subcontext->escapes >= total)
                        continue;
                    code = (enet_uint16)((decodeCode - decodeLow) / (decodeRange /= (total)));
                    if (code < subcontext->escapes)
                    {
                        {
                            decodeLow += (0) * decodeRange;
                            decodeRange *= (subcontext->escapes);
                            for (;;)
                            {
                                if ((decodeLow ^ (decodeLow + decodeRange)) >= ENET_RANGE_CODER_TOP)
                                {
                                    if (decodeRange >= ENET_RANGE_CODER_BOTTOM) break;
                                    decodeRange = (enet_uint32)(-decodeLow & (ENET_RANGE_CODER_BOTTOM - 1));
                                }

                                decodeCode <<= 8;
                                if (inData < inEnd) decodeCode |= *inData++;
                                decodeRange <<= 8;
                                decodeLow <<= 8;
                            }
                        }
                        ;
                        continue;
                    }

                    code -= subcontext->escapes;
                    {
                        {
                            under = 0;
                            count = 0;
                            if (!((subcontext)->symbols != 0))
                            {
                                return 0;
                            }
                            else
                            {
                                ENetSymbol* node = (subcontext) + (subcontext)->symbols;
                                for (;;)
                                {
                                    enet_uint16 after = (enet_uint16)(under + node->under + (node->value + 1) * 0), before = (enet_uint16)(node->count + 0);
                                    ;
                                    if (code >= after)
                                    {
                                        under += node->under;
                                        if (node->right != 0)
                                        {
                                            node += node->right;
                                            continue;
                                        }

                                        return 0;
                                    }
                                    else if (code < after - before)
                                    {
                                        node->under += (enet_uint16)ENET_SUBCONTEXT_SYMBOL_DELTA;
                                        if (node->left != 0)
                                        {
                                            node += node->left;
                                            continue;
                                        }

                                        return 0;
                                    }
                                    else
                                    {
                                        value = node->value;
                                        count += node->count;
                                        under = (enet_uint16)(after - before);
                                        node->under += (enet_uint16)ENET_SUBCONTEXT_SYMBOL_DELTA;
                                        node->count += (enet_uint8)ENET_SUBCONTEXT_SYMBOL_DELTA;
                                        symbol = node;
                                    }

                                    break;
                                }
                            }
                        }
                        ;
                    }
                    bottom = (enet_uint16)(symbol - &rangeCoder->symbols[0]);
                    {
                        decodeLow += (enet_uint32)((subcontext->escapes + under) * decodeRange);
                        decodeRange *= (count);
                        for (;;)
                        {
                            if ((decodeLow ^ (decodeLow + decodeRange)) >= ENET_RANGE_CODER_TOP)
                            {
                                if (decodeRange >= ENET_RANGE_CODER_BOTTOM) break;
                                decodeRange = (enet_uint32)(-decodeLow & (ENET_RANGE_CODER_BOTTOM - 1));
                            }

                            decodeCode <<= 8;
                            if (inData < inEnd) decodeCode |= *inData++;
                            decodeRange <<= 8;
                            decodeLow <<= 8;
                        }
                    }
                    ;
                    subcontext->total += (enet_uint16)ENET_SUBCONTEXT_SYMBOL_DELTA;
                    if (count > 0xFF - 2 * ENET_SUBCONTEXT_SYMBOL_DELTA || subcontext->total > ENET_RANGE_CODER_BOTTOM - 0x100)
                    {
                        (subcontext)->total = ((subcontext)->symbols != 0) ? enet_symbol_rescale((subcontext) + (subcontext)->symbols) : (enet_uint16)0;
                        (subcontext)->escapes -= (enet_uint16)((subcontext)->escapes >> 1);
                        (subcontext)->total += (enet_uint16)((subcontext)->escapes + 256 * 0);
                    }

                    ;
                    goto patchContexts;
                }

                total = root->total;
                code = (enet_uint16)((decodeCode - decodeLow) / (decodeRange /= (total)));
                if (code < root->escapes)
                {
                    {
                        decodeLow += (0) * decodeRange;
                        decodeRange *= (root->escapes);
                        for (;;)
                        {
                            if ((decodeLow ^ (decodeLow + decodeRange)) >= ENET_RANGE_CODER_TOP)
                            {
                                if (decodeRange >= ENET_RANGE_CODER_BOTTOM) break;
                                decodeRange = (enet_uint32)(-decodeLow & (ENET_RANGE_CODER_BOTTOM - 1));
                            }

                            decodeCode <<= 8;
                            if (inData < inEnd) decodeCode |= *inData++;
                            decodeRange <<= 8;
                            decodeLow <<= 8;
                        }
                    }
                    ;
                    break;
                }

                code -= root->escapes;
                {
                    {
                        under = 0;
                        count = (enet_uint16)ENET_CONTEXT_SYMBOL_MINIMUM;
                        if (!((root)->symbols != 0))
                        {
                            {
                                value = (enet_uint8)(code / ENET_CONTEXT_SYMBOL_MINIMUM);
                                under = (enet_uint16)(code - code % ENET_CONTEXT_SYMBOL_MINIMUM);
                                {
                                    symbol = &rangeCoder->symbols[(ssize_t)(nextSymbol++)];
                                    symbol->value = value;
                                    symbol->count = (enet_uint8)ENET_CONTEXT_SYMBOL_DELTA;
                                    symbol->under = (enet_uint16)ENET_CONTEXT_SYMBOL_DELTA;
                                    symbol->left = 0;
                                    symbol->right = 0;
                                    symbol->symbols = 0;
                                    symbol->escapes = 0;
                                    symbol->total = 0;
                                    symbol->parent = 0;
                                }
                                ;
                                (root)->symbols = (enet_uint16)(symbol - (root));
                            }
                            ;
                        }
                        else
                        {
                            ENetSymbol* node = (root) + (root)->symbols;
                            for (;;)
                            {
                                enet_uint16 after = (enet_uint16)(under + node->under + (node->value + 1) * ENET_CONTEXT_SYMBOL_MINIMUM), before = (enet_uint16)(node->count + ENET_CONTEXT_SYMBOL_MINIMUM);
                                ;
                                if (code >= after)
                                {
                                    under += node->under;
                                    if (node->right != 0)
                                    {
                                        node += node->right;
                                        continue;
                                    }

                                    {
                                        value = (enet_uint8)(node->value + 1 + (code - after) / ENET_CONTEXT_SYMBOL_MINIMUM);
                                        under = (enet_uint16)(code - (code - after) % ENET_CONTEXT_SYMBOL_MINIMUM);
                                        {
                                            symbol = &rangeCoder->symbols[(ssize_t)(nextSymbol++)];
                                            symbol->value = value;
                                            symbol->count = (enet_uint8)ENET_CONTEXT_SYMBOL_DELTA;
                                            symbol->under = (enet_uint16)ENET_CONTEXT_SYMBOL_DELTA;
                                            symbol->left = 0;
                                            symbol->right = 0;
                                            symbol->symbols = 0;
                                            symbol->escapes = 0;
                                            symbol->total = 0;
                                            symbol->parent = 0;
                                        }
                                        ;
                                        node->right = (enet_uint16)(symbol - node);
                                    }
                                    ;
                                }
                                else if (code < after - before)
                                {
                                    node->under += (enet_uint16)ENET_CONTEXT_SYMBOL_DELTA;
                                    if (node->left != 0)
                                    {
                                        node += node->left;
                                        continue;
                                    }

                                    {
                                        value = (enet_uint8)(node->value - 1 - (after - before - code - 1) / ENET_CONTEXT_SYMBOL_MINIMUM);
                                        under = (enet_uint16)(code - (after - before - code - 1) % ENET_CONTEXT_SYMBOL_MINIMUM);
                                        {
                                            symbol = &rangeCoder->symbols[(ssize_t)(nextSymbol++)];
                                            symbol->value = value;
                                            symbol->count = (enet_uint8)ENET_CONTEXT_SYMBOL_DELTA;
                                            symbol->under = (enet_uint16)ENET_CONTEXT_SYMBOL_DELTA;
                                            symbol->left = 0;
                                            symbol->right = 0;
                                            symbol->symbols = 0;
                                            symbol->escapes = 0;
                                            symbol->total = 0;
                                            symbol->parent = 0;
                                        }
                                        ;
                                        node->left = (enet_uint16)(symbol - node);
                                    }
                                    ;
                                }
                                else
                                {
                                    value = node->value;
                                    count += node->count;
                                    under = (enet_uint16)(after - before);
                                    node->under += (enet_uint16)ENET_CONTEXT_SYMBOL_DELTA;
                                    node->count += (enet_uint8)ENET_CONTEXT_SYMBOL_DELTA;
                                    symbol = node;
                                }

                                break;
                            }
                        }
                    }
                    ;
                }
                bottom = (enet_uint16)(symbol - &rangeCoder->symbols[0]);
                {
                    decodeLow += (enet_uint32)((root->escapes + under) * decodeRange);
                    decodeRange *= (count);
                    for (;;)
                    {
                        if ((decodeLow ^ (decodeLow + decodeRange)) >= ENET_RANGE_CODER_TOP)
                        {
                            if (decodeRange >= ENET_RANGE_CODER_BOTTOM) break;
                            decodeRange = (enet_uint32)(-decodeLow & (ENET_RANGE_CODER_BOTTOM - 1));
                        }

                        decodeCode <<= 8;
                        if (inData < inEnd) decodeCode |= *inData++;
                        decodeRange <<= 8;
                        decodeLow <<= 8;
                    }
                }
                ;
                root->total += (enet_uint16)ENET_CONTEXT_SYMBOL_DELTA;
                if (count > 0xFF - 2 * ENET_CONTEXT_SYMBOL_DELTA + ENET_CONTEXT_SYMBOL_MINIMUM || root->total > ENET_RANGE_CODER_BOTTOM - 0x100)
                {
                    (root)->total = ((root)->symbols != 0) ? enet_symbol_rescale((root) + (root)->symbols) : (enet_uint16)0;
                    (root)->escapes -= (enet_uint16)((root)->escapes >> 1);
                    (root)->total += (enet_uint16)((root)->escapes + 256 * ENET_CONTEXT_SYMBOL_MINIMUM);
                }

                ;

                patchContexts:
                for (patch = &rangeCoder->symbols[predicted];
                     patch != subcontext;
                     patch = &rangeCoder->symbols[patch->parent])
                {
                    {
                        under = (enet_uint16)(value * 0);
                        count = 0;
                        if (!((patch)->symbols != 0))
                        {
                            {
                                symbol = &rangeCoder->symbols[(ssize_t)(nextSymbol++)];
                                symbol->value = value;
                                symbol->count = (enet_uint8)ENET_SUBCONTEXT_SYMBOL_DELTA;
                                symbol->under = (enet_uint16)ENET_SUBCONTEXT_SYMBOL_DELTA;
                                symbol->left = 0;
                                symbol->right = 0;
                                symbol->symbols = 0;
                                symbol->escapes = 0;
                                symbol->total = 0;
                                symbol->parent = 0;
                            }
                            ;
                            (patch)->symbols = (enet_uint16)(symbol - (patch));
                        }
                        else
                        {
                            ENetSymbol* node = (patch) + (patch)->symbols;
                            for (;;)
                            {
                                if (value < node->value)
                                {
                                    node->under += (enet_uint16)ENET_SUBCONTEXT_SYMBOL_DELTA;
                                    if (node->left != 0)
                                    {
                                        node += node->left;
                                        continue;
                                    }

                                    {
                                        symbol = &rangeCoder->symbols[(ssize_t)(nextSymbol++)];
                                        symbol->value = value;
                                        symbol->count = (enet_uint8)ENET_SUBCONTEXT_SYMBOL_DELTA;
                                        symbol->under = (enet_uint16)ENET_SUBCONTEXT_SYMBOL_DELTA;
                                        symbol->left = 0;
                                        symbol->right = 0;
                                        symbol->symbols = 0;
                                        symbol->escapes = 0;
                                        symbol->total = 0;
                                        symbol->parent = 0;
                                    }
                                    ;
                                    node->left = (enet_uint16)(symbol - node);
                                }
                                else if (value > node->value)
                                {
                                    under += node->under;
                                    if (node->right != 0)
                                    {
                                        node += node->right;
                                        continue;
                                    }

                                    {
                                        symbol = &rangeCoder->symbols[(ssize_t)(nextSymbol++)];
                                        symbol->value = value;
                                        symbol->count = (enet_uint8)ENET_SUBCONTEXT_SYMBOL_DELTA;
                                        symbol->under = (enet_uint16)ENET_SUBCONTEXT_SYMBOL_DELTA;
                                        symbol->left = 0;
                                        symbol->right = 0;
                                        symbol->symbols = 0;
                                        symbol->escapes = 0;
                                        symbol->total = 0;
                                        symbol->parent = 0;
                                    }
                                    ;
                                    node->right = (enet_uint16)(symbol - node);
                                }
                                else
                                {
                                    count += node->count;
                                    under += (enet_uint16)(node->under - node->count);
                                    node->under += (enet_uint16)ENET_SUBCONTEXT_SYMBOL_DELTA;
                                    node->count += (enet_uint8)ENET_SUBCONTEXT_SYMBOL_DELTA;
                                    symbol = node;
                                }

                                break;
                            }
                        }
                    }
                    ;
                    *parent = (enet_uint16)(symbol - &rangeCoder->symbols[0]);
                    parent = &symbol->parent;
                    if (count <= 0)
                    {
                        patch->escapes += (enet_uint16)ENET_SUBCONTEXT_ESCAPE_DELTA;
                        patch->total += (enet_uint16)ENET_SUBCONTEXT_ESCAPE_DELTA;
                    }

                    patch->total += (enet_uint16)ENET_SUBCONTEXT_SYMBOL_DELTA;
                    if (count > 0xFF - 2 * ENET_SUBCONTEXT_SYMBOL_DELTA || patch->total > ENET_RANGE_CODER_BOTTOM - 0x100)
                    {
                        (patch)->total = ((patch)->symbols != 0) ? enet_symbol_rescale((patch) + (patch)->symbols) : (enet_uint16)0;
                        (patch)->escapes -= (enet_uint16)((patch)->escapes >> 1);
                        (patch)->total += (enet_uint16)((patch)->escapes + 256 * 0);
                    }

                    ;
                }

                *parent = bottom;

                {
                    if (outData >= outEnd) return 0;
                    *outData++ = value;
                }
                ;

                if (order >= ENET_SUBCONTEXT_ORDER)
                    predicted = rangeCoder->symbols[predicted].parent;
                else
                    order++;
                {
                    if ((enet_uint32)nextSymbol >= sizeof(ENetSymbols) / sizeof(ENetSymbol) - ENET_SUBCONTEXT_ORDER)
                    {
                        nextSymbol = 0;
                        {
                            {
                                root = &rangeCoder->symbols[(ssize_t)(nextSymbol++)];
                                root->value = 0;
                                root->count = 0;
                                root->under = 0;
                                root->left = 0;
                                root->right = 0;
                                root->symbols = 0;
                                root->escapes = 0;
                                root->total = 0;
                                root->parent = 0;
                            }
                            ;
                            (root)->escapes = (enet_uint16)ENET_CONTEXT_ESCAPE_MINIMUM;
                            (root)->total = (enet_uint16)(ENET_CONTEXT_ESCAPE_MINIMUM + 256 * ENET_CONTEXT_SYMBOL_MINIMUM);
                            (root)->symbols = 0;
                        }
                        ;
                        predicted = 0;
                        order = 0;
                    }
                }
                ;
            }

            return (size_t)(outData - outStart);
        }

        public static int enet_host_compress_with_range_coder(ENetHost* host)
        {
            ENetCompressor compressor;
            memset(&compressor, 0, (size_t)sizeof(ENetCompressor));
            compressor.context = enet_range_coder_create();
            if (compressor.context == null)
                return -1;
            compressor.compress = &enet_range_coder_compress;
            compressor.decompress = &enet_range_coder_decompress;
            compressor.destroy = &enet_range_coder_destroy;
            enet_host_compress(host, &compressor);
            return 0;
        }
    }
}