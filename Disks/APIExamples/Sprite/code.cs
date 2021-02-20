/**
Pixel Vision 8 - Sprite Example
Copyright(C) 2017, Pixel Vision 8 (http://pixelvision8.com)
Created by Jesse Freeman(@jessefreeman)

This project was designed to display some basic instructions when you create
a new game.Simply delete the following code and implement your own Init(),
Update() and Draw() logic.

Learn more about making Pixel Vision 8 games at
https://www.pixelvision8.com/getting-started
**/

using PixelVision8.Player;
using System;

namespace PixelVision8.Examples
{
    class SpriteExample : GameChip
    {
        private int delay = 500;
        private int time;

        // Create new random instance
        private Random random = new Random();

        public override void Init()
        {
            // Set time to delay so this is triggered on the first frame
            time = delay;
        }

        public override void Update(int timeDelta)
        {
            time += timeDelta;

            if (time > delay)
            {

                // Get the first sprite's pixel data
                var pixelData = Sprite(0);

                // Loop through all of the pixels
                for (int i = 0; i < pixelData.Length; i++)
                {

                    // Set a random pixel color
                    pixelData[i] = random.Next(0, 15);

                }

                // Save the pixel data back
                Sprite(0, pixelData);

                time = 0;
            }

        }

        public override void Draw()
        {
            // Redraw the display
            RedrawDisplay();
        }
    }
}