/**
Pixel Vision 8 - WordWrap Example
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
    class WordWrapExample : GameChip
    {

        // Message to display on the screen
        private string message = "PIXEL VISION 8\n\nVisit 'pixelvision8.com' to learn more about creating games from scratch.";

        public override void Init()
        {

            // Example Title
            DrawText("WordWrap()", 8, 8, DrawMode.TilemapCache, "large", 15);
            DrawText("C Sharp Example", 8, 16, DrawMode.TilemapCache, "medium", 15, -4);

            // To convert the message into lines of text we need to wrap it then split it
            var wrap = WordWrap(message, (Display().X / 8) - 2);
            var lines = SplitLines(wrap);

            // Loop through each line of text and draw it to the display
            for (int i = 0; i < lines.Length; i++)
            {
                DrawText(lines[i], 1, i + 1 + 4, DrawMode.Tile, "large", 15);
            }

        }

        public override void Draw()
        {
            // Redraw the display
            RedrawDisplay();
        }
    }
}