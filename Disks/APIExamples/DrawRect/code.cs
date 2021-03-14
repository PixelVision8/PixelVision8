/**
Pixel Vision 8 - DrawRect Example
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
        public override void Init()
        {
            // Draw a 100 x 100 pixel rect to the display
            DrawRect(16, 16, 100, 100, 5, DrawMode.TilemapCache);

        }

        public override void Draw()
        {
            // Redraw the display
            RedrawDisplay();

            // Draw a rect to the sprite layer
            DrawRect(12, 12, 25, 25, 14, DrawMode.Sprite);

            // Draw a rect to the sprite below layer
            DrawRect(100, 100, 25, 25, 15, DrawMode.SpriteBelow);

        }

    }
}