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
using System.Diagnostics;

namespace PixelVision8.Engine.Utils
{

    public static class PixelDataUtil
    {
        private static int _mergeDestWidth;
        private static int _mergeCol;
        private static int _mergeRow;
        private static int _mergeX;
        private static int _mergeY;
        private static int _total;
        private static int _tmpPixel;
        private static int[] _tmpPixels;

        public static int[] GetPixels(PixelData pixelData) => GetPixels(pixelData, 0, 0, pixelData.Width, pixelData.Height);

        public static int[] GetPixels(PixelData pixelData, int x, int y, int blockWidth, int blockHeight)
        {
            _tmpPixels = new int[blockWidth * blockHeight];

            CopyPixels(pixelData, x, y, blockWidth, blockHeight, ref _tmpPixels);

            return _tmpPixels;
        }

        public static void SetPixels(int[] srcPixels, PixelData destPixelData) => SetPixels(srcPixels, 0, 0, destPixelData.Width, destPixelData.Height, destPixelData);

        public static void SetPixels(int[] srcPixels, int x, int y, int blockWidth, int blockHeight, PixelData destPixelData)
        {
            ValidateBounds(ref blockWidth, ref blockHeight, ref x, ref y, destPixelData.Width, destPixelData.Height);

            // Exit if the width or height is not valid
            if (blockWidth < 1 || blockHeight < 1)
                return;

            for (var i = blockHeight - 1; i > -1; i--)
            {
                try
                {
                    Array.Copy(
                        srcPixels,
                        i * blockWidth,
                        destPixelData.Pixels,
                        x + (i + y) * destPixelData.Width,
                        blockWidth
                    );
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    // throw;
                }

            }

        }

        public static void CopyPixels(PixelData srcPixelData, int x, int y, int sampleWidth, int sampleHeight, ref int[] destPixels)
        {

            ValidateBounds(ref sampleWidth, ref sampleHeight, ref x, ref y, srcPixelData.Width, srcPixelData.Height);

            if (sampleWidth < 1 || sampleHeight < 1)
                return;

            // Copy each entire line at once.
            for (var i = sampleHeight - 1; i > -1; --i)
            {

                Array.Copy(
                    srcPixelData.Pixels,
                    x + (y + i) * srcPixelData.Width,
                    destPixels,
                    i * sampleWidth,
                    sampleWidth
                );

            }

        }

        public static void Clear(PixelData pixelData, int colorRef = -1)
        {
            for (var i = pixelData.TotalPixels - 1; i > -1; i--) pixelData[i] = colorRef;
        }

        public static void MergePixels(PixelData src, int sampleX, int sampleY, int sampleWidth, int sampleHeight,
            PixelData dest,
            int destX, int destY, bool flipH = false, bool flipV = false, int colorOffset = 0,
            bool ignoreTransparent = true)
        {
            _mergeDestWidth = dest.Width;

            ValidateBounds(ref sampleWidth, ref sampleHeight, ref destX, ref destY, _mergeDestWidth, dest.Height);

            if (sampleWidth < 1 || sampleHeight < 1)
                return;

            _total = sampleWidth * sampleHeight;

            if (_total == 0)
                return;

            _mergeCol = 0;
            _mergeRow = 0;

            for (var i = 0; i < _total; i++)
            {
                _mergeX = _mergeCol;
                _mergeY = _mergeRow;

                _tmpPixel = src.Pixels[(_mergeX + sampleX) + (src.Width * (_mergeY + sampleY))];

                if (_tmpPixel != -1 || ignoreTransparent != true)
                {

                    if (flipH)
                        _mergeX = sampleWidth - 1 - _mergeX;

                    if (flipV)
                        _mergeY = sampleWidth - 1 - _mergeY;

                    dest.Pixels[(_mergeX + destX) + _mergeDestWidth * (_mergeY + destY)] = _tmpPixel + colorOffset;
                }

                _mergeCol++;

                if (_mergeCol >= sampleWidth)
                {
                    _mergeCol = 0;
                    _mergeRow++;
                }
            }
        }

        private static void ValidateBounds(ref int srcWidth, ref int srcHeight, ref int destX,
            ref int destY, int destWidth, int destHeight)
        {

            // Adjust X
            if (destX < 0)
            {
                srcWidth += destX;
                destX = 0;
            }

            // Adjust Y
            if (destY < 0)
            {
                srcHeight += destY;
                destY = 0;
            }

            // Adjust Width
            if (destX + srcWidth > destWidth)
            {
                srcWidth -= (destX + srcWidth) - destWidth;
            }

            // Adjust Height.
            if (destY + srcHeight > destHeight)
            {
                srcHeight -= destY + srcHeight - destHeight;
            }

        }

        public static void Resize(PixelData pixelData, int blockWidth, int blockHeight)
        {
            pixelData.Resize(MathUtil.Clamp(blockWidth, 1, 2048), MathUtil.Clamp(blockHeight, 1, 2048));

            Clear(pixelData);
        }

    }

}