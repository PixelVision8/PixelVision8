//   
// Copyright (c) Jesse Freeman. All rights reserved.  
//  
// Licensed under the Microsoft Public License (MS-PL) License. 
// See LICENSE file in the project root for full license information. 
// 
// Contributors
// --------------------------------------------------------
// This is the official list of Pixel Vision 8 contributors:
//  
// Jesse Freeman - @JesseFreeman
// Christer Kaitila - @McFunkypants
// Pedro Medeiros - @saint11
// Shawn Rakowski - @shwany

using System;
using System.Text;
using PixelVisionSDK.Chips;

namespace PixelVisionSDK.Utils
{

    public class SpriteChipUtil
    {

        private static int[] pixels = new int[0];

        public static int[] tmpPixelData = new int[8 * 8];
        private static readonly StringBuilder tmpSB = new StringBuilder();

        public static int CalculateTotalSprites(int width, int height, int spriteWidth, int spriteHeight)
        {
            //TODO this needs to be double checked at different size sprites
            var cols = MathUtil.FloorToInt(width / spriteWidth);
            var rows = MathUtil.FloorToInt(height / spriteHeight);
            return cols * rows;
        }

        public static void CalculateSpritePos(int index, int width, int height, int spriteWidth, int spriteHeight,
            out int x, out int y,
            bool flipY = true)
        {
            var totalSprites = CalculateTotalSprites(width, height, spriteWidth, spriteHeight);

            // Make sure we stay in bounds
            index = index.Clamp(0, totalSprites - 1);

            var w = width / spriteWidth;

            x = index % w * spriteWidth;
            y = index / w * spriteHeight;

            if (flipY)
                y = height - y - spriteHeight;
        }

        public static void CloneTextureData(TextureData source, TextureData target)
        {
            source.CopyPixels(ref tmpPixelData);
            target.SetPixels(tmpPixelData);
        }

        public static void FlipSpriteData(ref int[] pixelData, int sWidth, int sHeight, bool flipH = false,
            bool flipV = false)
        {
            var total = pixelData.Length;
            if (pixels.Length < total)
                Array.Resize(ref pixels, total);

            Array.Copy(pixelData, pixels, total);

            for (var ix = 0; ix < sWidth; ix++)
            for (var iy = 0; iy < sHeight; iy++)
            {
                var newx = ix;
                var newY = iy;
                if (flipH)
                    newx = sWidth - 1 - ix;
                if (flipV)
                    newY = sHeight - 1 - iy;
                
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

            for (var i = 0; i < total; i++)
                tmpSB.Append(data[i]);

            return tmpSB.ToString(); //string.Join(",", data.Select(x => x.ToString()).ToArray()));
        }

    }

}