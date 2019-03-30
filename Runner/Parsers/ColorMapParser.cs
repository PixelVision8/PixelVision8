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

using Microsoft.Xna.Framework;
using PixelVision8.Engine.Chips;
using PixelVision8.Engine.Utils;

namespace PixelVision8.Runner.Parsers
{

    public class ColorMapParser : ColorParser
    {
        public static string chipName = "PixelVisionSDK.Chips.ColorMapChip";

        public ColorMapParser(IImageParser imageParser, ColorChip colorChip, Color magenta, bool unique = false) : base(imageParser, colorChip, magenta, unique)
        {
            
        }

        public override void CalculateSteps()
        {
            currentStep = 0;
            steps.Add(ParseImageData);
//            steps.Add(IndexColors);
            steps.Add(ReadColors);
            steps.Add(BuildColorMap);
        }

        public void BuildColorMap()
        {
           
            // Force the Color Map Chip to load all of the colors into a single page
            colorChip.total = totalColors;
            
            
//            colorChip.RebuildColorPages(totalColors);

            for (var i = 0; i < totalColors; i++)
            {
                var tmpColor = colors[i];
                var hex = ColorUtils.RgbToHex(tmpColor.R, tmpColor.G, tmpColor.B);

                colorChip.UpdateColorAt(i, hex);
            }

            currentStep++;
        }

    }

}