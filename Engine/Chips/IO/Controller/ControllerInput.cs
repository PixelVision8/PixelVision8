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
// 

using System.Collections.Generic;
using System.Text;
using PixelVisionSDK.Engine.Chips.IO.File;

namespace PixelVisionSDK.Engine.Chips.IO.Controller
{

    public class ControllerInput : ISave, ILoad
    {

        private readonly Dictionary<Buttons, IButtonState> buttonState = new Dictionary<Buttons, IButtonState>();
        protected float delay;
        public float inputDelay = 0.1f;

        public void DeserializeData(Dictionary<string, object> data)
        {
            //throw new NotImplementedException();
        }

        public string SerializeData()
        {
            var sb = new StringBuilder();
            sb.Append("{");
            CustomSerializedData(sb);
            sb.Append("}");
            return sb.ToString();
        }

        public void CustomSerializedData(StringBuilder sb)
        {
            //            sb.Append("[");
            //            var total = buttonState.Count;
            //            for (int i = 0; i < total; i++)
            //            {
            //                sb.Append("\"controller"+i+"\":" + buttonState[i].SerializeData());
            //            }
            //
            //            sb.Append("]");
        }

        public void Update(float timeDelta)
        {
            foreach (var item in buttonState)
            {
                item.Value.Update(timeDelta);
            }
        }

        /// <summary>
        ///     This updates the key map. If they key exists, it will overwrite the existing value.
        ///     if
        /// </summary>
        /// <param name="inputState"></param>
        public void UpdateKeyMap(IButtonState inputState)
        {
            if (buttonState.ContainsKey(inputState.button))
            {
                buttonState[inputState.button] = inputState;
            }
            else
            {
                buttonState.Add(inputState.button, inputState);
            }
        }

        public int ReadKeyMap(Buttons key)
        {
            if (!buttonState.ContainsKey(key))
                return 0;

            return buttonState[key].mapping;
        }

        public void ClearKeys()
        {
            buttonState.Clear();
        }

        public bool GetKeyValue(Buttons key)
        {
            if (buttonState.ContainsKey(key))
                return buttonState[key].value;

            return false;
        }

        public float GetKeyDownTime(Buttons key)
        {
            if (buttonState.ContainsKey(key))
                return buttonState[key].buttonTimes;

            return 0;
        }

        public bool KeyReleased(Buttons key)
        {
            if (buttonState.ContainsKey(key))
            {
                return buttonState[key].buttonReleased;
            }

            return false;
        }

        public Dictionary<Buttons, IButtonState> ReadStates()
        {
            //TODO this needs to be a clone and cleaned up
            return buttonState;
        }

        //        void OnGUI()
        //        {
        //            Event e = Event.current;
        //            if (e.isKey)
        //                Debug.Log("Detected key code: " + e.keyCode);
        //

        //        }
    }

}