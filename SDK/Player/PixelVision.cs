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

namespace PixelVision8.Player
{
    /// <summary>
    ///     This is the default engine class for Pixel Vision 8. It manages the
    ///     state of all chips, the game itself and helps with communication between
    ///     the two.
    /// </summary>
    public partial class PixelVision
    {
        protected List<AbstractChip> Chips = new List<AbstractChip>();
        protected List<IDraw> DrawChips = new List<IDraw>();
        protected List<IUpdate> UpdateChips = new List<IUpdate>();
        public int SpriteCounter { get; set; }

        public PixelVision(string[] chips = null, string name = "Player")
        {
            Name = name;

            if (chips != null)
            {
                foreach (var chip in chips) GetChip(chip);
            }
        }

        public virtual void ResetGame()
        {
            int counter = 0;

            while(counter < _totalChips)
            {
                Chips[counter].Reset();
                counter ++;
            }
            // foreach (var chip in Chips) chip.Reset();
        }

        /// <summary>
        ///     Attempts to run a game that has been loaded into memory via the
        ///     LoadGame() method. It resets the display and game as well as calling
        ///     init on the game itself. It also toggles the <see cref="running" />
        ///     flag to true.
        /// </summary>
        /// <tocexclude />
        public virtual void RunGame()
        {
            int counter = 0;

            while(counter < _totalChips)
            {
                Chips[counter].Init();
                counter ++;
            }
            // foreach (var chip in Chips) chip.Init();
        }

        /// <summary>
        /// </summary>
        /// <tocexclude />
        public string Name { get; set; }


        public int FPS { get; set; }

        /// <summary>
        ///     This method is called in order to update the business logic of the
        ///     engine, its chips and any loaded game. This method only executes if
        ///     the engine is running.
        /// </summary>
        /// <param name="timeDelta"></param>
        /// <tocexclude />
        public virtual void Update(int timeDelta)
        {
            SpriteCounter = 0;
            // foreach (var chip in UpdateChips) chip.Update(timeDelta);

            int counter = 0;

            while(counter < _totalChipsToUpdate)
            {
                UpdateChips[counter].Update(timeDelta);
                counter ++;
            }
        }

        /// <summary>
        ///     This method is called in order to update the display logic of the
        ///     engine, its chips and any loaded game. This method only executes if
        ///     the engine is running.
        /// </summary>
        /// <tocexclude />
        public virtual void Draw()
        {
            // foreach (var chip in DrawChips) chip.Draw();
            int counter = 0;

            while(counter < _totalChipsToDraw)
            {
                DrawChips[counter].Draw();
                counter ++;
            }
        }

        #region Chip Manager

        public AbstractChip FindChip(string id)
        {

            foreach (var chip in Chips)
            {
                if(chip.Id == id)
                    return chip;
                
            }

            return null;
        }

        public AbstractChip GetChip(string id, bool activeOnCreate = true)
        {
            
            var chipInstance = FindChip(id);

            if (chipInstance != null) 
                return chipInstance;

            var type = Type.GetType(id);

            if (type != null)
            {
                chipInstance = null;

                try
                {
                    chipInstance = Activator.CreateInstance(type) as AbstractChip;
                    ActivateChip(id, chipInstance, activeOnCreate);
                }
                catch (Exception error)
                {
                    Console.WriteLine("Chip '" + id + "' could not be created. " + error.Message);
                }

                return chipInstance;
            }

            return null;
        }

        protected int _totalChips;
        protected int _totalChipsToUpdate;
        protected int _totalChipsToDraw;

        public void ActivateChip(string id, AbstractChip chip, bool autoActivate = true)
        {
            var chipInstance = FindChip(id);

            if (chipInstance != null)
            {
                Chips.Remove(chipInstance);
                //TODO do we need to disable the old chip first
                // Chips[id] = chip;
            }

            chip.Id = id;
            _totalChips ++;

                //TODO fixed bug here but need to make sure we don't need to do this above
            Chips.Add(chip);

            if (chip is IUpdate update)
            {
                UpdateChips.Add(update);
                _totalChipsToUpdate ++;
            } 

            if (chip is IDraw draw)
            {
                DrawChips.Add(draw);
                _totalChipsToDraw ++;
            }
        
            if (autoActivate) chip.Activate(this);
        }

        #endregion
    }
}