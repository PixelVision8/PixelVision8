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

using System.Collections.Generic;

namespace PixelVision8.Engine.Chips
{
    /// <summary>
    ///     The font chip allows you to render text to the display. It is built on
    ///     top of the same APIs as the <see cref="SpriteChip" /> but has custom
    ///     methods for converting text into sprites.
    /// </summary>
    public class FontChip : AbstractChip
    {
        

        public Dictionary<string, int[]> fonts = new Dictionary<string, int[]>();
    
//        public int[] tmpPixels = new int[0];

        /// <summary>
        ///     This method configures the FontChip. It registers itself with the
        ///     engine, sets the default width and height to 8 and resizes the
        ///     <see cref="TextureData" /> to 96 x 64.
        /// </summary>
        public override void Configure()
        {
            engine.fontChip = this;
        }

        public override void Deactivate()
        {
            base.Deactivate();
            engine.fontChip = null;
        }

        /// <summary>
        ///     Adds a font to the FontChip. Each font requires a name and an array
        ///     of IDs for the sprites to be used. Each sprite should refer to their
        ///     character's ASCII code minus 32 since the font map starts at the empty
        ///     space character
        /// </summary>
        /// <param name="name"></param>
        /// <param name="fontMap"></param>
        public void AddFont(string name, int[] fontMap)
        {
            if (fonts.ContainsKey(name))
                fonts[name] = fontMap;
            else
                fonts.Add(name, fontMap);
        }

        public int[] ReadFont(string name)
        {
            if (fonts.ContainsKey(name))
            {
                return fonts[name];
            }
            
            return null;
        }
        
    }
}