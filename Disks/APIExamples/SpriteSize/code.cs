/**
Pixel Vision 8 - SpriteSize Example
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
    class SpriteSizeExample : GameChip
    {
        public override void Init()
        {
            // Get the sprite size
            var spriteSize = SpriteSize();

            // Draw the sprite size to the display
            DrawText("Sprite Size: " + spriteSize.X + "x" + spriteSize.Y, 1, 1, DrawMode.Tile, "large", 15);

        }

        public override void Draw()
        {
            // Redraw the display
            RedrawDisplay();
        }
    }
}