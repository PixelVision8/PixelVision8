//  SfxrSynth
//    
//  Copyright 2010 Thomas Vian
//  Copyright 2013 Zeh Fernando
//    
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//    
//  	http://www.apache.org/licenses/LICENSE-2.0
//    
//  Unless required by applicable law or agreed to in writing, software
//  distributed under the License is distributed on an "AS IS" BASIS,
//  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  See the License for the specific language governing permissions and
//  limitations under the License.
//  
//  
//  SfxrSynth
//  Generates and plays all necessary audio
//  
//  @author Zeh Fernando

using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.IO;

namespace PixelVision8.Player.Audio
{
    public class SfxrSynthChannel : SoundChannel
    {
        private const int LO_RES_NOISE_PERIOD = 8; // Should be < 32
        //        private float _overtoneFalloff; // Minimum frequency before stopping

        //        private int _overtones; // Minimum frequency before stopping

        // private readonly Dictionary<string, SoundEffectInstance> wavCache =
        //     new Dictionary<string, SoundEffectInstance>();

        private float _changeAmount; // Amount to change the note by

        private int _changeLimit; // Once the time reaches this limit, the note changes

        // From BFXR
        private float _changePeriod;
        private int _changePeriodTime;

        private bool _changeReached;
        private int _changeTime; // Counter for the note change
        private float _compressionFactor;
        private float _deltaSlide; // Change in slide
        private float _dutySweep; // Amount to change the duty by
        private uint _envelopeFullLength; // Full length of the volume envelop (and therefore sound)
        private float _envelopeLength; // Length of the current envelope stage
        private float _envelopeLength0; // Length of the attack stage
        private float _envelopeLength1; // Length of the sustain stage
        private float _envelopeLength2; // Length of the decay stage
        private float _envelopeOverLength0; // 1 / _envelopeLength0 (for quick calculations)
        private float _envelopeOverLength1; // 1 / _envelopeLength1 (for quick calculations)
        private float _envelopeOverLength2; // 1 / _envelopeLength2 (for quick calculations)
        private int _envelopeStage; // Current stage of the envelope (attack, sustain, decay, end)
        private float _envelopeTime; // Current time through current enelope stage

        private float _envelopeVolume; // Current volume of the envelope

        private bool _filters; // If the filters are active

        // Synth properies
        private bool _finished; // If the sound has finished
        private float _hpFilterCutoff; // Cutoff multiplier which adjusts the amount the wave position can move
        private float _hpFilterDeltaCutoff; // Speed of the high-pass cutoff multiplier

        private float _hpFilterPos; // Adjusted wave position after high-pass filter
        private float[] _loResNoiseBuffer; // Buffer of random values used to generate Tan waveform
        private float _lpFilterCutoff; // Cutoff multiplier which adjusts the amount the wave position can move
        private float _lpFilterDamping; // Damping muliplier which restricts how fast the wave position can move
        private float _lpFilterDeltaCutoff; // Speed of the low-pass cutoff multiplier
        private float _lpFilterDeltaPos; // Change in low-pass wave position, as allowed by the cutoff and damping
        private float _lpFilterOldPos; // Previous low-pass wave position
        private bool _lpFilterOn; // If the low pass filter is active
        private float _lpFilterPos; // Adjusted wave position after low-pass filter

        private float _masterVolume; // masterVolume * masterVolume (for quick calculations)
        private float _maxPeriod; // Maximum period before sound stops (from minFrequency)
        private float _minFrequency; // Minimum frequency before stopping

        // Pre-calculated data
        private float[] _noiseBuffer; // Buffer of random values used to generate noise

        private SfxrParams _original; // Copied properties for mutation base
        private float _period; // Period of the wave
        private float _periodTemp; // Period modified by vibrato
        private int _periodTempInt; // Period modified by vibrato (as an Int)

        private int _phase; // Phase through the wave

