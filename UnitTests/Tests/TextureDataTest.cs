using System.Linq;
using NUnit.Framework;
using PixelVision8.Engine;

namespace PixelVision8Tests
{
    public class TextureDataTest
    {
        private int maxSize = 2048;
        private int maskID = 0;

        [Test]
        public void DefaultSizeTest()
        {
            var td = new TextureData();

            Assert.AreEqual(td.width, 1);
            Assert.AreEqual(td.height, 1);
            Assert.AreEqual(td.pixels.Length, 1);
        }

        [Test]
        public void DefaultNegativeSizeTest()
        {
            var td = new TextureData(-100, -5);

            Assert.AreEqual(td.width, 1);
            Assert.AreEqual(td.height, 1);
            Assert.AreEqual(td.pixels.Length, 1);
        }

        [Test]
        public void DefaultMaxSizeTest()
        {
            var td = new TextureData(5000, 5000);

            Assert.AreEqual(td.width, maxSize);
            Assert.AreEqual(td.height, maxSize);
            Assert.AreEqual(td.pixels.Length, maxSize * maxSize);
        }

        [Test]
        public void DefaultResizeTest()
        {
            var td = new TextureData();
            td.Resize(160, 144);

            Assert.AreEqual(td.width, 160);
            Assert.AreEqual(td.height, 144);
            Assert.AreEqual(td.pixels.Length, 23040);
        }

        [Test]
        public void DefaultMaxResizeTest()
        {
            var td = new TextureData();
            td.Resize(5000, 5000);

            Assert.AreEqual(td.width, maxSize);
            Assert.AreEqual(td.height, maxSize);
            Assert.AreEqual(td.pixels.Length, maxSize * maxSize);
        }

        [Test]
        public void DefaultMinResizeTest()
        {
            var td = new TextureData();
            td.Resize(-100, -100);

            Assert.AreEqual(td.width, 1);
            Assert.AreEqual(td.height, 1);
            Assert.AreEqual(td.pixels.Length, 1);
        }

        [Test]
        public void DefaultPixelDataTest()
        {
            var td = new TextureData(8, 8);

            var pixelData = td.pixels;

            CollectionAssert.AreEqual(pixelData, zeroedPixelData);
        }

        [Test]
        public void DefaultPixelDataResizeTest()
        {
            var td = new TextureData(128, 128);
            td.Resize(8,8);

            CollectionAssert.AreEqual(td.pixels, zeroedPixelData);
            Assert.AreEqual(td.invalid, true);
        }

        [Test]
        public void CustomPixelDataResizeTest()
        {
            var td = new TextureData(128, 128);
            td.SetPixels(Enumerable.Range(0, td.pixels.Length).ToArray());
            
            td.Resize(8, 8);

            CollectionAssert.AreEqual(td.pixels, zeroedPixelData);
            Assert.AreEqual(td.invalid, true);
        }

        [Test]
        public void CropInBoundsTest()
        {
            var td = new TextureData(128, 128);
            td.SetPixels(Enumerable.Range(0, td.pixels.Length).ToArray());

            td.Crop(0, 0, 8, 8);

            var result = new int[]
            {
                000,001,002,003,004,005,006,007,
                128,129,130,131,132,133,134,135,
                256,257,258,259,260,261,262,263,
                384,385,386,387,388,389,390,391,
                512,513,514,515,516,517,518,519,
                640,641,642,643,644,645,646,647,
                768,769,770,771,772,773,774,775,
                896,897,898,899,900,901,902,903
            };

            CollectionAssert.AreEqual(td.pixels, result);
            Assert.AreEqual(td.invalid, true);
        }

        [Test]
        public void CropInBoundsOffsetTest()
        {
            var td = new TextureData(128, 128);
            td.SetPixels(Enumerable.Range(0, td.pixels.Length).ToArray());

            td.Crop(3, 3, 4, 4);

            var result = new int[]
            {
                387,388,389,390,
                515,516,517,518,
                643,644,645,646,
                771,772,773,774 
            };

            CollectionAssert.AreEqual(td.pixels, result);
            Assert.AreEqual(td.invalid, true);
        }

        [Test]
        public void CropOutBoundsTest()
        {
            var td = new TextureData(128, 128);
            td.SetPixels(Enumerable.Range(0, td.pixels.Length).ToArray());

            td.Crop(-4, -4, 8, 8);

            var result = new int[]
            {
                000,001,002,003,
                128,129,130,131,
                256,257,258,259,
                384,385,386,387,
            };

            CollectionAssert.AreEqual(td.pixels, result);
            Assert.AreEqual(td.invalid, true);
        }

        [Test]
        public void DefaultPixelDataSameResizeTest()
        {
            var td = new TextureData(8, 8);
            td.Resize(8, 8);

            Assert.AreEqual(td.pixels.Length, 64);
        }

        [Test]
        public void FullClearPixelDataTest()
        {
            var td = new TextureData(8, 8);
            td.Clear();

            var pixelData = td.pixels;

            var result = new int[8 * 8];

            for (var i = 0; i < result.Length; i++)
            {
                result[i] = maskID;
            }

            CollectionAssert.AreEqual(pixelData, result);
        }

