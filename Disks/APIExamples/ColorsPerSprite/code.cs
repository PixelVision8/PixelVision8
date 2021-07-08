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


namespace PixelVision8.Player
{
    class ColorsPerSpriteExample : GameChip
    {
        public override void Init()
        {

            // Change the background color
            BackgroundColor( 6 );

            // Example Title
            DrawText("ColorsPerSprite()", 8, 8, DrawMode.TilemapCache, "large", 15);
            DrawText("C Sharp Example", 8, 16, DrawMode.TilemapCache, "medium", 15, -4);

            // Draw the cps value to the display
            DrawText("Colors Per Sprite = " + ColorsPerSprite(), 8, 32, DrawMode.TilemapCache, "large", 15);

        }

        public override void Draw()
        {
            // Clear the display
            RedrawDisplay();

            // Draw meta sprite to the display
            DrawMetaSprite( "reaper-boy", 8, 48);
        }
    }
}