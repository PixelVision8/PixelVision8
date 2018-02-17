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
using PixelVisionSDK.Chips;

namespace PixelVisionSDK
{

    public class DrawRequest
    {

        protected int[] _pixelData = new int[0];
        public int colorOffset = 0;
        public int height;
        public int layer;
        public int width;
        public int x;
        public int y;

        private int totalPixels;
        
        public int[] pixelData
        {
            get { return _pixelData; }
            set
            {
                totalPixels = value.Length;

                if (_pixelData.Length != totalPixels)
                    Array.Resize(ref _pixelData, totalPixels);

                Array.Copy(value, _pixelData, totalPixels);
            }
        }

    }

}