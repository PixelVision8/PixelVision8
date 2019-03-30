using System;
using System.IO;
using PixelVision8.Engine;
using PixelVision8.Engine.Chips;
using PixelVision8.Runner.Chips.Sfxr;
using PixelVision8.Runner.Data;


namespace PixelVision8.Runner.Exporters
{
	public class AudioClip : IAudioClip
	{
		private readonly string name;

		public AudioClip(string name, int samples, int channels, int frequency)
		{
			this.name = name;
			this.samples = samples;
			this.channels = channels;
			this.frequency = frequency;
		}

		public bool SetData(float[] data, int offsetSamples)
		{
			throw new NotImplementedException();
		}

		public int samples { get;  set;}
		public int channels { get;  set;}
		public bool GetData(float[] data, int offsetSamples)
		{
			throw new NotImplementedException();
		}

		public int frequency { get; set; }
	}
	
    public class SongExporter : AbstractExporter
    {
        private readonly IAudioClipFactory audioClipFactory;
//        private IEngine engine;
        private readonly int MAX_NOTE_NUM = 127; // how many notes in these arrays below

        public float mixdownTrackVolume = 0.6f; // to avoid clipping, all tracks are a bit quieter since we ADD values - set to 1f for full mix

        private readonly MusicChip musicChip;
        public float[] note_hz;

        // TODO this is not set yet
        public float[] note_startFrequency; // = new float[MAX_NOTE_NUM]; // same, but for sfxr frequency 0..1 range
        public float note_tick_s = 30.0f / 120.0f; // (30.0f/120.0f) = 120BPM eighth notes
        public float note_tick_s_even;
        public float note_tick_s_odd;
//        private bool pcg_currently_exporting = false; // for multi-export to wait till we're done

        // optimization: precache silent playback to ram
        public int preRenderBitrate = 44100; //48000; // should be 44100; FIXME TODO EXPERIMENTING W BUGFIX
        private IAudioClip result;
        private readonly SoundChip soundChip;

        public float
            swing_rhythm_factor = 0.7f; //1.0f;//0.66666f; // how much "shuffle" - turnaround on the offbeat triplet

        private IAudioClip[] trackresult;

        public SongExporter(string path, MusicChip musicChip, SoundChip soundChip,
            IAudioClipFactory audioClipFactory) : base(path)
        {
//            Debug.Log("FileName " + musicChip.activeSongData.songName);

            // Rebuild the path by adding the active song name and wav extension
            fileName = path + musicChip.activeTrackerData.songName + ".wav";

            // Save references to the currentc chips
            this.musicChip = musicChip;
            this.soundChip = soundChip;

            // Save a reference to the audio factory
            this.audioClipFactory = audioClipFactory;

            // Calculate steps
//            CalculateSteps();
        }

        public override void CalculateSteps()
        {
            base.CalculateSteps();

            steps.Add(ExportSong);
            steps.Add(MixdownAudioClips);
            steps.Add(CreateSongByteData);
        }

