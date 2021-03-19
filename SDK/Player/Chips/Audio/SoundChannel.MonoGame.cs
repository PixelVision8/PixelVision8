using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Audio;

namespace PixelVision8.Player
{
    public partial class SoundChannel
    {
        private SoundEffectInstance _soundInstance;
        protected readonly Dictionary<string, SoundEffectInstance> SoundInstanceCache = new Dictionary<string, SoundEffectInstance>();
        
        public bool IsPlaying()
        {
            
            if (_soundInstance == null) return false;

            return _soundInstance.State == SoundState.Playing;
            
        }
        
        protected void CreateSoundEffect(byte[] bytes, float? frequency = null)
        {
            using (var stream = new MemoryStream(bytes))
            {
                var soundEffect = SoundEffect.FromStream(stream);

                _soundInstance = soundEffect.CreateInstance();

                // TODO need to make sure this is correct and we can use the frequency to manipulate the pitch
                if (frequency.HasValue)
                    _soundInstance.Pitch = frequency.Value;
            }
        }
    }
}