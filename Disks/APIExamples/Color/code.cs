/**
Pixel Vision 8 - Color Example
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

        // Create a delay and set the time to that value so it triggers right away
        private int delay = 500;
        private int time;

        // Create an array of colors and an index value to point to the currently selected color
        private int colorIndex = 1;
        private string[] colors = { "#000000", "#ffffff" };

        public override void Init()
        {
            // Set the time to the delay to force this run on the first frame
            time = delay;

            // Draw a rect with the second color
            DrawRect(8, 24, 32, 32, 1, DrawMode.TilemapCache);

        }

        public override void Update(int timeDelta)
        {
            // Increase the time value base on the timeDelta between the last frame
            time = time + timeDelta;

            // Text to see if time is greater than the delay
            if (time > delay)
            {

                // Increase the color index by 1 and reset if it's greater than the color array
                colorIndex = Repeat(colorIndex + 1, colors.Length);

                // Update the second color value from the array
                Color(1, colors[colorIndex]);

                // Reset the timer
                time = 0;

            }

        }

        public override void Draw()
        {
            // Redraw the display
            RedrawDisplay();

            // Draw a label showing the 2nd colors current HEX value
            DrawText("Color 1 is " + Color(1), 8, 8, DrawMode.Sprite, "large", 15);

        }
    }
}