using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using MonoGameRunner;
using PixelVisionRunner.Chips;
using PixelVisionRunner.Services;
using PixelVisionSDK;

namespace PixelVisionLua
{
    public class LuaRunner : RunnerGame
    {
        // Store the path to the game's files
        private readonly string gamePath;
        private IControllerChip controllerChip;
        
        /// <summary>
        ///     This constructor saves the path to the game's files and kicks off the base constructor
        /// </summary>
        /// <param name="gamePath"></param>
        public LuaRunner(string gamePath) : base()
        {
            this.gamePath = gamePath;
        }
        
        /// <summary>
        ///     The base runner contains a list of the core chips. Here you'll want to add the game chip to the list so it can run. This is called when a new game is created by the runner.
        /// </summary>
        public override List<string> defaultChips {
            get
            {
                
                // Get the list of default chips
                var chips = base.defaultChips;
                
                // Add the custom C# game chip
                chips.Add(typeof(LuaGameChip).FullName);
                
                // Return the list of chips
                return chips;
            }
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
            var fileExtensions = new[]
            {
                "lua",
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
    
            // Process the files
            ProcessFiles(tmpEngine, gameFiles);
            
            controllerChip = activeEngine.controllerChip;

        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            
            if (controllerChip.GetKeyDown(Keys.LeftControl) ||
                controllerChip.GetKeyDown(Keys.LeftControl))
            {

                if (controllerChip.GetKeyUp(Keys.R) || controllerChip.GetKeyUp(Keys.Alpha4))
                {
                    ResetGame();
                }
            }
        }

        public override void ResetGame()
        {
            LoadDefaultGame();
        }
    }
}