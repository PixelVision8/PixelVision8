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

namespace PixelVision8.Runner.Gif
{
    internal class ApplicationExtension : Block
    {
        public byte BlockSize;
        public byte[] ApplicationIdentifier;
        public byte[] ApplicationAuthenticationCode;
        public byte[] ApplicationData;

        public ApplicationExtension(byte[] bytes, ref int index)
        {
            if (bytes[index++] != ExtensionIntroducer) throw new Exception("Expected: " + ExtensionIntroducer);
            if (bytes[index++] != ApplicationExtensionLabel)
                throw new Exception("Expected: " + ApplicationExtensionLabel);

            BlockSize = bytes[index++];
            ApplicationIdentifier = BitHelper.ReadBytes(bytes, 8, ref index);
            ApplicationAuthenticationCode = BitHelper.ReadBytes(bytes, 3, ref index);
            ApplicationData = ReadDataSubBlocks(bytes, ref index);

            if (bytes[index++] != BlockTerminatorLabel) throw new Exception("Expected: " + BlockTerminatorLabel);
        }

        public ApplicationExtension()
        {
        }

        public byte[] GetBytes()
        {
            return new byte[]
            {
                0x21, 0xFF, 0x0B, 0x4E, 0x45, 0x54, 0x53, 0x43, 0x41, 0x50, 0x45, 0x32, 0x2E, 0x30, 0x03, 0x01, 0x00,
                0x00, 0x00
            };
        }
    }
}