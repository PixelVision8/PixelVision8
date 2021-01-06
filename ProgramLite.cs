using Microsoft.Xna.Framework;
using PixelVision8.Engine.Chips;
using PixelVision8.Engine.Utils;
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
            using (var game = new ExampleRunner(root))
            {
                game.Run();
            }

        }
    }

    class ExampleRunner : CSharpRunner
    {
        public ExampleRunner(string gamePath) : base(gamePath)
        {
        }

        protected override void AddGameChip()
        {
            _tmpEngine.ActivateChip("GameChip", new ExampleGameChip());
        }
    }

    class ExampleGameChip : GameChipLite
    {
        // Use floats to store the subpixel position
        private float speed = 5;
        private float nextPos;
        private int lastSpriteCount;

        // Use this point to position the  sprites
        private Point pos;

        // A group of sprite IDs for the DrawSprites() API
        private int[] spriteGroup =
        {
            -1, 33, 34, -1,
            48, 49, 50, 51,
            64, 65, 66, 67,
            -1, 81, 82, -1
        };

        private int[] test = new int[64];
        public override void Init()
        {
            BackgroundColor(1);

            for (int i = 0; i < test.Length; i++)
            {
                test[i] = 15;
            }

            // DrawPixels(test, 0, 0, 8, 8, false, false, DrawMode.TilemapCache);

            // DrawText("Works", 8, 0, DrawMode.TilemapCache, "large");
        }

        public override void Update(int timeDelta)
        {
            // Calculate the next position
            nextPos = nextPos + (speed * (timeDelta / 100f));

            // Need to convert the nextPoint to an int, so we'll save it in a point
            pos.X = MathUtil.Repeat((int)nextPos, display.X + 16);
            pos.Y = MathUtil.Repeat((int)nextPos, display.Y + 16);
        }


        public override void Draw()
        {
            // Redraw the display
            RedrawDisplay();

            // Draw sprite group moving horizontally and hide when it goes offscreen
            DrawSprites(spriteGroup, pos.X, 8, 4);

            // Draw flipped sprite group moving vertically but render when offscreen
            DrawSprites(spriteGroup, 36, pos.Y, 4, true);

            // Show the total number of sprites
            DrawText("FPS " + fps + " Sprites " + lastSpriteCount, 144 - (8 * 4), 224, DrawMode.Sprite, "large", 15);

            // Draw the x,y position of each sprite
            DrawText("(" + MathUtil.FloorToInt(nextPos) + ",8)", pos.X + 32, 8, DrawMode.Sprite, "large", 15);
            DrawText("(36," + MathUtil.FloorToInt(nextPos) + ")", 66, pos.Y + 12, DrawMode.Sprite, "large", 15);

            lastSpriteCount = CurrentSprites;
        }
    }
}
