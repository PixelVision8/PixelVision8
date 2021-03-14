/**
Pixel Vision 8 - DrawSprite Example
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
    class ExampleGameChip : GameChip
    {
        // Use floats to store the subpixel position
        private float speed = 5;
        private float nextPos;

        // Use this point to position the  sprites
        private Point pos;

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

            // Draw sprite moving horizontally
            DrawSprite(376, pos.X, 8);

            // Draw sprite moving vertically
            DrawSprite(377, 36, pos.Y);

            // Draw the x,y position of each sprite
            DrawText("(" + MathUtil.FloorToInt(nextPos) + ",8)", pos.X + 8, 8, DrawMode.Sprite, "large", 15);
            DrawText("(36," + MathUtil.FloorToInt(nextPos) + ")", 44, pos.Y, DrawMode.Sprite, "large", 15);

        }
    }
}