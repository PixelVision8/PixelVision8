/**
Pixel Vision 8 - DrawSprites Example
Copyright(C) 2017, Pixel Vision 8 (http://pixelvision8.com)
Created by Jesse Freeman(@jessefreeman)

This project was designed to display some basic instructions when you create
a new game.Simply delete the following code and implement your own Init(),
Update() and Draw() logic.

Learn more about making Pixel Vision 8 games at
https://www.pixelvision8.com/getting-started
**/

using Microsoft.Xna.Framework;
using PixelVision8.Player;
using PixelVision8.Engine.Utils;

namespace PixelVision8.Examples
{
    class DrawSpritesExample : GameChipLite
    {
        // Use floats to store the subpixel position
        private float speed = 5;
        private float nextPos;

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

        public override void Update(int timeDelta)
        {
            // Calculate the next position
            nextPos = nextPos + (speed * (timeDelta / 100f));

            // Need to convert the nextPoint to an int, so we'll save it in a point
            pos.X = (int)nextPos;
            pos.Y = (int)nextPos;
        }

        public override void Draw()
        {
            // Redraw the display
            RedrawDisplay();

            // Draw sprite group moving horizontally and hide when it goes offscreen
            DrawSprites(spriteGroup, pos.X, 8, 4);

            // Draw flipped sprite group moving vertically but render when offscreen
            DrawSprites(spriteGroup, 36, pos.Y, 4, true, false, DrawMode.Sprite, 0, false);

            // Show the total number of sprites
            DrawText("Sprites " + CurrentSprites, 144 + 24, 224, DrawMode.Sprite, "large", 15);

            // Draw the x,y position of each sprite
            DrawText("(" + MathUtil.FloorToInt(nextPos) + ",8)", pos.X + 32, 8, DrawMode.Sprite, "large", 15);
            DrawText("(36," + MathUtil.FloorToInt(nextPos) + ")", 66, pos.Y + 12, DrawMode.Sprite, "large", 15);

        }
    }
}