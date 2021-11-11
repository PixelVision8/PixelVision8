using System;
using System.Globalization;
using System.IO;

namespace PixelVision8.Runner
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

            // TODO there is a bug where this will not go to the boot error
            using (var game = new LuaRunner(root))
            {
                game.Run();
            }

        }
    }

}
