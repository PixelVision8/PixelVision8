/**
Pixel Vision 8 - CharacterToPixelData Example
Copyright(C) 2017, Pixel Vision 8 (http://pixelvision8.com)
Created by Jesse Freeman(@jessefreeman)

This project was designed to display some basic instructions when you create
a new game.Simply delete the following code and implement your own Init(),
Update() and Draw() logic.

Learn more about making Pixel Vision 8 games at
https://www.pixelvision8.com/getting-started
**/

using PixelVision8.Engine.Chips;

namespace PixelVision8.Examples
{
    class CharacterToPixelDataExample : GameChip
    {

        private int[] pixelData;

        public override void Init()
        {

            // Get the raw pixel data for the A character
            pixelData = CharacterToPixelData('A', "large");

            // Loop through all of the pixels
            for (int i = 0; i < pixelData.Length; i++)
            {

                // Test to see if the pixel is set to the color ID 0
                if (pixelData[i] == 0)
                {

                    // Change the color ID to 14
                    pixelData[i] = 14;

                }

            }

        }

        public override void Draw()
        {

            // Redraw display
            RedrawDisplay();

            // Use the normal DrawText() API to display the A
            DrawText("A", 8, 8, DrawMode.Sprite, "large", 15);

            // Draw the pixel data to the display next to the A
            DrawPixels(pixelData, 16, 8, 8, 8);

        }
    }
}