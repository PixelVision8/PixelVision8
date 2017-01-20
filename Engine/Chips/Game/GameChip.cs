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

using System.Collections.Generic;
using System.Linq;
using System.Text;
using PixelVisionSDK.Utils;

//using UnityEngine;

namespace PixelVisionSDK.Chips
{

    /// <summary>
    ///     The <see cref="GameChip" /> represents the foundation of a game class
    ///     with all the logic it needs to work correctly in the PixelVisionEngine.
    ///     The Abstract class manages configuring the game when created via the
    ///     chip life-cycle, game state, the game's own life-cycle and
    ///     serialization/deserialization of the game's data.
    /// </summary>
    public class GameChip : AbstractChip, IGame, IUpdate, IDraw
    {

        private string _name = "Untitle_Game";
        protected int _saveSlots;

        /// <summary>
        ///     Access to the core APIs of the engine. When building a game you'll
        ///     access these by directly talking to the <see cref="apiBridge" />
        ///     field.
        /// </summary>
        public IPixelVisionAPI apiBridge;

        /// <summary>
        ///     Flag for the maximum size the game should be.
        /// </summary>
        public int maxSize = 256;

        protected Dictionary<string, string> savedData = new Dictionary<string, string>();

        /// <summary>
        ///     Used to limit the amount of data the game can save.
        /// </summary>
        public int saveSlots
        {
            get { return _saveSlots; }
            set
            {
                value = value.Clamp(8, 96);
                _saveSlots = value;

                // resize dictionary?
                for (var i = savedData.Count - 1; i >= 0; i--)
                {
                    var item = savedData.ElementAt(i);
                    if (i > value)
                        savedData.Remove(item.Key);
                }
            }
        }

        /// <summary>
        ///     Used for drawing the game to the display.
        /// </summary>
        public virtual void Draw()
        {
        }

        public string name
        {
            get { return _name ?? GetType().Name; }
            set { _name = value; }
        }

        public bool ready { get; private set; }

        public string description { get; set; }

        public void SaveData(string key, string value)
        {
            if (savedData.Count > saveSlots)
                return;

            if (savedData.ContainsKey(key))
            {
                savedData[key] = value;
                return;
            }

            savedData.Add(key, value);
        }

        public void SaveData(string key, int value)
        {
            SaveData(key, value.ToString());
        }

        public void SaveData(string key, float value)
        {
            SaveData(key, value.ToString());
        }

        public string GetData(string key, string defaultValue)
        {
            if (!savedData.ContainsKey(key))
                SaveData(key, defaultValue);

            return savedData[key];
        }

        public int GetData(string key, int defaultValue)
        {
            return int.Parse(GetData(key, defaultValue.ToString()));
        }

        public float GetData(string key, float defaultValue)
        {
            return float.Parse(GetData(key, defaultValue.ToString()));
        }

        public Dictionary<string, object> GenerateMetaData()
        {
            var metaData = new Dictionary<string, object>();

            metaData.Add("name", name);
            metaData.Add("description", description);

            return metaData;
        }

        public void LoadMetaData(Dictionary<string, object> metaData)
        {
            if (metaData == null)
                return;

            if (metaData.ContainsKey("name"))
                name = metaData["name"] as string;

            if (metaData.ContainsKey("description"))
                description = metaData["description"] as string;
        }

        /// <summary>
        ///     Used for updating the game's logic.
        /// </summary>
        /// <param name="timeDelta"></param>
        public virtual void Update(float timeDelta)
        {
        }

        /// <summary>
        ///     Configures the <see cref="GameChip" /> instance by loading it into
        ///     the engine's memory, getting a reference to the
        ///     <see cref="APIBridge" /> and setting the <see cref="ready" /> flag to
        ///     true.
        /// </summary>
        public override void Configure()
        {
            //Debug.Log("Game: Configure");
            engine.currentGame = this;

            apiBridge = engine.apiBridge;
            ready = true;
        }

        /// <summary>
        ///     This unloads the game from the engine.
        /// </summary>
        public override void Deactivate()
        {
            base.Deactivate();
            engine.currentGame = null;
        }

        

    }

}