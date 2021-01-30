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
//  SfxrParams
//  Holds parameters used by SfxrSynth
//  
//  @author Zeh Fernando

using Microsoft.Xna.Framework;
using System;

namespace PixelVision8.Player.Audio
{
    public enum WaveType
    {
        None = -1,
        Square = 0,
        Saw = 1,
        Sine = 2,
        Noise = 3,
        Triangle = 4,
        Sample = 5
    }

    public class SfxrParams : AbstractData
    {
        private static readonly Random _random = new Random();

        private float _attackTime; // Length of the volume envelope attack (0 to 1)

        private float _changeAmount; // Shift in note, either up or down (-1 to 1)

        //        private float _changeAmount2; // Shift in note, either up or down (-1 to 1)
        private float
            _changeRepeat; // Pitch Jump Repeat Speed: larger Values means more pitch jumps, which can be useful for arpeggiation (0 to 1)

        private float _changeSpeed; // How fast the note shift happens (only happens once) (0 to 1)
        private float _changeSpeed2; // How fast the note shift happens (only happens once) (0 to 1)

        private float
            _compressionAmount; // Compression: pushes amplitudes together into a narrower range to make them stand out more. Very good for sound effects, where you want them to stick out against background music (0 to 1)

        private float _decayTime; // Length of the volume envelope decay (yes, I know it's called release) (0 to 1)
        private float _deltaSlide; // Accelerates the slide (-1 to 1)
        private float _dutySweep; // Sweeps the duty up or down (-1 to 1)

        private float
            _hpFilterCutoff; // Frequency at which the high-pass filter starts attenuating lower frequencies (0 to 1)

        private float _hpFilterCutoffSweep; // Sweeps the high-pass cutoff up or down (-1 to 1)

        private float
            _lpFilterCutoff; // Frequency at which the low-pass filter starts attenuating higher frequencies (0 to 1)

        private float _lpFilterCutoffSweep; // Sweeps the low-pass cutoff up or down (-1 to 1)

        private float
            _lpFilterResonance; // Changes the attenuation rate for the low-pass filter, changing the timbre (0 to 1)

        private float _masterVolume = 0.5f; // Overall volume of the sound (0 to 1)

        private float
            _minFrequency; // If sliding, the sound will stop at this frequency, to prevent really low notes (0 to 1)

        private float _phaserOffset; // Offsets a second copy of the wave by a small phase, changing the tibre (-1 to 1)
        private float _phaserSweep; // Sweeps the phase up or down (-1 to 1)
        private float _repeatSpeed; // Speed of the note repeating - certain variables are reset each time (0 to 1)
        private float _slide; // Slides the note up or down (-1 to 1)

        private float
            _squareDuty; // Controls the ratio between the up and down states of the square wave, changing the tibre (0 to 1)

        private float _startFrequency; // Base note of the sound (0 to 1)
        private float _sustainPunch; // Tilts the sustain envelope for more 'pop' (0 to 1)
        private float _sustainTime; // Length of the volume envelope sustain (0 to 1)
        private float _vibratoDepth; // Strength of the vibrato effect (0 to 1)
        private float _vibratoSpeed; // Speed of the vibrato effect (i.e. frequency) (0 to 1)

        private WaveType _waveType; // Shape of wave to generate (see enum WaveType)

        //        public bool
        //            paramsDirty; // Whether the parameters have been changed since last time (shouldn't used cached sound)

        /// <summary>
        ///     Shape of the wave (0:square, 1:sawtooth, 2:sin, 3:noise)
        /// </summary>
        public WaveType waveType
        {
            get => _waveType;
            set
            {
                _waveType = value; // > 8 ? 0 : value;
                Invalidate();
            }
        }

        /// <summary>
        ///     Overall volume of the sound (0 to 1)
        /// </summary>
        public float masterVolume
        {
            get => _masterVolume;
            set
            {
                _masterVolume = MathHelper.Clamp(value, 0, 1);
                Invalidate();
            }
        }

