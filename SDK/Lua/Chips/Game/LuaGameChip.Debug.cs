using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace PixelVision8.Player
{
    public partial class LuaGameChip
    {
        [LuaGameChipAPI]
        public void RegisterDebug()
        {
            #region Debug

            LuaScript.Globals["ReadFPS"] = new Func<int>(ReadFPS);
            LuaScript.Globals["ReadTotalSprites"] = new Func<int>(ReadTotalSprites);

            #endregion
        }
    }
}