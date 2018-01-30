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
        protected int[] pixels;
        public int width;
        public int height;
        
        public Pattern(int width, int height)
        {
            this.width = width;
            this.height = height;
            
            Resize(this.width, this.height);
            
        }
        
        /// <summary>
        ///     Returns a single pixel. If x or y is out of bounds it will wrap.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public virtual int GetPixel(int x, int y)
        {

            x = x % width;
            y = y % height;

            var index = x + width * y;
            
            return pixels[index];
            
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
            
            x = x % width;
            y = y % height;
            
            var index = x + width * y;
            
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
        
        /// <summary>
        ///     Attemps to replace the current pixels with new pixels.
        /// </summary>
        /// <param name="pixels"></param>
        public virtual void SetPixels(int[] pixels)
        {
            var total = Math.Min(pixels.Length, width * height);
            
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
            var total = blockWidth * blockHeight;

            for (var i = 0; i < total; i++)
            {
                SetPixel((i % blockWidth) + x, (i / blockWidth) + y, pixels[i]);                
            }
            
        }
        
        /// <summary>
        ///     
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public virtual void Resize(int width, int height)
        {
            this.width = width;
            this.height = height;
            
            pixels = new int[width * height];
            
            Clear();
        }
        
        /// <summary>
        ///     Clears all of the pixels with the supplied value or -1.
        /// </summary>
        /// <param name="value"></param>
        public virtual void Clear(int value = -1)
        {
            var total = width * height;
            
            for (int i = 0; i < total; i++)
            {
                pixels[i] = -1;
            }
            
            Invalidate();
        }
        
    }
}