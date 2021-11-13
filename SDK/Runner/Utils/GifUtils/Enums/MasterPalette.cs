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

namespace PixelVision8.Runner.Gif
{
    /// <summary>
    /// These are selections of colors based in evenly ordered RGB levels which provide complete RGB combinations,
    /// mainly used as master palettes to display any kind of image within the limitations of the 8-bit pixel depth.
    /// More info: https://en.wikipedia.org/wiki/List_of_software_palettes#RGB_arrangements
    /// </summary>
    public enum MasterPalette
    {
        DontApply,
        Levels666,
        Levels676,
        Levels685,
        Levels884,
        Grayscale
    }
}