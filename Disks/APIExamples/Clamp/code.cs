/**
Pixel Vision 8 - Clamp Example
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
    class ClampExample : GameChip
    {

        private int counter;
        private int time;
        private int delay = 300;

        public override void Init()
        {

            // Example Title
            DrawText("Clamp()", 8, 8, DrawMode.TilemapCache, "large", 15);
            DrawText("C Sharp Example", 8, 16, DrawMode.TilemapCache, "medium", 15, -4);

        }

        public override void Update(int timeDelta)
        {
            // Add the time delay to the time
            time = time + timeDelta;

            // Check if time is greater than the delay
            if (time > delay)
            {

                // Increase the counter by 1
                counter = Clamp(counter + 1, 0, 10);

                // Reset the time
                time = 0;

            }
        }

        public override void Draw()
        {

            // Redraw the display
            RedrawDisplay();

            // Draw the counter to the display
            DrawText("Counter " + counter, 8, 32, DrawMode.Sprite, "large", 15);

        }

    }
}