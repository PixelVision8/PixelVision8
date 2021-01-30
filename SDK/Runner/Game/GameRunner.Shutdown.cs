namespace PixelVision8.Runner
{
    public partial class GameRunner
    {
        protected bool _autoShutdown = false;

        public virtual void ShutdownActiveEngine()
        {
            // Look to see if there is an active engine

            // Show down the engine
            ActiveEngine?.Shutdown();

            // TODO need to move this over to the workspace
        }
    }
}