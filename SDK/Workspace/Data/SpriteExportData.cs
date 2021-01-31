//   
// Copyright (c) Jesse Freeman, Pixel Vision 8. All rights reserved.  
//  
// Licensed under the Microsoft Public License (MS-PL) except for a few
// portions of the code. See LICENSE file in the project root for full 
// license information. Third-party libraries used by Pixel Vision 8 are 
// under their own licenses. Please refer to those libraries for details 
// on the license they use.
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

using Microsoft.Xna.Framework;
using PixelVision8.Runner;
using System;

namespace PixelVision8.Runner
{
    public class SpriteExportData
    {
        // TODO this is hardcoded
        private readonly Point spriteSize = new Point(8, 8);
        public byte[] bytes;
        public string fileName;
        public int height;

        public int[] ids;

        //        public TextureData src;
        public IImageParser imageParser;
        public int width;

        public SpriteExportData(string fileName, byte[] bytes)
        {
            this.fileName = fileName;
            this.bytes = bytes;

            // TODO This is hard coded and should be injected in
            imageParser = new PNGReader(this.bytes);

            width = (int) Math.Ceiling((float) imageParser.Width / spriteSize.X);
            height = (int) Math.Ceiling((float) imageParser.Height / spriteSize.Y);

            var totalIDs = width * height;

            // Setup sprite id containers based on size
            ids = new int[totalIDs];

            // Clear all sprite IDs
            for (var i = 0; i < totalIDs; i++) ids[i] = -1;
        }
    }
}