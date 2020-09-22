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

namespace PixelVision8.Engine
{
    /// <summary>
    ///     <see cref="TextureData" /> represent a grid of pixel data in the engine.
    ///     Pixel data aren't values that can be used to
    ///     reference colors in the ColorChip when rendering to a display. The
    ///     <see cref="TextureData" /> class provides a set of APIs to make it easier
    ///     to work with this data. It also allows you to perform more advanced
    ///     operations around getting and setti ng pixel data including support for
    ///     wrapping.
    /// </summary>
    public class TextureData : AbstractData
    {
        protected int _height;

        // Those are accessed internally very often,
        // and field accesses (ldfld, stfld) are much faster than
        // property accesses (call / callvirt get_ / set_)
        protected int _width;
        public int[] pixels = new int[0];

        // private int[] tmpPixels;

        protected int total;


        /// <summary>
        ///     The constructor for a new TextureData class. It requires new
        ///     dimensions and an optional value for changing the wrap mode.
        /// </summary>
        /// <param name="width">
        ///     An int for the width of the TextureData.
        /// </param>
        /// <param name="height">
        ///     An int for the height of the TextureData.
        /// </param>
        public TextureData(int width = 1, int height = 1)
        {
            _width = width;
            _height = height;

            Resize(width, height);
        }

        /// <summary>
        ///     The <see cref="width" /> of the Pattern.
        /// </summary>
        public int width => _width;

        /// <summary>
        ///     The <see cref="height" /> of the Pattern.
        /// </summary>
        public int height => _height;

        /// <summary>
        ///     Returns a single pixel. If x or y is out of bounds it will wrap.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public virtual int GetPixel(int x, int y)
        {
            // Note: + size and the second modulo operation are required to get wrapped values between 0 and +size
            var size = _height;
            y = (y % size + size) % size;
            size = _width;
            x = (x % size + size) % size;
            // size is still == _width from the previous operation - let's reuse the local

            return pixels[x + size * y];
        }

        /// <summary>
        ///     This will set a single pixel. If x or y is out of bounds it will wrap.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="color"></param>
        public virtual void SetPixel(int x, int y, int color)
        {
            // Note: + size and the second modulo operation are required to get wrapped values between 0 and +size
            var size = _height;
            y = (y % size + size) % size;
            size = _width;
            x = (x % size + size) % size;
            // size is still == _width from the previous operation - let's reuse the local

            var index = x + size * y;

            pixels[index] = color;

            Invalidate();
        }

        /// <summary>
        ///     Return a copy of the pixel data.
        /// </summary>
        /// <returns></returns>
        public virtual int[] GetPixels()
        {
            var tmpPixels = new int[pixels.Length];

            Array.Copy(pixels, tmpPixels, pixels.Length);

            return tmpPixels;
        }

        public virtual int[] GetPixels(int x, int y, int blockWidth, int blockHeight)
        {
            var tmpPixels = new int[blockWidth * blockHeight];

            CopyPixels(ref tmpPixels, x, y, blockWidth, blockHeight);

            //            Array.Copy(pixels, tmpPixels, pixels.Length);

            return tmpPixels;
        }


        /// <summary>
        ///     This replaces all the pixels in the TextureData with the supplied
        ///     values.
        /// </summary>
        /// <param name="pixels">
        ///     Anint array of pixel data values.
        /// </param>
        public virtual void SetPixels(int[] pixels)
        {
            total = Math.Min(pixels.Length, _width * _height);

            Array.Copy(pixels, this.pixels, total);

            Invalidate();
        }

