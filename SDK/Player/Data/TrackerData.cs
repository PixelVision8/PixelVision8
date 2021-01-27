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
using System.Text;

namespace PixelVision8.Player
{
    /// <summary>
    ///     The SongData class represents a collection of tracks and
    ///     meta data used by the MusicChip to play back ISoundData
    ///     in a sequence.
    /// </summary>
    public class TrackerData : AbstractData
    {
        protected int _speedInBPM = 120;

        /// <summary>
        ///     All the tracks used in this loop
        /// </summary>
        public TrackData[] tracks = new TrackData[0];

        public TrackerData(string name = "Untitled", int tracks = 5)
        {
            Reset();
            totalTracks = tracks;
        }

        public int totalNotes
        {
            set
            {
                var total = tracks.Length;
                for (var i = 0; i < total; i++) tracks[i].totalNotes = value;
            }
        }

        /// <summary>
        ///     How many beats per minute (eg 120).
        /// </summary>
        public int speedInBPM
        {
            get => _speedInBPM;
            set => _speedInBPM = MathHelper.Clamp(value, 1, 480);
        }

        /// <summary>
        ///     Total number of tracks in the song.
        /// </summary>
        public int totalTracks
        {
            get => tracks.Length;
            set
            {
                if (tracks.Length != value)
                {
                    Array.Resize(ref tracks, value);
                    var total = tracks.Length;
                    for (var i = 0; i < total; i++)
                        if (tracks[i] == null)
                        {
                            tracks[i] = CreateNewTrack();
                            tracks[i].sfxID = i;
                        }
                }
            }
        }

        public virtual TrackData CreateNewTrack()
        {
            return new TrackData();
        }

        /// <summary>
        ///     Reset the default values of the SongData instance
        /// </summary>
        /// <param name="trackCount"></param>
        public void Reset(int trackCount = 5)
        {
            //            songName = name;
            totalTracks = trackCount;
            foreach (var track in tracks) track.Reset();
        }

        public string SerializeData()
        {
            var sb = new StringBuilder();
            JsonUtil.GetLineBreak(sb);
            sb.Append("{");
            JsonUtil.GetLineBreak(sb, 1);

            //            sb.Append("\"patternName\":\"");
            //            sb.Append(songName);
            //            sb.Append("\",");
            //            JsonUtil.GetLineBreak(sb, 1);

            sb.Append("\"speedInBPM\":");
            sb.Append(speedInBPM);
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 1);

            sb.Append("\"tracks\":");
            JsonUtil.GetLineBreak(sb, 1);
            sb.Append("[");
            JsonUtil.indentLevel++;
            var total = tracks.Length;
            for (var i = 0; i < total; i++)
                if (tracks[i] != null)
                {
                    JsonUtil.indentLevel++;
                    var track = tracks[i];

                    if (track != null) sb.Append(track.SerializeData());

                    if (i < total - 1) sb.Append(",");

                    JsonUtil.indentLevel--;
                }

            JsonUtil.indentLevel--;
            JsonUtil.GetLineBreak(sb, 1);
            sb.Append("]");

            JsonUtil.GetLineBreak(sb);
            sb.Append("}");

            return sb.ToString();
        }
    }
}