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

using PixelVisionSDK;
using PixelVisionSDK.Chips;

namespace PixelVisionRunner.Parsers
{

    public class ColorPaletteParser : ColorParser
    {
        public static string chipName = "PixelVisionSDK.Chips.ColorPaletteChip";

        public ColorPaletteParser(ITextureFactory textureFactory, byte[] data, ColorChip colorChip, IColor magenta, bool unique = false, bool ignoreTransparent = true) : base(textureFactory, data, colorChip, magenta, unique, ignoreTransparent)
        {
            
        }

        public override void CalculateSteps()
        {
            currentStep = 0;
            steps.Add(ParseImageData);
            steps.Add(IndexColors);
            steps.Add(ReadColors);
            steps.Add(BuildColorMap);
        }

        public void BuildColorMap()
        {
           
            // Force the Color Map Chip to load all of the colors into a single page
            colorChip.colorsPerPage = totalColors;
            
            
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