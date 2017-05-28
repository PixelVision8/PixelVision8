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

    public enum InputState
    {
        Down,
        Released
    }

    /// <summary>
    ///     The GameChip represents the foundation of a game class
    ///     with all the logic it needs to work correctly in the PixelVisionEngine.
    ///     The AbstractChip class manages configuring the game when created via the
    ///     chip life-cycle. The engine manages the game's state, the game's own life-cycle and
    ///     serialization/deserialization of the game's data.
    /// </summary>
    public class GameChip : AbstractChip, IUpdate, IDraw
    {
        protected string _name = "Untitle_Game";
        protected int _saveSlots;
        protected Dictionary<string, string> savedData = new Dictionary<string, string>();

        private int[] tmpSpriteData = new int[0];
        private int[] tmpPixelData;

        protected Vector spriteSize { get; set; }
        protected Vector displaySize { get; set; }

        private readonly Dictionary<string, int> tmpTileData = new Dictionary<string, int>
        {
            {"spriteID", -1},
            {"colorOffset", -1},
            {"flag", -1}
        };

        #region GameChip APIs
        
        /// <summary>
        ///     Flag for the maximum size the game should be.
        /// </summary>
        public int maxSize = 256;

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
        ///     Name of the game.
        /// </summary>
        public string name
        {
            get { return _name ?? GetType().Name; }
            set { _name = value; }
        }

        /// <summary>
        ///     Returns true if the game is ready to be run.
        /// </summary>
        public bool ready { get; private set; } //TODO remove this, it's not really needed

        /// <summary>
        ///     The description for the game.
        /// </summary>
        public string description { get; set; }

        #endregion


        #region Chip References
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

        #endregion

        #region Color APIs

        /// <summary>
        ///     The background color is used to fill the screen when clearing the display. You can use 
        ///     this method to read or update the background color at any point during the GameChip's 
        ///     draw phase. When calling BackgroundColor(), without an argument, it returns the current 
        ///     background color int. You can pass in an optional int to update the background color by 
        ///     calling BackgroundColor(0) where 0 is any valid ID in the ColorChip. Passing in a value 
        ///     such as -1, or one that is out of range, defaults the background color to magenta (#ff00ff) 
        ///     which is the engine's default transparent color.
        /// </summary>
        /// <param name="id">
        ///     This argument is optional. Supply an int to update the existing background color value.
        /// </param>
        /// <returns>
        ///     This method returns the current background color ID. If no color exists, it returns -1 
        ///     which is magenta (#FF00FF).
        /// </returns>
        public int BackgroundColor(int? id = null)
        {
            if (id.HasValue)
                colorChip.backgroundColor = id.Value.Clamp(0, colorChip.total);

            return colorChip.backgroundColor;
        }

        /// <summary>
        ///     The Color() method allows you to read and update color values in the ColorChip. This 
        ///     method has two modes which require a color ID to work. By calling the method with just 
        ///     an ID, like Color(0), it returns a hex string for the given color at the supplied color 
        ///     ID. By passing in a new hex string, like Color(0, "#FFFF00"), you can change the color 
        ///     with the given ID. While you can use this method to modify color values directly, you 
        ///     should avoid doing this at run time since the DisplayChip must parse and cache the new 
        ///     hex value. If you just want to change a color to an existing value, use the ReplaceColor() 
        ///     method.
        /// </summary>
        /// <param name="id">
        ///     The ID of the color you want to access.
        /// </param>
        /// <param name="value">
        ///     This argument is optional. It accepts a hex as a string and updates the supplied color ID's value.
        /// </param>
        /// <returns>
        ///     This method returns a hex string for the supplied color ID. If the color has not been set 
        ///     or is out of range, it returns magenta (#FF00FF) which is the default transparent system color.
        /// </returns>
        public string Color(int id, string value = null)
        {
            if (value == null)
                return colorChip.ReadColorAt(id);

            colorChip.UpdateColorAt(id, value);

            return value;
        }

        /// <summary>
        ///     The TotalColors() method simply returns the total number of colors in the ColorChip. By default, 
        ///     it returns only colors that have been set to value other than magenta (#FF00FF) which is the 
        ///     default transparent value used by the engine. By calling TotalColors(false), it returns the total 
        ///     available color slots in the ColorChip.
        /// </summary>
        /// <param name="ignoreEmpty">
        ///     This is an optional value that defaults to true. When set to true, the ColorChip returns the total 
        ///     number of colors not set to magenta (#FF00FF). Set this value to false if you want to get all of 
        ///     the available color slots in the ColorChip regardless if they are empty or not.
        /// </param>
        /// <returns>
        ///     This method returns the total number of colors in the color chip based on the ignoreEmpty argument's 
        ///     value.
        /// </returns>
        public int TotalColors(bool ignoreEmpty = true)
        {
            return colorChip.total;
        }

        /// <summary>
        ///     Pixel Vision 8 sprites have limits around how many colors they can display at once which is called 
        ///     the Colors Per Sprite or CPS. The ColorsPerSprite() method returns this value from the SpriteChip. 
        ///     While this is read-only at run-time, it has other important uses. If you set up your ColorChip in 
        ///     palettes, grouping sets of colors together based on the SpriteChip's CPS value, you can use this to 
        ///     shift a sprite's color offset up or down by a fixed amount when drawing it to the display. Since this 
        ///     value does not change when a game is running, it is best to get a reference to it when the game starts 
        ///     up and store it in a local variable.
        /// </summary>
        /// <returns>
        ///     This method returns the Color Per Sprite limit value as an int.
        /// </returns>
        public int ColorsPerSprite()
        {
            return spriteChip.colorsPerSprite;
        }

        /// <summary>
        ///     The ReplaceColor() method allows you to quickly change a color to an existing color without triggering 
        ///     the DisplayChip to parse and cache a new hex value. Consider this an alternative to the Color() method. 
        ///     It is useful for simulating palette swapping animation on sprites pointed to a fixed group of color IDs. 
        ///     Simply cal the ReplaceColor() method and supply a target color ID position, then the new color ID it 
        ///     should point to. Since you are only changing the color's ID pointer, there is little to no performance 
        ///     penalty during the GameChip's draw phase.
        /// </summary>
        /// <param name="index">The ID of the color you want to change.</param>
        /// <param name="id">The ID of the color you want to replace it with.</param>
        public void ReplaceColor(int index, int id)
        {
            colorChip.UpdateColorAt(index, colorChip.ReadColorAt(id));
        }

        #endregion

        #region Display APIs

        /// <summary>
        ///     Clears an area of the display using the background color. By not providing
        ///     any values, it will automatically clear the entire display. You can manually defaine
        ///     an area of the screen to clear by supplying an option x, y, witdh and height value.
        ///     When clearing a specific area of the display, anything outside of the defined 
        ///     boundaries will not be cleared. Use this for drawing a HUD but clearing the display 
        ///     below for a scrolling map.
        /// </summary>
        /// <param name="x">The left position on the display where the clear should start.</param>
        /// <param name="y">The right position on the display where the clear should start.</param>
        /// <param name="width">The width of the clear in pixels.</param>
        /// <param name="height">The height of the clear in pixels.</param>
        public void Clear(int x = 0, int y = 0, int width = 0, int height = 0)
        {
            displayChip.ClearArea(x, y, width, height);
        }

        /// <summary>
        ///     Allows you to get the resolution of the display at run time. While you can also define 
        ///     a new resolution by providing a width and height value, this may not work correctly at 
        ///     runtime. You should instead define the resolution before loading the game. If you are 
        ///     using overscane, you will need to subtract it from the width and height of the returned
        ///     vector to find the visible pixel dimensions. 
        /// </summary>
        /// <param name="width">New width for the display.</param>
        /// <param name="height">New height for the display.</param>
        /// <returns>Returns a Vector for the display's size. The X and Y values refere to the pixel 
        /// width and height of the display.</returns>
        public Vector DisplaySize(int? width = null, int? height = null)
        {
            var size = new Vector();
            var resize = false;

            if (width.HasValue)
            {
                size.x = width.Value;
                resize = true;
            }
            else
            {
                size.x = displayChip.width;
            }

            if (height.HasValue)
            {
                size.y = height.Value;
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

        /// <summary>
        ///     The overscan value allows you to define parts of the screen that will be cut off 
        ///     based on how older CRT TVs rendered inmages. The overscan area allows you to hide
        ///     sprites off screen so they do not wrap around the edges. Pixel Vision 8 automatically
        ///     crops the display to refelect the overscan. So a resolution of 256x244 with an 
        ///     overscan x and y value of 1 will only display 248x236 pixels.
        /// </summary>
        /// <param name="x">The number of columns from the right edge to use. Each column removes 8 pixels.</param>
        /// <param name="y">The number of rows from the bottom edge to use. Each row removes 8 pixels.</param>
        /// <returns>Returns the value of the overscan. Each value needs to be multiplied by 8 to get 
        /// the actual pixel size of the overscan border. Use this value to create a safe location 
        /// offscreen to hide sprites when not needed.</returns>
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

        /// <summary>
        ///     This method allows you to draw pixel data directly on the display. Depending on which display 
        ///     mode you use, the pixel data could be used as a sprite or drawn directly onto the tilemap 
        ///     cache. Sprited drawn with this method still count against the total number the display can 
        ///     render but you can draw iregular shaped sprites by defining a unique width and height. For 
        ///     drawnig into the tilemap cahce directly, you can use this to change the way the tilemap looks 
        ///     at run time. It's important to remember if you change a tile's sprite ID or color offset it 
        ///     it will be redrawn to the cache overwritting any pixel data that was drawn over it.
        /// </summary>
        /// <param name="pixelData"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="drawMode"></param>
        /// <param name="flipH"></param>
        /// <param name="flipV"></param>
        /// <param name="colorOffset"></param>
        public void DrawPixels(int[] pixelData, int x, int y, int width, int height, DrawMode drawMode = DrawMode.Sprite, bool flipH = false, bool flipV = false, int colorOffset = 0)
        {
            switch (drawMode)
            {
                // Mode 0 and 1 are for sprites (above/below bg)
                case DrawMode.Sprite:
                case DrawMode.SpriteBelow:
                    var layerOrder = drawMode == 0 ? 1 : -1;

                    displayChip.NewDrawCall(pixelData, x, y, width, height, flipH, !flipV, true, layerOrder, false, colorOffset);

                    break;
                case DrawMode.TilemapCache:

                    //y = displaySize.y - y;

                    tilemapChip.UpdateCachedTilemap(pixelData, x, y, width, height, colorOffset);

                    break;
            }
        }

        /// <summary>
        ///     Draws a sprite to the display. Each sprite counts against the total amount the display is 
        ///     able to render. IF you attempt to draw more sprites than the display can handle the call 
        ///     will be ignored.
        /// </summary>
        /// <param name="id">The ID of the sprite in the SpriteChip.</param>
        ///<param name = "x" >
        ///     X position to place sprite on the screen. 0 is left side of screen.
        /// </param>
        /// <param name="y">
        ///     Y position to place sprite on the screen. 0 is the top of the
        ///     screen.
        /// </param>
        /// <param name="flipH">
        ///     <para>
        ///         This flips the sprite horizontally. Set to false
        ///     </para>
        ///     <para>by default.</para>
        /// </param>
        /// <param name="flipV">
        ///     This flips the sprite vertically. Set to false by
        ///     default.
        /// </param>
        /// <param name="aboveBG">
        ///     Defines if the sprite is above or below the background layer. Set to
        ///     true by default.
        /// </param>
        /// <param name="colorOffset">
        ///     This value offsets all the color ID's of the sprite. Use this to simulate palette shifting.
        /// </param>
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

        /// <summary>
        ///     Draws a group of sprites in a grid. This is useful when trying to
        ///     draw sprites larger than 8x8. Each sprite in the ids array still counts as an 
        ///     individual sprite so it will only render as many sprites that are remaining during the
        ///     draw pass. This method will automatically hide sprites that go offscreen. When used 
        ///     with overscan border, it will greatly simplify drawing larger sprites to the display.
        /// </summary>
        ///<param name="id">The ID of the sprite in the SpriteChip.</param>
        ///<param name = "x" >
        ///     X position to place sprite on the screen. 0 is left side of screen.
        /// </param>
        /// <param name="y">
        ///     Y position to place sprite on the screen. 0 is the top of the
        ///     screen.
        /// </param>
        /// <param name="width">The width in sprites of the grid. A value of 2 will be 2 sprites wide.</param>
        /// <param name="flipH">
        ///     <para>
        ///         This flips the sprite horizontally. Set to false
        ///     </para>
        ///     <para>by default.</para>
        /// </param>
        /// <param name="flipV">
        ///     This flips the sprite vertically. Set to false by
        ///     default.
        /// </param>
        /// <param name="aboveBG">
        ///     Defines if the sprite is above or below the background layer. Set to
        ///     true by default.
        /// </param>
        /// <param name="colorOffset">
        ///     This value offsets all the color ID's of the sprite. Use this to simulate palette shifting.
        /// </param>
        /// <param name="onScreen">This flag defines if the sprites should be hidden when they are off 
        /// the screen. Use this in conjuntion with overscan border. If set to false, the sprites will
        /// wrap around the screen when they reach the edges of the display.</param>
        public void DrawSprites(int[] ids, int x, int y, int width, bool flipH = false, bool flipV = false, bool aboveBG = true, int colorOffset = 0, bool onScreen = true)
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

        /// <summary>
        ///     This allows you to draw text to the display. There are several modes allowing you to 
        ///     specify if the text should be rendered as spirtes, tiles or drawn directly into the 
        ///     tilemap cache. You can also define the color offset, letter spacing which only works 
        ///     for sprite and tilemap cache rendering, and a width in characters if you want the   
        ///     text to wrap. When rendering text as sprites, each character will count towards the 
        ///     maximumn number of sprites the display can render.
        /// </summary>
        /// <param name="text">
        ///     String that will be rendered to the display.
        /// </param>
        /// <param name="x">
        ///     X position where <paramref name="text" /> starts on the screen. 0 is
        ///     left side of screen.
        /// </param>
        /// <param name="y">
        ///     Y position where <paramref name="text" /> starts on the screen. 0 is
        ///     top side of screen.
        /// </param>
        /// <param name="mode">
        ///     This accepts the DrawMode enum. It supports drawing text as Sprite, Tilemap or TilemapCache.
        /// </param>
        /// <param name="font">The name of the font to use. You don't need to add the font's file extension.</param>
        /// <param name="colorOffset">Shift the color IDs by this value</param>
        /// <param name="spacing">The number of pixels between each character. This is ignored when
        /// rendering text as tiles.</param>
        /// <param name="width">The width in characters before the text should wrap. Leaving this empty
        /// will not force the text to wrap.</param>
        /// <returns></returns>
        public int DrawText(string text, int x, int y, DrawMode mode = DrawMode.Sprite, string font = "Default", int colorOffset = 0, int spacing = 0, int? width = null)
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

        /// <summary>
        ///     This is a helper method to make clearing the display and drawing the tilemap easier. Use this method 
        ///     to call both methods at the same time.
        /// </summary>
        public void RedrawDisplay()
        {
            Clear();
            DrawTilemap();
        }

        /// <summary>
        ///     This allows you to scroll start position (upper top left) of where the tilemap should start rendering 
        ///     at. This allows you to scroll the tilemap. By calling the method with no arguments you can get the 
        ///     current scroll position. If you supply an X and Y value it will update the display's scroll position. 
        /// </summary>
        /// <param name="x">Optional argument to update the scroll x position. The x position is the far left side of 
        /// the tilemap.</param>
        /// <param name="y">Optional argiment to update the scorll y position. The y position is at the top side of the 
        /// tilemap.</param>
        /// <returns>This method returns a vector with the current scroll x and y position.</returns>
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

        /// <summary>
        ///     Allows you to save string data to the game itself. This data can then be used when restarting a 
        ///     game for persistent data.
        /// </summary>
        /// <param name="key">A string to use as the key for the data.</param>
        /// <param name="value">A string representing the data to be saved.</param>
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

        /// <summary>
        ///     Allows you to read saved data by supplying a key. If no key is provied "undefined" will be returned.
        /// </summary>
        /// <param name="key">The string key used to find the data.</param>
        /// <param name="defaultValue">The optional string to be used if no data is found.</param>
        /// <returns>Returns string data associated with the supplied key.</returns>
        public string ReadSaveData(string key, string defaultValue = "undefine")
        {
            if (!savedData.ContainsKey(key))
                WriteSaveData(key, defaultValue);

            return savedData[key];
        }

        #endregion

        #region Input APIs

        /// <summary>
        ///     Returns the current state of a key. This accepts the Keys enum or an int for a specific key. In 
        ///     additon, you'll need to also provide the input state to check for. The InputState enum has Down 
        ///     and Released for options. By default, Down is automatically tested. Released returns true if the 
        ///     key was relased in the last frame.
        /// </summary>
        /// <param name="key">Accepts the Keys enum or int for the key's ID.</param>
        /// <param name="state">Optional InputState enum. Returns down state by default.</param>
        /// <returns>Returns a bool based on the state of the button.</returns>
        public bool Key(Keys key, InputState state = InputState.Down)
        {
            return state == InputState.Released
                ? controllerChip.GetKeyUp((int)key)
                : controllerChip.GetKeyDown((int)key);
        }

        /// <summary>
        ///     Returns the current state of the mouse's buttons (1 for left mouse and 2 for right mouse). In 
        ///     additon, you'll need to also provide the input state to check for. The InputState enum has Down 
        ///     and Released for options. By default, Down is automatically tested. Released returns true if the 
        ///     key was relased in the last frame.
        /// </summary>
        /// <param name="button">Accepts an int for the left (0) or right (1) mouse button.</param>
        /// <param name="state">Optional InputState enum. Returns down state by default.</param>
        /// <returns>Returns a bool based on the state of the button.</returns>
        public bool MouseButton(int button, InputState state = InputState.Down)
        {
            return state == InputState.Released
                ? controllerChip.GetMouseButtonUp(button)
                : controllerChip.GetMouseButtonDown(button);
        }

        /// <summary>
        ///     Returns the current state of a controller button for a given player. This accepts the Buttons 
        ///     enum or an int for a specific button. In additon, you'll need to also provide the input state 
        ///     to check for as well as the player ID 1 (0) and 2 (1). The InputState enum has Down and Released 
        ///     for options. By default, Down is automatically tested. Released returns true if the key was 
        ///     relased in the last frame.
        /// </summary>
        /// <param name="button">Accepts the Buttons enum or int for the button's ID.</param>
        /// <param name="state">Optional InputState enum. Returns down state by default.</param>
        /// <param name = "player">The player's controller 1 (0) and 2 (1).</param>
        /// <returns>Returns a bool based on the state of the button.</returns>
        public bool Button(Buttons button, InputState state = InputState.Down, int player = 0)
        {
            return state == InputState.Released
                ? controllerChip.ButtonReleased(button, player)
                : controllerChip.ButtonDown(button, player);
        }

        /// <summary>
        ///     The position of the mouse on the display.
        /// </summary>
        /// <returns>Returns a vector for the mouse's X and Y poisition.</returns>
        public Vector MousePosition()
        {
            return controllerChip.ReadMousePosition();
        }

        /// <summary>
        ///     Returns the keyboard input entered this frame.
        /// </summary>
        /// <returns>A string of all the characters entered during the frame.</returns>
        public string InputString()
        {
            return controllerChip.ReadInputString();
        }

        #endregion

        #region Math APIs

        /// <summary>
        ///     Limits a value between a minium and maximum.
        /// </summary>
        /// <param name="val">The value to clamp.</param>
        /// <param name="min">The minimum the value can be.</param>
        /// <param name="max">The maximum the value can be.</param>
        /// <returns></returns>
        public int Clamp(int val, int min, int max)
        {
            return val.Clamp(min, max);
        }

        /// <summary>
        ///     Repeates a value based on the max. When the value is greater than the max, it will 
        ///     become 0.
        /// </summary>
        /// <param name="val">The value to repeat.</param>
        /// <param name="max">The maximum the value can be.</param>
        /// <returns></returns>
        public int Repeat(int val, int max)
        {
            return MathUtil.Repeat(val, max);
        }

        /// <summary>
        ///     Converts an X and Y postion into an index. This is useful for finding positions in 1D
        ///     arrays that represent 2D data.
        /// </summary>
        /// <param name="x">The x position.</param>
        /// <param name="y">The y position.</param>
        /// <param name="width">The width of the data if it was represented as a 2D array.</param>
        /// <returns></returns>
        public int CalculateIndex(int x, int y, int width)
        {
            int index;
            PosUtil.CalculateIndex(x, y, width, out index);
            return index;
        }

        /// <summary>
        ///     Converts an index into an X and Y position to help when working with 1D arrays that represent 
        ///     2D data.
        /// </summary>
        /// <param name="index">The position of the 1D array.</param>
        /// <param name="width">The width of the data if it was a 2D array.</param>
        /// <returns></returns>
        public Vector CalculatePosition(int index, int width)
        {
            int x, y;

            PosUtil.CalculatePosition(index, width, out x, out y);

            return new Vector(x, y);
        }

        #endregion

        #region Sound APIs

        /// <summary>
        ///     This method plays back a sound on a specific channel. The SoundChip has a limit of active channels 
        ///     so playing a sound effect while another was is playing on the same channel will cancel it out and 
        ///     replace with the new sound.
        /// </summary>
        /// <param name="id">
        ///     The ID of the sound in the SoundCollection.
        /// </param>
        /// <param name="channel">
        ///     The channel the sound should play back on. Cannel 0 is set by default.
        /// </param>
        public void PlaySound(int id, int channel = 0)
        {
            soundChip.PlaySound(id, channel);
        }

        /// <summary>
        ///     This helper method allows you to autoamtically load a set of loops as a complete song and play 
        ///     them back. You can also define if the tracks should loop when they are done playing.
        /// </summary>
        /// <param name="trackIDs">An array of loop IDs to play back as a single song.</param>
        /// <param name="loop">Whether the song should loop back to the first ID when its done playing.</param>
        public void PlaySong(int[] trackIDs, bool loop = true)
        {
            var track = trackIDs[0];

            musicChip.LoadSong(track);

            musicChip.PlaySong(loop);
        }

        /// <summary>
        ///     Toggles the current playback state of the sequencer. If the song
        ///     is playing it will pause, if it is paused it will play
        /// </summary>
        public void PauseSong()
        {
            musicChip.PauseSong();
        }

        /// <summary>
        ///     Stops the sequencer.
        /// </summary>
        public void StopSong()
        {
            musicChip.StopSong();
        }

        /// <summary>
        ///     Rewinds the sequencer to the beginning of the currently loaded song. You can define the position 
        ///     in the loop and the loop where playback should begine. Calling this method without any arguments 
        ///     will simply rewind the song to the beginning of the first loop.
        /// </summary>
        /// <param name="position">Position in the loop to start playing at.</param>
        /// <param name="loopID">The loop to rewind too</param>
        public void RewindSong(int position = 0, int loopID = 0)
        {
            //TODO need to add in better support for rewinding a song across multiple loops
            musicChip.RewindSong();
        }

        #endregion

        #region Sprite APIs

        /// <summary>
        ///     Returns the size of the sprite as a Vector where X and Y represent the width and height.
        /// </summary>
        /// <param name="width">Optional argument to change the width of the sprite. Currently not enabled.</param>
        /// <param name="height">Optional argument to change the height of the sprite. Currently not enabled.</param>
        /// <returns></returns>
        public Vector SpriteSize(int? width = 8, int? height = 8)
        {
            var size = new Vector(spriteChip.width, spriteChip.height);

            // TODO you can't resize sprites at runtime

            return size;
        }

        /// <summary>
        ///     This allows you to return the pixel data of a sprite or overwrite it with new data. Sprite pixel data is
        ///     an array of color reference ids. When calling the method with only an id arugment, you'll get the sprite's 
        ///     pixel data. If you supply data, it will overwrite the sprite. It's importnat to make sure that any new 
        ///     pixel data should be the same length of the existing sprite's pixel data. This can be cauclated by 
        ///     multiplying the sprite's width and height. You can add transparent area to a sprite's data by using -1.
        /// </summary>
        /// <param name="id">The sprite to access.</param>
        /// <param name="data">Optional data to write over the sprite's current pixel data.</param>
        /// <returns>Returns an array of int data which points to color ids.</returns>
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

        /// <summary>
        ///     Returns the total number of sprites in the system. You can pass in an optional argument to get total number 
        ///     of sprites the Sprite Chip can store by passing in false for ignoreEmpty. By default, only sprites with 
        ///     pixel data will be included in the total returned.
        /// </summary>
        /// <param name="ignoreEmpty"></param>
        /// <returns></returns>
        public int TotalSprites(bool ignoreEmpty = true)
        {
            return spriteChip.spritesInRam;
        }

        #endregion

        #region Tilemap

        /// <summary>
        ///     This allows you to quickly access just the flag value of a tile. This is useful when trying to 
        ///     caluclate collision on the tilemap. By default, you can call this method and return the falg value. 
        ///     If you supply a new value, it will be overridden on the tile. Changing a tile's flag value does not 
        ///     force the tile to be redawn to the tilemap cache.
        /// </summary>
        /// <param name="column">The x position of the tile in the tilemap. The 0 position is on the far left of the tilemap.</param>
        /// <param name="row">The y position of the tile in the tilemap. The 0 position is on the top of the tilemap.</param>
        /// <param name="value">The new value for the flag. Setting the flag to -1 means no collision.</param>
        /// <returns></returns>
        public int Flag(int column, int row, int? value = null)
        {
            if (value.HasValue)
                tilemapChip.UpdateFlagAt(column, row, value.Value);

            return tilemapChip.ReadFlagAt(column, row);
        }

        /// <summary>
        ///     This allows you to get the current sprite id, color offset and flag values associated with a given tile. 
        ///     You can optionally supply your own if you want to change the tile's values. Changing a tile's sprite id 
        ///     or color offset will for the tilemap to redraw it to the cache on the next frame. If you are drawing 
        ///     raw pixel data into the tilemap cache in the same position it will be overwritten with the new tile's 
        ///     pixel data.
        /// </summary>
        /// <param name="column">The x position of the tile in the tilemap. The 0 position is on the far left of the tilemap.</param>
        /// <param name="row">The y position of the tile in the tilemap. The 0 position is on the top of the tilemap.</param>
        /// <param name="spriteID">The sprite id to use for the tile.</param>
        /// <param name="colorOffset">Shift the color IDs by this value</param>
        /// <param name="flag">An int value betwen -1 and 16 used for collision detection.</param>
        /// <returns></returns>
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
        ///     This forces the map to redraw its cached pixel data. Use this to clear any pixel data added
        ///     after the map created the pixel data cache.
        /// </summary>
        public void RebuildTilemap(int? columns = null, int? rows = null, int[] spriteIDs = null, int[] colorOffsets = null, int[] flags = null)
        {
            // TODO need to finish this method
            // If columns and rows are shown resize map
            // If sprites, colors or flags are used fill them in
            // If offset is present fill in new values at that offset


            tilemapChip.ClearCache();
        }

        /// <summary>
        ///     This will return a vector representing the size of the tilemap in columns (x) and rows (y). To find the 
        ///     size in pixels you'll need to multiply the returned vectors x and y values by the sprite size's x and y. 
        ///     This method alos allows you to resize the tilemap by passing in an optional new width and height. Resizeing 
        ///     the tile map is destructive so any changes will automatically clear the tilemap's sprite ids, color offsets 
        ///     and flag values. 
        /// </summary>
        /// <param name="width">An optional paramitor for the width in tiles of the map.</param>
        /// <param name="height">An option paramitor for the height in tiles of the map.</param>
        /// <returns>Returns a vector of the tile maps size in tiles where x and y are the columns and rows of the tilemap.</returns>
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

        /// <summary>
        ///     A helper method which allows you to update several tiles at once. Simply define the start column 
        ///     and row position, the width of the area to update in tiles and supply a new int array of sprite IDs. 
        ///     You can also modify the color offset and flag value of the tiles via the optional paramitors. This 
        ///     helper method uses calls the Tile() method to update each tile so any changes to a tile will be 
        ///     automatically redrawn to the tilemap's cache.
        /// </summary>
        /// <param name="column">Start column of the first tile to update. The 0 column is on the far left of the tilemap.</param>
        /// <param name="row">Start row of the first tile to update. The 0 row is on the top of the tilemap.</param>
        /// <param name="columns">The width of the area in tiles to update.</param>
        /// <param name="ids">An array of sprite IDs to use for each tile being updated.</param>
        /// <param name="colorOffset">An optional color offset int value to be applied to each updated tile.</param>
        /// <param name="flag">An optional flag int value to be applied to each updated tile.</param>
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
        ///     Configures the GameChip instance by loading it into
        ///     the engine's memory, getting a reference to the
        ///     APIBridge and setting the ready flag to
        ///     true.
        /// </summary>
        /// 
        public override void Configure()
        {
            engine.gameChip = this;

            //TODO this needs to be a service
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