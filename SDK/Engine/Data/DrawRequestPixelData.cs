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
using PixelVision8.Engine.Chips;

namespace PixelVision8.Engine
{

    public struct DrawPixelDataRequest : IDisplay
    {
        public PixelData PixelData { get; }

        public DrawPixelDataRequest(int[] pixels, int width, int height)
        {
            PixelData = new PixelData();
            PixelData.Resize(width, height);
            if(pixels != null)
                PixelData.Pixels = pixels;
        }
    }
    
    public struct DrawRequestPixelData
    {
        private PixelData _pixelData;
        
        private IDisplay SrcPixelData;
        public Rectangle SampleRect;
        public bool FlipH;
        public bool FlipV;
        public int ColorOffset;
        public int x;
        public int y;
        
        public byte Priority;

        public int height => SampleRect.Height;
        public int width => SampleRect.Width;
        private int totalPixels => PixelData.TotalPixels;

        public PixelData PixelData => SrcPixelData.PixelData;
        
        public void SampleFrom(IDisplay source, int srcX, int srcY, int blockWidth, int blockHeight)
        {
            SrcPixelData = source;
            SampleRect.X = srcX;
            SampleRect.Y = srcY;
            SampleRect.Width = blockWidth;
            SampleRect.Height = blockHeight;
        }

        public bool isRectangle => totalPixels < 0;

    }
}