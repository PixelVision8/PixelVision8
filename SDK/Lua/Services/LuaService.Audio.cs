using System;
using System.IO;
using Microsoft.Xna.Framework.Audio;
using MoonSharp.Interpreter;
using PixelVision8.Workspace;

namespace PixelVision8.Runner
{
    public partial class LuaService
    {
        private SoundEffectInstance currentSound;
        
        [RegisterLuaService]
        public void RegisterAudio(Script luaScript)
        {
            luaScript.Globals["Volume"] = new Func<int?, int>(runner.Volume);
            luaScript.Globals["Mute"] = new Func<bool?, bool>(runner.Mute);
            luaScript.Globals["PlayWav"] = new Action<WorkspacePath>(PlayWav);
            luaScript.Globals["StopWav"] = new Action(StopWav);
        }
        
        public void PlayWav(WorkspacePath workspacePath)
        {
            if (workspace.Exists(workspacePath) && workspacePath.GetExtension() == ".wav")
            {
                if (currentSound != null) StopWav();

                using (var stream = workspace.OpenFile(workspacePath, FileAccess.Read))
                {
                    currentSound = SoundEffect.FromStream(stream).CreateInstance();
                }

                currentSound.Play();
            }
        }
        
        public void StopWav()
        {
            if (currentSound != null)
            {
                currentSound.Stop();
                currentSound = null;
            }
        }
    }
}