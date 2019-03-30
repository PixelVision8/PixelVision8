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

using MonoGameRunner.Audio;
using PixelVisionRunner.Chips.Sfxr;
using PixelVisionSDK.Chips;

namespace MonoGameRunner
{
    public class AudioPlayerFactory : IAudioPlayerFactory, IVolumeManager
    {
        private int lastVolume;
        private bool _mute;
        private int muteVoldume;
        
        public IAudioPlayer Create(ISfxrSynth synth)
        {
            return new AudioPlayer(synth, this);
        }

        public int Volume(int? value = null)
        {
            if (value.HasValue)
            {
                lastVolume = value.Value;
            }

            return lastVolume;
        }

        public bool Mute(bool? value = null)
        {
            if (value.HasValue)
            {
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
            }
                
            return _mute;
        }
    }
}
