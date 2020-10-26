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

    public struct DrawRequestPixelData
    {
        private PixelData _tmpPixelData;
        
        public Rectangle SampleRect;
        public bool FlipH;
        public bool FlipV;
        public int ColorOffset;
        public int x;
        public int y;
        
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
            if(_tmpPixelData == null)
                _tmpPixelData = new PixelData(blockWidth, blockHeight);
            
            if(_tmpPixelData.Width != blockWidth || _tmpPixelData.Height != blockHeight)
                _tmpPixelData.Resize(blockWidth, blockHeight);
            
            _tmpPixelData.Pixels = pixels;
            
            PixelData = _tmpPixelData;
            SampleRect.X = srcX;
            SampleRect.Y = srcY;
            SampleRect.Width = blockWidth;
            SampleRect.Height = blockHeight;
        }
        
        

    }
}