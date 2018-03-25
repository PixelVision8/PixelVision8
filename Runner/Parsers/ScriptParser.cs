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

using PixelVisionSDK.Chips;

namespace PixelVisionRunner.Parsers
{

    public class ScriptParser : AbstractParser
    {

        private readonly string name;
        private readonly string script;
        private readonly GameChip target;

        public ScriptParser(string name, string script, GameChip target)
        {
            this.name = name;
            this.script = script;
            this.target = target;
        }

        public override void CalculateSteps()
        {
            base.CalculateSteps();

            steps.Add(LoadScript);
        }

        protected void LoadScript()
        {
            target.AddTextFile(name, script);
            currentStep++;
        }

    }

}