        [Test]
        public void FullClearAltColorPixelDataTest()
        {
            var td = new TextureData(8, 8);
            td.Clear(1);

            var pixelData = td.pixels;

            CollectionAssert.AreEqual(pixelData, Enumerable.Repeat(1, 64).ToArray());
        }

        [Test]
        public void PartialInBoundsClearAltColorPixelDataTest()
        {
            var td = new TextureData(8, 8);
            td.Clear(0, 1, 1, 6, 6);

            var pixelData = td.pixels;

            var result = new []
            {
                -1, -1, -1, -1, -1, -1, -1, -1,
                -1, 00, 00, 00, 00, 00, 00, -1,
                -1, 00, 00, 00, 00, 00, 00, -1,
                -1, 00, 00, 00, 00, 00, 00, -1,
                -1, 00, 00, 00, 00, 00, 00, -1,
                -1, 00, 00, 00, 00, 00, 00, -1,
                -1, 00, 00, 00, 00, 00, 00, -1,
                -1, -1, -1, -1, -1, -1, -1, -1,
            };

            CollectionAssert.AreEqual(pixelData, result);
        }

        [Test]
        public void PartialOutBoundsClearAltColorPixelDataTest()
        {
            var td = new TextureData(8, 8);
            td.Clear(0, 3, 3, 8, 8);
        
            var pixelData = td.pixels;
        
            var result = new[]
            {
                -1, -1, -1, -1, -1, -1, -1, -1,
                -1, -1, -1, -1, -1, -1, -1, -1,
                -1, -1, -1, -1, -1, -1, -1, -1,
                -1, -1, -1, 00, 00, 00, 00, 00,
                -1, -1, -1, 00, 00, 00, 00, 00,
                -1, -1, -1, 00, 00, 00, 00, 00,
                -1, -1, -1, 00, 00, 00, 00, 00,
                -1, -1, -1, 00, 00, 00, 00, 00,
            };
        
            CollectionAssert.AreEqual(pixelData, result);
        }

        [Test]
        public void SetFullPixelDataTest()
        {
            var td = new TextureData(8, 8);
            
            td.SetPixels(zeroedPixelData);

            var pixelData = td.pixels;

            CollectionAssert.AreEqual(pixelData, zeroedPixelData);
        }

        [Test]
        public void SetPixelInBoundsTest()
        {
            var td = new TextureData(8, 8);

            td.SetPixel(5,2, 5);
            td.SetPixel(0, 0, 5);

            var result = new[]
            {
                5,-1,-1,-1,-1,-1,-1,-1,
                -1,-1,-1,-1,-1,-1,-1,-1,
                -1,-1,-1,-1,-1, 5,-1,-1,
                -1,-1,-1,-1,-1,-1,-1,-1,
                -1,-1,-1,-1,-1,-1,-1,-1,
                -1,-1,-1,-1,-1,-1,-1,-1,
                -1,-1,-1,-1,-1,-1,-1,-1,
                -1,-1,-1,-1,-1,-1,-1,-1,
            };

            td.SetPixels(result);

            var pixelData = td.pixels;

            CollectionAssert.AreEqual(pixelData, result);
        }

        [Test]
        public void SetPixelOutBoundsTest()
        {
            var td = new TextureData(8, 8);

            td.SetPixel(-1, -5, 5);
            td.SetPixel(12, 10, 5);
            td.SetPixel(8, 8, 5);

            var result = new[]
            {
                -1,-1,-1,-1,-1,-1,-1,-1,
                -1,-1,-1,-1,-1,-1,-1,-1,
                -1,-1,-1,-1,-1,-1,-1,-1,
                -1,-1,-1,-1,-1,-1,-1,-1,
                -1,-1,-1,-1,-1,-1,-1,-1,
                -1,-1,-1,-1,-1,-1,-1,-1,
                -1,-1,-1,-1,-1,-1,-1,-1,
                -1,-1,-1,-1,-1,-1,-1,-1,
            };

            td.SetPixels(result);

            var pixelData = td.pixels;

            CollectionAssert.AreEqual(pixelData, result);
        }

        [Test]
        public void InvalidCreateTest()
        {
            var td = new TextureData(8, 8);

            Assert.AreEqual(td.invalid, true);
            
        }

        [Test]
        public void InvalidResetTest()
        {
            var td = new TextureData(8, 8);
            
            td.ResetValidation();

            Assert.AreEqual(td.invalid, false);

        }

        [Test]
        public void InvalidSetPixelInBoundsTest()
        {
            var td = new TextureData(8, 8);
            td.ResetValidation();

            // setting the same pixel shouldn't trigger invalidation
            td.SetPixel(0, 0, 1);

            Assert.AreEqual(td.invalid, true);

        }

        [Test]
        public void InvalidSetSamePixelInBoundsTest()
        {
            var td = new TextureData(8, 8);
            td.ResetValidation();

            // setting the same pixel shouldn't trigger invalidation
            td.SetPixel(0, 0, 0);

            Assert.AreEqual(td.invalid, false);

        }

