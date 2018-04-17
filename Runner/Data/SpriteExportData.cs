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

namespace PixelVisionRunner
{
    
    public class SpriteExportData
    {
        public string fileName;
        public int[] ids;
        public int width;
        public int height;
        public ITexture2D src;

        public SpriteExportData(string fileName)
        {
            this.fileName = fileName;
        }
    }
}