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

using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using PixelVision8.Engine.Chips;
using PixelVision8.Engine.Utils;

namespace PixelVision8.Runner.Parsers
{
    public class FlagColorParser : ImageParser
    {
        public static string flagColorChipName = "PixelVisionSDK.Chips.FlagColorChip";

        public static string[] flagColors =
        {
            "#000000",
            "#5E0104",
            "#FB061B",
            "#FFFFFF",
            "#FC8029",
            "#FEFF48",
            "#858926",
            "#33470D",
            "#2EFF41",
            "#1B8431",
            "#36FFFE",
            "#827FF9",
            "#1A00E3",
            "#7102D0",
            "#FC6EC4",
            "#A53E5A"
        };
//        private int flag;
//        private int offset;
//        private int realWidth;
//        private int realHeight;

        private readonly ColorChip flagColorChip;

        protected Color maskColor;

        public FlagColorParser(IImageParser imageParser, IEngineChips chips) : base(imageParser)
        {
            flagColorChip = new ColorChip();

            chips.ActivateChip(flagColorChipName, flagColorChip, false);

            maskColor = ColorUtils.HexToColor(chips.colorChip.maskColor);
        }

        public override void CalculateSteps()
        {
            base.CalculateSteps();

            steps.Add(ParseFlagColors);
        }

        public void ParseFlagColors()
        {
            var newFlagColors = new List<string>();

            if (bytes == null)
            {
                newFlagColors = flagColors.ToList();
            }
            else
            {
                // TODO This is broken?

                var total = imageParser.colorPalette.Count;

                for (var i = 0; i < total; i++)
                {
                    var tmpColor = imageParser.colorPalette[i];
                    newFlagColors.Add(ColorUtils.RgbToHex(tmpColor.R, tmpColor.G, tmpColor.B));
                }

//                var pixels = tex.GetPixels();
//    
//                var total = pixels.Length;
//    
//                for (int i = 0; i < total; i++)
//                {
//                    var color = pixels[i];
//                    var hex = ColorData.ColorToHex(color.r, color.g, color.b);
//                    
//                    
//                    
//                    if (color.a == 1f && !Equals(color, maskColor))
//                    {
//                        if (newFlagColors.IndexOf(hex) == -1)
//                        {
//                            
//                            newFlagColors.Add(hex);
//                        
//                        }
//                    }
//                    
//                }
            }

//            flagColorChip.Resize(newFlagColors.Count);

            for (var i = 0; i < newFlagColors.Count; i++) flagColorChip.UpdateColorAt(i, newFlagColors[i]);

            currentStep++;
        }
    }
}