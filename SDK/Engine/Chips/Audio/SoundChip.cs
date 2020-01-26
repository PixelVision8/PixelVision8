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
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using PixelVision8.Engine.Audio;

namespace PixelVision8.Engine.Chips
{
    /// <summary>
    ///     The <see cref="SoundChip" /> is responsible for playing back sound
    ///     effects in the engine. It's powered by SFxr.
    /// </summary>
    public class SoundChip : AbstractChip
    {
        private readonly Dictionary<string, byte[]> soundBank = new Dictionary<string, byte[]>();
        protected IChannel[] Channels = new IChannel[0];

        protected SoundData[] Sounds;

        /// <summary>
        ///     The total number of <see cref="Channels" /> available for playing
        ///     back sounds.
        /// </summary>
        public int totalChannels
        {
            get => Channels.Length;
            set
            {
                value = MathHelper.Clamp(value, 1, 5);
                Array.Resize(ref Channels, value);
                for (var i = 0; i < value; i++)
                    if (Channels[i] == null)
                        Channels[i] = CreateSoundChannel();
            }
        }

        /// <summary>
        ///     The total number of <see cref="Sounds" /> in the collection.
        /// </summary>
        public int TotalSounds
        {
            get => Sounds.Length;
            set
            {
                // TODO need to copy over existing sounds
                value = MathHelper.Clamp(value, 1, 96);

                Array.Resize(ref Sounds, value);

                for (var i = 0; i < value; i++)
                    if (Sounds[i] == null)
                        Sounds[i] = new SoundData("Untitled" + i.ToString("D2"));
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
            //            var synth = sounds[index];
            //            synth.UpdateSettings(param);

            Sounds[index].param = param;
        }

        /// <summary>
        ///     Clear all the <see cref="sounds" /> in the collection.
        /// </summary>
        /// <param name="index"></param>
        //        public void ClearSound(int index)
        //        {
        //            // TODO need to see if there is a better way to revert a sound
        //            sounds[index] = new SoundData("Untitled" + index.ToString("D2"));
        //        }

        /// <summary>
        ///     This stub methods is designed to be overridden with a Factory to
        ///     create new sound instances that implement the ISoundData interface.
        /// </summary>
        /// <returns></returns>
        public virtual IChannel CreateSoundChannel()
        {
            return new SfxrSynth();
        }

        /// <summary>
        ///     Configures the <see cref="SoundChip" /> by registering itself with
        ///     the engine and setting up the default values for total
        ///     <see cref="Sounds" /> and total channels.
        /// </summary>
        public override void Configure()
        {
            engine.SoundChip = this;
            TotalSounds = 16;
            totalChannels = 5;
        }

        /// <summary>
        ///     This method plays back a sound on a specific channel. The
        ///     <see cref="SoundChip" /> has a limit of active <see cref="Channels" />
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
            if (index > Sounds.Length) return;

            channelID = MathHelper.Clamp(channelID, 0, totalChannels - 1);

            var channel = Channels[channelID];

            channel?.Stop();

            //            channel = sounds[index];

            channel.Play(Sounds[index], frequency);
        }

        public bool IsChannelPlaying(int channelID)
        {
            return Channels[channelID] != null && Channels[channelID].Playing;
        }

        /// <summary>
        ///     Returns a Sfxr Synth to be played back at run time by the SoundChip.
        /// </summary>
        /// <param name="index">
        ///     The index where the sound is stored in the <see cref="Sounds" />
        ///     array.
        /// </param>
        /// <returns>
        ///     A reference to a SfxrSynth which contains the sound data.
        /// </returns>
        public SoundData ReadSound(int id)
        {
            return Sounds[id];
        }

        /// <summary>
        ///     Returns a reference to the current channel to control it's assigned
        ///     sound value.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        //        public IChannel ReadChannel(int index)
        //        {
        //            if (index < 0 || index >= totalChannels)
        //                return null;
        //
        //            return channels[index];
        //        }
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
            if (Channels[channel] != null) Channels[channel].Stop();
        }

        public override void Shutdown()
        {
            foreach (var channel in Channels)
                if (channel.Playing)
                    channel.Stop();

            base.Shutdown();
        }

        public WaveType ChannelType(int id, WaveType? type = null)
        {
            // The channel will handle this so pass the values over to its API.
            return Channels[id].ChannelType(type);
        }

        /// <summary>
        ///     This helper method allows you to pass raw     SFXR string data to the sound chip for playback. It works just
        ///     like the normal PlaySound() API but accepts a string instead of a sound ID. Calling PlayRawSound() could
        ///     be expensive since the sound effect data is not cached by the engine. It is mostly used for sound effects
        ///     in tools and shouldn't be called when playing a game.
        /// </summary>
        /// <param name="data">Raw string data representing SFXR sound properties in a comma-separated list.</param>
        /// <param name="channel">
        ///     The channel the sound should play back on. Channel 0 is set by default.
        /// </param>
        /// <param name="frequency">
        ///     An optional float argument to change the frequency of the raw sound. The default setting is 0.1266.
        /// </param>
        public void PlayRawSound(string data, int channelID = 0, float frequency = 0.1266f)
        {
            var channel = Channels[channelID];

            channel.Play(new SoundData("untitled", data), frequency);
        }


        public void AddSample(string name, byte[] bytes)
        {
            // Add the wav sample to the sound bank
            if (soundBank.ContainsKey(name))
                soundBank[name] = bytes;
            else
                soundBank.Add(name, bytes);
        }

        /// <summary>
        ///     Goes through the sounds and the sound bank and adds the sample byte data
        /// </summary>
        public void RefreshSamples()
        {
            for (var i = 0; i < TotalSounds; i++)
            {
                var name = Sounds[i].name;
                Sounds[i].bytes = soundBank.ContainsKey(name) ? soundBank[name] : null;
            }
        }
    }
}