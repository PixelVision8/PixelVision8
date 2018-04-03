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

using PixelVisionSDK.Chips;

namespace PixelVisionRunner.Parsers
{

    public class FontParser : SpriteParser
    {

        private readonly bool autoImport;

        private readonly FontChip fontChip;
        private int[] fontMap;
        private readonly string name;

        public FontParser(ITexture2D tex, IEngineChips chips, string name = "Default", bool autoImport = true) : base(tex, chips)
        {
            fontChip = chips.fontChip;
            if (fontChip == null)
            {
                // Create a new font chip to store data
                fontChip = new FontChip();
                chips.chipManager.ActivateChip(fontChip.GetType().FullName, fontChip);
            }
            this.autoImport = autoImport;
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

        public override bool IsEmpty(IColor[] pixels)
        {
            // Hack to make sure if the space is empty we still save it
            if (index == 0)
                return false;

            return base.IsEmpty(pixels);
        }

        protected override void ProcessSpriteData()
        {
            var id = spriteChip.FindSprite(spriteData);

            if (id == -1 && autoImport)
            {
                id = spriteChip.NextEmptyID();
                spriteChip.UpdateSpriteAt(id, spriteData);
            }

            fontMap[index] = id;
        }

    }

}