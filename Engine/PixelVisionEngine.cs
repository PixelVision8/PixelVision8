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

        protected string[] defaultChips;
        protected Dictionary<string, IService> _services = new Dictionary<string, IService>();

        protected Dictionary<string, AbstractChip> chips = new Dictionary<string, AbstractChip>();
        protected List<IDraw> drawChips = new List<IDraw>();
//        protected PixelVisionEngine engine;
        protected List<IUpdate> updateChips = new List<IUpdate>();

        
        public Dictionary<string, IService> services
        {
            get { return _services; }
        }
        
        public Dictionary<string, string> metaData => _metaData;

        protected Dictionary<string, string> _metaData = new Dictionary<string, string>
        {
            {"name", "untitled"}
        };
        
        public Dictionary<string, byte[]> files = new Dictionary<string, byte[]>();

        /// <summary>
        ///     The PixelVisionEngine constructor requires a render target and an
        ///     optional list of <paramref name="chips" /> to be properly configured.
        /// </summary>
        /// <param name="displayTarget"></param>
        /// <param name="inputFactory1"></param>
        /// <param name="chips"></param>
        /// <param name="name"></param>
        /// <tocexclude />
        public PixelVisionEngine(string[] chips = null, string name = "Engine", bool readOnly = true)
        {
            if (chips != null)
                defaultChips = chips;

            this.name = name;

            //this.canWrite = readOnly;
            
            Init();
        }

        /// <summary>
        /// </summary>
        /// <tocexclude />
        public string name { get; set; }

        //public bool canWrite { get; set; }

        /// <summary>
        ///     Access to the ChipManager.
        ///     <tocexclude />
        /// </summary>
//        public ChipManager chipManager => this;

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
        ///     Flag if the engine is <see cref="running" /> or not.
        /// </summary>
        /// <tocexclude />
//        public bool running { get; private set; }

        /// <summary>
        ///     This method allows you to load a <paramref name="game" /> into the
        ///     Engine's memory. Loading a <paramref name="game" /> doesn't run it.
        ///     It simply sets it up to be run by calling the RunGame() method. This
        ///     allows you to perform additional tasks once a <paramref name="game" />
        ///     is loaded before it plays.
        /// </summary>
        /// <param name="game"></param>
        /// <tocexclude />
//        public virtual void LoadGame(GameChip game)
//        {
////            running = false;
//            chipManager.ActivateChip(game.GetType().FullName, game);
//        }

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
        public virtual void Update(float timeDelta)
        {
//            if (!running)
//                return;

            foreach (var chip in updateChips)
                chip.Update(timeDelta);
            
//            chipManager.Update(timeDelta);
        }

        /// <summary>
        ///     This method is called in order to update the display logic of the
        ///     engine, its chips and any loaded game. This method only executes if
        ///     the engine is running.
        /// </summary>
        /// <tocexclude />
        public virtual void Draw()
        {
//            if (!running)
//                return;

//            chipManager.Draw();
            foreach (var chip in drawChips)
                chip.Draw();
        }

        /// <summary>
        ///     This method is called when shutting down the engine
        /// </summary>
        /// <tocexclude />
        public virtual void Shutdown()
        {
            foreach (var chip in chips)
                chip.Value.Shutdown();
        }

        /// <summary>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public string GetMetaData(string key, string defaultValue = "")
        {
            if (!_metaData.ContainsKey(key))
                _metaData.Add(key, defaultValue);

            return _metaData[key];
        }

        /// <summary>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetMetaData(string key, string value)
        {
            if (!_metaData.ContainsKey(key))
                _metaData.Add(key, value);
            else
                _metaData[key] = value;
        }

        /// <summary>
        /// </summary>
        /// <param name="target"></param>
        /// <param name="ignoreKeys"></param>
        public void DumpMetaData(Dictionary<string, string> target, string[] ignoreKeys)
        {
            target.Clear();

            foreach (var data in _metaData)
                if (Array.IndexOf(ignoreKeys, data.Key) == -1)
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
            // Add the service to the managed list
            if (services.ContainsKey(id))
                services[id] = service;
            else
                services.Add(id, service);

            // Add a reference of the service locator
            service.RegisterService(this);
        }

        public IService GetService(string id)
        {
            if (services.ContainsKey(id))
                return services[id];

            throw new Exception("The requested service '" + id + "' is not registered");
        }
        
        public bool HasChip(string id)
        {
            return chips.ContainsKey(id);
        }
        
        public AbstractChip GetChip(string id, bool activeOnCreate = true)
        {
            //Debug.Log("Chip Manager: Get Chip " + id);

            if (HasChip(id))
            {
                var chip = chips[id];

                if (activeOnCreate)
                    ActivateChip(id, chip);

                return chip;
            }

            // TODO create a chip
            var type = Type.GetType(id);

            AbstractChip chipInstance = null;

            try
            {
                chipInstance = Activator.CreateInstance(type) as AbstractChip;
                ActivateChip(id, chipInstance);
            }
            catch (Exception)
            {
                //throw new Exception("Chip '" + id + "' could not be created.");
            }

            return chipInstance;
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