        private bool _phaser; // If the phaser is active
        private float[] _phaserBuffer; // Buffer of wave values used to create the out of phase second wave
        private float _phaserDeltaOffset; // Change in phase offset
        private int _phaserInt; // Integer phaser offset, for bit maths
        private float _phaserOffset; // Phase offset for phaser effect
        private int _phaserPos; // Position through the phaser buffer

        private float _pos; // Phase expresed as a Number from 0-1, used for fast sin approx

        //
        //        private readonly Random _random = new Random();
        private int _repeatLimit; // Once the time reaches this limit, some of the variables are reset

        private int _repeatTime; // Counter for the repeats

        private float _sample; // Sub-sample calculated 8 times per actual sample, averaged out to get the super sample
        //        private float _sample2; // Used in other calculations

        private float _slide; // Note slide


        //        private float amp; // Used in other calculations
        private SoundEffectInstance _soundInstance;

        private float _squareDuty; // Offset of center switching point in the square wave

        // Temp
        private float _superSample; // Actual sample writen to the wave

        private float _sustainPunch; // The punch factor (louder at begining of sustain)
        private float _vibratoAmplitude; // Amount to change the period of the wave by at the peak of the vibrato wave

        private float _vibratoPhase; // Phase through the vibrato sine wave
        private float _vibratoSpeed; // Speed at which the vibrato phase moves

        private WaveType _waveType; // Shape of wave to generate (see enum WaveType)

        public float[] data;

        public SfxrSynthChannel(int samples = 0, int channels = 1, int frequency = 22050)
        {
            this.samples = samples;
            this.channels = channels;
            this.frequency = frequency;
            data = new float[samples];
        }

        public int samples { get; set; }
        public int channels { get; }

        public int frequency { get; }

        public WaveType waveLock { get; private set; } = WaveType.None;

        public WaveType waveType
        {
            get => waveLock == WaveType.None ? _waveType : waveLock;
            set => _waveType = value;
        }

        /// <summary>
        ///     Sound parameters
        /// </summary>
        public SfxrParams parameters { get; } = new SfxrParams();

        /// <summary>
        ///     Plays the sound. If the parameters are dirty, synthesises sound as it plays, caching it for later.
        ///     If they're not, plays from the cached sound. Won't play if caching asynchronously.
        /// </summary>
        public override void Play(SoundData soundData, float? frequency = null)
        {
            if (soundData is SfxSoundData)
            {
                // Stop any playing sound
                Stop();

                // TODO this logic isn't working correctly. Need to double check the cache
                
                // Clear the last sound instance
                _soundInstance = null;

                parameters.SetSettingsString(((SfxSoundData) soundData).param);

                if (frequency.HasValue) parameters.startFrequency = frequency.Value;

                if (parameters.Invalid) CacheSound();

                // Only play if there is a sound instance
                _soundInstance?.Play();
            }

            if (waveLock == WaveType.Sample || waveLock == WaveType.None)
                base.Play(soundData, frequency);
        }

        /// <summary>
        ///     Stops the currently playing sound
        /// </summary>
        public override void Stop()
        {
            base.Stop();

            if (_original != null)
            {
                parameters.CopyFrom(_original);
                _original = null;
            }
        }

        public WaveType ChannelType(WaveType? type)
        {
            if (type.HasValue)
                // Pass this value directly to the private variable
                waveLock = type.Value;

            return waveLock;
        }

        /// <summary>
        ///     Returns a ByteArray of the wave in the form of a .wav file, ready to be saved out
        /// </summary>
        /// <param name="__sampleRate">Sample rate to generate the .wav data at (44100 or 22050, default 44100)</param>
        /// <param name="__bitDepth">Bit depth to generate the .wav at (8 or 16, default 16)</param>
        /// <returns>Wave data (in .wav format) as a byte array</returns>
        // public override byte[] GenerateWav()
        // {
        //     Stop();
        //
        //     Reset(true);
        //
        //     Resize(Convert.ToInt32(_envelopeFullLength));
        //     SynthWave(data, 0, _envelopeFullLength);
        //
        //     return base.GenerateWav();
        // }
        //
        public void SetData(float[] data, int offsetSamples = 0)
        {
            var total = data.Length;
            for (var i = 0; i < total; i++)
            {
                var index = i + offsetSamples;

                if (index < samples) this.data[index] = data[i];
            }
        }