        [Test]
        public void InvalidSetPixelOutBoundsTest()
        {
            var td = new TextureData(8, 8);
            td.ResetValidation();
            
            // Out of bounds shouldn't change invalidation
            td.SetPixel(10, 10, -1);
            td.SetPixel(-5, -2, -1);

            Assert.AreEqual(td.invalid, false);

        }

        [Test]
        public void GetPixelInBoundsTest()
        {
            var td = new TextureData(8, 8);
            
            td.SetPixel(5, 2, 5);
            
            Assert.AreEqual(td.GetPixel(5, 2), 5);

        }

        [Test]
        public void GetPixelOutBoundsTest()
        {
            var td = new TextureData(8, 8);

            td.Clear(0);

            td.SetPixel(-1, -5, 5);

            Assert.AreEqual(td.GetPixel(-1, -5), -1);
            Assert.AreEqual(td.GetPixel(0, 8), -1);

        }

        [Test]
        public void SetAllPixelDataTest()
        {
            var td = new TextureData(8, 8);
            
            var pixelData = td.pixels;

            td.SetPixels(zeroedPixelData);

            CollectionAssert.AreEqual(pixelData, zeroedPixelData);
        }

        [Test]
        public void SetInBoundsTopLeftPixelDataTest()
        {
            var td = new TextureData(8, 8);
            
            var pixelData = td.pixels;

            var src = new[]
            {
                0, 1, 2,
                3, 4, 5,
                6, 7, 8,
            };

            var result = new[]
            {
                00,01,02,00,00,00,00,00,
                03,04,05,00,00,00,00,00,
                06,07,08,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
            };

            td.SetPixels(0, 0, 3, 3, src);

            CollectionAssert.AreEqual(pixelData, result);
        }

        [Test]
        public void SetInBoundsTopRightPixelDataTest()
        {
            var td = new TextureData(8, 8);

            var pixelData = td.pixels;

            var src = new[]
            {
                00,01,02,
                03,04,05,
                06,07,08,
            };

            var result = new[]
            {
                00,00,00,00,00,00,01,02,
                00,00,00,00,00,03,04,05,
                00,00,00,00,00,06,07,08,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
            };

            td.SetPixels(5, 0, 3, 3, src);

            CollectionAssert.AreEqual(pixelData, result);
        }

        [Test]
        public void SetInBoundsBottomRightPixelDataTest()
        {
            var td = new TextureData(8, 8);

            var pixelData = td.pixels;

            var src = new[]
            {
                00,01,02,
                03,04,05,
                06,07,08,
            };

            var result = new[]
            {
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,01,02,
                00,00,00,00,00,03,04,05,
                00,00,00,00,00,06,07,08,
            };

            td.SetPixels(5, 5, 3, 3, src);

            CollectionAssert.AreEqual(pixelData, result);
        }

        [Test]
        public void SetInBoundsBottomLeftPixelDataTest()
        {
            var td = new TextureData(8, 8);

            var pixelData = td.pixels;

            var src = new[]
            {
                00,01,02,
                03,04,05,
                06,07,08,
            };

            var result = new[]
            {
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,01,02,00,00,00,00,00,
                03,04,05,00,00,00,00,00,
                06,07,08,00,00,00,00,00,
            };

            td.SetPixels(0, 5, 3, 3, src);

            CollectionAssert.AreEqual(pixelData, result);
        }

        [Test]
        public void SetInBoundsMiddlePixelDataTest()
        {
            var td = new TextureData(8, 8);

            var pixelData = td.pixels;

            var src = new[]
            {
                00,01,02,
                03,04,05,
                06,07,08,
            };

            var result = new[]
            {
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,01,02,00,00,
                00,00,00,03,04,05,00,00,
                00,00,00,06,07,08,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
            };

            td.SetPixels(3, 2, 3, 3, src);

            CollectionAssert.AreEqual(pixelData, result);
        }

        [Test]
        public void SetAllPixelOutBoundsDataTest()
        {
            var td = new TextureData(8, 8);
            // td.Clear(0, 1, 1, 6, 6);

            var pixelData = td.pixels;

            var src = new[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            };

            td.SetPixels(src);

            CollectionAssert.AreEqual(pixelData, zeroedPixelData);
        }

        [Test]
        public void SetOutBoundsTopLeftPixelDataTest()
        {
            var td = new TextureData(8, 8);

            var pixelData = td.pixels;

            var src = new[]
            {
                0, 1, 2,
                3, 4, 5,
                6, 7, 8,
            };

            var result = new[]
            {
                
                04,05,00,00,00,00,00,00,
                07,08,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
            };

            td.SetPixels(-1, -1, 3, 3, src);

            // BlockSave(src, 3, 3, pixelData, -1, -1, td.width, td.height);

            CollectionAssert.AreEqual(pixelData, result);
        }