        /// <summary>
        ///     Length of the volume envelope attack (0 to 1)
        /// </summary>
        public float attackTime
        {
            get => _attackTime;
            set
            {
                _attackTime = MathHelper.Clamp(value, 0, 1);
                Invalidate();
            }
        }

        /// <summary>
        ///     Length of the volume envelope sustain (0 to 1)
        /// </summary>
        public float sustainTime
        {
            get => _sustainTime;
            set
            {
                _sustainTime = MathHelper.Clamp(value, 0, 1);
                Invalidate();
            }
        }

        /// <summary>
        ///     Tilts the sustain envelope for more 'pop' (0 to 1)
        /// </summary>
        public float sustainPunch
        {
            get => _sustainPunch;
            set
            {
                _sustainPunch = MathHelper.Clamp(value, 0, 1);
                Invalidate();
            }
        }

        /// <summary>
        ///     Length of the volume envelope decay (yes, I know it's called release) (0 to 1)
        /// </summary>
        public float decayTime
        {
            get => _decayTime;
            set
            {
                _decayTime = MathHelper.Clamp(value, 0, 1);
                Invalidate();
            }
        }

        /// <summary>
        ///     Base note of the sound (0 to 1)
        /// </summary>
        public float startFrequency
        {
            get => _startFrequency;
            set
            {
                _startFrequency = MathHelper.Clamp(value, 0, 1);
                Invalidate();
            }
        }

        /// <summary>
        ///     If sliding, the sound will stop at this frequency, to prevent really low notes (0 to 1)
        /// </summary>
        public float minFrequency
        {
            get => _minFrequency;
            set
            {
                _minFrequency = MathHelper.Clamp(value, 0, 1);
                Invalidate();
            }
        }

        /// <summary>
        ///     Slides the note up or down (-1 to 1)
        /// </summary>
        public float slide
        {
            get => _slide;
            set
            {
                _slide = MathHelper.Clamp(value, -1, 1);
                Invalidate();
            }
        }

        /// <summary>
        ///     Accelerates the slide (-1 to 1)
        /// </summary>
        public float deltaSlide
        {
            get => _deltaSlide;
            set
            {
                _deltaSlide = MathHelper.Clamp(value, -1, 1);
                Invalidate();
            }
        }

        /// <summary>
        ///     Strength of the vibrato effect (0 to 1)
        /// </summary>
        public float vibratoDepth
        {
            get => _vibratoDepth;
            set
            {
                _vibratoDepth = MathHelper.Clamp(value, 0, 1);
                Invalidate();
            }
        }

        /// <summary>
        ///     Speed of the vibrato effect (i.e. frequency) (0 to 1)
        /// </summary>
        public float vibratoSpeed
        {
            get => _vibratoSpeed;
            set
            {
                _vibratoSpeed = MathHelper.Clamp(value, 0, 1);
                Invalidate();
            }
        }

        /// <summary>
        ///     Shift in note, either up or down (-1 to 1)
        /// </summary>
        public float changeAmount
        {
            get => _changeAmount;
            set
            {
                _changeAmount = MathHelper.Clamp(value, -1, 1);
                Invalidate();
            }
        }

        /// <summary>
        ///     How fast the note shift happens (only happens once) (0 to 1)
        /// </summary>
        public float changeSpeed
        {
            get => _changeSpeed;
            set
            {
                _changeSpeed = MathHelper.Clamp(value, 0, 1);
                Invalidate();
            }
        }

        /// <summary>
        ///     Controls the ratio between the up and down states of the square wave, changing the tibre (0 to 1)
        /// </summary>
        public float squareDuty
        {
            get => _squareDuty;
            set
            {
                _squareDuty = MathHelper.Clamp(value, 0, 1);
                Invalidate();
            }
        }

        /// <summary>
        ///     Sweeps the duty up or down (-1 to 1)
        /// </summary>
        public float dutySweep
        {
            get => _dutySweep;
            set
            {
                _dutySweep = MathHelper.Clamp(value, -1, 1);
                Invalidate();
            }
        }

