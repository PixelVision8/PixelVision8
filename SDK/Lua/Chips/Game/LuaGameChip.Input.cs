using System;
using Microsoft.Xna.Framework.Input;
using MoonSharp.Interpreter;

namespace PixelVision8.Player
{
    public partial class LuaGameChip
    {
        [LuaGameChipAPI]
        public void RegisterInput()
        {
            #region Input APIs

            LuaScript.Globals["Key"] = new Func<Keys, InputState, bool>(Key);
            LuaScript.Globals["Button"] = new Func<Buttons, InputState, int, bool>(Button);
            LuaScript.Globals["MouseButton"] = new Func<int, InputState, bool>(MouseButton);
            LuaScript.Globals["MousePosition"] = new Func<Point>(MousePosition);
            LuaScript.Globals["MouseWheel"] = new Func<Point>(MouseWheel);
            LuaScript.Globals["InputString"] = new Func<string>(InputString);

            #endregion
            
            UserData.RegisterType<Buttons>();
            LuaScript.Globals["Buttons"] = UserData.CreateStatic<Buttons>();

            UserData.RegisterType<Keys>();
            LuaScript.Globals["Keys"] = UserData.CreateStatic<Keys>();

            UserData.RegisterType<InputState>();
            LuaScript.Globals["InputState"] = UserData.CreateStatic<InputState>();
        }
    }
}