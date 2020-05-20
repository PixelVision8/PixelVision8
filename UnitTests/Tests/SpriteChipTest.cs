using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using PixelVision8.Engine.Chips;
using PixelVision8.Runner.Importers;
using PixelVision8.Runner.Parsers;

namespace PixelVision8Tests
{
    public class SpriteChipTest
    {
        public static string GetTestDataFolder(string fileName, string projectName = "SpriteExamples")
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;

            return Path.Combine(path, "Content", projectName, fileName);

        }

        public ColorChip colorChip;
        public ColorParser colorParser;
        public SpriteChip spriteChip;
        public SpriteImageParser spriteParser;
        
        private int[] emptySpriteRef = new[]
        {
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 0,
        };

        public bool LoadSpriteChip(string spriteFileName, string colorFileName = null)
        {

            // Create default color chip
            colorChip = new ColorChip();
            colorChip.Configure();

            spriteChip = new SpriteChip();
            spriteChip.Configure();
       
            if (colorFileName != null)
            {
                var colorFilePath = GetTestDataFolder(colorFileName);

                if (File.Exists(colorFilePath))
                {
                    var imageBytes = File.ReadAllBytes(colorFilePath);

                    var pngReader = new PNGReader(imageBytes, colorChip.maskColor)
                        { FileName = Path.GetFileNameWithoutExtension(colorFilePath) };

                    colorParser = new ColorParser(pngReader, colorChip);

                    colorParser.CalculateSteps();

                    while (colorParser.completed == false)
                    {
                        colorParser.NextStep();
                    }

                }

            }

            var spriteFilePath = GetTestDataFolder(spriteFileName);

            if (File.Exists(spriteFilePath))
            {
                var imageBytes = File.ReadAllBytes(spriteFilePath);

                var pngReader = new PNGReader(imageBytes, colorChip.maskColor)
                    { FileName = Path.GetFileNameWithoutExtension(spriteFilePath) };

                spriteParser = new SpriteImageParser(pngReader, colorChip, spriteChip);

                spriteParser.CalculateSteps();

                while (spriteParser.completed == false)
                {
                    spriteParser.NextStep();
                }

                return true;
            }

            return false;
        }

        [Test]
        public void LoadSpritesTest()
        {
            if (LoadSpriteChip("sprites-small.png", "colors-pv8.png"))
            {

                Assert.Pass("Loaded " + spriteChip.totalSprites + " sprites.");
                
            }
            else
            {
                Assert.Fail("Couldn't configure Sprite Chip.");
            }
        }

        [Test]
        public void DefaultSpriteSizeTest()
        {
            var spriteChip = new SpriteChip();

            Assert.AreEqual(spriteChip.width, 8);
            Assert.AreEqual(spriteChip.height, 8);
            
        }

        [Test]
        public void DefaultEmptySpriteSizeTest()
        {
            var chip = new SpriteChip();

            var emptySprite = new int[0];

            chip.ReadSpriteAt(0, ref emptySprite);

            Assert.AreEqual(emptySprite.Length, chip.width * chip.height);

            CollectionAssert.AreEqual(emptySprite, emptySpriteRef);
        }

        [Test]
        public void DefaultEmptySpriteTest()
        {
            var chip = new SpriteChip();

            // Test default empty sprite
            Assert.AreEqual(chip.IsEmpty(emptySpriteRef), true);

            // Create new "empty" sprite
            var tmpSprite = new int[chip.width * chip.height];

            Assert.AreEqual(chip.IsEmpty(tmpSprite), true);

            // Test sprite with random pixel value
            var filledSprite = Enumerable.Range(0, chip.width * chip.height).ToArray();

            Assert.AreEqual(chip.IsEmpty(filledSprite), false);

            // Change a single pixel value in the empty sprite
            emptySpriteRef[60] = 1;

            Assert.AreEqual(chip.IsEmpty(emptySpriteRef), false);

        }

        [Test]
        public void DefaultIsEmptyTest()
        {
            var chip = new SpriteChip();

            Assert.AreEqual(chip.IsEmptyAt(0), true);

            var pixelData = Enumerable.Repeat(0, 64).ToArray();

            chip.UpdateSpriteAt(0, pixelData);

            Assert.AreEqual(chip.IsEmptyAt(0), false);

        }

        [Test]
        public void ReadSpritesTest()
        {
            if (LoadSpriteChip("sprites-small.png", "colors-pv8.png"))
            {

                var spriteData = new int[64];

                spriteChip.ReadSpriteAt(0, ref spriteData);

                var result = new[]
                {        
                    00,00,00,00,00,00,00,00,
                    00,00,00,01,01,01,00,00,
                    00,00,01,10,10,10,01,00,
                    00,01,10,14,10,10,10,01,
                    00,01,01,14,14,10,10,01,
                    00,01,00,01,14,14,01,00,
                    00,01,00,00,01,01,00,00,
                    00,00,00,00,00,00,00,00,
                };

                CollectionAssert.AreEqual(spriteData, result);

            }
            else
            {
                Assert.Fail("Couldn't configure Sprite Chip.");
            }
        }

        [Test]
        public void ReadSpritesNoColorTest()
        {
            if (LoadSpriteChip("sprites-small.png", "colors-nes.png"))
            {
                // Make sure sprite is not empty after not finding any colors
                Assert.AreEqual(spriteChip.IsEmptyAt(0), false);
            }
            else
            {
                Assert.Fail("Couldn't configure Sprite Chip.");
            }
        }

    }
}