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
using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace PixelVision8.Player
{

    #region Color Chip Class
    
    /// <summary>
    ///     The <see cref="ColorChip" /> represents the system colors of the engine.
    ///     It allows the engine to work in color indexes that the display can map
    ///     to actual colors in Unity via the class's set of APIs.
    /// </summary>
    public class ColorChip : AbstractChip
    {
    
        public static Color HexToColor(string hex)
        {
            HexToRgb(hex, out var r, out var g, out var b);

            return new Color(r, g, b);
        }

        /// <summary>
        ///     Static method for converting a HEX color into an RGB value.
        /// </summary>
        /// <param name="hex"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        public static void HexToRgb(string hex, out byte r, out byte g, out byte b)
        {
            if (hex == null) hex = "FF00FF";

            if (hex[0] == '#') hex = hex.Substring(1);

            r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber); // / (float) byte.MaxValue;
            g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber); // / (float) byte.MaxValue;
            b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber); // / (float) byte.MaxValue;
        }
        
        public static bool ValidateHexColor(string inputColor)
        {
            return Regex.Match(inputColor, "^#(?:[0-9a-fA-F]{3}){1,2}$").Success;
        }

        private int _bgColor;

        private string[] _colors =
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

        private string _maskColor = "#FF00FF";

        /// <summary>
        ///     The background color reference to use when rendering transparent in
        ///     the ScreenBufferTexture.
        /// </summary>
        public int BackgroundColor
        {
            get => _bgColor;
            set
            {
                // We make sure that the bg color is never set to a value out of the range of the color chip
                _bgColor = MathHelper.Clamp(value, 0, Total);
                Invalidate();
            }
        }

        public string MaskColor
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
        public int TotalUsedColors => HexColors.Length - HexColors.ToList().RemoveAll(c => c == MaskColor);

        public string[] HexColors
        {
            get
            {
                var colors = new string[Total];

                Array.Copy(_colors, colors, Total);

                return colors;
            }
        }

        /// <summary>
        ///     The <see cref="supportedColors" /> number of <see cref="colors" /> the chip can
        ///     support. This lock makes the sure that the <see cref="colors" />
        ///     array will never be larger than this value.
        /// </summary>
        /// <value>Int</value>
        public int Total
        {
            get => _colors.Length;
            set
            {
                var oldTotal = _colors.Length;

                Array.Resize(ref _colors, value);
                if (oldTotal < value)
                    for (var i = oldTotal; i < value; i++)
                        _colors[i] = MaskColor;

                Invalidate();
            }
        }

        // Setting this to true will use the mask color for empty colors instead of replacing them with the bg color
        public bool DebugMode
        {
            get => _debugMode;
            set
            {
                _debugMode = value;
                Invalidate();
            }
        }

        public bool Invalid { get; protected set; } = true;

        public void Invalidate()
        {
            Invalid = true;
        }

        public void ResetValidation(int value = 0)
        {
            Invalid = false;
        }

        public string ReadColorAt(int index)
        {
            return index < 0 || index > _colors.Length - 1 ? MaskColor : _colors[index];
        }

        public void Clear(string color = null)
        {
            color ??= MaskColor;

            // Console.WriteLine("Clear " + color);
            var t = _colors.Length;
            for (var i = 0; i < t; i++) UpdateColorAt(i, color);
        }

        public virtual void UpdateColorAt(int index, string color)
        {
            if (index >= _colors.Length || index < 0) return;

            // Make sure that all colors are uppercase
            color = color.ToUpper();

            if (ValidateHexColor(color))
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
        protected override void Configure()
        {
            Player.ColorChip = this;
            BackgroundColor = -1;
            // total = 256;
        }
    }

    #endregion

    #region Modify PixelVision
    
    public partial class PixelVision
    {
        public ColorChip ColorChip { get; set; }
    }
        
    #endregion
}