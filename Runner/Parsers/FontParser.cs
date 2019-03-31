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

using Microsoft.Xna.Framework;
using PixelVision8.Engine.Chips;

namespace PixelVision8.Runner.Parsers
{
    public class FontParser : SpriteParser
    {
//        private readonly bool autoImport;

        private readonly FontChip fontChip;
        private readonly string name;
        private int[] fontMap;

        public FontParser(IImageParser imageParser, IEngineChips chips, string name = "Default") : base(imageParser,
            chips)
        {
            fontChip = chips.fontChip;
            if (fontChip == null)
            {
                // Create a new font chip to store data
                fontChip = new FontChip();
                chips.ActivateChip(fontChip.GetType().FullName, fontChip);
            }

//            this.autoImport = autoImport;
            this.name = name;
        }

        protected override void CalculateBounds()
        {
            base.CalculateBounds();

            fontMap = new int[totalSprites];
//            base.PreCutOutSprites();
        }

        protected override void PostCutOutSprites()
        {
            fontChip.AddFont(name, fontMap);
            base.PostCutOutSprites();
        }

        public override bool IsEmpty(Color[] pixels)
        {
            // Hack to make sure if the space is empty we still save it
            if (index == 0)
                return false;

            return base.IsEmpty(pixels);
        }

        protected override void ProcessSpriteData()
        {
            var id = -1;

            // If the sprite chip has unique sprites, try to find an existing sprite first
            if (spriteChip.unique) id = spriteChip.FindSprite(spriteData);

            // If the sprite ID is -1 look for an empty sprite
            if (id == -1) id = spriteChip.NextEmptyID();

            // Add the font character sprite data
            spriteChip.UpdateSpriteAt(id, spriteData);

            // Set the id to the font map
            fontMap[index] = id;
        }
    }
}