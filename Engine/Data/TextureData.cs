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
using System.Diagnostics;
using PixelVisionSDK.Utils;

namespace PixelVisionSDK
{
    /// <summary>
    ///     <see cref="TextureData" /> represent a grid of pixel data in the engine.
    ///     Pixel data aren't values that can be used to
    ///     reference colors in the ColorChip when rendering to a display. The
    ///     <see cref="TextureData" /> class provides a set of APIs to make it easier
    ///     to work with this data. It also allows you to perform more advanced
    ///     operations around getting and setting pixel data including support for
    ///     wrapping.
    /// </summary>
    public class TextureData : AbstractData
    {
        protected Rect oRect = new Rect();
        protected int[] pixels = new int[0];
        protected Rect sRect = new Rect();
        protected int tmpColumn;
        protected int tmpMaxColumns;
        protected int tmpRow;
        protected int tmpTotal;
        protected int tmpX;
        protected int tmpY;
        protected Rect tRect = new Rect();

        /// <summary>
        ///     A flag to set whether the pixel data should wrap if trying to sample
        ///     larger than the texture's <see cref="width" /> and height. Set to
        ///     true by default.
        /// </summary>
        public bool wrapMode = true;

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
        /// <param name="wrapMode">
        ///     An optional value to support texture wrapping. It's set to
        ///     true by default.
        /// </param>
        public TextureData(int width, int height, bool wrapMode = true)
        {
            this.wrapMode = wrapMode;
            Resize(width, height);
        }

        /// <summary>
        ///     The <see cref="width" /> of the TextureData.
        /// </summary>
        public int width { get; private set; }

        /// <summary>
        ///     The <see cref="height" /> of the TextureData.
        /// </summary>
        public int height { get; private set; }

        /// <summary>
        ///     Returns a specific pixel at a given x,y position inside of the
        ///     TextureData.
        /// </summary>
        /// <param name="x">
        ///     An int for the x position. 0 is the left side of
        ///     the texture.
        /// </param>
        /// <param name="y">
        ///     An int for the y position. 0 is the top of the
        ///     texture.
        /// </param>
        /// <returns>
        ///     Returns an int which can be used to match up to a
        ///     color in the ColorChip.
        /// </returns>
        public int GetPixel(int x, int y)
        {
            if (wrapMode)
            {
                
                if(x < 0 || x >= width)
                    x = MathUtil.Repeat(x, width-1);

                if (y < 0 || y >= height)
                    y = MathUtil.Repeat(y, height-1);
            }

            var index = x + width * y;

            return pixels[index];
        }
        
        /// <summary>
        ///     A fast method for getting a copy of the texture's pixel data.
        /// </summary>
        /// <param name="data">
        ///     Supply an int array to get a copy of the pixel
        ///     data.
        /// </param>
        public void CopyPixels(ref int[] data)
        {
            var total = width * height;

            if (data.Length != total)
                Array.Resize(ref data, total);

            for (var i = 0; i < total; i++)
                data[i] = pixels[i];
        }

        /// <summary>
        ///     Returns a set of pixel <paramref name="data" /> from a specific
        ///     position and size. Supply anint array to get a
        ///     copy of the pixel <paramref name="data" /> back
        /// </summary>
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
        /// <param name="data">
        ///     An int array where pixel data will be copied to.
        /// </param>
        public void GetPixels(int x, int y, int blockWidth, int blockHeight, ref int[] data)
        {
            tmpTotal = blockWidth * blockHeight;

            if (data.Length != tmpTotal)
            {
                Array.Resize(ref data, tmpTotal);
            }

            if (!wrapMode)
                if (x + blockWidth > width || y + blockHeight > height)
                {
                    sRect.x = x;
                    sRect.y = y;
                    sRect.width = blockWidth;
                    sRect.height = blockHeight;

                    tRect.Intersect(sRect, ref oRect);

                    x = oRect.x;
                    y = oRect.y;
                    blockWidth = oRect.width;
                }
            
            for (var i = 0; i < tmpTotal; i++)
            {
                
                //PosUtil.CalculatePosition(i, blockWidth, out tmpX, out tmpY);
                tmpX = i % blockWidth;
                tmpY = i / blockWidth;

                var color = GetPixel(tmpX + x, tmpY + y);
                //if(color != -1)
                    data[i] = color;
            }
        }

