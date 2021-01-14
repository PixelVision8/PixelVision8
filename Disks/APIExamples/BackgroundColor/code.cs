/**
Pixel Vision 8 - BackgroundColor Example
Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
Created by Jesse Freeman (@jessefreeman)

Learn more about making Pixel Vision 8 games at
https://www.pixelvision8.com/getting-started
**/

using PixelVision8.Engine.Chips;

namespace PixelVision8.Examples
{

    public class BackgroundColorExample : GameChip
    {
        public override void Init()
        {
            // Example Title
            DrawText("BackgroundColor()", 1, 1, DrawMode.Tile, "large", 15);
            DrawText("C Sharp Example", 8, 16, DrawMode.TilemapCache, "medium", 15, -4);

            //  Get the current background color
            var defaultColor = BackgroundColor();

            // Draw the default background color ID to the display
            DrawText("Default Color " + defaultColor, 1, 4, DrawMode.Tile, "large", 15);

            //  Here we are manually changing the background color
            var newColor = BackgroundColor(2);

            //  Draw the new color ID to the display
            DrawText("New Color " + newColor, 1, 6, DrawMode.Tile, "large", 15);
        }

        public override void Draw()
        {
            //Redraw the display
            RedrawDisplay();
        }

    }
}
