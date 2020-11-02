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
using System.Text;

namespace PixelVision8.Engine.Utils
{
    public class SpriteChipUtil
    {
        private static int[] pixels = new int[0];

        // public static int[] tmpPixelData = new int[8 * 8];
        private static readonly StringBuilder tmpSB = new StringBuilder();

        public static void FlipSpriteData(ref int[] pixelData, int sWidth, int sHeight, bool flipH = false,
            bool flipV = false)
        {
            var total = pixelData.Length;
            if (pixels.Length < total) Array.Resize(ref pixels, total);

            Array.Copy(pixelData, pixels, total);

            for (var ix = 0; ix < sWidth; ix++)
            for (var iy = 0; iy < sHeight; iy++)
            {
                var newx = ix;
                var newY = iy;
                if (flipH) newx = sWidth - 1 - ix;

                if (flipV) newY = sHeight - 1 - iy;

                pixelData[ix + iy * sWidth] = pixels[newx + newY * sWidth];
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="data"></param>
        /// <returns>
        /// </returns>
        public static string SpriteDataToString(int[] data)
        {
            tmpSB.Length = 0;
            var total = data.Length;

            for (var i = 0; i < total; i++) tmpSB.Append(data[i]);

            return tmpSB.ToString();
        }

    }
}