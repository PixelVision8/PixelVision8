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
            total = _width * _height;

            if (data.Length != total)
                Array.Resize(ref data, total);

            int color;

            if (!ignoreTransparent)
            {
                Array.Copy(pixels, data, total);
            }
            else
            {
                for (var i = 0; i < total; i++)
                {
                    color = pixels[i];
                    if (color != transparentColor)
                        data[i] = color;
                }
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

            if (data.Length != total)
                Array.Resize(ref data, total);

            int srcY;
            for (var tmpY = blockHeight - 1; tmpY > -1; --tmpY)
            {
                // Vertical wrapping is not an issue. Horizontal wrapping requires splitting the copy into two operations.

                // Note: + size and the second modulo operation are required to get wrapped values between 0 and +size
                int size = _height;
                srcY = (((y + tmpY) % size) + size) % size;
                size = _width;
                int offsetStart = ((x % size) + size) % size;
                int offsetEnd = offsetStart + blockWidth;
                if (offsetEnd <= size)
                {
                    // Copy the entire line at once.
                    Array.Copy(pixels, offsetStart + srcY * size, data, tmpY * blockWidth, blockWidth);
                }
                else
                {
                    // Copy the non-wrapping section and the wrapped section separately.
                    int wrap = offsetEnd % size;
                    Array.Copy(pixels, offsetStart + srcY * size, data, tmpY * blockWidth, blockWidth - wrap);
                    Array.Copy(pixels, srcY * size, data, (blockWidth - wrap) + tmpY * blockWidth, wrap);
                }


            }

            /**/
        }

        

    }

}