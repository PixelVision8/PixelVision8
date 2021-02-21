
using System;
using System.Linq;
using NUnit.Framework;

namespace PixelVision8.Player
{
   
    public partial class PixelDataUtilitiesTest
    {

        // private PixelData pixelData = new PixelData(4, 4);

        private int[] _pixelMap = new[]
        {
            00, 01, 02, 03,
            04, 05, 06, 07,
            08, 09, 10, 11,
            12, 13, 14, 15
        };
        
        #region Get Pixels

        [Test]
        public void GetAllPixelsTest()
        {
            var pixelData = new PixelData(4, 4);

            CollectionAssert.AreEqual(Enumerable.Repeat(0, 16).ToArray(), Utilities.GetPixels(pixelData));
        }
        
        [Test]
        public void GetTopLeftPixelsTest()
        {

            var pixelData = new PixelData(4, 4);
            
            for (int i = 0; i < pixelData.Total; i++)
            {
                pixelData[i] = _pixelMap[i];
            }

            var pixels = new int[]
            {
                00, 01,
                04, 05,
            };
            
            CollectionAssert.AreEqual(pixels, Utilities.GetPixels(pixelData, 0, 0, 2, 2));
        }
        
        [Test]
        public void GetTopRightPixelsTest()
        {

            var pixelData = new PixelData(4, 4);

            for (int i = 0; i < pixelData.Total; i++)
            {
                pixelData[i] = _pixelMap[i];
            }

            var pixels = new int[]
            {
                02, 03,
                06, 07,
            };
            
            CollectionAssert.AreEqual(pixels, Utilities.GetPixels(pixelData, 2, 0, 2, 2));
        }
        
        [Test]
        public void GetBottomRightPixelsTest()
        {

            var pixelData = new PixelData(4, 4);

            for (int i = 0; i < pixelData.Total; i++)
            {
                pixelData[i] = _pixelMap[i];
            }

            var pixels = new int[]
            {
                10, 11,
                14, 15
            };
            
            CollectionAssert.AreEqual(pixels, Utilities.GetPixels(pixelData, 2, 2, 2, 2));
        }
        
        [Test]
        public void GetBottomLeftPixelsTest()
        {
            var pixelData = new PixelData(4, 4);

            for (int i = 0; i < pixelData.Total; i++)
            {
                pixelData[i] = _pixelMap[i];
            }

            var pixels = new int[]
            {
                08, 09,
                12, 13,
            };
            
            CollectionAssert.AreEqual(pixels, Utilities.GetPixels(pixelData, 0, 2, 2, 2));
        }
        
        [Test]
        public void GetOutOfBoundsTopLeftPixelsTest()
        {
            var pixelData = new PixelData(4, 4);

            for (int i = 0; i < pixelData.Total; i++)
            {
                pixelData[i] = _pixelMap[i];
            }

            var pixels = new int[]
            {
                00, 01,
                04, 05,
            };
            
            // Example
            // XX, XX, XX            
            // XX, 00, 01, --, --,
            // XX, 04, 05, --, --,
            //     --, --, --, --,
            //     --, --, --, --

            CollectionAssert.AreEqual(pixels, Utilities.GetPixels(pixelData, -1, -1, 3, 3));
        }
        
        [Test]
        public void GetOutOfBoundsTopRightPixelsTest()
        {
            var pixelData = new PixelData(4, 4);

            for (int i = 0; i < pixelData.Total; i++)
            {
                pixelData[i] = _pixelMap[i];
            }

            var pixels = new int[]
            {
                02, 03,
                06, 07,
            };
            
            // Example
            //         XX, XX, XX
            // --, --, 02, 03, XX
            // --, --, 06, 07, XX
            // --, --, --, --,
            // --, --, --, --,

            CollectionAssert.AreEqual(pixels, Utilities.GetPixels(pixelData, 2, -1, 3, 3));
        }
        
