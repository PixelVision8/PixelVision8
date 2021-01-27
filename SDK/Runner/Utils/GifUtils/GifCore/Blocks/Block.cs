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

using System.Collections.Generic;
using System.Linq;

namespace PixelVision8.Runner.Gif
{
    internal abstract class Block
    {
        public const byte ExtensionIntroducer = 0x21;
        public const byte PlainTextExtensionLabel = 0x1;
        public const byte GraphicControlExtensionLabel = 0xF9;
        public const byte CommentExtensionLabel = 0xFE;
        public const byte ImageDescriptorLabel = 0x2C;
        public const byte ApplicationExtensionLabel = 0xFF;
        public const byte BlockTerminatorLabel = 0x00;

        protected byte[] ReadDataSubBlocks(byte[] bytes, ref int index)
        {
            var data = new List<byte>();

            while (bytes[index] > 0) // Sub-block size
            {
                var subBlock = BitHelper.ReadBytes(bytes, bytes[index++], ref index);

                if (data.Count == 0)
                {
                    data = subBlock.ToList();
                }
                else
                {
                    data.AddRange(subBlock);
                }
            }

            return data.ToArray();
        }
    }
}