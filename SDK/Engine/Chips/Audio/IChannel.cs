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

namespace PixelVision8.Engine.Audio
{
    /// <summary>
    ///     The ISoundData interface represents a basic API for working
    ///     with sound objects in the PixelVisionSDK. Implement this
    ///     Interface with access to sound data to use it inside of
    ///     games and the MusicChip.
    /// </summary>
    public interface IChannel
    {
        bool Playing { get; }

        /// <summary>
        ///     Plays the sound at a specific frequency.
        /// </summary>
        /// <param name="soundData"></param>
        /// <param name="frequency"></param>
        void Play(SoundData soundData, float? frequency = null);

        /// <summary>
        ///     Stops the current sound from playing
        /// </summary>
        void Stop();

        /// <summary>
        ///     Handles setting a wave type to the channel. This will
        ///     lock the channel when manually set using this API. To
        ///     unlock, pass in WaveShape.None.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        WaveType ChannelType(WaveType? type);
    }
}