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

using PixelVision8.Player;
using System.Collections.Generic;

namespace PixelVision8.Runner
{
    public class ColorParser : ImageParser
    {
        private readonly List<ColorData> _colors = new List<ColorData>();

        private readonly ColorChip _colorChip;

        private readonly ColorData _magenta;
        private ColorData _tmpColor;
        private int _totalColors;

        public ColorParser(string sourceFile, IImageParser parser, ColorChip colorChip) : base(parser)
        {
            SourcePath = sourceFile;
            this._colorChip = colorChip;
            // unique = colorChip.unique;
            _magenta = new ColorData(colorChip.MaskColor);
        }

        public override void CalculateSteps()
        {
            _colorChip.Clear();

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
                _tmpColor = srcColors[i]; //pixels[i]);

                if (_tmpColor.A < 1) // && !ignoreTransparent)
                    _tmpColor = _magenta;

                _colors.Add(_tmpColor);
            }

            _totalColors = _colors.Count;

            CurrentStep++;
        }

        public void UpdateColors()
        {
            for (var i = 0; i < _totalColors; i++)
            {
                var tmpColor = _colors[i];
                var hex = ColorUtils.RgbToHex(tmpColor);

                _colorChip.UpdateColorAt(i, hex);
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