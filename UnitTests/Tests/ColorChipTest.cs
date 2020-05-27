using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using PixelVision8.Engine.Chips;
using PixelVision8.Engine.Utils;
using PixelVision8.Runner.Importers;
using PixelVision8.Runner.Parsers;

namespace PixelVision8Tests
{
    public class ColorChipTest
    {
        public static string GetTestDataFolder(string testDataFolder)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;

            return Path.Combine(path, "Content", testDataFolder);

        }

        readonly string filePath = Path.Combine(GetTestDataFolder("ProjectTemplate"), "colors.png");

        private ColorChip colorChip;
        
        public ColorParser colorParser;

        [SetUp]
        public void Setup()
        {
            colorChip = new ColorChip();
            colorChip.Configure();
        }

        public bool LoadColorChip()
        {
           
            if (File.Exists(filePath))
            {
                var imageBytes = File.ReadAllBytes(filePath);

                var pngReader = new PNGReader(imageBytes, colorChip.maskColor)
                    {FileName = Path.GetFileNameWithoutExtension(filePath)};

                if(colorParser == null)
                    colorParser = new ColorParser(pngReader, colorChip);
                
                colorParser.CalculateSteps();

                while (colorParser.completed == false)
                {
                    colorParser.NextStep();
                }

                return true;
            }

            return false;
        }

        [Test]
        public void TotalParsedColorsTest()
        {

            if (LoadColorChip())
            {
                
                // Assert.AreEqual(colorParser.totalColors, 16);

            }
            else
            {
                Assert.Fail("Couldn't configure Color Chip.");
            }

        }

        protected readonly string[] defaultColors =
        {
            "#FF00FF",
            "#2D1B2E",
            "#218A91",
            "#3CC2FA",
            "#9AF6FD",
            "#4A247C",
            "#574B67",
            "#937AC5",
            "#8AE25D",
            "#8E2B45",
            "#F04156",
            "#F272CE",
            "#D3C0A8",
            "#C5754A",
            "#F2A759",
            "#F7DB53",
            "#F9F4EA"
        };


        [Test]
        public void MaskColor()
        {
            var maskColor = colorChip.ReadColorAt(0);

            CollectionAssert.AreEqual(maskColor, this.defaultColors[0]);
        }

        [Test]
        public void ParsedColorsTest()
        {

            if (LoadColorChip())
            {
                var results = Enumerable.Repeat(defaultColors[1], 255).ToArray();

                Array.Copy(defaultColors, 1, results, 1, defaultColors.Length-1);
                
                CollectionAssert.AreEqual(colorChip.hexColors,  results);
            }
            else
            {
                Assert.Fail("Couldn't configure Color Chip.");
            }

        }

        [Test]
        public void ParsedColorsCappedSizeTest()
        {

            if (LoadColorChip())
            {

                // Set the max colors to 5
                byte max = 5;
                colorChip.maxColors = max;

                // Generate an array with filled with the default color
                var results = Enumerable.Repeat(defaultColors[1], 255).ToArray();

                // Copy over the default colors up to the max count, everything else will be set to the default color
                Array.Copy(defaultColors, 1, results, 1, max - 1);

                // Hex colors from chip should always be 256 (255)
                Assert.AreEqual(colorChip.hexColors.Length, byte.MaxValue);

                // Test that only colors 1-5 are returned from the color chip
                CollectionAssert.AreEqual(colorChip.hexColors, results);

                // Check for a color out of the max range to see if it was set to the default value correctly
                Assert.AreEqual(colorChip.hexColors[max+1], defaultColors[1]);
            }
            else
            {
                Assert.Fail("Couldn't configure Color Chip.");
            }

        }

        [Test]
        public void ParsedDebugColorsCappedSizeTest()
        {

            if (LoadColorChip())
            {

                // Set the max colors to 8
                byte max = 8;
                colorChip.maxColors = max;

                // Enable debug mode
                colorChip.debugMode = true;

                // Generate an array with filled with the mask color
                var results = Enumerable.Repeat(colorChip.maskColor, 255).ToArray();

                // Copy over the default colors up to the max count, everything else will be set to the max color
                Array.Copy(defaultColors, 1, results, 1, max - 1);

                // Hex colors from chip should always be 256 (255)
                Assert.AreEqual(colorChip.hexColors.Length, byte.MaxValue);

                // Test that only colors 1-5 are returned from the color chip
                CollectionAssert.AreEqual(colorChip.hexColors, results);

                // Check for a color out of the max range to see if it was set to the default value correctly
                Assert.AreEqual(colorChip.hexColors[max + 1], colorChip.maskColor);
            }
            else
            {
                Assert.Fail("Couldn't configure Color Chip.");
            }

        }

