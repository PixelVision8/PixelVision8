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
using System.Collections.Generic;

namespace PixelVision8.Player
{
    public static partial class Utilities
    {
        
        public static T[] GetPixels<T>(PixelData<T> pixelData) where T : IComparable
        {
            
            // Create a new temporary array to copy the pixel data into
            var tmpPixels = new T[pixelData.Total];
            
            // Use the Array.Copy() method to quickly copy all of the pixel data into the new temporary array
            Array.Copy(pixelData.Pixels, tmpPixels, pixelData.Total);

            // Return the temporary pixels
            return tmpPixels;
        }

        public static T[] GetPixels<T>(PixelData<T> pixelData, Rectangle rect) where T : IComparable
        {
            return GetPixels(pixelData, rect.X, rect.Y, rect.Width, rect.Height);
        }
        
        public static T[] GetPixels<T>(PixelData<T> pixelData, int x, int y, int blockWidth, int blockHeight) where T : IComparable
        {
            
            // Create a new temporary array to copy the pixel data into
            var tmpPixels = new T[blockWidth * blockHeight];

            // Copy each entire line at once.
            for (var i = blockHeight - 1; i > -1; --i)
            {
                Array.Copy(
                    pixelData.Pixels,
                    x + (y + i) * pixelData.Width,
                    tmpPixels,
                    i * blockWidth,
                    blockWidth
                );
            }
            
            return tmpPixels;
        }

        // public static void SetPixels(int[] srcPixels, PixelData destPixelData) => destPixelData.SetPixels(srcPixels, destPixelData.Width, destPixelData.Height);

        public static void SetPixels<T>(T[] srcPixels, int x, int y, int blockWidth, int blockHeight, PixelData<T> destPixelData) where T : IComparable
        {

            // var srcPWidth = blockWidth;
            var destPWidth = destPixelData.Width;
            var destPHeight = destPixelData.Height;

            var total = blockWidth * blockHeight;
        
            int col = 0, row = 0;

            int tmpX, tmpY, i;

            T tmpPixel;
        
            // TODO shouldn't this use copy instead of looping thorugh all the pixels?

            for (i = 0; i < total; i++)
            {
                
                tmpPixel = srcPixels[col + blockWidth * row];
                
                tmpX = col + x;
    
                tmpY = row + y;
                
                if (tmpX >= 0 && tmpX < destPWidth && tmpY >= 0 && tmpY < destPHeight)
                {
                    destPixelData.Pixels[tmpX + destPWidth * tmpY] = tmpPixel;
                }
                
                col++;

                if (col < blockWidth) continue;
                col = 0;
                row++;
            }
        }

        public static void Clear<T>(PixelData<T> pixelData) where T : IComparable
        {
            Array.Clear(pixelData.Pixels, 0, pixelData.Total);
        }
        
        // TODO change this to Fill
        public static void Fill<T>(PixelData<T> pixelData, T colorRef) where T : IComparable
        {
            for (int i = 0; i < pixelData.Total; i++) pixelData[i] = colorRef;
        }
        
        // TODO need to optimize this if we are not flipping the pixel data and ignoreTransparent is false
        public static void MergePixels<T>(
            // The source pixel data
            PixelData<T> src, 
            // The sample area
            int srcX, 
            int srcY, 
            int srcWidth, 
            int srcHeight,
            // The destination pixel data
            PixelData<T> dest,
            // Destination position
            int destX,
            int destY,
            // Flip pixel data when copying
            bool flipH = false,
            bool flipV = false,
            // Apply a color offset
            T colorOffset = default,
            int maskId = 0,
            int ignoreColorOffsetId = -1
        ) where T : IComparable
        {

            var srcPWidth = src.Width;
            var destPWidth = dest.Width;
            var destPHeight = dest.Height;
            
            var total = srcWidth * srcHeight;
        
            if (total == 0)
                return;
        
            int col = 0, row = 0;

            int tmpX, tmpY, i;

            dynamic tmpPixel;

            // dynamic offset = colorOffset;
        
            for (i = 0; i < total; i++)
            {
                tmpX = col + srcX;
                tmpY = row+ srcY;
        
                tmpPixel = src.Pixels[tmpX + srcPWidth * tmpY];
                
                if (tmpPixel != maskId)
                {
                    tmpX = (flipH ? srcWidth - 1 - col : col) + destX;
        
                    tmpY = (flipV ? srcWidth - 1 - row : row) + destY;
                    
                    if (tmpX >= 0 && tmpX < destPWidth && tmpY >= 0 && tmpY < destPHeight)
                    {
                        var tmp = tmpPixel;

                        if(tmp != ignoreColorOffsetId)
                            tmp += colorOffset;

                        // TODO need to figure out how to fix this
                        dest.Pixels[tmpX + destPWidth * tmpY] = tmp;
                    }
                }
        
                col++;

                if (col < srcWidth) continue;
                col = 0;
                row++;
            }
        }

