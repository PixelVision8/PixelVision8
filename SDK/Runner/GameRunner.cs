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
using System.Globalization;
using Microsoft.Xna.Framework;
using PixelVision8.Engine;
using PixelVision8.Engine.Chips;
using PixelVision8.Engine.Services;
using PixelVision8.Runner.Audio;
using PixelVision8.Runner.Chips.Sfxr;
using PixelVision8.Runner.Data;
using PixelVision8.Runner.Services;

namespace PixelVision8.Runner
{
    public class GameRunner : Game, IRunner
    {
        public enum BiosSettings
        {
            Resolution,
            Scale,
            Volume,
            Mute,
            SystemName,
            SystemVersion,
            FullScreen,
            StretchScreen,
            CropScreen
        }

        public enum ErrorCode
        {
            Exception,
            LoadError,
            NoAutoRun,
            NoDefaultTool,
            Warning
        }

        // Runner modes
        public enum RunnerMode
        {
            Playing,
            Booting,
            Loading,
            Error
        }

        public readonly Dictionary<InputMap, int> defaultKeys = new Dictionary<InputMap, int>
        {
            {InputMap.Player1UpKey, (int) Keys.Up},
            {InputMap.Player1DownKey, (int) Keys.Down},
            {InputMap.Player1RightKey, (int) Keys.Right},
            {InputMap.Player1LeftKey, (int) Keys.Left},
            {InputMap.Player1SelectKey, (int) Keys.A},
            {InputMap.Player1StartKey, (int) Keys.S},
            {InputMap.Player1AKey, (int) Keys.X},
            {InputMap.Player1BKey, (int) Keys.C},
            {InputMap.Player1UpButton, (int) Buttons.Up},
            {InputMap.Player1DownButton, (int) Buttons.Down},
            {InputMap.Player1RightButton, (int) Buttons.Right},
            {InputMap.Player1LeftButton, (int) Buttons.Left},
            {InputMap.Player1SelectButton, (int) Buttons.Select},
            {InputMap.Player1StartButton, (int) Buttons.Start},
            {InputMap.Player1AButton, (int) Buttons.A},
            {InputMap.Player1BButton, (int) Buttons.B},
            {InputMap.Player2UpKey, (int) Keys.I},
            {InputMap.Player2DownKey, (int) Keys.K},
            {InputMap.Player2RightKey, (int) Keys.L},
            {InputMap.Player2LeftKey, (int) Keys.J},
            {InputMap.Player2SelectKey, (int) Keys.Semicolon},
            {InputMap.Player2StartKey, (int) Keys.Comma},
            {InputMap.Player2AKey, (int) Keys.Enter},
            {InputMap.Player2BKey, (int) Keys.RightShift},
            {InputMap.Player2UpButton, (int) Buttons.Up},
            {InputMap.Player2DownButton, (int) Buttons.Down},
            {InputMap.Player2RightButton, (int) Buttons.Right},
            {InputMap.Player2LeftButton, (int) Buttons.Left},
            {InputMap.Player2SelectButton, (int) Buttons.Select},
            {InputMap.Player2StartButton, (int) Buttons.Start},
            {InputMap.Player2AButton, (int) Buttons.A},
            {InputMap.Player2BButton, (int) Buttons.B}
        };

//        protected int _scale = 1;
//        protected bool cropScreen = true;

        protected bool debugLayers = true;
        protected bool displayProgress;

        public DisplayTarget displayTarget;
        protected TimeSpan elapsedTime = TimeSpan.Zero;
        protected int frameCounter;
//        protected bool fullscreen;

        protected GraphicsDeviceManager graphics;
        protected RunnerMode lastMode;
        protected LoadService loadService;
        protected RunnerMode mode;

        protected bool resolutionInvalid = true;
//        protected bool stretchScreen;
        protected float timeDelta;
        protected IEngine tmpEngine;
        protected IServiceLocator serviceManager;

        public GameRunner()
        {
            // Fix a bug related to parsing numbers in Europe, among other things
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

            graphics = new GraphicsDeviceManager(this);
            
            serviceManager = new ServiceManager();
//            IsFixedTimeStep = true;
        }

