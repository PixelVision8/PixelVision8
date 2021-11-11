using System;
using MoonSharp.Interpreter;

namespace PixelVision8.Runner
{
    public partial class LuaService
    {

        [RegisterLuaService]
        public void RegisterBoot(Script luaScript)
        {
            if (runner.mode == RunnerMode.Booting)
                luaScript.Globals["BootDone"] = new Action<bool>(runner.BootDone);
        }
    }
}