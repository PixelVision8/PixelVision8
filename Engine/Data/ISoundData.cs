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

namespace PixelVision8.Engine
{

    /// <summary>
    ///     The ISoundData interface represents a basic API for working
    ///     with sound objects in the PixelVisionSDK. Implement this
    ///     Interface with access to sound data to use it inside of
    ///     games and the MusicChip.
    /// </summary>
    public interface ISoundData
    {

        string name { get; set; }
        bool playing { get;}

        /// <summary>
        ///     Plays the sound at a specific frequency.
        /// </summary>
        /// <param name="frequency"></param>
        void Play(float? frequency = null);

        /// <summary>
        ///     Stops the current sound from playing
        /// </summary>
        void Stop();

        /// <summary>
        ///     Caches the sound file to improve performance
        /// </summary>
        void CacheSound();

        void UpdateSettings(string param);

        string ReadSettings();

        void Mutate(float value = 0.05f);
    }

}