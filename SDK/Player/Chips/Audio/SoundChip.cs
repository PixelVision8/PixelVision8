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
using PixelVision8.Player;
using PixelVision8.Player.Audio;

namespace PixelVision8.Player
{

    public partial interface IPlayerChips
    {
        /// <summary>
        ///     The Sound Chip stores and manages playback of sound effects in the
        ///     game engine. This property offers direct access to it.
        /// </summary>
        ISoundChip SoundChip { get; set; }
    }
    
    /// <summary>
    ///     The <see cref="SfxrSoundChip" /> is responsible for playing back sound
    ///     effects in the engine. It's powered by SFxr.
    /// </summary>
    public class SoundChip : AbstractChip, ISoundChip
    {
        protected readonly Dictionary<string, byte[]> soundBank = new Dictionary<string, byte[]>();
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
                value = Utilities.Clamp(value, 1, 5);
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
                value = Utilities.Clamp(value, 1, 96);

                Array.Resize(ref Sounds, value);

                for (var i = 0; i < value; i++)
                    if (Sounds[i] == null)
                        Sounds[i] = CreateSoundData("Untitled" + i.ToString("D2"));
            }
        }

        public virtual SoundData CreateSoundData(string name)
        {
            return new SoundData(name);
        }

        /// <summary>
        ///     This stub methods is designed to be overridden with a Factory to
        ///     create new sound instances that implement the ISoundData interface.
        /// </summary>
        /// <returns></returns>
        public virtual IChannel CreateSoundChannel()
        {
            return new SoundChannel();
        }

        /// <summary>
        ///     Configures the <see cref="SfxrSoundChip" /> by registering itself with
        ///     the engine and setting up the default values for total
        ///     <see cref="Sounds" /> and total channels.
        /// </summary>
        public override void Configure()
        {
            Player.SoundChip = this;
            TotalSounds = 16;
            totalChannels = 5;
        }

        /// <summary>
        ///     This method plays back a sound on a specific channel. The
        ///     <see cref="SfxrSoundChip" /> has a limit of active <see cref="Channels" />
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

            channelID = Utilities.Clamp(channelID, 0, totalChannels - 1);

            var channel = Channels[channelID];

            channel?.Stop();

            //            channel = sounds[index];

            channel.Play(Sounds[index], frequency);
        }

        public void PlaySound(string name, int channelID = 0, float? frequency = null)
        {
            for (int i = 0; i < TotalSounds; i++)
            {
                if (Sounds[i].name == name)
                {
                    PlaySound(i, channelID, frequency);
                }
            }
        }

        public bool IsChannelPlaying(int channelID)
        {
            return Channels[channelID] != null && Channels[channelID].Playing;
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

namespace PixelVision8.Player
{
    public partial class PixelVision
    {
        public ISoundChip SoundChip { get; set; }
    }
}