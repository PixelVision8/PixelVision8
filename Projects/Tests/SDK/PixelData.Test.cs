
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace PixelVision8.Player
{
    
    public class PixelDataTest
    {

        #region Default Sizes

        [Test]
        public void DefaultPixelDataSizeTest()
        {

            var data = new PixelData();

            Assert.AreEqual(1, data.Width);
            Assert.AreEqual(1, data.Height);
            Assert.AreEqual(1, data.Total);

        }
        
        [Test]
        public void InvalidDefaultSizeTest()
        {

            var data = new PixelData();

            data.Resize(-1, -1);
            
            Assert.AreEqual(1, data.Width);
            Assert.AreEqual(1, data.Height);
            Assert.AreEqual(1, data.Total);
            
        }
        
        [Test]
        public void InvalidWidthDefaultSizeTest()
        {

            var data = new PixelData();

            data.Resize(-1, 8);
            
            Assert.AreEqual(1, data.Width);
            Assert.AreEqual(8, data.Height);
            Assert.AreEqual(8, data.Total);
            
        }
        
        [Test]
        public void InvalidHeightDefaultSizeTest()
        {

            var data = new PixelData();

            data.Resize(8, -1);
            
            Assert.AreEqual(8, data.Width);
            Assert.AreEqual(1, data.Height);
            Assert.AreEqual(8, data.Total);
            
        }
        
        [Test]
        public void ValidDefaultSizeTest()
        {

            var data = new PixelData(8, 8);

            data.Resize(8, 8);
            
            Assert.AreEqual(8, data.Width);
            Assert.AreEqual(8, data.Height);
            Assert.AreEqual(64, data.Total);
            
        }

        #endregion

        #region Resize

        [Test]
        public void ResizePixelTest()
        {

            var data = new PixelData();

            
            data.Resize(8, 8);
            
            Assert.AreEqual(8, data.Width);
            Assert.AreEqual(8, data.Height);
            Assert.AreEqual(64, data.Total);

        }
        
        [Test]
        public void InvalidResizeTest()
        {

            var data = new PixelData(8, 8);

            data.Resize(-1, -1);
            
            Assert.AreEqual(1, data.Width);
            Assert.AreEqual(1, data.Height);
            Assert.AreEqual(1, data.Total);
            
        }
        
        [Test]
        public void InvalidedWidthResizeTest()
        {

            var data = new PixelData();
            
            data.Resize(-1, 8);
            
            Assert.AreEqual(1, data.Width);
            Assert.AreEqual(8, data.Height);
            Assert.AreEqual(8, data.Total);

        }
        
        [Test]
        public void InvalidedHeightResizeTest()
        {

            var data = new PixelData();
            
            data.Resize(8, -1);
            
            Assert.AreEqual(8, data.Width);
            Assert.AreEqual(1, data.Height);
            Assert.AreEqual(8, data.Total);

        }

        #endregion

        #region Iterate Get Value
        
        [Test]
        public void GetValueInRangeTest()
        {
            
            var data = new PixelData();

            Assert.AreEqual(0, data[0]);

        }
        
        [Test]
        public void GetNegateValueTest()
        {
            var data = new PixelData();

            Assert.Throws<IndexOutOfRangeException>(() =>
            {
                var i = data[-1];
            });

        }
        
        [Test]
        public void GetOutOfBoundsValueTest()
        {
            var data = new PixelData();

            Assert.Throws<IndexOutOfRangeException>(() =>
            {
                var i = data[100];
            });

        }

        [Test]
        public void GetIterateInRangeTest()
        {

            var data = new PixelData(8, 8);

            var pixels = new List<int>();

            for (int i = 0; i < data.Total; i++)
            {
                pixels.Add(data[i]);
            }
            
            CollectionAssert.AreEqual(pixels.ToArray(), data.Pixels);

        }
        
        [Test]
        public void GetIterateOutOfRangeTest()
        {

            var data = new PixelData(8, 8);

            Assert.Throws<IndexOutOfRangeException>(() =>
            {
                
                var pixels = new List<int>();

                for (int i = 0; i < 100; i++)
                {
                    pixels.Add(data[i]);
                }
                
            });

        }

        #endregion
        
        #region Iterate Set Value
        
        [Test]
        public void SetValueInRangeTest()
        {
            
            var data = new PixelData();
            data[0] = 1;
            Assert.AreEqual(1, data[0]);

        }
        
        [Test]
        public void SetNegateValueTest()
        {
            var data = new PixelData();

            Assert.Throws<IndexOutOfRangeException>(() =>
            {
                data[-1] = 1;
            });

        }
        
        [Test]
        public void SetOutOfBoundsValueTest()
        {
            var data = new PixelData();

            Assert.Throws<IndexOutOfRangeException>(() =>
            {
                data[100] = 1;
            });

        }

        [Test]
        public void SetIterateInRangeTest()
        {

            var data = new PixelData(8, 8);
            
            var newPixels = Enumerable.Repeat(-1, 64).ToArray();

            for (int i = 0; i < newPixels.Length; i++)
            {
                data[i] = newPixels[i];
            }
            
            CollectionAssert.AreEqual(newPixels.ToArray(), data.Pixels);

        }
        
        [Test]
        public void SetIterateOutOfRangeTest()
        {

            var data = new PixelData(8, 8);
            var newPixels = Enumerable.Repeat(-1, 100).ToArray();

            Assert.Throws<IndexOutOfRangeException>(() =>
            {
                
                for (int i = 0; i < newPixels.Length; i++)
                {
                    data[i] = newPixels[i];
                }
                
            });

        }

        [Test]
        public void SetPixelsCleanCopyTest()
        {

            var pixels = new int[]
            {
                0, 1, 2, 3
            };

            var data = new PixelData(2, 2);
            data.SetPixels(pixels, 2, 2);

            pixels[0] = -1;
            
            CollectionAssert.AreEqual(new[]{0, 1, 2, 3}, data.Pixels);
            
        }


        #endregion

    }
}