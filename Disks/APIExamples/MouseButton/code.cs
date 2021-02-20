/**
Pixel Vision 8 - MouseButton Example
Copyright(C) 2017, Pixel Vision 8 (http://pixelvision8.com)
Created by Jesse Freeman(@jessefreeman)

This project was designed to display some basic instructions when you create
a new game.Simply delete the following code and implement your own Init(),
Update() and Draw() logic.

Learn more about making Pixel Vision 8 games at
https://www.pixelvision8.com/getting-started
**/

using PixelVision8.Player;
using System.Collections.Generic;

namespace PixelVision8.Examples
{
    class MouseButtonExample : GameChip
    {
        // This array will store any buttons pressed during the current frame
        List<string> pressedButtons = new List<string>();

        // A list of mouse buttons to check on each frame
        private string[] buttons = { "left", "right" };

        public override void Update(int timeDelta)
        {
            // Clear the pressedButtons array on each frame
            pressedButtons.Clear();

            // Loop through all the buttons
            for (int i = 0; i < buttons.Length; i++)
            {
                // Test if the current mouse button ID is down and saves it to the pressedButtons array
                if (MouseButton(i, InputState.Down))
                {
                    pressedButtons.Add(buttons[i]);
                }
            }

        }

        public override void Draw()
        {
            // Clear the display
            Clear();

            // Convert the pressedButtons into a string and draw to the display
            var message = string.Join(", ", pressedButtons.ToArray()).ToUpper();
            DrawText("Mouse Buttons Down:", 8, 8, DrawMode.Sprite, "large", 15);
            DrawText(message.Substring(0, message.Length), 8, 16, DrawMode.Sprite, "medium", 14, -4);

        }

    }
}