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

using PixelVision8.Player;
using System;
using System.Collections.Generic;

namespace PixelVision8.Editor
{
    public enum InstrumentType
    {
        Melody,
        Harmony,
        Bass,
        Drums,
        Lead,
        Pad,
        Snare,
        Kick,
        Random,
        None
    }

    public class TrackSettings
    {
        private readonly Point[] defaultOctaves =
        {
            new Point(6, 6), // MELODY
            new Point(6, 7), // HARMONY
            new Point(3, 4), // BASS
            new Point(6, 6), // DRUMS
            new Point(6, 7), // LEAD
            new Point(5, 6), // PAD
            new Point(6, 7), // SNARE
            new Point(6, 6), // KICK
            new Point(3, 7), // RAND
            new Point(3, 7) // NONE
        };

        private readonly Random r = new Random();

        private InstrumentType _instrumentType;

        // MELODY: square
        // HARMONY: square
        // BASS: triangle
        // DRUMS: noise
        // LEAD: sawtooth w vibrato and decay
        // PAD: smooth fade in + out
        // SFX: noise with reverb (snare?)
        // KICK: ooomph

        // TODO this should be part of the generator not each track
        public string[] instrumentTemplates =
        {
            "0,,.2,,.2,.1266,,,,,,,,,,,,,1,,,,,.5", //(MELODY,+0) = 
            "0,,.01,,.509,.1266,,,,,,,,.31,,,,,1,,,.1,,.5", // (HARMONY,+1)
            "4,,.01,,.509,.1266,,,,,,,,.31,,,,,1,,,.1,,1", //(BASS,-2)
            "3,,.01,,.209,.1668,,,,,,,,.31,,,,,.3,,,.1,,.5", // (DRUMS,+0)
            "4,.6,.01,,.609,.1347,,,,,.2,,,.31,,,,,1,,,.1,,.5", //(LEAD,+1)
            "4,.5706,.4763,.0767,.8052,.1266,,,-.002,,.1035,.2062,,,-.0038,.8698,-.0032,,.6377,.1076,,.0221,.0164,.5", // (PAD,+0)
            "3,.032,.11,.6905,.4,.1668,.0412,-.2434,.0259,.1296,.4162,.069,.7284,.5,-.213,.0969,-.1699,.8019,.1452,-.0715,.3,.1509,.9632,.5", // (SNARE,+1)
            "4,,.2981,.1079,.1122,.1826,.0583,-.2287,.1341,.3666,.0704,.1626,.2816,.0642,.3733,.2103,-.3137,-.3065,.8693,-.3045,.4969,.0218,-.015,.6" // (KICK,+0)
        };

        public InstrumentType InstrumentType
        {
            get => _instrumentType;
            set
            {
                _instrumentType = value;

                var typeID = (int) InstrumentType;

                //                if (typeID < defaultOctaves.Length)
                //                {
                // Changing the instrument should set the deafult octave values
                OctaveRange = defaultOctaves[typeID];
                //                }
            }
        }

        public int SfxId { get; set; }
        public Point OctaveRange { get; set; }

        //    public bool locked;
        public bool Generate => InstrumentType != InstrumentType.None;


        public string InstrumentSettings => ReadInstrumentSoundData((int) InstrumentType);

        public string ReadInstrumentSoundData(int value)
        {
            //            if (value == (int) InstrumentType.None) return null;

            var id = InstrumentType == InstrumentType.Random ? r.Next(0, 7) : value;

            // Update the octave to match the returned instrument id
            OctaveRange = defaultOctaves[id];

            return instrumentTemplates[id];
        }
    }

    public class SfxrMusicGeneratorChip
    {
        private readonly Random r = new Random();
        public Point octaveRange = new Point(1, 8);

        #region Music Generator

        // NES = 4 voices in this order
        // 1) square (melody)
        // 2) square (harmony)
        // 3) triangle (bass)
        // 4) noise (drums)

        // C64 = 3 voices, any of:
        // square
        // trangle
        // sawtooth
        // noise

        // so the default "new song" has instruments set up as:
        // MELODY: square
        // HARMONY: square
        // BASS: triangle
        // DRUMS: noise
        // LEAD: sawtooth

        // Analyzing Super Mario Bros. theme song, we see:
        // Key: C major
        // Two part harmony in major thirds?
        // Chord Changes: I64 I6 I IV V IV I6 IV iii vi6 V7

        // scale intervals: distance between notes to make a nice sounding scale
        // "do re mi fa so la ti do"
        // Musical Scale Lookup Tables