        public void ExportSong()
        {
            // Get the active song data
            var songData = musicChip.activeTrackerData;

            // to mix tracks, you just ADD THE DATA
            // but need to clip? nope just add (and maybe amp all down a little?)

            // for each loop
            // for each track
            // for each note
            // cache note wav data
            // combine all 32 note clips into one clip per track
            // mixdown all 8 track clips into one clip per loop
            // combine all n loop clips into one audioclip per song
            // save one giant float array for the whole song (several megs)
            // save data as a .wav

            var totalNotesRendered = 0;
//            int loopnum;
            int tracknum;
            int notenum;
            int gotANote;
//            string songExportFilename;

            // assumes all loops have same numbers of tracks and notes
//            int lcount = songData.loops.Length;
            var tcount = songData.tracks.Length;
            var ncount = songData.tracks[0].notes.Length;

            var totalNotesInSong = ncount; // total musical notes in song loops x notes

            // Calculate the notes
            var a = 440.0f; // a is 440 hz...
            note_hz = new float[MAX_NOTE_NUM];
            note_startFrequency = new float[MAX_NOTE_NUM];
            note_startFrequency[0] = 0f; // since we never set it below
            var SR = 44100.0f; // hmm preRenderBitrate? nah
            float hertz;
            for (var x = 0; x < MAX_NOTE_NUM; ++x)
            {
                // what Hz is a particular musical note? (eg A#)
                hertz = a / 32.0f * (float) Math.Pow(2.0f, (x - 9.0f) / 12.0f);
                note_hz[x] = hertz; // this appears to be correct: C = 60 = 261.6255Hz

                // derive the SFXR sine wave frequency to play this Hz
                // FIXME: this sounds about a semitone too high compared to real life piano!
                // note_startFrequency[x] = Mathf.Sqrt(hertz / SR * 100.0f / 8.0f - 0.001f);
                // maybe the algorithm assumes 1 based array etc?
                if (x < 126) // let's just hack in one semitone lower sounds (but not overflow array)
                    note_startFrequency[x + 1] =
                        (float) Math.Sqrt(hertz / SR * 100.0f / 8.0f - 0.001f) -
                        0.0018f; // last num is a hack using my ears to "tune"
            }


            updateNoteTickLengths(songData); // FIXME: allow shuffle rhythm note length changes?

            var notedatalength = (int) (preRenderBitrate * 2f * note_tick_s_odd); // one note worth of stereo audio (x2)
            var beatlength = preRenderBitrate * note_tick_s_odd; // one note worth of time
            var songdatalength = notedatalength * totalNotesInSong * 2; // stereo cd quality x song length

            var notebuffer = new float[notedatalength];
            var songdataCurrentPos = 0;

            // all the tracks we need - an array of audioclips that will be merged into result
            trackresult = new IAudioClip[tcount];

            var instrument = new SfxrSynth[songData.totalTracks];

            for (tracknum = 0; tracknum < tcount; tracknum++)
            {
                if (instrument[tracknum] == null)
                    instrument[tracknum] = new SfxrSynth();

                // stereo
                //trackresult[tracknum] = AudioClip.Create("Track"+tracknum, songdatalength / 2, 2, preRenderBitrate, false);
                // mono
                trackresult[tracknum] = audioClipFactory.NewAudioClip("Track" + tracknum, songdatalength / 2, 1,
                    preRenderBitrate, false);

                songdataCurrentPos = 0;

                // set the params to current track's instrument string
                instrument[tracknum].parameters
                    .SetSettingsString(soundChip.ReadSound(songData.tracks[tracknum].sfxID).ReadSettings());
//                        SongData.loops[loopnum].tracks[tracknum].instrument);

                for (notenum = 0; notenum < ncount; notenum++)
                {
                    // what note is it?
                    gotANote = songData.tracks[tracknum].notes[notenum];

                    // generate one note worth of audio data
                    if (gotANote > 0 && instrument[tracknum] != null)
                    {
                        // FIXME TODO HACK: prerendered notes seem pitch shifted too high
                        // manually fudged - WHY WHY WHY - no idea why this works, but it does.
                        // this is the most heinous hack I've ever implemented and it hurts so bad
                        // it isn't worth anything: this is duct tape solution
                        var noteFUDGE = 1; //- 12; // shift how many semitones
                        var freqFUDGE = 0.0025f; //0.005f; // slightly off without this offset

                        //instrument[tracknum].Reset(true); // seems to do nothing

                        // doing this for every note played is insane:
                        // try a fresh new instrument (RAM and GC spammy)
                        //instrument[tracknum] = new SfxrSynth(); // shouldn't be required
                        // set the params to current track's instrument string
                        instrument[tracknum].parameters
                            .SetSettingsString(soundChip.ReadSound(songData.tracks[tracknum].sfxID).ReadSettings());


                        // pitch shift the sound to the correct musical note frequency
                        instrument[tracknum].parameters.startFrequency =
                            note_startFrequency[gotANote + noteFUDGE] + freqFUDGE;


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
//                        notebuffer = instrument[tracknum].cachedWave; // the wave data for the current note

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


            currentStep++;
        }

        private void MixdownAudioClips()
        {
            result = MixdownAudioClips(trackresult);
            currentStep++;
        }

        private void CreateSongByteData()
        {
	        //TODO need to break this process up in to steps so it doesn't block the runner
            bytes = Save(fileName, result);
            currentStep++;
        }

        private void updateNoteTickLengths(TrackerData trackerData)
        {
            note_tick_s = 30.0f / trackerData.speedInBPM; // (30.0f/120.0f) = 120BPM eighth notes [tempo]
//            if (!SongData.shuffleRhythm) swing_rhythm_factor = 1; // not a swing beat: all beats same length
            note_tick_s_odd = note_tick_s * swing_rhythm_factor; // small beat
            note_tick_s_even = note_tick_s * 2 - note_tick_s_odd; // long beat
#if DEBUGMUSIC
		Debug.Log("updateNoteTickLengths: speedInBPM is " + SongData.speedInBPM + " so note_tick_s_odd=" + note_tick_s_odd);
		#endif
        }

        // pre-rendered waveforms - OPTIMIZATION - SLOW SLOW SLOW - FIXME
        public /* static */ IAudioClip MixdownAudioClips(params IAudioClip[] clips)
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
                if (clips[i] == null)
                    continue;

                clips[i].GetData(buffer, 0);

                // mix two signals together: we might get too loud...
                // FIXME: we may need to make all clips a bit quieter
                for (loop = 0; loop < length; loop++)
                {
                    data[loop] += buffer[loop] * mixdownTrackVolume; // add it to the old signal

                    if (data[loop] > maxVolume) maxVolume = data[loop];

                    if (data[loop] > 1f) // too loud?
                    {
                        clipWarnings++;
                        if (data[loop] - 1f > redlineMax)
                            redlineMax = data[loop] - 1f;
                        data[loop] = 1f;
                    }
                }
            }


            // stereo
            //AudioClip result = AudioClip.Create("MixdownSTEREO", length / 2, 2, preRenderBitrate, false);
            // mono
            var result = audioClipFactory.NewAudioClip("MixdownMONO", length / 2, 1, preRenderBitrate, false);
            result.SetData(data, 0); // TODO: we can get a warning here: data too large to fit: discarded x samples
            // the truncation can happen with a large sustain of a note that could go on after the song is over
            // one solution is to pad the end with 4sec of 0000s then maybe search and TRIM

            return result;
        }

        #region Save Wav Code
        
	    //  derived from Gregorio Zanon's script
//  http://forum.unity3d.com/threads/119295-Writing-AudioListener.GetOutputData-to-wav-problem?p=806734&viewfull=1#post806734

	    
        const int HEADER_SIZE = 44;

	public  byte[] Save(string filepath, IAudioClip clip) {

		MemoryStream fileStream;
		
		using (fileStream = CreateEmpty()) {

			ConvertAndWrite(fileStream, clip);

			WriteHeader(fileStream, clip);
		}

		return fileStream.GetBuffer(); // TODO: return false if there's a failure saving the file
	}

	 MemoryStream CreateEmpty() {
//		var fileStream = new FileStream(filepath, FileMode.Create);
	    byte emptyByte = new byte();
		
		var fileStream = new MemoryStream(emptyByte);
	    for(int i = 0; i < HEADER_SIZE; i++) //preparing the header
	    {
	        fileStream.WriteByte(emptyByte);
	    }

		return fileStream;
	}

	 void ConvertAndWrite(MemoryStream fileStream, IAudioClip clip) {

		var samples = new float[clip.samples];

		clip.GetData(samples, 0);

		Int16[] intData = new Int16[samples.Length];
		//converting in 2 float[] steps to Int16[], //then Int16[] to Byte[]

		Byte[] bytesData = new Byte[samples.Length * 2];
		//bytesData array is twice the size of
		//dataSource array because a float converted in Int16 is 2 bytes.

		int rescaleFactor = 32767; //to convert float to Int16

		for (int i = 0; i<samples.Length; i++) {
			intData[i] = (short) (samples[i] * rescaleFactor);
			Byte[] byteArr = new Byte[2];
			byteArr = BitConverter.GetBytes(intData[i]);
			byteArr.CopyTo(bytesData, i * 2);
		}

		fileStream.Write(bytesData, 0, bytesData.Length);
	}

	 void WriteHeader(MemoryStream fileStream, IAudioClip clip) {

		var hz = clip.frequency;
		var channels = clip.channels;
		var samples = clip.samples;

		fileStream.Seek(0, SeekOrigin.Begin);

		Byte[] riff = System.Text.Encoding.UTF8.GetBytes("RIFF");
		fileStream.Write(riff, 0, 4);

		Byte[] chunkSize = BitConverter.GetBytes(fileStream.Length - 8);
		fileStream.Write(chunkSize, 0, 4);

		Byte[] wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
		fileStream.Write(wave, 0, 4);

		Byte[] fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
		fileStream.Write(fmt, 0, 4);

		Byte[] subChunk1 = BitConverter.GetBytes(16);
		fileStream.Write(subChunk1, 0, 4);

		//UInt16 two = 2; // unused
		UInt16 one = 1;

		Byte[] audioFormat = BitConverter.GetBytes(one);
		fileStream.Write(audioFormat, 0, 2);

		Byte[] numChannels = BitConverter.GetBytes(channels);
		fileStream.Write(numChannels, 0, 2);

		Byte[] sampleRate = BitConverter.GetBytes(hz);
		fileStream.Write(sampleRate, 0, 4);

		Byte[] byteRate = BitConverter.GetBytes(hz * channels * 2); // sampleRate * bytesPerSample*number of channels, here 44100*2*2
		fileStream.Write(byteRate, 0, 4);

		UInt16 blockAlign = (ushort) (channels * 2);
		fileStream.Write(BitConverter.GetBytes(blockAlign), 0, 2);

		UInt16 bps = 16;
		Byte[] bitsPerSample = BitConverter.GetBytes(bps);
		fileStream.Write(bitsPerSample, 0, 2);

		Byte[] datastring = System.Text.Encoding.UTF8.GetBytes("data");
		fileStream.Write(datastring, 0, 4);

		Byte[] subChunk2 = BitConverter.GetBytes(samples * channels * 2);
		fileStream.Write(subChunk2, 0, 4);

//		fileStream.Close();
	}
        

        #endregion
    }
}