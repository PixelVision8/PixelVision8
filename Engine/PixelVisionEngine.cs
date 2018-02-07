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
using PixelVisionRunner;
using PixelVisionSDK.Chips;

namespace PixelVisionSDK
{

    /// <summary>
    ///     This is the default engine class for Pixel Vision 8. It manages the
    ///     state of all chips, the game itself and helps with communication between
    ///     the two.
    /// </summary>
    public class PixelVisionEngine : IEngine
    {

        protected string[] defaultChips;
        protected IDisplayTarget displayTarget;
        protected IInputFactory inputFactory;
        
        protected Dictionary<string, string> metaData = new Dictionary<string, string>
        {
            {"name", "untitled"}
        };

        /// <summary>
        ///     The PixelVisionEngine constructor requires a render target and an
        ///     optional list of <paramref name="chips" /> to be properly configured.
        /// </summary>
        /// <param name="displayTarget"></param>
        /// <param name="inputFactory1"></param>
        /// <param name="chips"></param>
        /// <param name="name"></param>
        /// <tocexclude />
        public PixelVisionEngine(IDisplayTarget displayTarget, IInputFactory inputFactory, string[] chips = null,
            string name = "Engine")
        {
            this.displayTarget = displayTarget;
            this.inputFactory = inputFactory;
            
            if (chips != null)
                defaultChips = chips;

            this.name = name;

            Init();
        }

        /// <summary>
        /// </summary>
        /// <tocexclude />
        public string name { get; set; }

        /// <summary>
        ///     Access to the ChipManager.
        ///     <tocexclude />
        /// </summary>
        public ChipManager chipManager { get; set; }

        /// <summary>
        ///     Access to the ColorChip.
        /// </summary>
        /// <tocexclude />
        public ColorChip colorChip { get; set; }

        /// <summary>
        ///     Access to the ColorMapChip.
        /// </summary>
        /// <tocexclude />
        public ColorMapChip colorMapChip { get; set; }

        /// <summary>
        ///     Access to the ControllerChip.
        /// </summary>
        /// <tocexclude />
        public ControllerChip controllerChip { get; set; }

        /// <summary>
        ///     Access to the DisplayChip.
        /// </summary>
        /// <tocexclude />
        public DisplayChip displayChip { get; set; }

        /// <summary>
        ///     Access to the ScreenBufferChip.
        /// </summary>
        /// <tocexclude />
//        public ScreenBufferChip screenBufferChip { get; set; }
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
        ///     Access to the APIBridge.
        /// </summary>
        /// <tocexclude />
        //public APIBridge apiBridge { get; set; }
        /// <summary>
        ///     Access to the current game in memory.
        /// </summary>
        /// <tocexclude />
        public GameChip gameChip { get; set; }

        /// <summary>
        ///     Flag if the engine is <see cref="running" /> or not.
        /// </summary>
        /// <tocexclude />
        public bool running { get; private set; }

        /// <summary>
        ///     This method allows you to load a <paramref name="game" /> into the
        ///     Engine's memory. Loading a <paramref name="game" /> doesn't run it.
        ///     It simply sets it up to be run by calling the RunGame() method. This
        ///     allows you to perform additional tasks once a <paramref name="game" />
        ///     is loaded before it plays.
        /// </summary>
        /// <param name="game"></param>
        /// <tocexclude />
        public virtual void LoadGame(GameChip game)
        {
            running = false;
            chipManager.ActivateChip(game.GetType().FullName, game);
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
            if (gameChip == null)
                return;

            // Make sure all chips are reset to their default values
            chipManager.Reset();
//
//            // Call init on all chips
            chipManager.Init();
//
            ConfigureInput();
            

            if (displayTarget != null)
            {
                displayTarget.ResetResolution(displayChip.width, displayChip.height);
            
                displayTarget.CacheColors();  
            }
            
            running = true;
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
            if (!running)
                return;

            chipManager.Update(timeDelta);
        }

        /// <summary>
        ///     This method is called in order to update the display logic of the
        ///     engine, its chips and any loaded game. This method only executes if
        ///     the engine is running.
        /// </summary>
        /// <tocexclude />
        public virtual void Draw()
        {
            if (!running)
                return;

            chipManager.Draw();

            if (displayTarget != null)
            {
                displayTarget.Render();
            }
        }

        /// <summary>
        ///     This method is called when shutting down the engine
        /// </summary>
        /// <tocexclude />
        public virtual void Shutdown()
        {
            chipManager.Shutdown();
        }

        /// <summary>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public string GetMetaData(string key, string defaultValue = "")
        {
            if (!metaData.ContainsKey(key))
                metaData.Add(key, defaultValue);

            return metaData[key];
        }

        /// <summary>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetMetaData(string key, string value)
        {
            if (!metaData.ContainsKey(key))
                metaData.Add(key, value);
            else
                metaData[key] = value;
        }

        /// <summary>
        /// </summary>
        /// <param name="target"></param>
        /// <param name="ignoreKeys"></param>
        public void DumpMetaData(Dictionary<string, string> target, string[] ignoreKeys)
        {
            target.Clear();

            foreach (var data in metaData)
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
            chipManager = new ChipManager(this);

            //apiBridge = new APIBridge(this);
            if (defaultChips != null)
                CreateChips(defaultChips);
        }

        public void CreateChips(string[] chips)
        {
            foreach (var chip in chips)
                chipManager.GetChip(chip);
        }

        /// <summary>
        ///     This method resets the engine. This method only executes if the
        ///     engine is running.
        /// </summary>
        /// <tocexclude />
        public virtual void Reset()
        {
            if (!running)
                return;

            chipManager.Reset();
        }
        
        protected virtual void ConfigureInput()
        {
            if (inputFactory == null)
                return;

            controllerChip.RegisterKeyInput(inputFactory.CreateKeyInput());

            var buttons = Enum.GetValues(typeof(Buttons)).Cast<Buttons>();
            foreach (var button in buttons)
            {
                controllerChip.UpdateControllerKey(0, inputFactory.CreateButtonBinding(0, button));
                controllerChip.UpdateControllerKey(1, inputFactory.CreateButtonBinding(1, button));
            }

            controllerChip.RegisterMouseInput(inputFactory.CreateMouseInput());

        }

    }

}