        [Test]
        public void SetOutBoundsTopRightPixelDataTest()
        {
            var td = new TextureData(8, 8);

            var pixelData = td.pixels;

            var src = new[]
            {
                00,01,02,
                03,04,05,
                06,07,08,
            };

            var result = new[]
            {
                
                00,00,00,00,00,00,03,04,
                00,00,00,00,00,00,06,07,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
            };

            td.SetPixels(6, -1, 3, 3, src);

            // BlockSave(src, 3, 3, pixelData, 6, -1, td.width, td.height);

            CollectionAssert.AreEqual(pixelData, result);
        }

        [Test]
        public void SetOutBoundsBottomRightPixelDataTest()
        {
            var td = new TextureData(8, 8);

            var pixelData = td.pixels;

            var src = new[]
            {
                00,01,02,
                03,04,05,
                06,07,08,
            };

            var result = new[]
            {
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,01,
                00,00,00,00,00,00,03,04,
            };

            td.SetPixels(6, 6, 3, 3, src);

            // BlockSave(src, 3, 3, pixelData, 6, 6, td.width, td.height);

            CollectionAssert.AreEqual(pixelData, result);
        }

        [Test]
        public void SetOutBoundsBottomLeftPixelDataTest()
        {
            var td = new TextureData(8, 8);

            var pixelData = td.pixels;

            var src = new[]
            {
                00,01,02,
                03,04,05,
                06,07,08,
            };

            var result = new[]
            {
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                01,02,00,00,00,00,00,00,
                04,05,00,00,00,00,00,00,
            };

            td.SetPixels(-1, 6, 3, 3, src);

            // BlockSave(src, 3, 3, pixelData, -1, 6, td.width, td.height);

            CollectionAssert.AreEqual(pixelData, result);
        }

        // void BlockSave(int[] src, int srcW, int srcH, int[] dest, int destX, int destY, int destW, int destH)
        // {
        //
        //     var srcX = 0;
        //     var srcY = 0;
        //     var srcLength = srcW;
        //
        //     // Adjust X
        //     if (destX < 0)
        //     {
        //         srcX = -destX;
        //
        //         srcW -= srcX;
        //          
        //         // destW += destX; 
        //         destX = 0;
        //     }
        //
        //     if (destX + srcW > destW)
        //         srcW -= ((destX + srcW) - destW);
        //
        //     if (srcW <= 0) return;
        //
        //     // Adjust Y
        //     if (destY < 0)
        //     {
        //         srcY = -destY;
        //
        //         srcH -= srcY;
        //
        //         // destW += destX; 
        //         destY = 0;
        //     }
        //
        //     if (destY + srcH > destH)
        //         srcH -= ((destY + srcH) - destH);
        //
        //     if (srcH <= 0) return;
        //
        //     var row = 0;
        //     var startCol = 0;
        //     var endCol = 0;
        //     var destCol = 0;
        //
        //     for (row = 0; row < srcH; row++)
        //     {
        //         startCol = srcX + (row + srcY) * srcLength;
        //         destCol = destX + (row + destY) * destW;
        //
        //         Array.Copy(src, startCol, dest, destCol, srcW);
        //     }
        //
        // }

        [Test]
        public void SetOutBoundsMiddlePixelDataTest()
        {
            var td = new TextureData(8, 8);

            var pixelData = td.pixels;

            var src = new[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            };

            td.SetPixels(-1, -1, 10, 10, src);

            CollectionAssert.AreEqual(pixelData, zeroedPixelData);
        }

        [Test]
        public void GetAllPixelDataTest()
        {
            var td = new TextureData(8, 8);
            
            td.SetPixels(zeroedPixelData);

            CollectionAssert.AreEqual(td.GetPixels(), zeroedPixelData);
        }

        [Test]
        public void GetInBoundsTopLeftPixelDataTest()
        {
            var td = new TextureData(8, 8);

            var src = new[]
            {
                0, 1, 2,
                3, 4, 5,
                6, 7, 8,
            };

            var result = new[]
            {
                00,01,02,00,00,00,00,00,
                03,04,05,00,00,00,00,00,
                06,07,08,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
            };
            
            td.SetPixels(result);

            var pixelData = td.GetPixels(0, 0, 3, 3);

            CollectionAssert.AreEqual(pixelData, src);
        }

        [Test]
        public void GetInBoundsTopRightPixelDataTest()
        {
            var td = new TextureData(8, 8);

            var src = new[]
            {
                00,01,02,
                03,04,05,
                06,07,08,
            };

            var result = new[]
            {
                00,00,00,00,00,00,01,02,
                00,00,00,00,00,03,04,05,
                00,00,00,00,00,06,07,08,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
            };
            
            td.SetPixels(result);

            var pixelData = td.GetPixels(5, 0, 3, 3);

            CollectionAssert.AreEqual(pixelData, src);
        }

        [Test]
        public void GetInBoundsBottomRightPixelDataTest()
        {
            var td = new TextureData(8, 8);

            var src = new[]
            {
                00,01,02,
                03,04,05,
                06,07,08,
            };

            var result = new[]
            {
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,01,02,
                00,00,00,00,00,03,04,05,
                00,00,00,00,00,06,07,08,
            };

            td.SetPixels(result);

            var pixelData = td.GetPixels(5, 5, 3, 3);

            CollectionAssert.AreEqual(pixelData, src);
        }

