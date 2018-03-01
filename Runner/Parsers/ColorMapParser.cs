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

    public class ColorMapParser : ColorParser
    {

//        private readonly IEngineChips chips;
//        private IColorChip colorChip;
//        private readonly List<IColor> colors = new List<IColor>();
//        private readonly bool ignoreTransparent;
//        private readonly ITexture2D tex;
//        private IColor tmpColor;
//        private int totalColors;
//        private int totalPixels;
//        private readonly bool unique;
//        private int x, y, width, height;
//        private IColor magenta;

        public ColorMapParser(ITexture2D tex, IEngineChips chips, IColor magenta, bool unique = false, bool ignoreTransparent = true) : base(tex, chips, magenta, unique, ignoreTransparent)
        {
            
        }

        public override void CalculateSteps()
        {
            
            var colorMapChip = new ColorMapChip();
            chips.chipManager.ActivateChip(colorMapChip.GetType().FullName, colorMapChip);
            
            colorChip = colorMapChip;
            
            currentStep = 0;

            steps.Add(IndexColors);
            steps.Add(ReadColors);
            steps.Add(BuildColorMap);
        }

//        public void IndexColors()
//        {
//
//            // Get the total pixels from the texture
//            var pixels = tex.GetPixels();
//            totalPixels = pixels.Length;
//
//            width = tex.width;
//            height = tex.height;
//
//            currentStep++;
//        }

//        public void ReadColors()
//        {
            //Debug.Log("Read Colors");

//            // Loop through each color and find the unique ones
//            for (var i = 0; i < totalPixels; i++)
//            {
//                //PosUtil.CalculatePosition(i, width, out x, out y);
//                x = i % width;
//                y = i / width;
////                y = height - y - 1;
//
//                // Get the current color
//                tmpColor = tex.GetPixel(x, y); //pixels[i]);
//
//                if (tmpColor.a < 1 && !ignoreTransparent)
//                    tmpColor = magenta;
//
//                // Look to see if the color is already in the list
//                if (!colors.Contains(tmpColor) && unique)
//                    colors.Add(tmpColor);
//                else if (unique == false)
//                    colors.Add(tmpColor);
//            }
//
//            totalColors = colors.Count;
//
//            currentStep++;
//        }

        public void BuildColorMap()
        {
            //Debug.Log("Build Color Map");

            colorChip.RebuildColorPages(totalColors);

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