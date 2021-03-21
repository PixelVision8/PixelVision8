using System;

namespace PixelVision8.Player
{
    public partial class LuaGameChip
    {
        [LuaGameChipAPI]
        public void RegisterMath()
        {
            #region Math APIs

            LuaScript.Globals["CalculateIndex"] = new Func<int, int, int, int>(CalculateIndex);
            LuaScript.Globals["CalculatePosition"] = new Func<int, int, Point>(CalculatePosition);
            LuaScript.Globals["Clamp"] = new Func<int, int, int, int>(Clamp);
            LuaScript.Globals["Repeat"] = new Func<int, int, int>(Repeat);

            #endregion
        }
    }
}