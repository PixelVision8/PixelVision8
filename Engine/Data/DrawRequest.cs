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
        public bool masked = true;
        public int offsetX;
        public int offsetY;
        public DrawMode drawMode;
        public int transparent = -1;
        public int width;
        public int x;
        public int y;

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

//        public void DrawPixels(ref int[] destPixelData, int destWidth, int destHeight, int[] mask = null)
//        {
//            var total = width * height;
//            int srcX;
//            int srcY;
//
//            var tmpWidth = width;
//            int destIndex;
//            var colorID = -1;
//            var totalPixels = destPixelData.Length;
//
//            for (var i = 0; i < total; i++)
//            {
//                srcX = i % tmpWidth + x;
//                srcY = i / tmpWidth + y;
//
//                destIndex = srcX + srcY * destWidth;
//
//                if (destIndex < totalPixels && destIndex > -1)
//                {
//                    colorID = _pixelData[i];
//
//                    if (colorID > -1)
//                    {
//                        if (colorOffset > 0)
//                            colorID += colorOffset;
//
////                        if (drawMode > 0)
//                            destPixelData[destIndex] = colorID;
////                        else if (drawMode == -1)
////                            if (mask != null)
////                            {
////                                if (mask[destIndex] == -1)
////                                    destPixelData[destIndex] = colorID;
////                            }
////                            else
////                            {
////                                if (destPixelData[destIndex] == -1)
////                                    destPixelData[destIndex] = colorID;
////                            }
//                    }
//                }
//            }
//        }

    }

}