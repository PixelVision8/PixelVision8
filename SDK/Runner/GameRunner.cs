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
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using PixelVision8.Engine;
using PixelVision8.Engine.Chips;
using PixelVision8.Engine.Services;
using PixelVision8.Runner.Data;
using PixelVision8.Runner.Services;
using PixelVision8.Runner.Utils;

using Buttons = PixelVision8.Engine.Chips.Buttons;

namespace PixelVision8.Runner
{
    public class GameRunner : Game, IRunner
    {

        public enum ErrorCode
        {
            Exception,
            LoadError,
            NoAutoRun
        }

        // Runner modes
        public enum RunnerMode
        {
            Playing,
            Booting,
            Loading,
            Error
        }

        private static bool _mute;
        private static int lastVolume;
        private static int muteVolume;

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
            {InputMap.Player2SelectKey, (int) Keys.OemSemicolon},
            {InputMap.Player2StartKey, (int) Keys.OemComma},
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

        protected bool autoShutdown = false;
        protected bool displayProgress;
        public DisplayTarget displayTarget;
        protected TimeSpan elapsedTime = TimeSpan.Zero;
        protected int frameCounter;
        protected GraphicsDeviceManager graphics;
        public LoadService loadService;
        protected RunnerMode mode;
        protected bool resolutionInvalid = true;
        public IServiceLocator ServiceManager { get; }
        protected int timeDelta;
        protected IEngine tmpEngine;

        public GameRunner()
        {
            // Fix a bug related to parsing numbers in Europe, among other things
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

            graphics = new GraphicsDeviceManager(this);

            ServiceManager = new ServiceManager();
            //            IsFixedTimeStep = true;
        }

        

        // Default chips for the engine
        public virtual List<string> DefaultChips
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
                    typeof(SoundChip).FullName,
                    typeof(MusicChip).FullName
                };

