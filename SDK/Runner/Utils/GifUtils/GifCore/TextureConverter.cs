//   
// Copyright (c) Jesse Freeman, Pixel Vision 8. All rights reserved.  
//  
// Licensed under the Microsoft Public License (MS-PL) except for a few
// portions of the code. See LICENSE file in the project root for full 
// license information. Third-party libraries used by Pixel Vision 8 are 
// under their own licenses. Please refer to those libraries for details 
// on the license they use.
//
// Based on SimpleGif (https://github.com/hippogamesunity/simplegif) by
// Nate River of Hippo Games
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

namespace PixelVision8.Runner.Gif
{
    /// <summary>
    /// Converter textures.
    /// </summary>
    internal static class TextureConverter
    {
        /// <summary>
        /// /// Apply master palette to convert true color image to 256-color image.
        /// </summary>
        public static void ConvertTo8Bits(ref Texture2D texture, MasterPalette palette)
        {
            if (palette == MasterPalette.DontApply) return;

            var pixels = texture.GetPixels32();

            if (palette == MasterPalette.Grayscale)
            {
                for (var j = 0; j < pixels.Length; j++)
                {
                    if (pixels[j].A < 128)
                    {
                        pixels[j] = new Color();
                    }
                    else
                    {
                        var brightness = (byte) (0.2126 * pixels[j].R + 0.7152 * pixels[j].G + 0.0722 * pixels[j].B);
                        var color = new Color(brightness, brightness, brightness, Convert.ToByte(255));

                        pixels[j] = color;
                    }
                }

                texture.SetPixels32(pixels);
            }
            else
            {
                var levels = GetLevels(palette);
                var dividers = new[] {256 / levels[0], 256 / levels[1], 256 / levels[2]};

                for (var j = 0; j < pixels.Length; j++)
                {
                    var r = (byte) (pixels[j].R / dividers[0] * dividers[0]);
                    var g = (byte) (pixels[j].G / dividers[1] * dividers[1]);
                    var b = (byte) (pixels[j].B / dividers[2] * dividers[2]);
                    var a = (byte) (pixels[j].A < 128 ? 0 : 255);
                    var color = a == 0 ? new Color() : new Color(r, g, b, a);

                    pixels[j] = color;
                }

                texture.SetPixels32(pixels);
            }
        }

        private static int[] GetLevels(MasterPalette palette)
        {
            switch (palette)
            {
                case MasterPalette.Levels666: return new[] {6, 6, 6};
                case MasterPalette.Levels676: return new[] {6, 7, 6};
                case MasterPalette.Levels685: return new[] {6, 8, 5};
                case MasterPalette.Levels884: return new[] {8, 8, 4};
                default: throw new ArgumentOutOfRangeException("Unsupported master palette: " + palette);
            }
        }
    }
}