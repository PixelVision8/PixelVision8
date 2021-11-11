//   
// Copyright (c) Jesse Freeman, Pixel Vision 8. All rights reserved.  
//  
// Licensed under the Microsoft Public License (MS-PL) except for a few
// portions of the code. See LICENSE file in the project root for full 
// license information. Third-party libraries used by Pixel Vision 8 are 
// under their own licenses. Please refer to those libraries for details 
// on the license they use.
// 
// Contributors
// --------------------------------------------------------
// This is the official list of Pixel Vision 8 contributors:
//  
// Jesse Freeman - @JesseFreeman
// Christina-Antoinette Neofotistou @CastPixel
// Christer Kaitila - @McFunkypants
// Pedro Medeiros - @saint11
// Shawn Rakowski - @shwany
//

using System;
using MoonSharp.Interpreter;

namespace PixelVision8.Player
{
    public partial class LuaGameChip
    {
        [LuaGameChipAPI]
        public void RegisterTimer()
        {

            #region Text APIs
            
            LuaScript.Globals["NewTimer"] = new Action<string, int, int, bool>(NewTimer);
            LuaScript.Globals["ClearAllTimers"] = new Action(ClearAllTimers);
            LuaScript.Globals["ClearTimer"] = new Action<string>(ClearTimer);
            LuaScript.Globals["PauseAllTimers"] = new Action<bool>(PauseAllTimers);
            LuaScript.Globals["TimerValue"] = new Func<string, int>(TimerValue);
            LuaScript.Globals["TimerDelay"] = new Func<string, int>(TimerDelay);
            LuaScript.Globals["TimerTriggered"] = new Func<string, bool>(TimerTriggered);
            LuaScript.Globals["TimerPaused"] = new Func<string, bool, bool>(TimerPaused);

            UserData.RegisterType<Timer>();

            #endregion
        }
    }
}