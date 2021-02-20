/**
Pixel Vision 8 - Key Example
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
using Microsoft.Xna.Framework.Input;

namespace PixelVision8.Examples
{
    class KeyExample : GameChip
    {
        // List of keys to test for
        private Dictionary<Keys, bool> keyStates = new Dictionary<Keys, bool>()
        {
            {Keys.D0, false},
            {Keys.D1, false},
            {Keys.D2, false},
            {Keys.D3, false},
            {Keys.D4, false},
            {Keys.D5, false},
            {Keys.D6, false},
            {Keys.D7, false},
            {Keys.D8, false},
            {Keys.D9, false}
        };

        public override void Init()
        {

            // Use this counter during the foreach loop below
            var counter = 1;

            // Create labels for all of the keys
            foreach (var keyState in keyStates)
            {
                DrawText("Key " + keyState.Key + " is down ", 1, counter, DrawMode.Tile, "large", 15);
                counter++;
            }

        }

        public override void Update(int timeDelta)
        {

            // Need t o get a list of all the Dictionary's keys so we can iterate over them while updating each value
            var keyNames = new List<Keys>(keyStates.Keys);

            // Loop through all of the number keys and save the current state value
            foreach (Keys keyName in keyNames)
            {
                keyStates[keyName] = Key(keyName);
            }
        }

        public override void Draw()
        {
            // Redraw the display
            RedrawDisplay();

            // Use this counter during the foreach loop below
            var counter = 1;

            // Loop through all the keys and display their current down state
            foreach (var key in keyStates.Keys)
            {
                DrawText(keyStates[key].ToString(), 128 + 36, (counter * 8), DrawMode.Sprite, "large", 14);
                counter++;
            }

        }
    }
}