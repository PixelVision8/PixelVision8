/**
Pixel Vision 8 - NewRect Example
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
    class NewRectExample : GameChip
    {
        // Store the rectangle
        Rectangle rectA;

        // This will store the mouse position
        private Point mousePos;

        // This will store the collision state
        private bool collision = false;

        public override void Init()
        {
            // Use the game's NewRect() to create a rectangle instance
            rectA = NewRect(8, 8, 128, 128);
        }

        public override void Update(int timeDelta)
        {
            // Get the mouse position
            mousePos = MousePosition();

            // Test for the collision
            collision = rectA.Contains(mousePos);

        }

        public override void Draw()
        {

            // Redraw the display
            RedrawDisplay();

            // Draw rectA and change the color if there is a collision
            DrawRect(rectA.X, rectA.Y, rectA.Width, rectA.Height, collision ? 6 : 5);

            // Draw the mouse cursor on the screen
            DrawRect(mousePos.X - 1, mousePos.Y - 1, 2, 2, 15);

        }

    }
}