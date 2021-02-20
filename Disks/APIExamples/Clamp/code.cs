/**
Pixel Vision 8 - Clamp Example
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

        private int counter;
        private int time;
        private int delay = 300;

        public override void Update(int timeDelta)
        {
            // Add the time delay to the time
            time = time + timeDelta;

            // Check if time is greater than the delay
            if (time > delay)
            {

                // Increase the counter by 1
                counter = Clamp(counter + 1, 0, 10);

                // Reset the time
                time = 0;

            }
        }

        public override void Draw()
        {

            // Redraw the display
            RedrawDisplay();

            // Draw the counter to the display
            DrawText("Counter " + counter, 8, 8, DrawMode.Sprite, "large", 15);

        }

    }
}