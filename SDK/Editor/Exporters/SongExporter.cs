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
using PixelVision8.Runner.Exporters;

namespace PixelVision8.Editor
{
    public class SongExporter : AbstractExporter
    {
        private readonly MusicChip musicChip;

        private readonly int[] patterns;
        private readonly SoundChip soundChip;
        private int currentPattern;

        // to avoid clipping, all tracks are a bit quieter since we ADD values - set to 1f for full mix
        public float mixdownTrackVolume = 0.6f;
        public float note_tick_s = 15.0f / 120.0f; // (15.0f/120.0f) = 120BPM sixteenth notes
        public float note_tick_s_odd;
        private SoundChannel result;


        public float
            swing_rhythm_factor = 1.0f; //0.7f;//0.66666f; // how much "shuffle" - turnaround on the offbeat triplet

        private SoundChannel[] trackresult;

        //        int songdataCurrentPos = 0;

        public SongExporter(string path, MusicChip musicChip, SoundChip soundChip, int[] patterns) : base(path)
        {
            // Rebuild the path by adding the active song name and wav extension
            fileName = path;

            // Save references to the currentc chips
            this.musicChip = musicChip;
            this.soundChip = soundChip;

            this.patterns = patterns;
        }

        public float[] noteStartFrequency => musicChip.noteStartFrequency;

        // optimization: precache silent playback to ram
        public int preRenderBitrate => musicChip.preRenderBitrate;

        public override void CalculateSteps()
        {
            base.CalculateSteps();

            for (var i = 0; i < patterns.Length; i++) Steps.Add(ExportSong);

            Steps.Add(MixdownAudioClips);
            Steps.Add(CreateSongByteData);
        }

        public void ExportSong()
        {
            // Get the active song data
            var songData = musicChip.trackerDataCollection[patterns[currentPattern]];

            var totalNotesRendered = 0;
            int tracknum;
            int notenum;
            int gotANote;

            var tcount = songData.tracks.Length;
            var ncount = songData.tracks[0].notes.Length;

            var totalNotesInSong = ncount; // total musical notes in song loops x notes

            // TODO needed to increase this number to match the speed better but this should be test out more.
            note_tick_s = 15.0f / songData.speedInBPM; // (30.0f/120.0f) = 120BPM eighth notes [tempo]
            note_tick_s_odd = note_tick_s * swing_rhythm_factor; // small beat

            var notedatalength = (int) (preRenderBitrate * 2f * note_tick_s_odd); // one note worth of stereo audio (x2)

            // TODO this is off. It is happening to fast and not matching up to the correct speed of the song.
            var beatlength = preRenderBitrate * note_tick_s_odd; // one note worth of time

            var songdatalength = notedatalength * totalNotesInSong; // stereo cd quality x song length


            var notebuffer = new float[notedatalength];

            if (trackresult == null)
            {
                // all the tracks we need - an array of audioclips that will be merged into result
                trackresult = new SoundChannel[tcount];

                for (var i = 0; i < tcount; i++)
                    trackresult[i] = new SoundChannel(0, 1,
                        preRenderBitrate);
            }

            var instrument = new SoundChannel[songData.totalTracks];

            var newStartPos = trackresult[0].samples / 2;
            var newLength = trackresult[0].samples + songdatalength;


            for (tracknum = 0; tracknum < tcount; tracknum++)
            {
                if (instrument[tracknum] == null) instrument[tracknum] = new SoundChannel();

                var songdataCurrentPos = newStartPos;
                trackresult[tracknum].Resize(newLength);

                // set the params to current track's instrument string
                instrument[tracknum].parameters.param = soundChip.ReadSound(songData.tracks[tracknum].sfxID).param;

                // Loop through all of the notes in the track
                for (notenum = 0; notenum < ncount; notenum++)
                {
                    // what note is it?
                    gotANote = songData.tracks[tracknum].notes[notenum];

                    // generate one note worth of audio data
                    if (gotANote > 0 && instrument[tracknum] != null)
                    {
                        //instrument[tracknum].Reset(true); // seems to do nothing

                        // doing this for every note played is insane:
                        // try a fresh new instrument (RAM and GC spammy)
                        instrument[tracknum] = new SoundChannel(); // shouldn't be required, but for some reason it is
                        // set the params to current track's instrument string
                        instrument[tracknum].parameters.param = soundChip.ReadSound(songData.tracks[tracknum].sfxID).param;


                        // pitch shift the sound to the correct musical note frequency
                        instrument[tracknum].parameters.startFrequency = noteStartFrequency[gotANote];


                        // maybe this will help
                        // instrument[tracknum].GenerateAudioFilterData(float[] __data, int __channels)

                        // or perhaps this - nope: outputs silence... hmm
                        //bool gotAllData = instrument[tracknum].SynthWave(notebuffer, 0, (uint)notedatalength);// (uint)notebuffer.Length);

                        // unlikely but helpful - used by GenerateAudioFilterData
                        // WriteSamples(float[] __originSamples, int __originPos, float[] __targetSamples, int __targetChannels) {
                        // Writes raw samples to Unity's format and return number of samples actually written

                        // does play need to be called for it to be generated properly?
                        instrument[tracknum].CacheSound(); // generate cachedWave data

                        // grab the sample data - this data is not in the same fmt as unity's internal?
                        // sounds pitch-shifted and faster - a different bitrate?
                        // WORKS GREAT IF WE RUN CACHESOUND() FIRST
                        // TODO need to figure out why we can't access the cachedWave
                        notebuffer = instrument[tracknum].data; // the wave data for the current note

                        // this SHOULD be the solution. but outputs all 0's
                        //bool gotAllData = instrument[tracknum].GenerateAudioFilterData(notebuffer, 2);


                        // move sound data into audioclip buffer
                        if (notebuffer != null && notebuffer.Length > 0)
                            trackresult[tracknum].SetData(notebuffer, songdataCurrentPos);
                        //                            else
                        //                                Debug.LogWarning("Notebuffer was blank. Ignoring.");

                        totalNotesRendered++;
                    }

                    // TODO converted this to an int, may break?
                    songdataCurrentPos += (int) beatlength; //notedatalength; this is x2 due to stereo

                    //yield return null; // if here, min one frame PER note PER track PER loop: too slow
                } // for all notes
            } // for all tracks


            currentPattern++;
            CurrentStep++;
        }

