using System;
using MoonSharp.Interpreter;
using PixelVision8.Runner.Workspace;

namespace PixelVision8.Runner
{
    public partial class LuaService
    {

        [RegisterLuaService]
        public void RegisterThreading(Script luaScript)
        {
            luaScript.Globals["RunBackgroundScript"] = new Func<string, string[], bool>(RunBackgroundScript);
            luaScript.Globals["BackgroundScriptData"] = new Func<string, string, string>(BackgroundScriptData);
            luaScript.Globals["CancelExport"] = new Action(CancelExport);
        }
    }
}