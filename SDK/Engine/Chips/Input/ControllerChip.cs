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

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

//using Microsoft.Xna.Framework.Input;
//using Keys = PixelVisionSDK.Keys;

namespace PixelVision8.Engine.Chips
{
    public enum MouseInput
    {
        LeftButton,
        RightButton,
        MiddleButton,
        Button1,
        Button2,
        None
    }

    public enum InputMap
    {
        Player1UpKey,
        Player1DownKey,
        Player1RightKey,
        Player1LeftKey,
        Player1SelectKey,
        Player1StartKey,
        Player1AKey,
        Player1BKey,
        Player1UpButton,
        Player1DownButton,
        Player1RightButton,
        Player1LeftButton,
        Player1SelectButton,
        Player1StartButton,
        Player1AButton,
        Player1BButton,
        Player2UpKey,
        Player2DownKey,
        Player2RightKey,
        Player2LeftKey,
        Player2SelectKey,
        Player2StartKey,
        Player2AKey,
        Player2BKey,
        Player2UpButton,
        Player2DownButton,
        Player2RightButton,
        Player2LeftButton,
        Player2SelectButton,
        Player2StartButton,
        Player2AButton,
        Player2BButton
    }

    public class ControllerChip : AbstractChip, IControllerChip
    {
        private static readonly int repsPerSec = 20;
        private readonly GamePadDeadZone gamePadDeadZone = GamePadDeadZone.IndependentAxes;

        private readonly StringBuilder inputStringBuilder = new StringBuilder();
        private readonly float timeUntilRepInMillis = 500f;

        //        private Array inputValues;
        //        private Array mouseValues;
        //        private List<int> gamepadIndices = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7 };
        //        private const int KEYBOARD_INDEX = -1;
        //        private const int MOUSE_INDEX = -2;
        public KeyboardState currentKeyboardState;
        public MouseState currentMouseState;
        private DateTime downSince = DateTime.Now;
        private DateTime lastRep = DateTime.Now;

        private List<Controller> players;
        public KeyboardState previousKeyboardState;
        public MouseState previousMouseState;

        private Keys? repChar;

