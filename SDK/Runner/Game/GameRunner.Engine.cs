using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using PixelVision8.Player;

namespace PixelVision8.Runner
{
    public partial class GameRunner
    {
        protected bool _autoShutdown = false;

        protected virtual bool RunnerActive => IsActive;
        protected TimeSpan _elapsedTime = TimeSpan.Zero;
        protected int _frameCounter;
        protected int _timeDelta;
        protected IPlay _tmpEngine;
        public virtual IPlay ActiveEngine { get; protected set; }
 
        public virtual IPlay CreateNewEngine(List<string> chips)
        {
            return new PixelVision(chips.ToArray());
        }

        public virtual void ActivateEngine(IPlay engine)
        {
            if (engine == null) return;

            // Make the loaded engine active
            ActiveEngine = engine;

            ActiveEngine.ResetGame();

            // After loading the game, we are ready to run it.
            ActiveEngine.RunGame();

        }

        public virtual void RunGame()
        {
            ActivateEngine(_tmpEngine);
        }
        
        public virtual void ConfigureEngine(List<string> chips)
        {

            // It's now time to set up a new instance of the PixelVisionEngine. Here we are passing in the string 
            // names of the chips it should use.
            _tmpEngine = CreateNewEngine(chips);

            // ConfigureKeyboard();
            // ConfigureControllers();
        }

        // protected virtual void ConfigureControllers()
        // {
        //     _tmpEngine.ControllerChip.RegisterControllers();
        // }

        public virtual void ShutdownActiveEngine()
        {
            // Look to see if there is an active engine

            // Show down the engine
            ActiveEngine?.Shutdown();

            // TODO need to move this over to the workspace
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
                ActiveEngine.FPS = _frameCounter;

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
        
        
    }
}