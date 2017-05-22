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

namespace PixelVisionSDK.Chips
{
    public enum DrawMode
    {
        Sprite,
        Tile,
        TilemapCache,
        SpriteBelow
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

    public enum InputState
    {
        Down,
        Released
    }

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
        private int[] tmpSpriteData = new int[0];

        /// <summary>
        ///     Access to the core APIs of the engine. When building a game you'll
        ///     access these by directly talking to the <see cref="apiBridge" />
        ///     field.
        /// </summary>
        /// <summary>
        ///     Flag for the maximum size the game should be.
        /// </summary>
        public int maxSize = 256;

        protected Dictionary<string, string> savedData = new Dictionary<string, string>();

        private int[] tmpPixelData;

        private readonly Dictionary<string, int> tmpTileData = new Dictionary<string, int>
        {
            {"spriteID", -1},
            {"colorOffset", -1},
            {"flag", -1}
        };

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


        public string name
        {
            get { return _name ?? GetType().Name; }
            set { _name = value; }
        }

        public bool ready { get; private set; }

        public string description { get; set; }

        public ColorChip colorChip
        {
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


        #region Color APIs

        /// <summary>
        ///     Allows you to read or update the background color. You can use this method to only
        ///     return the value if you do not supply an argument.
        /// </summary>
        /// <param name="id">Pass in a new color ID to update the background color.</param>
        /// <returns>Returns the current background color ID.</returns>
        public int BackgroundColor(int? id = null)
        {
            if (id.HasValue)
                colorChip.backgroundColor = id.Value.Clamp(0, colorChip.total);

            return colorChip.backgroundColor;
        }

        public string Color(int id, string value = null)
        {
            if (value == null)
                return colorChip.ReadColorAt(id);

            colorChip.UpdateColorAt(id, value);

            return value;
        }


        public int TotalColors(bool ignoreEmpty = true)
        {
            return colorChip.total;
        }

        public int ColorsPerSprite()
        {
            return spriteChip.colorsPerSprite;
        }

        public void ReplaceColor(int index, int id)
        {
            colorChip.UpdateColorAt(index, colorChip.ReadColorAt(id));
        }

        #endregion

        #region Display APIs

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

        public Vector DisplaySize(int? x = null, int? y = null)
        {
            var size = new Vector();
            var resize = false;

            if (x.HasValue)
            {
                size.x = x.Value;
                resize = true;
            }
            else
            {
                size.x = displayChip.width;
            }

            if (y.HasValue)
            {
                size.y = y.Value;
                resize = true;
            }
            else
            {
                size.y = displayChip.height;
            }

            if (resize)
                displayChip.ResetResolution(size.x, size.y);

            return size;
        }

        public Vector OverscanBorder(int? x, int? y)
        {
            var size = new Vector();

            if (x.HasValue)
            {
                size.x = x.Value;
                displayChip.overscanX = size.x;
            }
            else
            {
                size.x = displayChip.width;
            }

            if (y.HasValue)
            {
                size.y = y.Value;
                displayChip.overscanY = size.y;
            }
            else
            {
                size.y = displayChip.height;
            }

            return size;
        }


        public void DrawPixels(int[] pixelData, int x, int y, int width, int height, DrawMode mode = DrawMode.Sprite,
            bool flipH = false, bool flipV = false, int colorOffset = 0)
        {
            switch (mode)
            {
                // Mode 0 & 1 are for sprites (above/below bg)
                case DrawMode.Sprite:
                case DrawMode.SpriteBelow:
                    var layerOrder = mode == 0 ? 1 : -1;

                    displayChip.NewDrawCall(pixelData, x, y, width, height, flipH, !flipV, true, layerOrder, false, colorOffset);

                    break;
                case DrawMode.TilemapCache:

                    //y = displaySize.y - y;

                    tilemapChip.UpdateCachedTilemap(pixelData, x, y, width, height, colorOffset);

                    break;
            }
        }

        public virtual void DrawSprite(int id, int x, int y, bool flipH = false, bool flipV = false,
            bool aboveBG = true, int colorOffset = 0)
        {
            if (!displayChip.CanDraw())
                return;

            //TODO flipping H, V and colorOffset should all be passed into reading a sprite
            spriteChip.ReadSpriteAt(id, tmpSpriteData);

            // Mode 0 is sprite above bg and mode 1 is sprite below bg.
            var mode = aboveBG ? DrawMode.Sprite : DrawMode.SpriteBelow;
            DrawPixels(tmpSpriteData, x, y, spriteChip.width, spriteChip.height, mode, flipH, !flipV, colorOffset);
        }

        public void DrawSprites(int[] ids, int x, int y, int width, bool flipH = false, bool flipV = false,
            bool aboveBG = true, int colorOffset = 0, bool onScreen = true)
        {
            var size = SpriteSize();
            var sW = size.x;
            var sH = size.y;

            var displaySize = DisplaySize();

            var bounds = new Rect(-displayChip.overscanXPixels, -displayChip.overscanYPixels, displaySize.x - displayChip.overscanXPixels, displaySize.y - displayChip.overscanYPixels);
            var total = ids.Length;

            var height = MathUtil.CeilToInt(total / width);

            var startX = x - (onScreen ? displayChip.scrollX : 0);
            var startY = y + (onScreen ? displayChip.scrollY : 0);

            if (flipH || flipV)
                SpriteChipUtil.FlipSpriteData(ref ids, width, height, flipH, flipV);

            for (var i = 0; i < total; i++)
            {
                var id = ids[i];
                if (id > -1)
                {
                    x = MathUtil.FloorToInt(i % width) * sW + startX;
                    y = MathUtil.FloorToInt(i / width) * sH + startY;

                    var render = true;

                    if (onScreen)
                        render = x >= bounds.x && x <= bounds.width && y >= bounds.y && y <= bounds.height;

                    if (render)
                        DrawSprite(id, x, y, flipH, flipV, aboveBG, colorOffset);
                }
            }
        }


        public int DrawText(string text, int x, int y, DrawMode mode = DrawMode.Sprite, string font = "Default",
            int colorOffset = 0, int spacing = 0, int? width = null)
        {
            if (width > 1)
                text = FontChip.WordWrap(text, width.Value);

            var result = text.Split(new[] { "\n", "\r\n" }, StringSplitOptions.None);
            var lines = result.Length;

            var spriteSize = SpriteSize();

            var charWidth = spriteSize.x;
            var charHeight = spriteSize.y;
            var nextX = x;
            var nextY = y;
            int tmpW, tmpH;


            for (var i = 0; i < lines; i++)
            {
                var line = result[i];
                var spriteIDs = fontChip.ConvertTextToSprites(line, font);
                var total = spriteIDs.Length;

                for (var j = 0; j < total; j++)
                    if (mode == DrawMode.Tile)
                    {
                        Tile(nextX, nextY, spriteIDs[j], colorOffset);
                        nextX++;
                    }
                    else if (mode == DrawMode.TilemapCache)
                    {
                        var pixelData = fontChip.ConvertCharacterToPixelData(line[j], font);
                        if (pixelData != null)
                            DrawPixels(pixelData, nextX, nextY, spriteSize.x, spriteSize.y, DrawMode.TilemapCache,
                                false, false, colorOffset);

                        // Increase X even if no character was found
                        nextX += charWidth + spacing;
                    }
                    else
                    {
                        DrawSprite(spriteIDs[j], nextX, nextY, false, false, true, colorOffset);
                        nextX += charWidth + spacing;
                    }

                nextX = x;

                if (mode == DrawMode.Tile)
                    nextY++;
                else
                    nextY += charHeight;
            }

            return lines;
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

        public void RedrawDisplay()
        {
            Clear();
            DrawTilemap();
        }

        public Vector ScrollPosition(int? x = null, int? y = null)
        {
            var pos = new Vector();

            if (x.HasValue)
            {
                pos.x = x.Value;
                displayChip.scrollX = pos.x;
            }
            else
            {
                pos.x = displayChip.scrollX;
            }

            if (y.HasValue)
            {
                pos.y = y.Value;
                displayChip.scrollY = pos.y;
            }
            else
            {
                pos.y = displayChip.scrollY;
            }

            return pos;
        }

        #endregion

        #region File IO APIs

        public void WriteSaveData(string key, string value)
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

        public string ReadSaveData(string key, string defaultValue = "undefine")
        {
            if (!savedData.ContainsKey(key))
                WriteSaveData(key, defaultValue);

            return savedData[key];
        }

        #endregion

        #region Input APIs

        public bool Key(Keys key, InputState state = InputState.Down)
        {
            return state == InputState.Released
                ? controllerChip.GetKeyUp((int)key)
                : controllerChip.GetKeyDown((int)key);
        }

        public bool MouseButton(int button, InputState state = InputState.Down)
        {
            return state == InputState.Released
                ? controllerChip.GetMouseButtonUp(button)
                : controllerChip.GetMouseButtonDown(button);
        }

        public bool Button(Buttons buttons, InputState state = InputState.Down, int player = 0)
        {
            return state == InputState.Released
                ? controllerChip.ButtonReleased((int)buttons, player)
                : controllerChip.ButtonDown((int)buttons, player);
        }

        public Vector MousePosition()
        {
            return controllerChip.ReadMousePosition();
        }

        #endregion

        #region Math APIs

        #endregion

        #region Sound APIs

        public void PlaySound(int id, int channel = 0)
        {
            soundChip.PlaySound(id, channel);
        }

        public void PlaySong(int[] trackIDs, bool loop = true)
        {
            var track = trackIDs[0];

            musicChip.LoadSong(track);

            musicChip.PlaySong(loop);
        }

        public void PauseSong()
        {
            musicChip.PauseSong();
        }

        public void StopSong()
        {
            musicChip.StopSong();
        }

        public void RewindSong(int position = 0, int loopID = 0)
        {
            //TODO need to add in better support for rewinding a song across multiple loops
            musicChip.RewindSong();
        }

        #endregion

        #region Sprite APIs

        public Vector SpriteSize(int? width = 8, int? height = 8)
        {
            var size = new Vector(spriteChip.width, spriteChip.height);

            // TODO you can't resize sprites at runtime

            return size;
        }

        public int[] Sprite(int id, int[] data = null)
        {
            if (data != null)
            {
                spriteChip.UpdateSpriteAt(id, data);
                tilemapChip.InvalidateTileID(id);

                return data;
            }

            spriteChip.ReadSpriteAt(id, tmpPixelData);

            return tmpPixelData;
        }

        public int TotalSprites(bool ignoreEmpty = true)
        {
            return spriteChip.spritesInRam;
        }

        #endregion

        #region Tilemap

        public int Flag(int column, int row, int? value = null)
        {
            if (value.HasValue)
                tilemapChip.UpdateFlagAt(column, row, value.Value);

            return tilemapChip.ReadFlagAt(column, row);
        }

        public Dictionary<string, int> Tile(int column, int row, int? spriteID = null, int? colorOffset = null,
            int? flag = null)
        {
            if (spriteID.HasValue)
                tilemapChip.UpdateSpriteAt(column, row, spriteID.Value);

            if (colorOffset.HasValue)
                tilemapChip.UpdateTileColorAt(column, row, colorOffset.Value);

            if (flag.HasValue)
                tilemapChip.UpdateFlagAt(column, row, flag.Value);

            tmpTileData["spriteID"] = tilemapChip.ReadSpriteAt(column, row);
            tmpTileData["colorOffset"] = tilemapChip.ReadTileColorAt(column, row);
            tmpTileData["flag"] = tilemapChip.ReadFlagAt(column, row);

            return tmpTileData;
        }

        /// <summary>
        ///     This forces the map to redraw it's cached pixel data. Use this to clear any pixel data added
        ///     after the map created the pixel data cache.
        /// </summary>
        public void RebuildTilemap(int? columns = null, int? rows = null, int[] spriteIDs = null,
            int[] colorOffsets = null, int[] flags = null, int columnOffset = 0, int rowOffset = 0)
        {
            // TODO need to finish this method
            // If columns and rows are shown resize map
            // If sprites, colors or flags are used fill them in
            // If offset is present fill in new values at that offset


            tilemapChip.ClearCache();
        }

        public Vector TilemapSize(int? width = null, int? height = null)
        {
            var size = new Vector(tilemapChip.columns, tilemapChip.rows);

            var resize = false;

            if (width.HasValue)
            {
                size.x = width.Value;
                resize = true;
            }

            if (height.HasValue)
            {
                size.y = height.Value;
                resize = true;
            }

            if (resize)
                tilemapChip.Resize(size.x, size.y);

            return size;
        }

        public void UpdateTiles(int column, int row, int columns, int[] ids, int? colorOffset = null, int? flag = null)
        {
            var total = ids.Length;

            int id, newX, newY;

            //TODO need to get offset and flags working

            for (var i = 0; i < total; i++)
            {
                id = ids[i];

                newX = MathUtil.FloorToInt(i % columns) + column;
                newY = MathUtil.FloorToInt(i / (float)columns) + row;

                Tile(newX, newY, id, colorOffset, flag);
            }
        }

        #endregion

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
            engine.gameChip = this;

            //TODO this needs to be a service
            //apiBridge = engine.apiBridge;
            ready = true;

            Array.Resize(ref tmpSpriteData, engine.spriteChip.width * engine.spriteChip.height);

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
            engine.gameChip = null;
        }

        #endregion

    }
}