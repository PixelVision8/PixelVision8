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
        protected ColorData[] colorData;
        protected IColor[] colors;
        protected string emptySpriteData;
        protected int index;
        protected int maxSprites;
        protected int sHeight;
        protected SpriteChip spriteChip;
        protected int[] spriteData;
        protected int spritesAdded;
        protected int sWidth;

        protected ITexture2D tex;
        protected IColor tmpColor;
        protected IColor[] tmpPixels;
        protected int totalColors;
        protected int totalPixels;
        protected int totalSprites;
        protected IColorFactory colorFactory;

//        protected bool unique;
        protected int x, y, width, height;

        public SpriteParser(ITexture2D tex, IEngineChips chips, IColorFactory colorFactory, bool unique = true)
        {
            this.tex = tex;
//            this.unique = unique;
            this.chips = chips;
            spriteChip = chips.spriteChip;
            this.colorFactory = colorFactory;
//            Debug.Log("Unique " + spriteChip.unique);

            CalculateSteps();
        }

        public override void CalculateSteps()
        {
            base.CalculateSteps();
            steps.Add(ReadColors);
            steps.Add(ConvertColors);
            steps.Add(PrepareSprites);
            steps.Add(CutOutSprites);
        }

        public void ReadColors()
        {
            //Debug.Log("Read Colors");

            colorData = chips.colorMapChip != null ? chips.colorMapChip.colors : chips.colorChip.colors;

            currentStep++;
        }

        public void ConvertColors()
        {
            //Debug.Log("Convert  Colors");

            var total = colorData.Length;
            colors = colorFactory.CreateArray(total);

            for (var i = 0; i < total; i++)
            {
                var c1 = colors[i];
                var c2 = colorData[i];

                c1.r = c2.r;
                c1.g = c2.g;
                c1.b = c2.b;
                c1.a = 1;

                colors[i] = c1;
            }

            currentStep++;
        }

        //TextureData tmpData = new TextureData(1,1,false);

        public virtual void PrepareSprites()
        {
            //Debug.Log("Prepare Sprites");

            tex.UsePointFiltering();

            sWidth = spriteChip.width;
            sHeight = spriteChip.height;

            // Calculate values needed to cut out sprites
            width = MathUtil.CeilToInt(tex.width / sWidth);
            height = MathUtil.CeilToInt(tex.height / sHeight);
            totalSprites = width * height;

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
            //Debug.Log("Cut Out Sprites");

            // Loop through all the potential sprites and import them
            index = 0;
            var cps = spriteChip.colorsPerSprite;

            for (index = 0; index < totalSprites; index++)
            {
                // Cut out the sprite from the texture
                CutOutSpriteFromTexture2D();

                if (!IsEmpty(tmpPixels))
                {
                    // Convert sprite to color index
                    ConvertColorsToIndexes(cps);

                    ProcessSpriteData();
                }
            }

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

            // Flip Y position
            y = tex.height - y - sHeight;

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

                // if color is transparent set to -1, if not try to look up in the ref colors array
                tmpRefID = Array.IndexOf(colors, tmpColor);

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