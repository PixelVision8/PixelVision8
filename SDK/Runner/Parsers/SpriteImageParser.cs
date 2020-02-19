
using System;
using System.Linq;
using Microsoft.Xna.Framework;
using PixelVision8.Engine.Chips;
using PixelVision8.Engine.Utils;

namespace PixelVision8.Runner.Parsers
{
    public class SpriteImageParser : ImageParser
    {
        protected IEngineChips chips;
        protected Color[] colorData;
        protected int cps;
        protected int index;
        // protected Color maskColor;
        protected int maxSprites;
        protected SpriteChip spriteChip;
        protected int[] spriteData;
        protected int spriteHeight;
        protected int spritesAdded;
        protected int spriteWidth;
        // protected Color[] srcColors;
        protected Color[] tmpPixels;
        // protected int totalPixels;
        protected int totalSprites;
        protected int x, y;
        protected Image image;

        public SpriteImageParser(IImageParser parser, IEngineChips chips, bool unique = true,
            SpriteChip spriteChip = null) : base(parser)
        {

            this.chips = chips;
            this.spriteChip = spriteChip ?? chips.SpriteChip;

            spriteWidth = this.spriteChip.width;
            spriteHeight = this.spriteChip.height;

        }

        public override void CalculateSteps()
        {
            base.CalculateSteps();

            steps.Add(CreateImage);

            steps.Add(PrepareSprites);

            steps.Add(CutOutSprites);

            steps.Add(PostCutOutSprites);
        }

        public virtual void PrepareSprites()
        {
            
            cps = spriteChip.colorsPerSprite;

            totalSprites = image.TotalSprites;

            maxSprites = SpriteChipUtil.CalculateTotalSprites(spriteChip.textureWidth, spriteChip.textureHeight,
            spriteWidth, spriteHeight);
            
            // // Keep track of number of sprites added
            spritesAdded = 0;

            StepCompleted();

        }

        protected virtual void CreateImage()
        {
            colorData = chips.GetChip(ColorMapParser.chipName, false) is ColorChip colorMapChip
                ? colorMapChip.colors
                : chips.ColorChip.colors;

            var colorRefs = colorData.Select(c => ColorUtils.RgbToHex(c.R, c.G, c.B)).ToArray();

            // Remove the colors that are not supported
            Array.Resize(ref colorRefs, chips.ColorChip.totalUsedColors);

            // Convert all of the pixels into color ids
            var pixelIDs = Parser.colorPixels.Select(c => Array.IndexOf(colorRefs, ColorUtils.RgbToHex(c.R, c.G, c.B))).ToArray();

            image = new Image(Parser.width, Parser.height, colorRefs, pixelIDs, new Point(spriteWidth, spriteHeight));

            StepCompleted();
        }

        public virtual void CutOutSprites()
        {

            for (var i = 0; i < totalSprites; i++)
            {
                
                // Convert sprite to color index
                ConvertColorsToIndexes(cps);

                ProcessSpriteData();
                // }

                index++;

            }
            StepCompleted();
        }

        protected virtual void PostCutOutSprites()
        {
            // Add custom logic
            StepCompleted();
        }

        protected virtual void ProcessSpriteData()
        {
            if (spritesAdded < maxSprites)
            {
                // TODO need to deprecate this since the sprite file should load up exactly how it is read
                if (spriteChip.unique)
                {
                    if (spriteChip.FindSprite(spriteData) == -1)
                    {
                        spriteChip.UpdateSpriteAt(spritesAdded, spriteData);
                        spritesAdded++;
                    }
                }
                else
                {
                    if (spriteChip.IsEmpty(spriteData) == false)
                    {
                        spriteChip.UpdateSpriteAt(index, spriteData);
                        spritesAdded++;
                    }
                }
            }
        }

        public virtual bool IsEmpty(Color[] pixels)
        {
            var total = pixels.Length;
            var transPixels = 0;

            for (var i = 0; i < total; i++)
                if (pixels[i].A < 1)
                    transPixels++;

            return transPixels == total;
        }

        public virtual void ConvertColorsToIndexes(int totalColors)
        {

            spriteData = image.GetSpriteData(index, totalColors);
            
        }

        public override void Dispose()
        {
            base.Dispose();
            chips = null;
            spriteChip = null;
        }
    }
}