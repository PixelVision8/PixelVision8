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
// Jesse Freeman
// 

using System;
using PixelVisionSDK.Engine.Chips.Data;
using PixelVisionSDK.Engine.Utils;

namespace PixelVisionSDK.Engine.Chips.Audio
{
    /// <summary>
    ///     The <see cref="SoundChip" /> is responsible for playing back sound
    ///     effects in the engine. It's powered by SFxr.
    /// </summary>
    public class SoundChip : AbstractChip
    {
        protected int _totalSounds;
        protected ISoundData[] channels = new ISoundData[0];

        protected ISoundData[] sounds;

        /// <summary>
        ///     The total number of <see cref="channels" /> available for playing
        ///     back sounds.
        /// </summary>
        public int totalChannels
        {
            get { return channels.Length; }
            set { Array.Resize(ref channels, value); }
        }

        /// <summary>
        ///     The total number of <see cref="sounds" /> in the collection.
        /// </summary>
        public int totalSounds
        {
            get { return sounds.Length; }
            set
            {
                // TODO need to copy over existing sounds
                value = value.Clamp(1, 96);

                Array.Resize(ref sounds, value);

                for (var i = 0; i < value; i++)
                {
                    if (sounds[i] == null)
                        sounds[i] = CreateEmptySoundData();
                }
            }
        }

        /// <summary>
        ///     Updates a sound in the collection.
        /// </summary>
        /// <param name="index">The index to update the sound at.</param>
        /// <param name="param">
        ///     A string representing the synth properties.
        /// </param>
        public void UpdateSound(int index, string param)
        {
            var synth = sounds[index];
            synth.CacheSound();
        }

        /// <summary>
        ///     Clear all the <see cref="sounds" /> in the collection.
        /// </summary>
        /// <param name="index"></param>
        public void ClearSound(int index)
        {
            // TODO need to see if there is a better way to revert a sound
            sounds[index] = CreateEmptySoundData();
        }

        /// <summary>
        ///     This stub methods is designed to be overriden with a Factory to
        ///     create new sound instances that implement the ISoundData interface.
        /// </summary>
        /// <returns></returns>
        public virtual ISoundData CreateEmptySoundData()
        {
            throw new NotImplementedException(
                "Need to create a new ISoundData type and override SoundCollection CreateEmptySoundData method.");
        }

        /// <summary>
        ///     Configures the <see cref="SoundChip" /> by registering itself with
        ///     the engine and setting up the deafult values for total
        ///     <see cref="sounds" /> and total channels.
        /// </summary>
        public override void Configure()
        {
            engine.soundChip = this;
            totalSounds = 16;
            totalChannels = 4;
        }

        /// <summary>
        ///     This method plays back a sound on a specific channel. The
        ///     <see cref="SoundChip" /> has a limit of active <see cref="channels" />
        ///     so playing a sound effect while another was is playing on the same
        ///     <paramref name="channel" /> will cancel it out and replace with the
        ///     new sound.
        /// </summary>
        /// <param name="index">
        ///     The ID of the sound in the SoundCollection.
        /// </param>
        /// <param name="channel">
        ///     The channel the sound should play back on.
        /// </param>
        public void PlaySound(int index, int channel = 0)
        {
            if (index > sounds.Length)
                return;

            LoadSound(index, channel, true);
        }

        /// <summary>
        ///     Loads a sound into a channel so it can be played.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="channel"></param>
        /// <param name="autoPlay"></param>
        public void LoadSound(int index, int channel = 0, bool autoPlay = false)
        {
            channel = channel.Clamp(0, totalChannels - 1);

            if (channels[channel] != null)
            {
                channels[channel].Stop();
                channels[channel] = null;
            }

            if (channels[channel] == null)
            {
                channels[channel] = ReadSound(index);

                if (autoPlay)
                    channels[channel].Play();
            }
        }

        /// <summary>
        ///     Returns a Sfxr Synth to be played back at run time by the SoundChip.
        /// </summary>
        /// <param name="index">
        ///     The index where the sound is stored in the <see cref="sounds" />
        ///     array.
        /// </param>
        /// <returns>
        ///     A reference to a SfxrSynth which contains the sound data.
        /// </returns>
        public ISoundData ReadSound(int id)
        {
            return sounds[id];
        }

        /// <summary>
        ///     Returns a reference to the current channel to control it's assigned
        ///     sound value.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public ISoundData ReadChannel(int index)
        {
            if (index < 0 || index >= totalChannels)
                return null;

            return channels[index];
        }
    }
}