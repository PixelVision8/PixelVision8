//   
// Copyright (c) Jesse Freeman, Pixel Vision 8. All rights reserved.  
//  
// Licensed under the Microsoft Public License (MS-PL) except for a few
// portions of the code. See LICENSE file in the project root for full 
// license information. Third-party libraries used by Pixel Vision 8 are 
// under their own licenses. Please refer to those libraries for details 
// on the license they use.
//
// Based on SimpleGif (https://github.com/hippogamesunity/simplegif) by
// Nate River of Hippo Games
// 
// Contributors
// --------------------------------------------------------
// This is the official list of Pixel Vision 8 contributors:
//  
// Jesse Freeman - @JesseFreeman
// Christina-Antoinette Neofotistou @CastPixel
// Christer Kaitila - @McFunkypants
// Pedro Medeiros - @saint11
// Shawn Rakowski - @shwany
//

using System;
using System.Collections.Generic;

namespace PixelVision8.Runner.Gif
{
    internal static class LzwEncoder
    {
        public static byte GetMinCodeSize(byte max)
        {
            byte minCodeSize = 2;

            while (1 << minCodeSize <= max)
            {
                minCodeSize++;
            }

            return minCodeSize;
        }

        public static byte[] Encode(byte[] colorIndexes, int minCodeSize)
        {
            var dict = InitializeDictionary(minCodeSize);
            var clearCode = 1 << minCodeSize;
            var endOfInformation = clearCode + 1;
            var code = new[] {colorIndexes[0]};
            var codeSize = minCodeSize + 1;
            var bits = new List<bool>();

            ReadBits(clearCode, codeSize, ref bits);

            for (var i = 1; i < colorIndexes.Length; i++)
            {
                var next = new byte[code.Length + 1];

                Array.Copy(code, next, code.Length);
                next[next.Length - 1] = colorIndexes[i];

                if (dict.ContainsKey(next))
                {
                    code = next;
                }
                else
                {
                    ReadBits(dict[code], codeSize, ref bits);
                    code = new[] {colorIndexes[i]};

                    if (dict.Count + 2 < 4096) // + CC + EoF
                    {
                        dict.Add(next, dict.Count + 2);

                        if (dict.Count + 2 - 1 == 1 << codeSize)
                        {
                            codeSize++;
                        }
                    }
                }
            }

            ReadBits(dict[code], codeSize, ref bits);
            ReadBits(endOfInformation, codeSize, ref bits);

            var bytes = GetBytes(bits);

            return bytes;
        }

        private static Dictionary<byte[], int> InitializeDictionary(int minCodeSize)
        {
            var dict = new Dictionary<byte[], int>(new ByteArrayComparer());

            for (var i = 0; i < 1 << minCodeSize; i++)
            {
                dict.Add(new[] {(byte) i}, i);
            }

            return dict;
        }

        private static void ReadBits(int key, int codeSize, ref List<bool> destination)
        {
            for (var j = 0; j < codeSize; j++)
            {
                destination.Add(GetBit(key, j));
            }
        }

        private static bool GetBit(int value, int index)
        {
            return (value & (1 << index)) != 0;
        }

        private static byte[] GetBytes(IList<bool> bits)
        {
            var size = bits.Count >> 3;

            if ((bits.Count & 0x07) != 0) ++size;

            var bytes = new byte[size];

            for (var i = 0; i < bits.Count; i++)
            {
                if (bits[i])
                {
                    bytes[i >> 3] |= (byte) (1 << (i & 0x07));
                }
            }

            return bytes;
        }
    }
}