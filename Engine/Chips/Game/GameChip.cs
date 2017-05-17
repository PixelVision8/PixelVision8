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
using UnityEngine;

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
    public class GameChip : AbstractChip, IUpdate, IDraw
    {
        protected string _name = "Untitle_Game";
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


        public enum DrawMode
        {
            Sprite,
            Tile,
            TilemapCache,
            SpriteBelow,
        }

        public enum FlipMode
        {
            None,
            Horizontal,
            Vertical,
            Both
        }

        public enum DepthMode
        {
            Above,
            Below
        }

        private int[] tmpPixelData;

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

        #region Chip Lifecycle

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
        ///     Used for updating the game's logic.
        /// </summary>
        /// <param name="timeDelta"></param>
        public virtual void Update(float timeDelta)
        {
            // Overwrite this method and add your own update logic.
        }

        /// <summary>
        ///     Used for drawing the game to the display.
        /// </summary>
        public virtual void Draw()
        {
            // Overwrite this method and add your own draw logic.
        }

        /// <summary>
        ///     This unloads the game from the engine.
        /// </summary>
        public override void Deactivate()
        {
            base.Deactivate();
            engine.currentGame = null;
        }

        #endregion


        public string name
        {
            get { return _name ?? GetType().Name; }
            set { _name = value; }
        }

        public bool ready { get; private set; }

        public string description { get; set; }

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

        public void WriteData(string key, string value)
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

        public string ReadData(string key, string defaultValue = "undefine")
        {
            if (!savedData.ContainsKey(key))
                WriteData(key, defaultValue);

            return savedData[key];
        }

        #region Pixel Vision APIs

        /// <summary>
        ///     Allows you to read or update the background color. You can use this method to only
        ///     return the value if you do not supply an argument.
        /// </summary>
        /// <param name="id">Pass in a new color ID to update the background color.</param>
        /// <returns>Returns the current background color ID.</returns>
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

        /// <summary>
        ///     Clears an area of the display using the background color. By not providing
        ///     any values, it will automatically clear the entire display
        /// </summary>
        /// <param name="x">The left position on the display where the clear should start.</param>
        /// <param name="y">The right position on the display where the clear should start.</param>
        /// <param name="width">The width of the clear in pixels.</param>
        /// <param name="height">The height of the clear in pixels.</param>
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
            return tilemapChip.ReadFlagAt(column, row);
        }

        public void DrawPixels(int[] pixelData, int x, int y, int width, int height, DrawMode mode = DrawMode.Sprite, bool flipH = false, bool flipV = false, int colorOffset = 0)
        {
            switch (mode)
            {   
                // Mode 0 & 1 are for sprites (above/below bg)
                case DrawMode.Sprite: case DrawMode.SpriteBelow:
                    var layerOrder = mode == 0 ? 1 : -1;

                    displayChip.NewDrawCall(pixelData, x, y, width, height, flipH, !flipV, true, layerOrder, false, colorOffset);

                    break;
                case DrawMode.TilemapCache:

                    //y = displaySize.y - y;

                    tilemapChip.UpdateCachedTilemap(pixelData, x, y, width, height);

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
            var mode = aboveBG ? DrawMode.Sprite : DrawMode.SpriteBelow;
            DrawPixels(tmpSpriteData, x, y, spriteChip.width, spriteChip.height, mode, flipH, !flipV, colorOffset);

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


        public int DrawText(string text, int x, int y, DrawMode mode = DrawMode.Sprite, string font = "Default", int colorOffset = 0, int spacing = 0, int? width = null)
        {
            if (width > 1)
            {
                text = FontChip.WordWrap(text, width.Value);
            }
            
            var result = text.Split(new[] { "\n", "\r\n" }, StringSplitOptions.None);
            var lines = result.Length;
            var charWidth = spriteSize.x;
            var charHeight = spriteSize.y;
            var nextX = x;
            var nextY = y;
            int tmpW, tmpH;

            for (int i = 0; i < lines; i++)
            {
                var line = result[i];
                var spriteIDs = fontChip.ConvertTextToSprites(line, font);
                var total = spriteIDs.Length;

                for (int j = 0; j < total; j++)
                {
                    if (mode == DrawMode.Tile)
                    {
                        DrawTile(spriteIDs[j], nextX, nextY, colorOffset);
                        nextX ++;
                    }
                    else if (mode == DrawMode.TilemapCache)
                    {
                        var pixelData = fontChip.ConvertCharacterToPixelData(line[j], font);
                        if (pixelData != null)
                        {
                            DrawPixels(pixelData, nextX, nextY, SpriteWidth(), SpriteHeight(), DrawMode.TilemapCache, false, false, colorOffset);
                        }

                        // Increase X even if no character was found
                        nextX += charWidth + spacing;

                    }
                    else
                    {
                        DrawSprite(spriteIDs[j], nextX, nextY, false, false, true, colorOffset);
                        nextX += charWidth + spacing;
                    }

                }

                nextX = x;

                if (mode == DrawMode.Tile)
                {
                    nextY ++;
                }
                else
                {
                    nextY += charHeight;
                }
            }

            return lines;
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
        
        /// <summary>
        ///     This draws the Tilemap Cache to the display. You can set the x and y position on the display where
        ///     the tilemap should go as well as how many rows and columns to display. By calling this method without
        ///     any arguments, the tilemap will be drawn in the upper left hand corner and fill the display.
        /// </summary>
        /// <param name="x">The left position where to draw the tilemap on the display.</param>
        /// <param name="y">The top position where to draw the tilemap on the display.</param>
        /// <param name="columns">How many horizontal tiles to include when drawing the map.</param>
        /// <param name="rows">How many vertical tiles to include when drawing the map.</param>
        public void DrawTilemap(int x = 0, int y = 0, int columns = 0, int rows = 0)
        {
            displayChip.DrawTilemap(x, y, columns, rows);
        }

        public bool MouseButton(int button)
        {
            return controllerChip.ReadMouseButton(button);
        }

        public int MouseX()
        {
            return controllerChip.mousePosition.x;
        }

        public int MouseY()
        {
            return controllerChip.mousePosition.y;
        }

        /// <summary>
        ///     This forces the map to redraw it's cached pixel data. Use this to clear any pixel data added
        ///     after the map created the pixel data cache.
        /// </summary>
        public void RebuildMap()
        {
            tilemapChip.ClearCache();
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

        public void Sound(int id, int channel = 0)
        {
            soundChip.PlaySound(id, channel);
        }

        public void Song(int id, bool loop = true)
        {
            if (id == -1)
            {
                musicChip.StopSong();
            }

            if (musicChip.currentSongID != id)
            {
                musicChip.LoadSong(id);
            }

            if (musicChip.songCurrentlyPlaying)
            {
                musicChip.PauseSong();
            }
            else
            {
                musicChip.PlaySong(loop);
            
            }
           
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

        public void UpdateTile(int column, int row, int? id = null, int? colorOffset = null, int? flag = null)
        {
            if(id.HasValue)
                tilemapChip.UpdateSpriteAt(column, row, id.Value);

            if (colorOffset.HasValue)
                tilemapChip.UpdateTileColorAt(column, row, colorOffset.Value);

            if (flag.HasValue)
                tilemapChip.UpdateFlagAt(column, row, flag.Value);
        }

        public void UpdateTiles(int column, int row, int columns, int rows, int[] ids = null, int[] colorOffsets = null, int[] flags = null)
        {
            var total = columns * rows;

            int? id, offset, flag;
            
            for (var i = 0; i < total; i++)
            {
                id = null;

                if (ids != null)
                {
                    id = ids[i];
                }

                var newX = MathUtil.FloorToInt(i % columns) + column;
                var newY = MathUtil.FloorToInt(i / (float)columns) + row;

                UpdateTile(newX, newY, id);
            }
        }

        public void ChangeColor(int index, int id)
        {
            colorChip.UpdateColorAt(index, colorChip.ReadColorAt(id));    
        }

//        //TODO need to refactor the name of SaveData above
//        public void WriteData(string key, string value)
//        {
//            SaveData(key, value);
//        }

        public Vector TilemapSize()
        {
            return new Vector(tilemapChip.columns, tilemapChip.rows);
        }


        #endregion

    }
}