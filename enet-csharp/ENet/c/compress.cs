using System.Runtime.CompilerServices;

#pragma warning disable CS1591

// ReSharper disable ALL

namespace enet
{
    public partial class ENet
    {
        /* adaptation constants tuned aggressively for small packet sizes rather than large file compression */

        public const uint ENET_RANGE_CODER_TOP = 1 << 24;
        public const uint ENET_RANGE_CODER_BOTTOM = 1 << 16;

        public const uint ENET_CONTEXT_SYMBOL_DELTA = 3;
        public const uint ENET_CONTEXT_SYMBOL_MINIMUM = 1;
        public const uint ENET_CONTEXT_ESCAPE_MINIMUM = 1;

        public const uint ENET_SUBCONTEXT_ORDER = 2;
        public const uint ENET_SUBCONTEXT_SYMBOL_DELTA = 2;
        public const uint ENET_SUBCONTEXT_ESCAPE_DELTA = 5;
    }

    public unsafe struct ENetRangeCoder
    {
        /* only allocate enough symbols for reasonable MTUs, would need to be larger for large file compression */
        public ENetSymbols symbols_t;
        public ENetSymbol* symbols => (ENetSymbol*)Unsafe.AsPointer(ref symbols_t);
    }

    public unsafe partial class ENet
    {
        public static void* enet_range_coder_create()
        {
            ENetRangeCoder* rangeCoder = (ENetRangeCoder*)enet_malloc((nuint)sizeof(ENetRangeCoder));
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

        public static ushort enet_symbol_rescale(ENetSymbol* symbol)
        {
            ushort total = 0;
            for (;;)
            {
                symbol->count -= (byte)(symbol->count >> 1);
                symbol->under = symbol->count;
                if (symbol->left != 0)
                    symbol->under += enet_symbol_rescale(symbol + symbol->left);
                total += symbol->under;
                if (!(symbol->right != 0)) break;
                symbol += symbol->right;
            }

            return total;
        }

        public static nuint enet_range_coder_compress(void* context, ENetBuffer* inBuffers, nuint inBufferCount, nuint inLimit, byte* outData, nuint outLimit)
        {
            ENetRangeCoder* rangeCoder = (ENetRangeCoder*)context;
            byte* outStart = outData, outEnd = &outData[outLimit];
            byte* inData, inEnd;
            uint encodeLow = 0, encodeRange = unchecked((uint)(~0));
            ENetSymbol* root;
            ushort predicted = 0;
            nuint order = 0, nextSymbol = 0;

            if (rangeCoder == null || inBufferCount <= 0 || inLimit <= 0)
                return 0;

            inData = (byte*)inBuffers->data;
            inEnd = &inData[inBuffers->dataLength];
            inBuffers++;
            inBufferCount--;

            {
                {
                    root = &rangeCoder->symbols[(nint)(nextSymbol++)];
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
                (root)->escapes = (ushort)ENET_CONTEXT_ESCAPE_MINIMUM;
                (root)->total = (ushort)(ENET_CONTEXT_ESCAPE_MINIMUM + 256 * ENET_CONTEXT_SYMBOL_MINIMUM);
                (root)->symbols = 0;
            }
            ;

            for (;;)
            {
                ENetSymbol* subcontext, symbol;

                byte value;
                ushort count, under, total;
                ushort* parent = &predicted;
                if (inData >= inEnd)
                {
                    if (inBufferCount <= 0)
                        break;
                    inData = (byte*)inBuffers->data;
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
                        under = (ushort)(value * 0);
                        count = 0;
                        if (!((subcontext)->symbols != 0))
                        {
                            {
                                symbol = &rangeCoder->symbols[(nint)(nextSymbol++)];
                                symbol->value = value;
                                symbol->count = (byte)ENET_SUBCONTEXT_SYMBOL_DELTA;
                                symbol->under = (ushort)ENET_SUBCONTEXT_SYMBOL_DELTA;
                                symbol->left = 0;
                                symbol->right = 0;
                                symbol->symbols = 0;
                                symbol->escapes = 0;
                                symbol->total = 0;
                                symbol->parent = 0;
                            }
                            ;
                            (subcontext)->symbols = (ushort)(symbol - (subcontext));
                        }
                        else
                        {
                            ENetSymbol* node = (subcontext) + (subcontext)->symbols;
                            for (;;)
                            {
                                if (value < node->value)
                                {
                                    node->under += (ushort)ENET_SUBCONTEXT_SYMBOL_DELTA;
                                    if (node->left != 0)
                                    {
                                        node += node->left;
                                        continue;
                                    }

                                    {
                                        symbol = &rangeCoder->symbols[(nint)(nextSymbol++)];
                                        symbol->value = value;
                                        symbol->count = (byte)ENET_SUBCONTEXT_SYMBOL_DELTA;
                                        symbol->under = (ushort)ENET_SUBCONTEXT_SYMBOL_DELTA;
                                        symbol->left = 0;
                                        symbol->right = 0;
                                        symbol->symbols = 0;
                                        symbol->escapes = 0;
                                        symbol->total = 0;
                                        symbol->parent = 0;
                                    }
                                    ;
                                    node->left = (ushort)(symbol - node);
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
                                        symbol = &rangeCoder->symbols[(nint)(nextSymbol++)];
                                        symbol->value = value;
                                        symbol->count = (byte)ENET_SUBCONTEXT_SYMBOL_DELTA;
                                        symbol->under = (ushort)ENET_SUBCONTEXT_SYMBOL_DELTA;
                                        symbol->left = 0;
                                        symbol->right = 0;
                                        symbol->symbols = 0;
                                        symbol->escapes = 0;
                                        symbol->total = 0;
                                        symbol->parent = 0;
                                    }
                                    ;
                                    node->right = (ushort)(symbol - node);
                                }
                                else
                                {
                                    count += node->count;
                                    under += (ushort)(node->under - node->count);
                                    node->under += (ushort)ENET_SUBCONTEXT_SYMBOL_DELTA;
                                    node->count += (byte)ENET_SUBCONTEXT_SYMBOL_DELTA;
                                    symbol = node;
                                }

                                break;
                            }
                        }
                    }
                    ;
                    *parent = (ushort)(symbol - rangeCoder->symbols);
                    parent = &symbol->parent;
                    total = subcontext->total;

                    if (count > 0)
                    {
                        {
                            encodeRange /= (total);
                            encodeLow += (uint)((subcontext->escapes + under) * encodeRange);
                            encodeRange *= (count);
                            for (;;)
                            {
                                if ((encodeLow ^ (encodeLow + encodeRange)) >= ENET_RANGE_CODER_TOP)
                                {
                                    if (encodeRange >= ENET_RANGE_CODER_BOTTOM) break;
                                    encodeRange = (uint)(-encodeLow & (ENET_RANGE_CODER_BOTTOM - 1));
                                }

                                {
                                    if (outData >= outEnd) return 0;
                                    *outData++ = (byte)(encodeLow >> 24);
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
                                    encodeRange = (uint)(-encodeLow & (ENET_RANGE_CODER_BOTTOM - 1));
                                }

                                {
                                    if (outData >= outEnd) return 0;
                                    *outData++ = (byte)(encodeLow >> 24);
                                }
                                ;
                                encodeRange <<= 8;
                                encodeLow <<= 8;
                            }
                        }

                        ;
                        subcontext->escapes += (ushort)ENET_SUBCONTEXT_ESCAPE_DELTA;
                        subcontext->total += (ushort)ENET_SUBCONTEXT_ESCAPE_DELTA;
                    }

                    subcontext->total += (ushort)ENET_SUBCONTEXT_SYMBOL_DELTA;
                    if (count > 0xFF - 2 * ENET_SUBCONTEXT_SYMBOL_DELTA || subcontext->total > ENET_RANGE_CODER_BOTTOM - 0x100)
                    {
                        (subcontext)->total = ((subcontext)->symbols != 0) ? ((ushort)enet_symbol_rescale((subcontext) + (subcontext)->symbols)) : (ushort)0;
                        (subcontext)->escapes -= (ushort)((subcontext)->escapes >> 1);
                        (subcontext)->total += (ushort)((subcontext)->escapes + 256 * 0);
                    }

                    ;
                    if (count > 0) goto nextInput;
                }

                {
                    under = (ushort)(value * ENET_CONTEXT_SYMBOL_MINIMUM);
                    count = (ushort)ENET_CONTEXT_SYMBOL_MINIMUM;
                    if (!((root)->symbols != 0))
                    {
                        {
                            symbol = &rangeCoder->symbols[(nint)(nextSymbol++)];
                            symbol->value = value;
                            symbol->count = (byte)ENET_CONTEXT_SYMBOL_DELTA;
                            symbol->under = (ushort)ENET_CONTEXT_SYMBOL_DELTA;
                            symbol->left = 0;
                            symbol->right = 0;
                            symbol->symbols = 0;
                            symbol->escapes = 0;
                            symbol->total = 0;
                            symbol->parent = 0;
                        }
                        ;
                        (root)->symbols = (ushort)(symbol - (root));
                    }
                    else
                    {
                        ENetSymbol* node = (root) + (root)->symbols;
                        for (;;)
                        {
                            if (value < node->value)
                            {
                                node->under += (ushort)ENET_CONTEXT_SYMBOL_DELTA;
                                if (node->left != 0)
                                {
                                    node += node->left;
                                    continue;
                                }

                                {
                                    symbol = &rangeCoder->symbols[(nint)(nextSymbol++)];
                                    symbol->value = value;
                                    symbol->count = (byte)ENET_CONTEXT_SYMBOL_DELTA;
                                    symbol->under = (ushort)ENET_CONTEXT_SYMBOL_DELTA;
                                    symbol->left = 0;
                                    symbol->right = 0;
                                    symbol->symbols = 0;
                                    symbol->escapes = 0;
                                    symbol->total = 0;
                                    symbol->parent = 0;
                                }
                                ;
                                node->left = (ushort)(symbol - node);
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
                                    symbol = &rangeCoder->symbols[(nint)(nextSymbol++)];
                                    symbol->value = value;
                                    symbol->count = (byte)ENET_CONTEXT_SYMBOL_DELTA;
                                    symbol->under = (ushort)ENET_CONTEXT_SYMBOL_DELTA;
                                    symbol->left = 0;
                                    symbol->right = 0;
                                    symbol->symbols = 0;
                                    symbol->escapes = 0;
                                    symbol->total = 0;
                                    symbol->parent = 0;
                                }
                                ;
                                node->right = (ushort)(symbol - node);
                            }
                            else
                            {
                                count += node->count;
                                under += (ushort)(node->under - node->count);
                                node->under += (ushort)ENET_CONTEXT_SYMBOL_DELTA;
                                node->count += (byte)ENET_CONTEXT_SYMBOL_DELTA;
                                symbol = node;
                            }

                            break;
                        }
                    }
                }
                ;
                *parent = (ushort)(symbol - rangeCoder->symbols);
                parent = &symbol->parent;
                total = root->total;

                {
                    encodeRange /= (total);
                    encodeLow += (uint)((root->escapes + under) * encodeRange);
                    encodeRange *= (count);
                    for (;;)
                    {
                        if ((encodeLow ^ (encodeLow + encodeRange)) >= ENET_RANGE_CODER_TOP)
                        {
                            if (encodeRange >= ENET_RANGE_CODER_BOTTOM) break;
                            encodeRange = (uint)(-encodeLow & (ENET_RANGE_CODER_BOTTOM - 1));
                        }

                        {
                            if (outData >= outEnd) return 0;
                            *outData++ = (byte)(encodeLow >> 24);
                        }
                        ;
                        encodeRange <<= 8;
                        encodeLow <<= 8;
                    }
                }
                ;
                root->total += (ushort)ENET_CONTEXT_SYMBOL_DELTA;
                if (count > 0xFF - 2 * ENET_CONTEXT_SYMBOL_DELTA + ENET_CONTEXT_SYMBOL_MINIMUM || root->total > ENET_RANGE_CODER_BOTTOM - 0x100)
                {
                    (root)->total = ((root)->symbols != 0) ? enet_symbol_rescale((root) + (root)->symbols) : (ushort)0;
                    (root)->escapes -= (ushort)((root)->escapes >> 1);
                    (root)->total += (ushort)((root)->escapes + 256 * ENET_CONTEXT_SYMBOL_MINIMUM);
                }

                ;

                nextInput:
                if (order >= ENET_SUBCONTEXT_ORDER)
                    predicted = rangeCoder->symbols[predicted].parent;
                else
                    order++;
                {
                    if ((uint)nextSymbol >= sizeof(ENetSymbols) / sizeof(ENetSymbol) - ENET_SUBCONTEXT_ORDER)
                    {
                        nextSymbol = 0;
                        {
                            {
                                root = &rangeCoder->symbols[(nint)(nextSymbol++)];
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
                            (root)->escapes = (ushort)ENET_CONTEXT_ESCAPE_MINIMUM;
                            (root)->total = (ushort)(ENET_CONTEXT_ESCAPE_MINIMUM + 256 * ENET_CONTEXT_SYMBOL_MINIMUM);
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
                        *outData++ = (byte)(encodeLow >> 24);
                    }
                    ;
                    encodeLow <<= 8;
                }
            }
            ;

            return (nuint)(outData - outStart);
        }

        public static nuint enet_range_coder_decompress(void* context, byte* inData, nuint inLimit, byte* outData, nuint outLimit)
        {
            ENetRangeCoder* rangeCoder = (ENetRangeCoder*)context;
            byte* outStart = outData, outEnd = &outData[outLimit];
            byte* inEnd = &inData[inLimit];
            uint decodeLow = 0, decodeCode = 0, decodeRange = unchecked((uint)(~0));
            ENetSymbol* root;
            ushort predicted = 0;
            nuint order = 0, nextSymbol = 0;
            if (rangeCoder == null || inLimit <= 0)
                return 0;

            {
                {
                    root = &rangeCoder->symbols[(nint)(nextSymbol++)];
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
                (root)->escapes = (ushort)ENET_CONTEXT_ESCAPE_MINIMUM;
                (root)->total = (ushort)(ENET_CONTEXT_ESCAPE_MINIMUM + 256 * ENET_CONTEXT_SYMBOL_MINIMUM);
                (root)->symbols = 0;
            }
            ;

            {
                if (inData < inEnd) decodeCode |= (uint)(*inData++ << 24);
                if (inData < inEnd) decodeCode |= (uint)(*inData++ << 16);
                if (inData < inEnd) decodeCode |= (uint)(*inData++ << 8);
                if (inData < inEnd) decodeCode |= *inData++;
            }
            ;

            for (;;)
            {
                ENetSymbol* subcontext, symbol, patch;

                byte value = 0;
                ushort code, under, count, bottom, total;
                ushort* parent = &predicted;
                for (subcontext = &rangeCoder->symbols[predicted];
                     subcontext != root;
                     subcontext = &rangeCoder->symbols[subcontext->parent])
                {
                    if (subcontext->escapes <= 0)
                        continue;
                    total = subcontext->total;
                    if (subcontext->escapes >= total)
                        continue;
                    code = (ushort)((decodeCode - decodeLow) / (decodeRange /= (total)));
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
                                    decodeRange = (uint)(-decodeLow & (ENET_RANGE_CODER_BOTTOM - 1));
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
                                    ushort after = (ushort)(under + node->under + (node->value + 1) * 0), before = (ushort)(node->count + 0);
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
                                        node->under += (ushort)ENET_SUBCONTEXT_SYMBOL_DELTA;
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
                                        under = (ushort)(after - before);
                                        node->under += (ushort)ENET_SUBCONTEXT_SYMBOL_DELTA;
                                        node->count += (byte)ENET_SUBCONTEXT_SYMBOL_DELTA;
                                        symbol = node;
                                    }

                                    break;
                                }
                            }
                        }
                        ;
                    }
                    bottom = (ushort)(symbol - rangeCoder->symbols);
                    {
                        decodeLow += (uint)((subcontext->escapes + under) * decodeRange);
                        decodeRange *= (count);
                        for (;;)
                        {
                            if ((decodeLow ^ (decodeLow + decodeRange)) >= ENET_RANGE_CODER_TOP)
                            {
                                if (decodeRange >= ENET_RANGE_CODER_BOTTOM) break;
                                decodeRange = (uint)(-decodeLow & (ENET_RANGE_CODER_BOTTOM - 1));
                            }

                            decodeCode <<= 8;
                            if (inData < inEnd) decodeCode |= *inData++;
                            decodeRange <<= 8;
                            decodeLow <<= 8;
                        }
                    }
                    ;
                    subcontext->total += (ushort)ENET_SUBCONTEXT_SYMBOL_DELTA;
                    if (count > 0xFF - 2 * ENET_SUBCONTEXT_SYMBOL_DELTA || subcontext->total > ENET_RANGE_CODER_BOTTOM - 0x100)
                    {
                        (subcontext)->total = ((subcontext)->symbols != 0) ? enet_symbol_rescale((subcontext) + (subcontext)->symbols) : (ushort)0;
                        (subcontext)->escapes -= (ushort)((subcontext)->escapes >> 1);
                        (subcontext)->total += (ushort)((subcontext)->escapes + 256 * 0);
                    }

