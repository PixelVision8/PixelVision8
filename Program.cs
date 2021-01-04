using System;
using System.IO;

namespace PixelVision8.Runner
{
    public static class Program
    {

        [STAThread]
        public static void Main(string[] args)
        {
            var root = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content");

            using (var game = new DesktopRunner(root, args))
            {
                game.Run();
            }

        }
    }
}