                return chips;
            }
        }

        protected virtual bool RunnerActive
        {
            get
            {
                if (autoShutdown && mode != RunnerMode.Loading) return IsActive;

                return true;
            }
        }

        public virtual IEngine ActiveEngine { get; protected set; }

        public virtual int Volume(int? value = null)
        {
            if (value.HasValue) 
                lastVolume = value.Value;

            SoundEffect.MasterVolume = lastVolume / 100f;

            if (_mute == true && lastVolume > 0)
            {
                muteVolume = lastVolume;
            }

            return lastVolume;
        }

        public virtual bool Mute(bool? value = null)
        {
            if (value.HasValue)
            {
                
                _mute = value.Value;

                if (_mute)
                {
                    muteVolume = lastVolume;

                    Volume(0);
                }
                else
                {
                    // Restore volume to halfway if un-muting and last  value was 0
                    if (muteVolume < 5)
                    {
                        muteVolume = 50;
                    }
                    Volume(muteVolume);
                }
                
            }

            return SoundEffect.MasterVolume == 0 || _mute;
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
                displayTarget.monitorScale = scale.Value; //MathHelper.Clamp(, 1, 6);

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
            }

            return
                displayTarget
                    .fullscreen;
        }

        public virtual bool StretchScreen(bool? value = null)
        {
            if (value.HasValue)
            {
                displayTarget.stretchScreen = value.Value;
                InvalidateResolution();
            }

            return
                displayTarget
                    .stretchScreen;
        }

        public virtual bool CropScreen(bool? value = null)
        {
            if (value.HasValue)
            {
                displayTarget.cropScreen = value.Value;
                InvalidateResolution();
            }

            return
                displayTarget
                    .cropScreen;
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
        public virtual void ProcessFiles(IEngine tmpEngine, string[] files,
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

            //            CreateAudioPlayerFactory();

            CreateLoadService();
        }

        public virtual void CreateLoadService()
        {
            loadService = new LoadService(new FileLoadHelper());
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
            // Before trying to update the PixelVisionEngine instance, we need to make sure it exists. The guard clause protects us from throwing an 
            // error when the Runner loads up and starts before we've had a chance to instantiate the new engine instance.
            if (ActiveEngine == null) return;

            elapsedTime += gameTime.ElapsedGameTime;

            if (elapsedTime > TimeSpan.FromSeconds(1))
            {
                elapsedTime -= TimeSpan.FromSeconds(1);

                // Make sure the game chip has the current fps value
                ActiveEngine.GameChip.fps = frameCounter;

                frameCounter = 0;
            }

            if (RunnerActive)
            {
                timeDelta = (int) (gameTime.ElapsedGameTime.TotalSeconds * 1000);

                // It's important that we pass in the Time.deltaTime to the PixelVisionEngine. It is passed along to any Chip that registers itself with 
                // the ChipManager to be updated. The ControlsChip, GamesChip, and others use this time delta to synchronize their actions based on the 
                // current frame rate.
                ActiveEngine.Update(timeDelta);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            if (ActiveEngine == null) return;

            frameCounter++;

            // Clear with black and draw the runner.
            graphics.GraphicsDevice.Clear(Color.Black);

            // Now it's time to call the PixelVisionEngine's Draw() method. This Draw() call propagates throughout all of the Chips that have 
            // registered themselves as being able to draw such as the GameChip and the DisplayChip.

            // Only call draw if the window has focus
            if (RunnerActive) ActiveEngine.Draw();

            if (ActiveEngine.ColorChip.invalid)
            {
                displayTarget.RebuildColorPalette(ActiveEngine.ColorChip);
            }

            displayTarget.Render(ActiveEngine.DisplayChip.Pixels);

            // displayTarget.spriteBatch.End();
            if (resolutionInvalid)
            {
                ResetResolution();
                ResetResolutionValidation();
            }
        }

        public void ParseFiles(string[] files, IEngine engine, SaveFlags saveFlags,
            bool autoLoad = true)
        {
            loadService.ParseFiles(files, engine, saveFlags);

            if (autoLoad) loadService.LoadAll();
        }

        public virtual void ConfigureEngine(Dictionary<string, string> metaData = null)
        {
            // Detect if we should be preloading the archive
            if (mode == RunnerMode.Booting || mode == RunnerMode.Error || mode == RunnerMode.Loading)
                displayProgress = false;
            else
                displayProgress = true;

            if (ActiveEngine != null) ShutdownActiveEngine();

            // Pixel Vision 8 has a built in the JSON serialize/de-serialize. It allows chips to be dynamically 
            // loaded by their full class name. Above we are using typeof() along with the FullName property to 
            // get the string values for each chip. The engine will parse this string and automatically create 
            // the chip then register it with the ChipManager. You can manually instantiate chips but its best 
            // to let the engine do it for you.
            
            // It's now time to set up a new instance of the PixelVisionEngine. Here we are passing in the string 
            // names of the chips it should use.
            tmpEngine = CreateNewEngine(DefaultChips);

            ConfigureServices();

            // Pass all meta data into the engine instance
            if (metaData != null)
                foreach (var entry in metaData)
                    tmpEngine.SetMetadata(entry.Key, entry.Value);

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

                tmpEngine.SetMetadata(keyMap.Key.ToString(), keyValue.ToString());
            }

            tmpEngine.ControllerChip.RegisterKeyInput();
        }

        protected virtual void ConfiguredControllers()
        {
            tmpEngine.ControllerChip.RegisterControllers();
        }

        public virtual void ShutdownActiveEngine()
        {
            // Look to see if there is an active engine

            // Show down the engine
            ActiveEngine?.Shutdown();

            // TODO need to move this over to the workspace
        }

        public IEngine CreateNewEngine(List<string> chips)
        {
            return new PixelVisionEngine(ServiceManager, chips.ToArray());
        }

        public virtual void ConfigureServices()
        {
            // TODO need to overwrite with any custom services you need the engine to load
        }

        public virtual void ActivateEngine(IEngine engine)
        {
            if (engine == null) return;

            // Make the loaded engine active
            ActiveEngine = engine;

            ActiveEngine.ResetGame();

            // After loading the game, we are ready to run it.
            ActiveEngine.RunGame();

            // Reset the game's resolution
            ResetResolution();

            // Make sure that the first frame is cleared with the default color
            ActiveEngine.GameChip.Clear();
        }

        public void ResetResolution()
        {
            if (ActiveEngine == null) return;

            var displayChip = ActiveEngine.DisplayChip;

            var gameWidth = displayChip.Width;
            var gameHeight = displayChip.Height;
            var overScanX = displayChip.OverscanXPixels;
            var overScanY = displayChip.OverscanYPixels;

            displayTarget.ResetResolution(gameWidth, gameHeight, overScanX, overScanY);
            IsMouseVisible = false;

            // Update the mouse to use the new monitor scale
            var scale = displayTarget.scale;
            ActiveEngine.ControllerChip.MouseScale(scale.X, scale.Y);
        }

        protected void ParseFiles(string[] files, SaveFlags? flags = null)
        {
            if (!flags.HasValue)
            {
                flags = SaveFlags.System;
                flags |= SaveFlags.Colors;
                flags |= SaveFlags.ColorMap;
                flags |= SaveFlags.Sprites;
                flags |= SaveFlags.Tilemap;
                flags |= SaveFlags.Fonts;
                flags |= SaveFlags.Sounds;
                flags |= SaveFlags.Music;
                flags |= SaveFlags.SaveData;
                flags |= SaveFlags.MetaSprites;
            }

            loadService.ParseFiles(files, tmpEngine, flags.Value);
        }

        public virtual void RunGame()
        {
            ActivateEngine(tmpEngine);
        }
    }
}