                    ;
                    goto patchContexts;
                }

                total = root->total;
                code = (ushort)((decodeCode - decodeLow) / (decodeRange /= (total)));
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
                                decodeRange = (uint)(-decodeLow & (ENET_RANGE_CODER_BOTTOM - 1));
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
                        count = (ushort)ENET_CONTEXT_SYMBOL_MINIMUM;
                        if (!((root)->symbols != 0))
                        {
                            {
                                value = (byte)(code / ENET_CONTEXT_SYMBOL_MINIMUM);
                                under = (ushort)(code - code % ENET_CONTEXT_SYMBOL_MINIMUM);
                                {
                                    symbol = &rangeCoder->symbols[(nint)(nextSymbol++)];
                                    symbol->value = value;
                                    symbol->count = (byte)ENET_CONTEXT_SYMBOL_DELTA;
                                    symbol->under = (ushort)ENET_CONTEXT_SYMBOL_DELTA;
                                    symbol->left = 0;
                                    symbol->right = 0;
                                    symbol->symbols = 0;
                                    symbol->escapes = 0;
                                    symbol->total = 0;
                                    symbol->parent = 0;
                                }
                                ;
                                (root)->symbols = (ushort)(symbol - (root));
                            }
                            ;
                        }
                        else
                        {
                            ENetSymbol* node = (root) + (root)->symbols;
                            for (;;)
                            {
                                ushort after = (ushort)(under + node->under + (node->value + 1) * ENET_CONTEXT_SYMBOL_MINIMUM), before = (ushort)(node->count + ENET_CONTEXT_SYMBOL_MINIMUM);
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
                                        value = (byte)(node->value + 1 + (code - after) / ENET_CONTEXT_SYMBOL_MINIMUM);
                                        under = (ushort)(code - (code - after) % ENET_CONTEXT_SYMBOL_MINIMUM);
                                        {
                                            symbol = &rangeCoder->symbols[(nint)(nextSymbol++)];
                                            symbol->value = value;
                                            symbol->count = (byte)ENET_CONTEXT_SYMBOL_DELTA;
                                            symbol->under = (ushort)ENET_CONTEXT_SYMBOL_DELTA;
                                            symbol->left = 0;
                                            symbol->right = 0;
                                            symbol->symbols = 0;
                                            symbol->escapes = 0;
                                            symbol->total = 0;
                                            symbol->parent = 0;
                                        }
                                        ;
                                        node->right = (ushort)(symbol - node);
                                    }
                                    ;
                                }
                                else if (code < after - before)
                                {
                                    node->under += (ushort)ENET_CONTEXT_SYMBOL_DELTA;
                                    if (node->left != 0)
                                    {
                                        node += node->left;
                                        continue;
                                    }

                                    {
                                        value = (byte)(node->value - 1 - (after - before - code - 1) / ENET_CONTEXT_SYMBOL_MINIMUM);
                                        under = (ushort)(code - (after - before - code - 1) % ENET_CONTEXT_SYMBOL_MINIMUM);
                                        {
                                            symbol = &rangeCoder->symbols[(nint)(nextSymbol++)];
                                            symbol->value = value;
                                            symbol->count = (byte)ENET_CONTEXT_SYMBOL_DELTA;
                                            symbol->under = (ushort)ENET_CONTEXT_SYMBOL_DELTA;
                                            symbol->left = 0;
                                            symbol->right = 0;
                                            symbol->symbols = 0;
                                            symbol->escapes = 0;
                                            symbol->total = 0;
                                            symbol->parent = 0;
                                        }
                                        ;
                                        node->left = (ushort)(symbol - node);
                                    }
                                    ;
                                }
                                else
                                {
                                    value = node->value;
                                    count += node->count;
                                    under = (ushort)(after - before);
                                    node->under += (ushort)ENET_CONTEXT_SYMBOL_DELTA;
                                    node->count += (byte)ENET_CONTEXT_SYMBOL_DELTA;
                                    symbol = node;
                                }

