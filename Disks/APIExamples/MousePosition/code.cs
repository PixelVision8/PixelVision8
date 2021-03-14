/**
Pixel Vision 8 - MousePosition Example
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
    class MousePositionExample : GameChip
    {

        // Store the current position of the mouse
        private Point pos;

        public override void Init()
        {
            // Draw the text for where the position will be displayed
            DrawText("Mouse Position", 1, 1, DrawMode.Tile, "large", 15);

        }

        public override void Update(int timeDelta)
        {
            // Update the mouse position
            pos = MousePosition();

        }

        public override void Draw()
        {
            // Redraw the display
            RedrawDisplay();

            // Test if the mouse is offscreen first
            if (pos.X == -1 || pos.Y == -1)
            {

                // Display that the mouse is offscreen
                DrawText("Offscreen", 128, 8, DrawMode.Sprite, "large", 14);

            }
            else
            {

                // Draw a rectangle that follows the mouse on the screen
                DrawRect(pos.X, pos.Y, 8, 8, 5);

                // Display the X and Y position of the mouse
                DrawText(pos.X + "," + pos.Y, 128, 8, DrawMode.Sprite, "large", 14);

            }
        }
    }
}