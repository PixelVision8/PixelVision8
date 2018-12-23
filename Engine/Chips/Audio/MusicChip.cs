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
//using UnityEngine;

namespace PixelVisionSDK.Chips
{

    /// <summary>
    ///     The MusicChpip is a sequencer for playing back ISoundData. It
    ///     keeps track of playback time and moves through TrackData playing
    ///     each beat based on the supplied note frequency.
    ///     Loop = one set of 32 beats in X number of tracks. Stored in SongData class.
    ///     Song = Collection of loops for continuous playback. Think 'play list".
    /// </summary>
    public class MusicChip : AbstractChip, IUpdate
    {

        protected int _totalTracks;

        protected int currentLoop = -1;
        public bool loopSong;
        public int maxNoteNum = 127; // how many notes in these arrays below
        public int maxTracks = 8; // max number of instruments playing notes
        public float nextBeatTimestamp;
        protected float[] noteHZ; // a lookup table of all musical notes in Hz
        public int notesPerTrack = 32;
        public float[] noteStartFrequency; // same, but for sfxr frequency 0..1 range
        protected float noteTickS = 30.0f / 120.0f; // (30.0f/120.0f) = 120BPM eighth notes
        protected float noteTickSEven;
        protected float noteTickSOdd;
        public long sequencerBeatNumber;
        protected int sequencerLoopNum;
        public bool songCurrentlyPlaying;
        public TrackerData[] trackerDataCollection = new TrackerData[0];
        protected int songLoopCount = 0;

        protected float swingRhythmFactor = 0.7f;

        protected float time;
        public int tracksPerLoop = 8;
        private int[] loopPlayList;
        private int currentLoopID;
        
        public SongData[] songDataCollection = new SongData[0];

        public int CurrentLoopId
        {
            get { return currentLoop; }
        }

        /// <summary>
        ///     Total number of Loop stored in the music chip. There is a maximum
        ///     of 96 loops.
        /// </summary>
        public int totalLoops
        {
            get { return trackerDataCollection.Length; }
            set
            {
                if (trackerDataCollection.Length != value)
                {
                    Array.Resize(ref trackerDataCollection, value.Clamp(1, 96));
                    var total = trackerDataCollection.Length;
                    for (var i = 0; i < total; i++)
                    {
                        var sondData = trackerDataCollection[i];
                        if (sondData == null)
                            trackerDataCollection[i] = CreateNewTrackerData("Untitled" + i, maxTracks);
                        else
                            sondData.totalTracks = totalTracks;
                    }
                }
            }
        }

        public int totalNotes
        {
            get
            {
                return maxNoteNum;

            }
            set
            {
                if (maxNoteNum == value)
                    return;

                var total = totalLoops;
                for (var i = 0; i < total; i++)
                    trackerDataCollection[i].totalNotes = value;
            }
        }

        public int totalTracks
        {
            get { return _totalTracks; }
            set
            {
                value = value.Clamp(1, maxTracks);

                var total = trackerDataCollection.Length;
                for (var i = 0; i < total; i++)
                    trackerDataCollection[i].totalTracks = value;

                _totalTracks = value;
            }
        }

        /// <summary>
        ///     The active song's data that was loaded into memory.
        /// </summary>
        public TrackerData activeTrackerData
        {
            get
            {
                if (trackerDataCollection == null)
                    return null;

                return trackerDataCollection[currentLoop];
            }
        }

        protected SoundChip soundChip
        {
            get { return engine.soundChip; }
        }

        /// <summary>
        ///     Updates the sequencer if it is in playback mode. This will
        ///     move the play head to the next beat and play that note.
        /// </summary>
        /// <param name="timeDelta"></param>
        public void Update(float timeDelta)
        {
            time += timeDelta;

            //TODO need to make sure this still actually works after removing Time.time reference
            if (songCurrentlyPlaying)
            
                if (time >= nextBeatTimestamp)
                {
                    nextBeatTimestamp = time + (sequencerBeatNumber % 2 == 1 ? noteTickSOdd : noteTickSEven);
                    OnBeat();
                }
        }

        public virtual TrackerData CreateNewTrackerData(string name, int tracks = 4)
        {
            return new TrackerData(name, tracks);
        }

        /// <summary>
        ///     This method sets up the sequencer and all of its values.
        /// </summary>
        public override void Configure()
        {
            engine.musicChip = this;

            //engine.chipManager.AddToUpdateList(this);
            totalLoops = 16;
            maxTracks = 4;
            totalTracks = maxTracks;

            // Setup the sequencer values

            var a = 440.0f; // a is 440 hz...
            noteHZ = new float[maxNoteNum];
            noteStartFrequency = new float[maxNoteNum];
            noteStartFrequency[0] = 0f; // since we never set it below
            var SR = 44100.0f; // hmm preRenderBitrate? nah
            float hertz;

            for (var x = 0; x < maxNoteNum; ++x)
            {
                // what Hz is a particular musical note? (eg A#)
                hertz = a / 32.0f * (float) Math.Pow(2.0f, (x - 9.0f) / 12.0f);
                noteHZ[x] = hertz; // this appears to be correct: C = 60 = 261.6255Hz

                // derive the SFXR sine wave frequency to play this Hz
                // FIXME: this sounds about a semitone too high compared to real life piano!
                // note_startFrequency[x] = Mathf.Sqrt(hertz / SR * 100.0f / 8.0f - 0.001f);
                // maybe the algorithm assumes 1 based array etc?
                if (x < 126) // let's just hack in one semitone lower sounds (but not overflow array)
                    noteStartFrequency[x + 1] = (float) Math.Sqrt(hertz / SR * 100.0f / 8.0f - 0.001f) - 0.0018f;

                // last num is a hack using my ears to "tune"
            }
        }

