/**
Pixel Vision 8 - ColorsPerSprite Example
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

        // Store the CPS value
        private int cps;

        public override void Init()
        {
            // Get the colors per sprite value
            cps = ColorsPerSprite();
        }

        public override void Draw()
        {
            // Clear the display
            Clear();

            // Draw the cps value to the display
            DrawText("Colors Per Sprite = " + cps, 8, 8, DrawMode.Sprite, "large", 15);

        }
    }
}