        [Test]
        public void GetInBoundsBottomLeftPixelDataTest()
        {
            var td = new TextureData(8, 8);

            var src = new[]
            {
                00,01,02,
                03,04,05,
                06,07,08,
            };

            var result = new[]
            {
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,01,02,00,00,00,00,00,
                03,04,05,00,00,00,00,00,
                06,07,08,00,00,00,00,00,
            };

            td.SetPixels(result);

            var pixelData = td.GetPixels(0, 5, 3, 3);

            CollectionAssert.AreEqual(pixelData, src);
        }

        [Test]
        public void GetInBoundsMiddlePixelDataTest()
        {
            var td = new TextureData(8, 8);

            var src = new[]
            {
                00,01,02,
                03,04,05,
                06,07,08,
            };

            var result = new[]
            {
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,01,02,00,00,
                00,00,00,03,04,05,00,00,
                00,00,00,06,07,08,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
            };

            td.SetPixels(result);

            var pixelData = td.GetPixels(3, 2, 3, 3);

            CollectionAssert.AreEqual(pixelData, src);
        }

        [Test]
        public void GetAllPixelOutBoundsDataTest()
        {
            var td = new TextureData(8, 8);
            
            var pixelData = td.pixels;

            var src = new[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            };

            td.SetPixels(src);

            CollectionAssert.AreEqual(pixelData, zeroedPixelData);
        }

        // [Test]
        // public void GetOutBoundsTopLeftPixelDataTest()
        // {
        //     var td = new TextureData(8, 8);
        //
        //     var src = new[]
        //     {
        //         0, 1, 2,
        //         3, 4, 5,
        //         6, 7, 8,
        //     };
        //
        //     var result = new[]
        //     {
        //
        //         04,05,-1,-1,-1,-1,-1,-1,
        //         07,08,-1,-1,-1,-1,-1,-1,
        //         -1,-1,-1,-1,-1,-1,-1,-1,
        //         -1,-1,-1,-1,-1,-1,-1,-1,
        //         -1,-1,-1,-1,-1,-1,-1,-1,
        //         -1,-1,-1,-1,-1,-1,-1,-1,
        //         -1,-1,-1,-1,-1,-1,-1,-1,
        //         -1,-1,-1,-1,-1,-1,-1,-1,
        //     };
        //
        //     td.SetPixels(result);
        //
        //     var pixelData = td.GetPixels(-1, -1, 3, 3);
        //
        //     CollectionAssert.AreEqual(pixelData, src);
        // }
        //
        // [Test]
        // public void GetOutBoundsTopRightPixelDataTest()
        // {
        //     var td = new TextureData(8, 8);
        //
        //     var src = new[]
        //     {
        //         00,01,02,
        //         03,04,05,
        //         06,07,08,
        //     };
        //
        //     var result = new[]
        //     {
        //
        //         -1,-1,-1,-1,-1,-1,03,04,
        //         -1,-1,-1,-1,-1,-1,06,07,
        //         -1,-1,-1,-1,-1,-1,-1,-1,
        //         -1,-1,-1,-1,-1,-1,-1,-1,
        //         -1,-1,-1,-1,-1,-1,-1,-1,
        //         -1,-1,-1,-1,-1,-1,-1,-1,
        //         -1,-1,-1,-1,-1,-1,-1,-1,
        //         -1,-1,-1,-1,-1,-1,-1,-1,
        //     };
        //
        //     td.SetPixels(result);
        //
        //     var pixelData = td.GetPixels(6, -1, 3, 3);
        //
        //     CollectionAssert.AreEqual(pixelData, src);
        // }
        //
        // [Test]
        // public void GetOutBoundsBottomRightPixelDataTest()
        // {
        //     var td = new TextureData(8, 8);
        //
        //     var src = new[]
        //     {
        //         00,01,02,
        //         03,04,05,
        //         06,07,08,
        //     };
        //
        //     var result = new[]
        //     {
        //         -1,-1,-1,-1,-1,-1,-1,-1,
        //         -1,-1,-1,-1,-1,-1,-1,-1,
        //         -1,-1,-1,-1,-1,-1,-1,-1,
        //         -1,-1,-1,-1,-1,-1,1,-1,
        //         -1,-1,-1,-1,-1,-1,-1,-1,
        //         -1,-1,-1,-1,-1,-1,-1,-1,
        //         -1,-1,-1,-1,-1,-1,00,01,
        //         -1,-1,-1,-1,-1,-1,03,04,
        //     };
        //
        //     td.SetPixels(result);
        //
        //     var pixelData = td.GetPixels(5, 5, 3, 3);
        //
        //     CollectionAssert.AreEqual(pixelData, src);
        // }
        //
        // [Test]
        // public void GetOutBoundsBottomLeftPixelDataTest()
        // {
        //     var td = new TextureData(8, 8);
        //
        //     var src = new[]
        //     {
        //         00,01,02,
        //         03,04,05,
        //         06,07,08,
        //     };
        //
        //     var result = new[]
        //     {
        //         -1,-1,-1,-1,-1,-1,-1,-1,
        //         -1,-1,-1,-1,-1,-1,-1,-1,
        //         -1,-1,-1,-1,-1,-1,-1,-1,
        //         -1,-1,-1,-1,-1,-1,-1,-1,
        //         -1,-1,-1,-1,-1,-1,-1,-1,
        //         -1,-1,-1,-1,-1,-1,-1,-1,
        //         01,02,-1,-1,-1,-1,-1,-1,
        //         04,05,-1,-1,-1,-1,-1,-1,
        //     };
        //
        //     td.SetPixels(result);
        //
        //     var pixelData = td.GetPixels(-1, 6, 3, 3);
        //
        //     CollectionAssert.AreEqual(pixelData, src);
        // }
        //
        // [Test]
        // public void GetOutBoundsMiddlePixelDataTest()
        // {
        //     var td = new TextureData(8, 8);
        //
        //     var src = new[]
        //     {
        //         0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        //         0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        //         0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        //         0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        //         0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        //         0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        //         0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        //         0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        //         0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        //         0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        //     };
        //
        //     td.SetPixels(zeroedPixelData);
        //
        //     var pixelData = td.GetPixels(-1, -1, 10, 10);
        //
        //     CollectionAssert.AreEqual(pixelData, src);
        // }

