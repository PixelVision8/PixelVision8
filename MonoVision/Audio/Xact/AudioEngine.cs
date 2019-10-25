// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.Xna.Framework.Audio
{
    /// <summary>
    /// Class used to create and manipulate code audio objects.
    /// </summary> 
    public class AudioEngine : IDisposable
    {
        private readonly AudioCategory[] _categories;
        //        private readonly Dictionary<string, int> _categoryLookup = new Dictionary<string, int>();

        private readonly RpcVariable[] _variables;
        //        private readonly Dictionary<string, int> _variableLookup = new Dictionary<string, int>();

        private readonly RpcVariable[] _cueVariables;

        private readonly Stopwatch _stopwatch;
        private TimeSpan _lastUpdateTime;

        private readonly ReverbSettings _reverbSettings;
        private readonly RpcCurve[] _reverbCurves;

        internal List<Cue> ActiveCues = new List<Cue>();

        internal AudioCategory[] Categories { get { return _categories; } }

        internal Dictionary<string, WaveBank> Wavebanks = new Dictionary<string, WaveBank>();

        internal readonly RpcCurve[] RpcCurves;

        internal readonly object UpdateLock = new object();

        internal RpcVariable[] CreateCueVariables()
        {
            var clone = new RpcVariable[_cueVariables.Length];
            Array.Copy(_cueVariables, clone, _cueVariables.Length);
            return clone;
        }


        internal int GetRpcIndex(uint fileOffset)
        {
            for (var i = 0; i < RpcCurves.Length; i++)
            {
                if (RpcCurves[i].FileOffset == fileOffset)
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// Performs periodic work required by the audio engine.
        /// </summary>
        /// <remarks>Must be called at least once per frame.</remarks>
        public void Update()
        {
            var cur = _stopwatch.Elapsed;
            var elapsed = cur - _lastUpdateTime;
            _lastUpdateTime = cur;
            var dt = (float)elapsed.TotalSeconds;

            lock (UpdateLock)
            {
                for (var x = 0; x < ActiveCues.Count;)
                {
                    var cue = ActiveCues[x];

                    cue.Update(dt);

                    if (cue.IsStopped || cue.IsDisposed)
                    {
                        ActiveCues.Remove(cue);
                        continue;
                    }

                    x++;
                }
            }

            // The only global curves we can process seem to be 
            // specifically for the reverb DSP effect.
            if (_reverbSettings != null)
            {
                for (var i = 0; i < _reverbCurves.Length; i++)
                {
                    var curve = _reverbCurves[i];
                    var result = curve.Evaluate(_variables[curve.Variable].Value);
                    var parameter = curve.Parameter - RpcParameter.NumParameters;
                    _reverbSettings[parameter] = result;
                }

                SoundEffect.PlatformSetReverbSettings(_reverbSettings);
            }
        }


        internal float GetGlobalVariable(int index)
        {
            lock (UpdateLock)
                return _variables[index].Value;
        }

        /// <summary>
        /// This event is triggered when the AudioEngine is disposed.
        /// </summary>
        public event EventHandler<EventArgs> Disposing;

        /// <summary>
        /// Is true if the AudioEngine has been disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Disposes the AudioEngine.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~AudioEngine()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;

            IsDisposed = true;

            // TODO: Should we be forcing any active
            // audio cues to stop here?

            if (disposing)
                EventHelpers.Raise(this, Disposing, EventArgs.Empty);
        }
    }
}

