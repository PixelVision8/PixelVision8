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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using PixelVision8.Engine;
using PixelVision8.Engine.Chips;
using PixelVision8.Runner.Data;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace PixelVision8.Runner
{
    public class GameRunner : Game
    {

        protected static bool _mute;
        protected static int _lastVolume;
        protected static int _muteVolume;
        protected bool _autoShutdown = false;
        protected TimeSpan _elapsedTime = TimeSpan.Zero;
        protected int _frameCounter;
        protected int _timeDelta;
        public IDisplayTarget DisplayTarget;
        protected GraphicsDeviceManager _graphics;
        protected bool _resolutionInvalid = true;
        protected IEngine _tmpEngine;

        public GameRunner()
        {
            // Fix a bug related to parsing numbers in Europe, among other things
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

            _graphics = new GraphicsDeviceManager(this);

            IsFixedTimeStep = true;
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
                };

                return chips;
            }
        }

        protected virtual bool RunnerActive => IsActive;

        public virtual IEngine ActiveEngine { get; protected set; }

        public virtual int Volume(int? value = null)
        {
            if (value.HasValue)
                _lastVolume = value.Value;

            SoundEffect.MasterVolume = _lastVolume / 100f;

            if (_mute == true && _lastVolume > 0)
            {
                _muteVolume = _lastVolume;
            }

            return _lastVolume;
        }

        public virtual bool Mute(bool? value = null)
        {
            if (value.HasValue)
            {

                _mute = value.Value;

                if (_mute)
                {
                    _muteVolume = _lastVolume;

                    Volume(0);
                }
                else
                {
                    // Restore volume to halfway if un-muting and last  value was 0
                    if (_muteVolume < 5)
                    {
                        _muteVolume = 50;
                    }
                    Volume(_muteVolume);
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
                DisplayTarget.MonitorScale = scale.Value;

                InvalidateResolution();
            }

            return DisplayTarget.MonitorScale;
        }


        public virtual bool Fullscreen(bool? value = null)
        {
            if (value.HasValue)
            {
                DisplayTarget.Fullscreen = value.Value;

                InvalidateResolution();
            }

            return
                DisplayTarget.Fullscreen;
        }

        public virtual bool StretchScreen(bool? value = null)
        {
            if (value.HasValue)
            {
                DisplayTarget.StretchScreen = value.Value;
                InvalidateResolution();
            }

            return
                DisplayTarget.StretchScreen;
        }

        public virtual bool CropScreen(bool? value = null)
        {
            if (value.HasValue)
            {
                DisplayTarget.CropScreen = value.Value;
                InvalidateResolution();
            }

            return DisplayTarget.CropScreen;
        }

        protected virtual void ConfigureRunner()
        {
            ConfigureDisplayTarget();
        }

        public virtual void ConfigureDisplayTarget()
        {
            // Create the default display target
            DisplayTarget = new DisplayTargetLite(_graphics, 512, 480);
        }

        public void InvalidateResolution()
        {
            _resolutionInvalid = true;
        }

        public void ResetResolutionValidation()
        {
            _resolutionInvalid = false;
        }

        protected override void Update(GameTime gameTime)
        {
            // Before trying to update the PixelVisionEngine instance, we need to make sure it exists. The guard clause protects us from throwing an 
            // error when the Runner loads up and starts before we've had a chance to instantiate the new engine instance.
            if (ActiveEngine == null) return;

            _elapsedTime += gameTime.ElapsedGameTime;

            if (_elapsedTime > TimeSpan.FromSeconds(1))
            {
                _elapsedTime -= TimeSpan.FromSeconds(1);

                // Make sure the game chip has the current fps value
                ActiveEngine.GameChip.fps = _frameCounter;

                _frameCounter = 0;
            }

            if (RunnerActive)
            {
                _timeDelta = (int)(gameTime.ElapsedGameTime.TotalSeconds * 1000);

                // It's important that we pass in the Time.deltaTime to the PixelVisionEngine. It is passed along to any Chip that registers itself with 
                // the ChipManager to be updated. The ControlsChip, GamesChip, and others use this time delta to synchronize their actions based on the 
                // current frame rate.
                ActiveEngine.Update(_timeDelta);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            if (ActiveEngine == null) return;

            _frameCounter++;

            // Clear with black and draw the runner.
            _graphics.GraphicsDevice.Clear(Color.Black);

            // Now it's time to call the PixelVisionEngine's Draw() method. This Draw() call propagates throughout all of the Chips that have 
            // registered themselves as being able to draw such as the GameChip and the DisplayChip.

            // Only call draw if the window has focus
            if (RunnerActive) ActiveEngine.Draw();

            if (ActiveEngine.ColorChip.invalid)
            {
                DisplayTarget.RebuildColorPalette(ActiveEngine.ColorChip);
                ActiveEngine.ColorChip.ResetValidation();
            }

            DisplayTarget.Render(ActiveEngine.DisplayChip.Pixels, ActiveEngine.ColorChip.backgroundColor);

            // displayTarget.spriteBatch.End();
            if (_resolutionInvalid)
            {
                ResetResolution();
                ResetResolutionValidation();
            }
        }

        public virtual void ConfigureEngine(Dictionary<string, string> metaData = null)
        {

            // It's now time to set up a new instance of the PixelVisionEngine. Here we are passing in the string 
            // names of the chips it should use.
            _tmpEngine = CreateNewEngine(DefaultChips);

            // ConfigureKeyboard();
            ConfigureControllers();
        }

        protected virtual void ConfigureControllers()
        {
            _tmpEngine.ControllerChip.RegisterControllers();
        }

        public virtual void ShutdownActiveEngine()
        {
            // Look to see if there is an active engine

            // Show down the engine
            ActiveEngine?.Shutdown();

            // TODO need to move this over to the workspace
        }

        public virtual IEngine CreateNewEngine(List<string> chips)
        {
            return new PixelVisionEngineLite(chips.ToArray());
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
            
            DisplayTarget.ResetResolution(gameWidth, gameHeight);
            IsMouseVisible = false;

            // Update the mouse to use the new monitor scale
            var scale = DisplayTarget.Scale;
            ActiveEngine.ControllerChip.MouseScale(scale.X, scale.Y);
        }

        public virtual void RunGame()
        {
            ActivateEngine(_tmpEngine);
        }
    }
}