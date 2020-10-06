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
using Microsoft.Xna.Framework;

namespace PixelVision8.Engine.Utils
{
    
    public static class PixelDataUtil
    {
        public static int GetPixel(PixelData pixelData, int x, int y)
        {
            var size = pixelData.Height;
            y = (y % size + size) % size;
            size = pixelData.Width;
            x = (x % size + size) % size;
            // size is still == _width from the previous operation - let's reuse the local

            return pixelData.Pixels[x + size * y];
        }
        
        public static void SetPixel(PixelData pixelData, int x, int y, int color)
        {
            // Note: + size and the second modulo operation are required to get wrapped values between 0 and +size
            var size = pixelData.Height;
            y = (y % size + size) % size;
            size = pixelData.Width;
            x = (x % size + size) % size;
            // size is still == _width from the previous operation - let's reuse the local

            var index = x + size * y;

            pixelData.Pixels[index] = color;
        }
        
        public static int[] GetPixels(PixelData pixelData)
        {
            var tmpPixels = new int[pixelData.Pixels.Length];

            Array.Copy(pixelData.Pixels, tmpPixels, pixelData.Pixels.Length);

            return tmpPixels;
        }
        
        public static int[] GetPixels(PixelData pixelData, int x, int y, int blockWidth, int blockHeight)
        {
            var tmpPixels = new int[blockWidth * blockHeight];
        
            CopyPixels(ref tmpPixels, pixelData, x, y, blockWidth, blockHeight);
        
            //            Array.Copy(pixels, tmpPixels, pixels.Length);
        
            return tmpPixels;
        }
        
        public static void SetPixels(int[] pixels, PixelData pixelData)
        {
            var TotalPixels = Math.Min(pixels.Length, pixelData.Width * pixelData.Height);

            Array.Copy(pixels, pixelData.Pixels, TotalPixels);
            
        }
        
        public static void SetPixels(PixelData pixelData, int x, int y, int blockWidth, int blockHeight, int[] pixels)
        {
            var TotalPixels = blockWidth * blockHeight;

            if (TotalPixels == 0) return;

            // Per-line copy, as there is no special per-pixel logic required.

            // Vertical wrapping is not an issue. Horizontal wrapping requires splitting the copy into two operations.
            // Keep important data in local variables.
            int dstY;
            // var dst = Pixels;
            var width = pixelData.Width;
            var height = pixelData.Height;
            var offsetStart = (x % width + width) % width;
            var offsetEnd = offsetStart + blockWidth;
            if (offsetEnd <= width)
            {
                // Copy each entire line at once.
                for (var tmpY = blockHeight - 1; tmpY > -1; --tmpY)
                {
                    // Note: + size and the second modulo operation are required to get wrapped values between 0 and +size
                    dstY = ((y + tmpY) % height + height) % height;
                    Array.Copy(pixels, tmpY * blockWidth, pixelData.Pixels, offsetStart + dstY * width, blockWidth);
                }
            }
            else
            {
                // Copy each non-wrapping section and each wrapped section separately.
                var wrap = offsetEnd % width;
                for (var tmpY = blockHeight - 1; tmpY > -1; --tmpY)
                {
                    // Note: + size and the second modulo operation are required to get wrapped values between 0 and +size
                    dstY = ((y + tmpY) % height + height) % height;
                    Array.Copy(pixels, tmpY * blockWidth, pixelData.Pixels, offsetStart + dstY * width, blockWidth - wrap);
                    Array.Copy(pixels, blockWidth - wrap + tmpY * blockWidth, pixelData.Pixels, dstY * width, wrap);
                }
            }
        }
        
        public static void CopyPixels(ref int[] data, PixelData pixelData, int x, int y, int blockWidth, int blockHeight)
        {
            var TotalPixels = blockWidth * blockHeight;

            if (data.Length < TotalPixels) Array.Resize(ref data, TotalPixels);

            // Per-line copy, as there is no special per-pixel logic required.

            // Vertical wrapping is not an issue. Horizontal wrapping requires splitting the copy into two operations.
            // Keep important data in local variables.
            int srcY;
            var src = pixelData.Pixels;
            var width = pixelData.Width;
            var height = pixelData.Height;
            var offsetStart = (x % width + width) % width;
            var offsetEnd = offsetStart + blockWidth;
            if (offsetEnd <= width)
            {
                // Copy each entire line at once.
                for (var tmpY = blockHeight - 1; tmpY > -1; --tmpY)
                {
                    // Note: + size and the second modulo operation are required to get wrapped values between 0 and +size
                    srcY = ((y + tmpY) % height + height) % height;
                    Array.Copy(src, offsetStart + srcY * width, data, tmpY * blockWidth, blockWidth);
                }
            }
            else
            {
                // Copy each non-wrapping section and each wrapped section separately.
                var wrap = offsetEnd % width;
                for (var tmpY = blockHeight - 1; tmpY > -1; --tmpY)
                {
                    // Note: + size and the second modulo operation are required to get wrapped values between 0 and +size
                    srcY = ((y + tmpY) % height + height) % height;
                    Array.Copy(src, offsetStart + srcY * width, data, tmpY * blockWidth, blockWidth - wrap);
                    Array.Copy(src, srcY * width, data, blockWidth - wrap + tmpY * blockWidth, wrap);
                }
            }
        }
        
