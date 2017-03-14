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

using PixelVisionSDK.Chips;

namespace PixelVisionSDK
{
    public class ButtonState : IButtonState
    {
        protected bool _released;
        protected float _time;
        protected bool _value;
        public int mapping { get; set; }
        public Buttons button { get; set; }

        public virtual bool buttonReleased
        {
            get { return _released; }
        }

        public virtual float buttonTimes
        {
            get { return _time; }
        }

        public bool dirty { get; set; }

        public virtual bool value
        {
            get { return _value; }

            set
            {
                if (_value && !value) // If last value is true and new value is false, button was released
                    Release();
                else if (_released && !value) // if release is true and value is false reset button
                    Reset();
                else if (!_value && value)
                    _time = 0;

                _value = value;
            }
        }

        public virtual void Reset()
        {
            _time = 0;
            _value = false;
            dirty = false;
            _released = false;
        }

        public virtual void Release()
        {
            _released = true;
            _value = false;
        }

        public virtual void Update(float timeDelta)
        {
            if (_value)
                _time += timeDelta;
        }
    }
}