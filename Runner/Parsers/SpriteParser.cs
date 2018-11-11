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
using MonoGameRunner.Data;
using PixelVisionSDK;
using PixelVisionSDK.Chips;
using PixelVisionSDK.Utils;

namespace PixelVisionRunner.Parsers
{

    public class SpriteParser : PNGParser
    {

        protected IEngineChips chips;
        protected IColor[] colorData;
        protected int cps;
        protected int index;
        protected int maxSprites;
        protected int spriteHeight;
        protected SpriteChip spriteChip;
        protected int[] spriteData;
        protected int spritesAdded;
        protected int spriteWidth;

//        protected ITexture2D tex;
        protected IColor[] tmpPixels;
        protected int totalPixels;
        protected int totalSprites;
        protected int x, y, columns, rows;
        protected IColor maskColor;
        protected int maxPerLoop = 100;
        
//        protected ITextureFactory textureFactory;
//        protected byte[] data;
        
        public SpriteParser(byte[] bytes, IEngineChips chips, bool unique = true):base(bytes)
        {
 
//            this.textureFactory = textureFactory;
            
            
           
            this.chips = chips;
            spriteChip = chips.spriteChip;
            
            spriteWidth = spriteChip.width;
            spriteHeight = spriteChip.height;

        }

        protected virtual void CalculateBounds()
        {
            columns = MathUtil.CeilToInt(imageWidth / spriteWidth);
            rows = MathUtil.CeilToInt(imageHeight / spriteHeight);
            
            // Find the total from the width and height
            totalSprites = columns * rows;
        }
        
        public override void CalculateSteps()
        {

            base.CalculateSteps();
            
            CalculateBounds();
            
            steps.Add(PrepareSprites);
            
            var loops = MathUtil.CeilToInt((float) totalSprites / maxPerLoop);
            
            for (int i = 0; i < loops; i++)
            {
                steps.Add(CutOutSprites);
            }
            
            steps.Add(PostCutOutSprites);
        }

        protected IColor[] srcColors;
        
        public virtual void PrepareSprites()
        {
            cps = spriteChip.colorsPerSprite;
            
            var colorMapChip = chips.chipManager.GetChip(ColorPaletteParser.chipName, false) as ColorChip;
            
            // TODO This should be the only test once the colorMap is deprecated
            if (colorMapChip == null)
            {
                colorMapChip = chips.chipManager.GetChip(ColorMapParser.chipName, false) as ColorChip;
            }
            
            colorData = colorMapChip != null ? colorMapChip.colors : chips.colorChip.colors;
            maskColor = new ColorData(chips.colorChip.maskColor);
            maxSprites = SpriteChipUtil.CalculateTotalSprites(spriteChip.textureWidth, spriteChip.textureHeight, spriteWidth, spriteHeight);

            // Create tmp arrays for color and reference data
            totalPixels = spriteChip.width * spriteChip.height;
            tmpPixels = new IColor[totalPixels];
            spriteData = new int[totalPixels];

            // Keep track of number of sprites added
            spritesAdded = 0;

            //TODO this should be set by the parser
            srcColors = data.Select(c => new ColorAdapter(c) as IColor).ToArray();
            
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

                if (index >= totalSprites)
                {
                    break;
                }
                
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
                    spriteChip.UpdateSpriteAt(index, spriteData);
                    spritesAdded++;
                }
            }
                
        }

        public virtual bool IsEmpty(IColor[] pixels)
        {
            var total = pixels.Length;
            var transPixels = 0;

            for (var i = 0; i < total; i++)
                if (pixels[i].a < 1)
                    transPixels++;

            return transPixels == total;
        }
        
        public virtual void CutOutSpriteFromTexture2D()
        {
            x = index % columns * spriteWidth;
            y = index / columns * spriteHeight;

            tmpPixels = GetPixels(x, y, spriteWidth, spriteHeight);

        }
        
        public IColor[] GetPixels(int x, int y, int blockWidth, int blockHeight)
        {
            
            var total = blockWidth * blockHeight;
            var data = new IColor[total];
            
            if (data.Length < total)
                Array.Resize(ref data, total);

            // Per-line copy, as there is no special per-pixel logic required.

            // Vertical wrapping is not an issue. Horizontal wrapping requires splitting the copy into two operations.
            // Keep important data in local variables.
            int srcY;
            
            int offsetStart = ((x % imageWidth) + imageWidth) % imageWidth;
            int offsetEnd = offsetStart + blockWidth;
            if (offsetEnd <= imageWidth)
            {
                // Copy each entire line at once.
                for (var tmpY = blockHeight - 1; tmpY > -1; --tmpY)
                {
                    // Note: + size and the second modulo operation are required to get wrapped values between 0 and +size
                    srcY = (((y + tmpY) % imageHeight) + imageHeight) % imageHeight;
                    Array.Copy(srcColors, offsetStart + srcY * imageWidth, data, tmpY * blockWidth, blockWidth);
                }
            }
            else
            {
                // Copy each non-wrapping section and each wrapped section separately.
                int wrap = offsetEnd % imageWidth;
                for (var tmpY = blockHeight - 1; tmpY > -1; --tmpY)
                {
                    // Note: + size and the second modulo operation are required to get wrapped values between 0 and +size
                    srcY = (((y + tmpY) % imageHeight) + imageHeight) % imageHeight;
                    Array.Copy(srcColors, offsetStart + srcY * columns, data, tmpY * blockWidth, blockWidth - wrap);
                    Array.Copy(srcColors, srcY * imageWidth, data, (blockWidth - wrap) + tmpY * blockWidth, wrap);
                }
            }

            return data;

        }
        

        public void ConvertColorsToIndexes(int totalColors)
        {

            // Calculate the total number of pixels
            var total = tmpPixels.Length;

            // Adjust the size of the index array to match the pixel
            if (spriteData.Length != total)
                Array.Resize(ref spriteData, total);

            // Create a tmp color for the loop
            IColor tmpColor;
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