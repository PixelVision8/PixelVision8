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

namespace PixelVisionRunner
{
    public interface IAudioClip
    {
        bool SetData(float[] data, int offsetSamples);
        int samples { get;}
        int channels { get;}
        bool GetData(float[] data, int offsetSamples);
        int frequency { get;}
    }
}