        public void Resize(int samples)
        {
            Array.Resize(ref data, samples);
            this.samples = samples;
        }

        // CONVERT TO WAV

        /// <summary>
        ///     Returns a ByteArray of the wave in the form of a .wav file, ready to be saved out
        /// </summary>
        /// <param name="__sampleRate">Sample rate to generate the .wav data at (44100 or 22050, default 44100)</param>
        /// <param name="__bitDepth">Bit depth to generate the .wav at (8 or 16, default 16)</param>
        /// <returns>Wave data (in .wav format) as a byte array</returns>
        public virtual byte[] GenerateWav()
        {
            Stop();

            Reset(true);

            Resize(Convert.ToInt32(_envelopeFullLength));
            SynthWave(data, 0, _envelopeFullLength);

            var __sampleRate = 22050u;
            var __bitDepth = 8u;

            var soundLength = Convert.ToUInt32(samples);
            if (__bitDepth == 16) soundLength *= 2;

            if (__sampleRate == 22050) soundLength /= 2;

            var fileSize = 36 + soundLength;
            var blockAlign = __bitDepth / 8;
            var bytesPerSec = __sampleRate * blockAlign;

            // The file size is actually 8 bytes more than the fileSize
            var wav = new byte[fileSize + 8];

            var bytePos = 0;

            // Header

            // Chunk ID "RIFF"
            writeUintToBytes(wav, ref bytePos, 0x52494646, Endian.BIG_ENDIAN);

            // Chunck Data Size
            writeUintToBytes(wav, ref bytePos, fileSize, Endian.LITTLE_ENDIAN);

            // RIFF Type "WAVE"
            writeUintToBytes(wav, ref bytePos, 0x57415645, Endian.BIG_ENDIAN);

            // Format Chunk

            // Chunk ID "fmt "
            writeUintToBytes(wav, ref bytePos, 0x666D7420, Endian.BIG_ENDIAN);

            // Chunk Data Size
            writeUintToBytes(wav, ref bytePos, 16, Endian.LITTLE_ENDIAN);

            // Compression Code PCM
            writeShortToBytes(wav, ref bytePos, 1, Endian.LITTLE_ENDIAN);
            // Number of channels
            writeShortToBytes(wav, ref bytePos, 1, Endian.LITTLE_ENDIAN);
            // Sample rate
            writeUintToBytes(wav, ref bytePos, __sampleRate, Endian.LITTLE_ENDIAN);
            // Average bytes per second
            writeUintToBytes(wav, ref bytePos, bytesPerSec, Endian.LITTLE_ENDIAN);
            // Block align
            writeShortToBytes(wav, ref bytePos, (short) blockAlign, Endian.LITTLE_ENDIAN);
            // Significant bits per sample
            writeShortToBytes(wav, ref bytePos, (short) __bitDepth, Endian.LITTLE_ENDIAN);

            // Data Chunk

            // Chunk ID "data"
            writeUintToBytes(wav, ref bytePos, 0x64617461, Endian.BIG_ENDIAN);
            // Chunk Data Size
            writeUintToBytes(wav, ref bytePos, soundLength, Endian.LITTLE_ENDIAN);

            // Write data as bytes
            var sampleCount = 0;
            var bufferSample = 0f;
            for (var i = 0; i < data.Length; i++)
            {
                bufferSample += data[i];
                sampleCount++;

                if (sampleCount == 2)
                {
                    bufferSample /= sampleCount;
                    sampleCount = 0;

                    writeBytes(wav, ref bytePos, new[] {(byte) (Math.Round(bufferSample * 127f) + 128)},
                        Endian.LITTLE_ENDIAN);

                    bufferSample = 0f;
                }
            }

            return wav;
        }

