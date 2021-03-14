/**
Pixel Vision 8 - ReplaceColor Example
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
    class ReplaceColorExample : GameChip
    {
        // Stores the value of the source color
        private int colorID;

        // Store the total colors
        private int totalColors;

        // Store the target color ID to replace on the first empty color
        private int targetColorID;

        // Keep track of time and delay
        private int delay = 300;
        private int time;


        public override void Init()
        {
            // Set the time equal to the delay to run on the first frame
            time = delay;

            // Get the total colors and ignore any empty ones
            totalColors = TotalColors(true);

            // Set the target color ID
            targetColorID = totalColors + 1;

            // Set the background to the targetColorID
            BackgroundColor(targetColorID);

        }

        public override void Update(int timeDelta)
        {
            // Increase the time each frame and test if time is greater than the delay
            time = time + timeDelta;
            if (time > delay)
            {

                // Increase the color ID by 1 and repeat before reaching the last color
                colorID = Repeat(colorID + 1, totalColors - 1);

                // Replace the target color with another color
                ReplaceColor(targetColorID, colorID);

                // Reset time back to 0
                time = 0;

            }

        }

        public override void Draw()
        {
            // Redraws the display
            RedrawDisplay();

            // Draw the color value to the display
            DrawText("New Color " + Color(targetColorID), 8, 8, DrawMode.Sprite, "large", 15);

        }
    }
}