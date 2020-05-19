using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using PixelVision8.CoreDesktop;

namespace PixelVision8Core
{
    public static class Program
    {

        // Clear Example (Array.Clear) winner
        // static void Main()
        // {
        //     byte[] array = new byte[128 * 128];
        //
        //     // Version 1: clear with Array.Clear.
        //     var s1 = Stopwatch.StartNew();
        //     for (int i = 0; i < _max; i++)
        //     {
        //         Array.Clear(array, 0, array.Length);
        //     }
        //     s1.Stop();
        //
        //     // Version 2: clear with for-loop.
        //     var s2 = Stopwatch.StartNew();
        //     for (int i = 0; i < _max; i++)
        //     {
        //         for (int z = 0; z < array.Length; z++)
        //         {
        //             array[z] = 0;
        //         }
        //     }
        //     s2.Stop();
        //     
        //     Debug.WriteLine(s1.Elapsed.TotalMilliseconds);
        //     Debug.WriteLine(s2.Elapsed.TotalMilliseconds);
        // }

        // Array copy
        // static void Main()
        // {
        //     var total = 512 * 512;
        //     var src = new byte[total];
        //     var dst = new byte[total];
        //     byte offset = 5;
        //
        //     // Version 1: clear with Array.Clear.
        //     var s1 = Stopwatch.StartNew();
        //     for (int i = 0; i < _max; i++)
        //     {
        //         Array.Copy(src, dst, total);
        //     }
        //     s1.Stop();
        //
        //
        //     // Version 2: clear with for-loop.
        //     var s2 = Stopwatch.StartNew();
        //     for (int i = 0; i < _max; i++)
        //     {
        //         Buffer.BlockCopy(src, 0, dst, 0, total);
        //
        //     }
        //     s2.Stop();
        //    
        //
        //     // Version 2: clear with for-loop.
        //     var s3 = Stopwatch.StartNew();
        //     for (int i = 0; i < _max; i++)
        //     {
        //         for (int j = 0; j < total; j++)
        //         {
        //             dst[j] = src[j];
        //         }
        //
        //     }
        //     s3.Stop();
        //     
        //
        //     // Version 2: clear with for-loop.
        //     var s4 = Stopwatch.StartNew();
        //     for (int i = 0; i < _max; i++)
        //     {
        //         Parallel.For(0, _max, x =>
        //         {
        //             dst[x] = src[x];
        //         });
        //
        //     }
        //     s4.Stop();
        //     
        //     // Version 2: clear with for-loop.
        //     var s5 = Stopwatch.StartNew();
        //     for (int i = 0; i < _max; i++)
        //     {
        //         for (int j = 0; j < total; j++)
        //         {
        //             dst[j] = (byte) (src[j] + offset);
        //         }
        //
        //     }
        //     s5.Stop();
        //     
        //
        //     // Version 2: clear with for-loop.
        //     var s6 = Stopwatch.StartNew();
        //     for (int i = 0; i < _max; i++)
        //     {
        //         Parallel.For(0, _max, x =>
        //         {
        //             dst[x] = (byte)(src[x] + offset);
        //         });
        //
        //     }
        //     s6.Stop();
        //
        //     Debug.WriteLine("Array.Copy " + s1.Elapsed.TotalMilliseconds);
        //     Debug.WriteLine("Block.Copy " + s2.Elapsed.TotalMilliseconds);
        //     Debug.WriteLine("For loop " + s3.Elapsed.TotalMilliseconds);
        //     Debug.WriteLine("Parallel or loop " + s4.Elapsed.TotalMilliseconds);
        //     Debug.WriteLine("For loop increment " + s5.Elapsed.TotalMilliseconds);
        //     Debug.WriteLine("Parallel for loop increment " + s6.Elapsed.TotalMilliseconds);
        //     
        // }

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
