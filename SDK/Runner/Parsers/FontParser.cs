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


/* Unmerged change from project 'PixelVision8.CoreDesktop'
Before:
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using PixelVision8.Engine.Chips;
After:
using Microsoft.Xna.Framework;
using System.Collections.Chips;
using PixelVision8.Engine.Utils;
using System;
using System.Collections.Chips;
*/

using System;
using System.Collections.Generic;
using System.Linq;
using PixelVision8.Player;

namespace PixelVision8.Runner
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
            uniqueFontColors = Parser.colorPalette.Select(c => Utilities.RgbToHex(c.R, c.G, c.B)).ToList();

            // Remove the mask color
            uniqueFontColors.Remove(colorChip.maskColor);

            // Convert into an array
            var colorRefs = uniqueFontColors.ToArray();

            // Convert all of the pixels into color ids
            var pixelIDs = Parser.colorPixels.Select(c => Array.IndexOf(colorRefs, Utilities.RgbToHex(c.R, c.G, c.B))).ToArray();

            // Create new image
            ImageData = new ImageData(Parser.width, Parser.height, pixelIDs, colorRefs);

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
    
    public partial class Loader
    {
        [FileParser("font.png", "Fonts")]
        public void ParseFonts(string[] files, IPlayerChips engine)
        {
            for (int i = 0; i < files.Length; i++)
            {
                // We only want to parse a single sprite file so just take the first one in the list
                var imageParser = new PNGParser(files[i], _graphicsDevice, engine.ColorChip.maskColor);

                AddParser(new FontParser(imageParser, engine.ColorChip, engine.FontChip));
            }
            
        }
    }
}