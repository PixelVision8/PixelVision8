using System;
using MoonSharp.Interpreter;

namespace PixelVision8.Player
{
    public partial class LuaGameChip
    {
        [LuaGameChipAPI]
        public void RegisterCanvas()
        {
            // Register PV8's rect type
            UserData.RegisterType<Canvas>();
            LuaScript.Globals["NewCanvas"] =
                new Func<int, int, Canvas>(NewCanvas);
        }
    }
}