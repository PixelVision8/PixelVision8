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

using MoonSharp.Interpreter;
using System;
using System.Linq;
using PixelVision8.Editor;

namespace PixelVision8.Runner
{
    public class RegisterLuaService : Attribute
    {
    }

    public partial class LuaService : AbstractService
    {
        protected DesktopRunner runner;
        
        private readonly WorkspaceServicePlus workspace;

        
        

        /// <summary>
        ///     The LuaService exposes core Runner APIs to the Lua Game Chip
        /// </summary>
        /// <param name="runner"></param>
        public LuaService(DesktopRunner runner)
        {
            this.runner = runner;

            workspace = runner.workspaceService as WorkspaceServicePlus;
        }

        

        /// <summary>
        ///     This service exposes some of the runner's APIs to Lua Games.
        /// </summary>
        /// <param name="luaScript"></param>
        public virtual void ConfigureScript(Script luaScript)
        {
            var methods = GetType().GetMethods().Where(m => m.GetCustomAttributes(typeof(RegisterLuaService), false).Length > 0)
                .ToArray();

            for (int i = 0; i < methods.Length; i++)
            {
                // Call API register functions to add them to the service
                GetType().GetMethod(methods[i].Name)?.Invoke(this, new object[] {luaScript});
                
            }

        }


    }
}