        public enum Scale
        {
            MelodicMinorUp,
            Major,
            NaturalMinor,
            MelodicMinorDown,
            Dorian,
            HarmonicMinor,
            MinorPentatonic,
            Diatonic,
            Mixolydian,
            MajorPentatonic,
            Chromatic,
            AhavaRaba
        }

        //        private readonly float pcgInstMutateMax = 0.2f;
        //        private readonly float pcgInstMutateMin = 0f;
        //        private readonly float pcgInstMutationChance = 0f; // maybe add 0.1 chance we make instruments wacky
        private readonly int pcgKeyRangeMax = 8;
        private readonly int pcgKeyRangeMin = -8;

        private readonly int pcgNoteDistanceMax = 3;
        private readonly int pcgNoteDistanceMin = 0;

        /////////////////////////////////////////////////////////////////////////////
        // PROCEDURAL (RANDOM) MUSIC GENERATOR
        /////////////////////////////////////////////////////////////////////////////


        public int MAX_NOTE_NUM = 127; // how many notes in these arrays below

        //public int musicGridSnapNum = 1; // how many beats per piano note entry while editing
        //public LoopData oneBlankLoop;
        public int pcgBassShift = 0; // how many scale indeces to transpose a note for it's harmony

        public int pcgBassSuggestion; // while randomizing, make sure bass is not disspnant with melody
        public int[] pcgChordProgression; // what CHORD PROGRESSION to use? FIXME: unimplemented: uses RANDOM only


        // how likely each note is filled (0=all blank 1=fill every note)

        //    private float pcgComplexityMax = 0.75f;
        //    private float pcgComplexityMin = 0.2f;
        public int pcgDensity = 5;

        public int pcgFunk = 5;

        //    private int pcgHarmonyDistance = 3; // 5? // TODO: unimplemented - hardcoded 
        //private int pcgHarmonyShift = 2; // how many scale indeces to transpose a note for it's harmony
        private int pcgHarmonySuggestion; // while randomizing, suggest a nice harmony

        private int pcgKeyRootNote = 60; // what KEY is the song in? 60 = middle C
        public int pcgLayering = 5;

        //public int pcgLoopsToCreate = 4;
        public int pcgMaxTempo = 120;

        public int pcgMinTempo = 50;
        private int pcgPreviousNote; // phrasing reacts to previous notes

        //private int pcgPreviousProgressionIndex = 0; // phrasing reacts to previous chords in progression
        private int pcgPreviousScaleIndex; // phrasing reacts to previous notes

        private int[] pcgScale; // what SCALE to generate the melody? (eg the major scale)

        public int pcgTrackPAD = 5; // long chord or swell
        public int pcgTrackSFX = 6; // often a "snare drum"
        public bool recordingNoteInput = true; // when we play on piano does it record in SongData?

        public int scale;

        protected Dictionary<Scale, int[]> scaleTable = new Dictionary<Scale, int[]>
        {
            {Scale.Major, new[] {2, 2, 1, 2, 2, 2, 1}},
            {Scale.MajorPentatonic, new[] {2, 2, 3, 2, 3}},
            {Scale.MelodicMinorDown, new[] {2, 2, 1, 2, 2, 1, 2}},
            {Scale.MelodicMinorUp, new[] {2, 1, 2, 2, 2, 2, 1}},
            {Scale.MinorPentatonic, new[] {3, 2, 2, 3, 2}},
            {Scale.Mixolydian, new[] {2, 2, 1, 2, 2, 1, 2}},
            {Scale.NaturalMinor, new[] {2, 1, 2, 2, 1, 2, 2}},
            {Scale.AhavaRaba, new[] {1, 3, 1, 2, 1, 2, 2}},
            {Scale.Chromatic, new[] {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1}},
            {Scale.Diatonic, new[] {2, 2, 2, 2, 2, 2}},
            {Scale.Dorian, new[] {2, 1, 2, 2, 2, 1, 2}},
            {Scale.HarmonicMinor, new[] {2, 1, 2, 2, 1, 3, 1}}
        };

        public TrackSettings[] trackSettings = new TrackSettings[0];

        public void ConfigureGenerator(int tracks)
        {
            var total = tracks;

            Array.Resize(ref trackSettings, total);
            for (var i = 0; i < total; i++)
            {
                var settings = trackSettings[i];

                if (settings == null)
                {
                    trackSettings[i] = new TrackSettings();
                    settings = trackSettings[i];
                }

                settings.InstrumentType = (InstrumentType) i;
                settings.SfxId = i;
            }

            // forces scale to the default value
            UpdateScale((int) Scale.Major);

            //workspace.InvalidateSave();
        }