        protected bool exporting { get; set; }
        protected AudioPlayerFactory audioPlayerFactory { get; set; }

        public string LocalStorage
        {
            get
            {
                var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

                // TODO replace with something better
                DisplayWarning("Local Storage Located at " + localAppData);
//                Console.WriteLine("Local Storage " + localAppData);
                return localAppData;
            }
        }

        // Default chips for the engine
        public virtual List<string> defaultChips
        {
            get
            {
                var chips = new List<string>
                {
                    typeof(ColorChip).FullName,
                    typeof(SpriteChip).FullName,
                    typeof(TilemapChip).FullName,
                    typeof(FontChip).FullName,
                    typeof(ControllerChip).FullName,
                    typeof(DisplayChip).FullName,
                    typeof(MusicChip).FullName,
                    typeof(SfxrSoundChip).FullName
                };

                return chips;
            }
        }

        public virtual IEngine activeEngine { get; protected set; }

        /// <summary>
        ///     Change the volume value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual int Volume(int? value = null)
        {
            var vol = audioPlayerFactory.Volume(value);
            return vol;
        }

        /// <summary>
        ///     Change the mute value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public virtual bool Mute(bool? value = null)
        {
            var mute = audioPlayerFactory.Mute(value);
            return mute;
        }


        /// <summary>
        ///     Scale the resolution.
        /// </summary>
        /// <param name="scale"></param>
        /// <param name="fullScreen"></param>
        public virtual int Scale(int? scale = null)
        {
            if (scale.HasValue)
            {
                displayTarget.monitorScale = scale.Value;//MathHelper.Clamp(, 1, 6);

//                ResetResolution();
                InvalidateResolution();
            }

            return displayTarget.monitorScale;
        }


        public virtual bool Fullscreen(bool? value = null)
        {
            if (value.HasValue)
            {
                displayTarget.fullscreen = value.Value;

                InvalidateResolution();
                //ResetResolution();
            }

            return displayTarget.fullscreen; //Convert.ToBoolean(workspaceService.ReadBiosData(BiosSettings.FullScreen.ToString(), "False") as string);
        }

        public virtual bool StretchScreen(bool? value = null)
        {
            if (value.HasValue)
            {
//                var newScale = scale.Value.Clamp(1, 8);
                displayTarget.stretchScreen = value.Value;

//                workspaceService.UpdateBiosData(BiosSettings.StretchScreen.ToString(), value.Value.ToString());
//                ResetResolution();
                InvalidateResolution();
//                displayTarget?.MonitorResolution(scale: scale);
            }

            return
                displayTarget.stretchScreen; //Convert.ToBoolean(workspaceService.ReadBiosData(BiosSettings.StretchScreen.ToString(), "False") as string);
        }

        public virtual bool CropScreen(bool? value = null)
        {
            if (value.HasValue)
            {
//                var newScale = scale.Value.Clamp(1, 8);

                displayTarget.cropScreen = value.Value;

//                workspaceService.UpdateBiosData(BiosSettings.CropScreen.ToString(), value.Value.ToString());
                //ResetResolution();
                InvalidateResolution();
//                displayTarget?.MonitorResolution(scale: scale);
            }

            return displayTarget.cropScreen; //Convert.ToBoolean(workspaceService.ReadBiosData(BiosSettings.CropScreen.ToString(), "False") as string);
        }

        public void DebugLayers(bool value)
        {
            debugLayers = value;
        }

        public void ToggleLayers(int value)
        {
            if (!debugLayers)
                return;

//                if (mode == RunnerMode.Play)
            activeEngine.displayChip.layers = value - 1;
        }

        /// <summary>
        ///     Reset the currently running game.
        /// </summary>
        /// <param name="showBoot"></param>
        public virtual void ResetGame()
        {
            throw new NotImplementedException();
        }


        public virtual void DisplayWarning(string message)
        {
            // TODO this should be routed down to the game to show an error of some sort
        }

