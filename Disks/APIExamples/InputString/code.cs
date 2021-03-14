/**
Pixel Vision 8 - InputString Example
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
        // Store the text between frames
        private string inputText = "";

        // Cap on how much text will be displayed
        private int maxCharacters = 30;

        public override void Init()
        {
            // Display the instructions
            DrawText("Start Typing", 1, 1, DrawMode.Tile, "large", 15);

        }

        public override void Update(int timeDelta)
        {
            // Check how long the input text is and clear it if when it gets too long
            if (inputText.Length > maxCharacters)
            {
                inputText = "";
            }

            // Add the current frame's input to the previous frame's text
            inputText = inputText + InputString();

        }

        public override void Draw()
        {
            // Redraw display
            RedrawDisplay();

            // Display the text that has been entered
            DrawText(inputText, 8, 16, DrawMode.Sprite, "large", 14);

        }
    }
}