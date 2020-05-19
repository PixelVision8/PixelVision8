/****************************************************************************
 * NVorbis                                                                  *
 * Copyright (C) 2014, Andrew Ward <afward@gmail.com>                       *
 *                                                                          *
 * See COPYING for license terms (Ms-PL).                                   *
 *                                                                          *
 ***************************************************************************/
using System;
using System.Linq;
using System.IO;

namespace NVorbis
{
    class VorbisCodebook
    {
        internal static VorbisCodebook Init(VorbisStreamDecoder vorbis, DataPacket packet, int number)
        {
            var temp = new VorbisCodebook();
            temp.BookNum = number;
            temp.Init(packet);
            return temp;
        }

        private VorbisCodebook()
        {

        }

        internal void Init(DataPacket packet)
        {
            // first, check the sync pattern
            var chkVal = packet.ReadBits(24);
            if (chkVal != 0x564342UL) throw new InvalidDataException();

            // get the counts
            Dimensions = (int)packet.ReadBits(16);
            Entries = (int)packet.ReadBits(24);
            
            // init the storage
            Lengths = new int[Entries];

            InitTree(packet);
            InitLookupTable(packet);
        }

        void InitTree(DataPacket packet)
        {
            bool sparse;
            int total = 0;

            if (packet.ReadBit())
            {
                // ordered
                var len = (int)packet.ReadBits(5) + 1;
                for (var i = 0; i < Entries; )
                {
                    var cnt = (int)packet.ReadBits(Utils.ilog(Entries - i));

                    while (--cnt >= 0)
                    {
                        Lengths[i++] = len;
                    }

                    ++len;
                }
                total = 0;
                sparse = false;
            }
            else
            {
                // unordered
                sparse = packet.ReadBit();
                for (var i = 0; i < Entries; i++)
                {
                    if (!sparse || packet.ReadBit())
                    {
                        Lengths[i] = (int)packet.ReadBits(5) + 1;
                        ++total;
                    }
                    else
                    {
                        // mark the entry as unused
                        Lengths[i] = -1;
                    }
                }
            }
            // figure out the maximum bit size; if all are unused, don't do anything else
            if ((MaxBits = Lengths.Max()) > -1)
            {
                int sortedCount = 0;
                int[] codewordLengths = null;
                if (sparse && total >= Entries >> 2)
                {
                    codewordLengths = new int[Entries];
                    Array.Copy(Lengths, codewordLengths, Entries);

                    sparse = false;
                }

                // compute size of sorted tables
                if (sparse)
                {
                    sortedCount = total;
                }
                else
                {
                    sortedCount = 0;
                }

                int sortedEntries = sortedCount;

                int[] values = null;
                int[] codewords = null;
                if (!sparse)
                {
                    codewords = new int[Entries];
                }
                else if (sortedEntries != 0)
                {
                    codewordLengths = new int[sortedEntries];
                    codewords = new int[sortedEntries];
                    values = new int[sortedEntries];
                }

                if (!ComputeCodewords(sparse, sortedEntries, codewords, codewordLengths, len: Lengths, n: Entries, values: values)) throw new InvalidDataException();

                PrefixList = Huffman.BuildPrefixedLinkedList(values ?? Enumerable.Range(0, codewords.Length).ToArray(), codewordLengths ?? Lengths, codewords, out PrefixBitLength, out PrefixOverflowTree);
            }
        }

        bool ComputeCodewords(bool sparse, int sortedEntries, int[] codewords, int[] codewordLengths, int[] len, int n, int[] values)
        {
            int i, k, m = 0;
            uint[] available = new uint[32];

            for (k = 0; k < n; ++k) if (len[k] > 0) break;
            if (k == n) return true;

            AddEntry(sparse, codewords, codewordLengths, 0, k, m++, len[k], values);

            for (i = 1; i <= len[k]; ++i) available[i] = 1U << (32 - i);

            for (i = k + 1; i < n; ++i)
            {
                uint res;
                int z = len[i], y;
                if (z <= 0) continue;

                while (z > 0 && available[z] == 0) --z;
                if (z == 0) return false;
                res = available[z];
                available[z] = 0;
                AddEntry(sparse, codewords, codewordLengths, Utils.BitReverse(res), i, m++, len[i], values);

                if (z != len[i])
                {
                    for (y = len[i]; y > z; --y)
                    {
                        available[y] = res + (1U << (32 - y));
                    }
                }
            }

            return true;
        }

