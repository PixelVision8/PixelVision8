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
using PixelVisionSDK;
using PixelVisionSDK.Chips;
using PixelVisionSDK.Utils;

namespace PixelVisionRunner.Parsers
{

    public class SpriteParser : AbstractParser
    {

        protected IEngineChips chips;
        protected IColor[] colorData;
        protected int cps;
        protected int index;
        protected int maxSprites;
        protected int sHeight;
        protected SpriteChip spriteChip;
        protected int[] spriteData;
        protected int spritesAdded;
        protected int sWidth;

        protected ITexture2D tex;
        protected IColor[] tmpPixels;
        protected int totalPixels;
        protected int totalSprites;
        protected int x, y, width, height;
        protected IColor maskColor;
        protected int maxPerLoop = 100;
        
        public SpriteParser(ITexture2D tex, IEngineChips chips, bool unique = true)
        {
 
            this.tex = tex;
           
            this.chips = chips;
            spriteChip = chips.spriteChip;

        }


        protected virtual void CalculateBounds()
        {
            width = MathUtil.CeilToInt(tex.width / sWidth);
            height = MathUtil.CeilToInt(tex.height / sHeight);
            
            // Find the total from the width and height
            totalSprites = width * height;
        }
        
        public override void CalculateSteps()
        {
            base.CalculateSteps();
            
            
            sWidth = spriteChip.width;
            sHeight = spriteChip.height;

            CalculateBounds();
            
            
            
            steps.Add(PrepareSprites);
            
//            steps.Add(PreCutOutSprites);

            var loops = MathUtil.CeilToInt((float) totalSprites / maxPerLoop);
            
            for (int i = 0; i < loops; i++)
            {
                steps.Add(CutOutSprites);
            }
            
            steps.Add(PostCutOutSprites);
        }


        public virtual void PrepareSprites()
        {
            cps = spriteChip.colorsPerSprite;
            colorData = chips.colorMapChip != null ? chips.colorMapChip.colors : chips.colorChip.colors;
            maskColor = new ColorData(chips.colorChip.maskColor);
            maxSprites = SpriteChipUtil.CalculateTotalSprites(spriteChip.textureWidth, spriteChip.textureHeight, sWidth, sHeight);

            // Create tmp arrays for color and reference data
            totalPixels = spriteChip.width * spriteChip.height;
            tmpPixels = new IColor[totalPixels];
            spriteData = new int[totalPixels];

            // Keep track of number of sprites added
            spritesAdded = 0;

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
            x = index % width * sWidth;
            y = index / width * sHeight;

            tmpPixels = tex.GetPixels(x, y, sWidth, sHeight);

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