        /// <summary>
        ///     Sets a pixel data <paramref name="value" /> in the
        ///     <see cref="pixels" /> array at a specific x,y position.
        /// </summary>
        /// <param name="x">
        ///     The x position to set the value. 0 is the left of the texture.
        /// </param>
        /// <param name="y">
        ///     The y position to set the value. 0 is the top of the texture.
        /// </param>
        /// <param name="value">
        ///     The int value that corresponds to a index in the
        ///     ColorChip.
        /// </param>
        public virtual void SetPixel(int x, int y, int value)
        {
            //TODO removed some code from here to check x and y based on wrap mode. Make sure I didn't break anything
            if (wrapMode)
            {
                if (y < 0 || y >= height && wrapMode)
                    y = MathUtil.Repeat(y, height);
            }
            else
            {
                if (x < 0 || x >= width)
                    return;
            }

            var index = (x % width) + (width * y);

            if (index < 0)
                return;

            if (index < pixels.Length)
                pixels[index] = value;
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
            Array.Copy(pixels, this.pixels, pixels.Length);
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
        public virtual void SetPixels(int x, int y, int blockWidth, int blockHeight, int[] pixels, int colorOffset = 0)
        {
            var total = blockWidth * blockHeight;
            int pixel;

            for (var i = 0; i < total; i++)
            {
                //PosUtil.CalculatePosition(i, blockWidth, out tmpX, out tmpY);
                tmpX = i % blockWidth;
                tmpY = i / blockWidth;

                tmpX += x;
                tmpY += y;
                pixel = pixels[i];

                if (colorOffset > 0 && pixel != -1)
                    pixel += colorOffset;

                SetPixel(tmpX, tmpY, pixel);
            }

        }

        /// <summary>
        ///     This resizes the TextureData. Calling this will clear the pixel data
        ///     when resized.
        /// </summary>
        /// <param name="width">The new width of the TextureData.</param>
        /// <param name="height">The new height of the TextureData.</param>
        public void Resize(int width, int height)
        {
            this.width = width;
            this.height = height;
            tRect.width = width;
            tRect.height = height;

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
        public void Clear(int colorRef = -1)
        {
            var total = pixels.Length;
            for (var i = 0; i < total; i++)
                pixels[i] = colorRef;
        }

        /// <summary>
        ///     This method is used to merge pixel data from another TextureData.
        ///     Simply supply the source's pixel data int array
        ///     and flag if the merge should ignore transparency via the mask flag.
        ///     If <paramref name="masked" /> is set to true, the default
        ///     <paramref name="transparent" /> color (which can also be changed)
        ///     will be ignored allowing you to overlay new pixel data on top of
        ///     existing data.
        /// </summary>
        /// <param name="x">
        ///     The x position to start at. 0 is the left of the texture.
        /// </param>
        /// <param name="y">
        ///     The y position to start at. 0 is the top of the texture.
        /// </param>
        /// <param name="blockWidth">
        ///     The <see cref="width" /> of the data to be merged.
        /// </param>
        /// <param name="blockHeight">
        ///     The <see cref="height" /> of the data to be merged.
        /// </param>
        /// <param name="srcPixels">The new pixel data to be merged.</param>
        /// <param name="masked">
        ///     If the data should be masked when being merged.
        /// </param>
        /// <param name="transparent">
        ///     The mask color id. By default this is set to -1.
        /// </param>
        public void MergePixels(int x, int y, int blockWidth, int blockHeight, int[] srcPixels, bool masked = true,
            int transparent = -1)
        {
            // Exit out of a merge if the data doesn't match up
            if (srcPixels.Length != blockWidth * blockHeight)
                return;

            var total = blockWidth * blockHeight;

            for (var i = 0; i < total; i++)
            {
                
                //PosUtil.CalculatePosition(i, blockWidth, out tmpX, out tmpY);
                tmpX = i % blockWidth;
                tmpY = i / blockWidth;


                tmpX += x;
                tmpY += y;

                if (masked)
                {
                    if (srcPixels[i] != transparent)
                        SetPixel(tmpX, tmpY, srcPixels[i]);
                }
                else
                {
                    SetPixel(tmpX, tmpY, srcPixels[i]);
                }

            }
        }
    }
}