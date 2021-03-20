using System;
using MoonSharp.Interpreter;
using PixelVision8.Workspace;

namespace PixelVision8.Runner
{
    public partial class LuaService
    {

        [RegisterLuaService]
        public void RegisterDebug(Script luaScript)
        {
            luaScript.Options.DebugPrint = runner.DisplayWarning;
            
            if (runner.mode == RunnerMode.Loading)
                luaScript.Globals["DebuggerAttached"] = new Func<bool>(runner.AwaitDebuggerAttach);
        }
        
    }
}