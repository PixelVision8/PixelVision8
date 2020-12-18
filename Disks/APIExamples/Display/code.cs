/**
Pixel Vision 8 - Display Example
Copyright(C) 2017, Pixel Vision 8 (http.//pixelvision8.com)
Created by Jesse Freeman(@jessefreeman)

This project was designed to display some basic instructions when you create
a new game.Simply delete the following code and implement your own Init(),
Update() and Draw() logic.

Learn more about making Pixel Vision 8 games at
https.//www.pixelvision8.com/getting-started
**/

using PixelVision8.Engine;
using PixelVision8.Engine.Chips;

namespace PixelVision8.Examples
{
    class DisplayExample : GameChip
    {

        // Save a reference to the canvas
        private Canvas canvas;

        public override void Init()
        {
            // Create a canvas to visualize the screen sizes
            canvas = new Canvas(256, 240, this);

            // Get the full size of the display
            var sizeA = Display(false);

            // Get the visible size of the display
            var sizeB = Display();

            // Draw the two sizes to the display
            DrawText("Full Display Size " + sizeA.X + "x" + sizeB.Y, 1, 1, DrawMode.Tile, "large", 15);
            DrawText("Visible Display Size " + sizeB.X + "x" + sizeB.Y, 1, 2, DrawMode.Tile, "large", 15);

            // Set the canvas stroke to white
            canvas.SetStroke(new int[] { 15 }, 1, 1);

            // Set the fill color to 5 and draw the full size square
            canvas.SetPattern(new int[] { 5 }, 1, 1);
            canvas.DrawSquare(8, 32, sizeA.X / 2 + 8, sizeA.Y / 2 + 32, true);

            // Set the fill color to 0 and draw the visible size square
            canvas.SetPattern(new int[] { 0 }, 1, 1);
            canvas.DrawSquare(8, 32, sizeB.X / 2 + 8, sizeB.Y / 2 + 32, true);

        }

        public override void Draw()
        {
            // Redraw the display
            RedrawDisplay();

            // Draw the canvas to the display
            canvas.DrawPixels();
        }
    }
}