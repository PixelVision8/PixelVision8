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
        
        protected MouseInputChip MouseInputChip => Player.MouseInputChip;
        
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
                ? MouseInputChip.GetMouseButtonUp(button)
                : MouseInputChip.GetMouseButtonDown(button);
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
            return MouseInputChip.ReadMouseWheel();
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
            var pos = MouseInputChip.ReadMousePosition();

            // var bounds = DisplayChip.VisibleBounds;

            // Make sure that the mouse x position is inside of the display width
            if (pos.X < 0 || pos.X > DisplayChip.Width) pos.X = -1;

            // Make sure that the mouse y position is inside of the display height
            if (pos.Y < 0 || pos.Y > DisplayChip.Height) pos.Y = -1;

            return pos;
        }
        
    }
}