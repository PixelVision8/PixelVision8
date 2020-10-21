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
using PixelVision8.Engine;
using PixelVision8.Engine.Chips;
using PixelVision8.Runner.Data;

namespace PixelVision8.Runner
{
    public class GameRunner : Game
    {

        protected static bool _mute;
        protected static int lastVolume;
        protected static int muteVolume;
        protected bool autoShutdown = false;
        protected TimeSpan elapsedTime = TimeSpan.Zero;
        protected int frameCounter;
        // protected RunnerMode mode;
        protected int timeDelta;
        public IDisplayTarget displayTarget;
        protected GraphicsDeviceManager graphics;
        protected bool resolutionInvalid = true;
        protected IEngine tmpEngine;

        public GameRunner()
        {
            // Fix a bug related to parsing numbers in Europe, among other things
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

            graphics = new GraphicsDeviceManager(this);

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
                    // typeof(MusicChip).FullName
                };

                return chips;
            }
        }

        protected virtual bool RunnerActive => IsActive;

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

        protected virtual void ConfigureRunner()
        {
            ConfigureDisplayTarget();
        }

        public virtual void ConfigureDisplayTarget()
        {
            // Create the default display target
            displayTarget = new DisplayTargetLite(graphics, 512, 480);
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
                ActiveEngine.ColorChip.ResetValidation();
            }

            displayTarget.Render(ActiveEngine.DisplayChip.Pixels, ActiveEngine.ColorChip.backgroundColor);

            // displayTarget.spriteBatch.End();
            if (resolutionInvalid)
            {
                ResetResolution();
                ResetResolutionValidation();
            }
        }

        public virtual void ConfigureEngine(Dictionary<string, string> metaData = null)
        {
            
            // It's now time to set up a new instance of the PixelVisionEngine. Here we are passing in the string 
            // names of the chips it should use.
            tmpEngine = CreateNewEngine(DefaultChips);
            // tmpEngine.Init();
            
            // ConfigureKeyboard();
            ConfigureControllers();
        }

        // protected virtual void ConfigureKeyboard()
        // {
        //     
        //     // tmpEngine.ControllerChip.RegisterKeyInput();
        // }

        protected virtual void ConfigureControllers()
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
            var overScanX = displayChip.OverscanXPixels;
            var overScanY = displayChip.OverscanYPixels;

            displayTarget.ResetResolution(gameWidth, gameHeight, overScanX, overScanY);
            IsMouseVisible = false;

            // Update the mouse to use the new monitor scale
            var scale = displayTarget.Scale;
            ActiveEngine.ControllerChip.MouseScale(scale.X, scale.Y);
        }

        public virtual void RunGame()
        {
            ActivateEngine(tmpEngine);
        }
    }
}