using System.Collections.Generic;

namespace PixelVisionSDK.Chips
{
    public interface IGameChip
    {
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
        int BackgroundColor(int? id = null);

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
        /// </param>9
        /// <param name="value">
        ///     This argument is optional. It accepts a hex as a string and updates the supplied color ID's value.
        /// </param>
        /// <returns>
        ///     This method returns a hex string for the supplied color ID. If the color has not been set
        ///     or is out of range, it returns magenta (#FF00FF) which is the default transparent system color.
        /// </returns>
        string Color(int id, string value = null);

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
        int TotalColors(bool ignoreEmpty = false);

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
        int ColorsPerSprite();

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
        void ReplaceColor(int index, int id);

        /// <summary>
        ///     Clear1ing the display removed all of the existing pixel data, replacing it with the default background
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
        void Clear(int x = 0, int y = 0, int? width = null, int? height = null);

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
        Vector DisplaySize(int? width = null, int? height = null);

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
        Vector OverscanBorder(int? x, int? y);

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
        void DrawPixels(int[] pixelData, int x, int y, int width, int height, DrawMode drawMode = DrawMode.Sprite, bool flipH = false, bool flipV = false, int colorOffset = 0);

        void DrawPixel(int x, int y, int colorRef, DrawMode drawMode = DrawMode.Sprite);

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
        void DrawSprite(int id, int x, int y, bool flipH = false, bool flipV = false, DrawMode drawMode = DrawMode.Sprite, int colorOffset = 0);

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
        /// <param name="onScreen">
        ///     This flag defines if the sprites should not render when they are off the screen. Use this in conjunction with
        ///     overscan border control what happens to sprites at the edge of the display. If this value is false, the sprites
        ///     wrap around the screen when they reach the edges of the screen.
        /// </param>
        /// <param name="useScrollPos"></param>
        /// <param name="aboveBG">
        ///     An optional bool that defines if the sprite is above or below the tilemap. Sprites are set to render above the
        ///     tilemap by default. When rendering below the tilemap, the sprite is visible in the transparent area of the tile
        ///     above the background color.
        /// </param>
        void DrawSprites(int[] ids, int x, int y, int width, bool flipH = false, bool flipV = false, DrawMode drawMode = DrawMode.Sprite, int colorOffset = 0, bool onScreen = true, bool useScrollPos = true);

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
        /// <returns></returns>
        int DrawText(string text, int x, int y, DrawMode drawMode = DrawMode.Sprite, string font = "Default", int colorOffset = 0, int spacing = 0);

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
        void DrawTilemap(int x = 0, int y = 0, int columns = 0, int rows = 0, int? offsetX = null, int? offsetY = null);

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
        Vector ScrollPosition(int? x = null, int? y = null);

        /// <summary>
        ///     Allows you to save string data to the game file itself. This data persistent even after restarting a game.
        /// </summary>
        /// <param name="key">
        ///     A string to use as the key for the data.
        /// </param>
        /// <param name="value">
        ///     A string representing the data to be saved.
        /// </param>
        void WriteSaveData(string key, string value);

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
        string ReadSaveData(string key, string defaultValue = "undefine");

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
        void PlaySound(int id, int channel = 0);

        /// <summary>
        ///     This helper method allows you to automatically load a set of loops as a complete
        ///     song and plays them back. You can also define if the tracks should loop when they
        ///     are done playing.
        /// </summary>
        /// <param name="loopIDs">
        ///     An array of loop IDs to playback as a single song.
        /// </param>
        /// <param name="loop">
        ///     A bool that determines if the song should loop back to the first ID when it is
        ///     done playing.
        /// </param>
        void PlaySong(int[] loopIDs, bool loop = true);

        /// <summary>
        ///     Toggles the current playback state of the sequencer. If the song
        ///     is playing it will pause, if it is paused it will play.
        /// </summary>
        void PauseSong();

        /// <summary>
        ///     Stops the sequencer.
        /// </summary>
        void StopSong();

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
        void RewindSong(int position = 0, int loopID = 0);

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
        Vector SpriteSize(int? width = 8, int? height = 8);

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
        int[] Sprite(int id, int[] data = null);

        /// <summary>
        ///     This allows you to get the pixel data of multiple sprites. This is a read only method but
        ///     can be used to copy a collection of sprites into memory and draw them to the display in a
        ///     single pass.
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        int[] Sprites(int[] ids, int width);

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
        int TotalSprites(bool ignoreEmpty = true);

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
        int Flag(int column, int row, int? value = null);

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
        // TODO should this realy return a Dictionary?
        Dictionary<string, int> Tile(int column, int row, int? spriteID = null, int? colorOffset = null, int? flag = null);

        /// <summary>
        ///     This forces the map to redraw its cached pixel data. Use this to clear any pixel data added
        ///     after the map created the pixel data cache.
        /// </summary>
        void RebuildTilemap(int? columns = null, int? rows = null, int[] spriteIDs = null, int[] colorOffsets = null, int[] flags = null);

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
        Vector TilemapSize(int? width = null, int? height = null);

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
        void UpdateTiles(int column, int row, int columns, int[] ids, int? colorOffset = null, int? flag = null);


        string Sound(int id, string data = null);
        void StopSound(int channel = 0);

        void DrawTile(int id, int c, int r, DrawMode drawMode = DrawMode.Tile, int colorOffset = 0);
        void DrawTiles(int[] ids, int c, int r, int width, DrawMode drawMode = DrawMode.Tile, int colorOffset = 0);
        int MaxSpriteCount(int? total);
    }
}