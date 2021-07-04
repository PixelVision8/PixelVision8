/**
Pixel Vision 8 - NewCanvas Example
Copyright(C) 2017, Pixel Vision 8 (http://pixelvision8.com)
Created by Jesse Freeman(@jessefreeman)

This project was designed to display some basic instructions when you create
a new game.Simply delete the following code and implement your own Init(),
Update() and Draw() logic.

Learn more about making Pixel Vision 8 games at
https://www.pixelvision8.com/getting-started
**/

using PixelVision8.Player;

namespace PixelVision8.Examples
{
    class NewCanvasExample : GameChip
    {
        // Store a reference to the canvas here
        private Canvas canvas;

        // This will be our solid pattern value
        private int[] solidPattern = { 5 };

        // This will be our checkered pattern value
        private int[] checkeredPattern =
        {
            5, 0, 5, 0,
            0, 5, 0, 5,
            5, 0, 5, 0,
            0, 5, 0, 5
        };

        public override void Init()
        {
            
            // Create a new canvas the size of the display
            canvas = NewCanvas(Display().X, Display().Y);

            // Set the canvas stroke to a 1x1 pixel brush
            canvas.SetStroke(6, 1);

            // Draw the line label to the canvas
            canvas.DrawText("Line", 8, 48, "large", 15);

            // Draw a line between the two points
            canvas.DrawLine(8, 64, 80, 64);

            // Draw a line between the two points
            canvas.DrawLine(93, 64, 165, 72);

            // Draw the Ellipse label to the canvas
            canvas.DrawText("Ellipse", 8, 80, "large", 15);

            // Ellipse 1
            canvas.DrawEllipse(8, 92, 64, 64);

            // Ellipse 2
            canvas.SetPattern(solidPattern, 1, 1);
            canvas.DrawEllipse(79, 92, 64, 64, true);

            // Ellipse 3
            canvas.SetPattern(checkeredPattern, 4, 4);
            canvas.DrawEllipse(150, 92, 64, 64, true);

            // Draw the Rectangle label to the canvas
            canvas.DrawText("Rectangle", 8, 160, "large", 15);

            // Rectangle 1
            canvas.DrawRectangle(8, 176, 64, 64);

            // Rectangle 2
            canvas.SetPattern(solidPattern, 1, 1);
            canvas.DrawRectangle(79, 176, 64, 64, true);

            // Rectangle 3
            canvas.SetPattern(checkeredPattern, 4, 4);
            canvas.DrawRectangle(150, 176, 64, 64, true);

            // Draw the canvas to the display
            canvas.DrawPixels();

            // Example Title
            DrawText("NewCanvas()", 8, 16, DrawMode.TilemapCache, "large", 15);
            DrawText("C Sharp Example", 8, 24, DrawMode.TilemapCache, "medium", 15, -4);

        }

        public override void Draw()
        {
            // Redraw the display
            RedrawDisplay();
        }

    }
}