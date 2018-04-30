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
using UnityEngine;

namespace PixelVisionRunner.Parsers
{

    public class FlagColorParser : AbstractParser
    {
        
        public static string flagColorChipName = "PixelVisionSDK.Chips.FlagColorChip";

        public static string[] flagColors = new string[]
        {
            "#000000",
            "#101010",
            "#202020",
            "#303030",
            "#404040",
            "#505050",
            "#606060",
            "#707070",
            "#808080",
            "#8F8F8F",
            "#9F9F9F",
            "#AFAFAF",
            "#BFBFBF",
            "#CFCFCF",
            "#DFDFDF",
            "#EFEFEF",
        };
        
        protected IColor maskColor;
        private int flag;
        private int offset;
        private int realWidth;
        private int realHeight;
        

        private ColorChip flagColorChip;
        
        private ITexture2D flagTex;
        
        public FlagColorParser(ITexture2D tex, IEngineChips chips)
        {
            flagColorChip = new ColorChip();
            
            chips.chipManager.ActivateChip(flagColorChipName, flagColorChip, false);
            
            maskColor = new ColorData(chips.colorChip.maskColor);
            flagTex = tex;
            
        }

        public override void CalculateSteps()
        {
            steps.Add(ParseFlagColors);
            
            base.CalculateSteps();

        }

        
        public void ParseFlagColors()
        {
        
            var newFlagColors = new List<string>();
            
            if (flagTex == null)
            {
                Debug.Log("Use built in flag colors");
                newFlagColors = flagColors.ToList();
            }
            else
            {
                Debug.Log("Create custom flag colors");
                
                var pixels = flagTex.GetPixels();
    
                var total = pixels.Length;
    
                for (int i = 0; i < total; i++)
                {
                    var color = pixels[i];
                    var hex = ColorData.ColorToHex(color.r, color.g, color.b);
                    
                    
                    
                    if (color.a == 1f && !Equals(color, maskColor))
                    {
                        if (newFlagColors.IndexOf(hex) == -1)
                        {
                            
                            Debug.Log("Color "+ i + " " + hex);
                            
                            newFlagColors.Add(hex);
                        
                        }
                    }
                    
                }
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