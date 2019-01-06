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

using System;
using PixelVisionSDK.Utils;

namespace PixelVisionSDK
{

    /// <summary>
    ///     A track is a collection of ISoundData that is played
    ///     back in a specific sequence. The track uses notes to
    ///     play each sound back at a specific frequency.
    /// </summary>
    public class TrackData : AbstractData
    {

        /// <summary>
        ///     Total number of notes in a single track.
        /// </summary>
        public int maxNotes = 32;

        /// <summary>
        ///     All the notes in this track, played one per beat.
        /// </summary>
        public int[] notes = new int[0];

        /// <summary>
        ///     ID of SFX to use for the instrument
        /// </summary>
        public int sfxID;

        public bool mute;

        /// <summary>
        ///     Create a new TrackData instance by supplying the
        ///     number of notes. The default value is 32.
        /// </summary>
        /// <param name="maxNotes"></param>
        public TrackData(int maxNotes = 32)
        {
            this.maxNotes = maxNotes;
            Reset(false);
        }

        /// <summary>
        ///     Returns the total number of notes in the track.
        /// </summary>
        public int totalNotes
        {
            get { return notes.Length; }
            set { Array.Resize(ref notes, value.Clamp(0, maxNotes)); }
        }

        /// <summary>
        ///     Reset the track and clear all the notes.
        /// </summary>
        /// <param name="autoClear"></param>
        public void Reset(bool autoClear = true)
        {
            totalNotes = maxNotes;
            if (autoClear)
                Clear();
        }

        /// <summary>
        ///     Clears all of the note values.
        /// </summary>
        public void Clear()
        {
            for (var i = 0; i < totalNotes; i++)
                notes[i] = 0;
        }

    }

}