        public static void CopyPixels(PixelData pixelData, ref int[] data, bool ignoreTransparent, int transparentColor)
        {
            var TotalPixels = pixelData.Width * pixelData.Height;

            if (data.Length < TotalPixels) Array.Resize(ref data, TotalPixels);

            int color;

            if (!ignoreTransparent)
                Array.Copy(pixelData.Pixels, data, TotalPixels);
            else
                for (var i = 0; i < TotalPixels; i++)
                {
                    color = pixelData.Pixels[i];
                    if (color != transparentColor) data[i] = color;
                }
        }
        
        public static void Clear(PixelData pixelData,  int colorRef = -1, int x = 0, int y = 0, int? width = null, int? height = null)
        {
            int[] tmpPixels = null;

            var tmpWidth = width ?? pixelData.Width;
            var tmpHeight = height ?? pixelData.Height;

            var total = tmpWidth * tmpHeight;

            // TODO not sure why this sometimes goes to negative but this check should fix that
            if (total > 0)
            {
                tmpPixels = new int[total];

                for (var i = 0; i < total; i++) tmpPixels[i] = colorRef;

                SetPixels(pixelData, x, y, tmpWidth, tmpHeight, tmpPixels);
            }
        }
        
        public static void MergePixels(PixelData pixelData, int x, int y, int blockWidth, int blockHeight, int[] pixels, bool flipH = false, bool flipV = false, int colorOffset = 0, bool ignoreTransparent = true)
        {
            var TotalPixels = blockWidth * blockHeight;

            // Per-pixel copy.
            int pixel;
            int srcX, srcY;
            for (var i = TotalPixels - 1; i > -1; i--)
            {
                pixel = pixels?[i] ?? -1;

                if (pixel != -1 || ignoreTransparent != true)
                {
                    if (colorOffset > 0 && pixel != -1) pixel += colorOffset;

                    srcX = i % blockWidth;
                    srcY = i / blockWidth;

                    if (flipH) srcX = blockWidth - 1 - srcX;

                    if (flipV) srcY = blockWidth - 1 - srcY;

                    SetPixel(pixelData, srcX + x, srcY + y, pixel);
                }
            }
        }
        
        public static void Crop(PixelData pixelData, int x, int y, int blockWidth, int blockHeight)
        {
            if (!ValidateBounds(pixelData.Width, pixelData.Height, ref x, ref y, ref blockWidth, ref blockHeight))
                return;

            var tmpPixelData = GetPixels(pixelData, x, y, blockWidth, blockHeight);

            pixelData.Width = blockWidth;
            pixelData.Height = blockHeight;

            Array.Resize(ref pixelData.Pixels, pixelData.Width * pixelData.Height);

            SetPixels(tmpPixelData, pixelData);
        }
        
        public static bool ValidateBounds(int width, int height, ref int x, ref int y, ref int blockWidth, ref int blockHeight)
        {
            // Adjust X
            if (x < 0)
            {
                blockWidth += x;
                x = 0;
            }

            // Adjust Y
            if (y < 0)
            {
                blockHeight += y;
                y = 0;
            }

            // Adjust Width
            if ((x + blockWidth) > width)
            {
                blockWidth -= ((x + blockWidth) - width);
            }

            // Adjust Height
            if ((y + blockHeight) > height)
            {
                blockHeight -= ((y + blockHeight) - height);
            }

            return (blockWidth > 0 && blockHeight > 0);

        }
        
        public static void Resize(ref PixelData pixelData, int width, int height)
        {
            pixelData.Width = MathHelper.Clamp(width, 1, 2048);
            pixelData.Height = MathHelper.Clamp(height, 1, 2048);
            pixelData.TotalPixels = pixelData.Width * pixelData.Height;

            Array.Resize(ref pixelData.Pixels, pixelData.TotalPixels);

            Clear(pixelData);
        }
    }
    
}