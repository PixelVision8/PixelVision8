using System;

namespace PixelVision8.Player
{
    public partial class LuaGameChip
    {
        [LuaGameChipAPI]
        public void RegisterRunner()
        {
            #region Runner

            LuaScript.Globals["EnableBackKey"] = new Action<bool>(EnableBackKey);
    
            #endregion
        }
    }
}