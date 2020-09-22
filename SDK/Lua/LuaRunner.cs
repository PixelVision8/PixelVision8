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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PixelVision8.Engine;
using PixelVision8.Engine.Chips;
using PixelVision8.Runner.Services;

namespace PixelVision8.Runner
{
    public class LuaRunner : GameRunner
    {
        // Store the path to the game's files
        private readonly string gamePath;
        private IControllerChip controllerChip;

        /// <summary>
        ///     This constructor saves the path to the game's files and kicks off the base constructor
        /// </summary>
        /// <param name="gamePath"></param>
        public LuaRunner(string gamePath)
        {
            this.gamePath = gamePath;
        }

        public LuaRunner()
        {
            //throw new System.NotImplementedException();
        }

        /// <summary>
        ///     This is called when the runner first starts up.
        /// </summary>
        protected override void Initialize()
        {
            // Configure the runner
            ConfigureRunner();

            // Load the game
            LoadDefaultGame();
        }

        public override void ConfigureServices()
        {
            var luaService = new LuaService(this);

            // Register Lua Service
            tmpEngine.AddService(typeof(LuaService).FullName, luaService);
        }

        /// <summary>
        ///     This mthod manually loads the game file's binary data then configures the engine and processes the files.
        /// </summary>
        private void LoadDefaultGame()
        {
            // Create a list of valid files we want to load from the game directory
            string[] fileExtensions =
            {
                "lua",
                "png",
                "json"
            };

            // Create a new list to store the file paths
            var gameFiles = new List<string>();

            // Get only the files we need from the directory base on their extension above.
            var files = from p in Directory.EnumerateFiles(gamePath)
                where fileExtensions.Any(val => p.EndsWith(val))
                select p;

            // Loop through each file in the list
            foreach (var file in files)
                // Read the binary data and save it along with the file name to the dictionary.
                gameFiles.Add(file);

            // Configure a new PV8 engine to play the game
            ConfigureEngine();

            // Manually activate the custom game chip
            tmpEngine.ActivateChip("GameChip", new LuaGameChip { DefaultScriptPath = "Content/code.lua" });

            // Process the files
            ProcessFiles(tmpEngine, gameFiles.ToArray());

            controllerChip = ActiveEngine.ControllerChip;
        }

        public override void ActivateEngine(IEngine engine)
        {
            LuaGameChip gameChip = ((LuaGameChip)engine.GameChip);

            gameChip.LoadScript(gameChip.DefaultScriptPath);

            base.ActivateEngine(engine);
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (controllerChip.GetKeyDown(Keys.LeftControl) ||
                controllerChip.GetKeyDown(Keys.LeftControl))
                if (controllerChip.GetKeyUp(Keys.R) || controllerChip.GetKeyUp(Keys.D4))
                    ResetGame();

        }

        public void ResetGame()
        {
            LoadDefaultGame();
        }
    }
}