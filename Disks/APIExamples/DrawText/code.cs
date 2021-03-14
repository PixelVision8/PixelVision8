/**
Pixel Vision 8 - DrawText Example
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
    class ExampleGameChip : GameChip
    {
        public override void Init()
        {
            // Draw the large font as tiles
            DrawText("Large Font - Tiles", 1, 1, DrawMode.Tile, "large", 5);

            // Draw the medium font to the tilemap cache and change the letter spacing
            DrawText("Medium Font - Tilemap Cache", 8, 16, DrawMode.TilemapCache, "medium", 15, -3);

        }

        public override void Draw()
        {
            // Redraw the display
            RedrawDisplay();

            // Draw the small font as sprites and change the letter spacing
            DrawText("Small Font - Sprites", 8, 24, DrawMode.Sprite, "small", 14, -4);

        }

    }
}