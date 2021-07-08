/**
Pixel Vision 8 - Flag Example
Copyright(C) 2017, Pixel Vision 8 (http://pixelvision8.com)
Created by Jesse Freeman(@jessefreeman)

This project was designed to display some basic instructions when you create
a new game.Simply delete the following code and implement your own Init(),
Update() and Draw() logic.

Learn more about making Pixel Vision 8 games at
https://www.pixelvision8.com/getting-started
**/

using System;

namespace PixelVision8.Player
{
    class FlagExampleGame : GameChip
    {
        // This point will store the current tile's position
        private Point tilePosition;

        // This will store the current flag ID
        private int flagID = -1;

        public override void Init()
        {
            // Example Title
            DrawText("Flag()", 8, 8, DrawMode.TilemapCache, "large", 15);
            DrawText("C Sharp Example", 8, 16, DrawMode.TilemapCache, "medium", 15, -4);
            
            // Change bg color
            BackgroundColor(2);
        }

        public override void Update(int timeDelta)
        {
            // Get the current mouse position
            tilePosition = MousePosition();

            // Check to see if the mouse is out of bounds
            if(tilePosition.X < 0 || tilePosition.X > Display().X || tilePosition.Y < 0 || tilePosition.Y >= Display().Y)
            {

                // Set all of the values to -1
                tilePosition.X = -1;
                tilePosition.Y = -1;
                flagID = -1;

                // Return before the position and flag are calculated
                return;
            }
                
            // Convert the mouse position to the tilemap's correct column and row
            tilePosition.X = (int)Math.Floor(tilePosition.X / 8f);
            tilePosition.Y = (int)Math.Floor(tilePosition.Y / 8f);

            // Get the flag value of the current tile
            flagID = Flag(tilePosition.X, tilePosition.Y);

        }

        public override void Draw()
        {

            // Redraws the display
            RedrawDisplay();

            // Display the tile and flag text on the screen
            DrawText("Tile " + tilePosition.X + "," + tilePosition.Y, 8, 32, DrawMode.Sprite, "large", 15);
            DrawText("Flag " + flagID, 8, 40, DrawMode.Sprite, "large", 15);

            // Draw a rect to represent which tile the mouse is over and set the color to match the flag ID plus 1
            DrawRect(tilePosition.X * 8, tilePosition.Y * 8, 8, 8, flagID + 1, DrawMode.Sprite);

        }
    }
}
