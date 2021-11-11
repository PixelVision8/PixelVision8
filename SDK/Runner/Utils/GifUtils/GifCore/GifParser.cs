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
using System.Text;

namespace PixelVision8.Runner.Gif
{
    /// <summary>
    /// Gif specs: https://www.w3.org/Graphics/GIF/spec-gif89a.txt
    /// </summary>
    internal class GifParser
    {
        public string Header;
        public LogicalScreenDescriptor LogicalScreenDescriptor;
        public ColorTable GlobalColorTable;
        public List<Block> Blocks;

        public GifParser(byte[] bytes)
        {
            var index = 6;

            Header = Encoding.UTF8.GetString(bytes, 0, 6);
            LogicalScreenDescriptor = new LogicalScreenDescriptor(bytes, ref index);

            if (LogicalScreenDescriptor.GlobalColorTableFlag == 1)
            {
                GlobalColorTable = new ColorTable(LogicalScreenDescriptor.GlobalColorTableSize, bytes, ref index);
            }

            Blocks = ReadBlocks(bytes, ref index);
        }

        private static List<Block> ReadBlocks(byte[] bytes, ref int startIndex)
        {
            var blocks = new List<Block>();
            var index = startIndex;

            while (true)
            {
                switch (bytes[index])
                {
                    case Block.ExtensionIntroducer:
                    {
                        Block extension;

                        switch (bytes[index + 1])
                        {
                            case Block.PlainTextExtensionLabel:
                                extension = new PlainTextExtension(bytes, ref index);
                                break;
                            case Block.GraphicControlExtensionLabel:
                                extension = new GraphicControlExtension(bytes, ref index);
                                break;
                            case Block.CommentExtensionLabel:
                                extension = new CommentExtension(bytes, ref index);
                                break;
                            case Block.ApplicationExtensionLabel:
                                extension = new ApplicationExtension(bytes, ref index);
                                break;
                            default:
                                throw new NotSupportedException("Unknown extension!");
                        }

                        blocks.Add(extension);
                        break;
                    }
                    case Block.ImageDescriptorLabel:
                    {
                        var descriptor = new ImageDescriptor(bytes, ref index);

                        blocks.Add(descriptor);

                        if (descriptor.LocalColorTableFlag == 1)
                        {
                            var localColorTable = new ColorTable(descriptor.LocalColorTableSize, bytes, ref index);

                            blocks.Add(localColorTable);
                        }

                        var data = new TableBasedImageData(bytes, ref index);

                        blocks.Add(data);

                        break;
                    }
                    case 0x3B: // End
                    {
                        return blocks;
                    }
                    default:
                        throw new NotSupportedException($"Unsupported GIF block: {bytes[index]:X}.");
                }
            }
        }
    }
}