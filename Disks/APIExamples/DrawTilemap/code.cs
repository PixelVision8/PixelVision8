/**
Pixel Vision 8 - DrawTilemap Example
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

            // Update the scroll position
            ScrollPosition(pos.X);

        }

        public override void Draw()
        {
            // Clear the background
            Clear();

            // Draw the actual tilemap starting below the top border and manually adjust the scroll offset values
            DrawTilemap(0, 16, 32, 28, pos.X, 16);

            // Draw the tilemap top border over everything else and lock the x scroll value
            DrawTilemap(0, 0, 32, 2, 0);

            // Display the scroll position
            DrawText("Scroll Position: " + ScrollPosition().X + ", " + ScrollPosition().Y, 8, 16, DrawMode.Sprite,
                "large");

        }
    }
}