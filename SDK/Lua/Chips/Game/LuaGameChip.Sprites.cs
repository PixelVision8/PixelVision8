using System;
using MoonSharp.Interpreter;

namespace PixelVision8.Player
{
    public partial class LuaGameChip
    {
        [LuaGameChipAPI]
        public void RegisterSprites()
        {
            #region Sprite APIs

            LuaScript.Globals["Sprite"] = new Func<int, int[], int[]>(Sprite);
            LuaScript.Globals["ChangeSizeMode"] = new Action<SpriteSizes>(ChangeSizeMode);
            LuaScript.Globals["SpriteSize"] = new Func<Point>(SpriteSize);
            LuaScript.Globals["TotalSprites"] = new Func<bool, int>(TotalSprites);
            LuaScript.Globals["MaxSpriteCount"] = new Func<int>(MaxSpriteCount);

            #endregion
            
            UserData.RegisterType<SpriteSizes>();
            LuaScript.Globals["SpriteSizes"] = UserData.CreateStatic<SpriteSizes>();
        }
    }
}