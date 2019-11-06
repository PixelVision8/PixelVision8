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
using Microsoft.Xna.Framework;
using PixelVision8.Engine.Chips;
using PixelVision8.Engine.Utils;

namespace PixelVision8.Runner.Parsers
{
    public class SpriteParser : ImageParser
    {
        protected IEngineChips chips;
        protected Color[] colorData;
        protected int cps;
        protected int index;
        protected Color maskColor;
        protected int maxPerLoop = 100;
        protected int maxSprites;
        protected SpriteChip spriteChip;
        protected int[] spriteData;
        protected int spriteHeight;
        protected int spritesAdded;
        protected int spriteWidth;

        protected Color[] srcColors;

        //        protected ITexture2D tex;
        protected Color[] tmpPixels;
        protected int totalPixels;
        protected int totalSprites;
        protected int x, y, columns, rows;

        //        protected ITextureFactory textureFactory;
        //        protected byte[] data;

        public SpriteParser(IImageParser imageParser, IEngineChips chips, bool unique = true,
            SpriteChip spriteChip = null) : base(imageParser)
        {
            //            this.textureFactory = textureFactory;


            this.chips = chips;
            this.spriteChip = spriteChip ?? chips.spriteChip;

            spriteWidth = this.spriteChip.width;
            spriteHeight = this.spriteChip.height;
        }

        protected virtual void CalculateBounds()
        {
            columns = MathUtil.CeilToInt(imageParser.width / spriteWidth);
            rows = MathUtil.CeilToInt(imageParser.height / spriteHeight);

            // Find the total from the width and height
            totalSprites = columns * rows;
        }

        public override void CalculateSteps()
        {
            base.CalculateSteps();

            CalculateBounds();

            steps.Add(PrepareSprites);

            var loops = MathUtil.CeilToInt((float) totalSprites / maxPerLoop);

            for (var i = 0; i < loops; i++) steps.Add(CutOutSprites);

            steps.Add(PostCutOutSprites);
        }

        public virtual void PrepareSprites()
        {
            cps = spriteChip.colorsPerSprite;

            colorData = chips.GetChip(ColorMapParser.chipName, false) is ColorChip colorMapChip
                ? colorMapChip.colors
                : chips.colorChip.colors;

            maskColor = ColorUtils.HexToColor(chips.colorChip.maskColor);
            maxSprites = SpriteChipUtil.CalculateTotalSprites(spriteChip.textureWidth, spriteChip.textureHeight,
                spriteWidth, spriteHeight);

            // Create tmp arrays for color and reference data
            totalPixels = spriteChip.width * spriteChip.height;
            tmpPixels = new Color[totalPixels];
            spriteData = new int[totalPixels];

            // Keep track of number of sprites added
            spritesAdded = 0;

            //TODO this should be set by the parser
            srcColors = imageParser.colorPixels; //data.Select(c => new ColorAdapter(c) as Color).ToArray();

            currentStep++;
        }

        public virtual void CutOutSprites()
        {
            for (var i = 0; i < maxPerLoop; i++)
            {
                // Cut out the sprite from the texture
                CutOutSpriteFromTexture2D();

                if (!IsEmpty(tmpPixels))
                {
                    // Convert sprite to color index
                    ConvertColorsToIndexes(cps);

                    ProcessSpriteData();
                }

                index++;

                if (index >= totalSprites) break;
            }

            currentStep++;
        }

        protected virtual void PostCutOutSprites()
        {
            // Add custom logic
            currentStep++;
        }

        protected virtual void ProcessSpriteData()
        {
            if (spritesAdded < maxSprites)
            {
                // TODO need to deprecate this since the sprite file should load up exactly how it is read
                if (spriteChip.unique)
                {
                    if (spriteChip.FindSprite(spriteData) == -1)
                    {
                        spriteChip.UpdateSpriteAt(spritesAdded, spriteData);
                        spritesAdded++;
                    }
                }
                else
                {
                    if (spriteChip.IsEmpty(spriteData) == false)
                    {
                        spriteChip.UpdateSpriteAt(index, spriteData);
                        spritesAdded++;
                    }
                }
            }
        }

