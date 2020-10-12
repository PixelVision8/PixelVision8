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

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using PixelVision8.Engine.Utils;

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
        
        public string[] colors;
        public int Columns => width / _spriteSize.X;
        public int Rows => height / _spriteSize.Y;
        public int TotalSprites => Columns * Rows;
    
        protected Point _spriteSize;
        protected List<int> _colorIDs = new List<int>();
        protected int _colorID;
        protected Point _pos;
        protected int[] _pixelData;
        
        /// <summary>
        ///     Get a single sprite from the Image.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cps">Total number of colors supported by the sprite.</param>
        /// <returns></returns>
        public int[] GetSpriteData(int id, int? cps = null)
        {
            _pos = MathUtil.CalculatePosition(id, Columns);

            _pixelData = GetPixels(_pos.X * 8, _pos.Y * 8, _spriteSize.X, _spriteSize.Y);

            // If there is a CPS cap, we need to go through all the pixels and make sure they are in range.
            if (cps.HasValue)
            {

                _colorIDs.Clear();

                for (int i = 0; i < TotalPixels; i++)
                {
                    _colorID = _pixelData[i];

                    if (_colorID > -1 && _colorIDs.IndexOf(_colorID) == -1)
                    {
                        if (_colorIDs.Count < cps.Value)
                        {
                            _colorIDs.Add(_colorID);
                        }
                        else
                        {
                            _pixelData[i] = -1;
                        }
                    
                    }

        
                }
        
            }

            // Return the new sprite image
            return _pixelData;
        }
        
        
        
        
        public PixelData pixelData = new PixelData(256, 256);

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
        public virtual int[] Pixels => pixelData.Pixels;

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
            return PixelDataUtil.GetPixel(pixelData, x, y);

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
            PixelDataUtil.SetPixel(pixelData, x, y , color);

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
            return PixelDataUtil.GetPixels(pixelData);

        }

        public virtual int[] GetPixels(int x, int y, int blockWidth, int blockHeight)
        {
            return PixelDataUtil.GetPixels(pixelData, x, y, blockWidth, blockHeight);
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
            PixelDataUtil.SetPixels(pixels, pixelData);

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
            PixelDataUtil.SetPixels(pixelData, x, y, blockWidth, blockHeight, pixels);

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
            PixelDataUtil.Resize(ref pixelData, width, height);

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
            PixelDataUtil.Crop(pixelData, x, y, blockWidth, blockHeight);


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
            PixelDataUtil.Clear(pixelData, colorRef, x, y, width, height);

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

            PixelDataUtil.MergePixels(pixelData, x, y, blockWidth, blockHeight, pixels, flipH, flipV, colorOffset, ignoreTransparent);

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
            PixelDataUtil.CopyPixels(pixelData, ref data, ignoreTransparent, transparentColor);

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
            PixelDataUtil.CopyPixels(ref data, pixelData, x, y, blockWidth, blockHeight);

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