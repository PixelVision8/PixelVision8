using Microsoft.Xna.Framework;

namespace PixelVision8.Runner
{
    public partial class GameRunner
    {
        protected bool _resolutionInvalid = true;
        public DisplayTarget DisplayTarget;

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

        public virtual void ConfigureDisplayTarget()
        {
            // Create the default display target
            DisplayTarget = new DisplayTarget(Graphics, 512, 480);
        }

        public void InvalidateResolution()
        {
            _resolutionInvalid = true;
        }

        public void ResetResolutionValidation()
        {
            _resolutionInvalid = false;
        }

        public virtual void ResetResolution()
        {
            DisplayTarget.ResetResolution(ActiveEngine.DisplayChip.Width, ActiveEngine.DisplayChip.Height);
        }

        protected override void Draw(GameTime gameTime)
        {
            if (ActiveEngine == null) return;

            _frameCounter++;

            // Clear with black and draw the runner.
            Graphics.GraphicsDevice.Clear(Color.Black);

            // Now it's time to call the PixelVisionEngine's Draw() method. This Draw() call propagates throughout all of the Chips that have 
            // registered themselves as being able to draw such as the GameChip and the DisplayChip.

            // Only call draw if the window has focus
            if (RunnerActive) ActiveEngine.Draw();

            if (ActiveEngine.ColorChip.Invalid)
            {
                // Make sure the color palette doesn't need to rebuild itself
                DisplayTarget.RebuildColorPalette(
                    ActiveEngine.ColorChip.HexColors, 
                    ActiveEngine.ColorChip.BackgroundColor,
                    ActiveEngine.ColorChip.MaskColor,
                    ActiveEngine.ColorChip.DebugMode
                );
                
                ActiveEngine.ColorChip.ResetValidation();
            }

            DisplayTarget.Render(ActiveEngine.DisplayChip.Pixels, ActiveEngine.ColorChip.BackgroundColor);

            // displayTarget.spriteBatch.End();
            if (_resolutionInvalid)
            {
                ResetResolution();
                ResetResolutionValidation();
            }
        }
    }
}