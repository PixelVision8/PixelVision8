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
using System.Linq;
using MonoGameRunner.Data;
using PixelVisionSDK;
using PixelVisionSDK.Chips;

namespace PixelVisionRunner.Parsers
{

    public class ColorParser : PNGParser
    {

        protected ColorChip colorChip;
        protected readonly List<IColor> colors = new List<IColor>();
        
        protected readonly bool unique;
        protected IColor tmpColor;
        protected int totalColors;

        protected IColor magenta;

        public ColorParser(byte[] bytes, ColorChip colorChip, IColor magenta,
            bool unique = false):base(bytes)
        {

            this.colorChip = colorChip;
            this.unique = unique;
            this.magenta = magenta;

        }

        public override void CalculateSteps()
        {
            base.CalculateSteps();

//            steps.Add(IndexColors);
            steps.Add(ReadColors);
//            steps.Add(ResetColorChip);
            steps.Add(UpdateColors);
        }

        public virtual void ReadColors()
        {
            
            var srcColors = unique ? colorPalette.ToArray() : data.Select(c => new ColorAdapter(c) as IColor).ToArray();
            var total = srcColors.Length;
            
                // Loop through each color and find the unique ones
                for (var i = 0; i < total; i++)
                {
                    // Get the current color
                    tmpColor = srcColors[i]; //pixels[i]);

                    if (tmpColor.a < 1) // && !ignoreTransparent)
                        tmpColor = magenta;

                    colors.Add(tmpColor);
                }

            totalColors = colors.Count;

            currentStep++;
        }
//
//        public void ResetColorChip()
//        {
//            // Clear the colors first
////            colorChip.Clear();
//
//            currentStep++;
//        }

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

    }

}