        /// <summary>
        ///     Writes a short (Int16) to a byte array.
        ///     This is an aux function used when creating the WAV data.
        /// </summary>
        /// <param name="__bytes"></param>
        /// <param name="__position"></param>
        /// <param name="__newShort"></param>
        /// <param name="__endian"></param>
        protected void writeShortToBytes(byte[] __bytes, ref int __position, short __newShort, Endian __endian)
        {
            writeBytes(__bytes, ref __position,
                new byte[2] {(byte) ((__newShort >> 8) & 0xff), (byte) (__newShort & 0xff)}, __endian);
        }

        /// <summary>
        ///     Writes a uint (UInt32) to a byte array.
        ///     This is an aux function used when creating the WAV data.
        /// </summary>
        /// <param name="__bytes"></param>
        /// <param name="__position"></param>
        /// <param name="__newUint"></param>
        /// <param name="__endian"></param>
        protected void writeUintToBytes(byte[] __bytes, ref int __position, uint __newUint, Endian __endian)
        {
            writeBytes(__bytes, ref __position,
                new byte[4]
                {
                    (byte) ((__newUint >> 24) & 0xff), (byte) ((__newUint >> 16) & 0xff),
                    (byte) ((__newUint >> 8) & 0xff), (byte) (__newUint & 0xff)
                }, __endian);
        }

        /// <summary>
        ///     Writes any number of bytes into a byte array, at a given position.
        ///     This is an aux function used when creating the WAV data.
        /// </summary>
        /// <param name="__bytes"></param>
        /// <param name="__position"></param>
        /// <param name="__newBytes"></param>
        /// <param name="__endian"></param>
        protected void writeBytes(byte[] __bytes, ref int __position, byte[] __newBytes, Endian __endian)
        {
            // Writes __newBytes to __bytes at position __position, increasing the position depending on the length of __newBytes
            for (var i = 0; i < __newBytes.Length; i++)
            {
                __bytes[__position] = __newBytes[__endian == Endian.BIG_ENDIAN ? i : __newBytes.Length - i - 1];
                __position++;
            }
        }

        protected enum Endian
        {
            BIG_ENDIAN,
            LITTLE_ENDIAN
        }


        /**
         * Cache the sound for speedy playback.
         * If a callback is passed in, the caching will be done asynchronously, taking maxTimePerFrame milliseconds
         * per frame to cache, them calling the callback when it's done.
         * If not, the whole sound is cached immediately - can freeze the player for a few seconds, especially in debug mode.
         * @param	callback			Function to call when the caching is complete
         * @param	maxTimePerFrame		Maximum time in milliseconds the caching will use per frame
         */
        public void CacheSound()
        {
            Stop();

            var paramKey = parameters.GetSettingsString();


            if (SoundInstanceCache.ContainsKey(paramKey))
            {
                _soundInstance = SoundInstanceCache[paramKey];
            }
            else
            {
                // Needs to cache new data
                //                _cachedWavePos = 0;
                //                _cachingNormal = true;
                //                _waveData = null;

                Reset(true);


                if (_soundInstance != null) _soundInstance.Stop();

                //                _waveData = GenerateWav();

                parameters.ResetValidation();

                using (var stream = new MemoryStream(GenerateWav()))
                {
                    var soundEffect = SoundEffect.FromStream(stream);

                    _soundInstance = soundEffect.CreateInstance();
                }

                SoundInstanceCache[paramKey] = _soundInstance;
            }
        }

