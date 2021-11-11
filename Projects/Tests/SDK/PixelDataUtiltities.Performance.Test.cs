using System;
using System.Diagnostics;
using NUnit.Framework;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace PixelVision8.Player
{
    public partial class PixelDataUtilitiesTest
    {
        
        #region Performance Tests

        [Test]
        public void MergePerformanceTest()
        {
            Random r = new Random();
            int Iterations = 1000000;


            var srcPixelData = new PixelData(8,8);
            for (int i = 0; i < srcPixelData.Total; i++)
            {
                srcPixelData[i] = r.Next(-1, 16);
            }
            
            var destPixelData = new PixelData(256, 240);
            
            var destPixels = Enumerable.Range(0, destPixelData.Total-1).ToArray();//, destPixelData.Width, destPixelData.Height);
            
            var srcRect = new Rectangle(0, 0, 8, 8);
            
            var destPos = new Point();

            var average = new List<long>();
            
            Stopwatch sw = Stopwatch.StartNew();

            // Test 1
            sw = Stopwatch.StartNew();
            destPixelData.SetPixels(destPixels, destPixelData.Width, destPixelData.Height);
            for (int i=0; i < Iterations; i++)
            {
                Utilities.MergePixels(srcPixelData, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, destPixelData, destPos.X, destPos.Y);                
            }
            sw.Stop();
            average.Add(sw.ElapsedMilliseconds);
            Console.WriteLine("Merge H: false, V: false, offset: 0, ignore: false, speed: {0}ms", average.Last());

            // Test 2
            sw = Stopwatch.StartNew();
            destPixelData.SetPixels(destPixels, destPixelData.Width, destPixelData.Height);

            for (int i=0; i < Iterations; i++)
            {
                Utilities.MergePixels(srcPixelData, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, destPixelData, destPos.X, destPos.Y, false, false , 10);                
            }
            sw.Stop();
            average.Add(sw.ElapsedMilliseconds);
            Console.WriteLine("Merge H: false, V: false, offset: 100, ignore: false, speed: {0}ms", average.Last());

            // Test 3
            sw = Stopwatch.StartNew();
            destPixelData.SetPixels(destPixels, destPixelData.Width, destPixelData.Height);

            for (int i=0; i < Iterations; i++)
            {
                Utilities.MergePixels(srcPixelData, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, destPixelData, destPos.X, destPos.Y, true, false , 0);                
            }
            sw.Stop();
            average.Add(sw.ElapsedMilliseconds);
            Console.WriteLine("Merge H: true, V: false, offset: 0, ignore: false, speed: {0}ms", average.Last());

            // Test 4
            sw = Stopwatch.StartNew();
            destPixelData.SetPixels(destPixels, destPixelData.Width, destPixelData.Height);

            for (int i=0; i < Iterations; i++)
            {
                Utilities.MergePixels(srcPixelData, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, destPixelData, destPos.X, destPos.Y, false, true , 0);                
            }
            sw.Stop();
            average.Add(sw.ElapsedMilliseconds);
            Console.WriteLine("Merge H: false, V: true, offset: 0, ignore: false, speed: {0}ms", average.Last());

            // Test 3
            sw = Stopwatch.StartNew();
            destPixelData.SetPixels(destPixels, destPixelData.Width, destPixelData.Height);

            for (int i=0; i < Iterations; i++)
            {
                Utilities.MergePixels(srcPixelData, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, destPixelData, destPos.X, destPos.Y, true, true , 0);                
            }
            sw.Stop();
            average.Add(sw.ElapsedMilliseconds);
            Console.WriteLine("Merge H: true, V: true, offset: 0, ignore: false, speed: {0}ms", average.Last());

            // Test 4
            sw = Stopwatch.StartNew();
            destPixelData.SetPixels(destPixels, destPixelData.Width, destPixelData.Height);

            for (int i=0; i < Iterations; i++)
            {
                Utilities.MergePixels(srcPixelData, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, destPixelData, destPos.X, destPos.Y, true, true , 100);                
            }
            sw.Stop();
            average.Add(sw.ElapsedMilliseconds);
            Console.WriteLine("Merge H: true, V: true, offset: 100, ignore: false, speed: {0}ms", average.Last());

            
            // Test 5
            sw = Stopwatch.StartNew();
            destPixelData.SetPixels(destPixels, destPixelData.Width, destPixelData.Height);

            for (int i=0; i < Iterations; i++)
            {
                Utilities.MergePixels(srcPixelData, srcRect.X, srcRect.Y, srcRect.Width, srcRect.Height, destPixelData, destPos.X, destPos.Y, true, true , 100, false);                
            }
            sw.Stop();
            average.Add(sw.ElapsedMilliseconds);
            Console.WriteLine("Merge H: true, V: true, offset: 100, ignore: true, speed: {0}ms", average.Last());
            
            Console.WriteLine("Merge average: {0}ms", average.Average());
            
        }

        [Test]
        public void SetPerformanceTest()
        {
            Random r = new Random();
            int Iterations = 1000000;


            var srcPixelData = new PixelData(8, 8);

            var pixels = new int[8 * 8];
            
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = r.Next(-1, 16);
            }

            var average = new List<long>();

            Stopwatch sw = Stopwatch.StartNew();

            // Test 1
            sw = Stopwatch.StartNew();
            // Utilities.Clear(destPixelData);
            for (int i = 0; i < Iterations; i++)
            {
                srcPixelData.SetPixels(pixels);
            }

            sw.Stop();
            average.Add(sw.ElapsedMilliseconds);
            Console.WriteLine("Set all speed: {0}ms", average.Last());
            
            
            // Test 2
            sw = Stopwatch.StartNew();
            // Utilities.Clear(destPixelData);
            for (int i = 0; i < Iterations; i++)
            {
                Utilities.SetPixels(pixels, 0, 0, srcPixelData.Width, srcPixelData.Height, srcPixelData);
            }

            sw.Stop();
            average.Add(sw.ElapsedMilliseconds);
            Console.WriteLine("Set area speed: {0}ms", average.Last());
            
            // Test 3
            sw = Stopwatch.StartNew();

            var pxD = new PixelData(8, 8);
            pxD.SetPixels(pixels);
            
            for (int i = 0; i < Iterations; i++)
            {
                Utilities.MergePixels(pxD, 0, 0, pxD.Width, pxD.Height, srcPixelData, 0, 0);
            }

            sw.Stop();
            Console.WriteLine("Merge speed: {0}ms", sw.ElapsedMilliseconds);
        }

        #endregion
    }
}