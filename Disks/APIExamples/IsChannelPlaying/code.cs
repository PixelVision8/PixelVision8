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

using Microsoft.Xna.Framework.Input;
using PixelVision8.Player;

namespace PixelVision8.Examples
{
    class IsChannelPlayingExample : GameChip
    {
        public override void Init()
        {
            // Display the instructions
            DrawText("Press the 1 or 2 key", 1, 1, DrawMode.Tile, "large", 15);
            DrawText("Channel 0 is playing ", 1, 2, DrawMode.Tile, "large", 15);
            DrawText("Channel 1 is playing ", 1, 3, DrawMode.Tile, "large", 15);

        }

        public override void Update(int timeDelta)
        {
            // Check for the 1 key to be pressed and play sound ID 0 on channel 0
            if (Key(Keys.D1, InputState.Released))
            {
                PlaySound(0, 0);
            }

            // Only play sound 1 if the channel is not currently playing a sound
            if (Key(Keys.D2, InputState.Released) & IsChannelPlaying(1) == false)
            {
                PlaySound(1, 1);
            }

        }

        // The Draw() method is part of the game's life cycle. It is called after Update() and is where
        // all of our draw calls should go.We'll be using this to render sprites to the display.
        public override void Draw()
        {

            // Redraw the display
            RedrawDisplay();

            // Draw channel 0 and 1's current playing state to the display
            DrawText(IsChannelPlaying(0).ToString(), 176, 16, DrawMode.Sprite, "large", 14);
            DrawText(IsChannelPlaying(1).ToString(), 176, 24, DrawMode.Sprite, "large", 14);

        }
    }
}