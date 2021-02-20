/**
Pixel Vision 8 - PlaySong Example
Copyright(C) 2017, Pixel Vision 8 (http://pixelvision8.com)
Created by Jesse Freeman(@jessefreeman)

This project was designed to display some basic instructions when you create
a new game.Simply delete the following code && implement your own Init(),
Update() && Draw() logic.

Learn more about making Pixel Vision 8 games at
https://www.pixelvision8.com/getting-started
**/

using PixelVision8.Player;

namespace PixelVision8.Examples
{
    class PlaySongExample : GameChip
    {
        private int isPlaying;

        public override void Update(int timeDelta)
        {
            // Get the playing value
            isPlaying = SongData()["playing"];

            // Test if the left mouse button was released && if isPlaying equals 0
            if (MouseButton(0, InputState.Released) && isPlaying == 0)
            {

                // Play the first song without looping
                PlaySong(0, false);

            }
        }

        public override void Draw()
        {

            // Redraw display
            RedrawDisplay();

            // Reset the next row value so we know where to draw the first line of text
            var nextRow = 2;

            if (isPlaying == 1)
            {

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
            else
            {

                // Draw the instructions label
                DrawText("Click To Play Song", 8, 8, DrawMode.Sprite, "large", 15);

            }
        }
    }
}