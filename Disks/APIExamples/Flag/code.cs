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

using Microsoft.Xna.Framework;
using PixelVision8.Player;
using PixelVision8.Engine.Utils;

namespace PixelVision8.Examples
{
    class ExampleGameChip : GameChip
    {
        // This point will store the current tile's position
        private Point tilePosition;

        // This will store the current flag ID
        private int flagID = -1;

        public override void Update(int timeDelta)
        {
            // Get the current mouse position
            tilePosition = MousePosition();

            // Convert the mouse position to the tilemap's correct column and row
            tilePosition.X = MathUtil.FloorToInt(tilePosition.X / 8f);
            tilePosition.Y = MathUtil.FloorToInt(tilePosition.Y / 8f);

            // Get the flag value of the current tile
            flagID = Flag(tilePosition.X, tilePosition.Y);

        }

        public override void Draw()
        {

            // Redraws the display
            RedrawDisplay();

            // Display the tile and flag text on the screen
            DrawText("Tile " + tilePosition.X + "," + tilePosition.Y, 8, 8, DrawMode.Sprite, "large");
            DrawText("Flag " + flagID, 8, 16, DrawMode.Sprite, "large");

            // Draw a rect to represent which tile the mouse is over and set the color to match the flag ID plus 1
            DrawRect(tilePosition.X * 8, tilePosition.Y * 8, 8, 8, flagID + 1, DrawMode.Sprite);

        }
    }
}