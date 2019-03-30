﻿//
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
 using PixelVisionSDK;


namespace PixelVisionRunner.Exporters
{
    public class PixelDataExporter : ImageExporter
    {
        protected int[] pixelData;
        protected Color[] paletteColors;
        
        public PixelDataExporter(string fileName, int[] pixelData, int width, int height, Color[] paletteColors, IImageExporter imageExporter) : base(fileName, imageExporter, null)
        {
            this.paletteColors = paletteColors;
            this.pixelData = pixelData;
            this.width = width;
            this.height = height;
        }

        public override void CalculateSteps()
        {

            currentStep = 0;

            steps.Add(CopyPixels);
            
            steps.Add(WriteBytes);
            
        }
        
        protected virtual void CopyPixels()
        {
            var total = width * height;

            colors = new Color[total];
     
                for (var i = 0; i < total; i++)
                {
                    var refID = pixelData[i];

                    if (refID > -1 && refID < total)
                        colors[i] = paletteColors[refID];
                    else
                    {
                        colors[i] = PixelVisionSDK.Utils.ColorUtils.HexToColor("#FF00FF");
                    }
                }
            
            currentStep++;
        }

    }
}