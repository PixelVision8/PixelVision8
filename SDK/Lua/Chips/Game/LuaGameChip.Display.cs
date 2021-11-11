using System;
using MoonSharp.Interpreter;

namespace PixelVision8.Player
{
    public partial class LuaGameChip
    {
        [LuaGameChipAPI]
        public void RegisterDisplay()
        {
            #region Display APIs

            LuaScript.Globals["Clear"] = new Action(Clear);
            LuaScript.Globals["Display"] = new Func<Point>(Display);
            LuaScript.Globals["DrawPixels"] = new Action<int[], int, int, int, int, bool, bool, DrawMode, int, int>(DrawPixels);
            LuaScript.Globals["DrawSprite"] = new Action<int, int, int, bool, bool, DrawMode, int>(DrawSprite);
            LuaScript.Globals["DrawText"] = new Action<string, int, int, DrawMode, string, int, int, bool, bool>(DrawText);
            LuaScript.Globals["DrawColoredText"] = new Action<string, int, int, DrawMode, string, int[], int, bool, bool>(DrawColoredText);
            LuaScript.Globals["DrawTilemap"] = new Action<int, int, int, int, int?, int?>(DrawTilemap);
            LuaScript.Globals["DrawRect"] = new Action<int, int, int, int, int, DrawMode>(DrawRect);
            LuaScript.Globals["RedrawDisplay"] = new Action(RedrawDisplay);
            LuaScript.Globals["ScrollPosition"] = new Func<int?, int?, Point>(ScrollPosition);
            
            #endregion
            
            UserData.RegisterType<DrawMode>();
            LuaScript.Globals["DrawMode"] = UserData.CreateStatic<DrawMode>();

            
        }

    }
}