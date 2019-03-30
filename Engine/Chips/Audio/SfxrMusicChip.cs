using GameCreator;
using PixelVisionSDK;
using PixelVisionSDK.Chips;

namespace PixelVisionRunner.Chips
{

    public class SfxrMusicChip : MusicChip
    {

        public bool ignore { get; private set; }
        
        public override TrackerData CreateNewTrackerData(string name, int tracks = 4)
        {
            return new SfxrTrackerData(name);
        }

    }
}