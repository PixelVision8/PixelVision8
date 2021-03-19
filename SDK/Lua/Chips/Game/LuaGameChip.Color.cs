using System;

namespace PixelVision8.Player
{
    public partial class LuaGameChip
    {
        [LuaGameChipAPI]
        public void RegisterColor()
        {
            #region Color APIs

            LuaScript.Globals["BackgroundColor"] = new Func<int?, int>(BackgroundColor);
            LuaScript.Globals["Color"] = new Func<int, string, string>(Color);
            LuaScript.Globals["ColorsPerSprite"] = new Func<int>(ColorsPerSprite);
            LuaScript.Globals["TotalColors"] = new Func<bool, int>(TotalColors);
            LuaScript.Globals["ReplaceColor"] = new Action<int, int>(ReplaceColor);
            LuaScript.Globals["PaletteOffset"] = new Func<int, int, int>(PaletteOffset);
            #endregion
        }
    }
}