        [Test]
        public void MergeInBoundsTopLeftPixelDataTest()
        {
            var td = new TextureData(8, 8);

            var srcA = new[]
            {
                00,01,02,
                03,00,05,
                06,07,08,
            };

            var srcB = new[]
            {
                00,10,00,
                13,00,15,
                00,17,00,
            };

            var result = new[]
            {
                00,01,02,00,00,00,00,00,
                03,00,10,00,00,00,00,00,
                06,13,08,15,00,00,00,00,
                00,00,17,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
            };

            td.MergePixels(0, 0, 3, 3, srcA);
            td.MergePixels(1, 1, 3, 3, srcB);

            var pixelData = td.pixels;

            CollectionAssert.AreEqual(pixelData, result);
        }

        [Test]
        public void MergeInBoundsTopRightPixelDataTest()
        {
            var td = new TextureData(8, 8);

            var srcA = new[]
            {
                00,01,02,
                03,00,05,
                06,07,08,
            };

            var srcB = new[]
            {
                00,10,00,
                13,00,15,
                00,17,00,
            };

            var result = new[]
            {
                00,00,00,00,00,00,01,02,
                00,00,00,00,00,10,00,05,
                00,00,00,00,13,06,15,08,
                00,00,00,00,00,17,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
            };

            td.MergePixels(5, 0, 3, 3, srcA);
            td.MergePixels(4, 1, 3, 3, srcB);

            var pixelData = td.pixels;

            CollectionAssert.AreEqual(pixelData, result);
        }

        [Test]
        public void MergeInBoundsBottomRightPixelDataTest()
        {
            var td = new TextureData(8, 8);


            var srcA = new[]
            {
                00,01,02,
                03,00,05,
                06,07,08,
            };

            var srcB = new[]
            {
                00,10,00,
                13,00,15,
                00,17,00,
            };

            var result = new[]
            {
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,10,00,00,
                00,00,00,00,13,00,15,02,
                00,00,00,00,00,17,00,05,
                00,00,00,00,00,06,07,08,
            };

            td.MergePixels(5, 5, 3, 3, srcA);
            td.MergePixels(4, 4, 3, 3, srcB);

            var pixelData = td.pixels;

            CollectionAssert.AreEqual(pixelData, result);
        }

        [Test]
        public void MergeInBoundsBottomLeftPixelDataTest()
        {
            var td = new TextureData(8, 8);

            var srcA = new[]
            {
                00,01,02,
                03,00,05,
                06,07,08,
            };

            var srcB = new[]
            {
                00,10,00,
                13,00,15,
                00,17,00,
            };

            var result = new[]
            {
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,10,00,00,00,00,00,
                00,13,02,15,00,00,00,00,
                03,00,17,00,00,00,00,00,
                06,07,08,00,00,00,00,00,
            };

            td.MergePixels(0, 5, 3, 3, srcA);
            td.MergePixels(1, 4, 3, 3, srcB);

            var pixelData = td.pixels;

            CollectionAssert.AreEqual(pixelData, result);
        }

        [Test]
        public void MergeInBoundsMiddlePixelDataTest()
        {
            var td = new TextureData(8, 8);

            var srcA = new[]
            {
                00,01,02,
                03,00,05,
                06,07,08,
            };

            var srcB = new[]
            {
                00,10,00,
                13,00,15,
                00,17,00,
            };

            var result = new[]
            {
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,01,02,00,00,
                00,00,00,03,00,10,00,00,
                00,00,00,06,13,08,15,00,
                00,00,00,00,00,17,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
            };

            td.MergePixels(3, 2, 3, 3, srcA);
            td.MergePixels(4,3, 3, 3, srcB);

            var pixelData = td.pixels;

            CollectionAssert.AreEqual(pixelData, result);
        }

