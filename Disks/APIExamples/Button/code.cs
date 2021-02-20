/**
Pixel Vision 8 - Button Example
Copyright (C) 2017, Pixel Vision 8 (http://pixelvision8.com)
Created by Jesse Freeman (@jessefreeman)

Learn more about making Pixel Vision 8 games at
https://www.pixelvision8.com/getting-started
**/

using PixelVision8.Player;
using System.Collections.Generic;

namespace PixelVision8.Examples
{
    class ExampleGameChip : GameChip
    {
        // This array will store any buttons pressed during the current frame
        private List<string> pressedButtons = new List<string>();

        // A list of all the buttons to check on each frame
        private Buttons[] buttons =
        {
            Buttons.Up,
            Buttons.Down,
            Buttons.Left,
            Buttons.Right,
            Buttons.A,
            Buttons.B,
            Buttons.Select,
            Buttons.Start
        };

        public override void Init()
        {
            DrawText("Button()", 1, 1, DrawMode.Tile, "large", 15);
            DrawText("C Sharp Example", 8, 16, DrawMode.TilemapCache, "medium", 15, -4);
        }

        public override void Update(int timeDelta)
        {

            // Clear the pressedButtons array on each frame
            pressedButtons.Clear();

            // Loop through all the buttons
            for (int i = 0; i < buttons.Length; i++)
            {

                // Test if player 1's current button is down and save it to the pressedButtons array
                if (Button(buttons[i], InputState.Down, 0))
                {
                    pressedButtons.Add(buttons[i].ToString());
                }

            }
        }

        public override void Draw()
        {

            // Clear the display
            RedrawDisplay();

            // Convert the pressedButtons into a string and draw to the display
            var message = string.Join(", ", pressedButtons.ToArray()).ToUpper();
            // DrawText("Buttons Down:", 8, 8, DrawMode.Sprite, "large", 15);
            DrawText(message.Substring(0, message.Length), 8, 24, DrawMode.Sprite, "medium", 14, -4);

        }
    }
}
