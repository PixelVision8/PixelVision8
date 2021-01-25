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


using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using PixelVision8.Player;

namespace PixelVision8.Player
{

    public partial interface IPlayerChips
    {
        /// <summary>
        ///     The controller chip handles all input to the engine. This property
        ///     offers direct access to it.
        /// </summary>
        IControllerChip ControllerChip { get; set; }
    }
    
    

    

    public class Controller
    {

        public GamePadState CurrentState;
        public Dictionary<Buttons, Keys> KeyboardMap;

        public GamePadState PreviousState;

        public bool IsConnected()
        {
            return CurrentState.IsConnected;
        }
    }

    public partial class ControllerChip : AbstractChip, IControllerChip
    {
        private static readonly int repsPerSec = 20;

        private readonly float timeUntilRepInMillis = 500f;

        private DateTime downSince = DateTime.Now;
        private DateTime lastRep = DateTime.Now;

        

        private Keys? repChar;

        

        public void Update(int timeDelta)
        {
            // Build the input string
            // inputStringBuilder.Clear();

            // Save the one and only (if available) keyboardstate 
            previousKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();

            // Save the one and only (if available) mousestate 
            previousMouseState = currentMouseState;
            currentMouseState = Mouse.GetState();

            for (var i = players.Count - 1; i >= 0; i--)
            {
                var player = players[i];
                if (player.IsConnected())
                {
                    // Update gamepad state
                    player.PreviousState = player.CurrentState;
                    player.CurrentState = GamePad.GetState(i, gamePadDeadZone);
                    //                    players[i] = player;//TODO: needed?
                }
            }

            foreach (var key in currentKeyboardState.GetPressedKeys())
            {

                var pv8Key = (Keys)(int)key;

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

                var tmpKey = (Keys)(int)key;

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

        public new bool export { get; set; }

        public bool ButtonReleased(Buttons button, int controllerID = 0)
        {
            // TODO need to test this out
            var value = false;

            var player = getPlayer(controllerID);

            if (player != null)
            {
                Keys key = player.KeyboardMap.TryGetValue(button, out key) ? key : default;

                var tmpKey = (Keys)(int)key;

                // Test the keyboard or the controller
                value = !currentKeyboardState.IsKeyDown(tmpKey) && previousKeyboardState.IsKeyDown(tmpKey) ||
                        !IsPressed(player.CurrentState, button) && IsPressed(player.PreviousState, button);
            }

            return value;
        }

        public bool ButtonDown(Buttons button, int controllerID = 0)
        {
            var value = false;

            var player = getPlayer(controllerID);

            if (player != null)
            {
                Keys key = player.KeyboardMap.TryGetValue(button, out key) ? key : default;

                var tmpKey = (Keys)(int)key;

                // Test the keyboard or the controller
                value = currentKeyboardState.IsKeyDown(tmpKey) && previousKeyboardState.IsKeyDown(tmpKey) ||
                        IsPressed(player.CurrentState, button) && IsPressed(player.PreviousState, button);
            }

            return value;
        }

        

        public override void Configure()
        {
            Player.ControllerChip = this;

            players = new List<Controller>
            {
                new Controller(),
                new Controller()
            };


            // Setup Mouse
            currentMouseState = Mouse.GetState();
            previousMouseState = currentMouseState;

            RegisterControllers();
        }

        public override void Deactivate()
        {
            base.Deactivate();
            Player.ControllerChip = null;
        }

        private Controller getPlayer(int index)
        {
            return index >= 0 && index < players.Count ? players[index] : null;
        }

        private bool IsPressed(GamePadState state, Buttons input)
        {
            switch (input)
            {
                case Buttons.Start:
                    return state.Buttons.Start == ButtonState.Pressed;
                case Buttons.Up:
                    return state.DPad.Up == ButtonState.Pressed;
                case Buttons.Left:
                    return state.DPad.Left == ButtonState.Pressed;
                case Buttons.Down:
                    return state.DPad.Down == ButtonState.Pressed;
                case Buttons.Right:
                    return state.DPad.Right == ButtonState.Pressed;
                case Buttons.Select:
                    return state.Buttons.Back == ButtonState.Pressed;
                case Buttons.A:
                    return state.Buttons.A == ButtonState.Pressed;
                case Buttons.B:
                    return state.Buttons.B == ButtonState.Pressed;
            }

            return false;
        }

    }
}

namespace PixelVision8.Player
{
    public partial class PixelVision
    {
        public IControllerChip ControllerChip { get; set; }
    }
}