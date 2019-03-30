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

using Microsoft.Xna.Framework.Audio;
using PixelVisionRunner.Chips.Sfxr;
using System.IO;
using PixelVisionSDK.Chips;

namespace MonoGameRunner.Audio
{
    class AudioPlayer : IAudioPlayer
    {
        private SoundEffect _soundEffect;

        private SoundEffectInstance _soundEffectInstance;

        private bool _isDisposed = false;
        private IVolumeManager volumeManager;
        private SoundState state;

        public bool playing
        {
            get { return state == SoundState.Playing; }
        }
        
        public AudioPlayer(ISfxrSynth synth, IVolumeManager volumeManagerManager)
        {

            this.volumeManager = volumeManagerManager;
            
            var wavData = synth.GenerateWav();

            using (var stream = new MemoryStream(wavData))
            {
                _soundEffect = SoundEffect.FromStream(stream);
            }

            // may make these on a per-play instance at some point
            _soundEffectInstance = _soundEffect.CreateInstance();
            state = _soundEffectInstance.State;
        }

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
            if (volumeManager.Mute())
                return;

            // Apply sound and convert to a fraction
            _soundEffectInstance.Volume = volumeManager.Volume()/100f;
            _soundEffectInstance.Play();
            
        }

        public void Stop()
        {
            _soundEffectInstance.Stop();
        }
    }
}
