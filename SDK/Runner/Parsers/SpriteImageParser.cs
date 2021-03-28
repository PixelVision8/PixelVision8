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
    public class SpriteImageParser : ImageParser
    {
        protected ColorChip colorChip;
        protected int cps;
        protected int index;
        protected int maxSprites;
        protected SpriteChip spriteChip;
        protected int[] spriteData;
        protected int spriteHeight = 8;
        protected int spritesAdded;
        protected int spriteWidth = 8;
        protected int totalSprites;
        protected int x, y;
        public ImageData ImageData;

        public SpriteImageParser(string sourceFile, IImageParser parser, ColorChip colorChip,
            SpriteChip spriteChip = null) : base(parser)
        {
            SourcePath = sourceFile;

            Parser = parser;

            // this.chips = chips;
            this.spriteChip = spriteChip;
            this.colorChip = colorChip;

            // Read the color chip's mask color
            MaskHex = colorChip.MaskColor;

            if (spriteChip != null)
            {
                spriteWidth = SpriteChip.DefaultSpriteSize;
                spriteHeight = SpriteChip.DefaultSpriteSize;
            }
        }

        public override void CalculateSteps()
        {
            base.CalculateSteps();

            Steps.Add(CreateImage);

            if (spriteChip != null)
            {
                Steps.Add(PrepareSprites);

                Steps.Add(CutOutSprites);

                Steps.Add(PostCutOutSprites);
            }
        }

        public virtual void PrepareSprites()
        {
            cps = spriteChip.ColorsPerSprite;

            totalSprites = ImageData.TotalSprites;

            //TODO this needs to be double checked at different size sprites
            var cols = (int)Math.Floor((double)spriteChip.TextureWidth / spriteWidth);
            var rows = (int)Math.Floor((double)spriteChip.TextureHeight / spriteHeight);

            maxSprites = cols * rows;

            // // Keep track of number of sprites added
            spritesAdded = 0;

            StepCompleted();
        }

        public virtual void CreateImage()
        {
            // Get the chip colors and replace any transparent ones with the first color so we don't parse transparency
            var colorData = ColorUtils.ConvertColors(colorChip.HexColors, colorChip.MaskColor);

            // colorData = colorChip.colors;

            var colorRefs = colorData.Select(c => ColorUtils.RgbToHex(c)).ToArray();

            // Remove the colors that are not supported
            Array.Resize(ref colorRefs, colorChip.TotalUsedColors);

            var imageColors = Parser.ColorPalette.Select(c => ColorUtils.RgbToHex(c)).ToArray();

            var colorMap = new string[colorRefs.Length];

            var orphanColors = new List<string>();
            var uniqueColorIDs = new List<int>();

            var totalImageColors = imageColors.Length;
            // var totalColorRefs = colorRefs.Length;

            for (int i = 0; i < totalImageColors; i++)
            {
                // var color = imageColors[i];
                var color = imageColors[i];

                if (color == colorChip.MaskColor) continue;

                var id = Array.IndexOf(colorRefs, color);

                // Keep track of colors in and out of range
                if (uniqueColorIDs.IndexOf(id) == -1)
                {
                    uniqueColorIDs.Add(id);
                }

                if (id > -1)
                {
                    colorMap[id] = color;
                }
                else
                {
                    orphanColors.Add(color);
                }
            }

            // Sort colors
            uniqueColorIDs.Sort();

            var indexes = new List<int>();

            // find open slots
            for (int i = 0; i < colorMap.Length; i++)
            {
                if (colorMap[i] == null)
                {
                    indexes.Add(i);
                }
            }

            var totalOrphanColors = orphanColors.Count;

            for (int i = 0; i < indexes.Count; i++)
            {
                if (i < totalOrphanColors)
                {
                    colorMap[indexes[i]] = orphanColors[i];
                }
            }

            // clean up the color map
            for (int i = 0; i < colorMap.Length; i++)
            {
                if (colorMap[i] == null)
                {
                    colorMap[i] = colorRefs[i];
                }
            }

            // Convert all of the pixels into color ids
            var pixelIDs = Parser.ColorPixels.Select(c => Array.IndexOf(colorMap, ColorUtils.RgbToHex(c)))
                .ToArray();

            ImageData = new ImageData(Parser.Width, Parser.Height, pixelIDs, colorMap);

            StepCompleted();
        }

        public virtual void CutOutSprites()
        {
            for (var i = 0; i < totalSprites; i++)
            {
                // Convert sprite to color index
                ConvertColorsToIndexes(cps);

                ProcessSpriteData();

                index++;
            }

            StepCompleted();
        }

        protected virtual void PostCutOutSprites()
        {
            // Add custom logic
            StepCompleted();
        }

        protected virtual void ProcessSpriteData()
        {
            if (spritesAdded < maxSprites)
            {
                // TODO need to deprecate this since the sprite file should load up exactly how it is read
                if (spriteChip.Unique)
                {
                    if (spriteChip.FindSprite(spriteData) == -1)
                    {
                        spriteChip.UpdateSpriteAt(spritesAdded, spriteData);
                        spritesAdded++;
                    }
                }
                else
                {
                    if (SpriteChip.IsEmpty(spriteData) == false)
                    {
                        spriteChip.UpdateSpriteAt(index, spriteData);
                        spritesAdded++;
                    }
                }
            }
        }

        public virtual void ConvertColorsToIndexes(int totalColors)
        {
            spriteData = ImageData.GetSpriteData(index, totalColors);
        }

        public override void Dispose()
        {
            base.Dispose();
            colorChip = null;
            spriteChip = null;
        }
    }

    public partial class Loader
    {
        [FileParser("sprites.png", FileFlags.Sprites)]
        public void ParseSprites(string file, PixelVision engine)
        {
            AddParser(new SpriteImageParser(file, _imageParser, engine.ColorChip, engine.SpriteChip));
        }
    }
}