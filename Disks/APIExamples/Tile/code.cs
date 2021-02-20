/**
Pixel Vision 8 - Tile Example
Copyright(C) 2017, Pixel Vision 8 (http://pixelvision8.com)
Created by Jesse Freeman(@jessefreeman)

This project was designed to display some basic instructions when you create
a new game.Simply delete the following code and implement your own Init(),
Update() and Draw() logic.

Learn more about making Pixel Vision 8 games at
https://www.pixelvision8.com/getting-started
**/

using Microsoft.Xna.Framework;
using PixelVision8.Player;

namespace PixelVision8.Examples
{
    class TileExample : GameChip
    {
        // Set up a time and delay
        private int time;
        private int delay = 800;

        // This will be the direction value for the transition
        private int dir = 1;

        // Total number of palettes for transition
        private int max = 3;

        // Current palette ID
        private int paletteID;

        // Store the tilemap dimensions
        private Point mapSize;
        private int totalTiles;

        public override void Init()
        {

            // Set the tilemap dimensions
            mapSize = TilemapSize();
            totalTiles = mapSize.X * mapSize.Y;

        }

        public override void Update(int timeDelta)
        {
            // Increase the time on each frame and test if it is greater than the delay
            time = time + timeDelta;

            if (time > delay)
            {

                // Update the palette ID based on the direction
                paletteID += dir;

                // Test if the palette ID is too small or too large and reverse the direction
                if (paletteID >= max)
                {
                    dir = -1;
                }
                else if (paletteID <= 0)
                {
                    dir = 1;
                }

                // Reset the time value
                time = 0;

                // Loop through all of the tiles in the tilemap
                for (int i = 0; i < totalTiles; i++)
                {

                    // Convert the loop index to a column,row position
                    var pos = CalculatePosition((i - 1), mapSize.X);

                    // Get the TileData based on the new position
                    var tmpTile = Tile(pos.X, pos.Y);

                    // Check to see if the tile has a sprite
                    if (tmpTile.spriteID > -1)
                    {

                        // Update the tile by reassigning the same spriteID but calculating a new color offset
                        Tile(pos.X, pos.Y, tmpTile.spriteID, PaletteOffset(paletteID));

                    }
                }
            }
        }

        public override void Draw()
        {
            // Redraw the display
            RedrawDisplay();

            // Draw the text for the palette and color ID
            DrawText("Palette " + paletteID, 32, 16, DrawMode.Sprite, "large", 15);

        }
    }
}