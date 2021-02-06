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

namespace PixelVision8.Player
{
    public class PixelData
    {
        private int[] _pixels;

        public int[] Pixels => _pixels;
        
        public int Width { get; private set; } = 1;
        public int Height { get; private set; } = 1;
        public int Total { get; private set; } = 1;

        public PixelData(int width = 1, int height = 1)
        {
            Resize(width, height);
        }

        public void Resize(int width, int height)
        {
            if(width > 0)
                Width = width;
            
            if(height > 0)
                Height = height;
            
            Total = Width * Height;

            _pixels = Pixels;
            Array.Resize(ref _pixels, Total);
        }

        public int this[in int i]
        {
            get => _pixels[i];
            set => _pixels[i] = value;
        }

        public void SetPixels(int[] pixels, int width, int height)
        {

            if (width * height != pixels.Length)
                return;
            
            _pixels = pixels;
            Width = width;
            Height = height;
            Total = pixels.Length;
            
        }

        public override string ToString() => "{Width:" + Width + " Height:" + Height + " Total Pixels:" + Total + "}";
    }
}