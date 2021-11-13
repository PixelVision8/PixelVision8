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
    /// <summary>
    ///     The SoundChip is responsible for playing back sound
    ///     effects in the engine. It's powered by SFxr.
    /// </summary>
    public partial class SoundChip
    {
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

            ReadSound(index).param = param;
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
            if (id < 0 || id >= TotalSounds)
                return null;

            if (Sounds[id] == null)
                Sounds[id] = CreateSoundData("Untitled" + id.ToString("D2"));
            
            return Sounds[id];
        }

        public string ReadLabel(int id)
        {
            return ReadSound(id).name;
        }

        public void UpdateLabel(int id, string name)
        {
            ReadSound(id).name = name;
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
    }
}