        /// <summary>
        ///     Speed of the note repeating - certain variables are reset each time (0 to 1)
        /// </summary>
        public float repeatSpeed
        {
            get => _repeatSpeed;
            set
            {
                _repeatSpeed = MathHelper.Clamp(value, 0, 1);
                Invalidate();
            }
        }

        /// <summary>
        ///     Offsets a second copy of the wave by a small phase, changing the tibre (-1 to 1)
        /// </summary>
        public float phaserOffset
        {
            get => _phaserOffset;
            set
            {
                _phaserOffset = MathHelper.Clamp(value, -1, 1);
                Invalidate();
            }
        }

        /// <summary>
        ///     Sweeps the phase up or down (-1 to 1)
        /// </summary>
        public float phaserSweep
        {
            get => _phaserSweep;
            set
            {
                _phaserSweep = MathHelper.Clamp(value, -1, 1);
                Invalidate();
            }
        }

        /// <summary>
        ///     Frequency at which the low-pass filter starts attenuating higher frequencies (0 to 1)
        /// </summary>
        public float lpFilterCutoff
        {
            get => _lpFilterCutoff;
            set
            {
                _lpFilterCutoff = MathHelper.Clamp(value, 0, 1);
                Invalidate();
            }
        }

        /// <summary>
        ///     Sweeps the low-pass cutoff up or down (-1 to 1)
        /// </summary>
        public float lpFilterCutoffSweep
        {
            get => _lpFilterCutoffSweep;
            set
            {
                _lpFilterCutoffSweep = MathHelper.Clamp(value, -1, 1);
                Invalidate();
            }
        }

        /// <summary>
        ///     Changes the attenuation rate for the low-pass filter, changing the timbre (0 to 1)
        /// </summary>
        public float lpFilterResonance
        {
            get => _lpFilterResonance;
            set
            {
                _lpFilterResonance = MathHelper.Clamp(value, 0, 1);
                Invalidate();
            }
        }

        /// <summary>
        ///     Frequency at which the high-pass filter starts attenuating lower frequencies (0 to 1)
        /// </summary>
        public float hpFilterCutoff
        {
            get => _hpFilterCutoff;
            set
            {
                _hpFilterCutoff = MathHelper.Clamp(value, 0, 1);
                Invalidate();
            }
        }

        /// <summary>
        ///     Sweeps the high-pass cutoff up or down (-1 to 1)
        /// </summary>
        public float hpFilterCutoffSweep
        {
            get => _hpFilterCutoffSweep;
            set
            {
                _hpFilterCutoffSweep = MathHelper.Clamp(value, -1, 1);
                Invalidate();
            }
        }

        // From BFXR

        /// <summary>
        ///     Pitch Jump Repeat Speed: larger Values means more pitch jumps, which can be useful for arpeggiation (0 to 1)
        /// </summary>
        public float changeRepeat
        {
            get => _changeRepeat;
            set
            {
                _changeRepeat = MathHelper.Clamp(value, 0, 1);
                Invalidate();
            }
        }

        /// <summary>
        ///     How fast the note shift happens (only happens once) (0 to 1)
        /// </summary>
        public float changeSpeed2
        {
            get => _changeSpeed2;
            set
            {
                _changeSpeed2 = MathHelper.Clamp(value, 0, 1);
                Invalidate();
            }
        }

        /// <summary>
        ///     Pushes amplitudes together into a narrower range to make them stand out more. Very good for sound effects, where
        ///     you want them to stick out against background music (0 to 1)
        /// </summary>
        public float compressionAmount
        {
            get => _compressionAmount;
            set
            {
                _compressionAmount = MathHelper.Clamp(value, 0, 1);
                Invalidate();
            }
        }

