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
        private int i;
        
        /// <summary>
        ///     The <see cref="width" /> of the Pattern.
        /// </summary>
        public int width { get; protected set; }

        /// <summary>
        ///     The <see cref="height" /> of the Pattern.
        /// </summary>
        public int height { get; protected set; }

        
        public Pattern(int width = 1, int height = 1)
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
            
            // Wrap x,y values so they don't go out of bounds
//            x = x % width;
//            y = y % height;
//
//            return pixels[x + width * y];
            
            x = (int) (x - Math.Floor(x / (float) width) * width);

            y = (int) (y - Math.Floor(y / (float) height) * height);
            
            return pixels[x + width * y];
            
        }

        private int index;
        
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
            
            x = (int) (x - Math.Floor(x / (float) width) * width);

            y = (int) (y - Math.Floor(y / (float) height) * height);
            
            index = x + width * y;
            
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
            total = Math.Min(pixels.Length, width * height);
            
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

            for (i = 0; i < total; i++)
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
        public virtual void Clear(int colorRef = -1)
        {
            total = pixels.Length;
            
            for (int i = 0; i < total; i++)
            {
                pixels[i] = colorRef;
            }
            
            Invalidate();
        }

        private int pixel;
        
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
        public virtual void MergePixels(int x, int y, int blockWidth, int blockHeight, int[] pixels, int colorOffset = 0, bool ignoreTransparent = false)
        {
            total = blockWidth * blockHeight;
//            int pixel;

            for (i = 0; i < total; i++)
            {
                pixel = pixels[i];

                if (pixel == -1 && ignoreTransparent)
                {

                }
                else
                {
                    if (colorOffset > 0 && pixel != -1)
                        pixel += colorOffset;
                
                    SetPixel((i % blockWidth) + x, (i / blockWidth) + y, pixel);                

                }
                
            }
        }
        
    }
}