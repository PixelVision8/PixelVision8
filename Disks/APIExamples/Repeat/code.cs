/**
Pixel Vision 8 - Repeat Example
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
    class RepeatExample : GameChip
    {
        // Store the counter value and max value
        private int counter;
        private int counterMax = 500;

        public override void Init()
        {
            // Example Title
            DrawText("Repeat()", 8, 8, DrawMode.TilemapCache, "large", 15);
            DrawText("C Sharp Example", 8, 16, DrawMode.TilemapCache, "medium", 15, -4);
        }

        public override void Update(int timeDelta)
        {
            // Increase the counter by 1 every frame
            counter = Repeat(counter + 1, counterMax);

        }

        public override void Draw()
        {
            // Redraw display
            RedrawDisplay();

            // Draw the counter value to the display
            DrawText("Counter " + counter + "/" + counterMax, 8, 32, DrawMode.Sprite, "large", 15);

        }
    }
}