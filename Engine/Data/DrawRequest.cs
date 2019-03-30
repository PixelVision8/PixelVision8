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

namespace PixelVision8.Engine
{

    public struct DrawRequest
    {

        private int[] _pixelData;
        public bool flipH;
        public bool flipV;
        public int colorOffset;
        public int height;
        public int width;
        public int x;
        public int y;

        private int totalPixels;

        public bool isRectangle => totalPixels < 0;
        
        public int[] pixelData
        {
            get { return _pixelData; }
            set
            {
                totalPixels = value?.Length ?? -1;

                // If the DrawRequest is fresh and we're assigning it a new array, use it
                // This should only occur in DisplayChip.NextDrawRequest
                if (_pixelData == null)
                {
                    _pixelData = value;
                    return;
                }

                // ... except we set it to null to draw a solid rectangle
                if (value == null)
                {
                    return;
                }

                if (_pixelData.Length < totalPixels)
                    Array.Resize(ref _pixelData, totalPixels);

                Array.Copy(value, _pixelData, totalPixels);
            }
        }

    }

}