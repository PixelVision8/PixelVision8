/**
Pixel Vision 8 - SongData Example
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
    class SongDataExample : GameChip
    {
        public override void Init()
        {
            // Play the first song with no repeat
            PlaySong(0, false); ;
        }

        public override void Draw()
        {

            // Redraw display
            RedrawDisplay();

            // Reset the next row value so we know where to draw the first line of text
            var nextRow = 2;

            // Draw the song data label
            DrawText("Song Data:", 8, 8, DrawMode.Sprite, "large", 15);

            // Display the song's meta data
            foreach (var data in SongData())
            {

                // Draw the key value pair from the song data table
                DrawText(data.Key + ":", 8, nextRow * 8, DrawMode.Sprite, "large", 6);
                DrawText(data.Value.ToString(), 16 + (data.Key.Length * 8), nextRow * 8, DrawMode.Sprite, "large",
                    14);

                // Increment the row by 1 for the next loop
                nextRow++;

            }
        }
    }
}