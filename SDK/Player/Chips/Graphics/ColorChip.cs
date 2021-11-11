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
    
        public static bool ValidateHexColor(string inputColor)
        {
            return Regex.Match(inputColor, "^#(?:[0-9a-fA-F]{3}){1,2}$").Success;
        }

        private int _bgColor = 0;

        private string[] _colors = Constants.DefaultColors.Split(',');
        
        private bool _debugMode;


        // private string _maskColor => Constants.MaskColor;//ReadColorAt(Constants.EmptyPixel); // TODO need to have this return the correct mask color

        /// <summary>
        ///     The background color reference to use when rendering transparent in
        ///     the ScreenBufferTexture.
        /// </summary>
        

        // public string MaskColor
        // {
        //     get => ReadColorAt(Constants.EmptyPixel);//_maskColor; // TODO this should return the first color
        //     set
        //     {
        //         // if (ValidateHexColor(value))
        //         UpdateColorAt(Constants.EmptyPixel, value.ToUpper());// _maskColor = value.ToUpper();
        //     }
        // }

        /// <summary>
        ///     Get and Set the <see cref="supportedColors" /> number of <see cref="colors" />
        ///     in the palette. Changing the <see cref="supportedColors" /> will clear the
        ///     palette when it resizes.
        /// </summary>
        /// <value>Int</value>
        // TODO need to change this to totalSet colors or something more descriptive
        // public int TotalUsedColors => HexColors.Length - HexColors.ToList().RemoveAll(c => c == MaskColor);

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
                
                // Make sure the total is in range
                if (value < 2) value = 2;
                
                var oldTotal = _colors.Length;

                // TODO should the mask color be the first color?
                // _colors[0]= MaskColor;

                Array.Resize(ref _colors, value);
                if (oldTotal < value)
                    for (var i = oldTotal; i < value; i++)
                        _colors[i] = _colors[Constants.EmptyPixel]; // TODO this was set to the mask color before

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

        public void ResetValidation()
        {
            Invalid = false;
        }

        public string ReadColorAt(int index)
        {
            // TODO need to make sure this makes sense where it returns the bg color if it's out of range - should never be out of range
            return index < 0 || index >= _colors.Length ? _colors[Constants.EmptyPixel] : _colors[index];
        }

        public void Clear(string color)
        {
            // color ??= MaskColor;

            // Console.WriteLine("Clear " + color);
            var t = _colors.Length;
            for (var i = 0; i < t; i++) UpdateColorAt(i, color);
        }

        public virtual void UpdateColorAt(int index, string color)
        {
            if (index >= _colors.Length || index < 0) return;

            // Make sure that all colors are uppercase
            color = color.ToUpper();

            if (ValidateHexColor(color) && _colors[index] != color)
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
            // BackgroundColor = 1;
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