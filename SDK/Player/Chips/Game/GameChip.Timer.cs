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

using System.Collections.Generic;
using System;

namespace PixelVision8.Player
{

    public class Timer : IUpdate
    {
        public string Name;
        public int Time;
        public int Delay;
        public bool Paused;
        public bool Triggered;

        public Timer(string name, int delay, int time = 0, bool paused = false)
        {
            Name = name;
            Time = time;
            Delay = delay;
            Paused = paused;
            Triggered = false;
        }

        public void Update(int timeDelta)
        {  
            
            if(Paused == true)
                return;

            // Reset triggeed value
            Triggered = false;

            // Increment time
            Time += timeDelta;

            // Calculate trigger
            if(Time > Delay)
            {
                Time = 0;
                Triggered = true;
            }

        }

    }

    public partial class PixelVision
    {
        
        /// <summary>
        ///     Access to the TimerChip.
        /// </summary>
        /// <tocexclude />
        public TimerChip TimerChip { get; set; }
    }

    public class TimerChip : AbstractChip, IUpdate
    {
        protected Dictionary<string, Timer> _timers = new Dictionary<string, Timer>();

        protected override void Configure()
        {
            Player.TimerChip = this;
        }

        public void NewTimer(string name, int delay, int time = 0, bool paused = false)
        {
            
            if(_timers.ContainsKey(name) == false)
            {
                _timers[name] = new Timer(name, delay);
            }
            
            var timer = _timers[name];
            timer.Time = time;
            timer.Delay = delay;
            timer.Paused = false;

        }

        public void ClearTimer(string name)
        {

            if(_timers.ContainsKey(name))
            {
                _timers.Remove(name);
            }
        }
        
        public void PauseAllTimers(bool value)
        {
            foreach (var timer in _timers)
            {
                timer.Value.Paused = value;
            }
        }

        public void ClearAllTimers()
        {
            _timers.Clear();
        }

        public void Update(int timeDelta)
        {
            foreach (var timer in _timers)
            {
                timer.Value.Update(timeDelta);
            }
        }

        public int TimerValue(string name) => _timers.ContainsKey(name) ? _timers[name].Time : -1;
        public int TimerDelay(string name) => _timers.ContainsKey(name) ? _timers[name].Delay : -1;
        
        public bool TimerTriggerd(string name) => _timers.ContainsKey(name) ? _timers[name].Triggered : false;

        public bool PauseTimer(string name, bool value)
        {
            if(_timers.ContainsKey(name))
            {
                _timers[name].Paused = value;
                return value;
            }

            return false;
        }
    }

    public partial class GameChip
    {

        private TimerChip TimerChip
        {
            get 
            {

                if(Player.TimerChip == null)
                {
                    Player.GetChip(typeof(TimerChip).FullName);
                }


                return Player.TimerChip;
            }
        }

        public void NewTimer(string name, int delay, int time = 0, bool paused = false) => TimerChip.NewTimer(name, delay, time, paused);
        public void ClearTimer(string name) => TimerChip.ClearTimer(name);
        public void ClearAllTimers() => TimerChip.ClearAllTimers();
        
        public void PauseAllTimers(bool value) => TimerChip.PauseAllTimers(value);

        public int TimerValue(string name) => TimerChip.TimerValue(name);
        public int TimerDelay(string name) => TimerChip.TimerDelay(name);
        public bool TimerTriggered(string name) => TimerChip.TimerTriggerd(name);
        public bool TimerPaused(string name, bool value) => TimerChip.PauseTimer(name, value);


    }

}