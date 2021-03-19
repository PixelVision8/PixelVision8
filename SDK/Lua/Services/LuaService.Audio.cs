using System;
using MoonSharp.Interpreter;
using PixelVision8.Runner.Workspace;

namespace PixelVision8.Runner
{
    public partial class LuaService
    {

        [RegisterLuaService]
        public void RegisterAudio(Script luaScript)
        {
            luaScript.Globals["Volume"] = new Func<int?, int>(runner.Volume);
            luaScript.Globals["Mute"] = new Func<bool?, bool>(runner.Mute);
            luaScript.Globals["PlayWav"] = new Action<WorkspacePath>(PlayWav);
            luaScript.Globals["StopWav"] = new Action(StopWav);
        }
    }
}