        void AddEntry(bool sparse, int[] codewords, int[] codewordLengths, uint huffCode, int symbol, int count, int len, int[] values)
        {
            if (sparse)
            {
                codewords[count] = (int)huffCode;
                codewordLengths[count] = len;
                values[count] = symbol;
            }
            else
            {
                codewords[symbol] = (int)huffCode;
            }
        }

        void InitLookupTable(DataPacket packet)
        {
            MapType = (int)packet.ReadBits(4);
            if (MapType == 0) return;

            var minValue = Utils.ConvertFromVorbisFloat32(packet.ReadUInt32());
            var deltaValue = Utils.ConvertFromVorbisFloat32(packet.ReadUInt32());
            var valueBits = (int)packet.ReadBits(4) + 1;
            var sequence_p = packet.ReadBit();

            var lookupValueCount = Entries * Dimensions;
            var lookupTable = new float[lookupValueCount];
            if (MapType == 1)
            {
                lookupValueCount = lookup1_values();
            }

            var multiplicands = new uint[lookupValueCount];
            for (var i = 0; i < lookupValueCount; i++)
            {
                multiplicands[i] = (uint)packet.ReadBits(valueBits);
            }

            // now that we have the initial data read in, calculate the entry tree
            if (MapType == 1)
            {
                for (var idx = 0; idx < Entries; idx++)
                {
                    var last = 0.0;
                    var idxDiv = 1;
                    for (var i = 0; i < Dimensions; i++)
                    {
                        var moff = (idx / idxDiv) % lookupValueCount;
                        var value = (float)multiplicands[moff] * deltaValue + minValue + last;
                        lookupTable[idx * Dimensions + i] = (float)value;

                        if (sequence_p) last = value;

                        idxDiv *= lookupValueCount;
                    }
                }
            }
            else
            {
                for (var idx = 0; idx < Entries; idx++)
                {
                    var last = 0.0;
                    var moff = idx * Dimensions;
                    for (var i = 0; i < Dimensions; i++)
                    {
                        var value = multiplicands[moff] * deltaValue + minValue + last;
                        lookupTable[idx * Dimensions + i] = (float)value;

                        if (sequence_p) last = value;

                        ++moff;
                    }
                }
            }

            LookupTable = lookupTable;
        }

        int lookup1_values()
        {
            var r = (int)Math.Floor(Math.Exp(Math.Log(Entries) / Dimensions));
            
            if (Math.Floor(Math.Pow(r + 1, Dimensions)) <= Entries) ++r;
            
            return r;
        }

        internal int BookNum;

        internal int Dimensions;

        internal int Entries;

        int[] Lengths;

        float[] LookupTable;

        internal int MapType;

        HuffmanListNode PrefixOverflowTree;
        System.Collections.Generic.List<HuffmanListNode> PrefixList;
        int PrefixBitLength;
        int MaxBits;


        internal float this[int entry, int dim]
        {
            get
            {
                return LookupTable[entry * Dimensions + dim];
            }
        }

        internal int DecodeScalar(DataPacket packet)
        {
            int bitCnt;
            var bits = (int)packet.TryPeekBits(PrefixBitLength, out bitCnt);
            if (bitCnt == 0) return -1;

            // try to get the value from the prefix list...
            var node = PrefixList[bits];
            if (node != null)
            {
                packet.SkipBits(node.Length);
                return node.Value;
            }

            // nope, not possible... run the tree
            bits = (int)packet.TryPeekBits(MaxBits, out bitCnt);

            node = PrefixOverflowTree;
            do
            {
                if (node.Bits == (bits & node.Mask))
                {
                    packet.SkipBits(node.Length);
                    return node.Value;
                }
            } while ((node = node.Next) != null);
            return -1;
        }
    }
}
