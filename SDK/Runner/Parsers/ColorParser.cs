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
using System.Collections.Generic;

namespace PixelVision8.Runner
{
    public class ColorParser : ImageParser
    {
        protected readonly List<Color> colors = new List<Color>();

        // protected readonly bool unique;

        protected ColorChip colorChip;

        protected Color magenta;
        protected Color tmpColor;
        protected int totalColors;

        public ColorParser(string sourceFile, IImageParser parser, ColorChip colorChip) : base(parser)
        {
            SourcePath = sourceFile;
            this.colorChip = colorChip;
            // unique = colorChip.unique;
            magenta = DisplayTarget.HexToColor(colorChip.MaskColor);
        }

        public override void CalculateSteps()
        {
            colorChip.Clear();

            base.CalculateSteps();

            //            steps.Add(IndexColors);
            Steps.Add(ReadColors);
            //            steps.Add(ResetColorChip);
            Steps.Add(UpdateColors);
        }

        public virtual void ReadColors()
        {
            // Parse colors as normal

            var srcColors = Parser.ColorPixels;
            var total = srcColors.Length;

            // Loop through each color and find the unique ones
            for (var i = 0; i < total; i++)
            {
                // Get the current color
                tmpColor = srcColors[i]; //pixels[i]);

                if (tmpColor.A < 1) // && !ignoreTransparent)
                    tmpColor = magenta;

                colors.Add(tmpColor);
            }

            totalColors = colors.Count;

            CurrentStep++;
        }

        public void UpdateColors()
        {
            for (var i = 0; i < totalColors; i++)
            {
                var tmpColor = colors[i];
                var hex = SpriteImageParser.RgbToHex(tmpColor.R, tmpColor.G, tmpColor.B);

                colorChip.UpdateColorAt(i, hex);
            }

            CurrentStep++;
        }
    }

    public partial class Loader
    {
        [FileParser("colors.png", FileFlags.Colors)]
        public void ParseColors(string file, PixelVision engine)
        {
            AddParser(new ColorParser(file, _imageParser, engine.ColorChip));
        }
    }
}