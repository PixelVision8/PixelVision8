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
using PixelVision8.Player;

namespace PixelVision8.Runner
{

    public class CSharpRunner : GameRunner
    {

        // Store the path to the game's files
        // private readonly string gamePath;
        private readonly string gameClass;
        
        /// <summary>
        ///     This constructor saves the path to the game's files and kicks off the base constructor
        /// </summary>
        /// <param name="gamePath"></param>
        public CSharpRunner(string gamePath, string gameClass) : base(gamePath)
        {
            // this.gamePath = gamePath;
            this.gameClass = gameClass;
        }
        
        /// <summary>
        ///     This is called when the runner first starts up.
        /// </summary>
        protected override void Initialize()
        {

            // Configure the runner
            ConfigureDisplayTarget();
            
            // Manually override scale on boot up
            Scale(1);

            // Load the game
            LoadDefaultGame();
        }

        public override void ActivateEngine(PixelVision engine)
        {
            base.ActivateEngine(engine);
            
            // Reset the game's resolution before it loads up
            ResetResolution();

            // Make sure that the first frame is cleared with the default color
            ActiveEngine.GameChip.Clear();
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
                typeof(FontChip).FullName,
                typeof(ControllerChip).FullName,
                typeof(DisplayChip).FullName,
                typeof(SoundChip).FullName
            };

            // Configure a new PV8 engine to play the game
            TmpEngine = new PixelVision(chips.ToArray());

            // Load the default class based on it's full package path
            TmpEngine.GetChip(gameClass);

            var fileHelper = new FileLoadHelper();
            var imageParser = new PNGParser(Graphics.GraphicsDevice);

            var loader = new Loader(fileHelper, imageParser);

            // Process the files
            loader.ParseFiles(gameFiles.ToArray(), TmpEngine);
            loader.LoadAll();

            RunGame();

        }

    }

   
}