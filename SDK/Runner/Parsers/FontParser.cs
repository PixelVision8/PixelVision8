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

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using PixelVision8.Engine.Chips;
using PixelVision8.Engine.Utils;

namespace PixelVision8.Runner.Parsers
{
    public class FontParser : SpriteImageParser
    {
        private readonly FontChip fontChip;
        private readonly string name;
        private List<string> uniqueFontColors;
        private int[] fontMap;

        public FontParser(IImageParser parser, ColorChip colorChip, FontChip fontChip) : base(parser,
            colorChip, fontChip)
        {
            this.fontChip = fontChip;
            // imageParser.ReadStream();
            name = parser.FileName.Split('.').First();
        }

        public override void CreateImage()
        {

            // Get all the colors from the image
            uniqueFontColors = Parser.colorPalette.Select(c => ColorUtils.RgbToHex(c.R, c.G, c.B)).ToList();

            // Remove the mask color
            uniqueFontColors.Remove(colorChip.maskColor);

            // Convert into an array
            var colorRefs = uniqueFontColors.ToArray();

            // Convert all of the pixels into color ids
            var pixelIDs = Parser.colorPixels.Select(c => Array.IndexOf(colorRefs, ColorUtils.RgbToHex(c.R, c.G, c.B))).ToArray();

            // Create new image
            image = new Image(Parser.width, Parser.height, colorRefs, pixelIDs, new Point(spriteWidth, spriteHeight));

            StepCompleted();

        }

        public override void PrepareSprites()
        {
            base.PrepareSprites();

            fontMap = new int[totalSprites];
            //            base.PreCutOutSprites();
        }

        protected override void PostCutOutSprites()
        {
            fontChip.AddFont(name, fontMap);
            base.PostCutOutSprites();
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