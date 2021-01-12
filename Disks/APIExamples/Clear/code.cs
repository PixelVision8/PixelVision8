/**
Pixel Vision 8 - Clear Example
Copyright(C) 2017, Pixel Vision 8 (http://pixelvision8.com)
Created by Jesse Freeman(@jessefreeman)

This project was designed to display some basic instructions when you create
a new game.Simply delete the following code and implement your own Init(),
Update() and Draw() logic.

Learn more about making Pixel Vision 8 games at
https://www.pixelvision8.com/getting-started
**/

using Microsoft.Xna.Framework;
using PixelVision8.Engine.Chips;
using System;

namespace PixelVision8.Examples
{
    class ClearExample : GameChip
    {
        // Create a new random generator
        Random random = new Random();

        // We'll store the display's boundaries here
        private Point display;

        // Create a delay and time value
        private int delay = 2000;
        private int time = 0;

        // This flag will toggle between a full or partial clear
        private bool clearFlag = false;

        // Store random integers for drawing a random character to the screen
        private int charID, x, y, colorID;

        public override void Init()
        {
            // Save the display's boundaries when the game starts up
            display = Display();
        }

        public override void Update(int timeDelta)
        {
            // Increase the time value base on the timeDelta between the last frame
            time = time + timeDelta;

            // Text to see if time is greater than the delay
            if (time > delay)
            {

                // Toggle the clear flag
                clearFlag = true;

                // Reset the timer
                time = 0;

            }
        }

        public override void Draw()
        {
            // Test the clear flag and do a full or partial clear based on the value
            if (clearFlag == true)
            {
                Clear();

                clearFlag = false;
            }
            
            // Perform the next block of code 10 times
            for (int i = 0; i < 10; i++)
            {

                // Assign random values to each of these variable
                charID = random.Next(32, 126);
                x = random.Next(0, display.X);
                y = random.Next(0, display.Y);
                colorID = random.Next(1, 15);

                // Draw a random charIDacter at a random position on the screen with a random color
                DrawText(Convert.ToChar(charID).ToString(), x, y, DrawMode.Sprite, "large", colorID);

            }
        }
    }
}