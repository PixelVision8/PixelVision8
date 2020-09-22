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

using System;

namespace PixelVision8.Runner
{
    [Flags]
    public enum SaveFlags
    {
        None = 0,
        System = 1,
        Colors = 2,
        ColorMap = 4,
        Sprites = 8,
        Tilemap = 16,
        Fonts = 32,
        Meta = 64,
        Music = 128,
        Sounds = 256,
        SaveData = 512,
        MetaSprites = 1024,
        // TilemapFlags = 2048
        // TilemapCache = 4096,
        // TileColorOffset = 8192,
        // FlagColors = 16384
    }
}