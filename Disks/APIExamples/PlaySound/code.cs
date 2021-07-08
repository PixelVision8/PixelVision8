/**
Pixel Vision 8 - PlaySound Example
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
    class PlaySoundExample : GameChip
    {

        // Store the playback state of channel 0
        private bool isPlaying = false;

        public override void Init()
        {
            // Example Title
            DrawText("PlaySound()", 8, 8, DrawMode.TilemapCache, "large", 15);
            DrawText("C Sharp Example", 8, 16, DrawMode.TilemapCache, "medium", 15, -4);
        }

        public override void Update(int timeDelta)
        {
            // See if the channel has audio playing back on it
            isPlaying = IsChannelPlaying(0);

            // Test if the left mouse button was released and if isPlaying equals false
            if (MouseButton(0, InputState.Released) && isPlaying == false)
            {

                // Play the second sound effect
                PlaySound(1);

            }
        }

        public override void Draw()
        {

            // Redraw display
            RedrawDisplay();

            // Test to see if the sound effect is playing
            if (isPlaying)
            {

                // Draw the sound playback label
                DrawText("Playing On Channel 0", 8, 32, DrawMode.Sprite, "large", 14);

            }
            else
            {

                // Draw the instructions label
                DrawText("Click To Play Sound Effect", 8, 32, DrawMode.Sprite, "large", 15);

            }
        }
    }
}