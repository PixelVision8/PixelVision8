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

using System.Collections.Generic;
using Microsoft.Xna.Framework.Audio;
using System.IO;

namespace PixelVision8.Player.Audio
{
    public class SoundChannel
    {
        
        private SoundEffectInstance _soundInstance;
        protected readonly Dictionary<string, SoundEffectInstance> SoundInstanceCache = new Dictionary<string, SoundEffectInstance>();
        
        public bool Playing
        {
            get
            {
                if (_soundInstance == null) return false;

                return _soundInstance.State == SoundState.Playing;
            }
        }

        /// <summary>
        ///     Plays the sound. If the parameters are dirty, synthesises sound as it plays, caching it for later.
        ///     If they're not, plays from the cached sound. Won't play if caching asynchronously.
        /// </summary>
        public virtual void Play(SoundData soundData, float? frequency = null)
        {
            // TODO this probably doesn't work in the lite version
            // Stop any playing sound
            Stop();

            if (SoundInstanceCache.ContainsKey(soundData.name))
                _soundInstance = SoundInstanceCache[soundData.name];
            else
            {
                // Clear the last sound instance
                _soundInstance = null;

                // See if this is a wav
                if (soundData.bytes != null)
                {
                    // if (waveLock == WaveType.Sample || waveLock == WaveType.None)
                    using (var stream = new MemoryStream(soundData.bytes))
                    {
                        var soundEffect = SoundEffect.FromStream(stream);

                        _soundInstance = soundEffect.CreateInstance();

                        // TODO need to make sure this is correct and we can use the frequency to manipulate the pitch
                        if (frequency.HasValue)
                            _soundInstance.Pitch = frequency.Value;

                    }
                }

                SoundInstanceCache[soundData.name] = _soundInstance;
            }    

            // Only play if there is a sound instance
            _soundInstance?.Play();
        }

        /// <summary>
        ///     Stops the currently playing sound
        /// </summary>
        public virtual void Stop()
        {
            _soundInstance?.Stop();
        }

    }
}