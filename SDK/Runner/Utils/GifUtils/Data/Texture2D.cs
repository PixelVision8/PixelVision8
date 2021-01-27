//   
// Copyright (c) Jesse Freeman, Pixel Vision 8. All rights reserved.  
//  
// Licensed under the Microsoft Public License (MS-PL) except for a few
// portions of the code. See LICENSE file in the project root for full 
// license information. Third-party libraries used by Pixel Vision 8 are 
// under their own licenses. Please refer to those libraries for details 
// on the license they use.
//
// Based on SimpleGif (https://github.com/hippogamesunity/simplegif) by
// Nate River of Hippo Games
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

using Microsoft.Xna.Framework;
using System;
using System.Linq;

namespace PixelVision8.Runner.Gif
{
    /// <summary>
    /// Stub for Texture2D from UnityEngine.CoreModule
    /// </summary>
    public class Texture2D
    {
        // ReSharper disable once InconsistentNaming (original naming saved)
        public readonly int width;

        // ReSharper disable once InconsistentNaming (original naming saved)
        public readonly int height;

        private Color[] _pixels;

        public Texture2D(int width, int height)
        {
            this.width = width;
            this.height = height;
        }

        public void SetPixels32(Color[] pixels)
        {
            _pixels = pixels.ToArray();
        }

        public Color[] GetPixels32()
        {
            return _pixels.ToArray();
        }

        public void Apply()
        {
        }

        public virtual Color[] GetPixels(int x, int y, int blockWidth, int blockHeight)
        {
            var tmpPixels = new Color[blockWidth * blockHeight];

            CopyPixels(ref tmpPixels, x, y, blockWidth, blockHeight);

            //            Array.Copy(pixels, tmpPixels, pixels.Length);

            return tmpPixels;
        }

        public void CopyPixels(ref Color[] data, int x, int y, int blockWidth, int blockHeight)
        {
            var total = blockWidth * blockHeight;

            if (data.Length < total)
                Array.Resize(ref data, total);

            // Per-line copy, as there is no special per-pixel logic required.

            // Vertical wrapping is not an issue. Horizontal wrapping requires splitting the copy into two operations.
            // Keep important data in local variables.
            int srcY;
            var src = _pixels;

            var offsetStart = (x % width + width) % width;
            var offsetEnd = offsetStart + blockWidth;
            if (offsetEnd <= width)
            {
                // Copy each entire line at once.
                for (var tmpY = blockHeight - 1; tmpY > -1; --tmpY)
                {
                    // Note: + size and the second modulo operation are required to get wrapped values between 0 and +size
                    srcY = ((y + tmpY) % height + height) % height;
                    Array.Copy(src, offsetStart + srcY * width, data, tmpY * blockWidth, blockWidth);
                }
            }
            else
            {
                // Copy each non-wrapping section and each wrapped section separately.
                var wrap = offsetEnd % width;
                for (var tmpY = blockHeight - 1; tmpY > -1; --tmpY)
                {
                    // Note: + size and the second modulo operation are required to get wrapped values between 0 and +size
                    srcY = ((y + tmpY) % height + height) % height;
                    Array.Copy(src, offsetStart + srcY * width, data, tmpY * blockWidth, blockWidth - wrap);
                    Array.Copy(src, srcY * width, data, blockWidth - wrap + tmpY * blockWidth, wrap);
                }
            }
        }
    }
}