        /// <summary>
        ///     Sets the parameters to generate a pickup/coin sound
        /// </summary>
        public void GeneratePickupCoin()
        {
            resetParams();

            _startFrequency = 0.4f + GetRandom() * 0.5f;

            _sustainTime = GetRandom() * 0.1f;
            _decayTime = 0.1f + GetRandom() * 0.4f;
            _sustainPunch = 0.3f + GetRandom() * 0.3f;

            if (GetRandomBool())
            {
                _changeSpeed = 0.5f + GetRandom() * 0.2f;
                var cnum = (int) (GetRandom() * 7f) + 1;
                var cden = cnum + (int) (GetRandom() * 7f) + 2;
                _changeAmount = cnum / (float) cden;
            }
        }

        /// <summary>
        ///     Sets the parameters to generate a laser/shoot sound
        /// </summary>
        public void GenerateLaserShoot()
        {
            resetParams();

            _waveType = (WaveType) (GetRandom() * 3);

            // TODO need to remove sine
            if (_waveType == WaveType.Sine && GetRandomBool()) _waveType = (WaveType) (GetRandom() * 2f);

            _startFrequency = 0.5f + GetRandom() * 0.5f;
            _minFrequency = _startFrequency - 0.2f - GetRandom() * 0.6f;
            if (_minFrequency < 0.2f) _minFrequency = 0.2f;

            _slide = -0.15f - GetRandom() * 0.2f;

            if (GetRandom() < 0.33f)
            {
                _startFrequency = 0.3f + GetRandom() * 0.6f;
                _minFrequency = GetRandom() * 0.1f;
                _slide = -0.35f - GetRandom() * 0.3f;
            }

            if (GetRandomBool())
            {
                _squareDuty = GetRandom() * 0.5f;
                _dutySweep = GetRandom() * 0.2f;
            }
            else
            {
                _squareDuty = 0.4f + GetRandom() * 0.5f;
                _dutySweep = -GetRandom() * 0.7f;
            }

            _sustainTime = 0.1f + GetRandom() * 0.2f;
            _decayTime = GetRandom() * 0.4f;
            if (GetRandomBool()) _sustainPunch = GetRandom() * 0.3f;

            if (GetRandom() < 0.33f)
            {
                _phaserOffset = GetRandom() * 0.2f;
                _phaserSweep = -GetRandom() * 0.2f;
            }

            if (GetRandomBool()) _hpFilterCutoff = GetRandom() * 0.3f;
        }

        /// <summary>
        ///     Sets the parameters to generate an explosion sound
        /// </summary>
        public void GenerateExplosion()
        {
            resetParams();

            _waveType = WaveType.Noise;

            if (GetRandomBool())
            {
                _startFrequency = 0.1f + GetRandom() * 0.4f;
                _slide = -0.1f + GetRandom() * 0.4f;
            }
            else
            {
                _startFrequency = 0.2f + GetRandom() * 0.7f;
                _slide = -0.2f - GetRandom() * 0.2f;
            }

            _startFrequency *= _startFrequency;

            if (GetRandom() < 0.2f) _slide = 0.0f;

            if (GetRandom() < 0.33f) _repeatSpeed = 0.3f + GetRandom() * 0.5f;

            _sustainTime = 0.1f + GetRandom() * 0.3f;
            _decayTime = GetRandom() * 0.5f;
            _sustainPunch = 0.2f + GetRandom() * 0.6f;

            if (GetRandomBool())
            {
                _phaserOffset = -0.3f + GetRandom() * 0.9f;
                _phaserSweep = -GetRandom() * 0.3f;
            }

            if (GetRandom() < 0.33f)
            {
                _changeSpeed = 0.6f + GetRandom() * 0.3f;
                _changeAmount = 0.8f - GetRandom() * 1.6f;
            }
        }

