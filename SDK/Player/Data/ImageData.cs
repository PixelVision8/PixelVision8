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
using System.Collections.Generic;

namespace PixelVision8.Player
{
    /// <summary>
    ///     Wrapper for texture data that includes Hex color data to rebuild colors when
    ///     exporting and cutting up sprites. 
    /// </summary>
    public struct ImageData
    {
        public PixelData PixelData { get; }
        public int Width => PixelData.Width;
        public int Height => PixelData.Height;
        
        public string[] Colors;
        
        public ImageData(int width, int height, int[] pixels = null, string[] colors = null)
        {
            // Create sprite friendly grid
            PixelData = new PixelData(width, height);
            
            // Clear the pixel data with the default transparent color
            Utilities.Clear(PixelData);

            // See if there are any pixels
            if (pixels != null)
            {
                // Copy pixels over to PixelData at default size
                Utilities.SetPixels(pixels, 0, 0, width, height, PixelData);
            }

            // Save a color lookup table
            Colors = colors ?? Constants.DefaultColors.Split(',');

        }

        public int[] GetPixels(int x, int y, int width, int height) => Utilities.GetPixels(PixelData, x, y, width, height);
        
        public int[] GetPixels() => Utilities.GetPixels(PixelData);

        public void Resize(int newWidth, int newHeight) => Utilities.Resize(PixelData, newWidth, newHeight);

        public void Clear(int color = -1) => Utilities.Clear(PixelData, color);

    }
}