        [Test]
        public void GetOutOfBoundsBottomRightPixelsTest()
        {
            var pixelData = new PixelData(4, 4);

            for (int i = 0; i < pixelData.Total; i++)
            {
                pixelData[i] = _pixelMap[i];
            }

            var pixels = new int[]
            {
                10, 11,
                14, 15
            };
            
            // Example
            // --, --, --, --,
            // --, --, --, --,
            // --, --, 10, 11, XX
            // --, --, 14, 15, XX
            //         XX, XX, XX
            
            CollectionAssert.AreEqual(pixels, Utilities.GetPixels(pixelData, 2, 2, 3, 3));
        }
        
        [Test]
        public void GetOutOfBoundsBottomLeftPixelsTest()
        {
            var pixelData = new PixelData(4, 4);

            for (int i = 0; i < pixelData.Total; i++)
            {
                pixelData[i] = _pixelMap[i];
            }

            var pixels = new int[]
            {
                08, 09,
                12, 13,
            };
            
            // Example
            //     --, --, --, --,
            //     --, --, --, --,
            // XX, 08, 09, --, --,
            // XX, 12, 13, --, --,
            // XX, XX, XX
            
            CollectionAssert.AreEqual(pixels, Utilities.GetPixels(pixelData, -1, 2, 3, 3));
        }
        #endregion


        #region Resize

        
        [Test]
        public void ResizePixelsTest()
        {
            var pixelData = new PixelData(4, 4);

            Utilities.Resize(pixelData, 8, 8);
            
            Assert.AreEqual(8, pixelData.Width);
            Assert.AreEqual(8, pixelData.Height);
            Assert.AreEqual(64, pixelData.Total);
            CollectionAssert.AreEqual(Enumerable.Repeat(-1, 64).ToArray(), pixelData.Pixels);
        }
        
        [Test]
        public void ResizeClearPixelsTest()
        {
            var pixelData = new PixelData(4, 4);

            for (int i = 0; i < pixelData.Total; i++)
            {
                pixelData[i] = _pixelMap[i];
            }
            
            Utilities.Resize(pixelData, 8, 8);
            
            CollectionAssert.AreEqual(Enumerable.Repeat(-1, 64).ToArray(), pixelData.Pixels);

        }
        
        [Test]
        public void InvalidResizePixelsTest()
        {
            var pixelData = new PixelData(4, 4);

            Utilities.Resize(pixelData,-1, -1);
            Assert.AreEqual(1, pixelData.Width);
            Assert.AreEqual(1, pixelData.Height);
            Assert.AreEqual(1, pixelData.Total);
        }
        
        [Test]
        public void InvalidWidthResizePixelsTest()
        {
            var pixelData = new PixelData(4, 4);

            Utilities.Resize(pixelData,-1, 8);
            Assert.AreEqual(1, pixelData.Width);
            Assert.AreEqual(8, pixelData.Height);
            Assert.AreEqual(8, pixelData.Total);
        }
        
        [Test]
        public void InvalidHeightResizePixelsTest()
        {
            var pixelData = new PixelData(4, 4);

            Utilities.Resize(pixelData,8, -1);
            Assert.AreEqual(8, pixelData.Width);
            Assert.AreEqual(1, pixelData.Height);
            Assert.AreEqual(8, pixelData.Total);
        }
        
        [Test]
        public void OutOfBoundsResizePixelsTest()
        {
            var pixelData = new PixelData(4, 4);

            Utilities.Resize(pixelData,10000, 10000);
            Assert.AreEqual(2048, pixelData.Width);
            Assert.AreEqual(2048, pixelData.Height);
            Assert.AreEqual(4194304, pixelData.Total);
        }
        
        [Test]
        public void OutOfBoundsWidthResizePixelsTest()
        {
            var pixelData = new PixelData(4, 4);

            Utilities.Resize(pixelData,10000, 8);
            Assert.AreEqual(2048, pixelData.Width);
            Assert.AreEqual(8, pixelData.Height);
            Assert.AreEqual(16384, pixelData.Total);
        }
        
