using Microsoft.Xna.Framework.Audio;

namespace PixelVision8.Runner
{
    public partial class GameRunner
    {
        private static bool _mute;
        private static int _lastVolume;
        private static int _muteVolume;

        public virtual int Volume(int? value = null)
        {
            if (value.HasValue)
                _lastVolume = value.Value;

            SoundEffect.MasterVolume = _lastVolume / 100f;

            if (_mute == true && _lastVolume > 0)
            {
                _muteVolume = _lastVolume;
            }

            return _lastVolume;
        }

        public virtual bool Mute(bool? value = null)
        {
            if (value.HasValue)
            {
                _mute = value.Value;

                if (_mute)
                {
                    _muteVolume = _lastVolume;

                    Volume(0);
                }
                else
                {
                    // Restore volume to halfway if un-muting and last  value was 0
                    if (_muteVolume < 5)
                    {
                        _muteVolume = 50;
                    }

                    Volume(_muteVolume);
                }
            }

            return SoundEffect.MasterVolume == 0 || _mute;
        }
    }
}