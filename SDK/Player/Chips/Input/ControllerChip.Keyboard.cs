using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace PixelVision8.Player
{
    public partial class ControllerChip
    {
        public KeyboardState currentKeyboardState;
        public KeyboardState previousKeyboardState;

        private readonly StringBuilder inputStringBuilder = new StringBuilder();

        public void SetInputText(char character, Keys key)
        {
            var value = Convert.ToInt32(character);

            if (value > 31 && value != 127 && value < 169
            ) // TODO we don't support all the characters to need to define a limit
            {
                inputStringBuilder.Append(character);
            }
        }

        public string ReadInputString()
        {
            // Get the text value of the string builder
            var text = inputStringBuilder.ToString();

            // Clear the string builder
            inputStringBuilder.Clear();

            return text;
        }

        public bool GetKeyDown(Keys key)
        {
            var tmpKey = (Keys) (int) key;

            return currentKeyboardState.IsKeyDown(tmpKey) && previousKeyboardState.IsKeyDown(tmpKey);
        }

        public bool GetKeyUp(Keys key)
        {
            var tmpKey = (Keys) (int) key;

            return !currentKeyboardState.IsKeyDown(tmpKey) && previousKeyboardState.IsKeyDown(tmpKey);
        }

        public void RegisterKeyInput(Dictionary<Buttons, Keys> player1Map, Dictionary<Buttons, Keys> player2Map)
        {
            currentKeyboardState = Keyboard.GetState();
            previousKeyboardState = currentKeyboardState;

            var player1 = getPlayer(0);
            player1.KeyboardMap = player1Map;

            var player2 = getPlayer(1);
            player2.KeyboardMap = player2Map;
        }

        public bool JustPressed(Keys key)
        {
            var tmpKey = (Keys) (int) key;

            return currentKeyboardState.IsKeyDown(tmpKey) && !previousKeyboardState.IsKeyDown(tmpKey);
        }
    }

}