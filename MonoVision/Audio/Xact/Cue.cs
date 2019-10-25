// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Audio
{
    /// <summary>Manages the playback of a sound or set of sounds.</summary>
    /// <remarks>
    /// <para>Cues are comprised of one or more sounds.</para>
    /// <para>Cues also define specific properties such as pitch or volume.</para>
    /// <para>Cues are referenced through SoundBank objects.</para>
    /// </remarks>
    public class Cue : IDisposable
    {
        private readonly AudioEngine _engine;

        private readonly XactSound[] _sounds;
        private readonly float[] _probs;

        private readonly RpcVariable[] _variables;

        private XactSound _curSound;


        /// <summary>Indicates whether or not the cue is currently stopped.</summary>
        public bool IsStopped
        {
            get
            {
                if (_curSound != null)
                    return _curSound.Stopped;

                return !IsDisposed && !IsPrepared;
            }
        }

        public bool IsPrepared { get; internal set; }

        public bool IsCreated { get; internal set; }


        internal Cue(AudioEngine engine, string cuename, XactSound sound)
        {
            _engine = engine;
            _sounds = new XactSound[1];
            _sounds[0] = sound;
            _probs = new float[1];
            _probs[0] = 1.0f;
            _variables = engine.CreateCueVariables();
        }

        internal Cue(AudioEngine engine, string cuename, XactSound[] sounds, float[] probs)
        {
            _engine = engine;
            _sounds = sounds;
            _probs = probs;
            _variables = engine.CreateCueVariables();
        }

        internal void Prepare()
        {
            IsDisposed = false;
            IsCreated = false;
            IsPrepared = true;
            _curSound = null;
        }

        /// <summary>Requests playback of a prepared or preparing Cue.</summary>
        /// <remarks>Calling Play when the Cue already is playing can result in an InvalidOperationException.</remarks>
        public void Play()
        {
            lock (_engine.UpdateLock)
            {
                if (!_engine.ActiveCues.Contains(this))
                    _engine.ActiveCues.Add(this);

                //TODO: Probabilities
                var index = XactHelpers.Random.Next(_sounds.Length);
                _curSound = _sounds[index];

                var volume = UpdateRpcCurves();

                _curSound.Play(volume, _engine);
            }

            //            _played = true;
            IsPrepared = false;
        }

        /// <summary>Stops playback of a Cue.</summary>
        /// <param name="options">Specifies if the sound should play any pending release phases or transitions before stopping.</param>
        public void Stop(AudioStopOptions options)
        {
            lock (_engine.UpdateLock)
            {
                _engine.ActiveCues.Remove(this);

                if (_curSound != null)
                    _curSound.Stop(options);
            }

            IsPrepared = false;
        }

        internal void Update(float dt)
        {
            if (_curSound == null)
                return;

            _curSound.Update(dt);

            UpdateRpcCurves();
        }

        private float UpdateRpcCurves()
        {
            var volume = 1.0f;

            // Evaluate the runtime parameter controls.
            var rpcCurves = _curSound.RpcCurves;
            if (rpcCurves.Length > 0)
            {
                var pitch = 0.0f;
                var reverbMix = 1.0f;
                float? filterFrequency = null;
                float? filterQFactor = null;

                for (var i = 0; i < rpcCurves.Length; i++)
                {
                    var rpcCurve = _engine.RpcCurves[rpcCurves[i]];

                    // Some curves are driven by global variables and others by cue instance variables.
                    float value;
                    if (rpcCurve.IsGlobal)
                        value = rpcCurve.Evaluate(_engine.GetGlobalVariable(rpcCurve.Variable));
                    else
                        value = rpcCurve.Evaluate(_variables[rpcCurve.Variable].Value);

                    // Process the final curve value based on the parameter type it is.
                    switch (rpcCurve.Parameter)
                    {
                        case RpcParameter.Volume:
                            volume *= XactHelpers.ParseVolumeFromDecibels(value / 100.0f);
                            break;

                        case RpcParameter.Pitch:
                            pitch += value / 1000.0f;
                            break;

                        case RpcParameter.ReverbSend:
                            reverbMix *= XactHelpers.ParseVolumeFromDecibels(value / 100.0f);
                            break;

                        case RpcParameter.FilterFrequency:
                            filterFrequency = value;
                            break;

                        case RpcParameter.FilterQFactor:
                            filterQFactor = value;
                            break;

                        default:
                            throw new ArgumentOutOfRangeException("rpcCurve.Parameter");
                    }
                }

                pitch = MathHelper.Clamp(pitch, -1.0f, 1.0f);
                if (volume < 0.0f)
                    volume = 0.0f;

                _curSound.UpdateState(_engine, volume, pitch, reverbMix, filterFrequency, filterQFactor);
            }

            return volume;
        }

        /// <summary>
        /// This event is triggered when the Cue is disposed.
        /// </summary>
        public event EventHandler<EventArgs> Disposing;

        /// <summary>
        /// Is true if the Cue has been disposed.
        /// </summary>
        public bool IsDisposed { get; internal set; }

        /// <summary>
        /// Disposes the Cue.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;

            IsDisposed = true;

            if (disposing)
            {
                IsCreated = false;
                IsPrepared = false;
                EventHelpers.Raise(this, Disposing, EventArgs.Empty);
            }
        }
    }
}

