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

#region

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PixelVision8.Engine.Utils;
using System;

#endregion

namespace PixelVision8.Engine.Chips
{
    public enum DrawMode
    {
        SpriteBelow,
        TilemapCache,
        Tile,
        Sprite,
        UI,
        SpriteAbove
    }

    public enum InputState
    {
        Down,
        Released
    }

    public enum Buttons
    {
        Up,
        Down,
        Left,
        Right,
        A,
        B,
        Select,
        Start
    }

    /// <summary>
    ///     The GameChip represents the foundation of a game class
    ///     with all the logic it needs to work correctly in the PixelVisionEngine.
    ///     The AbstractChip class manages configuring the game when created via the
    ///     chip life-cycle. The engine manages the game's state, the game's own life-cycle and
    ///     serialization/deserialization of the game's data.
    /// </summary>
    public class GameChipLite : AbstractChip, IUpdate, IDraw
    {
        public int fps;
        // protected int[] tmpFontData = new int[0];
        protected int[] tmpSpriteData = new int[0];
        protected Point display;
        protected int offsetX;
        protected int offsetY;
        protected int[] tmpIDs = new int[0];
        protected int total;
        protected int height;
        protected int id;
        protected bool render;
        protected Point spriteSize;
        protected int charWidth;
        protected int nextX;
        protected int nextY;
        protected int[] spriteIDs;
        protected int _width;
        // protected Rectangle viewPort;
        protected Point _scrollPos = Point.Zero;
        protected int _index;
        protected int _spriteId;
        protected char character;
        protected int charOffset = 32;
        protected Point _spriteSize = new Point(8, 8);
        private PixelData _tmpPixelData = new PixelData();
        // protected Rectangle _tmpBounds;
        protected Point _tilemapSize = Point.Zero;
        private int _paddingW;
        private int _paddingH;

        public int CurrentSprites { get; set; }

        #region GameChip Properties

        #endregion

        #region Chip References

        protected ColorChip ColorChip => engine.ColorChip;

        protected IControllerChip ControllerChip => engine.ControllerChip;

        protected DisplayChip DisplayChip => engine.DisplayChip;

        protected ISoundChip SoundChip => engine.SoundChip;

        protected SpriteChip SpriteChip => engine.SpriteChip;

        protected TilemapChip TilemapChip => engine.TilemapChip;

        protected FontChip FontChip => engine.FontChip;

        #endregion

        #region Lifecycle

        public override void Configure()
        {

            // Set the engine's game to this instance
            engine.GameChip = this;

            display.X = DisplayChip.Width;
            display.Y = DisplayChip.Height;

            // metaSprites = new SpriteCollection[96];
        }

        /// <summary>
        ///     Update() is called once per frame at the beginning of the game loop. This is where you should put all
        ///     non-visual game logic such as character position calculations, detecting input and performing updates to
        ///     your animation system. The time delta is provided on each frame so you can calculate the difference in
        ///     milliseconds since the last render took place.
        /// </summary>
        /// <label>
        ///     Runner
        /// </label>
        /// <param name="timeDelta">A float value representing the time in milliseconds since the last Draw() call was completed.</param>
        public virtual void Update(int timeDelta)
        {
            // Overwrite this method and add your own draw logic.
        }

        /// <summary>
        ///     Draw() is called once per frame after the Update() has completed. This is where all visual updates to
        ///     your game should take place such as clearing the display, drawing sprites, and pushing raw pixel data
        ///     into the display.
        /// </summary>
        /// <label>
        ///     Runner
        /// </label>
        public virtual void Draw()
        {
            // Overwrite this method and add your own draw logic.
        }

        /// <summary>
        ///     Reset() is called when a game is restarted. This is usually called instead of reloading the entire game.
        ///     It allows you to perform additional configuration that would not be able to happen if the Init() method
        ///     is not called. This is mostly ignored in the Runner and is mainly used in the Game Creator.
        /// </summary>
        /// <label>
        ///     Runner
        /// </label>
        public override void Reset()
        {
            CurrentSprites = 0;

            _scrollPos.X = 0;
            _scrollPos.Y = 0;

            _spriteSize.X = SpriteChip.width;
            _spriteSize.Y = SpriteChip.height;

            // Resize the tmpSpriteData so it mateches the sprite's width and height
            Array.Resize(ref tmpSpriteData, SpriteChip.width * SpriteChip.height);
            // Array.Resize(ref tmpFontData, SpriteChip.width * SpriteChip.height);

            base.Reset();
        }

        public override void Deactivate()
        {
            base.Deactivate();
            engine.GameChip = null;
        }

        /// <summary>
        ///     Shutdown() is called when quitting a game or shutting down the Runner/Game Creator. This hook allows you
        ///     to perform any last minute changes to the game's data such as saving or removing any temp files that
        ///     will not be needed.
        /// </summary>
        public override void Shutdown()
        {
            // Put save logic here
        }

        #endregion

