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

        private readonly ColorChip colorChip;

        private readonly List<IColor> colors = new List<IColor>();

        //private readonly bool ignoreTransparent;

        private readonly ITexture2D tex;
        private readonly bool unique;
        private IColor tmpColor;
        private int totalColors;
        private int totalPixels;
        private int x, y, width, height;
        private IColor magenta;

        public ColorParser(ITexture2D tex, IEngineChips chips, IColor magenta, bool unique = false, bool ignoreTransparent = true)
        {
            this.tex = tex;
            colorChip = chips.colorChip;
            this.unique = unique;
            this.magenta = magenta;

            //this.ignoreTransparent = ignoreTransparent;

            CalculateSteps();
        }

        public override void CalculateSteps()
        {
            base.CalculateSteps();
            steps.Add(IndexColors);
            steps.Add(ReadColors);
            steps.Add(ResetColorChip);
            steps.Add(UpdateColors);
            steps.Add(RecalculateColors);
        }


        public void IndexColors()
        {
            //Debug.Log("Index Colors");
            // Get the total pixels from the texture
            var pixels = tex.GetPixels();
            totalPixels = pixels.Length;

            width = tex.width;
            height = tex.height;

            currentStep++;
        }

        public void ReadColors()
        {
            //Debug.Log("Read Colors");

            // Loop through each color and find the unique ones
            for (var i = 0; i < totalPixels; i++)
            {
                //PosUtil.CalculatePosition(i, width, out x, out y);
                x = i % width;
                y = i / width;

                y = height - y - 1;

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
            //Debug.Log("Reset Color Chip");

            ////Debug.Log("Total Colors Imported "+ total);
            // Clear the colors first
            colorChip.Clear();

            // Update the color chip to support the number of colors found
//            colorChip.RebuildColorPages(colors.Count);

            currentStep++;
        }

        public void UpdateColors()
        {
            //Debug.Log("Update Colors");

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
            //Debug.Log("Recalculate Colors");

            // Update supported colors based on what was imported
            colorChip.RecalculateSupportedColors();

            currentStep++;
        }

    }

}