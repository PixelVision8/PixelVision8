/**
Pixel Vision 8 - MaxSpriteCount Example
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
using System;

namespace PixelVision8.Examples
{
    class MaxSpriteCountExample : GameChip
    {

        Random random = new Random();

        // Store random integers for drawing a random character to the screen
        private int charID, x, y, colorID = 0;

        // Reference for the size of the display
        private Point display;

        // Total number of sprites to draw each frame
        private int sprites = 500;

        public override void Init()
        {

            // Get the display size when the game loads up
            display = Display();

            // Display the text for the maximum and total number of sprites
            DrawText("Maximum Sprites " + MaxSpriteCount(), 1, 1, DrawMode.Tile, "large", 15);
            DrawText("Total Sprites ", 1, 2, DrawMode.Tile, "large", 15);

        }

        public override void Draw()
        {
            // Redraw the display
            RedrawDisplay();

            // Perform the next block of code 10 times
            for (int i = 0; i < sprites; i++)
            {

                // Assign random values to each of these variable
                charID = random.Next(32, 126);
                x = random.Next(0, display.X);
                y = random.Next(32, display.Y);
                colorID = random.Next(1, 15);

                // Draw a random character at a random position on the screen with a random color
                DrawText(Convert.ToChar(charID).ToString(), x, y, DrawMode.Sprite, "large", colorID);

            }

            // Draw the total number sprite on the display
            DrawText(CurrentSprites.ToString(), 15, 2, DrawMode.Tile, "large", 15);

        }

    }
}