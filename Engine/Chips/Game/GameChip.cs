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
using PixelVisionSDK.Utils;

namespace PixelVisionSDK.Chips
{

    public enum DrawMode
    {

        Sprite,
        Tile,
        TilemapCache,
        SpriteBelow,
        UI,
        SpriteAbove
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
    public class GameChip : AbstractChip, IUpdate, IDraw, IGameChip
    {

        protected readonly Dictionary<string, int> tmpTileData = new Dictionary<string, int>
        {
            {"spriteID", -1},
            {"colorOffset", -1},
            {"flag", -1}
        };

        protected string _name = "Untitle_Game";
        protected int _saveSlots;
        protected Dictionary<string, string> savedData = new Dictionary<string, string>();

        //private int[] tmpPixelData = new int[0];
        private int[] tmpSpriteData = new int[0];

        protected Vector spriteSizeCached = new Vector();
        protected Vector displaySizeCached = new Vector();
        protected Vector overscanBorderCached = new Vector();
        protected Rect visibleBoundsCached = new Rect();
        protected int colorsPerSpriteCached = 0;
        protected Vector tilemapSizeCached = new Vector();

//        [Flags]
//        internal enum CachedData
//        {
//
//            SpriteSize = 1,
//            DisplaySize = 2,
//            OverscanBounds = 4,
//            VisibleBounds = 8,
//            ColorsPerSprite = 16,
//            TilemapSize = 32,
//            //            TileMapFlags = 64,
//            //            Fonts = 128,
//            //            Meta = 256,
//            //            Music = 512
//
//        }

        #region GameChip APIs

        /// <summary>
        ///     Flag for the maximum size the game should be.
        /// </summary>
        public int maxSize = 256;

        public bool lockSpecs = false;

        public string ext = ".pv8";
        public string version = "0.0.0";
        
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

        protected ColorChip colorChip
        {
            get { return engine.colorChip; }
        }

        protected ColorMapChip colorMapChip
        {
            get { return engine.colorMapChip; }
        }

        protected ControllerChip controllerChip
        {
            get { return engine.controllerChip; }
        }

        protected DisplayChip displayChip
        {
            get { return engine.displayChip; }
        }

        protected SoundChip soundChip
        {
            get { return engine.soundChip; }
        }

        protected SpriteChip spriteChip
        {
            get { return engine.spriteChip; }
        }

        protected TilemapChip tilemapChip
        {
            get { return engine.tilemapChip; }
        }

        protected FontChip fontChip
        {
            get { return engine.fontChip; }
        }

        protected MusicChip musicChip
        {
            get { return engine.musicChip; }
        }

        #endregion

        #region Chip Lifecycle

