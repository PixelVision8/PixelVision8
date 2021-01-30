using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using PixelVision8.Player;

namespace PixelVision8.Runner
{
    public partial class GameRunner
    {
        protected virtual bool RunnerActive => IsActive;
        private TimeSpan _elapsedTime = TimeSpan.Zero;
        private int _frameCounter;
        protected int TimeDelta;
        protected PixelVision TmpEngine;
        public PixelVision ActiveEngine { get; protected set; }

        public virtual void ActivateEngine(PixelVision engine)
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
            ActivateEngine(TmpEngine);
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

                // Make sure the game chip has the current fps value which is incremented in the draw call
                ActiveEngine.FPS = _frameCounter;

                _frameCounter = 0;
            }

            if (RunnerActive)
            {
                TimeDelta = (int) (gameTime.ElapsedGameTime.TotalSeconds * 1000);

                // It's important that we pass in the Time.deltaTime to the PixelVisionEngine. It is passed along to any Chip that registers itself with 
                // the ChipManager to be updated. The ControlsChip, GamesChip, and others use this time delta to synchronize their actions based on the 
                // current frame rate.
                ActiveEngine.Update(TimeDelta);
            }
        }
    }
}