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

namespace PixelVision8.Engine
{
    public class PixelData
    {
        public int[] Pixels = new int[0];
        public int Width { get; private set; }
        public int Height { get;  private set; }
        public int TotalPixels { get;  private set; }

        public PixelData(int width = 1, int height = 1)
        {
            Resize(width, height);
        }

        public void Resize(int width, int height)
        {
            Width = width;
            Height = height;
            TotalPixels = Width * Height;
            Array.Resize(ref Pixels, TotalPixels);
        }

        public int this[in int i]
        {
            get => Pixels[i];
            set => Pixels[i] = value;
        }

        public override string ToString() => "{Width:" + Width + " Height:" + Height + " Total Pixels:" + TotalPixels + "}";
    }
}