        /// <summary>
        ///     Sets the parameters to generate a powerup sound
        /// </summary>
        public void GeneratePowerup()
        {
            resetParams();

            if (GetRandomBool())
                _waveType = WaveType.Saw;
            else
                _squareDuty = GetRandom() * 0.6f;

            if (GetRandomBool())
            {
                _startFrequency = 0.2f + GetRandom() * 0.3f;
                _slide = 0.1f + GetRandom() * 0.4f;
                _repeatSpeed = 0.4f + GetRandom() * 0.4f;
            }
            else
            {
                _startFrequency = 0.2f + GetRandom() * 0.3f;
                _slide = 0.05f + GetRandom() * 0.2f;

                if (GetRandomBool())
                {
                    _vibratoDepth = GetRandom() * 0.7f;
                    _vibratoSpeed = GetRandom() * 0.6f;
                }
            }

            _sustainTime = GetRandom() * 0.4f;
            _decayTime = 0.1f + GetRandom() * 0.4f;
        }

        /// <summary>
        ///     Sets the parameters to generate a hit/hurt sound
        /// </summary>
        public void GenerateHitHurt()
        {
            resetParams();

            _waveType = (WaveType) (GetRandom() * 3f);
            if (_waveType == WaveType.Sine)
                _waveType = WaveType.Noise;
            else if (_waveType == 0) _squareDuty = GetRandom() * 0.6f;

            _startFrequency = 0.2f + GetRandom() * 0.6f;
            _slide = -0.3f - GetRandom() * 0.4f;

            _sustainTime = GetRandom() * 0.1f;
            _decayTime = 0.1f + GetRandom() * 0.2f;

            if (GetRandomBool()) _hpFilterCutoff = GetRandom() * 0.3f;
        }

        /// <summary>
        ///     Sets the parameters to generate a jump sound
        /// </summary>
        public void GenerateJump()
        {
            resetParams();

            _waveType = 0;
            _squareDuty = GetRandom() * 0.6f;
            _startFrequency = 0.3f + GetRandom() * 0.3f;
            _slide = 0.1f + GetRandom() * 0.2f;

            _sustainTime = 0.1f + GetRandom() * 0.3f;
            _decayTime = 0.1f + GetRandom() * 0.2f;

            if (GetRandomBool()) _hpFilterCutoff = GetRandom() * 0.3f;

            if (GetRandomBool()) _lpFilterCutoff = 1.0f - GetRandom() * 0.6f;
        }

        /// <summary>
        ///     Sets the parameters to generate a blip/select sound
        /// </summary>
        public void GenerateBlipSelect()
        {
            resetParams();

            _waveType = (WaveType) (GetRandom() * 2f);
            if (_waveType == 0) _squareDuty = GetRandom() * 0.6f;

            _startFrequency = 0.2f + GetRandom() * 0.4f;

            _sustainTime = 0.1f + GetRandom() * 0.1f;
            _decayTime = GetRandom() * 0.2f;
            _hpFilterCutoff = 0.1f;
        }

        /// <summary>
        ///     Resets the parameters, used at the start of each generate function
        /// </summary>
        protected void resetParams()
        {
            Invalidate();

            _waveType = 0;
            _startFrequency = 0.3f;
            _minFrequency = 0.0f;
            _slide = 0.0f;
            _deltaSlide = 0.0f;
            _squareDuty = 0.0f;
            _dutySweep = 0.0f;

            _vibratoDepth = 0.0f;
            _vibratoSpeed = 0.0f;

            _attackTime = 0.0f;
            _sustainTime = 0.3f;
            _decayTime = 0.4f;
            _sustainPunch = 0.0f;

            _lpFilterResonance = 0.0f;
            _lpFilterCutoff = 1.0f;
            _lpFilterCutoffSweep = 0.0f;
            _hpFilterCutoff = 0.0f;
            _hpFilterCutoffSweep = 0.0f;

            _phaserOffset = 0.0f;
            _phaserSweep = 0.0f;

            _repeatSpeed = 0.0f;

            _changeSpeed = 0.0f;
            _changeAmount = 0.0f;

            // From BFXR
            _changeRepeat = 0.0f;
            //            _changeAmount2 = 0.0f;
            _changeSpeed2 = 0.0f;

            _compressionAmount = 0.3f;

            //            _overtones = 0.0f;
            //            _overtoneFalloff = 0.0f;

            //            _bitCrush = 0.0f;
            //            _bitCrushSweep = 0.0f;
        }

