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

namespace PixelVisionRunner
{

    [Flags]
    public enum SaveFlags
    {
        None = 0,
        System = 1,
        Code = 2,
        Colors = 4,
        Sprites = 16,
        Tilemap = 32,
        Fonts = 128,
        Meta = 256,
        Music = 512,
        Sounds = 1024,
        SaveData = 4096,
        
        // TODO deprecated, need to remove these
        TilemapCache = 2048,
        TileColorOffset = 8192,
        TilemapFlags = 64,
        ColorMap = 8,
        FlagColors = 16384,


    }

}