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
using PixelVisionSDK;
using PixelVisionSDK.Chips;

namespace PixelVisionRunner.Parsers
{

    public class FlagColorParser : ImageParser
    {
        
        public static string flagColorChipName = "PixelVisionSDK.Chips.FlagColorChip";

        public static string[] flagColors = new string[]
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
            "#A53E5A",
        };
        
        protected IColor maskColor;
//        private int flag;
//        private int offset;
//        private int realWidth;
//        private int realHeight;

        private ColorChip flagColorChip;
        
        public FlagColorParser(IImageParser imageParser, IEngineChips chips) : base(imageParser)
        {

            flagColorChip = new ColorChip();
            
            chips.chipManager.ActivateChip(flagColorChipName, flagColorChip, false);
            
            maskColor = new ColorData(chips.colorChip.maskColor);

            
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

                for (int i = 0; i < total; i++)
                {
                    newFlagColors.Add(((ColorData)imageParser.colorPalette[i]).ToHex());
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

            flagColorChip.RebuildColorPages(newFlagColors.Count);
            
            for (int i = 0; i < newFlagColors.Count; i++)
            {
                flagColorChip.UpdateColorAt(i, newFlagColors[i]);
            }
            
            currentStep++;
        }
        
    }

}