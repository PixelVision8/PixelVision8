using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using PixelVision8.Runner;

namespace PixelVision8.Player
{
    public partial class LuaGameChip
    {
        [LuaGameChipAPI]
        public void RegisterFileIo()
        {
            #region File IO APIs

            LuaScript.Globals["AddScript"] = new Action<string, string>(AddScript);
            LuaScript.Globals["LoadScript"] = new Action<string>(LoadScript);
            LuaScript.Globals["ReadSaveData"] = new Func<string, string, string>(ReadSaveData);
            LuaScript.Globals["WriteSaveData"] = new Action<string, string>(WriteSaveData);
            LuaScript.Globals["ReadMetadata"] = new Func<string, string, string>(ReadMetadata);
            LuaScript.Globals["WriteMetadata"] = new Action<string, string>(WriteMetadata);
            LuaScript.Globals["ReadAllMetadata"] = new Func<Dictionary<string, string>>(ReadAllMetadata);

            #endregion
            
            UserData.RegisterType<FileFlags>();
            LuaScript.Globals["SaveFlags"] = UserData.CreateStatic<FileFlags>();
        }
    }
}