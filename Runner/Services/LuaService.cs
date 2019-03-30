//   
// Copyright (c) Jesse Freeman. All rights reserved.  
//  
// Licensed under the Microsoft Public License (MS-PL) License. 
// See LICENSE file in the project root for full license information. 
// 
// Contributors
// --------------------------------------------------------
// This is the official list of Pixel Vision 8 contributors:
//  
// Jesse Freeman - @JesseFreeman
// Christer Kaitila - @McFunkypants
// Pedro Medeiros - @saint11
// Shawn Rakowski - @shwany

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
            
            luaScript.Globals["Volume"] = (VolumeDelegator) runner.Volume;
            luaScript.Globals["Mute"] = (MuteDelegator) runner.Mute;
            
            luaScript.Globals["Scale"] = (ScaleDelegator) runner.Scale;
            luaScript.Globals["Fullscreen"] = (FullscreenDelegator)runner.Fullscreen;
            luaScript.Globals["CropScreen"] = (CropScreenDelegator)runner.CropScreen;
            luaScript.Globals["StretchScreen"] = (StretchScreenDelegator)runner.StretchScreen;
            
            luaScript.Globals["DebugLayers"] = new Action<bool>(runner.DebugLayers);
            luaScript.Globals["DebugLayers"] = new Action<int>(runner.ToggleLayers);
            
            luaScript.Globals["ResetGame"] = new Action(runner.ResetGame);
            
        }

        private delegate int VolumeDelegator(int? volume);
        private delegate bool MuteDelegator(bool? value);
        private delegate int ScaleDelegator(int? value);
        private delegate bool FullscreenDelegator(bool? newValue);
        private delegate bool CropScreenDelegator(bool? newValue);
        private delegate bool StretchScreenDelegator(bool? newValue);

    }
}