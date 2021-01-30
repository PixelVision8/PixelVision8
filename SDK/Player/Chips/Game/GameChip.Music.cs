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

using System.Collections.Generic;

namespace PixelVision8.Player
{
    /// <summary>
    ///     The GameChip represents the foundation of a game class
    ///     with all the logic it needs to work correctly in the PixelVisionEngine.
    ///     The AbstractChip class manages configuring the game when created via the
    ///     chip life-cycle. The engine manages the game's state, the game's own life-cycle and
    ///     serialization/deserialization of the game's data.
    /// </summary>
    public partial class GameChip
    {
        private MusicChip MusicChip => Player.MusicChip;

        /// <summary>
        ///     Plays a sing by it's ID. You can pass in a start position for it to being at a specific pattern ID in the song.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="loop"></param>
        /// <param name="startAt"></param>
        public void PlaySong(int id, bool loop = true, int startAt = 0)
        {
            MusicChip.PlaySong(id, loop, startAt);
        }

        /// <summary>
        ///     This helper method allows you to automatically load a set of patterns as a complete
        ///     song and plays them back. You can also define if the tracks should loop when they
        ///     are done playing.
        /// </summary>
        /// <param name="loopIDs">
        ///     An array of loop IDs to playback as a single song.
        /// </param>
        /// <param name="loop">
        ///     A bool that determines if the song should loop back to the first ID when it is
        ///     done playing.
        /// </param>
        public void PlayPattern(int id, bool loop = true)
        {
            MusicChip.PlayPatterns(new[] {id}, loop);
        }

        /// <summary>
        ///     This helper method allows you to automatically load a set of patterns as a complete
        ///     song and plays them back. You can also define if the tracks should loop when they
        ///     are done playing.
        /// </summary>
        /// <param name="loopIDs">
        ///     An array of loop IDs to playback as a single song.
        /// </param>
        /// <param name="loop">
        ///     A bool that determines if the song should loop back to the first ID when it is
        ///     done playing.
        /// </param>
        public void PlayPatterns(int[] loopIDs, bool loop = true)
        {
            MusicChip.PlayPatterns(loopIDs, loop);
        }

        /// <summary>
        ///     Returns a dictionary with information about the current state of the music chip.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, int> SongData()
        {
            return MusicChip.songData;
        }

        /// <summary>
        ///     Toggles the current playback state of the sequencer. If the song
        ///     is playing it will pause, if it is paused it will play.
        /// </summary>
        public void PauseSong()
        {
            MusicChip.PauseSong();
        }

        /// <summary>
        ///     Stops the sequencer.
        /// </summary>
        public void StopSong()
        {
            MusicChip.StopSong();
        }

        /// <summary>
        ///     Rewinds the sequencer to the beginning of the currently loaded song. You can define
        ///     the position in the loop and the loop where playback should begin. Calling this method
        ///     without any arguments will simply rewind the song to the beginning of the first loop.
        /// </summary>
        /// <param name="position">
        ///     Position in the loop to start playing at.
        /// </param>
        /// <param name="patternID">
        ///     The loop to rewind too.
        /// </param>
        public void RewindSong(int position = 0, int patternID = 0)
        {
            //TODO need to add in better support for rewinding a song across multiple loops
            MusicChip.RewindSong();
        }
    }
}