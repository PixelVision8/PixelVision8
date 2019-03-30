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
            if (synth != null)
            {
                synth.CacheSound();
                synth.UpdateSettings(param);
            }
        }
        
        /// <summary>
        ///     This allows you to feed soudn effect data directly to the sound chip and play it on
        ///     on a specific channel. Use this if you don't have a sound effect stored in the sound
        ///     chip or for hard coded sounds. These sounds are not cached by the engine so it may
        ///     cause performance issues when playing back at run time.
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="data"></param>
        public void PlayRawSound(string data, int channelID = 0, float frequency = 0.1266f)
        {
            var tmpSoundData = CreateEmptySoundData("Raw Sound") as SfxrSoundData;
            tmpSoundData.UpdateSettings(data);

            var channel = channels[channelID];

            if (channel != null)
            {
                channel.Stop();
            }

            tmpSoundData.Play(frequency);

        }

    }

}