        /**
         * Resets the runing variables from the params
         * Used once at the start (total reset) and for the repeat effect (partial reset)
         * @param	totalReset	If the reset is total
         */
        private void Reset(bool __totalReset)
        {
            // Shorter reference
            var p = parameters;

            _period = 100.0f / (p.startFrequency * p.startFrequency + 0.001f);
            _maxPeriod = 100.0f / (p.minFrequency * p.minFrequency + 0.001f);

            _slide = 1.0f - p.slide * p.slide * p.slide * 0.01f;
            _deltaSlide = -p.deltaSlide * p.deltaSlide * p.deltaSlide * 0.000001f;

            if (p.waveType == 0)
            {
                _squareDuty = 0.5f - p.squareDuty * 0.5f;
                _dutySweep = -p.dutySweep * 0.00005f;
            }

            _changePeriod = (1f - p.changeRepeat + 0.1f) / 1.1f * 20000f + 32f;
            _changePeriodTime = 0;

            if (p.changeAmount > 0.0)
                _changeAmount = 1.0f - p.changeAmount * p.changeAmount * 0.9f;
            else
                _changeAmount = 1.0f + p.changeAmount * p.changeAmount * 10.0f;

            _changeTime = 0;
            _changeReached = false;

            if (p.changeSpeed == 1.0f)
                _changeLimit = 0;
            else
                _changeLimit = (int) ((1f - p.changeSpeed) * (1f - p.changeSpeed) * 20000f + 32f);

            //            if (p.changeAmount2 > 0f)
            //                _changeAmount2 = 1f - p.changeAmount2 * p.changeAmount2 * 0.9f;
            //            else
            //                _changeAmount2 = 1f + p.changeAmount2 * p.changeAmount2 * 10f;

            //            _changeTime2 = 0;
            //            _changeReached2 = false;

            //            if (p.changeSpeed2 == 1.0f)
            //                _changeLimit2 = 0;
            //            else
            //                _changeLimit2 = (int) ((1f - p.changeSpeed2) * (1f - p.changeSpeed2) * 20000f + 32f);

            _changeLimit = (int) (_changeLimit * ((1f - p.changeRepeat + 0.1f) / 1.1f));
            //            _changeLimit2 = (int) (_changeLimit2 * ((1f - p.changeRepeat + 0.1f) / 1.1f));

            if (__totalReset)
            {
                p.ResetValidation();

                _masterVolume = p.masterVolume * p.masterVolume;

                waveType = p.waveType;

                if (p.sustainTime < 0.01) p.sustainTime = 0.01f;

                var totalTime = p.attackTime + p.sustainTime + p.decayTime;
                if (totalTime < 0.18f)
                {
                    var multiplier = 0.18f / totalTime;
                    p.attackTime *= multiplier;
                    p.sustainTime *= multiplier;
                    p.decayTime *= multiplier;
                }

                _sustainPunch = p.sustainPunch;

                _phase = 0;

                //                _overtones = (int) (p.overtones * 10f);
                //                _overtoneFalloff = p.overtoneFalloff;

                _minFrequency = p.minFrequency;

                //                _bitcrushFreq = 1f - (float)Math.Pow(p.bitCrush, 1f / 3f);
                //                _bitcrushFreqSweep = -p.bitCrushSweep * 0.000015f;
                //                _bitcrushPhase = 0;
                //                _bitcrushLast = 0;

                _compressionFactor = 1f / (1f + 4f * p.compressionAmount);

                _filters = p.lpFilterCutoff != 1.0 || p.hpFilterCutoff != 0.0;

                _lpFilterPos = 0.0f;
                _lpFilterDeltaPos = 0.0f;
                _lpFilterCutoff = p.lpFilterCutoff * p.lpFilterCutoff * p.lpFilterCutoff * 0.1f;
                _lpFilterDeltaCutoff = 1.0f + p.lpFilterCutoffSweep * 0.0001f;
                _lpFilterDamping = 5.0f / (1.0f + p.lpFilterResonance * p.lpFilterResonance * 20.0f) *
                                   (0.01f + _lpFilterCutoff);
                if (_lpFilterDamping > 0.8f) _lpFilterDamping = 0.8f;

                _lpFilterDamping = 1.0f - _lpFilterDamping;
                _lpFilterOn = p.lpFilterCutoff != 1.0f;

                _hpFilterPos = 0.0f;
                _hpFilterCutoff = p.hpFilterCutoff * p.hpFilterCutoff * 0.1f;
                _hpFilterDeltaCutoff = 1.0f + p.hpFilterCutoffSweep * 0.0003f;

                _vibratoPhase = 0.0f;
                _vibratoSpeed = p.vibratoSpeed * p.vibratoSpeed * 0.01f;
                _vibratoAmplitude = p.vibratoDepth * 0.5f;

                _envelopeVolume = 0.0f;
                _envelopeStage = 0;
                _envelopeTime = 0;
                _envelopeLength0 = p.attackTime * p.attackTime * 100000.0f;
                _envelopeLength1 = p.sustainTime * p.sustainTime * 100000.0f;
                _envelopeLength2 = p.decayTime * p.decayTime * 100000.0f + 10f;
                _envelopeLength = _envelopeLength0;
                _envelopeFullLength = (uint) (_envelopeLength0 + _envelopeLength1 + _envelopeLength2);

                _envelopeOverLength0 = 1.0f / _envelopeLength0;
                _envelopeOverLength1 = 1.0f / _envelopeLength1;
                _envelopeOverLength2 = 1.0f / _envelopeLength2;

                _phaser = p.phaserOffset != 0.0f || p.phaserSweep != 0.0f;

                _phaserOffset = p.phaserOffset * p.phaserOffset * 1020.0f;
                if (p.phaserOffset < 0.0f) _phaserOffset = -_phaserOffset;

                _phaserDeltaOffset = p.phaserSweep * p.phaserSweep * p.phaserSweep * 0.2f;
                _phaserPos = 0;

                if (_phaserBuffer == null) _phaserBuffer = new float[1024];

                if (_noiseBuffer == null) _noiseBuffer = new float[32];
                //                if (_pinkNoiseBuffer == null) _pinkNoiseBuffer = new float[32];
                //                if (_pinkNumber == null) _pinkNumber = new PinkNumber();
                if (_loResNoiseBuffer == null) _loResNoiseBuffer = new float[32];

                uint i;
                for (i = 0; i < 1024; i++) _phaserBuffer[i] = 0.0f;

                for (i = 0; i < 32; i++) _noiseBuffer[i] = parameters.GetRandom() * 2.0f - 1.0f;
                //                for (i = 0; i < 32; i++) _pinkNoiseBuffer[i] = _pinkNumber.getNextValue();
                for (i = 0; i < 32; i++)
                    _loResNoiseBuffer[i] = i % LO_RES_NOISE_PERIOD == 0
                        ? parameters.GetRandom() * 2.0f - 1.0f
                        : _loResNoiseBuffer[i - 1];

                _repeatTime = 0;

                if (p.repeatSpeed == 0.0)
                    _repeatLimit = 0;
                else
                    _repeatLimit = (int) ((1.0 - p.repeatSpeed) * (1.0 - p.repeatSpeed) * 20000) + 32;
            }
        }

