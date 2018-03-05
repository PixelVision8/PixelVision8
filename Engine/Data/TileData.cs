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

namespace PixelVisionSDK
{
    public class TileData : AbstractData
    {
        public int spriteID;
        public int colorOffset;
        public int flag;

        public TileData(int spriteID, int colorOffset = 0, int flag = -1)
        {
            this.spriteID = spriteID;
            this.colorOffset = colorOffset;
            this.flag = flag;
        }
    }
}