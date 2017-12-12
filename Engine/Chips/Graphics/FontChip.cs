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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PixelVisionSDK.Chips
{

    /// <summary>
    ///     The font chip allows you to render text to the display. It is built on
    ///     top of the same APIs as the <see cref="SpriteChip" /> but has custom
    ///     methods for converting text into sprites.
    /// </summary>
    public class FontChip : AbstractChip
    {

        protected static int charOffset = 32;

        protected Dictionary<string, int[]> fonts = new Dictionary<string, int[]>();

        public int[] tmpPixels = new int[0];

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
        
        internal int[] ConvertTextToSprites(string text, string fontName = "Default")
        {
            var total = text.Length;

            var spriteIDs = new int[total];

            char character;

            int spriteID, index;

            // Test to make sure font exists
            if (!fonts.ContainsKey(fontName))
                return new int[0];

            var fontMap = fonts[fontName];
            var totalCharacters = fontMap.Length;

            for (var i = 0; i < total; i++)
            {
                character = text[i];
                index = Convert.ToInt32(character) - charOffset;
                spriteID = -1;

                if (index < totalCharacters && index > -1)
                    spriteID = fontMap[index];

                spriteIDs[i] = spriteID;
            }

            return spriteIDs;
        }

        public int[] ConvertCharacterToPixelData(char character, string fontName)
        {
            var spriteChip = engine.spriteChip;

            // Test to make sure font exists
            if (!fonts.ContainsKey(fontName))
                throw new Exception("Font '" + fontName + "' not found.");

            var index = Convert.ToInt32(character) - charOffset;

            var fontMap = fonts[fontName];
            var totalCharacters = fontMap.Length;
            var spriteID = -1;

            if (index < totalCharacters && index > -1)
                spriteID = fontMap[index];

            if (spriteID > -1)
            {
                var totalPixels = spriteChip.width * spriteChip.height;

                if (tmpPixels.Length != totalPixels)
                    Array.Resize(ref tmpPixels, totalPixels);

                spriteChip.ReadSpriteAt(spriteID, tmpPixels);

                return tmpPixels;
            }

            return null;
        }
   }

}