using System;
using MoonSharp.Interpreter;
using PixelVision8.Runner.Workspace;

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
        }
    }
}