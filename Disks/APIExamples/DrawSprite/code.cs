/**
Pixel Vision 8 - DrawSprite Example
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
    public class DrawSpriteExample : GameChip
    {
        // Use floats to store the subpixel position
        private float speed = 5;
        private float nextPos;

        // Use this point to position the  sprites
        private Point pos;

        public override void Init()
        {

            // Example Title
            DrawText("DrawSprite()", 1, 1, DrawMode.Tile, "large", 15);
            DrawText("C Sharp Example", 8, 16, DrawMode.TilemapCache, "medium", 15, -4);

        }

        public override void Update(int timeDelta)
        {

            // Calculate the next position
            nextPos = nextPos + (speed * (timeDelta / 100f));

            // Need to convert the nextPoint to an int, so we'll save it in a point
            pos.X = Repeat( (int)nextPos, Display( ).X );

            pos.Y = Repeat( (int)nextPos, Display( ).Y );

        }

        public override void Draw()
        {
            // Redraw the display
            RedrawDisplay();

            // Draw sprite group moving horizontally and hide when it goes offscreen
            DrawSprite(0, pos.X, 32);

            // Draw flipped sprite group moving vertically but render when offscreen
            DrawSprite(1, 36, pos.Y);

            // Show the total number of sprites
            DrawText("Sprites " + ReadTotalSprites(), 144 + 24, 224, DrawMode.Sprite, "large", 15);

            // Draw the x,y position of each sprite
            DrawText("(" + pos.X + ",8)", pos.X + 8, 32, DrawMode.Sprite, "large", 15);
            DrawText("(36," + pos.Y + ")", 36 + 8, pos.Y, DrawMode.Sprite, "large", 15);

        }
    }
}