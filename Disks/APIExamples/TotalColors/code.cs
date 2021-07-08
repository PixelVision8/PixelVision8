/**
Pixel Vision 8 - TotalColors Example
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
    class TotalColorsExample : GameChip
    {
        public override void Init()
        {

            // Example Title
            DrawText("TotalColors()", 8, 8, DrawMode.TilemapCache, "large", 15);
            DrawText("C Sharp Example", 8, 16, DrawMode.TilemapCache, "medium", 15, -4);

            // Get total colors values
            var totalColors = TotalColors();
            var usedColors = TotalColors(true);

            // Display the used vs total colors on the screen
            DrawText("Total Colors " + usedColors + "/" + totalColors, 1, 4, DrawMode.Tile, "large", 15);
        }

        public override void Draw()
        {
            // Redraw the display
            RedrawDisplay();
        }

    }
}