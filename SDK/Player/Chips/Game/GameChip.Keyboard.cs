//   
// Copyright (c) Jesse Freeman, Pixel Vision 8. All rights reserved.  
//  
// Licensed under the Microsoft Public License (MS-PL) except for a few
// portions of the code. See LICENSE file in the project root for full 
// license information. Third-party libraries used by Pixel Vision 8 are 
// under their own licenses. Please refer to those libraries for details 
// on the license they use.
// 
// Contributors
// --------------------------------------------------------
// This is the official list of Pixel Vision 8 contributors:
//  
// Jesse Freeman - @JesseFreeman
// Christina-Antoinette Neofotistou @CastPixel
// Christer Kaitila - @McFunkypants
// Pedro Medeiros - @saint11
// Shawn Rakowski - @shwany
//

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace PixelVision8.Player
{
    public partial class GameChip
    {
        
        protected KeyboardInputChip KeyboardInputChip => Player.KeyboardInputChip;

        /// <summary>
        ///     While the main form of input in Pixel Vision 8 comes from the controllers, you can test for keyboard
        ///     input by calling the Key() method. When called, this method returns the current state of a key. The
        ///     method accepts the Keys enum, or an int, for a specific key. In additon, you need to provide the input
        ///     state to check for. The InputState enum has two states, Down and Released. By default, Down is
        ///     automatically used which returns true when the key is being pressed in the current frame. When using
        ///     Released, the method returns true if the key is currently up but was down in the last frame.
        /// </summary>
        /// <param name="key">
        ///     This argument accepts the Keys enum or an int for the key's ID.
        /// </param>
        /// <param name="state">
        ///     Optional InputState enum. Returns down state by default. This argument accepts InputState.Down (0)
        ///     or InputState.Released (1).
        /// </param>
        /// <returns>
        ///     This method returns a bool based on the state of the button.
        /// </returns>
        public bool Key(Keys key, InputState state = InputState.Down)
        {
            return state == InputState.Released
                ? KeyboardInputChip.GetKeyUp(key)
                : KeyboardInputChip.GetKeyDown(key);
        }
        
        /// <summary>
        ///     The InputString() method returns the keyboard input entered this frame. This method is
        ///     useful for capturing keyboard text input.
        /// </summary>
        /// <returns>
        ///     A string of all the characters entered during the frame.
        /// </returns>
        public string InputString()
        {
            return KeyboardInputChip.ReadInputString();
        }
        
    }
}