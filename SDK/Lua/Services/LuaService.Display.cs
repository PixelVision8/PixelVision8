using System;
using MoonSharp.Interpreter;

namespace PixelVision8.Runner
{
    public partial class LuaService
    {

        [RegisterLuaService]
        public void RegisterDisplay(Script luaScript)
        {
            luaScript.Globals["Scale"] = new Func<int?, int>(runner.Scale);
            luaScript.Globals["Fullscreen"] = new Func<bool?, bool>(runner.Fullscreen);
            luaScript.Globals["CropScreen"] = new Func<bool?, bool>(runner.CropScreen);
            luaScript.Globals["EnableCRT"] = new Func<bool?, bool>(runner.EnableCRT);
            luaScript.Globals["Brightness"] = new Func<float?, float>(runner.Brightness);
            luaScript.Globals["Sharpness"] = new Func<float?, float>(runner.Sharpness);
            
            // TODO remove this when tools are fixed to run at 256 colors
            luaScript.Globals["ResizeColorMemory"] = new Action<int, int>(ResizeColorMemory);
        }
        
        public void ResizeColorMemory(int newSize, int maxColors = -1)
        {
            // runner.ActiveEngine.ColorChip.maxColors = maxColors;
            runner.ActiveEngine.ColorChip.Total = newSize;
        }
    }
}