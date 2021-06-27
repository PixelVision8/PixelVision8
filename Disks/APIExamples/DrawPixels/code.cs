/**
Pixel Vision 8 - DrawPixels Example
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
        int[] pixelData = {
            -1, -1, -1, 0, 0, 0, 0, -1,
            -1, -1, 0, 0, 14, 14, 0, -1,
            -1, 0, 0, 14, 14, 14, 0, -1,
            -1, 0, 14, 14, 14, 13, 0, -1,
            -1, 0, 0, 0, 14, 13, 0, -1,
            -1, -1, -1, 0, 14, 13, 0, -1,
            -1, -1, -1, 0, 13, 13, 0, -1,
            -1, -1, -1, 0, 0, 0, 0, -1,
        };

        public override void Init()
        {
            
            // Example Title
            DrawText("DrawPixels()", 1, 1, DrawMode.Tile, "large", 15);
            DrawText("C Sharp Example", 8, 16, DrawMode.TilemapCache, "medium", 15, -4);

            // Draw the sprite data to the tilemap cache
            DrawText("Tilemap Cache", 1, 4, DrawMode.Tile, "large", 15);
            DrawPixels(pixelData, 8, 40, 8, 8, false, false, DrawMode.TilemapCache);
            DrawPixels(pixelData, 16, 40, 8, 8, true, false, DrawMode.TilemapCache);
            DrawPixels(pixelData, 24, 40, 8, 8, false, true, DrawMode.TilemapCache);
            DrawPixels(pixelData, 32, 40, 8, 8, true, true, DrawMode.TilemapCache);

            // Shift the pixel data color IDs by 1
            DrawPixels(pixelData, 40, 40, 8, 8, false, false, DrawMode.TilemapCache, 1);

        }

        public override void Draw()
        {
            // Redraw the display
            RedrawDisplay();

            // Label for the sprite layer examples
            DrawText("Sprite", 1, 7, DrawMode.Tile, "large", 15);

            // You can simplify the call if you are not flipping the pixel data
            DrawPixels(pixelData, 8, 64, 8, 8);

            // Fliping the pixel data on the sprite layer, which is used by default when not provided
            DrawPixels(pixelData, 16, 64, 8, 8, true, false);
            DrawPixels(pixelData, 24, 64, 8, 8, false, true);
            DrawPixels(pixelData, 32, 64, 8, 8, true, true);

            // Shift the pixel data color IDs over by 1 requires passing in the draw mode
            DrawPixels(pixelData, 40, 64, 8, 8, false, false, DrawMode.Sprite, 1);

            // Draw pixel data to the sprite below layer
            DrawText("Sprite Below", 1, 10, DrawMode.Tile, "large", 15);
            DrawPixels(pixelData, 8, 88, 8, 8, false, false, DrawMode.SpriteBelow);
            DrawPixels(pixelData, 16, 88, 8, 8, true, false, DrawMode.SpriteBelow);
            DrawPixels(pixelData, 24, 88, 8, 8, false, true, DrawMode.SpriteBelow);
            DrawPixels(pixelData, 32, 88, 8, 8, true, true, DrawMode.SpriteBelow);
            DrawPixels(pixelData, 40, 88, 8, 8, false, false, DrawMode.SpriteBelow, 1);

            // Display the total sprites used during this frame
            DrawText("Total Sprites " + ReadTotalSprites(), 8, 104, DrawMode.Sprite, "large", 14);

        }

    }
}