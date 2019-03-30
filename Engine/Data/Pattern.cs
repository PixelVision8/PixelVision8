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

namespace PixelVisionSDK
{
    public class Pattern : AbstractData
    {
        public int[] pixels = new int[0];

        // Those are accessed internally very often,
        // and field accesses (ldfld, stfld) are much faster than
        // property accesses (call / callvirt get_ / set_)
        protected int _width;
        protected int _height;

        /// <summary>
        ///     The <see cref="width" /> of the Pattern.
        /// </summary>
        public int width => _width;

        /// <summary>
        ///     The <see cref="height" /> of the Pattern.
        /// </summary>
        public int height => _height;

        
        public Pattern(int width = 1, int height = 1)
        {
            this._width = width;
            this._height = height;
            
            Resize(width, height);
            
        }
        
        /// <summary>
        ///     Returns a single pixel. If x or y is out of bounds it will wrap.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public virtual int GetPixel(int x, int y)
        {

            // Wrap x,y values so they don't go out of bounds
//            x = x % width;
//            y = y % height;
//
//            return pixels[x + width * y];

            // Note: + size and the second modulo operation are required to get wrapped values between 0 and +size
            int size = _height;
            y = ((y % size) + size) % size;
            size = _width;
            x = ((x % size) + size) % size;
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
//            if (color == -1)
//                return;

            // Note: + size and the second modulo operation are required to get wrapped values between 0 and +size
            int size = _height;
            y = ((y % size) + size) % size;
            size = _width;
            x = ((x % size) + size) % size;
            // size is still == _width from the previous operation - let's reuse the local

            var index = x + size * y;
            
            // TODO this should never be out of range
//            if(index > -1 && index < pixels.Length)
            pixels[index] = color;
//            else
//                Debug.Log("index "+ index + " "+x+ "," + y);
            
            Invalidate();
        }

        private int[] tmpPixels;
        
        /// <summary>
        ///     Return a copy of the pixel data.
        /// </summary>
        /// <returns></returns>
        public virtual int[] GetPixels()
        {
            tmpPixels = new int[pixels.Length];

            Array.Copy(pixels, tmpPixels, pixels.Length);
            
            return tmpPixels;
        }

        protected int total;
        
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
        ///     
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="blockWidth"></param>
        /// <param name="blockHeight"></param>
        /// <param name="pixels"></param>
        public virtual void SetPixels(int x, int y, int blockWidth, int blockHeight, int[] pixels)
        {
            total = blockWidth * blockHeight;

            if (total == 0)
                return;
            
            // Per-line copy, as there is no special per-pixel logic required.

            // Vertical wrapping is not an issue. Horizontal wrapping requires splitting the copy into two operations.
            // Keep important data in local variables.
            int dstY;
            int[] dst = this.pixels;
            int width = _width;
            int height = _height;
            int offsetStart = ((x % width) + width) % width;
            int offsetEnd = offsetStart + blockWidth;
            if (offsetEnd <= width)
            {
                // Copy each entire line at once.
                for (var tmpY = blockHeight - 1; tmpY > -1; --tmpY)
                {
                    // Note: + size and the second modulo operation are required to get wrapped values between 0 and +size
                    dstY = (((y + tmpY) % height) + height) % height;
                    Array.Copy(pixels, tmpY * blockWidth, dst, offsetStart + dstY * width, blockWidth);
                }
            }
            else
            {
                // Copy each non-wrapping section and each wrapped section separately.
                int wrap = offsetEnd % width;
                for (var tmpY = blockHeight - 1; tmpY > -1; --tmpY)
                {
                    // Note: + size and the second modulo operation are required to get wrapped values between 0 and +size
                    dstY = (((y + tmpY) % height) + height) % height;
                    Array.Copy(pixels, tmpY * blockWidth, dst, offsetStart + dstY * width, blockWidth - wrap);
                    Array.Copy(pixels, (blockWidth - wrap) + tmpY * blockWidth, dst, dstY * width, wrap);
                }
            }

        }
        
        /// <summary>
        ///     
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public virtual void Resize(int width, int height)
        {
            this._width = width;
            this._height = height;
            
            Array.Resize(ref pixels, width * height);
            
            Clear();
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
            
//            if (colorRef == -1)
//            {
                
                var total = tmpWidth * tmpHeight;

                tmpPixels = new int[total];
                
                for (int i = 0; i < total; i++)
                {
                    tmpPixels[i] = colorRef;
                }
                 
//            }

            
                SetPixels(x, y, tmpWidth, tmpHeight, tmpPixels);

//            
//            MergePixels(x, y, width ?? this.width, height ?? this.height, tmpPixels,
//                false, false, 0, false);
            
            
//            total = pixels.Length;
//            
//            for (int i = total - 1; i > -1; i--)
//            {
//                pixels[i] = colorRef;
//            }
            
            Invalidate();
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
        /// <param name="colorOffset"></param>
        /// <param name="ignoreTransparent"></param>
//        public void MergePixels(int x, int y, int blockWidth, int blockHeight, int[] pixels,
//            int colorOffset = 0, bool ignoreTransparent = true)
//        {
//            MergePixels(x, y, blockWidth, blockHeight, pixels, false, false, colorOffset, ignoreTransparent);
//        }

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
                    if (colorOffset > 0 && pixel != -1)
                        pixel += colorOffset;

                    srcX = i % blockWidth;
                    srcY = i / blockWidth;

                    if (flipH)
                        srcX = blockWidth - 1 - srcX;
                    if (flipV)
                        srcY = blockWidth - 1 - srcY;

                    SetPixel(srcX + x, srcY + y, pixel);
                }
                

//                
//                if (pixel != -1 && ignoreTransparent == false)
//                {
//                    
//                }
            }
            
            Invalidate();

        }
        
    }
}