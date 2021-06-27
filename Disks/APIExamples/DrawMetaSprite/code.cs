/**
Pixel Vision 8 - DrawMetaSprite Example
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
    public class DrawMetaSpriteExample : GameChip
    {
        // Use floats to store the subpixel position
        private float speed = 5;
        private float nextPos;

        // Use this point to position the  sprites
        private Point pos;

        // Track the animation frame and the total frames
        private int frame = 1;
        private int totalFrames = 4;

        // Track the delay between frames
        private int delay = 100;
        private int time = 0;

        public override void Init()
        {

            // Example Title
            DrawText("DrawMetaSprite()", 1, 1, DrawMode.Tile, "large", 15);
            DrawText("C Sharp Example", 8, 16, DrawMode.TilemapCache, "medium", 15, -4);

        }

        public override void Update(int timeDelta)
        {

            // Increment the time
            time += timeDelta;

            // Check to see if we should change the animation frame
            if(time > delay)
            {
                
                // Reset the timer
                time = 0;

                // Increment the frame
                frame ++;

                // reset the frame counter when out of bounds
                if(frame > totalFrames)
                    frame = 1;

            }

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
            DrawMetaSprite("ladybug-fly-" + frame, pos.X, 24);

            // Draw flipped sprite group moving vertically but render when offscreen
            DrawMetaSprite("ladybug-fly-" + frame, 36, pos.Y, true, false, DrawMode.Sprite, 0);

            // Show the total number of sprites
            DrawText("Sprites " + ReadTotalSprites(), 144 + 24, 224, DrawMode.Sprite, "large", 15);

            // Draw the x,y position of each sprite
            DrawText("(" + pos.X + ",8)", pos.X + 24, 32, DrawMode.Sprite, "large", 15);
            DrawText("(36," + pos.Y + ")", 60, pos.Y + 12, DrawMode.Sprite, "large", 15);

        }
    }
}