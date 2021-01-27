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

using Microsoft.Xna.Framework;
using PixelVision8.Runner;
using System;
using System.Linq;
using System.Text;

namespace PixelVision8.Player
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

        public bool mute;

        /// <summary>
        ///     All the notes in this track, played one per beat.
        /// </summary>
        public int[] notes = new int[0];

        /// <summary>
        ///     ID of SFX to use for the instrument
        /// </summary>
        public int sfxID;

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
            get => notes.Length;
            set => Array.Resize(ref notes, MathHelper.Clamp(value, 0, maxNotes));
        }

        /// <summary>
        ///     Reset the track and clear all the notes.
        /// </summary>
        /// <param name="autoClear"></param>
        public void Reset(bool autoClear = true)
        {
            totalNotes = maxNotes;
            if (autoClear) Clear();
        }

        /// <summary>
        ///     Clears all of the note values.
        /// </summary>
        public void Clear()
        {
            for (var i = 0; i < totalNotes; i++) notes[i] = 0;
        }

        public string SerializeData()
        {
            var sb = new StringBuilder();
            JsonUtil.GetLineBreak(sb);
            sb.Append("{");
            JsonUtil.GetLineBreak(sb, 1);

            sb.Append("\"SfxId\":");
            sb.Append(sfxID);
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 1);

            sb.Append("\"notes\":[");

            sb.Append(string.Join(",", notes.Select(x => x.ToString()).ToArray()));
            sb.Append("]");

            JsonUtil.GetLineBreak(sb);
            sb.Append("}");

            return sb.ToString();
        }
    }
}