        /// <summary>
        ///     Configures the GameChip instance by loading it into
        ///     the engine's memory, getting a reference to the
        ///     APIBridge and setting the ready flag to
        ///     true.
        /// </summary>
        public override void Configure()
        {
            
            // Set the engine's game to this instance
            engine.gameChip = this;
                
            // Cache commonly used chip properties that don't change during runtime
            
            // Cached system properties
            spriteSizeCached.x = spriteChip.width;
            spriteSizeCached.y = spriteChip.height;
            
            // Reload display size
            displaySizeCached.x = displayChip.width;
            displaySizeCached.y = displayChip.height;
            
            // Reload overscan size
            overscanBorderCached.x = displayChip.overscanX;
            overscanBorderCached.y = displayChip.overscanY;
            
            // Reload visible bounds
            visibleBoundsCached.x = displayChip.visibleBounds.x;
            visibleBoundsCached.y = displayChip.visibleBounds.y;
            visibleBoundsCached.width = displayChip.visibleBounds.width;
            visibleBoundsCached.height = displayChip.visibleBounds.height;

            // Reload colors per sprite
            colorsPerSpriteCached = spriteChip.colorsPerSprite;
            
            // Tilemap size
            tilemapSizeCached.x = tilemapChip.columns;
            tilemapSizeCached.x = tilemapChip.rows;
            
            // Resize the tmpSpriteData so it mateches the sprite's width and height
            Array.Resize(ref tmpSpriteData, spriteSizeCached.x * spriteSizeCached.x);

            // Mark the game as ready so the engine knows when it should start running
            ready = true;
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
        ///     This is called when a game is initialized. It is only called once when the game is first loaded.
        /// </summary>
        public override void Init()
        {
            // TODO need to cache all the commonly used values

            base.Init();
        }

//        internal void Invalidate(CachedData data)
//        {
//            // Reload the sprite size
//            if ((data & CachedData.SpriteSize) == CachedData.SpriteSize)
//            {
//                spriteSizeCached.x = spriteChip.width;
//                spriteSizeCached.y = spriteChip.height;
//            }
//
//            // Reload display size
//            if ((data & CachedData.DisplaySize) == CachedData.DisplaySize)
//            {
//                displaySizeCached.x = displayChip.width;
//                displaySizeCached.y = displayChip.height;
//            }
//
//            // Reload overscan size
//            if ((data & CachedData.OverscanBounds) == CachedData.OverscanBounds)
//            {
//                overscanBorderCached.x = displayChip.overscanX;
//                overscanBorderCached.y = displayChip.overscanY;
//            }
//
//            // Reload visible bounds
//            if ((data & CachedData.VisibleBounds) == CachedData.VisibleBounds)
//            {
//                visibleBoundsCached.x = displayChip.visibleBounds.x;
//                visibleBoundsCached.y = displayChip.visibleBounds.y;
//                visibleBoundsCached.width = displayChip.visibleBounds.width;
//                visibleBoundsCached.height = displayChip.visibleBounds.height;
//            }
//
//            // Reload colors per sprite
//            if ((data & CachedData.ColorsPerSprite) == CachedData.ColorsPerSprite)
//            {
//                colorsPerSpriteCached = spriteChip.colorsPerSprite;
//            }
//
//            // Tilemap size
//            if ((data & CachedData.TilemapSize) == CachedData.TilemapSize)
//            {
//                tilemapSizeCached.x = tilemapChip.columns;
//                tilemapSizeCached.x = tilemapChip.rows;
//            }
//
//        }



        /// <summary>
        ///     Used for drawing the game to the display.
        /// </summary>
        public virtual void Draw()
        {
            // Overwrite this method and add your own draw logic.
        }

        /// <summary>
        ///     This is called when a game is reset.
        /// </summary>
//        public override void Reset()
//        {
//            
//
//            base.Reset();
//        }
        
        /// <summary>
        ///     This unloads the game from the engine.
        /// </summary>
        public override void Deactivate()
        {
            base.Deactivate();
            engine.gameChip = null;
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
        public virtual int BackgroundColor(int? id = null)
        {
            if (id.HasValue)
                colorChip.backgroundColor = id.Value;

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
        public int TotalColors(bool ignoreEmpty = false)
        {
            return ignoreEmpty ? colorChip.supportedColors : colorChip.total;
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
            return colorsPerSpriteCached;//spriteChip.colorsPerSprite;
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
        ///     Clearing the display removed all of the existing pixel data, replacing it with the default background
        ///     color. The Clear() method allows you specify what region of the display to clear. By simply calling
        ///     Clear(), with no arguments, it automatically clears the entire display. You can manually define an area
        ///     of the screen to clear by supplying option x, y, width and height arguments. When clearing a specific
        ///     area of the display, anything outside of the defined boundaries remains on the next draw phase. This is
        ///     useful for drawing a HUD but clearing the display below for a scrolling map and sprites. Clear can only
        ///     be used once during the draw phase.
        /// </summary>
        /// <param name="x">
        ///     This is an optional value that defaults to 0 and defines where the clear's X position should begin.
        ///     When X is 0, clear starts on the far left-hand side of the display. Values less than 0 or greater than
        ///     the width of the display are ignored.
        /// </param>
        /// <param name="y">
        ///     This is an optional value that defaults to 0 and defines where the clear's Y position should begin. When Y
        ///     is 0, clear starts at the top of the display. Values less than 0 or greater than the height of the display
        ///     are ignored.
        /// </param>
        /// <param name="width">
        ///     This is an optional value that defaults to the width of the display and defines how many horizontal pixels
        ///     to clear. When the width is 0, clear starts at the x position and ends at the far right-hand side of the
        ///     display. Values less than 0 or greater than the width are adjusted to stay within the boundaries of the
        ///     screen's visible pixels.
        /// </param>
        /// <param name="height">
        ///     This is an optional value that defaults to the height of the display and defines how many vertical pixels
        ///     to clear. When the height is 0, clear starts at the Y position and ends at the bottom of the display.
        ///     Values less than 0 or greater than the height are adjusted to stay within the boundaries of the screen's
        ///     visible pixels.
        /// </param>
        public void Clear(int x = 0, int y = 0, int width = 0, int height = 0)
        {
            displayChip.ClearArea(x, y, width, height);
        }

        /// <summary>
        ///     The display's size defines the visible area where pixel data exists on the screen. Calculating this is
        ///     important for knowing how to position sprites on the screen. The DisplaySize() method allows you to get
        ///     the resolution of the display at run time. While you can also define a new resolution by providing a
        ///     width and height value, this may not work correctly at runtime and is currently experimental. You should
        ///     instead set the resolution before loading the game. If you are using overscan, you must subtract it from
        ///     the width and height of the returned vector to find the "visible pixel" dimensions.
        /// </summary>
        /// <param name="width">
        ///     An optional value that defaults to null. Setting this argument changes the pixel width of the display.
        ///     Avoid using this at run-time.
        /// </param>
        /// <param name="height">New height for the display.</param>
        /// <returns>
        ///     This method returns a Vector representing the display's size. The X and Y values refer to the pixel width
        ///     and height of the screen.
        /// </returns>
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

//            if (resize)
//            {
//                displayChip.ResetResolution(size.x, size.y);
//
//                // Since we are changing the resolution, we need to invalidate the display size, overscan and visible bounds
//                var dataFlags = CachedData.DisplaySize;
//                dataFlags |= CachedData.OverscanBounds;
//                dataFlags |= CachedData.VisibleBounds;
//
//                Invalidate(dataFlags);
//            }
                

            return displaySizeCached;
        }

        /// <summary>
        ///     Pixel Vision 8's overscan value allows you to define parts of the screen that are not visible similar
        ///     to how older CRT TVs rendered images. This overscan border allows you to hide sprites off the screen
        ///     so they do not wrap around the edges. You can call OverscanBorder() without any arguments to return a
        ///     vector for the right and bottom border value. This value represents a full column and row that the
        ///     renderer crops from the tilemap. To get the actual pixel value of the right and bottom border, multiply
        ///     this value by the sprite's size. It is also important to note that Pixel Vision 8 automatically crops
        ///     the display to reflect the overscan. So a resolution of 256x244, with an overscan x and y value of 1,
        ///     actually displays 248x236 pixels. While you can change the OverscanBorder at run-time by calling
        ///     OverscanBorder() and supplying a new X and Y value, this should not be done while a game is running.
        /// </summary>
        /// <param name="x">
        ///     An optional argument that represents the number of columns from the right edge of the screen to not
        ///     display. Each column value removes 8 pixels. So setting X to 1 eliminates the width of a single sprite
        ///     from the screen's right-hand border.
        /// </param>
        /// <param name="y">
        ///     An optional argument that represents the number of rows from the bottom edge of the screen to not
        ///     display. Each row value removes 8 pixels. So setting Y to 1 eliminates the height of a single sprite
        ///     from the screen's bottom border.
        /// </param>
        /// <returns>
        ///     This method returns the overscan's X (right) and Y (bottom) border value as a vector. Each X and Y
        ///     value needs to be multiplied by 8 to get the actual pixel size of the overscan border. Use this value
        ///     to calculate the actual visible screen area which may be different than the display's native resolution.
        ///     Also useful to position sprites offscreen when not needed, so they do not wrap around the screen.
        /// </returns>
        public Vector OverscanBorder(int? x, int? y)
        {
            //var size = new Vector();
            var changeBorder = false;

            if (x.HasValue)
            {
                //size.x = ;
                displayChip.overscanX = x.Value;
                changeBorder = true;
            }
//            else
//            {
//                size.x = displayChip.width;
//            }

            if (y.HasValue)
            {
                changeBorder = true;
                //size.y = y.Value;
                displayChip.overscanY = y.Value;
            }
//            else
//            {
//                size.y = displayChip.height;
//            }

//            if (changeBorder)
//            {
//                // Since we are changing the overscan border, we need to invalidate the overscan and visible bounds
//
//                var dataFlags = CachedData.OverscanBounds;
//                dataFlags |= CachedData.VisibleBounds;
//
//                Invalidate(dataFlags);
//
//            }

            return overscanBorderCached;
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
        /// <param name="width">
        ///     The width of the pixel data to use when rendering to the display.
        /// </param>
        /// <param name="height">
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
        public void DrawPixels(int[] pixelData, int x, int y, int width, int height, DrawMode drawMode = DrawMode.Sprite, bool flipH = false, bool flipV = false, int colorOffset = 0)
        {
            switch (drawMode)
            {
                // Mode 0 and 1 are for sprites (above/below bg)
                case DrawMode.Sprite:
                case DrawMode.SpriteAbove:
                case DrawMode.SpriteBelow:
                    var layerOrder = drawMode == DrawMode.SpriteBelow ? -1 : 1;
                    
                    displayChip.NewDrawCall(pixelData, x, y, width, height, flipH, !flipV, true, layerOrder, false, colorOffset);

                    break;
                case DrawMode.TilemapCache:

                    tilemapChip.UpdateCachedTilemap(pixelData, x, y, width, height, colorOffset);

                    break;
                case DrawMode.UI:
                    
                    displayChip.DrawToUI(pixelData, x, y, width, height, flipH, flipV, colorOffset);
                    break;
            }
        }

        public void DrawPixel(int x, int y, int colorRef, DrawMode drawMode = DrawMode.Sprite)
        {
            // TODO need to figure out how to make this work
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
        /// <param name="aboveBG">
        ///     An optional bool that defines if the sprite is above or below the tilemap. Sprites are set to render above the
        ///     tilemap by default. When rendering below the tilemap, the sprite is visible in the transparent area of the tile
        ///     above the background color.
        /// </param>
        /// <param name="colorOffset">
        ///     This optional argument accepts an int that offsets all the color IDs in the pixel data array. This value is added
        ///     to each int, in the pixel data array, allowing you to simulate palette shifting.
        /// </param>
        public virtual void DrawSprite(int id, int x, int y, bool flipH = false, bool flipV = false, DrawMode drawMode = DrawMode.Sprite, int colorOffset = 0)
        {
            if (!displayChip.CanDraw())
                return;

            //TODO flipping H, V and colorOffset should all be passed into reading a sprite
            spriteChip.ReadSpriteAt(id, tmpSpriteData);

            // Mode 0 is sprite above bg and mode 1 is sprite below bg.
            //var mode = aboveBG ? DrawMode.Sprite : DrawMode.SpriteBelow;
            DrawPixels(tmpSpriteData, x, y, spriteChip.width, spriteChip.height, drawMode, flipH, !flipV, colorOffset);
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
        /// <param name="aboveBG">
        ///     An optional bool that defines if the sprite is above or below the tilemap. Sprites are set to render above the
        ///     tilemap by default. When rendering below the tilemap, the sprite is visible in the transparent area of the tile
        ///     above the background color.
        /// </param>
        /// <param name="colorOffset">
        ///     This optional argument accepts an int that offsets all the color IDs in the pixel data array. This value is added
        ///     to each int, in the pixel data array, allowing you to simulate palette shifting.
        /// </param>
        /// <param name="onScreen">
        ///     This flag defines if the sprites should not render when they are off the screen. Use this in conjunction with
        ///     overscan border control what happens to sprites at the edge of the display. If this value is false, the sprites
        ///     wrap around the screen when they reach the edges of the screen.
        /// </param>
        public void DrawSprites(int[] ids, int x, int y, int width, bool flipH = false, bool flipV = false, DrawMode drawMode = DrawMode.Sprite, int colorOffset = 0, bool onScreen = true)
        {
            //var size = SpriteSize();
//            var sW = spriteSizeCached.x;
//            var sH = spriteSizeCached.y;
//            var displaySize = DisplaySize();
//
//            //TODO this should be cached somewhere
//            var bounds = new Rect(-displayChip.overscanXPixels, -displayChip.overscanYPixels, displaySize.x - displayChip.overscanXPixels, displaySize.y - displayChip.overscanYPixels);
            var bounds = visibleBoundsCached;//displayChip.visibleBounds;

            var total = ids.Length;

            var height = MathUtil.CeilToInt(total / width);

            var startX = x - (onScreen ? displayChip.scrollX : 0);
            var startY = y - (onScreen ? displayChip.scrollY : 0);

            if (flipH || flipV)
                SpriteChipUtil.FlipSpriteData(ref ids, width, height, flipH, flipV);

            // Store the sprite id from the ids array
            int id;
            bool render;

            // TODO need to offset the bounds based on the scroll position before testing against it

            for (var i = 0; i < total; i++)
            {
                // Set the sprite id
                id = ids[i];

                // TODO should also test that the sprite is not greater than the total sprites (from a cached value)
                // Test to see if the sprite is within range
                if (id > -1)
                {
                    x = (MathUtil.FloorToInt(i % width) * spriteSizeCached.x) + startX;
                    y = (MathUtil.FloorToInt(i / width) * spriteSizeCached.y) + startY;
//
//                    var render = true;
                    
                    // Check to see if we need to test the bounds
                    if (onScreen)
                        // This can set the render flag to true or false based on it's location
                        //TODO need to take into account the current bounds of the screen
                        render = x >= bounds.x && x <= bounds.width && y >= bounds.y && y <= bounds.height;
                    else
                    {
                        // If we are not testing to see if the sprite is onscreen it will always render and wrap based on its position
                        render = true;
                    }

                    // If the sprite shoudl be rendered, call DrawSprite()
                    if (render)
                        DrawSprite(id, x, y, flipH, flipV, drawMode, colorOffset);
                }
            }
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
        /// <param name="width">
        ///     This optional argument allows you to wrap text. This accepts an int representing the number of characters before
        ///     wrapping the text. Only set a value if you want the text to wrap. By default, it is set to null and is ignored.
        /// </param>
        /// <returns></returns>
        public int DrawText(string text, int x, int y, DrawMode drawMode = DrawMode.Sprite, string font = "Default", int colorOffset = 0, int spacing = 0, int? width = null)
        {
            if (width > 1)
                text = FontChip.WordWrap(text, width.Value);

            var result = text.Split(new[] {"\n", "\r\n"}, StringSplitOptions.None);
            var lines = result.Length;

            var spriteSize = SpriteSize();

            var charWidth = spriteSize.x;
            var charHeight = spriteSize.y;
            var nextX = x;
            var nextY = y;
            
            for (var i = 0; i < lines; i++)
            {
                var line = result[i];
                var spriteIDs = fontChip.ConvertTextToSprites(line, font);
                var total = spriteIDs.Length;

                for (var j = 0; j < total; j++)
                    if (drawMode == DrawMode.Tile)
                    {
                        Tile(nextX, nextY, spriteIDs[j], colorOffset);
                        nextX++;
                    }
                    else if (drawMode == DrawMode.TilemapCache || drawMode == DrawMode.UI)
                    {
                        var pixelData = fontChip.ConvertCharacterToPixelData(line[j], font);
                        
                        if (pixelData != null)
                            DrawPixels(pixelData, nextX, nextY, spriteSize.x, spriteSize.y, drawMode, false, false, colorOffset);

                        // Increase X even if no character was found
                        nextX += charWidth + spacing;
                    }
                    else
                    {
                        DrawSprite(spriteIDs[j], nextX, nextY, false, false, drawMode, colorOffset);
                        nextX += charWidth + spacing;
                    }

                nextX = x;

                if (drawMode == DrawMode.Tile)
                    nextY++;
                else
                    nextY += charHeight;
            }

            return lines;
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
        public void DrawTilemap(int x = 0, int y = 0, int columns = 0, int rows = 0)
        {
            displayChip.DrawTilemap(x, y, columns, rows);
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
        ///     Allows you to save string data to the game file itself. This data persistent even after restarting a game.
        /// </summary>
        /// <param name="key">
        ///     A string to use as the key for the data.
        /// </param>
        /// <param name="value">
        ///     A string representing the data to be saved.
        /// </param>
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
        ///     Allows you to read saved data by supplying a key. If no matching key exists, "undefined" is returned.
        /// </summary>
        /// <param name="key">
        ///     The string key used to find the data.
        /// </param>
        /// <param name="defaultValue">
        ///     The optional string to use if data does not exist.
        /// </param>
        /// <returns>
        ///     Returns string data associated with the supplied key.
        /// </returns>
        public string ReadSaveData(string key, string defaultValue = "undefine")
        {
            if (!savedData.ContainsKey(key))
                WriteSaveData(key, defaultValue);

            return savedData[key];
        }

        #endregion

        #region Input APIs

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
                ? controllerChip.GetKeyUp((int) key)
                : controllerChip.GetKeyDown((int) key);
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
                ? controllerChip.GetMouseButtonUp(button)
                : controllerChip.GetMouseButtonDown(button);
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
                ? controllerChip.ButtonReleased(button, controllerID)
                : controllerChip.ButtonDown(button, controllerID);
        }

        /// <summary>
        ///     The MousePosition() method returns a vector for the current cursor's X and Y position.
        ///     This value is read-only. The mouse's 0,0 position is in the upper left-hand corner of the
        ///     display
        /// </summary>
        /// <returns>
        ///     Returns a vector for the mouse's X and Y poisition.
        /// </returns>
        public Vector MousePosition()
        {
            return controllerChip.ReadMousePosition();
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
            return controllerChip.ReadInputString();
        }

        #endregion

        #region Math APIs

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
            return val.Clamp(min, max);
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
            int index;
            PosUtil.CalculateIndex(x, y, width, out index);
            return index;
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
        public Vector CalculatePosition(int index, int width)
        {
            int x, y;

            PosUtil.CalculatePosition(index, width, out x, out y);

            return new Vector(x, y);
        }
        
        public int CalculateTextHeight(string text, int characterWidth)
        {
            return FontChip.WordWrap(text, characterWidth).Split(new[] {"\n", "\r\n"}, StringSplitOptions.None).Length;
        }
        
        #endregion

        #region Sound APIs

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
            soundChip.PlaySound(id, channel);
        }

        /// <summary>
        ///     This method allows your read and write raw sound data on the SoundChip.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="data"></param>
        public string Sound(int id, string data = null)
        {

            if (data != null)
            {
                soundChip.UpdateSound(id, data);
            }
            
            return soundChip.ReadSound(id).ReadSettings();
        }
        
        /// <summary>
        ///     This helper method allows you to automatically load a set of loops as a complete
        ///     song and plays them back. You can also define if the tracks should loop when they
        ///     are done playing.
        /// </summary>
        /// <param name="trackIDs">
        ///     An array of loop IDs to playback as a single song.
        /// </param>
        /// <param name="loop">
        ///     A bool that determines if the song should loop back to the first ID when it is
        ///     done playing.
        /// </param>
        public void PlaySong(int[] trackIDs, bool loop = true)
        {
            var track = trackIDs[0];

            musicChip.LoadSong(track);

            musicChip.PlaySong(loop);
        }

        /// <summary>
        ///     Toggles the current playback state of the sequencer. If the song
        ///     is playing it will pause, if it is paused it will play.
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
        ///     Rewinds the sequencer to the beginning of the currently loaded song. You can define
        ///     the position in the loop and the loop where playback should begin. Calling this method
        ///     without any arguments will simply rewind the song to the beginning of the first loop.
        /// </summary>
        /// <param name="position">
        ///     Position in the loop to start playing at.
        /// </param>
        /// <param name="loopID">
        ///     The loop to rewind too.
        /// </param>
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
        /// <param name="width">
        ///     Optional argument to change the width of the sprite. Currently not enabled.
        /// </param>
        /// <param name="height">
        ///     Optional argument to change the height of the sprite. Currently not enabled.
        /// </param>
        /// <returns>
        ///     Returns a vector where the X and Y for the sprite's width and height.
        /// </returns>
        public Vector SpriteSize(int? width = 8, int? height = 8)
        {
            var size = new Vector(spriteChip.width, spriteChip.height);

            // TODO you can't resize sprites at runtime

            return size;
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
                spriteChip.UpdateSpriteAt(id, data);
                tilemapChip.InvalidateTileID(id);

                return data;
            }

            spriteChip.ReadSpriteAt(id, tmpSpriteData);

            return tmpSpriteData;
        }
        
        /// <summary>
        ///     This allows you to get the pixel data of multiple sprites. This is a read only method but
        ///     can be used to copy a collection of sprites into memory and draw them to the display in a
        ///     single pass.
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        public int[] Sprites(int[] ids, int width)
        {
            var spriteSize = SpriteSize();
            var realWidth = width * spriteSize.x;
            var realHeight = ((int)Math.Ceiling((float)ids.Length/width)) * spriteSize.y;

            var textureData = new TextureData(realWidth, realHeight);

            int[] tmpSpriteData;
            
            for (int i = 0; i < width; i++)
            {
                tmpSpriteData = Sprite(ids[i]);
                //TODO draw this to the textureData
            }
            
            var pixelData = new int[realWidth * realHeight];
            
            textureData.CopyPixels(ref pixelData);
            
            return pixelData;
            
        }
        
        /// <summary>
        ///     Returns the total number of sprites in the system. You can pass in an optional argument to
        ///     get a total number of sprites the Sprite Chip can store by passing in false for ignoreEmpty.
        ///     By default, only sprites with pixel data will be included in the total return.
        /// </summary>
        /// <param name="ignoreEmpty">
        ///     This is an optional value that defaults to true. When set to true, the SpriteChip returns
        ///     the total number of sprites that are not empty (where all the pixel data is set to -1).
        ///     Set this value to false if you want to get all of the available color slots in the ColorChip
        ///     regardless if they are empty or not.
        /// </param>
        /// <returns>
        ///     This method returns the total number of sprites in the color chip based on the ignoreEmpty
        ///     argument's value.
        /// </returns>
        public int TotalSprites(bool ignoreEmpty = true)
        {
            return ignoreEmpty ? spriteChip.totalSprites : spriteChip.spritesInRam;
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
            if (value.HasValue)
                tilemapChip.UpdateFlagAt(column, row, value.Value);

            return tilemapChip.ReadFlagAt(column, row);
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
        /// <returns>
        ///     Returns a dictionary containing the spriteID, colorOffset, and flag for an individual tile.
        /// </returns>
        //TODO this should return a custom class not a Dictionary
        public Dictionary<string, int> Tile(int column, int row, int? spriteID = null, int? colorOffset = null, int? flag = null)
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
        public void UpdateTiles(int column, int row, int columns, int[] ids, int? colorOffset = null, int? flag = null)
        {
            var total = ids.Length;

            int id, newX, newY;

            //TODO need to get offset and flags working

            for (var i = 0; i < total; i++)
            {
                id = ids[i];
    
                newX = MathUtil.FloorToInt(i % columns) + column;
                newY = MathUtil.FloorToInt(i / (float) columns) + row;

                Tile(newX, newY, id, colorOffset, flag);
            }
        }
        
        #endregion

        public void StopSound(int id, int channel = 0)
        {
            soundChip.StopSound(id, channel);
        }

        public int MaxSpriteCount()
        {
            return displayChip.maxSpriteCount;
        }
    }

}