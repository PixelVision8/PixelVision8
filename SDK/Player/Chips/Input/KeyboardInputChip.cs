using System;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace PixelVision8.Player
{
    public class KeyboardInputChip : AbstractChip, IUpdate
    {
        public KeyboardState currentKeyboardState;
        public KeyboardState previousKeyboardState;

        private static readonly int repsPerSec = 20;

        private readonly float timeUntilRepInMillis = 500f;

        private DateTime downSince = DateTime.Now;
        private DateTime lastRep = DateTime.Now;


        private Keys? repChar;
        
        private readonly StringBuilder inputStringBuilder = new StringBuilder();

        protected override void Configure()
        {
            Player.KeyboardInputChip = this;
        }

        public void Update(int timeDelta)
        {
            // Build the input string
            // inputStringBuilder.Clear();

            // Save the one and only (if available) keyboardstate 
            previousKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();

            foreach (var key in currentKeyboardState.GetPressedKeys())
            {
                var pv8Key = (Keys) (int) key;

                if (JustPressed(pv8Key))
                {
                    downSince = DateTime.Now;
                    repChar = pv8Key;

                    // BuildInputString(pv8Key);
                }
                else if (GetKeyUp(pv8Key))
                {
                    if (repChar == pv8Key) repChar = null;
                }

                var tmpKey = (Keys) (int) key;

                if (repChar != null && repChar == pv8Key && currentKeyboardState.IsKeyDown(tmpKey))
                {
                    var now = DateTime.Now;
                    var downFor = now.Subtract(downSince);
                    if (downFor.CompareTo(TimeSpan.FromMilliseconds(timeUntilRepInMillis)) > 0)
                    {
                        // Should repeat since the wait time is over now.
                        var repeatSince = now.Subtract(lastRep);
                        if (repeatSince.CompareTo(TimeSpan.FromMilliseconds(1000f / repsPerSec)) > 0)
                        {
                            // Time for another key-stroke.
                            lastRep = now;
                            // BuildInputString(pv8Key);
                        }
                    }
                }
            }

        }

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
        
        public bool JustPressed(Keys key)
        {
            var tmpKey = (Keys) (int) key;

            return currentKeyboardState.IsKeyDown(tmpKey) && !previousKeyboardState.IsKeyDown(tmpKey);
        }
    }
        
    public partial class PixelVision
    {
        public KeyboardInputChip KeyboardInputChip { get; set; }
    }    
}