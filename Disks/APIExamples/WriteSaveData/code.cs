/**
Pixel Vision 8 - WriteSaveData Example
Copyright(C) 2017, Pixel Vision 8 (http://pixelvision8.com)
Created by Jesse Freeman(@jessefreeman)

This project was designed to display some basic instructions when you create
a new game.Simply delete the following code and implement your own Init(),
Update() and Draw() logic.

Learn more about making Pixel Vision 8 games at
https://www.pixelvision8.com/getting-started
**/

using PixelVision8.Player;
using System;

namespace PixelVision8.Examples
{
    class WriteSaveDataExample : GameChip
    {
        public override void Init()
        {
            // Draw the last opneded text
            DrawText("Last Opened", 1, 1, DrawMode.Tile, "large", 15);

            // Draw the saved data to the display
            DrawText(ReadSaveData("LastOpened", "Never"), 1, 2, DrawMode.Tile, "large", 14);

        }

        public override void Draw()
        {
            // Redraw the display
            RedrawDisplay();
        }

        // When the game shuts down, it will automatically save the timestamp
        public override void Shutdown()
        {
            // Write timestamp to the saves.json file.
            WriteSaveData("LastOpened", DateTime.Now.ToString());

            //  TODO need a utility to write this to the file system since it's not run in the main engine

        }
    }
}