        [Test]
        public void MergeAllPixelOutBoundsDataTest()
        {
            var td = new TextureData(8, 8);
            // td.Clear(0, 1, 1, 6, 6);

            var pixelData = td.pixels;

            var src = new[]
            {
                00,10,00,00,00,00,00,10,00,
                13,00,15,00,00,00,13,00,15,
                00,17,00,00,00,00,00,17,00,
                00,00,00,00,01,02,00,00,00,
                00,00,00,03,00,05,00,00,00,
                00,00,00,06,07,08,00,00,00,
                00,10,00,00,00,00,00,10,00,
                13,00,15,00,00,00,13,00,15,
                00,17,00,00,00,00,00,17,00,

            };

            var result = new[]
            {
                00,15,00,00,00,13,00,15,
                17,00,00,00,00,00,17,00,
                00,00,00,01,02,00,00,00,
                00,00,03,00,05,00,00,00,
                00,00,06,07,08,00,00,00,
                10,00,00,00,00,00,10,00,
                00,15,00,00,00,13,00,15,
                17,00,00,00,00,00,17,00,
            };

            td.MergePixels(-1, -1, 9, 9, src);

            CollectionAssert.AreEqual(pixelData, result);
        }

        [Test]
        public void MergeOutBoundsTopLeftPixelDataTest()
        {
            var td = new TextureData(8, 8);
            
            var srcA = new[]
            {
                00,01,02,
                03,00,05,
                06,07,08,
            };

            var srcB = new[]
            {
                00,10,00,
                13,00,15,
                00,17,00,
            };

            var result = new[]
            {
                00,10,00,00,00,00,00,00,
                13,08,15,00,00,00,00,00,
                00,17,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
            };

            td.MergePixels(-1, -1, 3, 3, srcA);
            td.MergePixels(0, 0, 3, 3, srcB);

            var pixelData = td.pixels;

            CollectionAssert.AreEqual(pixelData, result);
        }

        [Test]
        public void MergeOutBoundsTopRightPixelDataTest()
        {
            var td = new TextureData(8, 8);
            
            var srcA = new[]
            {
                00,01,02,
                03,00,05,
                06,07,08,
            };

            var srcB = new[]
            {
                00,10,00,
                13,00,15,
                00,17,00,
            };

            var result = new[]
            {

                00,00,00,00,00,00,10,00,
                00,00,00,00,00,13,06,15,
                00,00,00,00,00,00,17,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
            };

            td.MergePixels(6, -1, 3, 3, srcA);
            td.MergePixels(5, 0, 3, 3, srcB);

            var pixelData = td.pixels;

            CollectionAssert.AreEqual(pixelData, result);
        }

        [Test]
        public void MergeOutBoundsBottomRightPixelDataTest()
        {
            var td = new TextureData(8, 8);

            var srcA = new[]
            {
                00,01,02,
                03,00,05,
                06,07,08,
            };

            var srcB = new[]
            {
                00,10,00,
                13,00,15,
                00,17,00,
            };

            var result = new[]
            {
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,10,00,
                00,00,00,00,00,13,00,15,
                00,00,00,00,00,00,17,00,
                
            };

            td.MergePixels(6, 6, 3, 3, srcA);
            td.MergePixels(5, 5, 3, 3, srcB);

            var pixelData = td.pixels;

            CollectionAssert.AreEqual(pixelData, result);
        }

        [Test]
        public void MergeOutBoundsBottomLeftPixelDataTest()
        {
            var td = new TextureData(8, 8);

            var srcA = new[]
            {
                00,01,02,
                03,00,05,
                06,07,08,
            };

            var srcB = new[]
            {
                00,10,00,
                13,00,15,
                00,17,00,
            };

            var result = new[]
            {
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,00,00,00,00,00,00,00,
                00,10,00,00,00,00,00,00,
                13,02,15,00,00,00,00,00,
                00,17,00,00,00,00,00,00,

            };

            td.MergePixels(-1, 6, 3, 3, srcA);
            td.MergePixels(0, 5, 3, 3, srcB);

            var pixelData = td.pixels;

            CollectionAssert.AreEqual(pixelData, result);
        }

        [Test]
        public void MergeOutBoundsMiddlePixelDataTest()
        {
            var td = new TextureData(8, 8);

            var pixelData = td.pixels;

            var src = new int[64];

            td.SetPixels(-1, -1, 10, 10, src);

            CollectionAssert.AreEqual(pixelData, zeroedPixelData);
        }

        private int[] zeroedPixelData = new int[64];


        private int[] incrementalPixelData = Enumerable.Range(0, 64).ToArray();


        [Test]
        public void BlockCopyInBoundsTopLeftTest()
        {
            var td = new TextureData(8, 8);
            td.SetPixels(incrementalPixelData);

            var result = new[]
            {
                00, 01, 02, 03,
                08, 09, 10, 11,
                16, 17, 18, 19,
                24, 25, 26, 27,
            };
            
            CollectionAssert.AreEqual(td.GetPixels(0, 0, 4, 4), result);
        }

