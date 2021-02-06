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

using Microsoft.Xna.Framework;

namespace PixelVision8.Player
{
    public interface IDisplay
    {
        /// <summary>
        ///     A public getter for the internal
        ///     TextureData. When requested, a clone of the <see cref="_texture" />
        ///     field is returned. This is expensive and only used for tools.
        /// </summary>
        PixelData PixelData { get; }
    }

    public struct DrawRequest
    {
        private PixelData _tmpPixelData;

        public Rectangle SampleRect;
        public bool FlipH;
        public bool FlipV;
        public int ColorOffset;
        public int X;
        public int Y;

        public byte Priority;

        public PixelData PixelData { get; private set; }

        public void SampleFrom(IDisplay source, int srcX, int srcY, int blockWidth, int blockHeight)
        {
            PixelData = source.PixelData;

            SampleRect.X = srcX;
            SampleRect.Y = srcY;
            SampleRect.Width = blockWidth;
            SampleRect.Height = blockHeight;
        }

        public void SampleFrom(int[] pixels, int srcX, int srcY, int blockWidth, int blockHeight)
        {
            _tmpPixelData ??= new PixelData(blockWidth, blockHeight);

            _tmpPixelData.SetPixels(pixels, blockWidth, blockHeight);

            PixelData = _tmpPixelData;
            SampleRect.X = srcX;
            SampleRect.Y = srcY;
            SampleRect.Width = blockWidth;
            SampleRect.Height = blockHeight;
        }
    }
}