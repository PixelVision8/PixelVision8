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
using PixelVision8.Engine.Utils;

namespace PixelVision8.Engine
{
    public struct DrawRequestPixelData
    {
        private PixelData _pixelData;
        public bool flipH;
        public bool flipV;
        public int colorOffset;
        public int x;
        public int y;
        public byte Priority;

        public int height => PixelData.Height;
        public int width => PixelData.Width;
        private int totalPixels => PixelData.TotalPixels;

        public PixelData PixelData
        {
            get
            {
                if(_pixelData == null)
                    _pixelData = new PixelData();

                return _pixelData;
            }
        }
        public void SetPixels(int[] pixels, int blockWidth, int blockHeight)
        {
            if (width != blockWidth || height != blockHeight)
            {
                PixelDataUtil.Resize(ref _pixelData, blockWidth, blockHeight);
            }
            
            PixelDataUtil.SetPixels(pixels, _pixelData);
        }

        public bool isRectangle => totalPixels < 0;

        // public int[] pixelData
        // {
        //     get => _pixelData;
        //     set
        //     {
        //         totalPixels = value?.Length ?? -1;
        //
        //         // If the DrawRequest is fresh and we're assigning it a new array, use it
        //         // This should only occur in DisplayChip.NextDrawRequest
        //         if (_pixelData == null)
        //         {
        //             _pixelData = value;
        //             return;
        //         }
        //
        //         // ... except we set it to null to draw a solid rectangle
        //         if (value == null) return;
        //
        //         if (_pixelData.Length < totalPixels) Array.Resize(ref _pixelData, totalPixels);
        //
        //         Array.Copy(value, _pixelData, totalPixels);
        //     }
        // }
    }
}