        private void MixdownAudioClips()
        {
            result = MixdownAudioClips(trackresult);
            CurrentStep++;
        }

        private void CreateSongByteData()
        {
            //TODO need to break this process up in to steps so it doesn't block the runner
            Bytes = result.GenerateWav();
            CurrentStep++;
        }

        // pre-rendered waveforms - OPTIMIZATION - SLOW SLOW SLOW - FIXME
        public SoundChannel MixdownAudioClips(params SoundChannel[] clips)
        {
            if (clips == null || clips.Length == 0) return null;

            var length = clips[0].samples * clips[0].channels; // assumes all are same length

            var buffer = new float[length];
            var data = new float[length];
            var loop = 0;
            var clipWarnings = 0; // did we red zone?
            var redlineMax = 0f; // so we know how overly loud it was
            var maxVolume = 0f; // before clipping

            // fill with silence
            for (loop = 0; loop < length; loop++) data[loop] = 0f;

            for (var i = 0; i < clips.Length; i++)
            {
                if (clips[i] == null) continue;

                buffer = clips[i].data; //.GetData(buffer);

                // mix two signals together: we might get too loud...
                // FIXME: we may need to make all clips a bit quieter
                for (loop = 0; loop < length; loop++)
                {
                    data[loop] += buffer[loop] * mixdownTrackVolume; // add it to the old signal

                    if (data[loop] > maxVolume) maxVolume = data[loop];

                    if (data[loop] > 1f) // too loud?
                    {
                        clipWarnings++;
                        if (data[loop] - 1f > redlineMax) redlineMax = data[loop] - 1f;

                        data[loop] = 1f;
                    }
                }
            }


            // stereo
            //AudioClip result = AudioClip.Create("MixdownSTEREO", length / 2, 2, preRenderBitrate, false);
            // mono
            var result = new SoundChannel(length / 2, 1, preRenderBitrate);
            result.SetData(data); // TODO: we can get a warning here: data too large to fit: discarded x samples
            // the truncation can happen with a large sustain of a note that could go on after the song is over
            // one solution is to pad the end with 4sec of 0000s then maybe search and TRIM

            return result;
        }
    }
}