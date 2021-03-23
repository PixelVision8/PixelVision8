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
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;
using PixelVision8.Player;

namespace PixelVision8.Runner
{
    public class LuaRunner : GameRunner
    {
        private KeyboardInputChip _controllerChip;
       
        public LuaRunner(string rootPath) : base(rootPath)
        {
        }

        /// <summary>
        ///     This is called when the runner first starts up.
        /// </summary>
        protected override void Initialize()
        {
            // Configure the runner
            ConfigureDisplayTarget();
            
            // Create a script loader that looks for files in the root path
            Script.DefaultOptions.ScriptLoader = new ScriptLoader(RootPath);
            Script.DefaultOptions.DebugPrint = s => Console.WriteLine(s);
            
            // Manually override scale on boot up
            Scale(1);

            // Load the game
            LoadDefaultGame();
        }

        /// <summary>
        ///     This mthod manually loads the game file's binary data then configures the engine and processes the files.
        /// </summary>
        private void LoadDefaultGame()
        {
            // Create a list of valid files we want to load from the game directory
            var fileExtensions = new[]
            {
                "lua",
                "png",
                "json"
            };

            // Create a new dictionary to store the file binary data
            var gameFiles = new List<string>();

            // Get only the files we need from the directory base on their extension above.
            var files = from p in Directory.EnumerateFiles(RootPath)
                where fileExtensions.Any(val => p.EndsWith(val))
                select p;

            // Loop through each file in the list
            foreach (string file in files)
            {
                // Read the binary data and save it along with the file name to the dictionary.
                gameFiles.Add(file);
            }
            
            var chips = new List<string>
            {
                typeof(ColorChip).FullName,
                typeof(SpriteChip).FullName,
                typeof(TilemapChip).FullName,
                typeof(MusicChip).FullName,
                typeof(FontChip).FullName,
                typeof(ControllerChip).FullName,
                typeof(KeyboardInputChip).FullName,
                typeof(DisplayChip).FullName,
                typeof(SoundChip).FullName,
                typeof(LuaGameChip).FullName,
            };

            // Configure a new PV8 engine to play the game
            TmpEngine = new PixelVision(chips.ToArray());
            
            var fileHelper = new FileLoadHelper();
            var imageParser = new PNGParser(Graphics.GraphicsDevice);

            var loader = new Loader(fileHelper, imageParser);

            // Process the files
            loader.ParseFiles(gameFiles.ToArray(), TmpEngine);
            loader.LoadAll();

            if (TmpEngine?.GameChip is LuaGameChip)
            {
                // Find the default script to start
                var tempQualifier = (LuaGameChip) TmpEngine.GameChip;
                tempQualifier.LoadScript(tempQualifier.DefaultScriptPath);
            }
            
            RunGame();

            _controllerChip = ActiveEngine.KeyboardInputChip;
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            
            if (_controllerChip.GetKeyDown(Keys.LeftControl) ||
                _controllerChip.GetKeyDown(Keys.LeftControl))
                if (_controllerChip.GetKeyUp(Keys.R) || _controllerChip.GetKeyUp(Keys.D4))
                    ResetGame();
        }

        private void ResetGame()
        {
            LoadDefaultGame();
        }
    }
    
    public class ScriptLoader : IScriptLoader
    {

        private string RootPath;
        
        public ScriptLoader(string rootPath)
        {
            RootPath = rootPath;
        }

        public bool ScriptFileExists(string name)
        {
            throw new System.NotImplementedException();
        }

        public object LoadFile(string file, Table globalContext)
        {
            Console.WriteLine("Load Script " + file + " from " + RootPath);
            
            string script = "";

            var path = Path.Combine(RootPath, file);
            
            if (File.Exists(path))
            {
                script = File.ReadAllText(path);

                if (!string.IsNullOrEmpty(script))
                {
                    // Replace math operators
                    var pattern = @"(\S+)\s*([+\-*/%])\s*=";
                    var replacement = "$1 = $1 $2 ";
                    script = Regex.Replace(script, pattern, replacement, RegexOptions.Multiline);

                    // Replace != conditions
                    pattern = @"!\s*=";
                    replacement = "~=";
                    script = Regex.Replace(script, pattern, replacement, RegexOptions.Multiline);

                    return script;
                }
            }
            
            Console.WriteLine($"Could not load '"+file+"' file because it is either missing or empty.");

            return script;

        }

        public string ResolveFileName(string filename, Table globalContext)
        {
            // Add extension
            if (!filename.EndsWith(".lua"))
            {
                filename = filename + ".lua";
            }

            if (filename.StartsWith("/"))
            {
                // TODO need to make sure this is an relative path
            }

            return filename;
        }

        public string ResolveModuleName(string modname, Table globalContext)
        {
            
            if (!modname.EndsWith(".lua"))
            {
                modname = modname + ".lua";
            }
            
            if (modname.StartsWith("/"))
            {
                // TODO need to make sure this is an relative path
            }
            
            var filePath = Path.Combine(RootPath, modname);
            if (File.Exists(filePath))
            {
                modname = filePath;
            }
            
            return modname;
        }
    }
}