        [Test]
        public void OutOfBoundsHeightResizePixelsTest()
        {
            var pixelData = new PixelData(4, 4);

            Utilities.Resize(pixelData, 8, 10000);
            Assert.AreEqual(8, pixelData.Width); 
            Assert.AreEqual(2048, pixelData.Height);
            Assert.AreEqual(16384, pixelData.Total);
        }

        #endregion

        // #region Flip
        //
        // [Test]
        // public void HorizontalFlipPixelsTest()
        // {
        //     
        //     var pixels = new int[_pixelMap.Length];
        //     
        //     Array.Copy(_pixelMap, pixels, _pixelMap.Length);
        //     
        //     Utilities.FlipPixelData(ref pixels, 4, 4, true, false);
        //     
        //     CollectionAssert.AreEqual(_pixelMapFlipH, pixels);
        // }
        //
        // [Test]
        // public void VerticalFlipPixelsTest()
        // {
        //     
        //     var pixels = new int[_pixelMap.Length];
        //     
        //     Array.Copy(_pixelMap, pixels, _pixelMap.Length);
        //     
        //     Utilities.FlipPixelData(ref pixels, 4, 4, false, true);
        //     
        //     CollectionAssert.AreEqual(_pixelMapFlipV, pixels);
        // }
        //
        // [Test]
        // public void FullFlipPixelsTest()
        // {
        //     
        //     var pixels = new int[_pixelMap.Length];
        //     
        //     Array.Copy(_pixelMap, pixels, _pixelMap.Length);
        //     
        //     Utilities.FlipPixelData(ref pixels, 4, 4, true, true);
        //     
        //     CollectionAssert.AreEqual(_pixelMapFlipHV, pixels);
        // }
        //
        // #endregion

