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

    public class ColorMapParser : ColorParser
    {

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

        public void BuildColorMap()
        {
           
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