        // TODO should this be moved over to the workspace?
        /// <summary>
        ///     This method goes through the list of files and prepares them for the load service
        /// </summary>
        /// <param name="tmpEngine"></param>
        /// <param name="files"></param>
        /// <param name="displayProgress"></param>
        public virtual void ProcessFiles(IEngine tmpEngine, Dictionary<string, byte[]> files,
            bool displayProgress = false)
        {
            this.displayProgress = displayProgress;

            this.tmpEngine = tmpEngine;

            ParseFiles(files);

            if (!displayProgress)
            {
                loadService.LoadAll();
                RunGame();
            }
        }

        protected virtual void ConfigureRunner()
        {
            ConfigureDisplayTarget();

            CreateAudioPlayerFactory();

            CreateLoadService();
        }

        public virtual void CreateAudioPlayerFactory()
        {
            audioPlayerFactory = new AudioPlayerFactory();

            SfxrSynth.AudioPlayerFactory = audioPlayerFactory;
        }

        public virtual void CreateLoadService()
        {
            loadService = new LoadService();
        }

        public virtual void ConfigureDisplayTarget()
        {
            // Create the default display target
            displayTarget = new DisplayTarget(graphics, 512, 480);
            
        }

        public void InvalidateResolution()
        {
            resolutionInvalid = true;
        }

        public void ResetResolutionValidation()
        {
            resolutionInvalid = false;
        }


        protected override void Update(GameTime gameTime)
        {
            if (activeEngine == null)
                return;
            
            

            timeDelta = (float) gameTime.ElapsedGameTime.TotalSeconds;

            elapsedTime += gameTime.ElapsedGameTime;

            if (elapsedTime > TimeSpan.FromSeconds(1))
            {
                elapsedTime -= TimeSpan.FromSeconds(1);

                // Make sure the game chip has the current fps value
                activeEngine.gameChip.fps = frameCounter;

                // Save this to the runner
//                frameRate = frameCounter;

                frameCounter = 0;
            }

            // Before trying to update the PixelVisionEngine instance, we need to make sure it exists. The guard clause protects us from throwing an 
            // error when the Runner loads up and starts before we've had a chance to instantiate the new engine instance.

            activeEngine.Update(timeDelta);

            // It's important that we pass in the Time.deltaTime to the PixelVisionEngine. It is passed along to any Chip that registers itself with 
            // the ChipManager to be updated. The ControlsChip, GamesChip, and others use this time delta to synchronize their actions based on the 
            // current framerate.
        }

        protected override void Draw(GameTime gameTime)
        {
            
            if (activeEngine == null)
                return;

            frameCounter++;

            // Clear with black and draw the runner.
//            else
//            {
            graphics.GraphicsDevice.Clear(Color.Black);
            // Now it's time to call the PixelVisionEngine's Draw() method. This Draw() call propagates throughout all of the Chips that have 
            // registered themselves as being able to draw such as the GameChip and the DisplayChip.
            activeEngine.Draw();

//            if (activeEngine.running && displayTarget != null)
//            {
//                if (displayTarget != null)
//                {
            displayTarget.Render(activeEngine.displayChip.pixels);
//                }
//            }


//            }

            if (resolutionInvalid)
            {
                ResetResolution();
                ResetResolutionValidation();
            }
        }

        public void ParseFiles(Dictionary<string, byte[]> files, IEngine engine, SaveFlags saveFlags,
            bool autoLoad = true)
        {
            loadService.ParseFiles(files, engine, saveFlags);

            if (autoLoad)
                loadService.LoadAll();
        }

        public virtual void ConfigureEngine(Dictionary<string, string> metaData = null)
        {
            // Detect if we should be preloading the archive
            if (mode == RunnerMode.Booting || mode == RunnerMode.Error || mode == RunnerMode.Loading)
                displayProgress = false;
            else
                displayProgress = true;

            if (activeEngine != null) ShutdownActiveEngine();

//		    // Pixel Vision 8 has a built in the JSON serialize/de-serialize. It allows chips to be dynamically 
//		    // loaded by their full class name. Above we are using typeof() along with the FullName property to 
//		    // get the string values for each chip. The engine will parse this string and automatically create 
//		    // the chip then register it with the ChipManager. You can manually instantiate chips but its best 
//		    // to let the engine do it for you.
//
//		    // It's now time to set up a new instance of the PixelVisionEngine. Here we are passing in the string 
//		    // names of the chips it should use.
            tmpEngine = CreateNewEngine(defaultChips);

            ConfigureServices();

            // Pass all meta data into the engine instance
            if (metaData != null)
                foreach (var entry in metaData)
                    tmpEngine.SetMetaData(entry.Key, entry.Value);

            ConfigureKeyboard();

            ConfiguredControllers();
        }