        [Test]
        public void BlockCopyInBoundsTopRightTest()
        {
            var td = new TextureData(8, 8);
            td.SetPixels(incrementalPixelData);

            var result = new[]
            {
                04, 05, 06, 07,
                12, 13, 14, 15,
                20, 21, 22, 23,
                28, 29, 30, 31,
            };

            CollectionAssert.AreEqual(td.GetPixels(4, 0, 4, 4), result);
        }

        [Test]
        public void BlockCopyInBoundsBottomRightTest()
        {
            var td = new TextureData(8, 8);
            td.SetPixels(incrementalPixelData);

            var result = new[]
            {
                36, 37, 38, 39,
                44, 45, 46, 47,
                52, 53, 54, 55,
                60, 61, 62, 63,
            };

            CollectionAssert.AreEqual(td.GetPixels(4, 4, 4, 4), result);
        }

        [Test]
        public void BlockCopyInBoundsBottomLeftTest()
        {
            var td = new TextureData(8, 8);
            td.SetPixels(incrementalPixelData);

            var result = new[]
            {
                32, 33, 34, 35,
                40, 41, 42, 43,
                48, 49, 50, 51,
                56, 57, 58, 59,
            };

            CollectionAssert.AreEqual(td.GetPixels(0, 4, 4, 4), result);
        }

        [Test]
        public void BlockCopyInBoundsTest()
        {
            var td = new TextureData(8, 8);
            td.SetPixels(incrementalPixelData);

            var result = new[]
            {
                18, 19, 20, 21,
                26, 27, 28, 29,
                34, 35, 36, 37,
                42, 43, 44, 45,
            };

            CollectionAssert.AreEqual(td.GetPixels(2, 2, 4, 4), result);
        }

        [Test]
        public void BlockCopyOutBoundsTopLeftTest()
        {
            var td = new TextureData(8, 8);
            td.SetPixels(incrementalPixelData);

            var result = new[]
            {
                00, 01, 02,
                08, 09, 10,
                16, 17, 18,
            };

            CollectionAssert.AreEqual(td.GetPixels(-1, -1, 4, 4), result);
        }

        [Test]
        public void BlockCopyOutBoundsTopRightTest()
        {
            var td = new TextureData(8, 8);
            td.SetPixels(incrementalPixelData);

            var result = new[]
            {
                05, 06, 07,
                13, 14, 15,
                21, 22, 23,
                
            };

            CollectionAssert.AreEqual(td.GetPixels(5, -1, 4, 4), result);
        }

        [Test]
        public void BlockCopyOutBoundsBottomRightTest()
        {
            var td = new TextureData(8, 8);
            td.SetPixels(incrementalPixelData);

            var result = new[]
            {
                45, 46, 47,
                53, 54, 55,
                61, 62, 63,
            };

            CollectionAssert.AreEqual(td.GetPixels(5, 5, 4, 4), result);
        }

        [Test]
        public void BlockCopyOutBoundsBottomLeftTest()
        {
            var td = new TextureData(8, 8);
            td.SetPixels(incrementalPixelData);

            var result = new[]
            {
                40, 41, 42,
                48, 49, 50,
                56, 57, 58,
            };

            CollectionAssert.AreEqual(td.GetPixels(-1, 5, 4, 4), result);
        }

        [Test]
        public void BlockCopyOutBoundsLargerTest()
        {
            var td = new TextureData(8, 8);
            td.SetPixels(incrementalPixelData);

            CollectionAssert.AreEqual(td.GetPixels(-1, -1, 9, 9), incrementalPixelData);
        }

        [Test]
        public void BlockCopyInverseTest()
        {
            var td = new TextureData(8, 8);
            td.SetPixels(incrementalPixelData);

            var result = new int[0];

            CollectionAssert.AreEqual(td.GetPixels(2, 1, -2, -1), result);
        }


        // int[] BlockCopy(int[] src, int srcW, int srcH, int x, int y, int w, int h)
        // {
        //
        //     // Adjust X
        //     if (x < 0)
        //     {
        //         w += x; 
        //         x = 0;
        //     }
        //     
        //     // Adjust Y
        //     if (y < 0)
        //     {
        //         h += y;
        //         y = 0;
        //     }
        //
        //     // Adjust Width
        //     if ((x + w) > srcW)
        //     {
        //         w -= ((x + w) - srcW);
        //     }
        //
        //     // Adjust Height
        //     if ((y + h) > srcH)
        //     {
        //         h -= ((y + h) - srcH);
        //     }
        //
        //     if (w <= 0 || h <= 0)
        //         return new int[0];
        //
        //     var output = new int[w * h];
        //
        //     var row = 0;
        //     var startCol = 0;
        //     var endCol = 0;
        //     var destCol = 0;
        //
        //     for (row = 0; row < h; row++)
        //     {
        //         startCol = x + (row + y) * srcW;
        //         destCol = row * w;
        //
        //         Array.Copy(src, startCol, output, destCol, w);
        //     }
        //
        //     return output;
        //
        // }


    }
}
