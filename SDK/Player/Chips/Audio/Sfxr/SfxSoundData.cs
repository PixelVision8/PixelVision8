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

namespace PixelVision8.Player
{
    public class SfxSoundData : SoundData
    {
        //public static readonly string DEFAULT_SOUND_PARAM = "0,,.0185,.4397,.1783,.8434,,,,,,,,,,,,,1,,,,,.5";

        public string param { get; set; }

        public bool isWav => bytes != null;

        public SfxSoundData(string name, string param = "0,,.0185,.4397,.1783,.8434,,,,,,,,,,,,,1,,,,,.5") : base(name,
            null)
        {
            this.param = param;
        }
    }
}