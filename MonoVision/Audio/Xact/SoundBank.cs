// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;

namespace Microsoft.Xna.Framework.Audio
{
    /// <summary>Represents a collection of Cues.</summary>
    public class SoundBank : IDisposable
    {
        readonly AudioEngine _audioengine;
        readonly string[] _waveBankNames;
        readonly WaveBank[] _waveBanks;

        /// <summary>
        /// Is true if the SoundBank has any live Cues in use.
        /// </summary>
        public bool IsInUse { get; private set; }

        internal SoundEffectInstance GetSoundEffectInstance(int waveBankIndex, int trackIndex, out bool streaming)
        {
            var waveBank = _waveBanks[waveBankIndex];

            // If the wave bank has not been resolved then do so now.
            if (waveBank == null)
            {
                var name = _waveBankNames[waveBankIndex];
                if (!_audioengine.Wavebanks.TryGetValue(name, out waveBank))
                    throw new Exception("The wave bank '" + name + "' was not found!");
                _waveBanks[waveBankIndex] = waveBank;
            }

            return waveBank.GetSoundEffectInstance(trackIndex, out streaming);
        }

        /// <summary>
        /// This event is triggered when the SoundBank is disposed.
        /// </summary>
        public event EventHandler<EventArgs> Disposing;

        /// <summary>
        /// Is true if the SoundBank has been disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Disposes the SoundBank.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~SoundBank()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;

            IsDisposed = true;

            if (disposing)
            {
                IsInUse = false;
                EventHelpers.Raise(this, Disposing, EventArgs.Empty);
            }
        }
    }
}

