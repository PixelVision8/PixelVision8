using System;
using MoonSharp.Interpreter;

namespace PixelVision8.Player
{
    public partial class LuaGameChip
    {
        [LuaGameChipAPI]
        public void RegisterText()
        {

            #region Text APIs
            // LuaScript.Globals["New"] = new Func<string, string, int, SpriteCollection>(ConvertTextToSprites);
           
            #endregion
        }
    }
}