        // #region Set Pixels
        //
        // [Test]
        // public void SetTopLeftPixelsTest()
        // {
        //     var pixelData = new PixelData(4, 4);
        //
        //     var pixels = new int[]
        //     {
        //         00, 01,
        //         04, 05,
        //     };
        //     
        //     var results = new[]
        //     {
        //         00, 01, 00, 00,
        //         04, 05, 00, 00,
        //         00, 00, 00, 00,
        //         00, 00, 00, 00
        //     };
        //     
        //     Utilities.SetPixels(pixels, 0, 0, 2, 2, pixelData);
        //
        //     CollectionAssert.AreEqual(results, Utilities.GetPixels(pixelData));
        // }
        //
        // [Test]
        // public void SetTopRightPixelsTest()
        // {
        //     var pixelData = new PixelData(4, 4);
        //
        //     var pixels = new int[]
        //     {
        //         02, 03,
        //         06, 07,
        //     };
        //     
        //     var results = new[]
        //     {
        //         00, 00, 02, 03,
        //         00, 00, 06, 07,
        //         00, 00, 00, 00,
        //         00, 00, 00, 00
        //     };
        //     
        //     Utilities.SetPixels(pixels, 2, 0, 2, 2, pixelData);
        //
        //     CollectionAssert.AreEqual(results, Utilities.GetPixels(pixelData));
        //    
        // }
        //
        // [Test]
        // public void SetBottomRightPixelsTest()
        // {
        //     var pixelData = new PixelData(4, 4);
        //
        //     var pixels = new int[]
        //     {
        //         10, 11,
        //         14, 15
        //     };
        //     
        //     var results = new[]
        //     {
        //         00, 00, 00, 00,
        //         00, 00, 00, 00,
        //         00, 00, 10, 11,
        //         00, 00, 14, 15            
        //     };
        //     
        //     Utilities.SetPixels(pixels, 2, 2, 2, 2, pixelData);
        //
        //     CollectionAssert.AreEqual(results, Utilities.GetPixels(pixelData));
        //     
        // }
        //
        // [Test]
        // public void SetBottomLeftPixelsTest()
        // {
        //     var pixelData = new PixelData(4, 4);
        //
        //     var pixels = new int[]
        //     {
        //         08, 09,
        //         12, 13,
        //     };
        //     
        //     var results = new[]
        //     {
        //         00, 00, 00, 00,
        //         00, 00, 00, 00,
        //         08, 09, 00, 00,
        //         12, 13, 00, 00
        //     };
        //     
        //     Utilities.SetPixels(pixels, 0, 2, 2, 2, pixelData);
        //
        //     CollectionAssert.AreEqual(results, Utilities.GetPixels(pixelData));
        //     
        // }
        //
        // [Test]
        // public void SetOutOfBoundsTopLeftPixelsTest()
        // {
        //     var pixelData = new PixelData(4, 4);
        //
        //     var pixels = new int[]
        //     {
        //         00, 01, 02,
        //         03, 04, 05,
        //         06, 07, 08
        //     };
        //     
        //     var results = new[]
        //     {
        //         05, 00, 00, 00,
        //         08, 00, 00, 00,
        //         00, 00, 00, 00,
        //         00, 00, 00, 00
        //     };
        //     
        //     // Example
        //     // 00, 01, 02            
        //     // 03, 04, 05, 00, 00,
        //     // 06, 07, 08, 00, 00,
        //     //     00, 00, 00, 00,
        //     //     00, 00, 00, 00
        //     
        //     // Utilities.SetPixels(pixels, 0, 0, 3, 3, pixelData);
        //     
        //     Utilities.SetPixels(pixels, -2, -1, 3, 3, pixelData);
        //     
        //     // Utilities.SetPixels(pixels, 2, -1, 3, 3, pixelData);
        //     
        //     // Utilities.SetPixels(pixels, 8, 20, 3, 3, pixelData);
        //
        //     // var tmpPD = new PixelData();
        //     // tmpPD.SetPixels(pixels, 2, 2);
        //     //
        //     // Utilities.MergePixels(tmpPD, 0, 0, 2, 2, pixelData, -1, -1);
        //     CollectionAssert.AreEqual(results, Utilities.GetPixels(pixelData));
        // }
        //
        // [Test]
        // public void SetOutOfBoundsTopRightPixelsTest()
        // {
        //     var pixelData = new PixelData(4, 4);
        //
        //     var pixels = new int[]
        //     {
        //         02, 03,
        //         06, 07,
        //     };
        //     
        //     var results = new[]
        //     {
        //         00, 00, 00, 06,
        //         00, 00, 00, 00,
        //         00, 00, 00, 00,
        //         00, 00, 00, 00
        //     };
        //     
        //     // Example
        //     //             02, 03
        //     // 00, 00, 00, 06, 07
        //     // 00, 00, 00, 00, 
        //     // 00, 00, 00, 00,
        //     // 00, 00, 00, 00,
        //     
        //     Utilities.SetPixels(pixels, 3, -1, 2, 2, pixelData);
        //
        //     CollectionAssert.AreEqual(results, Utilities.GetPixels(pixelData));
        //    
        // }
        //
        // [Test]
        // public void SetOutOfBoundsBottomRightPixelsTest()
        // {
        //     var pixelData = new PixelData(4, 4);
        //
        //     var pixels = new int[]
        //     {
        //         10, 11,
        //         14, 15
        //     };
        //     
        //     var results = new[]
        //     {
        //         00, 00, 00, 00,
        //         00, 00, 00, 00,
        //         00, 00, 00, 00,
        //         00, 00, 00, 10
        //     };
        //     
        //     // Example
        //     // 00, 00, 00, 00,
        //     // 00, 00, 00, 00,
        //     // 00, 00, 00, 00, 
        //     // 00, 00, 00, 10, 11
        //     //             14, 15
        //     
        //     Utilities.SetPixels(pixels, 3, 3, 2, 2, pixelData);
        //
        //     CollectionAssert.AreEqual(results, Utilities.GetPixels(pixelData));
        //     
        // }
        //
        // [Test]
        // public void SetOutOfBoundsBottomLeftPixelsTest()
        // {
        //     var pixelData = new PixelData(4, 4);
        //
        //     var pixels = new int[]
        //     {
        //         08, 09,
        //         12, 13,
        //     };
        //     
        //     var results = new[]
        //     {
        //         00, 00, 00, 00,
        //         00, 00, 00, 00,
        //         00, 00, 00, 00,
        //         09, 00, 00, 00
        //     };
        //     
        //     // Example
        //     //     00, 00, 00, 00,
        //     //     00, 00, 00, 00,
        //     //     00, 00, 00, 00,
        //     // 08, 09, 00, 00, 00,
        //     // 12, 13
        //     
        //     Utilities.SetPixels(pixels, -1, 3, 2, 2, pixelData);
        //
        //     CollectionAssert.AreEqual(results, Utilities.GetPixels(pixelData));
        //     
        // }
        //
        // #endregion

