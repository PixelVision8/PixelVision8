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
// 

using System;

namespace PixelVisionSDK
{

    public class DrawRequest
    {

        protected int[] _pixelData = new int[0];
        public bool masked = true;
        public int order;
        public int transparent = -1;
        public int x;
        public int y;
        public int width;
        public int height;
        public int offsetX;
        public int offsetY;

        public int[] pixelData
        {
            get { return _pixelData; }
            set
            {
                var totalPixels = value.Length;

                if (_pixelData.Length != totalPixels)
                    Array.Resize(ref _pixelData, totalPixels);

                Array.Copy(value, _pixelData, totalPixels);
            }
        }
//
//        public virtual void MergePixelData(TextureData target)
//        {
//            target.MergePixels(x, y, width, height, pixelData, masked, transparent);
//        }

        public void DrawPixels(ref int[] pixelData, int destWidth, int destHeight )
        {
            var total = width * height;
            int srcX;
            int srcY;
            
            var tmpWidth = width;
            int destIndex;
            int colorID = -1;
            int totalPixels = pixelData.Length;
            
            for (int i = 0; i < total; i++)
            {

                srcX = i % tmpWidth + x;
                srcY = i / tmpWidth + y;

                // Wrap Pixels
                //srcX = (int)(srcX - Math.Floor(x / (float)destWidth) * destWidth);
                srcY = (int)(srcY - Math.Floor(y / (float)destHeight) * destHeight);

                destIndex = srcX + srcY * destWidth;

                if (destIndex < totalPixels && destIndex > -1)
                {
                    colorID = _pixelData[i];

                    if (colorID > -1)
                        pixelData[destIndex] = _pixelData[i];
                }

                
            }
        }
    }

}