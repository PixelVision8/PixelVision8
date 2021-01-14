/**
Pixel Vision 8 - CalculateDistance Example
Copyright(C) 2017, Pixel Vision 8 (http://pixelvision8.com)
Created by Jesse Freeman(@jessefreeman)

This project was designed to display some basic instructions when you create
a new game.Simply delete the following code and implement your own Init(),
Update() and Draw() logic.

Learn more about making Pixel Vision 8 games at
https://www.pixelvision8.com/getting-started
**/

using Microsoft.Xna.Framework;
using PixelVision8.Engine;
using PixelVision8.Engine.Chips;

namespace PixelVision8.Examples
{
    class CalculateDistanceExample : GameChip
    {
        private Point pointA = new Point(8, 8);
        private Point pointB = new Point(248, 232);
        private Canvas canvas;
        private int distance;

        public override void Init()
        {

            // Create a new canvas and pass this GameChip into the constructor
            canvas = new Canvas(256, 240, this);

            // Set the canvas stroke to a white 1x1 pixel brush
            canvas.SetStroke(15, 1);

        }

        public override void Update(int timeDelta)
        {

            // // Update position B with the MousePosition
            // pointB = MousePosition();

            // // Calculate the distance between pointA and pointB
            // distance = CalculateDistance(pointA.X, pointA.Y, pointB.X, pointB.X);

        }

        public override void Draw()
        {

            // Redraw the display
            // RedrawDisplay();

            // // Clear the canvas with the background color
            // canvas.Clear(0);

            // // Draw 2 circles around each point
            // canvas.DrawEllipse(pointA.X - 4, pointA.Y - 4, 10, 10);
            // canvas.DrawEllipse(pointB.X - 4, pointB.Y - 4, 10, 10);

            // // Draw a line between the two points
            // canvas.DrawLine(pointA.X, pointA.Y, pointB.X, pointB.Y);

            // // Draw the distance value above pointB
            // canvas.DrawText(distance.ToString(), pointB.X, pointB.Y - 12, "small", 15, -4);

            // // Draw the canvas to the display
            // canvas.DrawPixels();

        }
    }
}