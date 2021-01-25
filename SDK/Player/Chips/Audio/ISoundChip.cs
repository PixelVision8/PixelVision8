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

namespace PixelVision8.Player
{

    public interface ISoundChip
    {
        /// <summary>
        ///     The total number of <see cref="Channels" /> available for playing
        ///     back sounds.
        /// </summary>
        int totalChannels { get; set; }

        /// <summary>
        ///     The total number of <see cref="Sounds" /> in the collection.
        /// </summary>
        int TotalSounds { get; set; }

        /// <summary>
        ///     This method plays back a sound on a specific channel. The
        ///     SoundChip has a limit of active <see cref="SfxrSoundChip.Channels" />
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
        void PlaySound(int index, int channelID = 0, float? frequency = null);

        void PlaySound(string name, int channelID = 0, float? frequency = null);

        bool IsChannelPlaying(int channelID);
        void StopSound(int channel);
        void Shutdown();
        void AddSample(string name, byte[] bytes);

        /// <summary>
        ///     Goes through the sounds and the sound bank and adds the sample byte data
        /// </summary>
        void RefreshSamples();

    }
}