        [Test]
        public void MergeDataTest()
        {

            // int i = 6;
            for (var i = 0; i < _testData.Length; i++)
            {
                Console.WriteLine("Merge Data Set Test " + i +"\n");
        
                var data = _testData[i];
        
                var finalSets = new int[][]
                {
                    data.FinalMerge,
                    data.FinalFlipHVMerge,
                    data.FinalOffsetMerge,
                };

                var labels = new string[] {"Merge", "FlipHV", "Color Offset"};
                
                for (int j = 0; j < finalSets.Length; j++)
                {
                    var finalSet = finalSets[j];

                    var srcPixelData = new PixelData();
                    srcPixelData.SetPixels(_srcData, _srcRect.Width, _srcRect.Height);
                
                    var destPixelData = new PixelData();
                    destPixelData.SetPixels(_destData, _destRect.Width, _destRect.Height);
        
                    var flipH = j == 1;
                    var flipV = j == 1;
                    var color = j == 2 ? data.ColorOffset : 0; 
                    Console.WriteLine("   Test " + labels[j] +" - H {0} V {1}\n", flipH, flipV);
                    
                    
                    Utilities.MergePixels(srcPixelData, data.SampleRect.X, data.SampleRect.Y, data.SampleRect.Width, data.SampleRect.Height, destPixelData, data.DestPos.X, data.DestPos.Y, flipH, flipV, color);
                    
                    CollectionAssert.AreEqual(destPixelData.Pixels, finalSet);
                }
                
            }
            
        }
        
        [Test]
        public void SetDataTest()
        {

            // int i = 1;
            for (var i = 0; i < _testData.Length; i++)
            {
                Console.WriteLine("Merge Data Set Test " + i +"\n");
        
                var data = _testData[i];
        
                var finalSet = data.FinalSet;

                var srcPixelData = new PixelData();
                srcPixelData.SetPixels(_srcData, _srcRect.Width, _srcRect.Height);
            
                var destPixelData = new PixelData();
                destPixelData.SetPixels(_destData, _destRect.Width, _destRect.Height);
    
                Console.WriteLine("src  " + string.Join(",", srcPixelData.Pixels));
                Console.WriteLine("dest " + string.Join(",", destPixelData.Pixels));
                
                Utilities.SetPixels(srcPixelData.Pixels, data.DestPos.X, data.DestPos.Y, data.SampleRect.Width, data.SampleRect.Height, destPixelData);
                
                Console.WriteLine("final " + string.Join(",", finalSet));
                Console.WriteLine("dest  " + string.Join(",", destPixelData.Pixels));

                CollectionAssert.AreEqual(destPixelData.Pixels, finalSet);
                
            }
            
        }
        
    }

    
}
