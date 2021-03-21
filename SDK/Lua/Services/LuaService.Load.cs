using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace PixelVision8.Runner
{
    public partial class LuaService
    {

        [RegisterLuaService]
        public void RegisterLoad(Script luaScript)
        {
            // if (runner.mode != RunnerMode.Booting)
            // {
                luaScript.Globals["LoadGame"] =
                    new Func<string, Dictionary<string, string>, bool>((path, metadata) =>
                        runner.Load(path, RunnerMode.Loading, metadata));
            // }
            
            if (runner.mode == RunnerMode.Loading)
            {
                luaScript.Globals["StartUnload"] = new Action(runner.StartUnload);
                luaScript.Globals["UnloadProgress"] = new Func<int>(runner.UnloadProgress);
                luaScript.Globals["EndUnload"] = new Action(runner.EndUnload);
            }
        }
    }
}