        /// <summary>
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="blockWidth"></param>
        /// <param name="blockHeight"></param>
        /// <param name="pixels"></param>
        public virtual void SetPixels(int x, int y, int blockWidth, int blockHeight, int[] pixels)
        {
            total = blockWidth * blockHeight;

            if (total == 0) return;

            // Per-line copy, as there is no special per-pixel logic required.

            // Vertical wrapping is not an issue. Horizontal wrapping requires splitting the copy into two operations.
            // Keep important data in local variables.
            int dstY;
            var dst = this.pixels;
            var width = _width;
            var height = _height;
            var offsetStart = (x % width + width) % width;
            var offsetEnd = offsetStart + blockWidth;
            if (offsetEnd <= width)
            {
                // Copy each entire line at once.
                for (var tmpY = blockHeight - 1; tmpY > -1; --tmpY)
                {
                    // Note: + size and the second modulo operation are required to get wrapped values between 0 and +size
                    dstY = ((y + tmpY) % height + height) % height;
                    Array.Copy(pixels, tmpY * blockWidth, dst, offsetStart + dstY * width, blockWidth);
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
                    Array.Copy(pixels, tmpY * blockWidth, dst, offsetStart + dstY * width, blockWidth - wrap);
                    Array.Copy(pixels, blockWidth - wrap + tmpY * blockWidth, dst, dstY * width, wrap);
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public virtual void Resize(int width, int height)
        {
            _width = MathHelper.Clamp(width, 1, 2048);
            _height = MathHelper.Clamp(height, 1, 2048);
            

            Array.Resize(ref pixels, width * height);

            Clear();
        }

        public virtual void Crop(int x, int y, int blockWidth, int blockHeight)
        {

            if (!ValidateBounds(ref x, ref y, ref blockWidth, ref blockHeight))
                return;

            var tmpPixelData = GetPixels(x, y, blockWidth, blockHeight);

            _width = blockWidth;
            _height = blockHeight;

            Array.Resize(ref pixels, _width * _height);

            SetPixels(tmpPixelData);
        }

        protected bool ValidateBounds(ref int x, ref int y, ref int blockWidth, ref int blockHeight)
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
            if ((x + blockWidth) > _width)
            {
                blockWidth -= ((x + blockWidth) - _width);
            }

            // Adjust Height
            if ((y + blockHeight) > _height)
            {
                blockHeight -= ((y + blockHeight) - _height);
            }

            return (blockWidth > 0 && blockHeight > 0);

        }

        /// <summary>
        ///     Clears the pixel data. The default empty value is -1 since the
        ///     ColorChip starts at 0. You can also use the Clear() method to
        ///     replace all the color in the TextureData at once.
        /// </summary>
        /// <param name="colorRef">
        ///     Optional clear value. This is set to -1 by default.
        /// </param>
        public virtual void Clear(int colorRef = -1, int x = 0, int y = 0, int? width = null, int? height = null)
        {
            int[] tmpPixels = null;

            var tmpWidth = width ?? this.width;
            var tmpHeight = height ?? this.height;

            var total = tmpWidth * tmpHeight;

            // TODO not sure why this sometimes goes to negative but this check should fix that
            if (total > 0)
            {
                tmpPixels = new int[total];

                for (var i = 0; i < total; i++) tmpPixels[i] = colorRef;

                SetPixels(x, y, tmpWidth, tmpHeight, tmpPixels);

                Invalidate();
            }
        }

        /// <summary>
        ///     This replaces all the pixels in a specific area of the TextureData.
        /// </summary>
        /// <param name="x">
        ///     The x position to start at. 0 is the left of the texture.
        /// </param>
        /// <param name="y">
        ///     The y position to start at. 0 is the top of the texture.
        /// </param>
        /// <param name="blockWidth">
        ///     The <see cref="width" /> of the area to replace.
        /// </param>
        /// <param name="blockHeight">
        ///     The <see cref="height" /> of the area to replace.
        /// </param>
        /// <param name="pixels">The pixel data to be used.</param>
        /// <param name="flipH">
        ///     This is an optional argument which accepts a bool. The default value is set to false but passing in true flips
        ///     the pixel data horizontally.
        /// </param>
        /// <param name="flipV">
        ///     This is an optional argument which accepts a bool. The default value is set to false but passing in true flips
        ///     the pixel data vertically.
        /// </param>
        /// <param name="colorOffset"></param>
        public virtual void MergePixels(int x, int y, int blockWidth, int blockHeight, int[] pixels,
            bool flipH = false, bool flipV = false, int colorOffset = 0, bool ignoreTransparent = true)
        {
            total = blockWidth * blockHeight;

            // Per-pixel copy.
            int pixel;
            int srcX, srcY;
            for (var i = total - 1; i > -1; i--)
            {
                pixel = pixels?[i] ?? -1;

                if (pixel != -1 || ignoreTransparent != true)
                {
                    if (colorOffset > 0 && pixel != -1) pixel += colorOffset;

                    srcX = i % blockWidth;
                    srcY = i / blockWidth;

                    if (flipH) srcX = blockWidth - 1 - srcX;

                    if (flipV) srcY = blockWidth - 1 - srcY;

                    SetPixel(srcX + x, srcY + y, pixel);
                }

            }

            Invalidate();
        }


        /// <summary>
        ///     A fast method for getting a copy of the texture's pixel data.
        /// </summary>
        /// <param name="data">
        ///     Supply an int array to get a copy of the pixel
        ///     data.
        /// </param>
        public void CopyPixels(ref int[] data, bool ignoreTransparent = false, int transparentColor = -1)
        {
            total = _width * _height;

            if (data.Length < total) Array.Resize(ref data, total);

            int color;

            if (!ignoreTransparent)
                Array.Copy(pixels, data, total);
            else
                for (var i = 0; i < total; i++)
                {
                    color = pixels[i];
                    if (color != transparentColor) data[i] = color;
                }
        }

        /// <summary>
        ///     Returns a set of pixel <paramref name="data" /> from a specific
        ///     position and size. Supply anint array to get a
        ///     copy of the pixel <paramref name="data" /> back
        /// </summary>
        /// <param name="data">
        ///     An int array where pixel data will be copied to.
        /// </param>
        /// <param name="x">
        ///     The x position to start the copy at. 0 is the left of the texture.
        /// </param>
        /// <param name="y">
        ///     The y position to start the copy at. 0 is the top of the texture.
        /// </param>
        /// <param name="blockWidth">
        ///     The <see cref="width" /> of the <paramref name="data" /> to be copied.
        /// </param>
        /// <param name="blockHeight">
        ///     The <see cref="height" /> of the <paramref name="data" /> to be
        ///     copied.
        /// </param>
        public void CopyPixels(ref int[] data, int x, int y, int blockWidth, int blockHeight)
        {
            total = blockWidth * blockHeight;

            if (data.Length < total) Array.Resize(ref data, total);

            // Per-line copy, as there is no special per-pixel logic required.

            // Vertical wrapping is not an issue. Horizontal wrapping requires splitting the copy into two operations.
            // Keep important data in local variables.
            int srcY;
            var src = pixels;
            var width = _width;
            var height = _height;
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
    }
}