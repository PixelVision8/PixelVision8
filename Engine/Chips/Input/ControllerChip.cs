using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PixelVisionSDK.Chips
{

    public enum MouseInput
    {
        LeftButton,
        RightButton,
        MiddleButton,
        Button1,
        Button2,
        None,
    }

    public class ControllerChip : AbstractChip, IControllerChip
    {
        private class Controller
        {
//            public int GamePadIndex;
            public GamePadState CurrentState;
            public GamePadState PreviousState;
            public Dictionary<Buttons, Keys> KeyboardMap;
            public Dictionary<Buttons, MouseInput> MouseMap;
            public bool IsConnected()
            {
                return CurrentState.IsConnected;
            }
        }
        
        private List<Controller> players;
        private GamePadDeadZone gamePadDeadZone = GamePadDeadZone.IndependentAxes;
//        private Array inputValues;
//        private Array mouseValues;
//        private List<int> gamepadIndices = new List<int> { 0, 1, 2, 3, 4, 5, 6, 7 };
//        private const int KEYBOARD_INDEX = -1;
//        private const int MOUSE_INDEX = -2;
        public KeyboardState currentKeyboardState;
        public KeyboardState previousKeyboardState;
        public MouseState currentMouseState;
        public MouseState previousMouseState;
//        public float DeadzoneSticks = 0.25f;
//        public float DeadzoneTriggers = 0.25f;
//        public int PlayerCount = 1;

        public override void Configure()
        {
            engine.controllerChip = this;
            
//            inputValues = Enum.GetValues(typeof(Buttons));
            players = new List<Controller>()
            {
                new Controller(),
                new Controller()
            };

            
            // Setup Mouse
            currentMouseState = Mouse.GetState();
            previousMouseState = currentMouseState;
            
            RegisterKeyInput();
            RegisterControllers();
//            FindNewGamepads();
        }
        
        public override void Deactivate()
        {
            base.Deactivate();
            engine.controllerChip = null;
        }

        private Keys? repChar;
        private DateTime lastRep = DateTime.Now;
        private DateTime downSince = DateTime.Now;
        private float timeUntilRepInMillis = 500f;
        private static int repsPerSec = 20;
        
        public void Update(float timeDelta)
        {
            // Save the one and only (if available) keyboardstate 
            previousKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();
            
            // Save the one and only (if available) mousestate 
            previousMouseState = currentMouseState;
            currentMouseState = Mouse.GetState();

            for (int i = players.Count - 1; i >= 0; i--)
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
            
            foreach (Keys key in (Keys[]) Enum.GetValues(typeof(Keys)))
            {
                
                if (JustPressed(key))
                {
//                    KeyDown?.Invoke(null, new KeyEventArgs(key), keyState);
//                    if (KeyPressed != null)
//                    {
                    downSince = DateTime.Now;
                    repChar = key;
                
                    BuildInputStreang(key);
//                        KeyPressed(null, new KeyEventArgs(key), keyState);
//                    }
                }
                else if (GetKeyUp(key))
                {
//                    if (KeyUp != null)
//                    {
                    if (repChar == key)
                    {
                        repChar = null;
                    }

//                        KeyUp(null, new KeyEventArgs(key), keyState);
//                    }
                }

                if (repChar != null && repChar == key && currentKeyboardState.IsKeyDown(key))
                {
                    DateTime now = DateTime.Now;
                    TimeSpan downFor = now.Subtract(downSince);
                    if (downFor.CompareTo(TimeSpan.FromMilliseconds(timeUntilRepInMillis)) > 0)
                    {
                        // Should repeat since the wait time is over now.
                        TimeSpan repeatSince = now.Subtract(lastRep);
                        if (repeatSince.CompareTo(TimeSpan.FromMilliseconds(1000f / repsPerSec)) > 0)
                        {
                            // Time for another key-stroke.
//                            if (KeyPressed != null)
//                            {
                            lastRep = now;
                            BuildInputStreang(key);
//                                KeyPressed(null, new KeyEventArgs(key), keyState);
//                            }
                        }
                    }
                }
                
//                if (IsPressed(key))
//                {
//                    inputStringBuilder.Append(GetChar(key, currentKeyboardState.CapsLock, currentKeyboardState.NumLock, currentKeyboardState.IsKeyDown(Keys.LeftShift) || currentKeyboardState.IsKeyDown(Keys.RightShift)));
//                }
            }
//            base.Update(gameTime);
        }

        private void BuildInputStreang(Keys key)
        {
            inputStringBuilder.Append(GetChar(key, currentKeyboardState.CapsLock, currentKeyboardState.NumLock, currentKeyboardState.IsKeyDown(Keys.LeftShift) || currentKeyboardState.IsKeyDown(Keys.RightShift)));
        }
        
        private Controller getPlayer(int index)
        {
            return index >= 0 && index < players.Count ? players[index] : null;
        }

//        public bool IsPressed(Keys key)
//        {
//            return currentKeyboardState.IsKeyDown(key);
//        }
//        public bool IsPressed(MouseInput input)
//        {
//            return IsPressed(currentMouseState, input);
//        }
//        public bool IsPressed(Buttons button, int index)
//        {
//            var player = getPlayer(index);
//            if (player != null && player.GamePadIndex >= 0)
//                return player.CurrentState.IsButtonDown(button);
//            return false;
//        }
//        public bool IsPressed(Buttons input)
//        {
//            for (int i = 0; i < players.Count; i++)
//                if (IsPressed(input, i))
//                    return true;
//            return false;
//        }
//        public bool IsPressed(Buttons input, int index)
//        {
//            var player = getPlayer(index);
//            if (player != null)
//            {
//                switch (player.GamePadIndex)
//                {
//                    case KEYBOARD_INDEX:
//                        Keys key = player.KeyboardMap.TryGetValue(input, out key) ? key : default(Keys);
//                        return currentKeyboardState.IsKeyDown(key);
//                    case MOUSE_INDEX:
//                        MouseInput mouse = player.MouseMap.TryGetValue(input, out mouse) ? mouse : default(MouseInput);
//                        return IsPressed(currentMouseState, mouse);
//                    default:
//                        return IsPressed(player.CurrentState, input);
//                }
//            }
//            return false;
//        }
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

//        public bool IsHeld(Keys key)
//        {
//            return ;
//        }
//        public bool IsHeld(MouseInput input)
//        {
//            return IsPressed(currentMouseState, input) && IsPressed(previousMouseState, input);
//        }
//        public bool IsHeld(Buttons button, int index)
//        {
//            var player = getPlayer(index);
//            if (player != null && player.GamePadIndex >= 0)
//                return player.CurrentState.IsButtonDown(button) && player.PreviousState.IsButtonDown(button);
//            return false;
//        }
//        public bool IsHeld(Buttons input)
//        {
//            for (int i = 0; i < players.Count; i++)
//                if (IsHeld(input, i))
//                    return true;
//            return false;
//        }
//        public bool IsHeld(Buttons input, int index)
//        {
//            var player = getPlayer(index);
//            if (player != null)
//            {
//                switch (player.GamePadIndex)
//                {
//                    case KEYBOARD_INDEX:
//                        Keys key = player.KeyboardMap.TryGetValue(input, out key) ? key : default(Keys);
//                        return currentKeyboardState.IsKeyDown(key) && previousKeyboardState.IsKeyDown(key);
//                    case MOUSE_INDEX:
//                        MouseInput mouse = player.MouseMap.TryGetValue(input, out mouse) ? mouse : default(MouseInput);
//                        return IsPressed(currentMouseState, mouse) && IsPressed(currentMouseState, mouse);
//                    default:
//                        return IsPressed(player.CurrentState, input) && IsPressed(player.PreviousState, input);
//                }
//            }
//            return false;
//        }

        public bool JustPressed(Keys key)
        {
            return currentKeyboardState.IsKeyDown(key) && !previousKeyboardState.IsKeyDown(key);
        }
//        public bool JustPressed(MouseInput input)
//        {
//            return IsPressed(currentMouseState, input) && !IsPressed(previousMouseState, input);
//        }
//        public bool JustPressed(Buttons button, int index)
//        {
//            var player = getPlayer(index);
//            if (player != null && player.GamePadIndex >= 0)
//                return player.CurrentState.IsButtonDown(button) && !player.PreviousState.IsButtonDown(button);
//            return false;
//        }
//        public bool JustPressed(Buttons input)
//        {
//            for (int i = 0; i < players.Count; i++)
//                if (JustPressed(input, i))
//                    return true;
//            return false;
//        }
//        public bool JustPressed(Buttons input, int index)
//        {
//            var player = getPlayer(index);
//            if (player != null)
//            {
//                switch (player.GamePadIndex)
//                {
//                    case KEYBOARD_INDEX:
//                        Keys key = player.KeyboardMap.TryGetValue(input, out key) ? key : default(Keys);
//                        return currentKeyboardState.IsKeyDown(key) && !previousKeyboardState.IsKeyDown(key);
//                    case MOUSE_INDEX:
//                        MouseInput mouse = player.MouseMap.TryGetValue(input, out mouse) ? mouse : default(MouseInput);
//                        return IsPressed(currentMouseState, mouse) && !IsPressed(currentMouseState, mouse);
//                    default:
//                        return IsPressed(player.CurrentState, input) && !IsPressed(player.PreviousState, input);
//                }
//            }
//            return false;
//        }

//        public bool JustReleased(Keys key)
//        {
//            return !currentKeyboardState.IsKeyDown(key) && previousKeyboardState.IsKeyDown(key);
//        }
//        public bool JustReleased(MouseInput input)
//        {
//            return !IsPressed(currentMouseState, input) && IsPressed(previousMouseState, input);
//        }
//        public bool JustReleased(Buttons button, int index)
//        {
//            var player = getPlayer(index);
//            if (player != null && player.GamePadIndex >= 0)
//                return !player.CurrentState.IsButtonDown(button) && player.PreviousState.IsButtonDown(button);
//            return false;
//        }
//        public bool JustReleased(Buttons input)
//        {
//            for (int i = 0; i < players.Count; i++)
//                if (JustReleased(input, i))
//                    return true;
//            return false;
//        }
//        public bool JustReleased(Buttons input, int index)
//        {
//            var player = getPlayer(index);
//            if (player != null)
//            {
//                switch (player.GamePadIndex)
//                {
//                    case KEYBOARD_INDEX:
//                        Keys key = player.KeyboardMap.TryGetValue(input, out key) ? key : default(Keys);
//                        return !currentKeyboardState.IsKeyDown(key) && previousKeyboardState.IsKeyDown(key);
//                    case MOUSE_INDEX:
//                        MouseInput mouse = player.MouseMap.TryGetValue(input, out mouse) ? mouse : default(MouseInput);
//                        return !IsPressed(currentMouseState, mouse) && IsPressed(currentMouseState, mouse);
//                    default:
//                        return !IsPressed(player.CurrentState, input) && IsPressed(player.PreviousState, input);
//                }
//            }
//            return false;
//        }
//
//        public bool SomethingDown()
//        {
//            for (int i = 0; i < players.Count; i++)
//                if (SomethingDown(i))
//                    return true;
//            return false;
//        }
//        public bool SomethingDown(int index)
//        {
//            var player = getPlayer(index);
//            if (player != null)
//            {
//                switch (player.GamePadIndex)
//                {
//                    case KEYBOARD_INDEX:
//                        return currentKeyboardState.GetPressedKeys().Length > 0;
//                    case MOUSE_INDEX:
//                        foreach (MouseInput key in mouseValues)
//                            if (IsPressed(currentMouseState, key))
//                                return true;
//                        break;
//                    default:
//                        foreach (Buttons key in inputValues)
//                            if (IsPressed(player.CurrentState, key))
//                                return true;
//                        break;
//                }
//            }
//            return false;
//        }
//
//        public float GetRaw(Buttons input)
//        {
//            for (int i = 0, l = players.Count; i < l; i++)
//            {
//                var raw = GetRaw(input, i);
//                if (raw != 0f)
//                    return raw;
//            }
//            return 0f;
//        }
//        public float GetRaw(Buttons input, int index)
//        {
//            var player = getPlayer(index);
//            if (player != null && player.GamePadIndex >= 0)
//            {
//                var state = player.CurrentState;
//                switch (input)
//                {
////                    case Buttons.LeftTrigger:
////                        return state.Triggers.Left;
////                    case Buttons.RightTrigger:
////                        return state.Triggers.Right;
////                    case Buttons.LeftStickUp:
////                        return state.ThumbSticks.Left.Y;
////                    case Buttons.LeftStickLeft:
////                        return state.ThumbSticks.Left.X;
////                    case Buttons.LeftStickDown:
////                        return state.ThumbSticks.Left.Y;
////                    case Buttons.LeftStickRight:
////                        return state.ThumbSticks.Left.X;
////                    case Buttons.RightStickUp:
////                        return state.ThumbSticks.Right.Y;
////                    case Buttons.RightStickLeft:
////                        return state.ThumbSticks.Right.X;
////                    case Buttons.RightStickDown:
////                        return state.ThumbSticks.Right.Y;
////                    case Buttons.RightStickRight:
////                        return state.ThumbSticks.Right.X;
//                }
//            }
//            return 0f;
//        }

        /* General Methods */
//        public bool AllPlayersConnected()
//        {
//            var l = players.Count;
//            for (int i = 0; i < l; i++)
//            {
//                var player = players[i];
//                if (player == null || !player.IsConnected())
//                    return false;
//            }
//            return l != 0;
//        }
//        public int GetPlayerCount()
//        {
//            return players.Count;
//        }
//        public bool FindNewGamepads()
//        {
//            var found = false;
//            var indices = new List<int>();
//            indices.AddRange(gamepadIndices);
//            for (int i = 0, l = players.Count; i < l; i++)
//            {
//                var player = players[i];
//                if (player.GamePadIndex >= 0)
//                    indices.RemoveAt(player.GamePadIndex);
//            }
//
//            foreach (var j in indices)
//            {
//                var state = GamePad.GetState(j, gamePadDeadZone);
//                if (state.IsConnected)
//                {
//                    found = true;
//                    AddGamepadPlayer(new Player()
//                    {
//                        GamePadIndex = j,
//                        CurrentState = state
//                    });
//
//                    if (players.Count == PlayerCount)
//                        break;
//                }
//            }
//            return found;
//        }
//        public void SetActivePlayers(List<int> playerIds)
//        {
//            var newPlayers = new List<Player>();
//            for (int i = 0, l = playerIds.Count; i < l; i++)
//            {
//                var newPlayer = getPlayer(playerIds[i]);
//                if (newPlayer != null)
//                    newPlayers.Add(newPlayer);
//            }
//            players = newPlayers;
//            PlayerCount = players.Count;
//        }
        /* Gamepad Methods */
//        private void AddGamepadPlayer(Player player)
//        {
//            for (int i = 0, l = players.Count; i < l; i++)
//            {
//                var p = players[i];
//                if (!p.IsConnected())
//                {
//                    players[i] = player;
//                    return;
//                }
//            }
//            players.Add(player);
//        }
//        public bool SetGamepadVibration(int index, float left, float right)
//        {
//            var player = getPlayer(index);
//            return player != null && player.GamePadIndex >= 0 && GamePad.SetVibration(player.GamePadIndex, left, right);
//        }
//        public GamePadCapabilities GetCapabilities(int index)
//        {
//            var player = getPlayer(index);
//            return player == null || player.GamePadIndex < 0
//                ? new GamePadCapabilities()
//                : GamePad.GetCapabilities(player.GamePadIndex);
//        }
//        /* Keyboard Methods */
//        public void AddKeyboardPlayer(Dictionary<Buttons, Keys> map)
//        {
//            if (map != null)
//            {
//                var keyboardPlayer = new Player();
//                keyboardPlayer.GamePadIndex = KEYBOARD_INDEX;
//                keyboardPlayer.KeyboardMap = map;
//                players.Add(keyboardPlayer);
//            }
//        }
//        /* Mouse Methods */
//        public void AddMousePlayer(Dictionary<Buttons, MouseInput> map)
//        {
//            if (map != null)
//            {
//                var mousePlayer = new Player();
//                mousePlayer.GamePadIndex = MOUSE_INDEX;
//                mousePlayer.MouseMap = map;
//                players.Add(mousePlayer);
//            }
//        }
//        public Point GetMousePosition()
//        {
//            return currentMouseState.Position;
//        }
//        public bool IsMouseMoved()
//        {
//            return currentMouseState.X != previousMouseState.X || currentMouseState.Y != previousMouseState.Y;
//        }
//        public int GetMouseScroll()
//        {
//            return currentMouseState.ScrollWheelValue - previousMouseState.ScrollWheelValue;
//        }

        StringBuilder inputStringBuilder = new StringBuilder();
        
        public string ReadInputString()
        {
            
            
            return inputStringBuilder.ToString(); //throw new NotImplementedException();
        }
        
        private string GetChar(Keys key, bool caps, bool num, bool shift)
        {
            if (key.ToString().Length == 1) // TODO: Optmize by checking if its in range instead
            {
                if (shift || caps)
                    return key.ToString();
                
                return key.ToString().ToLower();
            }

            switch (key)
            {
                case Keys.Space:
                    return " ";
                case Keys.OemTilde:
                    return (shift) ? "~" : "`";
                case Keys.D1:
                    return (shift) ? "!" : "1";
                case Keys.D2:
                    return (shift) ? "@" : "2";
                case Keys.D3:
                    return (shift) ? "#" : "3";
                case Keys.D4:
                    return (shift) ? "$" : "4";
                case Keys.D5:
                    return (shift) ? "%" : "5";
                case Keys.D6:
                    return (shift) ? "^" : "6";
                case Keys.D7:
                    return (shift) ? "&" : "7";
                case Keys.D8:
                    return (shift) ? "*" : "8";
                case Keys.D9:
                    return (shift) ? "(" : "9";
                case Keys.D0:
                    return (shift) ? ")" : "0";
                case Keys.OemMinus:
                    return (shift) ? "_" : "-";
                case Keys.OemPlus:
                    return (shift) ? "+" : "=";
                case Keys.OemOpenBrackets:
                    return (shift) ? "{" : "[";
                case Keys.OemCloseBrackets:
                    return (shift) ? "}" : "]";
                case Keys.OemSemicolon:
                    return (shift) ? ":" : ";";
                case Keys.OemPipe:
                    return (shift) ? "|" : "\\";
                case Keys.OemQuotes:
                    return (shift) ? "\"" : "'";
                case Keys.OemBackslash:
                    return (shift) ? "|" : "\\";
                case Keys.OemComma:
                    return (shift) ? "<" : ",";
                case Keys.OemPeriod:
                    return (shift) ? ">" : ".";
                case Keys.OemQuestion:
                    return (shift) ? "?" : "/";
                case Keys.NumPad0:
                    return (num) ? "0" : "";
                case Keys.NumPad1:
                    return (num) ? "1" : "";
                case Keys.NumPad2:
                    return (num) ? "2" : "";
                case Keys.NumPad3:
                    return (num) ? "3" : "";
                case Keys.NumPad4:
                    return (num) ? "4" : "";
                case Keys.NumPad5:
                    return (num) ? "5" : "";
                case Keys.NumPad6:
                    return (num) ? "6" : "";
                case Keys.NumPad7:
                    return (num) ? "7" : "";
                case Keys.NumPad8:
                    return (num) ? "8" : "";
                case Keys.NumPad9:
                    return (num) ? "9" : "";
                case Keys.Add:
                    return "+";
                case Keys.Divide:
                    return "/";
                case Keys.Multiply:
                    return "*";
                case Keys.Decimal:
                    return (num) ? "." : "";
            }

            return string.Empty;
        }

        public bool GetKey(int key)
        {
            return false; //throw new NotImplementedException();
        }

//        Dictionary<PixelVisionSDK.Keys, Keys> PV8KeyMap = new Dictionary<PixelVisionSDK.Keys, Keys>()
//        {
//            {PixelVisionSDK.Keys.None, Keys.None},
//            {PixelVisionSDK.Keys.Backspace, Keys.Back},
//            {PixelVisionSDK.Keys.Tab, Keys.Tab},
//            {PixelVisionSDK.Keys.Return, Keys.Enter}, // 0x0000000D
////            {Keys.Pause = 19, // 0x00000013
//            {PixelVisionSDK.Keys.CapsLock, Keys.CapsLock}, // 0x00000014
////            {Keys.Kana = 21, // 0x00000015
////            {Keys.Kanji = 25, // 0x00000019
//            {PixelVisionSDK.Keys.Escape, Keys.Escape}, // 0x0000001B
////            {Keys.ImeConvert = 28, // 0x0000001C
////            {Keys.ImeNoConvert = 29, // 0x0000001D
////            {PixelVisionSDK.Keys.Space, Keys.Space}, // 0x00000020
//            {PixelVisionSDK.Keys.PageUp, Keys.PageUp}, // 0x00000021
//            {PixelVisionSDK.Keys.PageDown, Keys.PageDown}, // 0x00000022
//            {PixelVisionSDK.Keys.End, Keys.End}, // 0x00000023
//            {PixelVisionSDK.Keys.Home, Keys.Home}, // 0x00000024
//            {PixelVisionSDK.Keys.LeftArrow, Keys.Left}, // 0x00000025
//            {PixelVisionSDK.Keys.UpArrow, Keys.Up}, // 0x00000026
//            {PixelVisionSDK.Keys.RightArrow, Keys.Right}, // 0x00000027
//            {PixelVisionSDK.Keys.DownArrow, Keys.Down}, // 0x00000028
////            {Keys.Select, PixelVisionSDK.Keys.Sele}, // 0x00000029
////            {Keys.Print, PixelVisionSDK.Keys.Print}, // 0x0000002A
////            {Keys.Execute = 43, // 0x0000002B
//            {PixelVisionSDK.Keys.Print, Keys.PrintScreen}, // 0x0000002C
//            {PixelVisionSDK.Keys.Insert, Keys.Insert}, // 0x0000002D
//            {PixelVisionSDK.Keys.Delete, Keys.Delete}, // 0x0000002E
//            {PixelVisionSDK.Keys.Help, Keys.Help}, // 0x0000002F
//            {PixelVisionSDK.Keys.Alpha0, Keys.D0}, // 0x00000030
//            {PixelVisionSDK.Keys.Alpha1, Keys.D1}, // 0x00000031
//            {PixelVisionSDK.Keys.Alpha2, Keys.D2}, // 0x00000032
//            {PixelVisionSDK.Keys.Alpha3, Keys.D3}, // 0x00000033
//            {PixelVisionSDK.Keys.Alpha4, Keys.D4}, // 0x00000034
//            {PixelVisionSDK.Keys.Alpha5, Keys.D5}, // 0x00000035
//            {PixelVisionSDK.Keys.Alpha6, Keys.D6}, // 0x00000036
//            {PixelVisionSDK.Keys.Alpha7, Keys.D7}, // 0x00000037
//            {PixelVisionSDK.Keys.Alpha8, Keys.D8}, // 0x00000038
//            {PixelVisionSDK.Keys.Alpha9, Keys.D9}, // 0x00000039
//            {PixelVisionSDK.Keys.A, Keys.A}, // 0x00000041
//            {PixelVisionSDK.Keys.B, Keys.B}, // 0x00000042
//            {PixelVisionSDK.Keys.C, Keys.C}, // 0x00000043
//            {PixelVisionSDK.Keys.D, Keys.D}, // 0x00000044
//            {PixelVisionSDK.Keys.E, Keys.E}, // 0x00000045
//            {PixelVisionSDK.Keys.F, Keys.F}, // 0x00000046
//            {PixelVisionSDK.Keys.G, Keys.G}, // 0x00000047
//            {PixelVisionSDK.Keys.H, Keys.H}, // 0x00000048
//            {PixelVisionSDK.Keys.I, Keys.I}, // 0x00000049
//            {PixelVisionSDK.Keys.J, Keys.J}, // 0x0000004A
//            {PixelVisionSDK.Keys.K, Keys.K}, // 0x0000004B
//            {PixelVisionSDK.Keys.L, Keys.L}, // 0x0000004C
//            {PixelVisionSDK.Keys.M, Keys.M}, // 0x0000004D
//            {PixelVisionSDK.Keys.N, Keys.N}, // 0x0000004E
//            {PixelVisionSDK.Keys.O, Keys.O}, // 0x0000004F
//            {PixelVisionSDK.Keys.P, Keys.P}, // 0x00000050
//            {PixelVisionSDK.Keys.Q, Keys.Q}, // 0x00000051
//            {PixelVisionSDK.Keys.R, Keys.R}, // 0x00000052
//            {PixelVisionSDK.Keys.S, Keys.S}, // 0x00000053
//            {PixelVisionSDK.Keys.T, Keys.T}, // 0x00000054
//            {PixelVisionSDK.Keys.U, Keys.U}, // 0x00000055
//            {PixelVisionSDK.Keys.V, Keys.V}, // 0x00000056
//            {PixelVisionSDK.Keys.W, Keys.W}, // 0x00000057
//            {PixelVisionSDK.Keys.X, Keys.X}, // 0x00000058
//            {PixelVisionSDK.Keys.Y, Keys.Y}, // 0x00000059
//            {PixelVisionSDK.Keys.Z, Keys.Z}, // 0x0000005A
//////            {Keys.LeftWindows, PixelVisionSDK.Keys.LeftWindows}, // 0x0000005B
//////            {Keys.RightWindows = 92, // 0x0000005C
//////            {Keys.Apps = 93, // 0x0000005D
//////            {Keys.Sleep = 95, // 0x0000005F
//            {PixelVisionSDK.Keys.Keypad0, Keys.NumPad0}, // 0x00000060
//            {PixelVisionSDK.Keys.Keypad1, Keys.NumPad1}, // 0x00000061
//            {PixelVisionSDK.Keys.Keypad2, Keys.NumPad2}, // 0x00000062
//            {PixelVisionSDK.Keys.Keypad3, Keys.NumPad3}, // 0x00000063
//            {PixelVisionSDK.Keys.Keypad4, Keys.NumPad4}, // 0x00000064
//            {PixelVisionSDK.Keys.Keypad5, Keys.NumPad5}, // 0x00000065
//            {PixelVisionSDK.Keys.Keypad6, Keys.NumPad6}, // 0x00000066
//            {PixelVisionSDK.Keys.Keypad7, Keys.NumPad7}, // 0x00000067
//            {PixelVisionSDK.Keys.Keypad8, Keys.NumPad8}, // 0x00000068
//            {PixelVisionSDK.Keys.Keypad9, Keys.NumPad9}, // 0x00000069
//            {PixelVisionSDK.Keys.KeypadMultiply, Keys.Multiply}, // 0x0000006A
//            {PixelVisionSDK.Keys.KeypadPlus, Keys.Add}, // 0x0000006B
//////            {Keys.Separator, PixelVisionSDK.Keys}, // 0x0000006C
//            {PixelVisionSDK.Keys.KeypadMinus, Keys.Subtract}, // 0x0000006D
//            {PixelVisionSDK.Keys.KeypadPeriod, Keys.Decimal}, // 0x0000006E
//            {PixelVisionSDK.Keys.KeypadDivide, Keys.Divide}, // 0x0000006F
//            {PixelVisionSDK.Keys.F1, Keys.F1}, // 0x00000070
//            {PixelVisionSDK.Keys.F2, Keys.F2}, // 0x00000071
//            {PixelVisionSDK.Keys.F3, Keys.F3}, // 0x00000072
//            {PixelVisionSDK.Keys.F4, Keys.F4}, // 0x00000073
//            {PixelVisionSDK.Keys.F5, Keys.F5}, // 0x00000074
//            {PixelVisionSDK.Keys.F6, Keys.F6}, // 0x00000075
//            {PixelVisionSDK.Keys.F7, Keys.F7}, // 0x00000076
//            {PixelVisionSDK.Keys.F8, Keys.F8}, // 0x00000077
//            {PixelVisionSDK.Keys.F9, Keys.F9}, // 0x00000078
//            {PixelVisionSDK.Keys.F10, Keys.F10}, // 0x00000079
//            {PixelVisionSDK.Keys.F11, Keys.F11}, // 0x0000007A
//            {PixelVisionSDK.Keys.F12, Keys.F12}, // 0x0000007B
//            {PixelVisionSDK.Keys.F13, Keys.F13}, // 0x0000007C
//            {PixelVisionSDK.Keys.F14, Keys.F14}, // 0x0000007D
//            {PixelVisionSDK.Keys.F15, Keys.F15}, // 0x0000007E
//////            {Keys.F16, PixelVisionSDK.Keys.F16}, // 0x0000007F
//////            {Keys.F17, PixelVisionSDK.Keys.F17}, // 0x00000080
//////            {Keys.F18, PixelVisionSDK.Keys.F18}, // 0x00000081
//////            {Keys.F19, PixelVisionSDK.Keys.F}, // 0x00000082
//////            {Keys.F20, PixelVisionSDK.Keys.F}, // 0x00000083
//////            {Keys.F21, PixelVisionSDK.Keys.F}, // 0x00000084
//////            {Keys.F22, PixelVisionSDK.Keys.F}, // 0x00000085
//////            {Keys.F23, PixelVisionSDK.Keys.F}, // 0x00000086
//////            {Keys.F24, PixelVisionSDK.Keys.F}, // 0x00000087
//            {PixelVisionSDK.Keys.Numlock, Keys.NumLock}, // 0x00000090
//            {PixelVisionSDK.Keys.ScrollLock, Keys.Scroll}, // 0x00000091
//            {PixelVisionSDK.Keys.LeftShift, Keys.LeftShift}, // 0x000000A0
//            {PixelVisionSDK.Keys.RightShift, Keys.RightShift}, // 0x000000A1
//            {PixelVisionSDK.Keys.LeftControl, Keys.LeftControl}, // 0x000000A2
//            {PixelVisionSDK.Keys.RightControl, Keys.RightControl}, // 0x000000A3
//            {PixelVisionSDK.Keys.LeftAlt, Keys.LeftAlt}, // 0x000000A4
//            {PixelVisionSDK.Keys.RightAlt, Keys.RightAlt}, // 0x000000A5
//////            {Keys.BrowserBack = 166, // 0x000000A6
//////            {Keys.BrowserForward = 167, // 0x000000A7
//////            {Keys.BrowserRefresh = 168, // 0x000000A8
//////            {Keys.BrowserStop = 169, // 0x000000A9
//////            {Keys.BrowserSearch = 170, // 0x000000AA
//////            {Keys.BrowserFavorites = 171, // 0x000000AB
//////            {Keys.BrowserHome = 172, // 0x000000AC
//////            {Keys.VolumeMute = 173, // 0x000000AD
//////            {Keys.VolumeDown = 174, // 0x000000AE
//////            {Keys.VolumeUp = 175, // 0x000000AF
//////            {Keys.MediaNextTrack = 176, // 0x000000B0
//////            {Keys.MediaPreviousTrack = 177, // 0x000000B1
//////            {Keys.MediaStop = 178, // 0x000000B2
//////            {Keys.MediaPlayPause = 179, // 0x000000B3
//////            {Keys.LaunchMail = 180, // 0x000000B4
//////            {Keys.SelectMedia = 181, // 0x000000B5
//////            {Keys.LaunchApplication1 = 182, // 0x000000B6
//////            {Keys.LaunchApplication2 = 183, // 0x000000B7
//            {PixelVisionSDK.Keys.Semicolon, Keys.OemSemicolon}, // 0x000000BA
//            {PixelVisionSDK.Keys.Plus, Keys.OemPlus}, // 0x000000BB
//            {PixelVisionSDK.Keys.Comma, Keys.OemComma}, // 0x000000BC
//            {PixelVisionSDK.Keys.Minus, Keys.OemMinus}, // 0x000000BD
//            {PixelVisionSDK.Keys.Period, Keys.OemPeriod}, // 0x000000BE
//            {PixelVisionSDK.Keys.Question, Keys.OemQuestion}, // 0x000000BF
//            {PixelVisionSDK.Keys.BackQuote, Keys.OemTilde}, // 0x000000C0
//////            {Keys.ChatPadGreen = 202, // 0x000000CA
//////            {Keys.ChatPadOrange = 203, // 0x000000CB
//            {PixelVisionSDK.Keys.LeftBracket, Keys.OemOpenBrackets}, // 0x000000DB
//            {PixelVisionSDK.Keys.Backslash, Keys.OemPipe}, // 0x000000DC
//            {PixelVisionSDK.Keys.RightBracket, Keys.OemCloseBrackets}, // 0x000000DD
//            {PixelVisionSDK.Keys.Quote, Keys.OemQuotes}, // 0x000000DE
//////            {Keys.Oem8 = 223, // 0x000000DF
//            // TODO for some reason Backslash crashes the game if it is uncommneted
////            {PixelVisionSDK.Keys.Backslash, Keys.OemBackslash}, // 0x000000E2
//////            {Keys.ProcessKey = 229, // 0x000000E5
//////            {Keys.OemCopy = 242, // 0x000000F2
//////            {Keys.OemAuto = 243, // 0x000000F3
//////            {Keys.OemEnlW = 244, // 0x000000F4
//////            {Keys.Attn = 246, // 0x000000F6
//////            {Keys.Crsel = 247, // 0x000000F7
//////            {Keys.Exsel = 248, // 0x000000F8
//////            {Keys.EraseEof = 249, // 0x000000F9
//////            {Keys.Play = 250, // 0x000000FA
//////            {Keys.Zoom = 251, // 0x000000FB
//////            {Keys.Pa1 = 253, // 0x000000FD
//////            {Keys.OemClear = 254, // 0x000000FE
//        };
        
        public bool GetKeyDown(Keys key)
        {
            return currentKeyboardState.IsKeyDown(key) && previousKeyboardState.IsKeyDown(key);
        }

        public bool GetKeyUp(Keys key)
        {
            return !currentKeyboardState.IsKeyDown(key) && previousKeyboardState.IsKeyDown(key);
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

        public Vector ReadMousePosition()
        {
            var pos = PointToScreen(currentMouseState.Position);
            return new Vector(pos.X, pos.Y);
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
            var vx = x;// - viewport.X;
            var vy = y;// - viewport.Y;
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

        

        public bool export { get; set; }

        public bool ButtonReleased(Buttons button, int controllerID = 0)
        {
            // TODO need to test this out
            var value = false;
            
            var player = getPlayer(controllerID);
            
            if (player != null)
            {
                Keys key = player.KeyboardMap.TryGetValue(button, out key) ? key : default(Keys);

                // Test the keyboard or the controller
                value = (!currentKeyboardState.IsKeyDown(key) && previousKeyboardState.IsKeyDown(key)) || !IsPressed(player.CurrentState, button) && IsPressed(player.PreviousState, button);
            }
            
            return value;
        }

        public bool ButtonDown(Buttons button, int controllerID = 0)
        {
            var value = false;
            
            var player = getPlayer(controllerID);
            
            if (player != null)
            {
                Keys key = player.KeyboardMap.TryGetValue(button, out key) ? key : default(Keys);

                // Test the keyboard or the controller
                value = (currentKeyboardState.IsKeyDown(key) && previousKeyboardState.IsKeyDown(key)) || IsPressed(player.CurrentState, button) && IsPressed(player.PreviousState, button);
            }
            
            return value;
        }

        public void RegisterKeyInput()
        {
            currentKeyboardState = Keyboard.GetState();
            previousKeyboardState = currentKeyboardState;
            
            var player1 = getPlayer(0);
//            player1.GamePadIndex = KEYBOARD_INDEX;
            player1.KeyboardMap = new Dictionary<Buttons, Keys>
            {
                { Buttons.Up, Keys.Up },
                { Buttons.Left, Keys.Left },
                { Buttons.Right, Keys.Right },
                { Buttons.Down, Keys.Down },
                { Buttons.Start, Keys.A },
                { Buttons.A, Keys.X},
                { Buttons.B, Keys.C}
            };
            
            var player2 = getPlayer(1);
//            player2.GamePadIndex = KEYBOARD_INDEX;
            player2.KeyboardMap = new Dictionary<Buttons, Keys>
            {
                { Buttons.Up, Keys.I },
                { Buttons.Left, Keys.J },
                { Buttons.Right, Keys.L },
                { Buttons.Down, Keys.K },
                { Buttons.Start, Keys.OemSemicolon },
                { Buttons.A, Keys.Enter},
                { Buttons.B, Keys.RightShift}
            };

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
    }
}