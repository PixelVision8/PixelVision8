using System;
using System.Globalization;
using System.IO;
using PixelVision8.Runner;

namespace PixelVision8Core
{
    public static class Program
    {

        [STAThread]
        public static void Main(string[] args)
        {
            // Fix a bug related to parsing numbers in Europe, among other things
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
        
            var root = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content");
        
            // Need to do this for MacOS
            if (root.EndsWith("/MonoBundle/Content")) root = root.Replace("/MonoBundle/Content", "/Resources/Content");
        
            // TODO there is a bug where this will not go to the boot error
            using (var game = new PixelVision8.Runner.PixelVision8Runner(root))
            // using (var game = new ShaderByteTest())
            {
                game.Run();
            }
        
        }
    }
}