        [Test]
        public void BGDefaultValueTest()
        {

            if (LoadColorChip())
            {
                Assert.AreEqual(colorChip.backgroundColor, 0);
            }
            else
            {
                Assert.Fail("Couldn't configure Color Chip.");
            }

        }

        [Test]
        public void BGNegativeBoundsTest()
        {
            colorChip.backgroundColor = 5;
            colorChip.backgroundColor = unchecked((byte)-1);

            // Expecting this to wrap around to the last color since there is no max color cap
            Assert.AreEqual(5, colorChip.backgroundColor);

        }

        [Test]
        public void BGOutBoundsTest()
        {
            colorChip.maxColors = 15;

            colorChip.backgroundColor = 200;

            Assert.AreEqual(1, colorChip.backgroundColor);

        }

        [Test]
        public void MaxColorDefaultTest()
        {
            if (LoadColorChip())
            {

                Assert.AreEqual(colorChip.total, defaultColors.Length);
            }
            else
            {
                Assert.Fail("Couldn't configure Color Chip.");
            }

        }

        [Test]
        public void MaxColorLimitTest()
        {
            colorChip.maxColors = 2;
                
            Assert.AreEqual(colorChip.total, 3);
            Assert.AreEqual(colorChip.hexColors.Take(4), new[] { defaultColors[1], defaultColors[1], defaultColors[2], defaultColors[1] });

        }

        [Test]
        public void MaxColorLimitDebugTest()
        {
            colorChip.maxColors = 2;
            colorChip.debugMode = true;
            
            Assert.AreEqual(colorChip.total, 3);
            Assert.AreEqual(colorChip.hexColors.Take(4), new []{colorChip.maskColor, defaultColors[1], defaultColors[2], colorChip.maskColor });
        }

        [Test]
        public void DebugColorTest()
        {
            colorChip.debugMode = true;

            Assert.AreEqual(colorChip.ReadColorAt(0), colorChip.maskColor);

            Assert.AreEqual(colorChip.ReadColorAt(17), colorChip.maskColor);

        }

        [Test]
        public void BackgroundColorZeroTest()
        {
            colorChip.backgroundColor = 14;

            var colors = colorChip.hexColors;

            // reading the colors from the chip directly will always return the mask color
            Assert.AreEqual(defaultColors[14], colors[0]);
            
        }

        [Test]
        public void BackgroundColorBGIndexTest()
        {
            colorChip.backgroundColor = 14;

            var colors = colorChip.hexColors;

            // reading the colors from the chip directly will always return the mask color
            Assert.AreEqual(colors[17], defaultColors[14]);

        }

        [Test]
        public void BackgroundColorZeroDebugTest()
        {
            colorChip.backgroundColor = 14;

            var colors = colorChip.hexColors;

            // reading the colors from the chip directly will always return the mask color
            Assert.AreEqual(colorChip.ReadColorAt(0), colorChip.maskColor);
            
        }

        [Test]
        public void BackgroundColorBGIndexDebugTest()
        {
            colorChip.backgroundColor = 14;

            var colors = colorChip.hexColors;

            // reading the colors from the chip directly will always return the mask color
            Assert.AreEqual(colorChip.ReadColorAt(17), colorChip.maskColor);

        }

        [Test] 
        public void TotalUsedColorsDefaultTest()
        {
            // the mask color counts as a color
            Assert.AreEqual(defaultColors.Length, colorChip.totalUsedColors);
        }

        [Test]
        public void TotalUsedColorsSmallMaxTest()
        {
            colorChip.maxColors = 4;

            // The mask color counts as a color
            Assert.AreEqual(colorChip.maxColors, colorChip.totalUsedColors);

        }

        [Test]
        public void TotalUsedColorsSmallMaxDebugTest()
        {
            colorChip.maxColors = 4;
            colorChip.debugMode = true;

            // Test if the maximum is less than the total filled colors
            Assert.AreEqual(colorChip.totalUsedColors, colorChip.maxColors);

        }

        [Test]
        public void TotalUsedColorsExactFitMaxTest()
        {

            colorChip.maxColors = (byte)defaultColors.Length;

            // Test if the maximum is less than the total filled colors
            Assert.AreEqual(colorChip.totalUsedColors, defaultColors.Length);

        }

        [Test]
        public void MaxOutOfBoundsTest()
        {
            colorChip.maxColors = unchecked((byte) 256);
            
            // Test if the maximum is less than the total filled colors
            Assert.AreEqual(3, colorChip.maxColors);

        }

    }
}
