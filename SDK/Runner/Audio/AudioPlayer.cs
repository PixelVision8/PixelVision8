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

using System.IO;
using Microsoft.Xna.Framework.Audio;

namespace PixelVision8.Runner.Audio
{
    internal class AudioPlayer : IAudioPlayer
    {
        private bool _isDisposed;
        private readonly SoundEffect _soundEffect;

        private readonly SoundEffectInstance _soundEffectInstance;
        private readonly SoundState state;

        public AudioPlayer(byte[] wavData)
        {

            using (var stream = new MemoryStream(wavData))
            {
                _soundEffect = SoundEffect.FromStream(stream);
            }

            // may make these on a per-play instance at some point
            _soundEffectInstance = _soundEffect.CreateInstance();
            state = _soundEffectInstance.State;
        }

        public bool playing => state == SoundState.Playing;

        public void Dispose()
        {
            if (_isDisposed) return;
            _soundEffectInstance.Stop();
            _soundEffectInstance.Dispose();
            _soundEffect.Dispose();
            _isDisposed = true;
        }

        public void Play()
        {
            if (VolumeManager.Mute())
                return;

            // Apply sound and convert to a fraction
            _soundEffectInstance.Volume = VolumeManager.Volume() / 100f;
            _soundEffectInstance.Play();
        }

        public void Stop()
        {
            _soundEffectInstance.Stop();
        }
    }
}