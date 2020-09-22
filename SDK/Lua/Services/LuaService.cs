//   
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
using PixelVision8.Engine.Services;

namespace PixelVision8.Runner.Services
{
    public class LuaService : AbstractService
    {
        protected IRunner runner;

        /// <summary>
        ///     The LuaService exposes core Runner APIs to the Lua Game Chip
        /// </summary>
        /// <param name="runner"></param>
        public LuaService(IRunner runner)
        {
            this.runner = runner;
        }

        /// <summary>
        ///     This service exposes some of the runner's APIs to Lua Games.
        /// </summary>
        /// <param name="luaScript"></param>
        public virtual void ConfigureScript(Script luaScript)
        {
            luaScript.Options.DebugPrint = runner.DisplayWarning;

            luaScript.Globals["Volume"] = new Func<int?, int>(runner.Volume);
            luaScript.Globals["Mute"] = new Func<bool?, bool>(runner.Mute);

            luaScript.Globals["Scale"] = new Func<int?, int>(runner.Scale);
            luaScript.Globals["Fullscreen"] = new Func<bool?, bool>(runner.Fullscreen);
            luaScript.Globals["CropScreen"] = new Func<bool?, bool>(runner.CropScreen);
            luaScript.Globals["StretchScreen"] = new Func<bool?, bool>(runner.StretchScreen);

            // luaScript.Globals["ResetGame"] = new Action(runner.ResetGame);
        }
    }
}