        public virtual bool IsEmpty(Color[] pixels)
        {
            var total = pixels.Length;
            var transPixels = 0;

            for (var i = 0; i < total; i++)
                if (pixels[i].A < 1)
                    transPixels++;

            return transPixels == total;
        }

        public virtual void CutOutSpriteFromTexture2D()
        {
            x = index % columns * spriteWidth;
            y = index / columns * spriteHeight;

            tmpPixels = GetPixels(x, y, spriteWidth, spriteHeight);
        }

        public Color[] GetPixels(int x, int y, int blockWidth, int blockHeight)
        {
            var total = blockWidth * blockHeight;
            var data = new Color[total];

            if (data.Length < total)
                Array.Resize(ref data, total);

            // Per-line copy, as there is no special per-pixel logic required.

            // Vertical wrapping is not an issue. Horizontal wrapping requires splitting the copy into two operations.
            // Keep important data in local variables.
            int srcY;

            var offsetStart = (x % imageParser.width + imageParser.width) % imageParser.width;
            var offsetEnd = offsetStart + blockWidth;
            if (offsetEnd <= imageParser.width)
            {
                // Copy each entire line at once.
                for (var tmpY = blockHeight - 1; tmpY > -1; --tmpY)
                {
                    // Note: + size and the second modulo operation are required to get wrapped values between 0 and +size
                    srcY = ((y + tmpY) % imageParser.height + imageParser.height) % imageParser.height;
                    Array.Copy(srcColors, offsetStart + srcY * imageParser.width, data, tmpY * blockWidth, blockWidth);
                }
            }
            else
            {
                // Copy each non-wrapping section and each wrapped section separately.
                var wrap = offsetEnd % imageParser.width;
                for (var tmpY = blockHeight - 1; tmpY > -1; --tmpY)
                {
                    // Note: + size and the second modulo operation are required to get wrapped values between 0 and +size
                    srcY = ((y + tmpY) % imageParser.height + imageParser.height) % imageParser.height;
                    Array.Copy(srcColors, offsetStart + srcY * columns, data, tmpY * blockWidth, blockWidth - wrap);
                    Array.Copy(srcColors, srcY * imageParser.width, data, blockWidth - wrap + tmpY * blockWidth, wrap);
                }
            }

            return data;
        }


        public virtual void ConvertColorsToIndexes(int totalColors)
        {
            // Calculate the total number of pixels
            var total = tmpPixels.Length;

            // Adjust the size of the index array to match the pixel
            if (spriteData.Length != total)
                Array.Resize(ref spriteData, total);

            // Create a tmp color for the loop
            Color tmpColor;
            int tmpRefID;

            var colorIndex = new List<int>();

            // Loop through all the pixels and match up to color references
            for (var i = 0; i < total; i++)
            {
                // Find the current color in the loop
                tmpColor = tmpPixels[i];

                tmpRefID = Equals(tmpColor, maskColor) ? -1 : Array.IndexOf(colorData, tmpColor);

                // Look to see if color is not transparent
                if (tmpRefID > -1)
                {
                    // compare against the color index
                    var indexed = colorIndex.IndexOf(tmpRefID);

                    // if the color is not found let's index it
                    if (indexed == -1)
                        if (colorIndex.Count < totalColors)
                        {
                            // Add the color
                            colorIndex.Add(tmpRefID);

                            // Update the index
                            indexed = colorIndex.Count - 1;
                        }

                    // if the index is still empty (we ran out of colors, then make transparent)
                    if (indexed == -1)
                        tmpRefID = -1;
                }

                // Update the value in the indexes array
                spriteData[i] = tmpRefID;
            }
        }
    }
}