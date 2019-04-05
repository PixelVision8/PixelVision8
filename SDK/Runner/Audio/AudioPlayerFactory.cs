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

using PixelVision8.Engine.Chips;
using PixelVision8.Runner.Chips.Sfxr;

namespace PixelVision8.Runner.Audio
{
    public class AudioPlayerFactory : IAudioPlayerFactory, IVolumeManager
    {
        private bool _mute;
        private int lastVolume;
        private int muteVoldume;

        public IAudioPlayer Create(ISfxrSynth synth)
        {
            return new AudioPlayer(synth, this);
        }

        public int Volume(int? value = null)
        {
            if (value.HasValue) lastVolume = value.Value;

            return lastVolume;
        }

        public bool Mute(bool? value = null)
        {
            if (value.HasValue)
                if (_mute != value)
                {
                    _mute = value.Value;

                    if (_mute)
                    {
                        muteVoldume = lastVolume;

                        Volume(0);
                    }
                    else
                    {
                        Volume(muteVoldume);
                    }
                }

            return _mute;
        }
    }
}