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

using PixelVisionSDK.Chips;

namespace PixelVisionSDK
{

    public interface IButtonState
    {

        bool value { get; set; }
        Buttons buttons { get; set; }
        bool buttonReleased { get; }
        float buttonTimes { get; }
        bool dirty { get; set; }
        int mapping { get; set; }
        void Reset();
        void Release();
        void Update(float timeDelta);

    }

}