        /// <summary>
        ///     Randomly adjusts the parameters ever so slightly
        /// </summary>
        /// <param name="__mutation"></param>
        public void Mutate(float __mutation = 0.05f)
        {
            if (GetRandomBool()) startFrequency += GetRandom() * __mutation * 2f - __mutation;

            if (GetRandomBool()) minFrequency += GetRandom() * __mutation * 2f - __mutation;

            if (GetRandomBool()) slide += GetRandom() * __mutation * 2f - __mutation;

            if (GetRandomBool()) deltaSlide += GetRandom() * __mutation * 2f - __mutation;

            if (GetRandomBool()) squareDuty += GetRandom() * __mutation * 2f - __mutation;

            if (GetRandomBool()) dutySweep += GetRandom() * __mutation * 2f - __mutation;

            if (GetRandomBool()) vibratoDepth += GetRandom() * __mutation * 2f - __mutation;

            if (GetRandomBool()) vibratoSpeed += GetRandom() * __mutation * 2f - __mutation;

            if (GetRandomBool()) attackTime += GetRandom() * __mutation * 2f - __mutation;

            if (GetRandomBool()) sustainTime += GetRandom() * __mutation * 2f - __mutation;

            if (GetRandomBool()) decayTime += GetRandom() * __mutation * 2f - __mutation;

            if (GetRandomBool()) sustainPunch += GetRandom() * __mutation * 2f - __mutation;

            if (GetRandomBool()) lpFilterCutoff += GetRandom() * __mutation * 2f - __mutation;

            if (GetRandomBool()) lpFilterCutoffSweep += GetRandom() * __mutation * 2f - __mutation;

            if (GetRandomBool()) lpFilterResonance += GetRandom() * __mutation * 2f - __mutation;

            if (GetRandomBool()) hpFilterCutoff += GetRandom() * __mutation * 2f - __mutation;

            if (GetRandomBool()) hpFilterCutoffSweep += GetRandom() * __mutation * 2f - __mutation;

            if (GetRandomBool()) phaserOffset += GetRandom() * __mutation * 2f - __mutation;

            if (GetRandomBool()) phaserSweep += GetRandom() * __mutation * 2f - __mutation;

            if (GetRandomBool()) repeatSpeed += GetRandom() * __mutation * 2f - __mutation;

            if (GetRandomBool()) changeSpeed += GetRandom() * __mutation * 2f - __mutation;

            if (GetRandomBool()) changeAmount += GetRandom() * __mutation * 2f - __mutation;

            // From BFXR
            if (GetRandomBool()) changeRepeat += GetRandom() * __mutation * 2f - __mutation;
            //            if (GetRandomBool()) changeAmount2 += GetRandom() * __mutation * 2f - __mutation;
            if (GetRandomBool()) changeSpeed2 += GetRandom() * __mutation * 2f - __mutation;

            if (GetRandomBool()) compressionAmount += GetRandom() * __mutation * 2f - __mutation;
        }

