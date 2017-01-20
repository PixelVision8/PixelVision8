//  
// Copyright (c) Jesse Freeman. All rights reserved.  
// 
// Licensed under the Microsoft Public License (MS-PL) License. 
// See LICENSE file in the project root for full license information. 
// 
// Contributors
// --------------------------------------------------------
// This is the official list of Pixel Vision 8 contributors:
//  
// Jesse Freeman - @JesseFreeman
// Christer Kaitila - @McFunkypants
// Pedro Medeiros - @saint11
// Shawn Rakowski - @shwany
// 

using System;

namespace PixelVisionSDK.Chips
{

    public enum Buttons
    {

        Up,
        Down,
        Left,
        Right,
        A,
        B,
        Select,
        Start

    }

    public class ControllerChip : AbstractChip, IUpdate, IKeyInput, IMouseInput
    {

        protected ControllerInput[] controllers = new ControllerInput[2];
        protected IKeyInput keyInput;
        protected IMouseInput mouseInput;

        public bool mouseInputActive
        {
            get { return mouseInput != null; }
        }

        public bool keyInputActive
        {
            get { return keyInput != null; }
        }

        public int maxControllers
        {
            get { return controllers.Length; }
            set
            {
                if (controllers.Length != value)
                    Array.Resize(ref controllers, value);

                for (var i = 0; i < value; i++)
                {
                    if (controllers[i] == null)
                        controllers[i] = new ControllerInput();
                }
            }
        }

        public int totalControllers
        {
            get { return controllers.Length; }
        }

        public string inputString
        {
            get
            {
                if (keyInputActive)
                    return "";

                return keyInput.inputString;
            }
        }

        public bool GetKeyUp(int key)
        {
            if (keyInputActive)
                return false;

            return keyInput.GetKeyUp(key);
        }

        public bool GetKey(int key)
        {
            if (keyInputActive)
                return false;

            return keyInput.GetKey(key);
        }

        public bool GetKeyDown(int key)
        {
            if (keyInputActive)
                return false;

            return keyInput.GetKeyDown(key);
        }

        public bool GetMouseButtonDown(int id = 0)
        {
            if (!mouseInputActive)
                return false;

            return mouseInput.GetMouseButtonDown(id);
        }

        public bool GetMouseButtonUp(int id = 0)
        {
            if (!mouseInputActive)
                return false;

            return mouseInput.GetMouseButtonUp(id);
        }

        public bool GetMouseButton(int id = 0)
        {
            if (!mouseInputActive)
                return false;

            return mouseInput.GetMouseButton(id);
        }

        public Vector mousePosition
        {
            get
            {
                if (!mouseInputActive)
                    return new Vector(-1, -1);

                return mouseInput.mousePosition;
            }
        }

        public void Update(float timeDelta)
        {
            foreach (var controllerInput in controllers)
            {
                controllerInput.Update(timeDelta);
            }
        }

        public void RegisterMouseInput(IMouseInput target)
        {
            mouseInput = target;
        }

        public void RegisterKeyInput(IKeyInput target)
        {
            keyInput = target;
        }

        public bool ButtonDown(int buttonID, int controllerID = 0)
        {
            if (controllerID > controllers.Length)
                return false;

            return controllers[controllerID].GetKeyValue((Buttons) buttonID);
        }

        public bool ButtonReleased(int buttonID, int controllerID = 0)
        {
            return controllers[controllerID].KeyReleased((Buttons) buttonID);
        }

        public void UpdateControllerKey(int controllerID, ButtonState state)
        {
            if (controllerID < 0 || controllerID > controllers.Length)
                return;

            controllers[controllerID].UpdateKeyMap(state);
        }

        public override void Configure()
        {
            engine.controllerChip = this;

            // Setup controllers
            maxControllers = 2;
        }

        public override void Deactivate()
        {
            base.Deactivate();
            engine.controllerChip = null;
        }

        public int ReadControllerKey(int controllerID, Buttons button)
        {
            if (controllerID < 0 || controllerID > controllers.Length)
                return 0;

            return controllers[controllerID].ReadKeyMap(button);
        }

    }

}