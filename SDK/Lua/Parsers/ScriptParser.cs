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

using PixelVision8.Player;

namespace PixelVision8.Runner
{
    public class ScriptParser : AbstractParser
    {
        private readonly string name;
        private readonly string script;
        private readonly LuaGameChip target;

        public ScriptParser(string name, string script, GameChip target)
        {
            this.name = name;
            this.script = script;
            this.target = target as LuaGameChip;
        }

        public override void CalculateSteps()
        {
            base.CalculateSteps();

            Steps.Add(LoadScript);
        }

        protected void LoadScript()
        {
            target?.AddScript(name, script);
            CurrentStep++;
        }
    }
}