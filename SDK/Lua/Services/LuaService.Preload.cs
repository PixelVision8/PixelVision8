using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using PixelVision8.Player;
using PixelVision8.Runner.Workspace;

namespace PixelVision8.Runner
{
    public partial class LuaService
    {

        [RegisterLuaService]
        public void RegisterPreload(Script luaScript)
        {
            luaScript.Globals["StartNextPreload"] = new Action(runner.StartNextPreload);
            luaScript.Globals["PreloaderComplete"] = new Action(runner.RunGame);
            luaScript.Globals["ReadPreloaderPercent"] =
                new Func<int>(() => Utilities.Clamp((int)(runner.loadService.Percent * 100), 0, 100));
        }
    }
}