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

using PixelVision8.Engine.Chips;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PixelVision8.Engine
{
    /// <summary>
    ///     This is the default engine class for Pixel Vision 8. It manages the
    ///     state of all chips, the game itself and helps with communication between
    ///     the two.
    /// </summary>
    public class PixelVisionEngineLite : IEngine
    {
        protected Dictionary<string, AbstractChip> Chips = new Dictionary<string, AbstractChip>();
        protected List<IDraw> DrawChips = new List<IDraw>();
        protected List<IUpdate> UpdateChips = new List<IUpdate>();

        public PixelVisionEngineLite(string[] chips = null, string name = "Engine")
        {
            Name = name;

            if (chips != null)
            {
                foreach (var chip in chips) GetChip(chip);
            }
        }



        /// <summary>
        /// </summary>
        /// <tocexclude />
        public string Name { get; set; }

        /// <summary>
        ///     Access to the ColorChip.
        /// </summary>
        /// <tocexclude />
        public ColorChip ColorChip { get; set; }

        /// <summary>
        ///     Access to the ControllerChip.
        /// </summary>
        /// <tocexclude />
        public IControllerChip ControllerChip { get; set; }

        /// <summary>
        ///     Access to the DisplayChip.
        /// </summary>
        /// <tocexclude />
        public DisplayChip DisplayChip { get; set; }

        /// <summary>
        ///     Access to the SoundChip.
        /// </summary>
        /// <tocexclude />
        public ISoundChip SoundChip { get; set; }

        /// <summary>
        ///     Access to the SpriteChip.
        /// </summary>
        /// <tocexclude />
        public SpriteChip SpriteChip { get; set; }

        /// <summary>
        ///     Access to the TileMapChip.
        /// </summary>
        /// <tocexclude />
        public TilemapChip TilemapChip { get; set; }

        /// <summary>
        ///     Access to the FontChip.
        /// </summary>
        /// <tocexclude />
        public FontChip FontChip { get; set; }

        /// <summary>
        ///     Access to the current game in memory.
        /// </summary>
        /// <tocexclude />
        public GameChipLite GameChip { get; set; }

        public virtual void ResetGame()
        {
            if (GameChip == null) return;

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
            if (GameChip == null) return;

            foreach (var chip in Chips) chip.Value.Init();

        }

        /// <summary>
        ///     This method is called in order to update the business logic of the
        ///     engine, its chips and any loaded game. This method only executes if
        ///     the engine is running.
        /// </summary>
        /// <param name="timeDelta"></param>
        /// <tocexclude />
        public virtual void Update(int timeDelta)
        {
            // Reset the sprite counter on each frame
            GameChip.CurrentSprites = 0;

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

        /// <summary>
        ///     This method is called when shutting down the engine
        /// </summary>
        /// <tocexclude />
        public virtual void Shutdown()
        {
            // Shutdown chips
            foreach (var chip in Chips) chip.Value.Shutdown();

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
                catch // (Exception)
                {
                    //Console.WriteLine("Chip '" + id + "' could not be created.");
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

                if (chip is IUpdate) UpdateChips.Add(chip as IUpdate);

                if (chip is IDraw) DrawChips.Add(chip as IDraw);
            }

            if (autoActivate) chip.Activate(this);
        }

        public void DeactivateChip(string id, AbstractChip chip)
        {
            chip.Deactivate();

            if (chip is IUpdate update) UpdateChips.Remove(update);

            if (chip is IDraw draw) DrawChips.Remove(draw);

            Chips.Remove(id);
        }

        public void RemoveInactiveChips()
        {
            foreach (var item in Chips.Where(c => c.Value.Active == false).ToArray()) Chips.Remove(item.Key);
        }

        #endregion
    }
}