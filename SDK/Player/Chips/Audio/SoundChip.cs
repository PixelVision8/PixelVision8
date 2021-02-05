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
using PixelVision8.Player;
using PixelVision8.Player.Audio;

namespace PixelVision8.Player
{
    
    /// <summary>
    ///     The <see cref="SfxrSoundChip" /> is responsible for playing back sound
    ///     effects in the engine. It's powered by SFxr.
    /// </summary>
    public class SoundChip : AbstractChip
    {
        // private readonly Dictionary<string, byte[]> _soundBank = new Dictionary<string, byte[]>();
        protected SoundChannel[] Channels = new SoundChannel[0];
        protected SoundData[] Sounds;

        /// <summary>
        ///     The total number of <see cref="Channels" /> available for playing
        ///     back sounds.
        /// </summary>
        public int TotalChannels
        {
            get => Channels.Length;
            set
            {
                value = MathHelper.Clamp(value, 1, 5);
                Array.Resize(ref Channels, value);
                
                // There should never be an empty sound channel so loop through them and make sure one is created
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

                // for (var i = 0; i < value; i++)
                //     if (Sounds[i] == null)
                //         Sounds[i] = CreateSoundData("Untitled" + i.ToString("D2"));
            }
        }

        public virtual SoundData CreateSoundData(string name, byte[] bytes = null)
        {
            return new SoundData(name, bytes);
        }

        /// <summary>
        ///     This stub methods is designed to be overridden with a Factory to
        ///     create new sound instances that implement the ISoundData interface.
        /// </summary>
        /// <returns></returns>
        public virtual SoundChannel CreateSoundChannel()
        {
            return new SoundChannel();
        }

        /// <summary>
        ///     Configures the <see cref="SfxrSoundChip" /> by registering itself with
        ///     the engine and setting up the default values for total
        ///     <see cref="Sounds" /> and total channels.
        /// </summary>
        protected override void Configure()
        {
            Player.SoundChip = this;
            TotalSounds = 16;
            TotalChannels = 5;
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
        /// <param name="channelId"></param>
        public void PlaySound(int index, int channelId = 0, float? frequency = null)
        {
            if (index < 0 || index >= Sounds.Length || Sounds[index] == null) 
                return;
            
            channelId = MathHelper.Clamp(channelId, 0, TotalChannels - 1);

            Channels[channelId].Play(Sounds[index], frequency);

        }

        protected int FindNextEmptySound()
        {

            for (int i = 0; i < TotalSounds; i++)
            {
                if (Sounds[i] == null)
                    return i;
            }

            return -1;

        }

        protected int FindSoundId(string name)
        {
            for (int i = 0; i < TotalSounds; i++)
            {
                if (Sounds[i] != null && Sounds[i].name == name)
                {
                    return i;
                }
            }

            return - 1;
        }

        public void PlaySound(string name, int channelID = 0, float? frequency = null) => PlaySound(FindSoundId(name), channelID, frequency);
        
        public bool IsChannelPlaying(int channelId)
        {
            return Channels[channelId] != null && Channels[channelId].Playing;
        }

        public void StopSound(int channel)
        {
            if (Channels[channel] != null) Channels[channel].Stop();
        }

        public void AddSample(string name, byte[] bytes)
        {

            var id = FindSoundId(name);

            if (id == -1)
                id = FindNextEmptySound();
            
            if(id == -1)
                return;

            Sounds[id] = CreateSoundData(name, bytes);
            
        }
        
        public override void Shutdown()
        {
            foreach (var channel in Channels)
                if (channel.Playing)
                    channel.Stop();

            base.Shutdown();
        }

    }
}

namespace PixelVision8.Player
{
    public partial class PixelVision
    {
        public SoundChip SoundChip { get; set; }
    }
}