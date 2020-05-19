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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MoonSharp.Interpreter;
using PixelVision8.Engine.Utils;
using PixelVision8.Runner.Exporters;
using PixelVision8.Runner.Services;

namespace PixelVision8
{
    class BackgroundScriptRunner : AbstractExporter
    {
        protected Script _luaScript;
        protected List<string> stepQueue = new List<string>();

        public Script LuaScript
        {
            get
            {
                if (_luaScript == null) _luaScript = new Script(CoreModules.Preset_SoftSandbox);

                return _luaScript;
            }
        }

        public BackgroundScriptRunner(string scriptName, LuaServicePlus luaService, string[] args = null) : base(null)
        {

            LuaScript.DoFile(scriptName);

            // Add all of the core File System APIs to the scrip
            luaService.ConfigureScript(LuaScript);

            LuaScript.Globals["args"] = args.Clone();
            LuaScript.Globals["AddStep"] = new Action<string>(AddStep);
            LuaScript.Globals["SetStringAsData"] = new Action<string>(SetStringAsData);
            LuaScript.Globals["SetImageAsData"] = new Action<Image, string>(SetImageAsData);
            LuaScript.Globals["BackgroundScriptData"] = new Func<string, string, string>(luaService.BackgroundScriptData);

        }

        public override void CalculateSteps()
        {
            currentStep = 0;

            if (LuaScript?.Globals["CalculateSteps"] == null) return;
            
            try
            {
                LuaScript.Call(LuaScript.Globals["CalculateSteps"]);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Exit();
            }
            
        }

        public override void LoadSourceData()
        {
            if (LuaScript?.Globals["LoadSourceData"] == null) return;

            try
            {
                LuaScript.Call(LuaScript.Globals["LoadSourceData"]);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Exit();
            }
        }

        public void AddStep(string functionName)
        {
            stepQueue.Add(functionName);

            steps.Add(CallStep);
        }

        protected void CallStep()
        {

            try
            {
                // Console.WriteLine("calling " + stepQueue[currentStep]);
                LuaScript.Call(LuaScript.Globals[stepQueue[currentStep]]);
                StepCompleted();
            }
            catch (Exception e)
            {
                 Console.WriteLine(e);
                Exit();
            }
            
        }

        public void SetStringAsData(string value)
        {

            // TODO need to make sure nothing is saved if bytes is empty
            try
            {
                bytes = Encoding.UTF8.GetBytes(value);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                
            }
            
        }

        public void SetImageAsData(Image image, string maskColor = "#FF00FF")
        {

            try
            {
                var palette = image.colors.Select(ColorUtils.HexToColor).ToArray();

                var imageExporter = new PNGWriter();

                var exporter = new PixelDataExporter(fileName, image.pixels, image.width, image.height, palette, imageExporter,
                    maskColor);

                exporter.CalculateSteps();

                while (exporter.completed == false)
                {
                    exporter.NextStep();
                }

                bytes = exporter.bytes;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                
            }

        }

        public void Exit()
        {
            currentStep = totalSteps;
        }
    }
}
