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

        protected int currentSong;
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
        protected SongData[] songDataCollection = new SongData[0];
        protected int songLoopCount = 0;

        protected float swingRhythmFactor = 0.7f;

        //1.0f;//0.66666f; // how much "shuffle" - turnaround on the offbeat triplet

        protected float time;
        public int tracksPerLoop = 8;

        /// <summary>
        ///     Total number of Loop stored in the music chip. There is a maximum
        ///     of 96 loops.
        /// </summary>
        public int totalLoops
        {
            get { return songDataCollection.Length; }
            set
            {
                if (songDataCollection.Length != value)
                {
                    Array.Resize(ref songDataCollection, value.Clamp(1, 96));
                    var total = songDataCollection.Length;
                    for (var i = 0; i < total; i++)
                    {
                        var sondData = songDataCollection[i];
                        if (sondData == null)
                            songDataCollection[i] = CreateNewSongData("Untitled" + i, maxTracks);
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
                ;
            }
            set
            {
                if (maxNoteNum == value)
                    return;

                var total = totalLoops;
                for (var i = 0; i < total; i++)
                    songDataCollection[i].totalNotes = value;
            }
        }

        public int totalTracks
        {
            get { return _totalTracks; }
            set
            {
                value = value.Clamp(1, maxTracks);

                var total = songDataCollection.Length;
                for (var i = 0; i < total; i++)
                    songDataCollection[i].totalTracks = value;

                _totalTracks = value;
            }
        }

        /// <summary>
        ///     The active song's data that was loaded into memory.
        /// </summary>
        public SongData activeSongData
        {
            get
            {
                if (songDataCollection == null)
                    return null;

                return songDataCollection[currentSong];
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

        public virtual SongData CreateNewSongData(string name, int tracks = 4)
        {
            return new SongData(name, tracks);
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
        public void LoadSong(int id)
        {
            currentSong = id;
            UpdateNoteTickLengths();
            tracksPerLoop = activeSongData.tracks.Length;
            UpdateMusicNotes();
            LoadInstruments(activeSongData);
        }

        /// <summary>
        ///     Plays back a song and allows you to pass in a value to loop
        ///     the song or have it stop when it reaches the end.
        /// </summary>
        /// <param name="loop"></param>
        public void PlaySong(bool loop = false)
        {
            loopSong = loop;
            RewindSong();

            if (songCurrentlyPlaying)
                songCurrentlyPlaying = false;
            else
                songCurrentlyPlaying = true;
        }

        /// <summary>
        ///     Allows you to string songs together into longer ones. Each
        ///     ID will be played in order and you can also have the entire
        ///     set loop.
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="loop"></param>
        public void PlaySongs(int[] ids, bool loop = false)
        {
            //TODO not implemented yet
        }

        public void ResetSong()
        {
            activeSongData.Reset();
            LoadInstruments(activeSongData);
            sequencerBeatNumber = 0;
            UpdateMusicNotes();
        }

        /// <summary>
        ///     Run when a beat in song occurs: time for more sounds
        /// </summary>
        protected void OnBeat()
        {
            // just started a song?
            if (sequencerBeatNumber == 0 && sequencerLoopNum == 0)
            {
                // starting a song

                LoadInstruments(activeSongData);
            }
            else if (sequencerBeatNumber >= notesPerTrack) // at end of a loop?
            {
                // Finished Loop;

                sequencerLoopNum++;
                if (sequencerLoopNum >= songLoopCount)
                    if (loopSong)
                    {
                        sequencerLoopNum = 0;
                    }
                    else
                    {
                        songCurrentlyPlaying = false;
                        return;
                    }

                sequencerBeatNumber = 0;

                // the next loop might have different instruments
                LoadInstruments(activeSongData);
            }

            var total = activeSongData.tracks.Length;
            // loop through each oldInstruments track
            for (var trackNum = 0; trackNum < total; trackNum++)
            {
                // what note is it?
                var gotANote = activeSongData.tracks[trackNum].notes[sequencerBeatNumber % notesPerTrack];

                var instrument = soundChip.ReadChannel(trackNum);

                if (instrument != null)
                    if (gotANote > 0 && gotANote < maxNoteNum && instrument != null)
                    {
                        var frequency = noteStartFrequency[gotANote];

                        //$CTK midi num offset fix -1]; // -1 to account for 0 based array
                        instrument.Play(frequency);
                    }
            }

            sequencerBeatNumber++; // next beat will use array index +1
        }

        public void LoadInstruments(SongData song)
        {
            var trackCount = song.tracks.Length;

            var channels = soundChip.totalChannels;

            for (var trackNum = 0; trackNum < trackCount; trackNum++)
                if (trackNum < channels)
                {
                    var sfxID = song.tracks[trackNum].sfxID;

                    soundChip.LoadSound(sfxID, trackNum);
                }
        }

        public void UpdateNoteTickLengths()
        {
            noteTickS = 30.0f / activeSongData.speedInBPM; // (30.0f/120.0f) = 120BPM eighth notes [tempo]
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
                if (tracksPerLoop < activeSongData.tracks.Length)
                    for (var y = 0; y < tracksPerLoop; y++)
                        if (activeSongData.tracks[y].notes.Length != notesPerTrack)
                            Array.Resize(ref activeSongData.tracks[y].notes, notesPerTrack);
        }
    }
}