        /// <summary>
        ///     Loads a song into memory. This needs to be called before trying to
        ///     play back a song or it will fail.
        /// </summary>
        /// <param name="id"></param>
        public void LoadPattern(int id)
        {   
//            Debug.Log("Load Song " + id);
            // Rewind the playhead
            sequencerBeatNumber = 0;
            
            // Update the current loop
            currentLoop = id;
            
            // Double check the loop's length
            UpdateNoteTickLengths();
            
            // Updates the tracks per loop
            tracksPerLoop = activeTrackerData.tracks.Length;
            
            // Update the music notes?
            UpdateMusicNotes();
            
            
        }

        /// <summary>
        ///     Allows you to string songs together into longer ones. Each
        ///     ID will be played in order and you can also have the entire
        ///     set loop.
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="loop"></param>
        public void PlayPatterns(int[] ids, bool loop = false, int startAt = 0)
        {

            loopPlayList = ids;
            
            
            loopSong = loop;
            RewindSong();

            //            loopPlayList = ids;
            currentLoopID = startAt;
//            songLoopCount = ids.Length;

            LoadPattern(loopPlayList[0]);
            
            if (songCurrentlyPlaying)
                songCurrentlyPlaying = false;
            else
                songCurrentlyPlaying = true;
            
            //TODO not implemented yet
        }

        public void ResetTracker()
        {
            activeTrackerData.Reset();
            sequencerBeatNumber = 0;
            UpdateMusicNotes();
        }

        /// <summary>
        ///     Run when a beat in song occurs: time for more sounds
        /// </summary>
        protected void OnBeat()
        {

            if (sequencerBeatNumber >= notesPerTrack) // at end of a loop?
            {
                // Finished Loop;
                
                // Increase the next loop value
                sequencerLoopNum++;
    
                if (sequencerLoopNum >= loopPlayList.Length)
                {
                    if (loopSong)
                    {    
                        sequencerLoopNum = 0;
                    }
                    else
                    {
                        songCurrentlyPlaying = false;
                        return;
                    }
                }
                
                // Load the next song in the playlist
                LoadPattern(loopPlayList[sequencerLoopNum]);
               
            }

            var total = activeTrackerData.tracks.Length;

            // loop through each oldInstruments track
            for (var trackNum = 0; trackNum < total; trackNum++)
            {
                var sfxId = activeTrackerData.tracks[trackNum].sfxID;
                // what note is it?
                var gotANote = activeTrackerData.tracks[trackNum].notes[sequencerBeatNumber % notesPerTrack];

//                var instrument = soundChip.ReadChannel(trackNum);

//                if (instrument != null)
                if (gotANote > 0 && gotANote < maxNoteNum)
                {
                    var frequency = noteStartFrequency[gotANote];

                    //$CTK midi num offset fix -1]; // -1 to account for 0 based array
                    soundChip.PlaySound(sfxId, trackNum, frequency);
                    //;//.Play(frequency);
                    
                    
                }
            }

            sequencerBeatNumber++; // next beat will use array index +1
        }

        public void UpdateNoteTickLengths()
        {
            noteTickS = 30.0f / activeTrackerData.speedInBPM; // (30.0f/120.0f) = 120BPM eighth notes [tempo]
            noteTickSOdd = noteTickS * swingRhythmFactor; // small beat
            noteTickSEven = noteTickS * 2 - noteTickSOdd; // long beat
        }

        /// <summary>
        ///     Rewinds the sequencer to the beginning of the currently loaded song.
        /// </summary>
        public void RewindSong()
        {
            songCurrentlyPlaying = false; // stop
            sequencerLoopNum = 0;
            sequencerBeatNumber = 0;
        }

        /// <summary>
        ///     Stops the sequencer.
        /// </summary>
        public void StopSong()
        {
            songCurrentlyPlaying = false;
        }

        /// <summary>
        ///     Toggles the current playback state of the sequencer. If the song
        ///     is playing it will pause, if it is paused it will play
        /// </summary>
        public void PauseSong() // unused
        {
            songCurrentlyPlaying = !songCurrentlyPlaying;
        }

        protected void UpdateMusicNotes()
        {
            for (var x = 0; x < notesPerTrack; x++)
                if (tracksPerLoop < activeTrackerData.tracks.Length)
                    for (var y = 0; y < tracksPerLoop; y++)
                        if (activeTrackerData.tracks[y].notes.Length != notesPerTrack)
                            Array.Resize(ref activeTrackerData.tracks[y].notes, notesPerTrack);
        }

    }

}