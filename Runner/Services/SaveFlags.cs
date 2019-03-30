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
        FlagColors = 16384,


    }

}