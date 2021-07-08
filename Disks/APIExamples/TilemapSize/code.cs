/**
Pixel Vision 8 - TilemapSize Example
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
    class TilemapSizeExample : GameChip
    {
        public override void Init()
        {

            // Example Title
            DrawText("TilemapSize()", 8, 8, DrawMode.TilemapCache, "large", 15);
            DrawText("C Sharp Example", 8, 16, DrawMode.TilemapCache, "medium", 15, -4);

            //Get the tilemap size and sprite size
            var mapSize = TilemapSize();
            var tileSize = SpriteSize();

            // Display the tilemap size in tiels and pixels to the screen
            DrawText("Tilemap Size:", 1, 4, DrawMode.Tile, "large", 15);
            DrawText("Tiles " + mapSize.X + " x " + mapSize.Y, 1, 5, DrawMode.Tile, "large", 15);
            DrawText("Total Tiles " + (mapSize.X * mapSize.Y), 1, 6, DrawMode.Tile, "large", 15);
            DrawText("Pixels " + (mapSize.X * tileSize.X) + " x " + (mapSize.Y * tileSize.Y), 1, 7, DrawMode.Tile, "large", 15);

        }

        public override void Draw()
        { 
                // Redraw the display
                RedrawDisplay();
        }
    }
}