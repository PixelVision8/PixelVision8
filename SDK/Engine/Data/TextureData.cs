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
    
    public struct PixelData
    {
        public int[] Pixels;
        public int Width;
        public int Height;
        public int TotalPixels;

        public PixelData(int width, int height)
        {
            Width = width;
            Height = height;
            TotalPixels = Width * Height;
            Pixels = new int[TotalPixels];
        }
    }
    
    public static class PixelDataUtil
    {
        public static int GetPixelStatic(PixelData pixelData, int x, int y)
        {
            var size = pixelData.Height;
            y = (y % size + size) % size;
            size = pixelData.Width;
            x = (x % size + size) % size;
            // size is still == _width from the previous operation - let's reuse the local

            return pixelData.Pixels[x + size * y];
        }
        
        public static void SetPixelStatic(PixelData pixelData, int x, int y, int color)
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
        
        public static int[] GetPixelsStatic(PixelData pixelData)
        {
            var tmpPixels = new int[pixelData.Pixels.Length];

            Array.Copy(pixelData.Pixels, tmpPixels, pixelData.Pixels.Length);

            return tmpPixels;
        }
        
        public static int[] GetPixelsStatic(PixelData pixelData, int x, int y, int blockWidth, int blockHeight)
        {
            var tmpPixels = new int[blockWidth * blockHeight];
        
            CopyPixelsStatic(ref tmpPixels, pixelData.Pixels, pixelData.Width, pixelData.Height, x, y, blockWidth, blockHeight);
        
            //            Array.Copy(pixels, tmpPixels, pixels.Length);
        
            return tmpPixels;
        }
        
        public static void SetPixelsStatic(int[] pixels, PixelData pixelData)
        {
            var TotalPixels = Math.Min(pixels.Length, pixelData.Width * pixelData.Height);

            Array.Copy(pixels, pixelData.Pixels, TotalPixels);

            
        }
        
        public static void SetPixelsStatic(PixelData pixelData, int x, int y, int blockWidth, int blockHeight, int[] pixels)
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
        
        public static void CopyPixelsStatic(ref int[] data, int[] Pixels, int Width, int Height, int x, int y, int blockWidth, int blockHeight)
        {
            var TotalPixels = blockWidth * blockHeight;

            if (data.Length < TotalPixels) Array.Resize(ref data, TotalPixels);

            // Per-line copy, as there is no special per-pixel logic required.

            // Vertical wrapping is not an issue. Horizontal wrapping requires splitting the copy into two operations.
            // Keep important data in local variables.
            int srcY;
            var src = Pixels;
            var width = Width;
            var height = Height;
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
        
        public static void CopyPixelsStatic(int[] Pixels, int Width, int Height, ref int[] data, bool ignoreTransparent, int transparentColor)
        {
            var TotalPixels = Width * Height;

            if (data.Length < TotalPixels) Array.Resize(ref data, TotalPixels);

            int color;

            if (!ignoreTransparent)
                Array.Copy(Pixels, data, TotalPixels);
            else
                for (var i = 0; i < TotalPixels; i++)
                {
                    color = Pixels[i];
                    if (color != transparentColor) data[i] = color;
                }
        }
        
        public static void ClearStatic(PixelData pixelData,  int colorRef = -1, int x = 0, int y = 0, int? width = null, int? height = null)
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

                SetPixelsStatic(pixelData, x, y, tmpWidth, tmpHeight, tmpPixels);
            }
        }
        
        public static void MergePixelsStatic(PixelData pixelData, int x, int y, int blockWidth, int blockHeight, int[] pixels, bool flipH, bool flipV,
            int colorOffset, bool ignoreTransparent)
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

                    SetPixelStatic(pixelData, srcX + x, srcY + y, pixel);
                }
            }
        }
        
        public static void CropStatic(PixelData pixelData, int x, int y, int blockWidth, int blockHeight)
        {
            if (!ValidateBounds(pixelData.Width, pixelData.Height, ref x, ref y, ref blockWidth, ref blockHeight))
                return;

            var tmpPixelData = GetPixelsStatic(pixelData, x, y, blockWidth, blockHeight);

            pixelData.Width = blockWidth;
            pixelData.Height = blockHeight;

            Array.Resize(ref pixelData.Pixels, pixelData.Width * pixelData.Height);

            SetPixelsStatic(tmpPixelData, pixelData);
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
        
        public static void ResizeStatic(ref PixelData pixelData, int width, int height)
        {
            pixelData.Width = MathHelper.Clamp(width, 1, 2048);
            pixelData.Height = MathHelper.Clamp(height, 1, 2048);
            pixelData.TotalPixels = pixelData.Width * pixelData.Height;

            Array.Resize(ref pixelData.Pixels, pixelData.TotalPixels);

            ClearStatic(pixelData);
        }
    }
    
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
        protected PixelData pixelData = new PixelData(256, 256);

        protected int _height
        {
            get => pixelData.Height;
            set => pixelData.Height = value;
        }

        protected int _width
        {
            get => pixelData.Width;
            set => pixelData.Width = value;
        }
        
        // protected int _height;

        // Those are accessed internally very often,
        // and field accesses (ldfld, stfld) are much faster than
        // property accesses (call / callvirt get_ / set_)
        // protected int _width;
        public int[] Pixels => pixelData.Pixels;

        // private int[] tmpPixels;

        protected int TotalPixels;


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
            return PixelDataUtil.GetPixelStatic(pixelData, x, y);

            // // Note: + size and the second modulo operation are required to get wrapped values between 0 and +size
            // var size = _height;
            // y = (y % size + size) % size;
            // size = _width;
            // x = (x % size + size) % size;
            // // size is still == _width from the previous operation - let's reuse the local
            //
            // return Pixels[x + size * y];
        }

        /// <summary>
        ///     This will set a single pixel. If x or y is out of bounds it will wrap.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="color"></param>
        public virtual void SetPixel(int x, int y, int color)
        {
            // // Note: + size and the second modulo operation are required to get wrapped values between 0 and +size
            // var size = _height;
            // y = (y % size + size) % size;
            // size = _width;
            // x = (x % size + size) % size;
            // // size is still == _width from the previous operation - let's reuse the local
            //
            // var index = x + size * y;
            //
            // Pixels[index] = color;
            //
            // Invalidate();
            PixelDataUtil.SetPixelStatic(pixelData, x, y , color);

            Invalidate();
        }

        /// <summary>
        ///     Return a copy of the pixel data.
        /// </summary>
        /// <returns></returns>
        public virtual int[] GetPixels()
        {
            // var tmpPixels = new int[Pixels.Length];
            //
            // Array.Copy(Pixels, tmpPixels, Pixels.Length);
            //
            // return tmpPixels;
            return PixelDataUtil.GetPixelsStatic(pixelData);

        }

        public virtual int[] GetPixels(int x, int y, int blockWidth, int blockHeight)
        {
            return PixelDataUtil.GetPixelsStatic(pixelData, x, y, blockWidth, blockHeight);
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
            // TotalPixels = Math.Min(pixels.Length, _width * _height);
            //
            // Array.Copy(pixels, this.Pixels, TotalPixels);
            PixelDataUtil.SetPixelsStatic(pixels, pixelData);

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
            PixelDataUtil.SetPixelsStatic(pixelData, x, y, blockWidth, blockHeight, pixels);

            // TotalPixels = blockWidth * blockHeight;
            //
            // if (TotalPixels == 0) return;
            //
            // // Per-line copy, as there is no special per-pixel logic required.
            //
            // // Vertical wrapping is not an issue. Horizontal wrapping requires splitting the copy into two operations.
            // // Keep important data in local variables.
            // int dstY;
            // var dst = this.Pixels;
            // var width = _width;
            // var height = _height;
            // var offsetStart = (x % width + width) % width;
            // var offsetEnd = offsetStart + blockWidth;
            // if (offsetEnd <= width)
            // {
            //     // Copy each entire line at once.
            //     for (var tmpY = blockHeight - 1; tmpY > -1; --tmpY)
            //     {
            //         // Note: + size and the second modulo operation are required to get wrapped values between 0 and +size
            //         dstY = ((y + tmpY) % height + height) % height;
            //         Array.Copy(pixels, tmpY * blockWidth, dst, offsetStart + dstY * width, blockWidth);
            //     }
            // }
            // else
            // {
            //     // Copy each non-wrapping section and each wrapped section separately.
            //     var wrap = offsetEnd % width;
            //     for (var tmpY = blockHeight - 1; tmpY > -1; --tmpY)
            //     {
            //         // Note: + size and the second modulo operation are required to get wrapped values between 0 and +size
            //         dstY = ((y + tmpY) % height + height) % height;
            //         Array.Copy(pixels, tmpY * blockWidth, dst, offsetStart + dstY * width, blockWidth - wrap);
            //         Array.Copy(pixels, blockWidth - wrap + tmpY * blockWidth, dst, dstY * width, wrap);
            //     }
            // }
        }

        /// <summary>
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public virtual void Resize(int width, int height)
        {
            PixelDataUtil.ResizeStatic(ref pixelData, width, height);

            // _width = MathHelper.Clamp(width, 1, 2048);
            // _height = MathHelper.Clamp(height, 1, 2048);
            //
            //
            // Array.Resize(ref Pixels, width * height);
            //
            // Clear();
        }

        public virtual void Crop(int x, int y, int blockWidth, int blockHeight)
        {
            PixelDataUtil.CropStatic(pixelData, x, y, blockWidth, blockHeight);


            // if (!ValidateBounds(ref x, ref y, ref blockWidth, ref blockHeight))
            //     return;
            //
            // var tmpPixelData = GetPixels(x, y, blockWidth, blockHeight);
            //
            // _width = blockWidth;
            // _height = blockHeight;
            //
            // Array.Resize(ref Pixels, _width * _height);
            //
            // SetPixels(tmpPixelData);
        }

        // protected bool ValidateBounds(ref int x, ref int y, ref int blockWidth, ref int blockHeight)
        // {
        //     // Adjust X
        //     if (x < 0)
        //     {
        //         blockWidth += x;
        //         x = 0;
        //     }
        //
        //     // Adjust Y
        //     if (y < 0)
        //     {
        //         blockHeight += y;
        //         y = 0;
        //     }
        //
        //     // Adjust Width
        //     if ((x + blockWidth) > _width)
        //     {
        //         blockWidth -= ((x + blockWidth) - _width);
        //     }
        //
        //     // Adjust Height
        //     if ((y + blockHeight) > _height)
        //     {
        //         blockHeight -= ((y + blockHeight) - _height);
        //     }
        //
        //     return (blockWidth > 0 && blockHeight > 0);
        //
        // }

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
            PixelDataUtil.ClearStatic(pixelData, colorRef, x, y, width, height);

            // int[] tmpPixels = null;
            //
            // var tmpWidth = width ?? this.width;
            // var tmpHeight = height ?? this.height;
            //
            // var total = tmpWidth * tmpHeight;
            //
            // // TODO not sure why this sometimes goes to negative but this check should fix that
            // if (total > 0)
            // {
            //     tmpPixels = new int[total];
            //
            //     for (var i = 0; i < total; i++) tmpPixels[i] = colorRef;
            //
            //     SetPixels(x, y, tmpWidth, tmpHeight, tmpPixels);

                Invalidate();
            // }
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
            // TotalPixels = blockWidth * blockHeight;
            //
            // // Per-pixel copy.
            // int pixel;
            // int srcX, srcY;
            // for (var i = TotalPixels - 1; i > -1; i--)
            // {
            //     pixel = pixels?[i] ?? -1;
            //
            //     if (pixel != -1 || ignoreTransparent != true)
            //     {
            //         if (colorOffset > 0 && pixel != -1) pixel += colorOffset;
            //
            //         srcX = i % blockWidth;
            //         srcY = i / blockWidth;
            //
            //         if (flipH) srcX = blockWidth - 1 - srcX;
            //
            //         if (flipV) srcY = blockWidth - 1 - srcY;
            //
            //         SetPixel(srcX + x, srcY + y, pixel);
            //     }
            //
            // }

            PixelDataUtil.MergePixelsStatic(pixelData, x, y, blockWidth, blockHeight, pixels, flipH, flipV, colorOffset, ignoreTransparent);

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
            PixelDataUtil.CopyPixelsStatic(Pixels, _width, _height, ref data, ignoreTransparent, transparentColor);

            // TotalPixels = _width * _height;
            //
            // if (data.Length < TotalPixels) Array.Resize(ref data, TotalPixels);
            //
            // int color;
            //
            // if (!ignoreTransparent)
            //     Array.Copy(Pixels, data, TotalPixels);
            // else
            //     for (var i = 0; i < TotalPixels; i++)
            //     {
            //         color = Pixels[i];
            //         if (color != transparentColor) data[i] = color;
            //     }
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
            PixelDataUtil.CopyPixelsStatic(ref data, Pixels, _width, _height, x, y, blockWidth, blockHeight);

            // TotalPixels = blockWidth * blockHeight;
            //
            // if (data.Length < TotalPixels) Array.Resize(ref data, TotalPixels);
            //
            // // Per-line copy, as there is no special per-pixel logic required.
            //
            // // Vertical wrapping is not an issue. Horizontal wrapping requires splitting the copy into two operations.
            // // Keep important data in local variables.
            // int srcY;
            // var src = Pixels;
            // var width = _width;
            // var height = _height;
            // var offsetStart = (x % width + width) % width;
            // var offsetEnd = offsetStart + blockWidth;
            // if (offsetEnd <= width)
            // {
            //     // Copy each entire line at once.
            //     for (var tmpY = blockHeight - 1; tmpY > -1; --tmpY)
            //     {
            //         // Note: + size and the second modulo operation are required to get wrapped values between 0 and +size
            //         srcY = ((y + tmpY) % height + height) % height;
            //         Array.Copy(src, offsetStart + srcY * width, data, tmpY * blockWidth, blockWidth);
            //     }
            // }
            // else
            // {
            //     // Copy each non-wrapping section and each wrapped section separately.
            //     var wrap = offsetEnd % width;
            //     for (var tmpY = blockHeight - 1; tmpY > -1; --tmpY)
            //     {
            //         // Note: + size and the second modulo operation are required to get wrapped values between 0 and +size
            //         srcY = ((y + tmpY) % height + height) % height;
            //         Array.Copy(src, offsetStart + srcY * width, data, tmpY * blockWidth, blockWidth - wrap);
            //         Array.Copy(src, srcY * width, data, blockWidth - wrap + tmpY * blockWidth, wrap);
            //     }
            // }
        }
    }
}