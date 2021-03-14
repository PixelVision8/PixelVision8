/**
Pixel Vision 8 - Sound Example
Copyright(C) 2017, Pixel Vision 8 (http://pixelvision8.com)
Created by Jesse Freeman(@jessefreeman)

This project was designed to display some basic instructions when you create
a new game.Simply delete the following code and implement your own Init(),
Update() and Draw() logic.

Learn more about making Pixel Vision 8 games at
https://www.pixelvision8.com/getting-started
**/

using PixelVision8.Player;
using System.Collections.Generic;

namespace PixelVision8.Examples
{
    class SoundExample : GameChip
    {

        // Stores the current wave type
        private int waveType;

        // Stores all the sound effect properties
        List<string> soundProps = new List<string>();

        public override void Init()
        {

            // Add label text
            DrawText("Click to Play Sound", 1, 1, DrawMode.Tile, "large", 15);
            DrawText("WaveType", 1, 2, DrawMode.Tile, "large", 15);

            // Read first sound effect
            var soundData = Sound(0);

            // Create a temp value for the parser
            var tmpValue = "";

            // Loop through all the of the characters in the soundData string
            for (int i = 0; i < soundData.Length; i++)
            {

                // Get a single character from the soundData string
                var c = soundData.Substring(i, 1);

                // If the character is a comma
                if (c == ",")
                {

                    // Add the current string of characters to the next table position
                    soundProps.Add(tmpValue);

                    // Reset the tmpValue
                    tmpValue = "";

                }
                else
                {

                    // Concatenate the current character with the previous ones in the tmpValue
                    tmpValue += c;

                }
            }

            // Always add the last value since it doesn't } in a comma
            soundProps.Add(tmpValue);

        }

        public override void Update(int timeDelta)
        {

            // Test to see if the mouse button was released and the sound is not playing
            if (MouseButton(0, InputState.Released) && IsChannelPlaying(0) == false)
            {

                // Update the waveType value
                waveType = Repeat(waveType + 1, 5);
                soundProps[0] = waveType.ToString();

                // Save the new sound data
                Sound(0, string.Join(",", soundProps));

                // Play the first sound on channel 0
                PlaySound(0);

            }
        }

        public override void Draw()
        {
            // Redraw the display
            RedrawDisplay();

            // Draw the wavetype ID
            DrawText(waveType.ToString(), 80, 16, DrawMode.Sprite, "large", 14);

        }
    }
}