                                break;
                            }
                        }
                    }
                    ;
                }
                bottom = (ushort)(symbol - rangeCoder->symbols);
                {
                    decodeLow += (uint)((root->escapes + under) * decodeRange);
                    decodeRange *= (count);
                    for (;;)
                    {
                        if ((decodeLow ^ (decodeLow + decodeRange)) >= ENET_RANGE_CODER_TOP)
                        {
                            if (decodeRange >= ENET_RANGE_CODER_BOTTOM) break;
                            decodeRange = (uint)(-decodeLow & (ENET_RANGE_CODER_BOTTOM - 1));
                        }

                        decodeCode <<= 8;
                        if (inData < inEnd) decodeCode |= *inData++;
                        decodeRange <<= 8;
                        decodeLow <<= 8;
                    }
                }
                ;
                root->total += (ushort)ENET_CONTEXT_SYMBOL_DELTA;
                if (count > 0xFF - 2 * ENET_CONTEXT_SYMBOL_DELTA + ENET_CONTEXT_SYMBOL_MINIMUM || root->total > ENET_RANGE_CODER_BOTTOM - 0x100)
                {
                    (root)->total = ((root)->symbols != 0) ? enet_symbol_rescale((root) + (root)->symbols) : (ushort)0;
                    (root)->escapes -= (ushort)((root)->escapes >> 1);
                    (root)->total += (ushort)((root)->escapes + 256 * ENET_CONTEXT_SYMBOL_MINIMUM);
                }

                ;

                patchContexts:
                for (patch = &rangeCoder->symbols[predicted];
                     patch != subcontext;
                     patch = &rangeCoder->symbols[patch->parent])
                {
                    {
                        under = (ushort)(value * 0);
                        count = 0;
                        if (!((patch)->symbols != 0))
                        {
                            {
                                symbol = &rangeCoder->symbols[(nint)(nextSymbol++)];
                                symbol->value = value;
                                symbol->count = (byte)ENET_SUBCONTEXT_SYMBOL_DELTA;
                                symbol->under = (ushort)ENET_SUBCONTEXT_SYMBOL_DELTA;
                                symbol->left = 0;
                                symbol->right = 0;
                                symbol->symbols = 0;
                                symbol->escapes = 0;
                                symbol->total = 0;
                                symbol->parent = 0;
                            }
                            ;
                            (patch)->symbols = (ushort)(symbol - (patch));
                        }
                        else
                        {
                            ENetSymbol* node = (patch) + (patch)->symbols;
                            for (;;)
                            {
                                if (value < node->value)
                                {
                                    node->under += (ushort)ENET_SUBCONTEXT_SYMBOL_DELTA;
                                    if (node->left != 0)
                                    {
                                        node += node->left;
                                        continue;
                                    }

                                    {
                                        symbol = &rangeCoder->symbols[(nint)(nextSymbol++)];
                                        symbol->value = value;
                                        symbol->count = (byte)ENET_SUBCONTEXT_SYMBOL_DELTA;
                                        symbol->under = (ushort)ENET_SUBCONTEXT_SYMBOL_DELTA;
                                        symbol->left = 0;
                                        symbol->right = 0;
                                        symbol->symbols = 0;
                                        symbol->escapes = 0;
                                        symbol->total = 0;
                                        symbol->parent = 0;
                                    }
                                    ;
                                    node->left = (ushort)(symbol - node);
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
                                        symbol = &rangeCoder->symbols[(nint)(nextSymbol++)];
                                        symbol->value = value;
                                        symbol->count = (byte)ENET_SUBCONTEXT_SYMBOL_DELTA;
                                        symbol->under = (ushort)ENET_SUBCONTEXT_SYMBOL_DELTA;
                                        symbol->left = 0;
                                        symbol->right = 0;
                                        symbol->symbols = 0;
                                        symbol->escapes = 0;
                                        symbol->total = 0;
                                        symbol->parent = 0;
                                    }
                                    ;
                                    node->right = (ushort)(symbol - node);
                                }
                                else
                                {
                                    count += node->count;
                                    under += (ushort)(node->under - node->count);
                                    node->under += (ushort)ENET_SUBCONTEXT_SYMBOL_DELTA;
                                    node->count += (byte)ENET_SUBCONTEXT_SYMBOL_DELTA;
                                    symbol = node;
                                }

                                break;
                            }
                        }
                    }
                    ;
                    *parent = (ushort)(symbol - rangeCoder->symbols);
                    parent = &symbol->parent;
                    if (count <= 0)
                    {
                        patch->escapes += (ushort)ENET_SUBCONTEXT_ESCAPE_DELTA;
                        patch->total += (ushort)ENET_SUBCONTEXT_ESCAPE_DELTA;
                    }

                    patch->total += (ushort)ENET_SUBCONTEXT_SYMBOL_DELTA;
                    if (count > 0xFF - 2 * ENET_SUBCONTEXT_SYMBOL_DELTA || patch->total > ENET_RANGE_CODER_BOTTOM - 0x100)
                    {
                        (patch)->total = ((patch)->symbols != 0) ? enet_symbol_rescale((patch) + (patch)->symbols) : (ushort)0;
                        (patch)->escapes -= (ushort)((patch)->escapes >> 1);
                        (patch)->total += (ushort)((patch)->escapes + 256 * 0);
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
                    if ((uint)nextSymbol >= sizeof(ENetSymbols) / sizeof(ENetSymbol) - ENET_SUBCONTEXT_ORDER)
                    {
                        nextSymbol = 0;
                        {
                            {
                                root = &rangeCoder->symbols[(nint)(nextSymbol++)];
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
                            (root)->escapes = (ushort)ENET_CONTEXT_ESCAPE_MINIMUM;
                            (root)->total = (ushort)(ENET_CONTEXT_ESCAPE_MINIMUM + 256 * ENET_CONTEXT_SYMBOL_MINIMUM);
                            (root)->symbols = 0;
                        }
                        ;
                        predicted = 0;
                        order = 0;
                    }
                }
                ;
            }

            return (nuint)(outData - outStart);
        }

        /// <summary>
        ///     Sets the packet compressor the host should use to the default range coder.
        /// </summary>
        /// <param name="host">host to enable the range coder for</param>
        /// <returns>0 on success, &lt; 0 on failure</returns>
        public static int enet_host_compress_with_range_coder(ENetHost* host)
        {
            ENetCompressor compressor;
            memset(&compressor, 0, (nuint)sizeof(ENetCompressor));
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