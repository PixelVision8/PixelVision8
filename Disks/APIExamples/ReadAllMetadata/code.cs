/**
Pixel Vision 8 - ReadAllMetadata Example
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
    class ReadAllMetadataExample : GameChip
    {
        public override void Init()
        {
            // Set up the values we'll use to keep track of the metadata
            var nextRow = 3;
            var maxRows = 29;
            var counter = 0;
            var metadata = ReadAllMetadata();

            // Iterate over the game's metadata
            foreach (var data in metadata)
            {

                // We only display a key/value pair if there is space on the display
                if (nextRow < maxRows)
                {

                    // Draw the key value pair from the game's metadata
                    DrawText(data.Key + ":", 8, nextRow * 8, DrawMode.TilemapCache, "large", 6);
                    DrawText(data.Value, (2 + data.Key.Length) * 8, nextRow * 8, DrawMode.TilemapCache, "large", 14);

                    // Increment the row by 1 for the next loop
                    nextRow++;

                }

                // Keep track of the total amount of metadata keys
                counter++;

            }

            // Display the amount displayed and the total in the game's metadata
            DrawText("Showing " + (nextRow - 3) + " of " + counter + " Metadata Keys", 8, 8, DrawMode.TilemapCache,
                "large", 15);

        }

        public override void Draw()
        {
            // Redraw the display
            RedrawDisplay();
        }
    }
}