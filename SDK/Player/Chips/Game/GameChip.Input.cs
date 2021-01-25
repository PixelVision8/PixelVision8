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
        
        // TODO mouse position reads display from GameChip_Display
        
        protected IControllerChip ControllerChip => Player.ControllerChip;
        
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
                ? ControllerChip.GetKeyUp(key)
                : ControllerChip.GetKeyDown(key);
        }

        /// <summary>
        ///     Pixel Vision 8 supports mouse input. You can get the current state of the mouse's left (0) and
        ///     right (1) buttons by calling MouseButton(). In addition to supplying a button ID, you also need
        ///     to provide the InputState enum. The InputState enum contains options for testing the Down and
        ///     Released states of the supplied button ID. By default, Down is automatically used which returns
        ///     true when the key was pressed in the current frame. When using Released, the method returns true
        ///     if the key is currently up but was down in the last frame.
        /// </summary>
        /// <param name="button">
        ///     Accepts an int for the left (0) or right (1) mouse button.
        /// </param>
        /// <param name="state">
        ///     An optional InputState enum. Uses InputState.Down default.
        /// </param>
        /// <returns>
        ///     Returns a bool based on the state of the button.
        /// </returns>
        public bool MouseButton(int button, InputState state = InputState.Down)
        {
            return state == InputState.Released
                ? ControllerChip.GetMouseButtonUp(button)
                : ControllerChip.GetMouseButtonDown(button);
        }

        /// <summary>
        ///     The MouseWheel() method returns an int for how far the scroll wheel has moved since the last frame.
        ///     This value is read-only. Generally speaking, one "tick" of the scroll wheel is 120.
        ///     The returned value is positive for up, and negative for down.
        /// </summary>
        /// <returns>
        ///     Returns a Point representing a vector with how far the scroll wheel has moved since the last frame.
        /// </returns>
        /// 
        public Point MouseWheel()
        {
            return ControllerChip.ReadMouseWheel();
        }

        /// <summary>
        ///     The main form of input for Pixel Vision 8 is the controller's buttons. You can get the current
        ///     state of any button by calling the Button() method and supplying a button ID, an InputState enum,
        ///     and the controller ID. When called, the Button() method returns a bool for the requested button
        ///     and its state. The InputState enum contains options for testing the Down and Released states of
        ///     the supplied button ID. By default, Down is automatically used which returns true when the key
        ///     was pressed in the current frame. When using Released, the method returns true if the key is
        ///     currently up but was down in the last frame.
        /// </summary>
        /// <param name="button">
        ///     Accepts the Buttons enum or int for the button's ID.
        /// </param>
        /// <param name="state">
        ///     Optional InputState enum. Returns down state by default.
        /// </param>
        /// <param name="controllerID">
        ///     An optional InputState enum. Uses InputState.Down default.
        /// </param>
        /// <returns>
        ///     Returns a bool based on the state of the button.
        /// </returns>
        public bool Button(Buttons button, InputState state = InputState.Down, int controllerID = 0)
        {
            return state == InputState.Released
                ? ControllerChip.ButtonReleased(button, controllerID)
                : ControllerChip.ButtonDown(button, controllerID);
        }

        /// <summary>
        ///     The MousePosition() method returns a vector for the current cursor's X and Y position.
        ///     This value is read-only. The mouse's 0,0 position is in the upper left-hand corner of the
        ///     display
        /// </summary>
        /// <returns>
        ///     Returns a vector for the mouse's X and Y poisition.
        /// </returns>
        public Point MousePosition()
        {
            var pos = ControllerChip.ReadMousePosition();

            // var bounds = DisplayChip.VisibleBounds;

            // Make sure that the mouse x position is inside of the display width
            if (pos.X < 0 || pos.X > DisplayChip.Width) pos.X = -1;

            // Make sure that the mouse y position is inside of the display height
            if (pos.Y < 0 || pos.Y > DisplayChip.Height) pos.Y = -1;

            return pos;
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
            return ControllerChip.ReadInputString();
        }

    }
}