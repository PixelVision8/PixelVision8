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

using PixelVision8.Runner.Data;

namespace PixelVision8.Engine.Chips
{
    public class SfxrSoundChip : SoundChip
    {
        /// <summary>
        ///     This stub methods is designed to be overridden with a Factory to
        ///     create new sound instances that implement the ISoundData interface.
        /// </summary>
        /// <returns></returns>
        public override ISoundData CreateEmptySoundData(string name = "Untitled")
        {
            return new SfxrSoundData(name);
        }

        /// <summary>
        ///     Updates a sound in the collection.
        /// </summary>
        /// <param name="index">The index to update the sound at.</param>
        /// <param name="param">
        ///     A string representing the synth properties.
        /// </param>
        public override void UpdateSound(int index, string param)
        {
            var synth = sounds[index] as SfxrSoundData;
            synth?.UpdateSettings(param);
        }

        /// <summary>
        ///     This helper method allows you to pass raw SFXR string data to the sound chip for playback. It works just
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
            var tmpSoundData = CreateEmptySoundData("Raw Sound") as SfxrSoundData;
            tmpSoundData.UpdateSettings(data);

            var channel = channels[channelID];

            if (channel != null) channel.Stop();

            tmpSoundData.Play(frequency);
        }
    }
}