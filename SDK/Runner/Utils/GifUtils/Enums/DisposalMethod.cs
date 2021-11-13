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
    /// Indicates the way in which the graphic is to be treated after being displayed.
    /// More info: https://www.w3.org/Graphics/GIF/spec-gif89a.txt
    /// </summary>
    public enum DisposalMethod
    {
        /// <summary>
        /// The decoder is not required to take any action.
        /// </summary>
        NoDisposalSpecified = 0,

        /// <summary>
        /// The graphic is to be left in place.
        /// </summary>
        DoNotDispose = 1,

        /// <summary>
        /// The area used by the graphic must be restored to the background color.
        /// </summary>
        RestoreToBackgroundColor = 2,

        /// <summary>
        /// The decoder is required to restore the area overwritten by the graphic with what was there prior to rendering the graphic.
        /// </summary>
        RestoreToPrevious = 3
    }
}