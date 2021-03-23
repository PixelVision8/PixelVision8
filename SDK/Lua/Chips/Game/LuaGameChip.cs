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
using PixelVision8.Runner;
using System;
using System.Linq;

namespace PixelVision8.Player
{
    
    public class LuaGameChipAPI : Attribute
    {
         
    }
    
    public partial class LuaGameChip : GameChip
    {
        private Script _luaScript;
        public readonly string DefaultScriptPath = "code";

        public Script LuaScript
        {
            get
            {
                if (_luaScript == null) _luaScript = new Script(CoreModules.Preset_SoftSandbox);

                return _luaScript;
            }
        }

        

        #region Lifecycle

        public override void Init()
        {
            
            if (LuaScript?.Globals["Init"] == null) return;

            LuaScript.Call(LuaScript.Globals["Init"]);
        }

        public override void Update(int timeDelta)
        {
            base.Update(timeDelta);

            if (LuaScript?.Globals["Update"] == null) return;

            LuaScript.Call(LuaScript.Globals["Update"], timeDelta);
        }

        public override void Draw()
        {
            if (LuaScript?.Globals["Draw"] == null) return;

            LuaScript.Call(LuaScript.Globals["Draw"]);
        }

        public override void Shutdown()
        {
            if (LuaScript?.Globals["Shutdown"] == null) return;

            LuaScript.Call(LuaScript.Globals["Shutdown"]);
        }

        public override void Reset()
        {
            // Setup the GameChip
            base.Reset();

            if (LuaScript.Globals["Reset"] != null) LuaScript.Call(LuaScript.Globals["Reset"]);
        }

        #endregion

        #region Scripts

        // TODO need to update the docs on both of these APIs. Load is for a file and Add is for a string. Both automatically parse the script.

        /// <summary>
        ///     This allows you to load a script into memory. External scripts can be located in the System/Libs/,
        ///     Workspace/Libs/ or Workspace/Sandbox/ directory. All scripts, including built-in ones from the Game
        ///     Creator, are accessible via their file name (with or without the extension). You can keep additional
        ///     scripts in your game folder and load them up. Call this method before Init() in your game's Lua file to
        ///     have access to any external code loaded by the Game Creator or Runner.
        /// </summary>
        /// <param name="name">
        ///     Name of the Lua file. You can drop the .lua extension since only Lua files will be accessible to this
        ///     method.
        /// </param>
        public virtual void LoadScript(string name)
        {
            LuaScript.DoFile(name);
        }

        /// <summary>
        ///     This allows you to add your Lua scripts at runtime to a game from a string. This could be useful for
        ///     dynamically generating code such as level data or other custom Lua objects in memory. Simply give the
        ///     script a name and pass in a string with valid Lua code. If a script with the same name exists, this will
        ///     override it. Make sure to call LoadScript() after to parse it.
        /// </summary>
        /// <param name="name">Name of the script. This should contain the .lua extension.</param>
        /// <param name="script">The string text representing the Lua script data.</param>
        public void AddScript(string name, string script)
        {
            LuaScript.DoString(script, null, name);
        }

        #endregion
        
        
    }
    
    
    
    
}