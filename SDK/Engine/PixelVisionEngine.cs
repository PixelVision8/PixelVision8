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
        protected Dictionary<string, string> _metaData = new Dictionary<string, string>
        {
            {"name", "untitled"}
        };

        protected Dictionary<string, AbstractChip> chips = new Dictionary<string, AbstractChip>();
        protected string[] defaultChips;
        protected List<IDraw> drawChips = new List<IDraw>();
        protected IServiceLocator serviceLocator;
        protected List<IUpdate> updateChips = new List<IUpdate>();

        /// <summary>
        ///     The PixelVisionEngine constructor requires a render target and an
        ///     optional list of <paramref name="chips" /> to be properly configured.
        /// </summary>
        /// <param name="displayTarget"></param>
        /// <param name="inputFactory1"></param>
        /// <param name="chips"></param>
        /// <param name="name"></param>
        /// <tocexclude />
        public PixelVisionEngine(IServiceLocator serviceLocator, string[] chips = null, string name = "Engine")
        {
            if (chips != null)
                defaultChips = chips;

            this.name = name;
            this.serviceLocator = serviceLocator;
            //this.canWrite = readOnly;

            Init();
        }

        public Dictionary<string, string> metaData => _metaData;

        /// <summary>
        /// </summary>
        /// <tocexclude />
        public string name { get; set; }

        /// <summary>
        ///     Access to the ColorChip.
        /// </summary>
        /// <tocexclude />
        public ColorChip colorChip { get; set; }

        /// <summary>
        ///     Access to the ControllerChip.
        /// </summary>
        /// <tocexclude />
        public IControllerChip controllerChip { get; set; }

        /// <summary>
        ///     Access to the DisplayChip.
        /// </summary>
        /// <tocexclude />
        public DisplayChip displayChip { get; set; }

        /// <summary>
        ///     Access to the SoundChip.
        /// </summary>
        /// <tocexclude />
        public SoundChip soundChip { get; set; }

        /// <summary>
        ///     Access to the SpriteChip.
        /// </summary>
        /// <tocexclude />
        public SpriteChip spriteChip { get; set; }

        /// <summary>
        ///     Access to the TileMapChip.
        /// </summary>
        /// <tocexclude />
        public TilemapChip tilemapChip { get; set; }

        /// <summary>
        ///     Access to the FontChip.
        /// </summary>
        /// <tocexclude />
        public FontChip fontChip { get; set; }

        /// <summary>
        ///     Access to the MusicChip.
        /// </summary>
        /// <tocexclude />
        public MusicChip musicChip { get; set; }

        /// <summary>
        ///     Access to the current game in memory.
        /// </summary>
        /// <tocexclude />
        public GameChip gameChip { get; set; }

        /// <summary>
        ///     Attempts to run a game that has been loaded into memory via the
        ///     LoadGame() method. It resets the display and game as well as calling
        ///     init on the game itself. It also toggles the <see cref="running" />
        ///     flag to true.
        /// </summary>
        /// <tocexclude />
        public virtual void RunGame()
        {
            if (gameChip == null)
                return;

            // Make sure all chips are reset to their default values
            //            chipManager.Reset();
            foreach (var chip in chips)
                chip.Value.Reset();

            // Call init on all chips
            //            chipManager.Init();
            var chipNames = chips.Keys.ToList();

            foreach (var chipName in chipNames)
                chips[chipName].Init();

            //            running = true;
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
            gameChip.CurrentSprites = 0;

            foreach (var chip in updateChips)
                chip.Update(timeDelta);//delta / 1000f);
        }

        /// <summary>
        ///     This method is called in order to update the display logic of the
        ///     engine, its chips and any loaded game. This method only executes if
        ///     the engine is running.
        /// </summary>
        /// <tocexclude />
        public virtual void Draw()
        {
            foreach (var chip in drawChips)
                chip.Draw();
        }

        /// <summary>
        ///     This method is called when shutting down the engine
        /// </summary>
        /// <tocexclude />
        public virtual void Shutdown()
        {
            // Shutdown chips
            foreach (var chip in chips)
                chip.Value.Shutdown();

            //            // Remove chips
            //            chips.Clear();
            //            
            //            // Removed references to services
            //            _services.Clear();
        }

        /// <summary>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public string GetMetadata(string key, string defaultValue = "")
        {
            if (!_metaData.ContainsKey(key))
                _metaData.Add(key, defaultValue);

            return _metaData[key];
        }

        /// <summary>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetMetadata(string key, string value)
        {
            if (!_metaData.ContainsKey(key))
            {
                if (value == "")
                    _metaData.Remove(key);
                else
                    _metaData.Add(key, value);
            }
            else if (value != "")
            {
                _metaData[key] = value;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="target"></param>
        /// <param name="ignoreKeys"></param>
        public void ReadAllMetadata(Dictionary<string, string> target)
        {
            target.Clear();

            foreach (var data in _metaData)
                target.Add(data.Key, data.Value);
        }


        /// <summary>
        ///     The PixelVisionEngine Init() method creates the
        ///     <see cref="ChipManager" /> and <see cref="APIBridge" /> as well as any
        ///     additional chips supplied in the <see cref="defaultChips" /> array.
        /// </summary>
        /// <tocexclude />
        public virtual void Init()
        {
            //            chipManager = new ChipManager(this);

            //apiBridge = new APIBridge(this);
            if (defaultChips != null)
                CreateChips(defaultChips);
        }

        public void CreateChips(string[] chips)
        {
            foreach (var chip in chips)
                GetChip(chip);
        }

        #region Chip Manager

        public void AddService(string id, IService service)
        {
            serviceLocator.AddService(id, service);
        }

        public IService GetService(string id)
        {
            return serviceLocator.GetService(id);
        }

        public void RemoveService(string id)
        {
            serviceLocator.RemoveService(id);
        }

        public bool HasChip(string id)
        {
            return chips.ContainsKey(id);
        }

        public AbstractChip GetChip(string id, bool activeOnCreate = true)
        {
            //Debug.Log("Chip Manager: Get Chip " + id);

            if (HasChip(id)) return chips[id];

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
                chips[id] = chip;
            }
            else
            {
                //TODO fixed bug here but need to make sure we don't need to do this above
                chips.Add(id, chip);

                if (chip is IUpdate)
                    updateChips.Add(chip as IUpdate);

                if (chip is IDraw)
                    drawChips.Add(chip as IDraw);
            }

            if (autoActivate)
                chip.Activate(this);
        }

        public void DeactivateChip(string id, AbstractChip chip)
        {
            chip.Deactivate();

            if (chip is IUpdate)
                updateChips.Remove(chip as IUpdate);

            if (chip is IDraw)
                drawChips.Remove(chip as IDraw);

            chips.Remove(id);
        }

        public void RemoveInactiveChips()
        {
            foreach (var item in chips.Where(c => c.Value.active == false).ToArray())
                chips.Remove(item.Key);
        }

        #endregion
    }
}