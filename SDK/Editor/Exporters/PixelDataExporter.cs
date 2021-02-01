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
using PixelVision8.Player;

namespace PixelVision8.Runner.Exporters
{
    public class PixelDataExporter : ImageExporter
    {
        protected Color maskColor = new Color(255, 0, 255);
        protected Color[] paletteColors;
        protected int[] pixelData;

        public PixelDataExporter(string fileName, int[] pixelData, int width, int height, Color[] paletteColors,
            IImageExporter imageExporter, string maskHex) : base(fileName, imageExporter, null, width, height)
        {
            this.paletteColors = paletteColors;
            this.pixelData = pixelData;
            maskColor = DisplayTarget.HexToColor(maskHex);
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

            colors = new Color[total];

            for (var i = 0; i < total; i++)
            {
                var refID = pixelData[i];

                if (refID > -1 && refID < total)
                    colors[i] = paletteColors[refID];
                else
                    colors[i] = maskColor;
            }

            CurrentStep++;
        }
    }
}