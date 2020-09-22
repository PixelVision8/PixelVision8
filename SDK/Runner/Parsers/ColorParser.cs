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
using Microsoft.Xna.Framework;
using PixelVision8.Engine.Chips;
using PixelVision8.Engine.Utils;

namespace PixelVision8.Runner.Parsers
{
    public class ColorParser : ImageParser
    {
        protected readonly List<Color> colors = new List<Color>();

        protected readonly bool unique;

        protected ColorChip colorChip;

        protected Color magenta;
        protected Color tmpColor;
        protected int totalColors;

        public ColorParser(IImageParser parser, ColorChip colorChip) : base(parser)
        {
            this.colorChip = colorChip;
            unique = colorChip.unique;
            magenta = ColorUtils.HexToColor(colorChip.maskColor);
        }

        public override void CalculateSteps()
        {
            colorChip.Clear();

            base.CalculateSteps();

            //            steps.Add(IndexColors);
            steps.Add(ReadColors);
            //            steps.Add(ResetColorChip);
            steps.Add(UpdateColors);
        }

        public virtual void ReadColors()
        {
            // TODO this should be removed in future releases, it's only here to support legacy games

            // If we are loading a legacy game and no system colors are defined, used the image parser's palette

            //            if (colorChip.supportedColors == null)
            //            {
            //                string[] systemColors = imageParser.colorPalette.Select(c => ((ColorData) c).ToHex()).ToArray();
            ////
            //                for (int i = 0; i < systemColors.Length; i++)
            //                {
            //                    colorChip.AddSupportedColor(systemColors[i]);
            //                }
            //            }

            // Parse colors as normal

            var srcColors =
                unique
                    ? Parser.colorPalette.ToArray()
                    : Parser.colorPixels; //data.Select(c => new ColorAdapter(c) as Color).ToArray();
            var total = srcColors.Length;

            // Loop through each color and find the unique ones
            for (var i = 0; i < total; i++)
            {
                // Get the current color
                tmpColor = srcColors[i]; //pixels[i]);

                if (tmpColor.A < 1) // && !ignoreTransparent)
                    tmpColor = magenta;

                if (unique && tmpColor == magenta)
                {
                }
                else
                {
                    //                if(tmpColor != magenta)
                    colors.Add(tmpColor);
                }
            }

            totalColors = colors.Count;

            currentStep++;
        }
        //
        //        public void ResetColorChip()
        //        {
        //            // Clear the colors first
        ////            colorChip.Clear();
        //
        //            currentStep++;
        //        }

        public void UpdateColors()
        {
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