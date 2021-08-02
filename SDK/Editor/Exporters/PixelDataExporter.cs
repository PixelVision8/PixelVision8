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

// using Microsoft.Xna.Framework;
using PixelVision8.Runner;
using PixelVision8.Player;
using System;

namespace PixelVision8.Editor
{
    public class PixelDataExporter : ImageExporter
    {
        // protected ColorData maskColor = new ColorData(255, 0, 255);
        protected ColorData[] paletteColors;
        protected int[] pixelData;

        public PixelDataExporter(string fileName, int[] pixelData, int width, int height, ColorData[] paletteColors,
            IImageExporter imageExporter) : base(fileName, imageExporter, null, width, height)
        {

            this.paletteColors = paletteColors;//new ColorData[paletteColors.Length + 1];

            // this.paletteColors[0] = new ColorData(maskHex);

            // Array.Copy(paletteColors, 0, this.paletteColors, 1, paletteColors.Length);

            // this.paletteColors = paletteColors;
            this.pixelData = pixelData;
            // maskColor = new ColorData(maskHex);
        }

        public override void CalculateSteps()
        {
            CurrentStep = 0;

            Steps.Add(CopyPixels);

            Steps.Add(WriteBytes);
        }

        protected virtual void CopyPixels()
        {
            var total = width * height;

            colors = new ColorData[total];

            var totalColors = paletteColors.Length;

            for (var i = 0; i < total; i++)
            {
                var refID = Utilities.Clamp(pixelData[i], 0, totalColors);
                colors[i] = paletteColors[refID];
            }

            CurrentStep++;
        }
    }
}