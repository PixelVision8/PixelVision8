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
    internal class LogicalScreenDescriptor
    {
        public ushort LogicalScreenWidth;
        public ushort LogicalScreenHeight;
        public byte GlobalColorTableFlag;
        public byte ColorResolution;
        public byte SortFlag;
        public byte GlobalColorTableSize;
        public byte BackgroundColorIndex;
        public byte PixelAspecRatio;

        public LogicalScreenDescriptor(byte[] bytes, ref int index)
        {
            LogicalScreenWidth = BitHelper.ReadInt16(bytes, ref index);
            LogicalScreenHeight = BitHelper.ReadInt16(bytes, ref index);

            GlobalColorTableFlag = BitHelper.ReadPackedByte(bytes[index], 0, 1);
            ColorResolution = BitHelper.ReadPackedByte(bytes[index], 1, 3);
            SortFlag = BitHelper.ReadPackedByte(bytes[index], 4, 1);
            GlobalColorTableSize = BitHelper.ReadPackedByte(bytes[index++], 5, 3);

            BackgroundColorIndex = bytes[index++];
            PixelAspecRatio = bytes[index++];
        }

        public LogicalScreenDescriptor(ushort logicalScreenWidth, ushort logicalScreenHeight,
            byte globalColorTableFlag, byte colorResolution, byte sortFlag, byte gobalColorTableSize,
            byte backgroundColorIndex, byte pixelAspecRatio)
        {
            LogicalScreenWidth = logicalScreenWidth;
            LogicalScreenHeight = logicalScreenHeight;
            GlobalColorTableFlag = globalColorTableFlag;
            ColorResolution = colorResolution;
            SortFlag = sortFlag;
            GlobalColorTableSize = gobalColorTableSize;
            BackgroundColorIndex = backgroundColorIndex;
            PixelAspecRatio = pixelAspecRatio;
        }

        public List<byte> GetBytes()
        {
            var bytes = new List<byte>();

            bytes.AddRange(BitConverter.GetBytes(LogicalScreenWidth));
            bytes.AddRange(BitConverter.GetBytes(LogicalScreenHeight));

            var packedByte = BitHelper.PackByte(
                GlobalColorTableFlag == 1,
                BitHelper.ReadByte(ColorResolution, 2),
                BitHelper.ReadByte(ColorResolution, 1),
                BitHelper.ReadByte(ColorResolution, 0),
                SortFlag == 1,
                BitHelper.ReadByte(GlobalColorTableSize, 2),
                BitHelper.ReadByte(GlobalColorTableSize, 1),
                BitHelper.ReadByte(GlobalColorTableSize, 0));

            bytes.Add(packedByte);
            bytes.Add(BackgroundColorIndex);
            bytes.Add(PixelAspecRatio);

            return bytes;
        }
    }
}