        public static void Resize<T>(PixelData<T> pixelData, int blockWidth, int blockHeight) where T : IComparable
        {
            
            pixelData.Resize(Utilities.Clamp(blockWidth, 1, 2048), Utilities.Clamp(blockHeight, 1, 2048));

        }

        /// <summary>
        ///     Tests to see if sprite <paramref name="data" /> is empty. This method
        ///     iterates over all the ints in the supplied <paramref name="data" />
        ///     array and looks for a value of -1. If all values are -1 then it
        ///     returns true.
        /// </summary>
        /// <param name="data">An array of ints</param>
        /// <param name="emptyPixel"></param>
        /// <returns>
        /// </returns>
        public static bool IsEmpty<T>(T[] data, T emptyPixel)
        {
            // TODO this need to be generic
            var total = data.Length;
            
            for (var i = 0; i < total; i++)
                
                if ((dynamic)data[i] > emptyPixel)
                    return false;

            return true;
        }
        
        /// <summary>
        ///     Get a single sprite from the Image.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cps">Total number of colors supported by the sprite.</param>
        /// <returns></returns>
        public static T[] GetSpriteData<T>(PixelData<T> pixelData, int id, int? cps = null, T colorRef = default) where T : IComparable
        {

            var Columns = pixelData.Width / Constants.SpriteSize;
            
            var _pos = CalculatePosition(id, Columns);

            // _pixelData = GetPixels(_pos.X * 8, _pos.Y * 8, _spriteSize.X, _spriteSize.Y);
            T[] _tmpPixelData = GetPixels(pixelData, _pos.X * Constants.SpriteSize, _pos.Y * Constants.SpriteSize, Constants.SpriteSize, Constants.SpriteSize);
            // If there is a CPS cap, we need to go through all the pixels and make sure they are in range.
            if (cps.HasValue)
            {
                List<T> _colorIDs = new List<T>();

                for (int i = 0; i < _tmpPixelData.Length; i++)
                {
                    T _colorId = _tmpPixelData[i];

                    if (_colorId.CompareTo(colorRef) >= 0 && _colorIDs.IndexOf(_colorId) == -1)
                    {
                        if (_colorIDs.Count < cps.Value)
                        {
                            _colorIDs.Add(_colorId);
                        }
                        else
                        {
                            _tmpPixelData[i] = colorRef;
                        }
                    }
                }
            }

            // Return the new sprite image
            return _tmpPixelData;
        }

        public static void WriteSpriteData<T>(PixelData<T> pixelData, int id, T[] pixels) where T : IComparable
        {
            var Columns = pixelData.Width / Constants.SpriteSize;
            
            // The total sprite pixel size should be cached
            if (pixels.Length != Constants.SpriteSize * Constants.SpriteSize)
                return;

            // Make sure we stay in bounds
            // id = Utilities.Clamp(id, 0, TotalSprites - 1);

            var pos = Utilities.CalculatePosition(id, Columns);
            pos.X *= Constants.SpriteSize;
            pos.Y *= Constants.SpriteSize;

            SetPixels(pixels, pos.X, pos.Y, Constants.SpriteSize, Constants.SpriteSize, pixelData);
        }
        
    }
}