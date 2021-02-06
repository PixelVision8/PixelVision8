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
        protected Dictionary<string, AbstractChip> Chips = new Dictionary<string, AbstractChip>();
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
            foreach (var chip in Chips) chip.Value.Reset();
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
            foreach (var chip in Chips) chip.Value.Init();
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
            foreach (var chip in UpdateChips) chip.Update(timeDelta);
        }

        /// <summary>
        ///     This method is called in order to update the display logic of the
        ///     engine, its chips and any loaded game. This method only executes if
        ///     the engine is running.
        /// </summary>
        /// <tocexclude />
        public virtual void Draw()
        {
            foreach (var chip in DrawChips) chip.Draw();
        }

        #region Chip Manager

        public bool HasChip(string id)
        {
            return Chips.ContainsKey(id);
        }

        public AbstractChip GetChip(string id, bool activeOnCreate = true)
        {
            
            if (HasChip(id)) return Chips[id];

            var type = Type.GetType(id);

            if (type != null)
            {
                AbstractChip chipInstance = null;

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

        public void ActivateChip(string id, AbstractChip chip, bool autoActivate = true)
        {
            if (HasChip(id))
            {
                //TODO do we need to disable the old chip first
                Chips[id] = chip;
            }
            else
            {
                //TODO fixed bug here but need to make sure we don't need to do this above
                Chips.Add(id, chip);

                if (chip is IUpdate update) UpdateChips.Add(update);

                if (chip is IDraw draw) DrawChips.Add(draw);
            }

            if (autoActivate) chip.Activate(this);
        }

        #endregion
    }
}