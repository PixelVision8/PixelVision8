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

namespace PixelVision8.Engine.Chips
{
    public interface IDisplay
    {
        /// <summary>
        ///     The global <see cref="width" /> of sprites in the engine. By default
        ///     this is set to 8.
        /// </summary>
        // int visibleWidth { get;}

        /// <summary>
        ///     The global <see cref="width" /> of sprites in the engine. By default
        ///     this is set to 8.
        /// </summary>
        // int visibleHeight { get;}

        /// <summary>
        ///     A public getter for the internal
        ///     TextureData. When requested, a clone of the <see cref="_texture" />
        ///     field is returned. This is expensive and only used for tools.
        /// </summary>
        PixelData PixelData
        {
            get;
        }

        /// <summary>
        ///     Return's the <see cref="Sprite" /> Ram's internal
        ///     <see cref="texture" /> <see cref="width" />
        /// </summary>
        // int textureWidth => PixelData.Width;

        /// <summary>
        ///     Return's the <see cref="Sprite" /> Ram's internal
        ///     <see cref="texture" /> <see cref="width" />
        /// </summary>
        // int textureHeight => PixelData.Height;
    }
}