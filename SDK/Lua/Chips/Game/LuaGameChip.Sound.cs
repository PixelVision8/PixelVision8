using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace PixelVision8.Player
{
    public partial class LuaGameChip
    {
        [LuaGameChipAPI]
        public void RegisterSound()
        {
            #region Sound APIs

            // LuaScript.Globals["Sound"] = new Func<int, string, string>(Sound);
            LuaScript.Globals["PlaySound"] = new Action<int, int>(PlaySound);
            LuaScript.Globals["StopSound"] = new Action<int>(StopSound);
            LuaScript.Globals["PlayRawSound"] =
                new Action<string, int, float>(SoundChip.PlayRawSound);

            LuaScript.Globals["IsChannelPlaying"] = new Func<int, bool>(IsChannelPlaying);
            LuaScript.Globals["PlayPattern"] = new Action<int, bool>(PlayPattern);
            LuaScript.Globals["PlayPatterns"] = new Action<int[], bool>(PlayPatterns);
            LuaScript.Globals["PlaySong"] = new Action<int, bool, int>(PlaySong);
            LuaScript.Globals["PauseSong"] = new Action(PauseSong);
            LuaScript.Globals["RewindSong"] = new Action<int, int>(RewindSong);
            LuaScript.Globals["StopSong"] = new Action(StopSong);
            LuaScript.Globals["SongData"] = new Func<Dictionary<string, int>>(SongData);

            #endregion
            
            // Enums
            UserData.RegisterType<WaveType>();
            LuaScript.Globals["WaveType"] = UserData.CreateStatic<WaveType>();
        }
    }
}