        /**
         * Writes the wave to the supplied buffer array of floats (it'll contain the mono audio)
         * @param	buffer		A float[] to write the wave to
         * @param	waveData	If the wave should be written for the waveData
         * @return				If the wave is finished
         */
        private bool SynthWave(float[] __buffer, int __bufferPos, uint __length)
        {
            _finished = false;

            int i, j, n;
            var l = (int) __length;
            float tempPhase, sampleTotal;

            for (i = 0; i < l; i++)
            {
                if (_finished) return true;

                // Repeats every _repeatLimit times, partially resetting the sound parameters
                if (_repeatLimit != 0)
                    if (++_repeatTime >= _repeatLimit)
                    {
                        _repeatTime = 0;
                        Reset(false);
                    }

                _changePeriodTime++;
                if (_changePeriodTime >= _changePeriod)
                {
                    _changeTime = 0;
                    //                    _changeTime2 = 0;
                    _changePeriodTime = 0;
                    if (_changeReached)
                    {
                        _period /= _changeAmount;
                        _changeReached = false;
                    }

                    //                    if (_changeReached2)
                    //                    {
                    //                        _period /= _changeAmount2;
                    //                        _changeReached2 = false;
                    //                    }
                }

                // If _changeLimit is reached, shifts the pitch
                if (!_changeReached)
                    if (++_changeTime >= _changeLimit)
                    {
                        _changeReached = true;
                        _period *= _changeAmount;
                    }

                // If _changeLimit is reached, shifts the pitch
                //                if (!_changeReached2)
                //                    if (++_changeTime2 >= _changeLimit2)
                //                    {
                //                        _changeReached2 = true;
                //                        _period *= _changeAmount2;
                //                    }

                // Acccelerate and apply slide
                _slide += _deltaSlide;
                _period *= _slide;

                // Checks for frequency getting too low, and stops the sound if a minFrequency was set
                if (_period > _maxPeriod)
                {
                    _period = _maxPeriod;
                    if (_minFrequency > 0) _finished = true;
                }

                _periodTemp = _period;

                // Applies the vibrato effect
                if (_vibratoAmplitude > 0)
                {
                    _vibratoPhase += _vibratoSpeed;
                    _periodTemp = _period * (1.0f + (float) Math.Sin(_vibratoPhase) * _vibratoAmplitude);
                }

                _periodTempInt = (int) _periodTemp;
                if (_periodTemp < 8) _periodTemp = _periodTempInt = 8;

                // Sweeps the square duty
                if (waveType == 0)
                {
                    _squareDuty += _dutySweep;
                    if (_squareDuty < 0.0)
                        _squareDuty = 0.0f;
                    else if (_squareDuty > 0.5) _squareDuty = 0.5f;
                }

                // Moves through the different stages of the volume envelope
                if (++_envelopeTime > _envelopeLength)
                {
                    _envelopeTime = 0;

                    switch (++_envelopeStage)
                    {
                        case 1:
                            _envelopeLength = _envelopeLength1;
                            break;
                        case 2:
                            _envelopeLength = _envelopeLength2;
                            break;
                    }
                }

                // Sets the volume based on the position in the envelope
                switch (_envelopeStage)
                {
                    case 0:
                        _envelopeVolume = _envelopeTime * _envelopeOverLength0;
                        break;
                    case 1:
                        _envelopeVolume = 1.0f + (1.0f - _envelopeTime * _envelopeOverLength1) * 2.0f * _sustainPunch;
                        break;
                    case 2:
                        _envelopeVolume = 1.0f - _envelopeTime * _envelopeOverLength2;
                        break;
                    case 3:
                        _envelopeVolume = 0.0f;
                        _finished = true;
                        break;
                }

                // Moves the phaser offset
                if (_phaser)
                {
                    _phaserOffset += _phaserDeltaOffset;
                    _phaserInt = (int) _phaserOffset;
                    if (_phaserInt < 0)
                        _phaserInt = -_phaserInt;
                    else if (_phaserInt > 1023) _phaserInt = 1023;
                }

                // Moves the high-pass filter cutoff
                if (_filters && _hpFilterDeltaCutoff != 0)
                {
                    _hpFilterCutoff *= _hpFilterDeltaCutoff;
                    if (_hpFilterCutoff < 0.00001f)
                        _hpFilterCutoff = 0.00001f;
                    else if (_hpFilterCutoff > 0.1f) _hpFilterCutoff = 0.1f;
                }

                _superSample = 0;
                for (j = 0; j < 8; j++)
                {
                    // Cycles through the period
                    _phase++;
                    if (_phase >= _periodTempInt)
                    {
                        _phase = _phase % _periodTempInt;

                        // Generates new random noise for this period
                        if (waveType == WaveType.Noise)
                            for (n = 0; n < 32; n++)
                                _noiseBuffer[n] = parameters.GetRandom() * 2.0f - 1.0f;
                    }

                    _sample = 0;
                    sampleTotal = 0;
                    //                    overtoneStrength = 1f;

                    //                    for (k = 0; k <= _overtones; k++)
                    //                    {
                    tempPhase = _phase * (0 + 1) % _periodTemp;

                    // Gets the sample from the oscillator
                    switch (waveType)
                    {
                        case WaveType.Square:
                            // Square
                            _sample = tempPhase / _periodTemp < _squareDuty ? 0.5f : -0.5f;
                            break;
                        case WaveType.Saw:
                            // Sawtooth
                            _sample = 1.0f - tempPhase / _periodTemp * 2.0f;
                            break;
                        case WaveType.Sine:
                            // Sine: fast and accurate approx
                            _pos = tempPhase / _periodTemp;
                            _pos = _pos > 0.5f ? (_pos - 1.0f) * 6.28318531f : _pos * 6.28318531f;
                            _sample = _pos < 0
                                ? 1.27323954f * _pos + 0.405284735f * _pos * _pos
                                : 1.27323954f * _pos - 0.405284735f * _pos * _pos;
                            _sample = _sample < 0
                                ? 0.225f * (_sample * -_sample - _sample) + _sample
                                : 0.225f * (_sample * _sample - _sample) + _sample;
                            break;
                        case WaveType.Noise:
                            // Noise
                            _sample = _noiseBuffer[(uint) (tempPhase * 32f / _periodTempInt) % 32];
                            break;
                        case WaveType.Triangle:
                            // Triangle
                            _sample = Math.Abs(1f - tempPhase / _periodTemp * 2f) - 1f;
                            break;
                    }

                    sampleTotal += _sample;
                    //                        overtoneStrength *= 1f - _overtoneFalloff;
                    //                    }

                    _sample = sampleTotal;

                    // Applies the low and high pass filters
                    if (_filters)
                    {
                        _lpFilterOldPos = _lpFilterPos;
                        _lpFilterCutoff *= _lpFilterDeltaCutoff;
                        if (_lpFilterCutoff < 0.0)
                            _lpFilterCutoff = 0.0f;
                        else if (_lpFilterCutoff > 0.1) _lpFilterCutoff = 0.1f;

                        if (_lpFilterOn)
                        {
                            _lpFilterDeltaPos += (_sample - _lpFilterPos) * _lpFilterCutoff;
                            _lpFilterDeltaPos *= _lpFilterDamping;
                        }
                        else
                        {
                            _lpFilterPos = _sample;
                            _lpFilterDeltaPos = 0.0f;
                        }

                        _lpFilterPos += _lpFilterDeltaPos;

                        _hpFilterPos += _lpFilterPos - _lpFilterOldPos;
                        _hpFilterPos *= 1.0f - _hpFilterCutoff;
                        _sample = _hpFilterPos;
                    }

                    // Applies the phaser effect
                    if (_phaser)
                    {
                        _phaserBuffer[_phaserPos & 1023] = _sample;
                        _sample += _phaserBuffer[(_phaserPos - _phaserInt + 1024) & 1023];
                        _phaserPos = (_phaserPos + 1) & 1023;
                    }

                    _superSample += _sample;
                }

                // Averages out the super samples and applies volumes
                _superSample = _masterVolume * _envelopeVolume * _superSample * 0.125f;

                // Bit crush
                //                _bitcrushPhase += _bitcrushFreq;
                //                if (_bitcrushPhase > 1f)
                //                {
                //                    _bitcrushPhase = 0;
                //                    _bitcrushLast = _superSample;
                //                }
                //
                //                _bitcrushFreq = Math.Max(Math.Min(_bitcrushFreq + _bitcrushFreqSweep, 1f), 0f);
                //
                //                _superSample = _bitcrushLast;

                // Compressor
                if (_superSample > 0f)
                    _superSample = (float) Math.Pow(_superSample, _compressionFactor);
                else
                    _superSample = -(float) Math.Pow(-_superSample, _compressionFactor);

                // Clipping if too loud
                if (_superSample < -1f)
                    _superSample = -1f;
                else if (_superSample > 1f) _superSample = 1f;

                // Writes value to list, ignoring left/right sound channels (this is applied when filtering the audio later)
                __buffer[i + __bufferPos] = _superSample;
            }

            return false;
        }

    }
}