        public void Update(int timeDelta)
        {
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

            // Build the input string
            inputStringBuilder.Clear();

            foreach (var key in (Keys[]) Enum.GetValues(typeof(Keys)))
            {
                if (JustPressed(key))
                {
                    //                    KeyDown?.Invoke(null, new KeyEventArgs(key), keyState);
                    //                    if (KeyPressed != null)
                    //                    {
                    downSince = DateTime.Now;
                    repChar = key;

                    BuildInputString(key);
                    //                        KeyPressed(null, new KeyEventArgs(key), keyState);
                    //                    }
                }
                else if (GetKeyUp(key))
                {
                    //                    if (KeyUp != null)
                    //                    {
                    if (repChar == key) repChar = null;

                    //                        KeyUp(null, new KeyEventArgs(key), keyState);
                    //                    }
                }

                var tmpKey = (Microsoft.Xna.Framework.Input.Keys) (int) key;
                if (repChar != null && repChar == key && currentKeyboardState.IsKeyDown(tmpKey))
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
                            //                            if (KeyPressed != null)
                            //                            {
                            lastRep = now;
                            BuildInputString(key);
                            //                                KeyPressed(null, new KeyEventArgs(key), keyState);
                            //                            }
                        }
                    }
                }
            }

            //            base.Update(gameTime);
        }

        public string ReadInputString()
        {
            return inputStringBuilder.ToString(); //throw new NotImplementedException();
        }

        public bool GetKeyDown(Keys key)
        {
            var tmpKey = (Microsoft.Xna.Framework.Input.Keys) (int) key;

            return currentKeyboardState.IsKeyDown(tmpKey) && previousKeyboardState.IsKeyDown(tmpKey);
        }

        public bool GetKeyUp(Keys key)
        {
            var tmpKey = (Microsoft.Xna.Framework.Input.Keys) (int) key;

            return !currentKeyboardState.IsKeyDown(tmpKey) && previousKeyboardState.IsKeyDown(tmpKey);
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

                var tmpKey = (Microsoft.Xna.Framework.Input.Keys) (int) key;

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

                var tmpKey = (Microsoft.Xna.Framework.Input.Keys) (int) key;

                // Test the keyboard or the controller
                value = currentKeyboardState.IsKeyDown(tmpKey) && previousKeyboardState.IsKeyDown(tmpKey) ||
                        IsPressed(player.CurrentState, button) && IsPressed(player.PreviousState, button);
            }

            return value;
        }


        public void RegisterKeyInput()
        {
            currentKeyboardState = Keyboard.GetState();
            previousKeyboardState = currentKeyboardState;

            var player1 = getPlayer(0);

            var test = engine.GetMetadata(InputMap.Player1UpKey.ToString());
            //            Console.WriteLine("Key test up - "+test + " - " + InputMap.Player1UpKey.ToString());
            //(Keys) Enum.Parse(typeof(Keys), 
            //            player1.GamePadIndex = KEYBOARD_INDEX;

            try
            {
                player1.KeyboardMap = new Dictionary<Buttons, Keys>
                {
                    {Buttons.Up, (Keys) Enum.Parse(typeof(Keys), engine.GetMetadata(InputMap.Player1UpKey.ToString()))},
                    {
                        Buttons.Left,
                        (Keys) Enum.Parse(typeof(Keys), engine.GetMetadata(InputMap.Player1LeftKey.ToString()))
                    },
                    {
                        Buttons.Right,
                        (Keys) Enum.Parse(typeof(Keys), engine.GetMetadata(InputMap.Player1RightKey.ToString()))
                    },
                    {
                        Buttons.Down,
                        (Keys) Enum.Parse(typeof(Keys), engine.GetMetadata(InputMap.Player1DownKey.ToString()))
                    },
                    {
                        Buttons.Select,
                        (Keys) Enum.Parse(typeof(Keys), engine.GetMetadata(InputMap.Player1SelectKey.ToString()))
                    },
                    {
                        Buttons.Start,
                        (Keys) Enum.Parse(typeof(Keys), engine.GetMetadata(InputMap.Player1StartKey.ToString()))
                    },
                    {Buttons.A, (Keys) Enum.Parse(typeof(Keys), engine.GetMetadata(InputMap.Player1AKey.ToString()))},
                    {Buttons.B, (Keys) Enum.Parse(typeof(Keys), engine.GetMetadata(InputMap.Player1BKey.ToString()))}
                };

                var player2 = getPlayer(1);
                //            player2.GamePadIndex = KEYBOARD_INDEX;
                player2.KeyboardMap = new Dictionary<Buttons, Keys>
                {
                    {Buttons.Up, (Keys) Enum.Parse(typeof(Keys), engine.GetMetadata(InputMap.Player2UpKey.ToString()))},
                    {
                        Buttons.Left,
                        (Keys) Enum.Parse(typeof(Keys), engine.GetMetadata(InputMap.Player2LeftKey.ToString()))
                    },
                    {
                        Buttons.Right,
                        (Keys) Enum.Parse(typeof(Keys), engine.GetMetadata(InputMap.Player2RightKey.ToString()))
                    },
                    {
                        Buttons.Down,
                        (Keys) Enum.Parse(typeof(Keys), engine.GetMetadata(InputMap.Player2DownKey.ToString()))
                    },
                    {
                        Buttons.Select,
                        (Keys) Enum.Parse(typeof(Keys), engine.GetMetadata(InputMap.Player2SelectKey.ToString()))
                    },
                    {
                        Buttons.Start,
                        (Keys) Enum.Parse(typeof(Keys), engine.GetMetadata(InputMap.Player2StartKey.ToString()))
                    },
                    {Buttons.A, (Keys) Enum.Parse(typeof(Keys), engine.GetMetadata(InputMap.Player2AKey.ToString()))},
                    {Buttons.B, (Keys) Enum.Parse(typeof(Keys), engine.GetMetadata(InputMap.Player2BKey.ToString()))}
                };
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        //        public void UpdateControllerKey(int controllerID, PixelVisionSDK.ButtonState state)
        //        {
        //            
        //            //throw new NotImplementedException();
        //        }

        public void RegisterControllers()
        {
            var state = GamePad.GetState(0, gamePadDeadZone);
            if (state.IsConnected)
            {
                var player1 = getPlayer(0);
                //                player1.GamePadIndex = 0;
                player1.CurrentState = state;
            }

            state = GamePad.GetState(1, gamePadDeadZone);
            if (state.IsConnected)
            {
                var player2 = getPlayer(1);
                //                player2.GamePadIndex = 1;
                player2.CurrentState = state;
            }
        }
        //        public float DeadzoneSticks = 0.25f;
        //        public float DeadzoneTriggers = 0.25f;
        //        public int PlayerCount = 1;

        public override void Configure()
        {
            engine.ControllerChip = this;

            //            inputValues = Enum.GetValues(typeof(Buttons));
            players = new List<Controller>
            {
                new Controller(),
                new Controller()
            };


            // Setup Mouse
            currentMouseState = Mouse.GetState();
            previousMouseState = currentMouseState;

            //            RegisterKeyInput();
            //            RegisterControllers();
            //            FindNewGamepads();
        }

        public override void Deactivate()
        {
            base.Deactivate();
            engine.ControllerChip = null;
        }

        private void BuildInputString(Keys key)
        {
            inputStringBuilder.Append(GetChar(key, currentKeyboardState.CapsLock, currentKeyboardState.NumLock,
                currentKeyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift) ||
                currentKeyboardState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightShift)));
        }

        private Controller getPlayer(int index)
        {
            return index >= 0 && index < players.Count ? players[index] : null;
        }

        private bool IsPressed(GamePadState state, Buttons input)
        {
            switch (input)
            {
                //                case Buttons.Home:
                //                    return state.Buttons.BigButton == ButtonState.Pressed;
                case Buttons.Start:
                    return state.Buttons.Start == ButtonState.Pressed;
                //                case Buttons.Back:
                //                    return state.Buttons.Back == ButtonState.Pressed;
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
                //                case Buttons.X:
                //                    return state.Buttons.X == ButtonState.Pressed;
                case Buttons.A:
                    return state.Buttons.A == ButtonState.Pressed;
                case Buttons.B:
                    return state.Buttons.B == ButtonState.Pressed;
                //                case Buttons.LeftShoulder:
                //                    return state.Buttons.LeftShoulder == ButtonState.Pressed;
                //                case Buttons.RightShoulder:
                //                    return state.Buttons.RightShoulder == ButtonState.Pressed;
                //                case Buttons.LeftTrigger:
                //                    return state.Triggers.Left > DeadzoneTriggers;
                //                case Buttons.RightTrigger:
                //                    return state.Triggers.Right > DeadzoneTriggers;
                //                case Buttons.LeftStick:
                //                    return state.Buttons.LeftStick == ButtonState.Pressed;
                //                case Buttons.LeftStickUp:
                //                    return state.ThumbSticks.Left.Y > DeadzoneSticks;
                //                case Buttons.LeftStickLeft:
                //                    return state.ThumbSticks.Left.X < -DeadzoneSticks;
                //                case Buttons.LeftStickDown:
                //                    return state.ThumbSticks.Left.Y < -DeadzoneSticks;
                //                case Buttons.LeftStickRight:
                //                    return state.ThumbSticks.Left.X > DeadzoneSticks;
                //                case Buttons.RightStick:
                //                    return state.Buttons.RightStick == ButtonState.Pressed;
                //                case Buttons.RightStickUp:
                //                    return state.ThumbSticks.Right.Y > DeadzoneSticks;
                //                case Buttons.RightStickLeft:
                //                    return state.ThumbSticks.Right.X < -DeadzoneSticks;
                //                case Buttons.RightStickDown:
                //                    return state.ThumbSticks.Right.Y < -DeadzoneSticks;
                //                case Buttons.RightStickRight:
                //                    return state.ThumbSticks.Right.X > DeadzoneSticks;
            }

            return false;
        }

        private bool IsPressed(MouseState state, int input)
        {
            switch (input)
            {
                case 0:
                    return state.LeftButton == ButtonState.Pressed;
                case 1:
                    return state.RightButton == ButtonState.Pressed;
            }

            return false;
        }

        public bool JustPressed(Keys key)
        {
            var tmpKey = (Microsoft.Xna.Framework.Input.Keys) (int) key;

            return currentKeyboardState.IsKeyDown(tmpKey) && !previousKeyboardState.IsKeyDown(tmpKey);
        }

        private string GetChar(Keys key, bool caps, bool num, bool shift)
        {
            if (key.ToString().Length == 1) // TODO: Optmize by checking if its in range instead
            {
                if (shift || caps) return key.ToString();

                return key.ToString().ToLower();
            }

            switch (key)
            {
                case Keys.Space:
                    return " ";
                case Keys.Tilde:
                    return shift ? "~" : "`";
                case Keys.Alpha1:
                    return shift ? "!" : "1";
                case Keys.Alpha2:
                    return shift ? "@" : "2";
                case Keys.Alpha3:
                    return shift ? "#" : "3";
                case Keys.Alpha4:
                    return shift ? "$" : "4";
                case Keys.Alpha5:
                    return shift ? "%" : "5";
                case Keys.Alpha6:
                    return shift ? "^" : "6";
                case Keys.Alpha7:
                    return shift ? "&" : "7";
                case Keys.Alpha8:
                    return shift ? "*" : "8";
                case Keys.Alpha9:
                    return shift ? "(" : "9";
                case Keys.Alpha0:
                    return shift ? ")" : "0";
                case Keys.Minus:
                    return shift ? "_" : "-";
                case Keys.Plus:
                    return shift ? "+" : "=";
                case Keys.OpenBrackets:
                    return shift ? "{" : "[";
                case Keys.CloseBrackets:
                    return shift ? "}" : "]";
                case Keys.Semicolon:
                    return shift ? ":" : ";";
                case Keys.Pipe:
                    return shift ? "|" : "\\";
                case Keys.Quotes:
                    return shift ? "\"" : "'";
                case Keys.Backslash:
                    return shift ? "|" : "\\";
                case Keys.Comma:
                    return shift ? "<" : ",";
                case Keys.Period:
                    return shift ? ">" : ".";
                case Keys.Question:
                    return shift ? "?" : "/";
                case Keys.NumPad0:
                    return num ? "0" : "";
                case Keys.NumPad1:
                    return num ? "1" : "";
                case Keys.NumPad2:
                    return num ? "2" : "";
                case Keys.NumPad3:
                    return num ? "3" : "";
                case Keys.NumPad4:
                    return num ? "4" : "";
                case Keys.NumPad5:
                    return num ? "5" : "";
                case Keys.NumPad6:
                    return num ? "6" : "";
                case Keys.NumPad7:
                    return num ? "7" : "";
                case Keys.NumPad8:
                    return num ? "8" : "";
                case Keys.NumPad9:
                    return num ? "9" : "";
                case Keys.Add:
                    return "+";
                case Keys.Divide:
                    return "/";
                case Keys.Multiply:
                    return "*";
                case Keys.Decimal:
                    return num ? "." : "";
            }

            return string.Empty;
        }

        public bool GetKey(int key)
        {
            return false; //throw new NotImplementedException();
        }

        public void InputMapping(string key, string value = null)
        {
            // TODO need to parse the type

            // Remove player
            key = key.Remove(0, "Player".Length);

            var player = key.Substring(0, "Player2".Length);

            //            getPlayer(0).KeyboardMap
        }

        public override void Shutdown()
        {
            //            Console.WriteLine("Save keys");

            // TODO loop through all the keys and save them back to the bios
            base.Shutdown();
        }

        private class Controller
        {
            //            public int GamePadIndex;
            public GamePadState CurrentState;
            public Dictionary<Buttons, Keys> KeyboardMap;

            public GamePadState PreviousState;

            //            public Dictionary<Buttons, MouseInput> MouseMap;
            public bool IsConnected()
            {
                return CurrentState.IsConnected;
            }
        }

        #region Mouse APIs

        //        private bool mouseInputActive => mouseInput != null;

        public bool GetMouseButtonDown(int id = 0)
        {
            return IsPressed(currentMouseState, id) && IsPressed(previousMouseState, id);
        }

        public bool GetMouseButtonUp(int id = 0)
        {
            return !IsPressed(currentMouseState, id) && IsPressed(previousMouseState, id);
        }

        public Point ReadMousePosition()
        {
            var pos = PointToScreen(currentMouseState.Position);
            return new Point(pos.X, pos.Y);
        }


        //        public void ConvertMousePosition(Vector pos)
        //        {
        //           
        ////            var state = InputStates.CurrMouseState;
        //            var newPoint = PointToScreen(new Point(pos.x, pos.y));
        //            pos.x = newPoint.X;
        //            pos.y = newPoint.Y;
        //            
        ////            return new Vector(pos.X, pos.Y);
        //        }

        private Matrix scaleMatrix = Matrix.CreateScale(1, 1, 1);

        public void MouseScale(float x, float y)
        {
            scaleMatrix = Matrix.CreateScale(x, y, 1.0f);
        }

        //        public Matrix GetScaleMatrix()
        //        {
        ////            var scaleX = (float)GraphicsDeviceManager.DefaultBackBufferWidth / (engine.displayChip.width - engine.displayChip.overscanXPixels);
        ////            var scaleY = (float)GraphicsDeviceManager.DefaultBackBufferHeight/ (engine.displayChip.height - engine.displayChip.overscanYPixels);
        //            return ;
        //        }

        public Point PointToScreen(Point point)
        {
            return PointToScreen(point.X, point.Y);
        }

        public Point PointToScreen(int x, int y)
        {
            //            var viewport = GraphicsDevice.Viewport;
            var vx = x; // - viewport.X;
            var vy = y; // - viewport.Y;
            //            var scaleMatrix = GetScaleMatrix();
            var invertedMatrix = Matrix.Invert(scaleMatrix);
            return Vector2.Transform(new Vector2(vx, vy), invertedMatrix).ToPoint();
        }

        //        private IMouseInput mouseInput;

        //        public void RegisterMouseInput()
        //        {
        //            
        ////            mouseValues = Enum.GetValues(typeof(MouseInput));
        //
        //            
        //            
        ////            mouseInput = target;
        //        }

        #endregion
    }
}