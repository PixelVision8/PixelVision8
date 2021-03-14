/**
Pixel Vision 8 - UpdateTiles Example
Copyright(C) 2017, Pixel Vision 8 (http://pixelvision8.com)
Created by Jesse Freeman(@jessefreeman)

This project was designed to display some basic instructions when you create
a new game.Simply delete the following code and implement your own Init(),
Update() and Draw() logic.

Learn more about making Pixel Vision 8 games at
https://www.pixelvision8.com/getting-started
**/

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using PixelVision8.Player;

namespace PixelVision8.Examples
{
    class UpdateTilesExample : GameChip
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

        // Store all of the tiles that should be updated
        int[] tileIDs;

        public override void Init()
        {

            // Set the tilemap dimensions
            mapSize = TilemapSize();
            totalTiles = mapSize.X * mapSize.Y;

            // We'll use a list to store tile IDs
            var tileList = new List<int>();

            // Loop through all of the tiles in the tilemap
            for (int i = 0; i < totalTiles; i++)
            {
           
                // Convert the loop index to a column,row position
                var pos = CalculatePosition(i, mapSize.X);

                // Check to see if the tile has a sprite
                if (Tile(pos.X, pos.Y).spriteID > -1)
                {
                    tileList.Add(i);

                }
            }

            // Convert the tileList into an array
            tileIDs = tileList.ToArray();
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

                // Update each tile's color offset
                UpdateTiles(tileIDs, PaletteOffset(paletteID));
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