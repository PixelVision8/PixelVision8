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
using System.Linq;
using PixelVision8.Engine.Chips;
using PixelVision8.Engine.Services;

namespace PixelVision8.Engine
{
    /// <summary>
    ///     This is the default engine class for Pixel Vision 8. It manages the
    ///     state of all chips, the game itself and helps with communication between
    ///     the two.
    /// </summary>
    public class PixelVisionEngine : IEngine, IServiceLocator
    {
        protected Dictionary<string, AbstractChip> Chips = new Dictionary<string, AbstractChip>();
        protected string[] DefaultChips;
        protected List<IDraw> DrawChips = new List<IDraw>();
        protected IServiceLocator ServiceLocator;
        protected List<IUpdate> UpdateChips = new List<IUpdate>();

        public PixelVisionEngine(IServiceLocator serviceLocator, string[] chips = null, string name = "Engine")
        {
            if (chips != null) DefaultChips = chips;

            Name = name;
            ServiceLocator = serviceLocator;

            Init();
        }

        public Dictionary<string, string> MetaData { get; } = new Dictionary<string, string>
        {
            {"name", "untitled"}
        };

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
        public SoundChip SoundChip { get; set; }

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
        ///     Access to the MusicChip.
        /// </summary>
        /// <tocexclude />
        public MusicChip MusicChip { get; set; }

        /// <summary>
        ///     Access to the current game in memory.
        /// </summary>
        /// <tocexclude />
        public GameChip GameChip { get; set; }

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

            foreach (var chip in UpdateChips) chip.Update(timeDelta); //delta / 1000f);
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

        /// <summary>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public string GetMetadata(string key, string defaultValue = "")
        {
            if (!MetaData.ContainsKey(key)) MetaData.Add(key, defaultValue);

            return MetaData[key];
        }

        /// <summary>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetMetadata(string key, string value)
        {
            if (!MetaData.ContainsKey(key))
            {
                MetaData.Add(key, value);
            }
            else
            {
                if (value == "")
                    MetaData.Remove(key);
                else 
                    MetaData[key] = value;

            }
        }

        /// <summary>
        /// </summary>
        /// <param name="target"></param>
        /// <param name="ignoreKeys"></param>
        public void ReadAllMetadata(Dictionary<string, string> target)
        {
            target.Clear();

            foreach (var data in MetaData) target.Add(data.Key, data.Value);
        }


        /// <summary>
        ///     The PixelVisionEngine Init() method creates the
        ///     <see cref="ChipManager" /> and <see cref="APIBridge" /> as well as any
        ///     additional chips supplied in the <see cref="DefaultChips" /> array.
        /// </summary>
        /// <tocexclude />
        public virtual void Init()
        {
            //apiBridge = new APIBridge(this);
            if (DefaultChips != null) CreateChips(DefaultChips);
        }

        public void CreateChips(string[] chips)
        {
            foreach (var chip in chips) GetChip(chip);
        }

        #region Chip Manager

        public void AddService(string id, IService service)
        {
            ServiceLocator.AddService(id, service);
        }

        public IService GetService(string id)
        {
            return ServiceLocator.GetService(id);
        }

        public void RemoveService(string id)
        {
            ServiceLocator.RemoveService(id);
        }

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
            foreach (var item in Chips.Where(c => c.Value.active == false).ToArray()) Chips.Remove(item.Key);
        }

        #endregion
    }
}