//
// Copyright (c) Jesse Freeman. All rights reserved.  
//
// Licensed under the Microsoft Public License (MS-PL) License. 
// See LICENSE file in the project root for full license information. 
//
// Contributors
// --------------------------------------------------------
// This is the official list of Pixel Vision 8 contributors:
//
// Jesse Freeman - @JesseFreeman
// Christer Kaitila - @McFunkypants
// Pedro Medeiros - @saint11
// Shawn Rakowski - @shwany

using System;
using Microsoft.Xna.Framework;
using PixelVision8.Runner.Importers;
using PixelVision8.Runner.Parsers;

namespace PixelVision8.Runner.Data
{
    
    public class SpriteExportData
    {
        public string fileName;
        public int[] ids;
        public int width;
        public int height;
//        public TextureData src;
        public IImageParser imageParser;

        public byte[] bytes;
        
        // TODO this is hardcoded
        private Point spriteSize = new Point(8, 8);
        
        public SpriteExportData(string fileName, byte[] bytes)
        {
            this.fileName = fileName;
            this.bytes = bytes;
            
            // TODO This is hard coded and should be injected in
            imageParser = new PNGReader(this.bytes);
            
            width = (int)Math.Ceiling((float) imageParser.width / spriteSize.X);
            height = (int)Math.Ceiling((float) imageParser.height / spriteSize.Y);
            
            var totalIDs = width * height;
                
            // Setup sprite id containers based on size
            ids = new int[totalIDs];
                
            // Clear all sprite IDs
            for (int i = 0; i < totalIDs; i++)
            {
                ids[i] = -1;
            }
        }
    }
}