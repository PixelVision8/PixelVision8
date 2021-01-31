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

using System.Collections.Generic;

namespace PixelVision8.Player
{
    
    #region Font Chip Class
    
    /// <summary>
    ///     The font chip allows you to render text to the display. It is built on
    ///     top of the same APIs as the <see cref="SpriteChip" /> but has custom
    ///     methods for converting text into sprites.
    /// </summary>
    public class FontChip : SpriteChip
    {
        public readonly Dictionary<string, int[]> Fonts = new Dictionary<string, int[]>();

        /// <summary>
        ///     This method configures the FontChip. It registers itself with the
        ///     engine, sets the default width and height to 8 and resizes the
        ///     <see cref="TextureData" /> to 96 x 64.
        /// </summary>
        protected override void Configure()
        {
            Player.FontChip = this;

            Pages = 2;
            ColorsPerSprite = 2;
            Unique = true;
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
            if (Fonts.ContainsKey(name))
                Fonts[name] = fontMap;
            else
                Fonts.Add(name, fontMap);
        }

        public int[] ReadFont(string name)
        {
            if (Fonts.ContainsKey(name)) return Fonts[name];

            return null;
        }

        // TODO don't forget to add 'typeof(FontChip).FullName' to the Chip list in the GameRunner.Activate.cs class
    }
    
    #endregion

    #region Modify PixelVision

    public partial class PixelVision
    {
        public FontChip FontChip { get; set; }
    }
    
    #endregion
}
