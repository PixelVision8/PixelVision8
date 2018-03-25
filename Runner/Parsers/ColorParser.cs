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

using System.Collections.Generic;
using PixelVisionSDK;
using PixelVisionSDK.Chips;

namespace PixelVisionRunner.Parsers
{

    public class ColorParser : AbstractParser
    {
        protected IEngineChips chips;
        
        protected IColorChip colorChip;
        protected readonly List<IColor> colors = new List<IColor>();
        protected readonly ITexture2D tex;
        protected readonly bool unique;
        protected IColor tmpColor;
        protected int totalColors;
        protected int totalPixels;
        protected int x, y, width;
        protected IColor magenta;
        
        public ColorParser(ITexture2D tex, IEngineChips chips, IColor magenta, bool unique = false, bool ignoreTransparent = true)
        {
            this.tex = tex;
            this.chips = chips;
            this.unique = unique;
            this.magenta = magenta;

        }

        public override void CalculateSteps()
        {
            colorChip = chips.colorChip;
            
            base.CalculateSteps();
            steps.Add(IndexColors);
            steps.Add(ReadColors);
            steps.Add(ResetColorChip);
            steps.Add(UpdateColors);
            steps.Add(RecalculateColors);
        }


        public virtual void IndexColors()
        {
            //Debug.Log("Index Colors");
            // Get the total pixels from the texture
            var pixels = tex.GetPixels();
            totalPixels = pixels.Length;

            width = tex.width;

            currentStep++;
        }

        public virtual void ReadColors()
        {
            // Loop through each color and find the unique ones
            for (var i = 0; i < totalPixels; i++)
            {
                //PosUtil.CalculatePosition(i, width, out x, out y);
                x = i % width;
                y = i / width;

                // Get the current color
                tmpColor = tex.GetPixel(x, y); //pixels[i]);

                if (tmpColor.a < 1) // && !ignoreTransparent)
                    tmpColor = magenta;

                // Look to see if the color is already in the list
                if (!colors.Contains(tmpColor) && unique)
                    colors.Add(tmpColor);
                else if (unique == false)
                    colors.Add(tmpColor);
            }

            totalColors = colors.Count;

            currentStep++;
        }

        public void ResetColorChip()
        {
            // Clear the colors first
            colorChip.Clear();

            currentStep++;
        }

        public void UpdateColors()
        {
            for (var i = 0; i < totalColors; i++)
            {
                var tmpColor = colors[i];
                var hex = ColorData.ColorToHex(tmpColor.r, tmpColor.g, tmpColor.b);

                colorChip.UpdateColorAt(i, hex);
            }

            currentStep++;
        }

        public void RecalculateColors()
        {
            // Update supported colors based on what was imported
            colorChip.RecalculateSupportedColors();

            currentStep++;
        }

    }

}