/**
Pixel Vision 8 - NewPoint Example
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
    class NewPointExample : GameChip
    {
        // Store the point
        private Point pos;

        public override void Init()
        {
            // Use the game's NewPoint() to create a point instance
            pos = NewPoint();
        }

        public override void Update(int timeDelta)
        {

            // Increase the position by one and have it reset back to 0 if it gets bigger than the display's boundaries
            pos.X = Repeat(pos.X + 1, Display().X);
            pos.Y = Repeat(pos.Y + 1, Display().Y);

        }

        public override void Draw()
        {
            // Redraw the display
            RedrawDisplay();

            // Draw the X and Y value of the position
            DrawText("Position " + pos.X + "," + pos.Y, 8, 8, DrawMode.Sprite, "large", 15);

        }

    }
}