        public void GenerateSong(PixelVision chips, bool resetInstruments = false)
        {
            float noteChangeBoost; // more likely to play a note on beat one of each bar
            var restHere = true; // if true, silence
            var beginPhrase = true; // 1st note of entire loop?
            var beginBar = true; // 1st note of each bar?
            var noteTranspose = 0; // shifts the key away from middle C
            var notesWrittenThisTrack = 0; // keep count
            var notesWritten = 0; // for entire song
            var randy = 0f; // for debugging random number generator
            bool wasFunkyLastNote;

            var musicChip = chips.MusicChip;
            var soundChip = chips.SoundChip;

            var activeTrackerData = musicChip.ActiveTrackerData;

            musicChip.ResetTracker();

            pcgScale = scaleTable[Scale.Major]; //scaleMajor;

            //pcgScale = scaleMinorPentatonic;

            // reset melody to root
            pcgPreviousNote = pcgKeyRootNote;
            pcgPreviousScaleIndex = 0;

            // randomize the song data


            // random tempo
            activeTrackerData.speedInBPM = r.Next(pcgMinTempo, pcgMaxTempo);
            musicChip.UpdateNoteTickLengths();


            //            UpdateTempo();

            //updateNoteTickLengths();

            // random song name
            //            UpdateSongName(randomSongName());

            // random key (root note) /* 60 = middle C */
            pcgKeyRootNote = 60 + r.Next(pcgKeyRangeMin, pcgKeyRangeMax); // vary the key up or down


            // random scale TODO
            pcgScale = scaleTable[Scale.Major]; //scaleMajor;

            // mutate instruments FIXME
            //            if ((float) r.NextDouble() < pcgInstMutationChance) // only mutate half the time
            //                MutateAllInstruments((float) r.NextDouble() * (pcgInstMutateMax - pcgInstMutateMin) +
            //                                     pcgInstMutateMin); //Random.Range(pcgInstMutateMin, pcgInstMutateMax)); //TODO need to make sure this is working correctly

            // sanity check (not needed?)
            //        if (pcgTrackMax < 1)
            //            pcgTrackMax = 1;
            //        if (pcgTrackMax > GUItracksPerLoop)
            //            pcgTrackMax = GUItracksPerLoop;


            TrackSettings settings = null;
            var total = musicChip
                .totalTracks; //trackSettings.Length;//activeSongData.tracks.Length;//trackSettings.Length;

            //            Console.WriteLine("total tracks" + total);

            TrackData trackData = null;

            var harmonyTrackID = Array.FindIndex(trackSettings, t => t.InstrumentType == InstrumentType.Harmony);

            //Debug.Log("Found Harmony Track " + harmonyTrackID);

            // Make sure that the track settings is alwasy the correct size
            if (trackSettings.Length < total) Array.Resize(ref trackSettings, total);

            for (var trackNum = 0; trackNum < total; trackNum++) // in each track
            {
                // Need to account for a null track if the generator was not reconfigured
                if (trackSettings[trackNum] == null)
                    trackSettings[trackNum] = new TrackSettings
                    {
                        InstrumentType = (InstrumentType) trackNum,
                        SfxId = trackNum
                    };

                // Get the current track settings
                settings = trackSettings[trackNum];
                trackData = activeTrackerData.tracks[trackNum];

                // Make sure the track isn't locked
                //            if (!settings.locked)
                //            {

                // Update track to new SFX
                //trackData.SfxId = settings.

                // Look to see if we should generate a new instrument for the track
                if (resetInstruments == true || settings.Generate)
                {
                    //Debug.Log("Generate " + settings.instrumentType + " for track " + trackNum);
                    soundChip.UpdateSound(trackData.sfxID, settings.InstrumentSettings);
                    soundChip.UpdateLabel(trackData.sfxID, settings.InstrumentType.ToString());
                }

                // reset melody to root note of scale
                pcgPreviousNote = pcgKeyRootNote;
                pcgPreviousScaleIndex = 0;
                notesWrittenThisTrack = 0;
                wasFunkyLastNote = true; // so the very first note is never an octave too high

                var pcgComplexity = 1f - pcgDensity / 10f;

                // pure 0-1 (but density 10 means complexity 0 due to the way we calc prob)

                // Loop through each beat and create a note
                for (var noteNum = 0; noteNum < musicChip.NotesPerTrack; noteNum++) // for each note
                {
                    // Get the instrument ID for the track
                    var instrument = settings.InstrumentType;

                    beginPhrase = noteNum == 0; // the very start of this loop/section//phrase
                    beginBar = noteNum % 4 == 0; // put emphasis on each new bar's 1st note

                    // always more likely to play something at the very beginning
                    if (beginBar)
                        noteChangeBoost = 0.5f;
                    else
                        noteChangeBoost = 0f;

                    // different tracks act different ways according to musical "conventions"
                    // (my own opinionated mental models of pop music song structure)

                    // bass likes the first beat
                    if (instrument == InstrumentType.Bass && beginBar)
                        noteChangeBoost = 1.0f; // always play
                    else if (instrument == InstrumentType.Bass) noteChangeBoost = -0.2f; // less bass except at beat 1

                    // which octaves sound most appropriate?
                    noteTranspose = 0;
                    if (instrument == InstrumentType.Bass) noteTranspose = -24; // bass is low

                    if (instrument == InstrumentType.Drums) noteTranspose = 24; // high hat

                    if (instrument == InstrumentType.Kick)
                        noteTranspose = -24; // kick drum is atonal, but not mega low either

                    if (instrument == InstrumentType.Lead) noteTranspose = 24; // lead solo is often one octave higher

                    if (instrument == InstrumentType.Pad) noteTranspose = 12; // pad is really just lead-like here

                    // drums: kick likes beat 1
                    if (instrument == InstrumentType.Kick && beginBar)
                        noteChangeBoost = 1.0f; // always play
                    else if (instrument == InstrumentType.Kick)
                        noteChangeBoost = -0.3f; // kick drum less often if not beat 1

                    // drums: snare likes beat 3 but otherwise is more quiet
                    if (instrument == InstrumentType.Snare) noteChangeBoost = -0.4f; // less often

                    if (instrument == InstrumentType.Snare && noteNum % 4 == 2) noteChangeBoost = 1.0f; // always play

                    // drums: hat loves to play a lot
                    if (instrument == InstrumentType.Drums) noteChangeBoost = 0.4f; // play often

                    // lead is good for sustained notes: we don't want too many
                    if (instrument == InstrumentType.Lead) noteChangeBoost = -0.4f; // much less often

                    // the pad is very slow, held notes like a string orchestra
                    if (instrument == InstrumentType.Pad) noteChangeBoost = -0.5f; // very rare

                    if (instrument == InstrumentType.Pad && beginPhrase) noteChangeBoost = 0.8f; // except at the start

                    if (instrument == InstrumentType.Pad && beginBar) noteChangeBoost = 0.3f; // except at the start

                    // is silence appropriate for this moment?
                    //restHere = UnityEngine.Random.Range(0f,1f - noteChangeBoost) > pcgComplexity;
                    randy = (float) r.NextDouble(); //Random.value;
                    restHere = randy + noteChangeBoost < pcgComplexity;

                    // never rest the first beat of the song on bass
                    if (instrument == InstrumentType.Bass && beginPhrase) restHere = false;

                    if (!restHere) // play something?
                    {
                        notesWritten++;
                        notesWrittenThisTrack++;

                        // always play the root note on bass beat one
                        if (instrument == InstrumentType.Bass && beginPhrase)
                        {
                            trackData.notes[noteNum] = pcgKeyRootNote + noteTranspose;
                        }
                        else if (instrument == InstrumentType.Bass && beginBar)
                        {
                            // no note here - in melody track we ensure nice bass harmony is beat 1
                        }

                        // special case: harmony lines always "match" the melody
                        else if (instrument == InstrumentType.Melody)
                        {
                            // normal "random note"
                            trackData.notes[noteNum] = RandomNote() + noteTranspose;

                            // Need to set the harmony track to a note if it exists
                            if (harmonyTrackID > -1)
                                activeTrackerData.tracks[harmonyTrackID].notes[noteNum] =
                                    pcgHarmonySuggestion + noteTranspose;

                            // BASS: always sound nice with the harmony on beat 1 of other bars
                            if (beginBar) trackData.notes[noteNum] = pcgBassSuggestion + noteTranspose;
                        }
                        else if (instrument == InstrumentType.Harmony)
                        {
                            // we already wrote these in prev track so don't overwrite
                        }
                        else if (instrument == InstrumentType.Kick

                            //|| (trackNum == pcgTrackSFX)		// snare drum, usually
                            //|| (trackNum == pcgTrackDRUMS)
                        ) // high hat, usually
                        {
                            // drums have no tonality
                            trackData.notes[noteNum] = pcgKeyRootNote + noteTranspose;
                        }
                        else if (instrument == InstrumentType.Bass) // and not beat 1, done above
                        {
                            if (!wasFunkyLastNote && (float) r.NextDouble() < pcgFunk
                            ) // we want a funky OCTAVE bass note
                            {
                                wasFunkyLastNote = true;

                                // either stay the same, or jump higher or lower by an entire octave
                                var funkyOffset = 12;
                                if ((float) r.NextDouble() > 0.5f) // octave higher half the time
                                {
                                    if ((float) r.NextDouble() > 0.5f) // stay same 25% of the time
                                        funkyOffset = 0;
                                    else // octave lower %25 of the time
                                        funkyOffset = -12;
                                }

                                trackData.notes[noteNum] = pcgPreviousNote + funkyOffset +
                                                           noteTranspose;
                            }
                            else // totally random bass note
                            {
                                wasFunkyLastNote = false;
                                trackData.notes[noteNum] = RandomNote() + noteTranspose;
                            }
                        }
                        else
                        {
                            // normal "random note" that steps up and down a scale based on prev note
                            trackData.notes[noteNum] = RandomNote() + noteTranspose;
                        }
                    }
                    else // leave this note blank
                    {
                        trackData.notes[noteNum] = 0;
                    }
                } // each note

                //            } // end of locked
            } // each track

            //workspace.InvalidateSave();
        }

