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
namespace PixelVisionSDK.Chips
{

    public interface ILayer: IInvalidate
    {
//        int scrollX { get; set; }
//        int scrollY { get; set; }
        void ReadPixelData(int width, int height, ref int[] pixelData, int offsetX = 0, int offsetY = 0 );
    }

}