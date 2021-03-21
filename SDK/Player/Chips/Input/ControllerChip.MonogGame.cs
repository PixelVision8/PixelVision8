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
using System.Collections.Generic;

namespace PixelVision8.Player
{
   
    public class Controller
    {
        public GamePadState CurrentState;
        public GamePadState PreviousState;
        
        public Dictionary<Buttons, Keys> KeyboardMap;

        public bool IsConnected()
        {
            return CurrentState.IsConnected;
        }
    }

    public class ControllerChip : AbstractChip, IUpdate
    {
        private KeyboardState _currentKeyboardState;
        private KeyboardState _previousKeyboardState;
        
        private readonly Controller[] _controllers = new Controller[]
        {
            
            new Controller()
            {
                
                KeyboardMap = new Dictionary<Buttons, Keys>()
                {
                    {Buttons.Up, Keys.Up},
                    {Buttons.Down, Keys.Down},
                    {Buttons.Right, Keys.Right},
                    {Buttons.Left, Keys.Left},
                    {Buttons.Select, Keys.A},
                    {Buttons.Start, Keys.S},
                    {Buttons.A, Keys.X},
                    {Buttons.B, Keys.C},
                }
                
            },
            
            new Controller()
            {
            
                KeyboardMap = new Dictionary<Buttons, Keys>()
                {
                    {Buttons.Up, Keys.I},
                    {Buttons.Down, Keys.K},
                    {Buttons.Right, Keys.J},
                    {Buttons.Left, Keys.L},
                    {Buttons.Select, Keys.OemSemicolon},
                    {Buttons.Start, Keys.OemComma},
                    {Buttons.A, Keys.Enter},
                    {Buttons.B, Keys.RightShift},
                }
                
            }
            
        };

        public void RegisterControllers()
        {
            var state = GamePad.GetState(0, GamePadDeadZone.IndependentAxes);
            if (state.IsConnected)
            {
                var player1 = GetController(0);
                player1.CurrentState = state;
            }

            state = GamePad.GetState(1, GamePadDeadZone.IndependentAxes);
            if (state.IsConnected)
            {
                var player2 = GetController(1);
                player2.CurrentState = state;
            }
        }
        
        public void Update(int timeDelta)
        {
            // Save the one and only (if available) keyboardstate 
            _previousKeyboardState = _currentKeyboardState;
            _currentKeyboardState = Keyboard.GetState();

            // Save the one and only (if available) mousestate 
            // previousMouseState = currentMouseState;
            // currentMouseState = Mouse.GetState();

            for (var i = _controllers.Length - 1; i >= 0; i--)
            {
                var player = _controllers[i];
                if (player.IsConnected())
                {
                    // Update gamepad state
                    player.PreviousState = player.CurrentState;
                    player.CurrentState = GamePad.GetState(i, GamePadDeadZone.IndependentAxes);
                }
            }
            
        }

        public bool ButtonReleased(Buttons button, int controllerId = 0)
        {
            // TODO need to test this out
            var value = false;

            var player = GetController(controllerId);

            if (player != null)
            {
                Keys key = player.KeyboardMap.TryGetValue(button, out key) ? key : default;

                var tmpKey = (Keys) (int) key;

                // Test the keyboard or the controller
                value = !_currentKeyboardState.IsKeyDown(tmpKey) && _previousKeyboardState.IsKeyDown(tmpKey) ||
                        !IsPressed(player.CurrentState, button) && IsPressed(player.PreviousState, button);
            }

            return value;
        }

        public bool ButtonDown(Buttons button, int controllerId = 0)
        {
            var value = false;

            var player = GetController(controllerId);

            if (player != null)
            {
                Keys key = player.KeyboardMap.TryGetValue(button, out key) ? key : default;

                var tmpKey = (Keys) (int) key;

                // Test the keyboard or the controller
                value = _currentKeyboardState.IsKeyDown(tmpKey) && _previousKeyboardState.IsKeyDown(tmpKey) ||
                        IsPressed(player.CurrentState, button) && IsPressed(player.PreviousState, button);
            }

            return value;
        }


        protected override void Configure()
        {
            Player.ControllerChip = this;

            RegisterControllers();
            
            _currentKeyboardState = Keyboard.GetState();
            _previousKeyboardState = _currentKeyboardState;
            
        }

        public Controller GetController(int index)
        {
            return index >= 0 && index < _controllers.Length ? _controllers[index] : null;
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
    
    public partial class PixelVision
    {
        public ControllerChip ControllerChip { get; set; }
    }
}
