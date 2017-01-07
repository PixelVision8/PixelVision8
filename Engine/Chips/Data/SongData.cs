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
// Jesse Freeman
// 

using System;
using System.Collections.Generic;
using System.Text;
using PixelVisionSDK.Engine.Utils;

namespace PixelVisionSDK.Engine.Chips.Data
{
    /// <summary>
    ///     The SongData class represents a collection of tracks and
    ///     meta data used by the MusicChip to play back ISoundData
    ///     in a sequence.
    /// </summary>
    public class SongData : AbstractData
    {
        protected int _speedInBPM = 120;

        /// <summary>
        ///     The song title
        /// </summary>
        public string songName = "Untitled";

        /// <summary>
        ///     All the tracks used in this loop
        /// </summary>
        public TrackData[] tracks = new TrackData[0];

        public SongData(string name = "Untitled")
        {
            Reset(name);
        }

        /// <summary>
        ///     How many beats per minute (eg 120).
        /// </summary>
        public int speedInBPM
        {
            get { return _speedInBPM; }
            set { _speedInBPM = value.Clamp(60, 480); }
        }

        /// <summary>
        ///     Total number of tracks in the song.
        /// </summary>
        public int totalTracks
        {
            get { return tracks.Length; }
            set
            {
                if (tracks.Length != value)
                {
                    Array.Resize(ref tracks, value);
                    var total = tracks.Length;
                    for (var i = 0; i < total; i++)
                    {
                        if (tracks[i] == null)
                        {
                            tracks[i] = new TrackData();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="data"></param>
        public override void DeserializeData(Dictionary<string, object> data)
        {
            if (data.ContainsKey("songName"))
                songName = (string) data["songName"];

            if (data.ContainsKey("speedInBPM"))
                speedInBPM = Convert.ToInt32((long) data["speedInBPM"]);

            if (data.ContainsKey("tracks"))
            {
                var tracksData = (List<object>) data["tracks"];
                totalTracks = tracksData.Count;

                for (var i = 0; i < totalTracks; i++)
                {
                    var trackData = tracksData[i];
                    var track = tracks[i];
                    track.DeserializeData((Dictionary<string, object>) trackData);
                    tracks[i] = track;
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="sb"></param>
        public override void CustomSerializedData(StringBuilder sb)
        {
            sb.Append("\"songName\":\"");
            sb.Append(songName);
            sb.Append("\",");

            sb.Append("\"speedInBPM\":");
            sb.Append(speedInBPM);
            sb.Append(",");

            sb.Append("\"tracks\":[");

            var total = tracks.Length;
            for (var i = 0; i < total; i++)
            {
                if (tracks[i] != null)
                {
                    sb.Append("{");

                    if (tracks[i] != null)
                    {
                        tracks[i].CustomSerializedData(sb);
                    }

                    sb.Append("}");

                    if (i < total - 1)
                    {
                        sb.Append(",");
                    }
                }
            }

            sb.Append("]");
        }

        /// <summary>
        ///     Reset the default values of the SongData instance
        /// </summary>
        /// <param name="name"></param>
        /// <param name="trackCount"></param>
        public void Reset(string name = "Untitled", int trackCount = 4)
        {
            songName = name;
            totalTracks = trackCount;
            foreach (var track in tracks)
            {
                track.Reset(true);
            }
        }
    }
}