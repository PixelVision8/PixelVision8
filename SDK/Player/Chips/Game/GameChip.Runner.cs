using System;

namespace PixelVision8.Player
{

    public partial class PixelVision
    {
        public bool BackKeyEnabled => GameChip.BackKeyEnabled;
        
    }

    public partial class GameChip
    {

        public bool BackKeyEnabled = true;

        public void EnableBackKey(bool value)
        {
            BackKeyEnabled = value;
        }

        // TODO add flags to change resolution and other runner specific settings

    }

}