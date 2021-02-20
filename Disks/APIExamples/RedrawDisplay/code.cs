/**
Pixel Vision 8 - RedrawDisplay Example
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
    class RedrawDisplayExample : GameChip
    {
        public override void Init()
        {
            // Draw the FPS label to the tilemap
            DrawText("FPS ", 1, 1, DrawMode.Tile, "large", 15);

        }

        public override void Draw()
        {
            // Redraw the display
            RedrawDisplay();

            // Draw the FPS value to the display on every frame
            DrawText(fps.ToString(), 40, 8, DrawMode.Sprite, "large", 14);
        }
    }
}