        /// <summary>
        ///     Sets all parameters to random values
        /// </summary>
        public void Randomize()
        {
            resetParams();

            // TODO Need to make sure this stays within range
            _waveType = (WaveType) (GetRandom() * 9f);

            _attackTime = Pow(GetRandom() * 2f - 1f, 4);
            _sustainTime = Pow(GetRandom() * 2f - 1f, 2);
            _sustainPunch = Pow(GetRandom() * 0.8f, 2);
            _decayTime = GetRandom();

            _startFrequency = GetRandomBool() ? Pow(GetRandom() * 2f - 1f, 2) : Pow(GetRandom() * 0.5f, 3) + 0.5f;
            _minFrequency = 0.0f;

            _slide = Pow(GetRandom() * 2f - 1f, 3);
            _deltaSlide = Pow(GetRandom() * 2f - 1f, 3);

            _vibratoDepth = Pow(GetRandom() * 2f - 1f, 3);
            _vibratoSpeed = GetRandom() * 2f - 1f;

            _changeAmount = GetRandom() * 2f - 1f;
            _changeSpeed = GetRandom() * 2f - 1f;

            _squareDuty = GetRandom() * 2f - 1f;
            _dutySweep = Pow(GetRandom() * 2f - 1f, 3);

            _repeatSpeed = GetRandom() * 2f - 1f;

            _phaserOffset = Pow(GetRandom() * 2f - 1f, 3);
            _phaserSweep = Pow(GetRandom() * 2f - 1f, 3);

            _lpFilterCutoff = 1f - Pow(GetRandom(), 3);
            _lpFilterCutoffSweep = Pow(GetRandom() * 2f - 1f, 3);
            _lpFilterResonance = GetRandom() * 2f - 1f;

            _hpFilterCutoff = Pow(GetRandom(), 5);
            _hpFilterCutoffSweep = Pow(GetRandom() * 2f - 1f, 5);

            if (_attackTime + _sustainTime + _decayTime < 0.2f)
            {
                _sustainTime = 0.2f + GetRandom() * 0.3f;
                _decayTime = 0.2f + GetRandom() * 0.3f;
            }

            if (_startFrequency > 0.7f && _slide > 0.2 || _startFrequency < 0.2 && _slide < -0.05) _slide = -_slide;

            if (_lpFilterCutoff < 0.1f && _lpFilterCutoffSweep < -0.05f) _lpFilterCutoffSweep = -_lpFilterCutoffSweep;

            // From BFXR
            _changeRepeat = GetRandom();
            //            _changeAmount2 = GetRandom() * 2f - 1f;
            _changeSpeed2 = GetRandom();

            _compressionAmount = GetRandom();
        }

        // Setting string methods

        /// <summary>
        ///     Returns a string representation of the parameters for copy/paste sharing in the old format (24 parameters,
        ///     SFXR/AS3SFXR compatible)
        /// </summary>
        /// <returns>A comma-delimited list of parameter values</returns>
        public string GetSettingsString()
        {
            var str = "";

            // 24 params

            str += (int) waveType + ",";
            str += To4DP(_attackTime) + ",";
            str += To4DP(_sustainTime) + ",";
            str += To4DP(_sustainPunch) + ",";
            str += To4DP(_decayTime) + ",";
            str += To4DP(_startFrequency) + ",";
            str += To4DP(_minFrequency) + ",";
            str += To4DP(_slide) + ",";
            str += To4DP(_deltaSlide) + ",";
            str += To4DP(_vibratoDepth) + ",";
            str += To4DP(_vibratoSpeed) + ",";
            str += To4DP(_changeAmount) + ",";
            str += To4DP(_changeSpeed) + ",";
            str += To4DP(_squareDuty) + ",";
            str += To4DP(_dutySweep) + ",";
            str += To4DP(_repeatSpeed) + ",";
            str += To4DP(_phaserOffset) + ",";
            str += To4DP(_phaserSweep) + ",";
            str += To4DP(_lpFilterCutoff) + ",";
            str += To4DP(_lpFilterCutoffSweep) + ",";
            str += To4DP(_lpFilterResonance) + ",";
            str += To4DP(_hpFilterCutoff) + ",";
            str += To4DP(_hpFilterCutoffSweep) + ",";
            str += To4DP(_masterVolume);

            return str;
        }

