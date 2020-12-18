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

namespace PixelVision8.Engine.Chips
{
    class LuaDebugGameChip : LuaGameChip
    {
        public override void LoadScript(string name)
        {
            // Get a reference to the script loader
            var scriptLoader = Script.DefaultOptions.ScriptLoader;

            // Find the system path to the current script
            var scriptPath = scriptLoader.ResolveModuleName(name, null);

            // Attach the file name to the script when loading it
            LuaScript.DoFile(name, null, scriptPath);
        }

        public override void Shutdown()
        {
            base.Shutdown();

            TriggerShutdown();
        }

        public void TriggerShutdown()
        {
            // TODO this needs to tell the runner when it is ready to actually shutdown
            // Console.WriteLine("Ready to shutdown game");
        }
    }
}