        protected virtual void ConfigureKeyboard()
        {
            // Pass input mapping
            foreach (var keyMap in defaultKeys)
            {
                var rawValue = keyMap.Value;

                var keyValue = rawValue;

                tmpEngine.SetMetaData(keyMap.Key.ToString(), keyValue.ToString());
            }

            tmpEngine.controllerChip.RegisterKeyInput();
        }

        protected virtual void ConfiguredControllers()
        {
            tmpEngine.controllerChip.RegisterControllers();
        }

        public virtual void ShutdownActiveEngine()
        {
            // Look to see if there is an active engine
            if (activeEngine == null)
                return;

            // Show down the engine
            activeEngine.Shutdown();


            // TODO need to move this over to the workspace
        }

        public IEngine CreateNewEngine(List<string> chips)
        {
            return new PixelVisionEngine(serviceManager, chips.ToArray());
        }

        public virtual void ConfigureServices()
        {
            // TODO need to overwrite with any custom services you need the engine to load
        }

//        public int ReadPreLoaderPercent()
//        {
//            return (int) (loadService.percent * 100);
//        }


        /// <summary>
        ///     Reads a metadata property from the active game engine. It requires a string for the key and returns a
        ///     string value. You can also supply an optional default value. When a game is loaded, it can be passed
        ///     metadata to help retain state between loading, and this allows you to read that data.
        /// </summary>
        /// <param name="key">A string for the metadata property's key.</param>
        /// <param name="defaultValue"></param>
        /// <returns>If no key is found, this will return the default data, so it does not return nil.</returns>
//        public string ReadMetaData(string key, string defaultValue = "undefined")
//        {
//            return activeEngine.GetMetaData(key, defaultValue);
//        }
//
//        public void WriteMetaData(string key, string value)
//        {
//            activeEngine.SetMetaData(key, value);
//        }

        #region Preloader APIs

        

        #endregion

        public virtual void ActivateEngine(IEngine engine)
        {
            if (engine == null)
                return;

            // Make the loaded engine active
            activeEngine = engine;

            // After loading the game, we are ready to run it.
            activeEngine.RunGame();
            
//            if (displayTarget != null)
//            {

//            displayTarget.CacheColors(activeEngine);


            // Reset the game's resolution
            ResetResolution();

//            InvalidateResolution();
//            }
        }

        public void ResetResolution()
        {
            if (activeEngine == null)
                return;

            var displayChip = activeEngine.displayChip;

            var gameWidth = displayChip.width;
            var gameHeight = displayChip.height;
            var overScanX = displayChip.overscanXPixels;
            var overScanY = displayChip.overscanYPixels;
            
            displayTarget.ResetResolution(gameWidth, gameHeight, overScanX, overScanY);
            IsMouseVisible = false;
            
            // Update the mouse to use the new monitor scale
            var scale = displayTarget.scale;
            activeEngine.controllerChip.MouseScale(scale.X, scale.Y);
        }

        protected void ParseFiles(Dictionary<string, byte[]> files, SaveFlags? flags = null)
        {
            if (!flags.HasValue)
            {
                flags = SaveFlags.System;
                flags |= SaveFlags.Code;
                flags |= SaveFlags.Colors;
                flags |= SaveFlags.ColorMap;
                flags |= SaveFlags.Sprites;
                flags |= SaveFlags.Tilemap;
                flags |= SaveFlags.TilemapFlags;
                flags |= SaveFlags.Fonts;
                flags |= SaveFlags.Sounds;
                flags |= SaveFlags.Music;
                flags |= SaveFlags.SaveData;
            }

            loadService.ParseFiles(files, tmpEngine, flags.Value);
        }

        public void RunGame()
        {
            ActivateEngine(tmpEngine);
        }
    }
}