        /// <summary>
        ///     Parses a settings string into the parameters
        /// </summary>
        /// <param name="__string">string Settings string to parse</param>
        /// <returns>If the string successfully parsed</returns>
        public void SetSettingsString(string __string)
        {
            var values = __string.Split(',');

            resetParams();

            // Parse the params if there are enough values
            if (values.Length == 24)
            {
                waveType = (WaveType) ParseUint(values[0]);
                attackTime = ParseFloat(values[1]);
                sustainTime = ParseFloat(values[2]);
                sustainPunch = ParseFloat(values[3]);
                decayTime = ParseFloat(values[4]);
                startFrequency = ParseFloat(values[5]);
                minFrequency = ParseFloat(values[6]);
                slide = ParseFloat(values[7]);
                deltaSlide = ParseFloat(values[8]);
                vibratoDepth = ParseFloat(values[9]);
                vibratoSpeed = ParseFloat(values[10]);
                changeAmount = ParseFloat(values[11]);
                changeSpeed = ParseFloat(values[12]);
                squareDuty = ParseFloat(values[13]);
                dutySweep = ParseFloat(values[14]);
                repeatSpeed = ParseFloat(values[15]);
                phaserOffset = ParseFloat(values[16]);
                phaserSweep = ParseFloat(values[17]);
                lpFilterCutoff = ParseFloat(values[18]);
                lpFilterCutoffSweep = ParseFloat(values[19]);
                lpFilterResonance = ParseFloat(values[20]);
                hpFilterCutoff = ParseFloat(values[21]);
                hpFilterCutoffSweep = ParseFloat(values[22]);
                masterVolume = ParseFloat(values[23]);
            }
        }

        // Copying methods

        /// <summary>
        ///     Returns a copy of this SfxrParams with all settings duplicated
        /// </summary>
        /// <returns>A copy of this SfxrParams</returns>
        public SfxrParams Clone()
        {
            var outp = new SfxrParams();
            outp.CopyFrom(this);

            return outp;
        }

        /// <summary>
        ///     Copies parameters from another instance
        /// </summary>
        /// <param name="__params">Instance to copy parameters from</param>
        /// <param name="__makeDirty"></param>
        public void CopyFrom(SfxrParams __params, bool __makeDirty = false)
        {
            var wasDirty = Invalid;
            SetSettingsString(GetSettingsString());
            Invalid = wasDirty || __makeDirty;
        }

        // Utility methods

        /// <summary>
        ///     Faster power function; this function takes about 36% of the time Mathf.Pow() would take in our use cases
        /// </summary>
        /// <param name="__pbase">base		Base to raise to power</param>
        /// <param name="__power">power		Power to raise base by</param>
        /// <returns>The calculated power</returns>
        private float Pow(float __pbase, int __power)
        {
            switch (__power)
            {
                case 2: return __pbase * __pbase;
                case 3: return __pbase * __pbase * __pbase;
                case 4: return __pbase * __pbase * __pbase * __pbase;
                case 5: return __pbase * __pbase * __pbase * __pbase * __pbase;
            }

            return 1f;
        }

        /// <summary>
        ///     Returns the number as a string to 4 decimal places
        /// </summary>
        /// <param name="__value">Number to convert</param>
        /// <returns>Number to 4dp as a string</returns>
        private string To4DP(float __value)
        {
            if (__value < 0.0001f && __value > -0.0001f) return "";

            return __value.ToString("#.####");
        }

        /// <summary>
        ///     Parses a string into an uint value; also returns 0 if the string is empty, rather than an error
        /// </summary>
        /// <param name="__value"></param>
        /// <returns></returns>
        private uint ParseUint(string __value)
        {
            if (__value.Length == 0) return 0;

            return uint.Parse(__value);
        }

        /// <summary>
        ///     Parses a string into a float value; also returns 0 if the string is empty, rather than an error
        /// </summary>
        /// <param name="__value"></param>
        /// <returns></returns>
        private float ParseFloat(string __value)
        {
            if (__value.Length == 0) return 0;

            return float.Parse(__value);
        }

        /// <summary>
        ///     Returns a random value: 0 <= n < 1
        /// </summary>
        /// <returns></returns>
        public float GetRandom()
        {
            return (float) _random.NextDouble();
        }

        /// <summary>
        ///     Returns a random bool
        /// </summary>
        /// <returns></returns>
        private bool GetRandomBool()
        {
            return GetRandom() < .5f;
        }
    }
}