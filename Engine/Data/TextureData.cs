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
    public class TextureData : Pattern
    {

        protected int tmpTotal;
        protected int tmpX;
        protected int tmpY;

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
        public TextureData(int width, int height) : base(width, height)
        {
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
            var total = width * height;

            if (data.Length != total)
                Array.Resize(ref data, total);

            var color = -1;
                
            for (var i = 0; i < total; i++)
            {
                if (ignoreTransparent && pixels[i] != transparentColor)
                data[i] = pixels[i];
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
            tmpTotal = blockWidth * blockHeight;

            if (data.Length != tmpTotal)
                Array.Resize(ref data, tmpTotal);

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
        public virtual void SetPixels(int x, int y, int blockWidth, int blockHeight, int[] pixels, int colorOffset = 0, bool ignoreTransparent = false)
        {
            var total = blockWidth * blockHeight;
            int pixel;

            for (var i = 0; i < total; i++)
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