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

using PixelVision8.Engine;
using PixelVision8.Runner.Chips.Sfxr;

namespace PixelVision8.Runner.Data
{
    public class SfxrSoundData : ISoundData
    {
        protected SfxrSynth synth;
//        protected AudioPlayer audioPlayer;
        
        public SfxrSoundData(string name = "Untitled")
        {
            this.name = name;
            synth = new SfxrSynth();
        }

        public SfxrParams parameters => synth.parameters;

        public bool ignore { get; private set; }

        public string name { get; set; }

        public bool playing => synth.playing;

        /// <summary>
        ///     Plays the sound at a specific frequency.
        /// </summary>
        /// <param name="frequency"></param>
        public void Play(float? frequency = null)
        {
            if (frequency.HasValue)
                synth.parameters.startFrequency = frequency.Value;

            synth.Play();
        }

        /// <summary>
        ///     Stops the current sound from playing
        /// </summary>
        public void Stop()
        {
            synth.Stop();
        }

        /// <summary>
        ///     Caches the sound file to improve performance
        /// </summary>
//        public void CacheSound()
//        {
//            synth.CacheSound();
//        }

        public void UpdateSettings(string param)
        {
            synth.parameters.SetSettingsString(param);
//            CacheSound();
        }

        public string ReadSettings()
        {
            return synth.parameters.GetSettingsString();
        }

        public void Mutate(float value = 0.05f)
        {
            synth.parameters.Mutate(value);
        }
    }
}