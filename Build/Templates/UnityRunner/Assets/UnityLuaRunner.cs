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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;
using PixelVision8.Player;
using UnityEngine;
using System.IO;
using System.IO.Compression;
using System.Text;

#if UNITY_WEBGL
using System.Runtime.InteropServices;
#endif

namespace PixelVision8.Runner
{
    public class UnityLuaRunner : GameRunner
    {
        
        private string[] _files;
        private IFileLoader _fileLoader;
        
        // Create a list of valid files we want to load from the game directory
        private string[] _fileExtensions = new[]
        {
            "lua",
            "png",
            "json"
        };

        private string[] _defaultFonts = new string[]
        {
            "small.font.png",
            "medium.font.png",
            "large.font.png"
        };

        #if UNITY_WEBGL
        [DllImport("__Internal")]
        private static extern string GetURL();
        #endif
        
        public override void Start()
        {
            base.Start();

            // Configure the display
            ConfigureDisplayTarget();
            
            // Route all print statements to Unity's Debug.Log
            Script.DefaultOptions.DebugPrint = s => Debug.Log(s);

            // Manually override scale on boot up
            Scale(1);

            #if UNITY_WEBGL && !UNITY_EDITOR
                RootPath = GetURL();
            #endif

            if(RootPath.EndsWith(".pv8"))
            {
                LoadFromZip(RootPath);
            }
            else
            {
                LoadFromDirectory(RootPath);
            }

        }

        public void LoadFromDirectory(string path)
        {

            // Get only the files we need from the directory base on their extension above.
            _files = (from p in Directory.EnumerateFiles(RootPath)
                where _fileExtensions.Any(val => p.EndsWith(val))
                select p).ToArray();

            _fileLoader = new FileLoadHelper();

            // Create a script loader that looks for files via the File Loader instance
            Script.DefaultOptions.ScriptLoader = new ScriptLoader(_fileLoader, RootPath);

            // Load the game
            LoadDefaultGame();
        }

        public void LoadFromZip(string path)
        {
            var www = new WWW(path);
            StartCoroutine(WaitForRequest(www));
        }

        protected bool displayProgress = true;

        private IEnumerator WaitForRequest(WWW www)
        {
            yield return www;

            // check for errors
            if (string.IsNullOrEmpty(www.error))
            {
                var mStream = new MemoryStream(www.bytes);

                var zipHelper = new ZipFileHelper(mStream);

                _files = (from p in zipHelper.Files
                where _fileExtensions.Any(val => p.EndsWith(val))
                select p).ToArray();

                _fileLoader = zipHelper;

                // Create a script loader that looks for files via the File Loader instance
                Script.DefaultOptions.ScriptLoader = new ScriptLoader(_fileLoader);

                // Load the game
                LoadDefaultGame();
            }
            else
            {
                Debug.Log("WWW Error: " + www.error +" "+www.url);
            }
        }

        /// <summary>
        ///     This mthod manually loads the game file's binary data then configures the engine and processes the files.
        /// </summary>
        private void LoadDefaultGame()
        {

            // Create a new dictionary to store the file binary data
            var gameFiles = new List<string>();

            // Loop through each file in the list
            foreach (string file in _files)
            {
                // Read the binary data and save it along with the file name to the dictionary.
                gameFiles.Add(file);
            }
            
            var chips = new List<string>
            {
                typeof(ColorChip).FullName,
                typeof(SpriteChip).FullName,
                typeof(TilemapChip).FullName,
                // typeof(MusicChip).FullName,
                typeof(FontChip).FullName,
                // typeof(ControllerChip).FullName,
                // typeof(KeyboardInputChip).FullName,
                typeof(DisplayChip).FullName,
                // typeof(SoundChip).FullName,
                typeof(LuaGameChip).FullName,
            };

            // Configure a new PV8 engine to play the game
            TmpEngine = new PixelVision(chips.ToArray());
            
            var imageParser = new PNGFileReader(_fileLoader);

            var loader = new Loader(_fileLoader, imageParser);

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

            // _controllerChip = ActiveEngine.KeyboardInputChip;
        }

        public override void Update()
        {
            base.Update();
            
            // if (_controllerChip.GetKeyDown(Keys.LeftControl) ||
            //     _controllerChip.GetKeyDown(Keys.LeftControl))
            //     if (_controllerChip.GetKeyUp(Keys.R) || _controllerChip.GetKeyUp(Keys.D4))
            //         ResetGame();
        }

        private void ResetGame()
        {
            LoadDefaultGame();
        }
    }
    
    public class ScriptLoader : IScriptLoader
    {

        private string _rootPath;
        private IFileLoader _fileLoader;
        
        public ScriptLoader(IFileLoader fileLoader, string rootPath = "")
        {
            _fileLoader = fileLoader;
            _rootPath = rootPath;
        }

        public bool ScriptFileExists(string name)
        {
            return _fileLoader.Exists(name);
        }

        public object LoadFile(string file, Table globalContext)
        {
            
            string script = "";

            var path = Path.Combine(_rootPath, file);
            
            if (_fileLoader.Exists(path))
            {
                script = Encoding.UTF8.GetString(_fileLoader.ReadAllBytes(path));

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
            
            var filePath = Path.Combine(_rootPath, modname);
            if (File.Exists(filePath))
            {
                modname = filePath;
            }
            
            return modname;
        }
    }
}