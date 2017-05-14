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

using System;
using System.Collections.Generic;
using System.Linq;
using PixelVisionSDK.Utils;

//using UnityEngine;

namespace PixelVisionSDK.Chips
{
    /// <summary>
    ///     The GameChip represents the foundation of a game class
    ///     with all the logic it needs to work correctly in the PixelVisionEngine.
    ///     The Abstract class manages configuring the game when created via the
    ///     chip life-cycle, game state, the game's own life-cycle and
    ///     serialization/deserialization of the game's data.
    /// </summary>
    public class GameChip : AbstractChip, IGame, IGameAPI, IUpdate, IDraw
    {
        private string _name = "Untitle_Game";
        protected int _saveSlots;

        /// <summary>
        ///     Access to the core APIs of the engine. When building a game you'll
        ///     access these by directly talking to the <see cref="apiBridge" />
        ///     field.
        /// </summary>
        //public APIBridge apiBridge;

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

        public ChipManager chipManager
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public ColorChip colorChip {
            get { return engine.colorChip; }
        }
        public ColorMapChip colorMapChip
        {
            get { return engine.colorMapChip; }
        }
        public ControllerChip controllerChip
        {
            get { return engine.controllerChip; }
        }
        public DisplayChip displayChip
        {
            get { return engine.displayChip; }
        }
        public SoundChip soundChip
        {
            get { return engine.soundChip; }
        }
        public SpriteChip spriteChip
        {
            get { return engine.spriteChip; }
        }
        public TilemapChip tilemapChip
        {
            get { return engine.tilemapChip; }
        }
        public FontChip fontChip
        {
            get { return engine.fontChip; }
        }
        public MusicChip musicChip
        {
            get { return engine.musicChip; }
        }

        // Cached values
        protected Vector spriteSize { get; set; }
        protected Vector displaySize { get; set; }

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

            //TODO this needs to be a service
            //apiBridge = engine.apiBridge;
            ready = true;

            Array.Resize(ref tmpSpriteData, engine.spriteChip.width * engine.spriteChip.height);

            // To speed up accessing each chip, get a reference to them
//            colorChip = engine.colorChip;
//            colorMapChip = engine.colorMapChip;
//            controllerChip = engine.controllerChip;
//            displayChip = engine.displayChip;
//            soundChip = engine.soundChip;
//            spriteChip = engine.spriteChip;
//            tilemapChip = engine.tilemapChip;
//            fontChip = engine.fontChip;
//            musicChip = engine.musicChip;

            // cache used common properties
            spriteSize = new Vector(spriteChip.width, spriteChip.height);
            displaySize = new Vector(displayChip.width, displayChip.height);
        }

        /// <summary>
        ///     This unloads the game from the engine.
        /// </summary>
        public override void Deactivate()
        {
            base.Deactivate();
            engine.currentGame = null;
        }

        #region Pixel Vision APIs
        
        public int BackgroundColor(int? id = null)
        {
            if (id.HasValue)
            {
                colorChip.backgroundColor = MathUtil.Clamp(id.Value, 0, colorChip.total);
            }

            return colorChip.backgroundColor;
        }

        public bool Button(int button, int player)
        {
            var totalButtons = Enum.GetNames(typeof(Buttons)).Length;

            if (button >= totalButtons)
                return false;

            return controllerChip.ButtonDown(button, player);
        }

        public bool ButtonReleased(int button, int player)
        {
            var totalButtons = Enum.GetNames(typeof(Buttons)).Length;

            if (button >= totalButtons)
                return false;

            return controllerChip.ButtonReleased(button, player);
        }

        public void Clear(int x = 0, int y = 0, int width = 0, int height = 0)
        {
            displayChip.ClearArea(x, y, width, height);
        }

        public int DisplayHeight(bool visiblePixels = true)
        {
            var offset = visiblePixels ? displayChip.overscanYPixels : 0;

            return displayChip.height - offset;
        }

        public int DisplayWidth(bool visiblePixels = true)
        {
            var offset = visiblePixels ? displayChip.overscanXPixels : 0;

            return displayChip.width - offset;
        }

        public int Flag(int column, int row)
        {
            throw new NotImplementedException();
        }

        public void DrawPixels(int[] pixelData, int x, int y, int width, int height, int mode = 0, bool flipH = false, bool flipV = false, int colorOffset = 0)
        {
            switch (mode)
            {   
                // Mode 0 & 1 are for sprites (above/below bg)
                case 0: case 1:
                    var layerOrder = mode == 0 ? 1 : -1;

                    y += height - spriteChip.height;
                    
                    displayChip.NewDrawCall(pixelData, x, y, width, height, flipH, flipV, true, layerOrder, false, colorOffset);

                    break;
            }
            
        }

        private int[] tmpSpriteData = new int[0];

