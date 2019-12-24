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
using PixelVision8.Runner;
using PixelVisionRunner.Games;

namespace Desktop
{
    public class CSharpRunner : GameRunner
    {

        // Store the path to the game's files
        private readonly string gamePath;
        
        /// <summary>
        ///     This constructor saves the path to the game's files and kicks off the base constructor
        /// </summary>
        /// <param name="gamePath"></param>
        public CSharpRunner(string gamePath) : base()
        {
            this.gamePath = gamePath;
        }
        
        /// <summary>
        ///     This is called when the runner first starts up.
        /// </summary>
        protected override void Initialize()
        {

            // Configure the runner
            ConfigureRunner();
            
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
                "png",
                "json"
            };
            
            // Create a new dictionary to store the file binary data
            var gameFiles = new Dictionary<string, byte[]>();
            
            // Get only the files we need from the directory base on their extension above.
            var files = from p in Directory.EnumerateFiles(gamePath)
                where fileExtensions.Any(val => p.EndsWith(val))
                select p;
            
            // Loop through each file in the list
            foreach (string file in files)
            {
                // Read the binary data and save it along with the file name to the dictionary.
                gameFiles.Add(Path.GetFileName(file), File.ReadAllBytes(file));
            }

            // Configure a new PV8 engine to play the game
            ConfigureEngine();

            tmpEngine.ActivateChip("GameChip",  new EmptyTemplateDemoChip());

            // Process the files
            ProcessFiles(tmpEngine, gameFiles);

        }

    }
}