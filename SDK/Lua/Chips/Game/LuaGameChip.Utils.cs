using System;

namespace PixelVision8.Player
{
    public partial class LuaGameChip
    {
        [LuaGameChipAPI]
        public void RegisterUtils()
        {
            #region Utils

            LuaScript.Globals["WordWrap"] = new Func<string, int, string>(WordWrap);
            LuaScript.Globals["SplitLines"] = new Func<string, string[]>(SplitLines);
            LuaScript.Globals["BitArray"] = new Func<int, int[]>(BitArray);
            LuaScript.Globals["CalculateDistance"] = new Func<int, int, int, int, int>(CalculateDistance);

            #endregion
        }
    }
}