        public virtual void DrawSprite(int id, int x, int y, bool flipH = false, bool flipV = false, bool aboveBG = true, int colorOffset = 0)
        {
            if (!displayChip.CanDraw())
                return;

            //TODO flipping H, V and colorOffset should all be passed into reading a sprite
            spriteChip.ReadSpriteAt(id, tmpSpriteData);

            // Mode 0 is sprite above bg and mode 1 is sprite below bg.
            var mode = aboveBG ? 0 : 1;
            DrawPixels(tmpSpriteData, x, y, spriteChip.width, spriteChip.height, mode, flipH, flipV, colorOffset);

        }

        public void DrawSprites(int[] ids, int x, int y, int width, bool flipH = false, bool flipV = false, bool aboveBG = true, int colorOffset = 0)
        {
            var total = ids.Length;

            if (flipH || flipV)
            {
                var height = MathUtil.CeilToInt(total / width);
                SpriteChipUtil.FlipSpriteData(ref ids, width, height, flipH, flipV);
            }

            for (var i = 0; i < total; i++)
            {
                var id = ids[i];
                if (id > -1)
                {
                    //TODO should cache the sprite size value
                    var newX = MathUtil.FloorToInt(i % width) * spriteChip.width + x;
                    var newY = MathUtil.FloorToInt(i / width) * spriteChip.height + y;
                    DrawSprite(id, newX, newY, flipH, flipV, aboveBG, colorOffset);
                }
            }
        }

        public void DrawText(string text, int x, int y, int mode = 0, string font = "Default", int colorOffset = 0, int spacing = 0)
        {
            switch (mode)
            {
                case 0:

                    var width = spriteSize.x;
                    var nextX = x;

                    var spriteIDs = fontChip.ConvertTextToSprites(text, font);
                    var total = spriteIDs.Length;

                    // Draw each character
                    for (int i = 0; i < total; i++)
                    {
                        DrawSprite(spriteIDs[i], nextX, y, false, false, true, colorOffset);
                        nextX += width + spacing;
                    }

                    break;
            }
        }

        public void DrawTile(int id, int column, int row, int colorOffset = 0, int flag = -1)
        {
            if (column < 0 || column >= tilemapChip.columns || row < 0 || row >= tilemapChip.rows)
                return;

            tilemapChip.UpdateTileAt(id, column, row, flag, colorOffset);
        }

        public void DrawTiles(int[] ids, int column, int row, int columns, int colorOffset = 0, int flag = -1)
        {
            var total = ids.Length;

            for (var i = 0; i < total; i++)
            {
                var id = ids[i];
                if (id > -1)
                {
                    //TODO should cache the sprite size value
                    var newColumn = MathUtil.FloorToInt(i % columns + column);
                    var newRow = MathUtil.FloorToInt(i / columns + row);
                    DrawTile(ids[i], newColumn, newRow, colorOffset, flag);
                }
            }
        }

        public void DrawTilemap(int x = 0, int y = 0, int columns = 0, int rows = 0)
        {
            displayChip.DrawTilemap(x, y, columns, rows);
        }

        //TODO need to refactor GetData above
        public string ReadData(string key)
        {
            return GetData(key, "undefined");
        }

        public void RebuildMap()
        {
            throw new NotImplementedException();
        }

        public void ScrollTo(int x = 0, int y = 0)
        {
            displayChip.scrollX = x;
            displayChip.scrollY = y;
        }

        public int ScrollX()
        {
            return displayChip.scrollX;
        }

        public int ScrollY()
        {
            return displayChip.scrollY;
        }

        public void Sfx(int id, int channel = 0)
        {
            soundChip.PlaySound(id, channel);
        }

        public void Song(int id, bool loop = true)
        {
            throw new NotImplementedException();
        }

        public int SpriteHeight()
        {
            return spriteChip.height;
        }

        public int SpriteWidth()
        {
            return spriteChip.width;
        }

        public int TilemapWidth()
        {
            return tilemapChip.columns;
        }

        public int TilemapHeight()
        {
            return tilemapChip.rows;
        }

        public Vector SpriteSize()
        {
            throw new NotImplementedException();
        }

        public void UpdateSprite(int id, int[] pixelData)
        {
            throw new NotImplementedException();
        }

        public void UpdateTile(int column, int row, int id = -1, int colorOffset = -1, int flag = -1)
        {
            if(id > -1)
                tilemapChip.UpdateSpriteAt(column, row, id);

            if (colorOffset > -1)
                tilemapChip.UpdateTileColorAt(column, row, colorOffset);

            if (flag > -1)
                tilemapChip.UpdateFlagAt(column, row, flag);
        }

        public void ChangeColor(int index, int id)
        {
            colorChip.UpdateColorAt(index, colorChip.ReadColorAt(id));    
        }

        //TODO need to refactor the name of SaveData above
        public void WriteData(string key, string value)
        {
            SaveData(key, value);
        }

        public Vector TilemapSize()
        {
            return new Vector(tilemapChip.columns, tilemapChip.rows);
        }


        #endregion

    }
}