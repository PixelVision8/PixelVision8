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

namespace PixelVision8.Player
{
    public static partial class Utilities
    {
        
        public static int[] GetPixels(PixelData pixelData)
        {
            
            // Create a new temporary array to copy the pixel data into
            var tmpPixels = new int[pixelData.Total];
            
            // Use the Array.Copy() method to quickly copy all of the pixel data into the new temporary array
            Array.Copy(pixelData.Pixels, tmpPixels, pixelData.Total);

            // Return the temporary pixels
            return tmpPixels;
        }

        public static int[] GetPixels(PixelData pixelData, int x, int y, int blockWidth, int blockHeight)
        {
            
            // Create a new temporary array to copy the pixel data into
            var tmpPixels = new int[blockWidth * blockHeight];

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

        public static void SetPixels(int[] srcPixels, int x, int y, int blockWidth, int blockHeight,
            PixelData destPixelData)
        {

            // var srcPixelData = new PixelData(blockWidth, blockHeight);
            // srcPixelData.SetPixels(srcPixels);

            // var srcPWidth = blockWidth;
            var destPWidth = destPixelData.Width;
            var destPHeight = destPixelData.Height;

            var total = blockWidth * blockHeight;
        
            // Console.WriteLine("Run Set Pixels {0} {1}", total, srcPixels.Length);
            //
            //
            // if (total == 0 || total != srcPixels.Length)
            //     return;
            
        
            int col = 0, row = 0;

            int tmpX, tmpY, tmpPixel, i;
        
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
        
        public static void Clear(PixelData pixelData, int colorRef = -1)
        {
            for (var i = pixelData.Total - 1; i > -1; i--) pixelData[i] = colorRef;
        }
        
        public static void MergePixels(
            // The source pixel data
            PixelData src, 
            // The sample area
            int srcX, 
            int srcY, 
            int srcWidth, 
            int srcHeight,
            // The destination pixel data
            PixelData dest,
            // Destination position
            int destX,
            int destY,
            // Flip pixel data when copying
            bool flipH = false,
            bool flipV = false,
            // Apply a color offset
            int colorOffset = 0,
            bool ignoreTransparent = true
        )
        {

            var srcPWidth = src.Width;
            var destPWidth = dest.Width;
            var destPHeight = dest.Height;
            
            var total = srcWidth * srcHeight;
        
            if (total == 0)
                return;
        
            int col = 0, row = 0;

            int tmpX, tmpY, tmpPixel, i;
        
            for (i = 0; i < total; i++)
            {
                tmpX = col + srcX;
                tmpY = row+ srcY;
        
                tmpPixel = src.Pixels[tmpX + srcPWidth * tmpY];
                
                if (tmpPixel != -1 || ignoreTransparent != true)
                {
                    tmpX = (flipH ? srcWidth - 1 - col : col) + destX;
        
                    tmpY = (flipV ? srcWidth - 1 - row : row) + destY;
                    
                    if (tmpX >= 0 && tmpX < destPWidth && tmpY >= 0 && tmpY < destPHeight)
                    {
                        dest.Pixels[tmpX + destPWidth * tmpY] = tmpPixel + colorOffset;
                    }
                }
        
                col++;

                if (col < srcWidth) continue;
                col = 0;
                row++;
            }
        }

        public static void Resize(PixelData pixelData, int blockWidth, int blockHeight)
        {
            
            pixelData.Resize(MathHelper.Clamp(blockWidth, 1, 2048), MathHelper.Clamp(blockHeight, 1, 2048));

            Clear(pixelData);
        }
        
    }
}