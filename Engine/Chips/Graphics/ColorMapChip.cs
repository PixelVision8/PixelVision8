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

namespace PixelVisionSDK.Chips
{

    /// <summary>
    ///     The color map chip is used to help import sprites and tile maps into the
    ///     engine if their colors don't match the system colors. When loading
    ///     artwork, if a color map is present, it will use the order of the colors
    ///     in the map as the index to remap to the system colors in the ColorChip.
    /// </summary>
    public class ColorMapChip : ColorChip
    {

        /// <summary>
        ///     This method registers the color map with the engine. It also marks
        ///     export to false so it is not serialized with the
        ///     other chips in the ChipManager.
        /// </summary>
        public override void Configure()
        {
            engine.colorMapChip = this;
        }

    }

}