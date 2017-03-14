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
// 

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

        public static void ClearTextureData(ref TextureData textureData, int colorRef = -1)
        {
            var pixels = textureData.GetPixels();
            var total = pixels.Length;
            for (var i = 0; i < total; i++)
                pixels[i] = colorRef;

            textureData.SetPixels(pixels);
        }

        public static int CalcualteTotalSprites(int width, int height, int spriteWidth, int spriteHeight)
        {
            //TODO this needs to be double checked at different size sprites
            var cols = MathUtil.FloorToInt(width / spriteWidth);
            var rows = MathUtil.FloorToInt(height / spriteHeight);
            return cols * rows;
        }

        public static void CutOutSpriteFromTextureData(int index, TextureData textureData, int spriteWidth,
            int spriteHeight, int[] data)
        {
            int x;
            int y;
            CalculateSpritePos(index, textureData.width, textureData.height, spriteWidth, spriteHeight, out x, out y);

            textureData.GetPixels(x, y, spriteWidth, spriteHeight, data);
        }

        public static void AddSpriteToTextureData(int index, int[] pixels, TextureData textureData, int spriteWidth,
            int spriteHeight)
        {
            int x;
            int y;
            CalculateSpritePos(index, textureData.width, textureData.height, spriteWidth, spriteHeight, out x, out y);

            textureData.SetPixels(x, y, spriteWidth, spriteHeight, pixels);
        }

        public static void CalculateSpritePos(int index, int width, int height, int spriteWidth, int spriteHeight,
            out int x, out int y,
            bool flipY = true)
        {
            var totalSprites = CalcualteTotalSprites(width, height, spriteWidth, spriteHeight);

            // Make sure we stay in bounds
            index = index.Clamp(0, totalSprites - 1);

            var w = width / spriteWidth;

            x = index % w * spriteWidth;
            y = index / w * spriteHeight;

            if (flipY)
                y = height - y - spriteHeight;
        }

        public static void CalculatePixelPos(int index, int width, int height, out int x, out int y)
        {
            var totalPixels = width * height;

            // Make sure we stay in bounds
            index = index.Clamp(0, totalPixels - 1);

            x = index % width;
            y = index / height;

            y = height - 1 - y;
        }

        public static void CloneTextureData(TextureData source, TextureData target)
        {
            target.SetPixels(source.GetPixels());
        }

        public static void FlipSpriteData(ref int[] pixelData, int sWidth, int sHeight, bool flipH = false,
            bool flipV = false)
        {
            var total = pixelData.Length;
            if (pixels.Length != total)
                Array.Resize(ref pixels, total);

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

        public static void ScalePixelData(ref int[] pixelData, int sWidth, int sHeight, int scale = 1)
        {
            // make sure we don't go smaller than 1
            scale = Math.Max(1, scale);
            var oldSize = pixelData.Length;

            var oldData = new int[oldSize];

            Array.Copy(pixelData, oldData, oldSize);

            var newSpriteWidth = sWidth * scale;
            var newSpriteHeight = sHeight * scale;

            var newSize = newSpriteWidth * newSpriteHeight;

            Array.Resize(ref pixelData, newSize);
            for (var i = 0; i < newSize; i++)
            {
                int x, y, oldIndex;

                // Calculate the nex x,y pos
                PosUtil.CalculatePosition(i, newSpriteWidth, out x, out y);

                // Convert to the old index
                PosUtil.CalculateIndex(x / scale, y / scale, 8, out oldIndex);

                pixelData[i] = oldData[oldIndex];
            }

            //Debug.Log("Old Size "+ oldSize + " new size "+newSize +" new size "+ newSpriteWidth+"x"+ newSpriteHeight);
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

        public static void ShiftPixelData(ref int[] pixelData, int offset, int emptyColorID = -1)
        {
            var total = pixelData.Length;
            for (var i = 0; i < total; i++)
                if (pixelData[i] > -1)
                    pixelData[i] = pixelData[i] + offset;
                else
                    pixelData[i] = emptyColorID;
        }

        /// <summary>
        ///     Tests to see if sprite <paramref name="data" /> is empty. This method
        ///     iterates over all the ints in the supplied <paramref name="data" />
        ///     array and looks for a value of -1. If all values are -1 then it
        ///     returns true.
        /// </summary>
        /// <param name="data">An array of ints</param>
        /// <returns>
        /// </returns>
        public static bool IsEmpty(int[] data)
        {
            var total = data.Length;
            for (var i = 0; i < total; i++)
                if (data[i] > -1)
                    return false;

            return false;
        }

        public static void CovertSpritesToRawData(ref int[] pixelData, int[] spriteIDs, int width, IEngineChips chips)
        {
            var spriteChip = chips.spriteChip;

            var spriteWidth = spriteChip.width;
            var spriteHeight = spriteChip.height;


            //TODO need to allow flipping and match the draw sprite API
            var realHeight = spriteHeight * MathUtil.CeilToInt(spriteIDs.Length / width);
            var realWidth = spriteWidth * width;

            var data = new TextureData(realWidth, realHeight);

            //Debug.Log("Draw "+ids.Length + " sprites.");

            var total = spriteIDs.Length;
            var spriteData = new int[8 * 8];
            for (var i = 0; i < total; i++)
            {
                var id = spriteIDs[i];
                if (id > -1)
                {
                    var newX = MathUtil.FloorToInt(i % width) * spriteWidth;
                    var newY = MathUtil.FloorToInt(i / width) * spriteWidth;
                    newY = realHeight - spriteHeight - newY;
                    spriteChip.ReadSpriteAt(id, spriteData);
                    data.SetPixels(newX, newY, spriteWidth, spriteHeight, spriteData);

                    //DrawSprite(id, newX, newY);
                }
            }

            data.CopyPixels(pixelData);

            //var pixelData = data.GetPixels();
            //return pixelData;
        }
    }
}