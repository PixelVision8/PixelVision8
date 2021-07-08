/**
Pixel Vision 8 - CalculateIndex Example
Copyright(C) 2017, Pixel Vision 8 (http://pixelvision8.com)
Created by Jesse Freeman(@jessefreeman)

This project was designed to display some basic instructions when you create
a new game.Simply delete the following code and implement your own Init(),
Update() and Draw() logic.

Learn more about making Pixel Vision 8 games at
https://www.pixelvision8.com/getting-started
**/


namespace PixelVision8.Player
{
    class CalculateIndexExample : GameChip
    {

        // A 1D array of example values
        private string[] exampleGrid =
        {
            "A", "B", "C",
            "D", "E", "F",
            "G", "H", "I",
        };

        public override void Init()
        {

            // Example Title
            DrawText("CalculateIndex()", 8, 8, DrawMode.TilemapCache, "large", 15);
            DrawText("C Sharp Example", 8, 16, DrawMode.TilemapCache, "medium", 15, -4);

            // Calculate the center index based on a grid with 3 columns
            var index = CalculateIndex(1, 1, 3);

            // Draw the index and value to the display
            DrawText("Position 1,1 is Index " + index + " is " + exampleGrid[index], 1, 4, DrawMode.Tile, "large",
                15);

        }

        public override void Draw()
        {
            // Redraw the display
            RedrawDisplay();
        }

    }
}