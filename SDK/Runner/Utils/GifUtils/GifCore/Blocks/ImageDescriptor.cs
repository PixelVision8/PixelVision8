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
    internal class ImageDescriptor : Block
    {
        public ushort ImageLeftPosition;
        public ushort ImageTopPosition;
        public ushort ImageWidth;
        public ushort ImageHeight;
        public byte LocalColorTableFlag;
        public byte InterlaceFlag;
        public byte SortFlag;
        public byte Reserved;
        public byte LocalColorTableSize;

        public ImageDescriptor(byte[] bytes, ref int index)
        {
            if (bytes[index++] != ImageDescriptorLabel) throw new Exception("Expected: " + ImageDescriptorLabel);

            ImageLeftPosition = BitHelper.ReadInt16(bytes, ref index);
            ImageTopPosition = BitHelper.ReadInt16(bytes, ref index);
            ImageWidth = BitHelper.ReadInt16(bytes, ref index);
            ImageHeight = BitHelper.ReadInt16(bytes, ref index);

            LocalColorTableFlag = BitHelper.ReadPackedByte(bytes[index], 0, 1);
            InterlaceFlag = BitHelper.ReadPackedByte(bytes[index], 1, 1);
            SortFlag = BitHelper.ReadPackedByte(bytes[index], 2, 1);
            Reserved = BitHelper.ReadPackedByte(bytes[index], 3, 2);
            LocalColorTableSize = BitHelper.ReadPackedByte(bytes[index++], 5, 3);
        }

        public ImageDescriptor(ushort imageLeftPosition, ushort imageTopPosition, ushort imageWidth, ushort imageHeight,
            byte localColorTableFlag, byte interlaceFlag, byte sortFlag, byte reserved, byte localColorTableSize)
        {
            ImageLeftPosition = imageLeftPosition;
            ImageTopPosition = imageTopPosition;
            ImageWidth = imageWidth;
            ImageHeight = imageHeight;
            LocalColorTableFlag = localColorTableFlag;
            InterlaceFlag = interlaceFlag;
            SortFlag = sortFlag;
            Reserved = reserved;
            LocalColorTableSize = localColorTableSize;
        }

        public List<byte> GetBytes()
        {
            var bytes = new List<byte> {ImageDescriptorLabel};

            bytes.AddRange(BitConverter.GetBytes(ImageLeftPosition));
            bytes.AddRange(BitConverter.GetBytes(ImageTopPosition));
            bytes.AddRange(BitConverter.GetBytes(ImageWidth));
            bytes.AddRange(BitConverter.GetBytes(ImageHeight));

            var packedByte = BitHelper.PackByte(
                LocalColorTableFlag == 1,
                InterlaceFlag == 1,
                SortFlag == 1,
                BitHelper.ReadByte(Reserved, 1),
                BitHelper.ReadByte(Reserved, 0),
                BitHelper.ReadByte(LocalColorTableSize, 2),
                BitHelper.ReadByte(LocalColorTableSize, 1),
                BitHelper.ReadByte(LocalColorTableSize, 0));

            bytes.Add(packedByte);

            return bytes;
        }
    }
}