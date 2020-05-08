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
using System.Text;

namespace PixelVision8.Engine.Utils
{
    public class SpriteChipUtil
    {
        private static int[] pixels = new int[0];

        public static int[] tmpPixelData = new int[8 * 8];
        private static readonly StringBuilder tmpSB = new StringBuilder();

        public static int CalculateTotalSprites(int width, int height, int spriteWidth, int spriteHeight)
        {
            //TODO this needs to be double checked at different size sprites
            var cols = MathUtil.FloorToInt(width / spriteWidth);
            var rows = MathUtil.FloorToInt(height / spriteHeight);
            return cols * rows;
        }

        public static void CloneTextureData(TextureData source, TextureData target)
        {
            source.CopyPixels(ref tmpPixelData);
            target.SetPixels(tmpPixelData);
        }

        public static void FlipSpriteData(ref int[] pixelData, int sWidth, int sHeight, bool flipH = false,
            bool flipV = false)
        {
            var total = pixelData.Length;
            if (pixels.Length < total) Array.Resize(ref pixels, total);

            Array.Copy(pixelData, pixels, total);

            for (var ix = 0; ix < sWidth; ix++)
            for (var iy = 0; iy < sHeight; iy++)
            {
                var newx = ix;
                var newY = iy;
                if (flipH) newx = sWidth - 1 - ix;

                if (flipV) newY = sHeight - 1 - iy;

                pixelData[ix + iy * sWidth] = pixels[newx + newY * sWidth];
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="data"></param>
        /// <returns>
        /// </returns>
        public static string SpriteDataToString(int[] data)
        {
            tmpSB.Length = 0;
            var total = data.Length;

            for (var i = 0; i < total; i++) tmpSB.Append(data[i]);

            return tmpSB.ToString();
        }

        // private static void InternalRotateImage(int rotationAngle,
        //                                 Bitmap originalBitmap,
        //                                 Bitmap rotatedBitmap)
        // {
        //     // It should be faster to access values stored on the stack
        //     // compared to calling a method (in this case a property) to 
        //     // retrieve a value. That's why we store the width and height
        //     // of the bitmaps here so that when we're traversing the pixels
        //     // we won't have to call more methods than necessary.
        //
        //     int newWidth = rotatedBitmap.Width;
        //     int newHeight = rotatedBitmap.Height;
        //
        //     int originalWidth = originalBitmap.Width;
        //     int originalHeight = originalBitmap.Height;
        //
        //     // We're going to use the new width and height minus one a lot so lets 
        //     // pre-calculate that once to save some more time
        //     int newWidthMinusOne = newWidth - 1;
        //     int newHeightMinusOne = newHeight - 1;
        //
        //     // To grab the raw bitmap data into a BitmapData object we need to
        //     // "lock" the data (bits that make up the image) into system memory.
        //     // We lock the source image as ReadOnly and the destination image
        //     // as WriteOnly and hope that the .NET Framework can perform some
        //     // sort of optimization based on this.
        //     // Note that this piece of code relies on the PixelFormat of the 
        //     // images to be 32 bpp (bits per pixel). We're not interested in 
        //     // the order of the components (red, green, blue and alpha) as 
        //     // we're going to copy the entire 32 bits as they are.
        //     BitmapData originalData = originalBitmap.LockBits(
        //         new Rectangle(0, 0, originalWidth, originalHeight),
        //         ImageLockMode.ReadOnly,
        //         PixelFormat.Format32bppRgb);
        //     BitmapData rotatedData = rotatedBitmap.LockBits(
        //         new Rectangle(0, 0, rotatedBitmap.Width, rotatedBitmap.Height),
        //         ImageLockMode.WriteOnly,
        //         PixelFormat.Format32bppRgb);
        //
        //     // We're not allowed to use pointers in "safe" code so this
        //     // section has to be marked as "unsafe". Cool!
        //     unsafe
        //     {
        //         // Grab int pointers to the source image data and the 
        //         // destination image data. We can think of this pointer
        //         // as a reference to the first pixel on the first row of the 
        //         // image. It's actually a pointer to the piece of memory 
        //         // holding the int pixel data and we're going to treat it as
        //         // an array of one dimension later on to address the pixels.
        //         int* originalPointer = (int*)originalData.Scan0.ToPointer();
        //         int* rotatedPointer = (int*)rotatedData.Scan0.ToPointer();
        //
        //         // There are nested for-loops in all of these case statements
        //         // and one might argue that it would have been neater and more
        //         // tidy to have the switch statement inside the a single nested
        //         // set of for loops, doing it this way saves us up to three int 
        //         // to int comparisons per pixel. 
        //         //
        //         switch (rotationAngle)
        //         {
        //             case 90:
        //                 for (int y = 0; y < originalHeight; ++y)
        //                 {
        //                     int destinationX = newWidthMinusOne - y;
        //                     for (int x = 0; x < originalWidth; ++x)
        //                     {
        //                         int sourcePosition = (x + y * originalWidth);
        //                         int destinationY = x;
        //                         int destinationPosition =
        //                                 (destinationX + destinationY * newWidth);
        //                         rotatedPointer[destinationPosition] =
        //                             originalPointer[sourcePosition];
        //                     }
        //                 }
        //                 break;
        //             case 180:
        //                 for (int y = 0; y < originalHeight; ++y)
        //                 {
        //                     int destinationY = (newHeightMinusOne - y) * newWidth;
        //                     for (int x = 0; x < originalWidth; ++x)
        //                     {
        //                         int sourcePosition = (x + y * originalWidth);
        //                         int destinationX = newWidthMinusOne - x;
        //                         int destinationPosition = (destinationX + destinationY);
        //                         rotatedPointer[destinationPosition] =
        //                             originalPointer[sourcePosition];
        //                     }
        //                 }
        //                 break;
        //             case 270:
        //                 for (int y = 0; y < originalHeight; ++y)
        //                 {
        //                     int destinationX = y;
        //                     for (int x = 0; x < originalWidth; ++x)
        //                     {
        //                         int sourcePosition = (x + y * originalWidth);
        //                         int destinationY = newHeightMinusOne - x;
        //                         int destinationPosition =
        //                             (destinationX + destinationY * newWidth);
        //                         rotatedPointer[destinationPosition] =
        //                             originalPointer[sourcePosition];
        //                     }
        //                 }
        //                 break;
        //         }
        //
        //         // We have to remember to unlock the bits when we're done.
        //         originalBitmap.UnlockBits(originalData);
        //         rotatedBitmap.UnlockBits(rotatedData);
        //     }
        // }
    }
}