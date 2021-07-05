/**
Pixel Vision 8 - TotalSprites Example
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
    class TotalSpritesExample : GameChip
    {
        public override void Init()
        {

            // Example Title
            DrawText("Total Sprites()", 8, 16, DrawMode.TilemapCache, "large", 15);
            DrawText("C Sharp Example", 8, 24, DrawMode.TilemapCache, "medium", 15, -4);

            // Change the BG color to make the sprites easier to see
            BackgroundColor(2);

            // Get total colors values
            var totalSprites = TotalSprites();
            var usedSprites = TotalSprites(true);

            // Display the used vs total colors on the screen
            DrawText("Total Sprites " + usedSprites + "/" + totalSprites, 1, 5, DrawMode.Tile, "large", 15);

            // Loop through all of the sprites in memory and draw them into the tilemap
            for (int i = 0; i < usedSprites; i++)
            {
                var pos = CalculatePosition( i, 16 );
                DrawSprite( i, pos.X + 4, pos.Y + 7, false, false, DrawMode.Tile);
            }

        }

        public override void Draw()
        {
            // Redraw the display
            RedrawDisplay();
        }
    }
}