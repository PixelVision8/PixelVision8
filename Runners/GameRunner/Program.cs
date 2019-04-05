using System;
using System.Globalization;
using System.IO;
using PixelVision8.Runner;

namespace Desktop
{
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // Fix a bug related to parsing numbers in Europe, among other things
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
                    
            var root = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content");
                
            // Need to do this for MacOS
            if (root.EndsWith("/MonoBundle/Content"))
            {
                root = root.Replace("/MonoBundle/Content", "/Resources/Content");
            }
            
            using (var game = new GameRunner(root, "/App/DefaultGame/"))
                game.Run();
        }
    }
}
