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
        Code = 2,
        Colors = 4,
        ColorMap = 8,
        Sprites = 16,
        Tilemap = 32,
        Fonts = 64,
        Meta = 128,
        Music = 256,
        Sounds = 512,
        SaveData = 1024,

        // TODO deprecated, need to remove these
        TilemapFlags = 2048,
        TilemapCache = 4096,
        TileColorOffset = 8192,
        FlagColors = 16384
    }
}