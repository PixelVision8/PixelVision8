/**
Pixel Vision 8 - ScrollPosition Example
Copyright(C) 2017, Pixel Vision 8 (http://pixelvision8.com)
Created by Jesse Freeman(@jessefreeman)

This project was designed to display some basic instructions when you create
a new game.Simply delete the following code and implement your own Init(),
Update() and Draw() logic.

Learn more about making Pixel Vision 8 games at
https://www.pixelvision8.com/getting-started
**/

//using Microsoft.Xna.Framework;
using PixelVision8.Player;

namespace PixelVision8.Examples
{
    class ScrollPositionExample : GameChip
    {
        // Use floats to store the subpixel position
        private float speed = 5;
        private float nextPos;

        // Use this point to position the  sprites
        private Point pos;

        // Store the width of the screen
        private int screenWidth;

        // Store the full width of the map
        private int mapWidth;

        public override void Init()
        {

            // Set the the width of the screen
            screenWidth = Display().X;
            // We need total columns in the tilemap and multiply that by the sprite size to get the full width
            mapWidth = TilemapSize().X * SpriteSize().X;

        }

        public override void Update(int timeDelta)
        {
            // We need to text if the next position plus the screen width is less than the map's width
            if (nextPos + screenWidth < mapWidth)
            {

                // Calculate the next position
                nextPos = nextPos + (speed * (timeDelta / 100f));

                // Need to convert the nextPoint to an int, so we'll save it in a point
                pos.X = (int)nextPos;
                pos.Y = (int)nextPos;

                // Update the scroll position
                ScrollPosition(pos.X);

            }
        }

        public override void Draw()
        {

            // Redraw the display
            RedrawDisplay();

            // Draw the life bar sprite block but ignore the scroll position so it stays fixed on the screen
            DrawSpriteBlock(300, 8, 8, 4, 2, false, false, DrawMode.Sprite, 0, false, false);

            // Draw the exit offscreen and it will become visible when the maps scrolls to the }
            DrawSpriteBlock(104, 432, 88, 4, 4, false, false, DrawMode.Sprite);

        }
    }
}
