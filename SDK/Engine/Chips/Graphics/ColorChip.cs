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

using System;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using PixelVision8.Engine.Utils;

namespace PixelVision8.Engine.Chips
{
    /// <summary>
    ///     The <see cref="ColorChip" /> represents the system colors of the engine.
    ///     It allows the engine to work in color indexes that the display can map
    ///     to actual colors in Unity via the class's set of APIs.
    /// </summary>
    public class ColorChip : AbstractChip
    {
        protected int _bgColor;

        protected string[] _colors =
        {
            "#2d1b2e",
            "#218a91",
            "#3cc2fa",
            "#9af6fd",
            "#4a247c",
            "#574b67",
            "#937ac5",
            "#8ae25d",
            "#8e2b45",
            "#f04156",
            "#f272ce",
            "#d3c0a8",
            "#c5754a",
            "#f2a759",
            "#f7db53",
            "#f9f4ea"
        };

        private bool _debugMode;

        protected string _maskColor = "#FF00FF";

        private int _maxColors = -1;

        // protected Color[] colorCache;

        public bool unique;

        /// <summary>
        ///     This can be used as a flag to limit the number of colors on the chip.
        /// </summary>
        public int maxColors
        {
            //TODO this is not used in the chip and is here for the color tool.
            get => _maxColors == -1 ? _colors.Length : _maxColors;
            set => _maxColors = value == -1 ? -1 : MathHelper.Clamp(value, 2, 256);
        }


        // A flag to let the game chip know the last 128  colors are reserved for palettes
        //        public bool paletteMode;

        /// <summary>
        ///     The background color reference to use when rendering transparent in
        ///     the ScreenBufferTexture.
        /// </summary>
        public int backgroundColor
        {
            get => _bgColor;
            set
            {
                // We make sure that the bg color is never set to a value out of the range of the color chip
                _bgColor = MathHelper.Clamp(value, 0, total);
                Invalidate();
            }
        }

        public string maskColor
        {
            get => _maskColor;
            set
            {
                if (ValidateHexColor(value)) _maskColor = value.ToUpper();
            }
        }

        /// <summary>
        ///     Get and Set the <see cref="supportedColors" /> number of <see cref="colors" />
        ///     in the palette. Changing the <see cref="supportedColors" /> will clear the
        ///     palette when it resizes.
        /// </summary>
        /// <value>Int</value>
        // TODO need to change this to totalSet colors or something more descriptive
        public int totalUsedColors => hexColors.Length - hexColors.ToList().RemoveAll(c => c == maskColor);

        //TODO need to figure out a better way to set this up?
        //        public int maxColors
        //        {
        //            get => _maxColors == -1 ? total : _maxColors;
        //            set => _maxColors = value;
        //        }

        public string[] hexColors
        {
            get
            {
                var colors = new string[total];

                Array.Copy(_colors, colors, total);

                return colors;
            }
        }

        /// <summary>
        ///     The <see cref="supportedColors" /> number of <see cref="colors" /> the chip can
        ///     support. This lock makes the sure that the <see cref="colors" />
        ///     array will never be larger than this value.
        /// </summary>
        /// <value>Int</value>
        public int total
        {
            get => _colors.Length;
            set
            {
                var oldTotal = _colors.Length;

                Array.Resize(ref _colors, value);
                if (oldTotal < value)
                    for (var i = oldTotal; i < value; i++)
                        _colors[i] = maskColor;

                Invalidate();
            }
        }

        /// <summary>
        ///     Returns a list of color data to be used for rendering.
        /// </summary>
        /// <value>ColorData[]</value>
        // public Color[] colors
        // {
        //     get
        //     {
        //         if (invalid || colorCache == null)
        //         {
        //             var t = total;
        //             colorCache = new Color[t];
        //
        //             for (var i = 0; i < t; i++)
        //             {
        //                 var colorHex = _colors[i];
        //
        //                 if (colorHex == maskColor && debugMode == false) colorHex = _colors[backgroundColor];
        //
        //                 var color = ColorUtils.HexToColor(colorHex); // {flag = invalidColors[i]};
        //                 colorCache[i] = color;
        //             }
        //
        //             ResetValidation();
        //         }
        //
        //         return colorCache;
        //     }
        // }

        // Setting this to true will use the mask color for empty colors instead of replacing them with the bg color
        public bool debugMode
        {
            get => _debugMode;
            set
            {
                _debugMode = value;
                Invalidate();
            }
        }

        public bool invalid { get; protected set; } = true;

        public void Invalidate()
        {
            invalid = true;
        }

        public void ResetValidation(int value = 0)
        {
            invalid = false;
        }

        public string ReadColorAt(int index)
        {
            return index < 0 || index > _colors.Length - 1 ? maskColor : _colors[index];
        }

        //        public int FindColorID(string color)
        //        {
        //            return Array.IndexOf(_colors, color);
        //        }

        public void Clear(string color = null)
        {
            if (color == null)
                color = maskColor;

            // Console.WriteLine("Clear " + color);
            var t = _colors.Length;
            for (var i = 0; i < t; i++) UpdateColorAt(i, color);
        }

        public virtual void UpdateColorAt(int index, string color)
        {
            if (index >= _colors.Length || index < 0) return;

            // Make sure that all colors are uppercase
            color = color.ToUpper();

            if (ColorUtils.ValidateHexColor(color))
            {
                _colors[index] = color;
                Invalidate();
            }
        }

        /// <summary>
        ///     This method configures the chip. It registers itself with the
        ///     engine as the default ColorChip, it sets the supported
        ///     <see cref="colors" /> to the maximum value of 256 and calls
        ///     <see cref="RevertColors" /> to add the default <see cref="colors" />
        ///     to the <see cref="colors" /> array.
        /// </summary>
        public override void Configure()
        {
            engine.ColorChip = this;
            backgroundColor = -1;
            total = 256;

            //            RebuildColorPages(16);
        }

        public override void Deactivate()
        {
            base.Deactivate();
            engine.ColorChip = null;
        }

        public bool ValidateHexColor(string inputColor)
        {
            return Regex.Match(inputColor, "^#(?:[0-9a-fA-F]{3}){1,2}$").Success;
        }
    }
}