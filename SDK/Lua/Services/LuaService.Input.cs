using System;
using MoonSharp.Interpreter;

namespace PixelVision8.Runner
{
    public partial class LuaService
    {

        [RegisterLuaService]
        public void RegisterInput(Script luaScript)
        {
            luaScript.Globals["ControllerConnected"] = new Func<int, bool>(runner.IsControllerConnected);
            luaScript.Globals["EnableAutoRun"] = new Action<bool>(runner.EnableAutoRun);
            luaScript.Globals["EnableBackKey"] = new Action<bool>(runner.EnableBackKey);

            luaScript.Globals["RefreshActionKeys"] = new Action(runner.RefreshActionKeys);
            
        }
    }
}