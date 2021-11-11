using System;
using MoonSharp.Interpreter;

namespace PixelVision8.Player
{
    public partial class LuaGameChip
    {
        [LuaGameChipAPI]
        public void RegisterData()
        {
            // Register PV8's vector type
            UserData.RegisterType<Point>();
            LuaScript.Globals["NewPoint"] = new Func<int, int, Point>(NewPoint);

            // Register PV8's rect type
            UserData.RegisterType<Rectangle>();
            LuaScript.Globals["NewRect"] = new Func<int, int, int, int, Rectangle>(NewRect);
        }
    }
}