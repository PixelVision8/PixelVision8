using PixelVision8.Runner.Data;

namespace PixelVision8.Engine.Chips
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