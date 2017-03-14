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
// 

using System;
using PixelVisionSDK.Utils;

namespace PixelVisionSDK
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

        public int totalNotes
        {
            set
            {
                var total = tracks.Length;
                for (var i = 0; i < total; i++)
                {
                    tracks[i].totalNotes = value;
                }
            }
        }

        public SongData(string name = "Untitled", int tracks = 4)
        {
            Reset(name);
            totalTracks = tracks;
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
                            tracks[i] = CreateNewTrack();
                            tracks[i].sfxID = i;
                        }
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