        #region Color

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
        public virtual int BackgroundColor(int? id = null)
        {
            if (id.HasValue) ColorChip.backgroundColor = id.Value;

            return ColorChip.backgroundColor;
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
            if (value == null) return ColorChip.ReadColorAt(id);

            ColorChip.UpdateColorAt(id, value);

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
        public int TotalColors(bool ignoreEmpty = false)
        {
            return ignoreEmpty ? ColorChip.totalUsedColors : ColorChip.total;
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
            // This can not be changed at run time so it will never need to be invalidated
            return SpriteChip.colorsPerSprite; //colorsPerSpriteCached;//;
        }

        /// <summary>
        ///     The ReplaceColor() method allows you to quickly change a color to an existing color without triggering
        ///     the DisplayChip to parse and cache a new hex value. Consider this an alternative to the Color() method.
        ///     It is useful for simulating palette swapping animation on sprites pointed to a fixed group of color IDs.
        ///     Simply call the ReplaceColor() method and supply a target color ID position, then the new color ID it
        ///     should point to. Since you are only changing the color's ID pointer, there is little to no performance
        ///     penalty during the GameChip's draw phase.
        /// </summary>
        /// <param name="index">The ID of the color you want to change.</param>
        /// <param name="id">The ID of the color you want to replace it with.</param>
        public void ReplaceColor(int index, int id)
        {
            ColorChip.UpdateColorAt(index, ColorChip.ReadColorAt(id));
        }

        #endregion

        #region Display

        /// <summary>
        ///     Clearing the display removed all of the existing pixel data, replacing it with the default background
        ///     color. The Clear() method allows you specify what region of the display to clear. By simply calling
        ///     Clear(), with no arguments, it automatically clears the entire display. You can manually define an area
        ///     of the screen to clear by supplying option x, y, width and height arguments. When clearing a specific
        ///     area of the display, anything outside of the defined boundaries remains on the next draw phase. This is
        ///     useful for drawing a HUD but clearing the display below for a scrolling map and sprites. Clear can only
        ///     be used once during the draw phase.
        /// </summary>
        public void Clear()
        {
            // DisplayChip.Clear();
            //
            // w = width ?? display.X - x;
            // h = height ?? display.Y - y;
            //
            // DrawRect(x, y, w, h, ColorChip.backgroundColor);
            DisplayChip.Clear(ColorChip.backgroundColor);

        }

        /// <summary>
        ///     The display's size defines the visible area where pixel data exists on the screen. Calculating this is
        ///     important for knowing how to position sprites on the screen. The Display() method allows you to get
        ///     the resolution of the display at run time. By default, this will return the visble screen area based on
        ///     the overscan value set on the display chip. To calculate the exact overscan in pixels, you must subtract
        ///     the full size from the visible size. Simply supply false as an argument to get the full display dimensions.
        /// </summary>
        public Point Display()
        {
            // offsetX = visible ? DisplayChip.OverscanXPixels : 0;
            // offsetY = visible ? DisplayChip.OverscanYPixels : 0;

            display.X = DisplayChip.Width;// - offsetX;
            display.Y = DisplayChip.Height;// - offsetY;

            return display;
        }


        /// <summary>
        ///     This method allows you to draw raw pixel data directly to the display. Depending on which draw mode you
        ///     use, the pixel data could be rendered as a sprite or drawn directly onto the tilemap cache. Sprites drawn
        ///     with this method still count against the total number the display can render but you can draw irregularly
        ///     shaped sprites by defining a custom width and height. For drawnig into the tilemap cache directly, you can
        ///     use this to change the way the tilemap looks at run-time without having to modify a sprite's pixel data.
        ///     It is important to note that when you change a tile's sprite ID or color offset, the tilemap redraws it
        ///     back to the cache overwriting any pixel data that was previously there.
        /// </summary>
        /// <param name="pixelData">
        ///     The pixelData argument accepts an int array representing references to color IDs. The pixelData array length
        ///     needs to be the same size as the supplied width and height, or it will throw an error.
        /// </param>
        /// <param name="x">
        ///     The x position where to display the new pixel data. The display's horizontal 0 position is on the far left-hand
        ///     side.
        ///     When using DrawMode.TilemapCache, the pixel data is drawn into the tilemap's cache instead of directly on
        ///     the display when using DrawMode.Sprite.
        /// </param>
        /// <param name="y">
        ///     The Y position where to display the new pixel data. The display's vertical 0 position is on the top. When using
        ///     DrawMode.TilemapCache, the pixel data is drawn into the tilemap's cache instead of directly on the display
        ///     when using DrawMode.Sprite.
        /// </param>
        /// <param name="blockWidth">
        ///     The width of the pixel data to use when rendering to the display.
        /// </param>
        /// <param name="blockHeight">
        ///     The height of the pixel data to use when rendering to the display.
        /// </param>
        /// <param name="drawMode">
        ///     This argument accepts the DrawMode enum. You can use Sprite, SpriteBelow, and TilemapCache to change where the
        ///     pixel data is drawn to. By default, this value is DrawMode.Sprite.
        /// </param>
        /// <param name="flipH">
        ///     This is an optional argument which accepts a bool. The default value is set to false but passing in true flips
        ///     the pixel data horizontally.
        /// </param>
        /// <param name="flipV">
        ///     This is an optional argument which accepts a bool. The default value is set to false but passing in true flips
        ///     the pixel data vertically.
        /// </param>
        /// <param name="colorOffset">
        ///     This optional argument accepts an int that offsets all the color IDs in the pixel data array. This value is added
        ///     to each int, in the pixel data array, allowing you to simulate palette shifting.
        /// </param>
        public void DrawPixels(int[] pixelData, int x, int y, int blockWidth, int blockHeight, bool flipH = false,
            bool flipV = false, DrawMode drawMode = DrawMode.Sprite, int colorOffset = 0)
        {
            switch (drawMode)
            {
                case DrawMode.Tile:
                    // Do nothing since we can't set raw pixel data to a tile
                    break;

                case DrawMode.TilemapCache:

                    if (_tmpPixelData.Width != blockWidth || _tmpPixelData.Height != blockHeight)
                        _tmpPixelData.Resize(blockWidth, blockHeight);

                    _tmpPixelData.Pixels = pixelData;

                    // Copy pixel data directly into the tilemap chip's texture
                    PixelDataUtil.MergePixels(_tmpPixelData, 0, 0, blockWidth, blockHeight, TilemapChip.PixelData, x, y, flipH, flipV, colorOffset);

                    break;

                default:

                    DisplayChip.NewDrawCall(pixelData, x, y, blockWidth, blockHeight, (byte)drawMode, flipH, flipV, colorOffset);

                    break;
            }
        }

        /// <summary>
        ///     Sprites represent individual collections of pixel data at a fixed size. By default, Pixel Vision 8 sprites are
        ///     8 x 8 pixels and have a set limit of visible colors. You can use the DrawSprite() method to render any sprite
        ///     stored in the Sprite Chip. The display also has a limitation on how many sprites can be on the screen at one time.
        ///     Each time you call DrawSprite(), the sprite counts against the total amount the display can render. If you attempt
        ///     to
        ///     draw more sprites than the display can handle, the call is ignored. One thing to keep in mind when drawing sprites
        ///     is that their x and y position wraps if they reach the right or bottom border of the screen. You need to change
        ///     the overscan border to hide sprites offscreen.
        /// </summary>
        /// <param name="id">
        ///     The unique ID of the sprite to use in the SpriteChip.
        /// </param>
        /// <param name="x">
        ///     An int value representing the X position to place sprite on the display. If set to 0, it renders on the far
        ///     left-hand side of the screen.
        /// </param>
        /// <param name="y">
        ///     An int value representing the Y position to place sprite on the display. If set to 0, it renders on the top
        ///     of the screen.
        /// </param>
        /// <param name="flipH">
        ///     This is an optional argument which accepts a bool. The default value is set to false but passing in true flips
        ///     the pixel data horizontally.
        /// </param>
        /// <param name="flipV">
        ///     This is an optional argument which accepts a bool. The default value is set to false but passing in true flips
        ///     the pixel data vertically.
        /// </param>
        /// <param name="drawMode"></param>
        /// <param name="colorOffset">
        ///     This optional argument accepts an int that offsets all the color IDs in the pixel data array. This value is added
        ///     to each int, in the pixel data array, allowing you to simulate palette shifting.
        /// </param>
        /// <param name="srcChip"></param>
        /// <param name="aboveBG">
        ///     An optional bool that defines if the sprite is above or below the tilemap. Sprites are set to render above the
        ///     tilemap by default. When rendering below the tilemap, the sprite is visible in the transparent area of the tile
        ///     above the background color.
        /// </param>
        public virtual void DrawSprite(int id, int x, int y, bool flipH = false, bool flipV = false,
            DrawMode drawMode = DrawMode.Sprite, int colorOffset = 0, SpriteChip srcChip = null)
        {
            // Only apply the max sprite count to sprite draw modes

            srcChip ??= SpriteChip;

            if (drawMode == DrawMode.Tile)
            {
                Tile(x, y, id, colorOffset);
            }
            else
            {
                // x -=(useScrollPos ? _scrollPos.X : 0);
                // y -=(useScrollPos ? _scrollPos.Y : 0);

                if (drawMode == DrawMode.TilemapCache)
                {

                    srcChip.ReadSpriteAt(id, ref tmpSpriteData);

                    DrawPixels(tmpSpriteData, x, y, SpriteChip.width, SpriteChip.height, flipH, flipV, drawMode, colorOffset);

                }
                else
                {
                    if (SpriteChip.maxSpriteCount > 0 && CurrentSprites >= SpriteChip.maxSpriteCount) return;

                    // Check to see if we need to test the bounds
                    // if (onScreen)
                    // {
                    //     // _tmpBounds = bounds ?? DisplayChip.VisibleBounds;
                    //
                    //     // This can set the render flag to true or false based on it's location
                    //     //TODO need to take into account the current bounds of the screen
                    //     render = x >= 0 && x <= display.X && y >= 0 && y <= display.Y;
                    // }
                    // else
                    // {   
                    //     // If we are not testing to see if the sprite is onscreen it will always render and wrap based on its position
                    //     render = true;
                    // }
                    //
                    // // If the sprite should be rendered, call DrawSprite()
                    // if (render)
                    // {

                    var pos = MathUtil.CalculatePosition(id, srcChip.Columns);
                    pos.X *= SpriteChip.width;
                    pos.Y *= SpriteChip.height;

                    DisplayChip.NewDrawCall(srcChip, x, y, SpriteChip.width, SpriteChip.height, (byte)drawMode, flipH, flipV, colorOffset, pos.X, pos.Y);

                    CurrentSprites++;
                    // }
                }

            }
        }

        /// <summary>
        ///     The DrawSprites method makes it easier to combine and draw groups of sprites to the display in a grid. This is
        ///     useful when trying to render 4 sprites together as a larger 16x16 pixel graphic. While there is no limit on the
        ///     size of the sprite group which can be rendered, it is important to note that each sprite in the array still counts
        ///     as an individual sprite. Sprites passed into the DrawSprites() method are visible if the display can render it.
        ///     Under the hood, this method uses DrawSprite but solely manages positioning the sprites out in a grid. Another
        ///     unique feature of his helper method is that it automatically hides sprites that go offscreen. When used with
        ///     overscan border, it greatly simplifies drawing larger sprites to the display.
        /// </summary>
        /// <param name="ids">
        ///     An array of sprite IDs to display on the screen.
        /// </param>
        /// <param name="x">
        ///     An int value representing the X position to place sprite on the display. If set to 0, it renders on the far
        ///     left-hand side of the screen.
        /// </param>
        /// <param name="y">
        ///     An int value representing the Y position to place sprite on the display. If set to 0, it renders on the top
        ///     of the screen.
        /// </param>
        /// <param name="width">
        ///     The width, in sprites, of the grid. A value of 2 renders 2 sprites wide. The DrawSprites method continues to
        ///     run through all of the sprites in the ID array until reaching the end. Sprite groups do not have to be perfect
        ///     squares since the width value is only used to wrap sprites to the next row.
        /// </param>
        /// <param name="flipH">
        ///     This is an optional argument which accepts a bool. The default value is set to false but passing in true flips
        ///     the pixel data horizontally.
        /// </param>
        /// <param name="flipV">
        ///     This is an optional argument which accepts a bool. The default value is set to false but passing in true flips
        ///     the pixel data vertically.
        /// </param>
        /// <param name="drawMode"></param>
        /// <param name="colorOffset">
        ///     This optional argument accepts an int that offsets all the color IDs in the pixel data array. This value is added
        ///     to each int, in the pixel data array, allowing you to simulate palette shifting.
        /// </param>
        public void DrawSprites(int[] ids, int x, int y, int width, bool flipH = false, bool flipV = false,
            DrawMode drawMode = DrawMode.Sprite, int colorOffset = 0)
        {
            total = ids.Length;

            // TODO added this so C# code isn't corrupted, need to check performance impact
            if (tmpIDs.Length != total) Array.Resize(ref tmpIDs, total);

            Array.Copy(ids, tmpIDs, total);

            height = MathUtil.CeilToInt(total / width);

            // TODO this needs to be moved into the Draw Sprite
            // startX = x;// - (useScrollPos ? _scrollPos.X : 0);
            // startY = y;// - (useScrollPos ? _scrollPos.Y : 0);

            _paddingW = drawMode == DrawMode.Tile ? 1 : _spriteSize.X;
            _paddingH = drawMode == DrawMode.Tile ? 1 : _spriteSize.Y;

            if (flipH || flipV) SpriteChipUtil.FlipSpriteData(ref tmpIDs, width, height, flipH, flipV);

            // Store the sprite id from the ids array
            //            int id;
            //            bool render;

            // TODO need to offset the bounds based on the scroll position before testing against it

            for (var i = 0; i < total; i++)
            {
                // Set the sprite id
                id = tmpIDs[i];

                // TODO should also test that the sprite is not greater than the total sprites (from a cached value)
                // Test to see if the sprite is within range
                if (id > -1)
                {
                    // startX = 
                    // startY = MathUtil.FloorToInt(i / width) * _paddingH + y;
                    //
                    //                    var render = true;


                    DrawSprite(
                        id,
                        MathUtil.FloorToInt(i % width) * _paddingW + x,
                        MathUtil.FloorToInt(i / width) * _paddingH + y,
                        flipH,
                        flipV,
                        drawMode,
                        colorOffset);
                }
            }
        }

        /// <summary>
        ///     DrawSpriteBlock() is similar to DrawSprites except you define the first sprite (upper left corner) and the width x
        ///     height
        ///     (in sprites) to sample from sprite ram. This will create a larger sprite by using neighbor sprites.
        /// </summary>
        /// <param name="id">The top left sprite to start with. </param>
        /// <param name="x">
        ///     An int value representing the X position to place sprite on the display. If set to 0, it renders on the far
        ///     left-hand side of the screen.
        /// </param>
        /// <param name="y">
        ///     An int value representing the Y position to place sprite on the display. If set to 0, it renders on the top
        ///     of the screen.
        /// </param>
        /// <param name="columns"></param>
        /// <param name="rows"></param>
        /// <param name="flipH">
        ///     This is an optional argument which accepts a bool. The default value is set to false but passing in true flips
        ///     the pixel data horizontally.
        /// </param>
        /// <param name="flipV">
        ///     This is an optional argument which accepts a bool. The default value is set to false but passing in true flips
        ///     the pixel data vertically.
        /// </param>
        /// <param name="drawMode"></param>
        /// <param name="colorOffset">
        ///     This optional argument accepts an int that offsets all the color IDs in the pixel data array. This value is added
        ///     to each int, in the pixel data array, allowing you to simulate palette shifting.
        /// </param>
        public void DrawSpriteBlock(int id, int x, int y, int columns = 1, int rows = 1, bool flipH = false,
            bool flipV = false, DrawMode drawMode = DrawMode.Sprite, int colorOffset = 0)
        {
            total = columns * rows;

            var sprites = new int[total];

            var sW = MathUtil.CeilToInt((float)SpriteChip.textureWidth / SpriteChip.width);

            var startC = id % sW;
            var tmpC = id % sW;
            var tmpR = (int)Math.Floor(id / (double)sW);

            var tmpCols = tmpC + columns;

            for (var i = 0; i < total; i++)
            {
                sprites[i] = tmpC + tmpR * sW;

                tmpC += 1;

                if (tmpC >= tmpCols)
                {
                    tmpC = startC;
                    tmpR += 1;
                }
            }

            DrawSprites(sprites, x, y, columns, flipH, flipV, drawMode, colorOffset);
        }

        /// <summary>
        ///     The DrawText() method allows you to render text to the display. By supplying a custom DrawMode, you can render
        ///     characters as individual sprites (DrawMode.Sprite), tiles (DrawMode.Tile) or drawn directly into the tilemap
        ///     cache (DrawMode.TilemapCache). When drawing text as sprites, you have more flexibility over position, but each
        ///     character counts against the displays' maximum sprite count. When rendering text to the tilemap, more characters
        ///     are shown and also increase performance when rendering large amounts of text. You can also define the color offset,
        ///     letter spacing which only works for sprite and tilemap cache rendering, and a width in characters if you want the
        ///     text to wrap.
        /// </summary>
        /// <param name="text">
        ///     A text string to display on the screen.
        /// </param>
        /// <param name="x">
        ///     An int value representing the X position to start the text on the display. If set to 0, it renders on the far
        ///     left-hand side of the screen.
        /// </param>
        /// <param name="y">
        ///     An int value representing the Y position to place sprite on the display. If set to 0, it renders on the top
        ///     of the screen.
        /// </param>
        /// <param name="drawMode">
        ///     This argument accepts the DrawMode enum. You can use Sprite, SpriteBelow, and TilemapCache to change where the
        ///     pixel data is drawn to. By default, this value is DrawMode.Sprite.
        /// </param>
        /// <param name="font">
        ///     The name of the font to use. You do not need to add the font's file extension. If the file is called
        ///     The name of the font to use. You do not need to add the font's file extension. If the file is called
        ///     default.font.png,
        ///     you can simply refer to it as "default" when supplying an argument value.
        /// </param>
        /// <param name="colorOffset">
        ///     This optional argument accepts an int that offsets all the color IDs in the pixel data array. This value is added
        ///     to each color ID in the font's pixel data, allowing you to simulate palette shifting.
        /// </param>
        /// <param name="spacing">
        ///     This optional argument sets the number of pixels between each character when rendering text. This value is ignored
        ///     when rendering text as tiles. This value can be positive or negative depending on your needs. By default, it is 0.
        /// </param>
        /// <returns></returns>
        public void DrawText(string text, int x, int y, DrawMode drawMode = DrawMode.Sprite, string font = "Default",
            int colorOffset = 0, int spacing = 0)
        {
            // TODO this should use DrawSprites() API
            spriteSize = SpriteSize();
            charWidth = spriteSize.X;

            nextX = x;
            nextY = y;

            spriteIDs = ConvertTextToSprites(text, font);
            total = spriteIDs.Length;

            var clearTiles = false;

            if (drawMode == DrawMode.Tile)
            {
                // Set the clear tile flag
                clearTiles = true;

                // Change to Tilemap Cache since we are not actually drawing the tiles
                drawMode = DrawMode.TilemapCache;

                // Need to adjust the position since tile mode is in columns,rows
                nextX *= 8;
                nextY *= 8;

                // spacing is disabled when in tilemode
                spacing = 0;
            }

            var offset = charWidth + spacing;

            for (var j = 0; j < total; j++)
            {
                
                // Clear the background when in tile mode
                if (clearTiles) Tile(nextX / 8, nextY / 8, -1);

                // Manually increase the sprite counter if drawing to one of the sprite layers
                if (drawMode == DrawMode.Sprite || drawMode == DrawMode.SpriteAbove ||
                    drawMode == DrawMode.SpriteBelow)
                {
                    // If the sprite counter has been met, exit out of the draw call
                    if (SpriteChip.maxSpriteCount > 0 && CurrentSprites >= SpriteChip.maxSpriteCount) return;

                    CurrentSprites++;
                }

                DrawSprite(spriteIDs[j], nextX, nextY, false, false, drawMode, colorOffset, FontChip);

                // }

                nextX += offset;
            }
        }

        /// <summary>
        ///     By default, the tilemap renders to the display by simply calling DrawTilemap(). This automatically fills the entire
        ///     display with the visible portion of the tilemap. To have more granular control over how to render the tilemap, you
        ///     can supply an optional X and Y position to change where it draws on the screen. You can also modify the width
        ///     (columns) and height (rows) that are displayed too. This is useful if you want to show a HUD or some other kind of
        ///     image on the screen that is not overridden by the tilemap. To scroll the tilemap, you need to call the
        ///     ScrollPosition() and supply a new scroll X and Y value.
        /// </summary>
        /// <param name="x">
        ///     An optional int value representing the X position to render the tilemap on the display. If set to 0, it
        ///     renders on the far left-hand side of the screen.
        /// </param>
        /// <param name="y">
        ///     An optional int value representing the Y position to render the tilemap on the display. If set to 0, it
        ///     renders on the top of the screen.
        /// </param>
        /// <param name="columns">
        ///     An optional int value representing how many horizontal tiles to include when drawing the map. By default, this is
        ///     0 which automatically uses the full visible width of the display, while taking into account the X position offset.
        /// </param>
        /// <param name="rows">
        ///     An optional int value representing how many vertical tiles to include when drawing the map. By default, this is 0
        ///     which automatically uses the full visible height of the display, while taking into account the Y position offset.
        /// </param>
        /// <param name="offsetX">
        ///     An optional int value to override the scroll X position. This is useful when you need to change the left x position
        ///     from where to sample the tilemap data from.
        /// </param>
        /// <param name="offsetY">
        ///     An optional int value to override the scroll Y position. This is useful when you need to change the top y position
        ///     from where to sample the tilemap data from.
        /// </param>
        public void DrawTilemap(int x = 0, int y = 0, int columns = 0, int rows = 0, int? offsetX = null,
            int? offsetY = null)
        {

            DisplayChip.NewDrawCall(
                TilemapChip,
                x,
                y,
                columns == 0 ? display.X : columns * SpriteChip.width,
                rows == 0 ? display.Y : rows * SpriteChip.height,
                (byte)DrawMode.Tile,
                false,
                false,
                0,
                offsetX ?? _scrollPos.X,
                offsetY ?? _scrollPos.Y
            );


        }

        /// <summary>
        ///     This method allows you to draw a rectangle with a fill color. By default, this method is used to clear the screen
        ///     but you can supply a color offset to change the color value and use it to fill a rectangle area with a specific
        ///     color instead.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="color"></param>
        /// <param name="drawMode"></param>
        public void DrawRect(int x, int y, int width, int height, int color = -1,
            DrawMode drawMode = DrawMode.TilemapCache)
        {
            var pixels = new int[width * height];
            // TODO is there a faster way to do this?
            DrawPixels(pixels, x, y, width, height, false, false, drawMode, color);
        }

        /// <summary>
        ///     You can use RedrawDisplay to make clearing and drawing the tilemap easier. This is a helper method automatically
        ///     calls both Clear() and DrawTilemap() for you.
        /// </summary>
        public void RedrawDisplay()
        {
            Clear();
            DrawTilemap();
        }

        /// <summary>
        ///     You can scroll the tilemap by calling the ScrollPosition() method and supplying a new scroll X and Y position.
        ///     By default, calling ScrollPosition() with no arguments returns a vector with the current scroll X and Y values.
        ///     If you supply an X and Y value, it updates the tilemap's scroll position the next time you call the
        ///     DrawTilemap() method.
        /// </summary>
        /// <param name="x">
        ///     An optional int value representing the scroll X position of the tilemap. If set to 0, it starts on the far
        ///     left-hand side of the tilemap.
        /// </param>
        /// <param name="y">
        ///     An optional int value representing the scroll Y position of the tilemap. If set to 0, it starts on the top of
        ///     the tilemap.
        /// </param>
        /// <returns>
        ///     By default, this method returns a vector with the current scroll X and Y position.
        /// </returns>
        public Point ScrollPosition(int? x = null, int? y = null)
        {

            if (x.HasValue)
            {
                _scrollPos.X = x.Value;
            }

            if (y.HasValue)
            {
                _scrollPos.Y = y.Value;
            }

            return _scrollPos;
        }

        #endregion

        #region Pixel Data

        /// <summary>
        ///     A helper method to convert a string of characters into an array of sprite IDs.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="fontName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public int[] ConvertTextToSprites(string text, string fontName = "default")
        {
            total = text.Length;

            spriteIDs = new int[total];

            //            char character;

            //            int spriteID, index;

            var fontMap = FontChip.ReadFont(fontName);

            // Test to make sure font exists
            if (fontMap == null) throw new Exception("Font '" + fontName + "' not found.");

            var totalCharacters = fontMap.Length;

            for (var i = 0; i < total; i++)
            {
                character = text[i];
                _index = Convert.ToInt32(character) - charOffset;
                _spriteId = -1;

                if (_index < totalCharacters && _index > -1) _spriteId = fontMap[_index];

                spriteIDs[i] = _spriteId;
            }

            return spriteIDs;
        }

        #endregion

        #region Input

        /// <summary>
        ///     While the main form of input in Pixel Vision 8 comes from the controllers, you can test for keyboard
        ///     input by calling the Key() method. When called, this method returns the current state of a key. The
        ///     method accepts the Keys enum, or an int, for a specific key. In additon, you need to provide the input
        ///     state to check for. The InputState enum has two states, Down and Released. By default, Down is
        ///     automatically used which returns true when the key is being pressed in the current frame. When using
        ///     Released, the method returns true if the key is currently up but was down in the last frame.
        /// </summary>
        /// <param name="key">
        ///     This argument accepts the Keys enum or an int for the key's ID.
        /// </param>
        /// <param name="state">
        ///     Optional InputState enum. Returns down state by default. This argument accepts InputState.Down (0)
        ///     or InputState.Released (1).
        /// </param>
        /// <returns>
        ///     This method returns a bool based on the state of the button.
        /// </returns>
        public bool Key(Keys key, InputState state = InputState.Down)
        {
            return state == InputState.Released
                ? ControllerChip.GetKeyUp(key)
                : ControllerChip.GetKeyDown(key);
        }

        /// <summary>
        ///     Pixel Vision 8 supports mouse input. You can get the current state of the mouse's left (0) and
        ///     right (1) buttons by calling MouseButton(). In addition to supplying a button ID, you also need
        ///     to provide the InputState enum. The InputState enum contains options for testing the Down and
        ///     Released states of the supplied button ID. By default, Down is automatically used which returns
        ///     true when the key was pressed in the current frame. When using Released, the method returns true
        ///     if the key is currently up but was down in the last frame.
        /// </summary>
        /// <param name="button">
        ///     Accepts an int for the left (0) or right (1) mouse button.
        /// </param>
        /// <param name="state">
        ///     An optional InputState enum. Uses InputState.Down default.
        /// </param>
        /// <returns>
        ///     Returns a bool based on the state of the button.
        /// </returns>
        public bool MouseButton(int button, InputState state = InputState.Down)
        {
            return state == InputState.Released
                ? ControllerChip.GetMouseButtonUp(button)
                : ControllerChip.GetMouseButtonDown(button);
        }

        /// <summary>
        ///     The MouseWheel() method returns an int for how far the scroll wheel has moved since the last frame.
        ///     This value is read-only. Generally speaking, one "tick" of the scroll wheel is 120.
        ///     The returned value is positive for up, and negative for down.
        /// </summary>
        /// <returns>
        ///     Returns a Point representing a vector with how far the scroll wheel has moved since the last frame.
        /// </returns>
        /// 
        public Point MouseWheel()
        {
            return ControllerChip.ReadMouseWheel();
        }

        /// <summary>
        ///     The main form of input for Pixel Vision 8 is the controller's buttons. You can get the current
        ///     state of any button by calling the Button() method and supplying a button ID, an InputState enum,
        ///     and the controller ID. When called, the Button() method returns a bool for the requested button
        ///     and its state. The InputState enum contains options for testing the Down and Released states of
        ///     the supplied button ID. By default, Down is automatically used which returns true when the key
        ///     was pressed in the current frame. When using Released, the method returns true if the key is
        ///     currently up but was down in the last frame.
        /// </summary>
        /// <param name="button">
        ///     Accepts the Buttons enum or int for the button's ID.
        /// </param>
        /// <param name="state">
        ///     Optional InputState enum. Returns down state by default.
        /// </param>
        /// <param name="controllerID">
        ///     An optional InputState enum. Uses InputState.Down default.
        /// </param>
        /// <returns>
        ///     Returns a bool based on the state of the button.
        /// </returns>
        public bool Button(Buttons button, InputState state = InputState.Down, int controllerID = 0)
        {
            return state == InputState.Released
                ? ControllerChip.ButtonReleased(button, controllerID)
                : ControllerChip.ButtonDown(button, controllerID);
        }

        /// <summary>
        ///     The MousePosition() method returns a vector for the current cursor's X and Y position.
        ///     This value is read-only. The mouse's 0,0 position is in the upper left-hand corner of the
        ///     display
        /// </summary>
        /// <returns>
        ///     Returns a vector for the mouse's X and Y poisition.
        /// </returns>
        public Point MousePosition()
        {
            var pos = ControllerChip.ReadMousePosition();

            // var bounds = DisplayChip.VisibleBounds;

            // Make sure that the mouse x position is inside of the display width
            if (pos.X < 0 || pos.X > display.X) pos.X = -1;

            // Make sure that the mouse y position is inside of the display height
            if (pos.Y < 0 || pos.Y > display.Y) pos.Y = -1;

            return pos;
        }

        /// <summary>
        ///     The InputString() method returns the keyboard input entered this frame. This method is
        ///     useful for capturing keyboard text input.
        /// </summary>
        /// <returns>
        ///     A string of all the characters entered during the frame.
        /// </returns>
        public string InputString()
        {
            return ControllerChip.ReadInputString();
        }

        #endregion

        #region Sound

        /// <summary>
        ///     This method plays back a sound on a specific channel. The SoundChip has a limit of
        ///     active channels so playing a sound effect while another was is playing on the same
        ///     channel will cancel it out and replace with the new sound.
        /// </summary>
        /// <param name="id">
        ///     The ID of the sound in the SoundCollection.
        /// </param>
        /// <param name="channel">
        ///     The channel the sound should play back on. Channel 0 is set by default.
        /// </param>
        public void PlaySound(int id, int channel = 0)
        {
            SoundChip.PlaySound(id, channel);
        }

        public void PlaySample(string name, int channel = 0)
        {
            SoundChip.PlaySound(name, channel);
        }

        /// <summary>
        ///     Use StopSound() to stop any sound playing on a specific channel.
        /// </summary>
        /// <param name="channel">The channel ID to stop a sound on.</param>
        public void StopSound(int channel = 0)
        {
            SoundChip.StopSound(channel);
        }

        /// <summary>
        ///     Returns a bool if the channel is playing a sound.
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public bool IsChannelPlaying(int channel)
        {
            return SoundChip.IsChannelPlaying(channel);
        }

        #endregion

        #region Sprite

        /// <summary>
        ///     Returns the size of the sprite as a Vector where X and Y represent the width and height.
        /// </summary>
        /// <param name="width">
        ///     Optional argument to change the width of the sprite. Currently not enabled.
        /// </param>
        /// <param name="height">
        ///     Optional argument to change the height of the sprite. Currently not enabled.
        /// </param>
        /// <returns>
        ///     Returns a vector where the X and Y for the sprite's width and height.
        /// </returns>
        public Point SpriteSize()
        {
            // var size = new Point(spriteChip.width, spriteChip.height);

            return _spriteSize;
        }

        /// <summary>
        ///     This allows you to return the pixel data of a sprite or overwrite it with new data. Sprite
        ///     pixel data is an array of color reference ids. When calling the method with only an id
        ///     argument, you will get the sprite's pixel data. If you supply data, it will overwrite the
        ///     sprite. It is important to make sure that any new pixel data should be the same length of
        ///     the existing sprite's pixel data. This can be calculated by multiplying the sprite's width
        ///     and height. You can add the transparent area to a sprite's data by using -1.
        /// </summary>
        /// <param name="id">
        ///     The sprite to access.
        /// </param>
        /// <param name="data">
        ///     Optional data to write over the sprite's current pixel data.
        /// </param>
        /// <returns>
        ///     Returns an array of int data which points to color ids.
        /// </returns>
        public int[] Sprite(int id, int[] data = null)
        {

            if (data != null)
            {
                SpriteChip.UpdateSpriteAt(id, data);

                TilemapChip.InvalidateTileID(id);

                return data;
            }

            SpriteChip.ReadSpriteAt(id, ref tmpSpriteData);

            return tmpSpriteData;
        }

        public int[] FontChar(char character, string fontName, int[] data = null)
        {

            var fontMap = FontChip.ReadFont(fontName);

            // Test to make sure font exists
            if (fontMap == null) throw new Exception("Font '" + fontName + "' not found.");

            // TODO need to test this out
            id = ConvertTextToSprites(character.ToString(), fontName)[0];

            if (data != null)
            {
                FontChip.UpdateSpriteAt(id, data);

                return data;
            }

            FontChip.ReadSpriteAt(id, ref tmpSpriteData);

            return tmpSpriteData;
        }

        #endregion

        #region Tilemap

        /// <summary>
        ///     This allows you to quickly access just the flag value of a tile. This is useful when trying
        ///     to the caluclate collision on the tilemap. By default, you can call this method and return
        ///     the flag value. If you supply a new value, it will be overridden on the tile. Changing a
        ///     tile's flag value does not force the tile to be redrawn to the tilemap cache.
        /// </summary>
        /// <param name="column">
        ///     The X position of the tile in the tilemap. The 0 position is on the far left of the tilemap.
        /// </param>
        /// <param name="row">
        ///     The Y position of the tile in the tilemap. The 0 position is on the top of the tilemap.
        /// </param>
        /// <param name="value">
        ///     The new value for the flag. Setting the flag to -1 means no collision.
        /// </param>
        /// <returns></returns>
        public int Flag(int column, int row, int? value = null)
        {
            var tile = TilemapChip.GetTile(column, row);

            if (value.HasValue) tile.Flag = value.Value;

            return tile.Flag;
        }


        /// <summary>
        ///     This allows you to get the current sprite id, color offset and flag values associated with
        ///     a given tile. You can optionally supply your own if you want to change the tile's values.
        ///     Changing a tile's sprite id or color offset will for the tilemap to redraw it to the cache
        ///     on the next frame. If you are drawing raw pixel data into the tilemap cache in the same
        ///     position, it will be overwritten with the new tile's pixel data.
        /// </summary>
        /// <param name="column">
        ///     The X position of the tile in the tilemap. The 0 position is on the far left of the tilemap.
        /// </param>
        /// <param name="row">
        ///     The Y position of the tile in the tilemap. The 0 position is on the top of the tilemap.
        /// </param>
        /// <param name="spriteID">
        ///     The sprite id to use for the tile.
        /// </param>
        /// <param name="colorOffset">
        ///     Shift the color IDs by this value.
        /// </param>
        /// <param name="flag">
        ///     An int value between -1 and 16 used for collision detection.
        /// </param>
        /// <param name="flipH"></param>
        /// <param name="flipV"></param>
        /// <returns>
        ///     Returns a dictionary containing the spriteID, colorOffset, and flag for an individual tile.
        /// </returns>
        //TODO this should return a custom class not a Dictionary
        public TileData Tile(int column, int row, int? spriteID = null, int? colorOffset = null, int? flag = null,
            bool? flipH = null, bool? flipV = null)
        {
            var invalidateTileMap = false;

            var tile = TilemapChip.GetTile(column, row);

            if (spriteID.HasValue)
            {
                tile.SpriteId = spriteID.Value;
                invalidateTileMap = true;
            }

            if (colorOffset.HasValue)
            {
                tile.ColorOffset = colorOffset.Value;
                invalidateTileMap = true;
            }

            if (flag.HasValue)
            {
                tile.Flag = flag.Value;
                invalidateTileMap = true;
            }

            if (flipH.HasValue)
            {
                tile.FlipH = flipH.Value;
                invalidateTileMap = true;
            }


            if (flipV.HasValue)
            {
                tile.FlipV = flipV.Value;
                invalidateTileMap = true;
            }

            if (invalidateTileMap) TilemapChip.Invalidate();

            return tile;
            //            return new TileData(CalculateIndex(column, row, tilemapChip.columns), tilemapChip.ReadSpriteAt(column, row), tilemapChip.ReadTileColorAt(column, row), tilemapChip.ReadFlagAt(column, row));
        }

        /// <summary>
        ///     This will return a vector representing the size of the tilemap in columns (x) and rows (y).
        ///     To find the size in pixels, you will need to multiply the returned vectors x and y values by
        ///     the sprite size's x and y. This method also allows you to resize the tilemap by passing in an
        ///     optional new width and height. Resizing the tile map is destructive, so any changes will
        ///     automatically clear the tilemap's sprite ids, color offsets, and flag values.
        /// </summary>
        /// <param name="width">
        ///     An optional parameter for the width in tiles of the map.
        /// </param>
        /// <param name="height">
        ///     An option parameter for the height in tiles of the map.
        /// </param>
        /// <returns>
        ///     Returns a vector of the tile maps size in tiles where x and y are the columns and rows of the tilemap.
        /// </returns>
        public Point TilemapSize(int? width = null, int? height = null, bool clear = false)
        {

            // Update with the latest value
            _tilemapSize.X = TilemapChip.columns;
            _tilemapSize.Y = TilemapChip.rows;

            var resize = false;

            if (width.HasValue)
            {
                _tilemapSize.X = width.Value;
                resize = true;
            }

            if (height.HasValue)
            {
                _tilemapSize.Y = height.Value;
                resize = true;
            }

            if (resize) TilemapChip.Resize(_tilemapSize.X, _tilemapSize.Y, clear);

            return _tilemapSize;
        }

        /// <summary>
        ///     A helper method which allows you to update several tiles at once. Simply define the start column
        ///     and row position, the width of the area to update in tiles and supply a new int array of sprite
        ///     IDs. You can also modify the color offset and flag value of the tiles via the optional parameters.
        ///     This helper method uses calls the Tile() method to update each tile, so any changes to a tile
        ///     will be automatically redrawn to the tilemap's cache.
        /// </summary>
        /// <param name="column">
        ///     Start column of the first tile to update. The 0 column is on the far left of the tilemap.
        /// </param>
        /// <param name="row">
        ///     Start row of the first tile to update. The 0 row is on the top of the tilemap.
        /// </param>
        /// <param name="columns">
        ///     The width of the area in tiles to update.
        /// </param>
        /// <param name="ids">
        ///     An array of sprite IDs to use for each tile being updated.
        /// </param>
        /// <param name="colorOffset">
        ///     An optional color offset int value to be applied to each updated tile.
        /// </param>
        /// <param name="flag">
        ///     An optional flag int value to be applied to each updated tile.
        /// </param>
        public void UpdateTiles(int[] ids, int? colorOffset = null, int? flag = null)
        {
            total = ids.Length;
            _width = TilemapSize().X;

            //TODO need to get offset and flags working

            for (var i = 0; i < total; i++)
            {
                id = ids[i];

                var pos = MathUtil.CalculatePosition(id, _width);

                Tile(pos.X, pos.Y, null, colorOffset, flag);
            }
        }

        #endregion

        #region Utils

        public int ReadFPS()
        {
            return fps;
        }

        public int ReadTotalSprites()
        {
            return CurrentSprites;
        }

        #endregion

        /// <summary>
        ///     This method will automatically calculate the start color offset for palettes in the color chip.
        /// </summary>
        /// <param name="paletteID">The palette number, 1 - 8</param>
        /// <returns></returns>
        public int PaletteOffset(int paletteID, int paletteColorID = 0)
        {
            // TODO this is hardcoded right now but there are 8 palettes with a max of 16 colors each
            return 128 + MathUtil.Clamp(paletteID, 0, 7) * 16 + MathUtil.Clamp(paletteColorID, 0, ColorsPerSprite() - 1);
        }

        #region Math

        /// <summary>
        ///     Limits a value between a minimum and maximum.
        /// </summary>
        /// <param name="val">
        ///     The value to clamp.
        /// </param>
        /// <param name="min">
        ///     The minimum the value can be.
        /// </param>
        /// <param name="max">
        ///     The maximum the value can be.
        /// </param>
        /// <returns>
        ///     Returns an int within the min and max range.
        /// </returns>
        public int Clamp(int val, int min, int max)
        {
            return MathHelper.Clamp(val, min, max);
        }

        /// <summary>
        ///     Repeats a value based on the max. When the value is greater than the max, it starts
        ///     over at 0 plus the remaining value.
        /// </summary>
        /// <param name="val">
        ///     The value to repeat.
        /// </param>
        /// <param name="max">
        ///     The maximum the value can be.
        /// </param>
        /// <returns>
        ///     Returns an int that is never less than 0 or greater than the max.
        /// </returns>
        public int Repeat(int val, int max)
        {
            return MathUtil.Repeat(val, max);
        }

        /// <summary>
        ///     Converts an X and Y position into an index. This is useful for finding positions in 1D
        ///     arrays that represent 2D data.
        /// </summary>
        /// <param name="x">
        ///     The x position.
        /// </param>
        /// <param name="y">
        ///     The y position.
        /// </param>
        /// <param name="width">
        ///     The width of the data if it was represented as a 2D array.
        /// </param>
        /// <returns>
        ///     Returns an int value representing the X and Y position in a 1D array.
        /// </returns>
        public int CalculateIndex(int x, int y, int width)
        {
            // int index;
            // index = x + y * width;
            return MathUtil.CalculateIndex(x, y, width);
        }

        /// <summary>
        ///     Converts an index into an X and Y position to help when working with 1D arrays that
        ///     represent 2D data.
        /// </summary>
        /// <param name="index">
        ///     The position of the 1D array.
        /// </param>
        /// <param name="width">
        ///     The width of the data if it was a 2D array.
        /// </param>
        /// <returns>
        ///     Returns a vector representing the X and Y position of an index in a 1D array.
        /// </returns>
        public Point CalculatePosition(int index, int width)
        {
            return MathUtil.CalculatePosition(index, width);
        }

        /// <summary>
        ///     Fast calculation to get the distance between two points
        /// </summary>
        /// <param name="x0"></param>
        /// <param name="y0"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <returns></returns>
        public int CalculateDistance(int x0, int y0, int x1, int y1)
        {
            var dx = x1 - x0;
            var dy = y1 - y0;
            var distance = MathUtil.Sqrt(dx * dx + dy * dy);
            return (int)distance;
        }

        #endregion

    }
}