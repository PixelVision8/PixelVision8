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

using System;
using Microsoft.Xna.Framework;

namespace PixelVision8.Engine.Chips
{
    /// <summary>
    ///     The <see cref="SoundChip" /> is responsible for playing back sound
    ///     effects in the engine. It's powered by SFxr.
    /// </summary>
    public class SoundChip : AbstractChip
    {
        protected ISoundData[] channels = new ISoundData[0];

        protected ISoundData[] sounds;

        /// <summary>
        ///     The total number of <see cref="channels" /> available for playing
        ///     back sounds.
        /// </summary>
        public int totalChannels
        {
            get => channels.Length;
            set => Array.Resize(ref channels, value);
        }

        /// <summary>
        ///     The total number of <see cref="sounds" /> in the collection.
        /// </summary>
        public int totalSounds
        {
            get => sounds.Length;
            set
            {
                // TODO need to copy over existing sounds
                value = MathHelper.Clamp(value, 1, 96);

                Array.Resize(ref sounds, value);

                for (var i = 0; i < value; i++)
                    if (sounds[i] == null)
                        sounds[i] = CreateEmptySoundData("Untitled" + i.ToString("D2"));
            }
        }

        /// <summary>
        ///     Updates a sound in the collection.
        /// </summary>
        /// <param name="index">The index to update the sound at.</param>
        /// <param name="param">
        ///     A string representing the synth properties.
        /// </param>
        public virtual void UpdateSound(int index, string param)
        {
            var synth = sounds[index];
            synth.CacheSound();
            synth.UpdateSettings(param);
        }

        /// <summary>
        ///     Clear all the <see cref="sounds" /> in the collection.
        /// </summary>
        /// <param name="index"></param>
        public void ClearSound(int index)
        {
            // TODO need to see if there is a better way to revert a sound
            sounds[index] = CreateEmptySoundData("Untitled" + index.ToString("D2"));
        }

        /// <summary>
        ///     This stub methods is designed to be overridden with a Factory to
        ///     create new sound instances that implement the ISoundData interface.
        /// </summary>
        /// <returns></returns>
        public virtual ISoundData CreateEmptySoundData(string name = "Untitled")
        {
            throw new NotImplementedException(
                "Need to create a new ISoundData type and override SoundCollection CreateEmptySoundData method.");
        }

        /// <summary>
        ///     Configures the <see cref="SoundChip" /> by registering itself with
        ///     the engine and setting up the default values for total
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
        public void PlaySound(int index, int channelID = 0, float? frequency = null)
        {
            if (index > sounds.Length)
                return;

            channelID = MathHelper.Clamp(channelID, 0, totalChannels - 1);

            var channel = channels[channelID];

            if (channel != null) channel.Stop();

            channel = sounds[index];

            channel.Play(frequency);
        }

        public bool IsChannelPlaying(int channelID)
        {
            return channels[channelID] != null && channels[channelID].playing;
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

        public string ReadLabel(int id)
        {
            return ReadSound(id).name;
        }

        public void UpdateLabel(int id, string name)
        {
            ReadSound(id).name = name;
        }

        public void StopSound(int channel)
        {
            if (channels[channel] != null) channels[channel].Stop();
        }
    }
}