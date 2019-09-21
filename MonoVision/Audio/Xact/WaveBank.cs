// MonoGame - Copyright (C) The MonoGame Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

using System;
using System.IO;

namespace Microsoft.Xna.Framework.Audio
{
    /// <summary>Represents a collection of wave files.</summary>
    public partial class WaveBank : IDisposable
    {
        private readonly SoundEffect[] _sounds;
        private readonly StreamInfo[] _streams;
        
        private readonly bool _streaming;
        
        struct StreamInfo
        {
            public int Format;
        }

        /// <summary>
        /// </summary>
        public bool IsInUse { get; private set; }

        /// <summary>
        /// </summary>
        public bool IsPrepared { get; private set; }
        
        internal SoundEffectInstance GetSoundEffectInstance(int trackIndex, out bool streaming)
        {
            if (_streaming)
            {
                streaming = true;
                var stream = _streams[trackIndex];
                return PlatformCreateStream(stream);
            }
            else
            {
                streaming = false;
                var sound = _sounds[trackIndex];
                return sound.GetPooledInstance(true);
            }
        }

        /// <summary>
        /// This event is triggered when the WaveBank is disposed.
        /// </summary>
        public event EventHandler<EventArgs> Disposing;

        /// <summary>
        /// Is true if the WaveBank has been disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Disposes the WaveBank.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~WaveBank()
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
                foreach (var s in _sounds)
                    s.Dispose();

                IsPrepared = false;
                IsInUse = false;
                EventHelpers.Raise(this, Disposing, EventArgs.Empty);
            }
        }
    }
}

