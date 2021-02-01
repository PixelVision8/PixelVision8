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
using PixelVision8.Player;

namespace PixelVision8.Runner
{
    public class FontParser : SpriteImageParser
    {
        
        private readonly FontChip _fontChip;
        private List<string> _uniqueFontColors;
        private int[] _fontMap;

        public FontParser(string sourceFile, IImageParser parser, ColorChip colorChip, FontChip fontChip) : base(
            sourceFile, parser, colorChip, fontChip)
        {
            _fontChip = fontChip;
        }

        public override void CreateImage()
        {
            // Get all the colors from the image
            _uniqueFontColors = Parser.ColorPalette.Select(c => RgbToHex(c.R, c.G, c.B)).ToList();

            // Remove the mask color
            _uniqueFontColors.Remove(colorChip.MaskColor);

            // Convert into an array
            var colorRefs = _uniqueFontColors.ToArray();

            // Convert all of the pixels into color ids
            var pixelIDs = Parser.ColorPixels.Select(c => Array.IndexOf(colorRefs, RgbToHex(c.R, c.G, c.B)))
                .ToArray();

            // Create new image
            ImageData = new ImageData(Parser.Width, Parser.Height, pixelIDs, colorRefs);

            StepCompleted();
        }

        public override void PrepareSprites()
        {
            base.PrepareSprites();

            _fontMap = new int[totalSprites];
        }

        protected override void PostCutOutSprites()
        {
            _fontChip.AddFont(Parser.FileName.Split('.').First(), _fontMap);
            base.PostCutOutSprites();
        }

        protected override void ProcessSpriteData()
        {
            var id = -1;

            // If the sprite chip has unique sprites, try to find an existing sprite first
            if (spriteChip.Unique) id = spriteChip.FindSprite(spriteData);

            // If the sprite ID is -1 look for an empty sprite
            if (id == -1) id = spriteChip.NextEmptyId();

            // Add the font character sprite data
            spriteChip.UpdateSpriteAt(id, spriteData);

            // Set the id to the font map
            _fontMap[index] = id;
        }
    }

    public partial class Loader
    {
        [FileParser("font.png", FileFlags.Fonts)]
        public void ParseFonts(string file, PixelVision engine)
        {
            AddParser(new FontParser(file, _imageParser, engine.ColorChip, engine.FontChip));
        }
    }
}