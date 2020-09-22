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

using Microsoft.Xna.Framework;
using PixelVision8.Engine.Chips;
using PixelVision8.Engine.Utils;

namespace PixelVision8.Runner.Parsers
{
    public class ColorMapParser : ColorParser
    {
        public static string chipName = "PixelVisionSDK.Chips.ColorMapChip";

        public ColorMapParser(IImageParser parser, ColorChip colorChip, Color magenta, bool unique = false) : base(
            parser, colorChip)
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