        public void UpdateScale(int value)
        {
            scale = value;
            pcgScale = scaleTable[(Scale) value];
        }

        /////////////////////////////////////////////////////////////////////////////
        private int RandomNote() // output a random MIDI note number
            /////////////////////////////////////////////////////////////////////////////
        {
            // TODO use pcg params to select notes that fit the current scale/mode/chord/harmony
            var aNote = 0;
            var noteDistance = r.Next(pcgNoteDistanceMin, pcgNoteDistanceMax); // 0,1,2
            var noteDir = 1; // +1 or -1 only
            if ((float) r.NextDouble() > 0.5f) noteDir = -1; // down

            // pure random noise - works
            // aNote = Mathf.CeilToInt(UnityEngine.Random.Range(32f,64f));

            aNote = pcgPreviousNote;

            // move a couple steps away in the scale
            for (var noteStep = 0; noteStep < noteDistance; noteStep++)
            {
                // modulo "%" won't work with negative deltas so we check bound manually
                //pcgPreviousScaleIndex = ((pcgPreviousScaleIndex + noteDir) % pcgScale.Length);
                pcgPreviousScaleIndex = pcgPreviousScaleIndex + noteDir;
                if (pcgPreviousScaleIndex > pcgScale.Length - 1) pcgPreviousScaleIndex -= pcgScale.Length;

                if (pcgPreviousScaleIndex < 0) pcgPreviousScaleIndex += pcgScale.Length;

                if (noteDir > 0)
                    aNote += pcgScale[pcgPreviousScaleIndex];
                else
                    aNote -= pcgScale[
                        (pcgPreviousScaleIndex + 1) % pcgScale.Length]; // FIXME are these the right intervals?

#if DEBUGMUSIC //Debug.Log("aNote=" + aNote + " noteDistance="+noteDistance+" noteDir="+noteDir);
#endif
            }

            // if we shift two notes forward in the scale we get a lovely 3rd
            pcgHarmonySuggestion = aNote
                                   + pcgScale[(pcgPreviousScaleIndex + 1) % pcgScale.Length]
                                   + pcgScale[(pcgPreviousScaleIndex + 2) % pcgScale.Length];

            // fixme maybe use pcgHarmonyShift for loop that many times?

            pcgBassSuggestion = aNote - 12; // one octave lower than melody

            // fixme maybe use pcgBassShift for loop that many times?

            // watch super high notes
            // simply mute note rather than reverse shift in key
            if (aNote > MAX_NOTE_NUM) aNote = 0;

            if (pcgHarmonySuggestion > MAX_NOTE_NUM) pcgHarmonySuggestion = 0;

            if (aNote < 0)
            {
                aNote = 0;
                pcgHarmonySuggestion = 0;
                pcgBassSuggestion = 0;
            }

            if (pcgBassSuggestion < 1) pcgBassSuggestion = aNote;

            pcgPreviousNote = aNote;
            return aNote;
        }

        #endregion
    }
}