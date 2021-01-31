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

namespace PixelVision8.Player
{
    public partial class PixelVision
    {
        /// <summary>
        ///     This method is called when shutting down the engine
        /// </summary>
        /// <tocexclude />
        public virtual void Shutdown()
        {
            // Shutdown chips
            foreach (var chip in Chips) chip.Value.Shutdown();
        }

    }
}