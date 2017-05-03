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

namespace PixelVisionSDK
{
    /// <summary>
    ///     This interface represents the main API for the
    ///     engine. This is exposed to games so they can interact with the sub
    ///     systems of the engine.
    /// </summary>
    public interface IPixelVisionAPI : IKeyInput, IMouseInput
    {
        #region Sprite APIs

        /// <summary>
        ///     Returns the pixel data of a sprite.
        /// </summary>
        /// <param name="id">The unique ID of the sprite to look up.</param>
        /// <returns>Returns an int array containing color IDs.</returns>
        int[] ReadSpriteAt(int id);

        /// <summary>
        ///     Replaces the pixel data of a sprite.
        /// </summary>
        /// <param name="id">The unique ID of the sprite to look up.</param>
        /// <param name="pixels">An int array containing color IDs.</param>
        void UpdateSpriteAt(int id, int[] pixels);

        /// <summary>
        ///     Draws a sprite to the display.
        /// </summary>
        /// <param name="id">
        ///     Index of the sprite inside of the sprite chip.
        /// </param>
        /// <param name="x">
        ///     X position to place sprite on the screen. 0 is left side of screen.
        /// </param>
        /// <param name="y">
        ///     Y position to place sprite on the screen. 0 is the top of the screen.
        /// </param>
        /// <param name="flipH">
        ///     This flips the sprite horizontally. Set to false by default
        /// </param>
        /// <param name="flipV">
        ///     This flips the sprite vertically. Set to false by default.
        /// </param>
        /// <param name="aboveBG">
        ///     Defines if the sprite is above or below the background layer. Set to true by default.
        /// </param>
        /// <param name="colorOffset">
        ///     This value offsets all the color ID's of the sprite. Use this to simulate palette shifting.
        /// </param>
        void DrawSprite(int id, int x, int y, bool flipH = false, bool flipV = false, bool aboveBG = true, int colorOffset = 0);

        /// <summary>
        ///     Draws a group of sprites in a grid. This is useful when trying to
        ///     draw sprites larger than 8x8. Each sprite in the array still counts 
        ///     as an individual sprite so it will only render as many sprites that 
        ///     are remaining during the draw pass.
        /// </summary>
        /// <param name="ids">Ids of the sprites to draw</param>
        /// <param name="x">
        ///     The upper left corner of where the first sprite should be drawn.
        /// </param>
        /// <param name="y">The top of where the sprite should be drawn.</param>
        /// <param name="width">
        ///     The width of the larger sprite in columns.
        /// </param>
        /// <param name="flipH">
        ///     This flips the sprite horizontally. Set to false by default
        /// </param>
        /// <param name="flipV">
        ///     This flips the sprite vertically. Set to false by default.
        /// </param>
        /// <param name="aboveBG">
        ///     Defines if the sprite is above or below the background layer. Set to true by default.
        /// </param>
        /// <param name="colorOffset">
        ///     This value offsets all the color ID's of the sprite. Use this to simulate palette shifting.
        /// </param>
        void DrawSprites(int[] ids, int x, int y, int width, bool flipH = false, bool flipV = false, bool aboveBG = true,
            int colorOffset = 0);

        #endregion

        #region Tile APIs

        /// <summary>
        ///     This helper method makes it easy to draw a new tile in the tilemap.
        /// </summary>
        /// <param name="id">The id of the sprite to use.</param>
        /// <param name="column">
        ///     Column position to draw tile to. 0 is the left of the tilemap.
        /// </param>
        /// <param name="row">
        ///     Row position to draw tile to. 0 is the top of the tilemap.
        /// </param>
        /// <param name="colorOffset">
        ///     This value offsets all the color ID's of the sprite. Use this to simulate palette shifting.
        /// </param>
        void DrawTile(int id, int column, int row, int colorOffset = 0);

        /// <summary>
        ///     This helper method allows you to draw multiple tiles to the tilemap in a grid.
        /// </summary>
        /// <param name="ids">The sprite ids to use for the tiles.</param>
        /// <param name="column">
        ///     Column position to draw tile to. 0 is the left of the tilemap.
        /// </param>
        /// <param name="row">
        ///     Row position to draw tile to. 0 is the top of the tilemap.
        /// </param>
        /// <param name="columns">The total width of the tiles to be drawn.</param>
        /// <param name="colorOffset">
        ///     This value offsets all the color ID's of the sprite. Use this to simulate palette shifting.
        /// </param>
        void DrawTiles(int[] ids, int column, int row, int columns, int colorOffset = 0);

        /// <summary>
        ///     This method allows you to update all of the values of a single tile in the tilemap.
        /// </summary>
        /// <param name="id">The sprite id to use for the tile.</param>
        /// <param name="column">
        ///     Column position to draw tile to. 0 is the left of the tilemap.
        /// </param>
        /// <param name="row">
        ///     Row position to draw tile to. 0 is the top of the tilemap.
        /// </param>
        /// <param name="flag">The collision flag value for this tile. Default value is -1 for no collision.</param>
        /// <param name="colorOffset">
        ///     This value offsets all the color ID's of the sprite. Use this to simulate palette shifting.
        /// </param>
        void UpdateTile(int id, int column, int row, int flag = -1, int colorOffset = 0);

        /// <summary>
        ///     Returns the value of a flag set in the tile map. Used mostly for
        ///     collision detection.
        /// </summary>
        /// <param name="column">
        ///     Column in the tile map to read from. 0 is the left side of the tilemap.
        /// </param>
        /// <param name="row">
        ///     Row in the tile map to read from. 0 is the top side of the tilemap.
        /// </param>
        /// <returns>
        ///     Returns a bit value based on the total number of flags set in the
        ///     tile map chip.
        /// </returns>
        int ReadFlagAt(int column, int row);

        /// <summary>
        ///     Update a tile's flag value.
        /// </summary>
        /// <param name="flag">The collision flag value for this tile. Default value is -1 for no collision.</param>
        /// <param name="column">
        ///     Column position to draw tile to. 0 is the left of the tilemap.
        /// </param>
        /// <param name="row">
        ///     Row position to draw tile to. 0 is the top of the tilemap.
        /// </param>
        void UpdateFlagAt(int flag, int column, int row);

        /// <summary>
        ///     Use this to read the sprite id of a tile.
        /// </summary>
        /// <param name="column">
        ///     Column position to draw tile to. 0 is the left of the tilemap.
        /// </param>
        /// <param name="row">
        ///     Row position to draw tile to. 0 is the top of the tilemap.
        /// </param>
        /// <returns>Returns the sprite id as an int.</returns>
        int ReadTile(int column, int row);

        /// <summary>
        ///     Use this to read the tile's color offset value.
        /// </summary>
        /// <param name="column">
        ///     Column position to draw tile to. 0 is the left of the tilemap.
        /// </param>
        /// <param name="row">
        ///     Row position to draw tile to. 0 is the top of the tilemap.
        /// </param>
        /// <returns>Returns an int for the colorOffset.</returns>
        int ReadTileColorAt(int column, int row);

        /// <summary>
        ///     Use this to update a tile's color offset to simulate palette swapping.
        /// </summary>
        /// <param name="value">The value of the color offset which is applied to all of the color ids making up the tile's pixel data.</param>
        /// <param name="column">
        ///     Column position to draw tile to. 0 is the left of the tilemap.
        /// </param>
        /// <param name="row">
        ///     Row position to draw tile to. 0 is the top of the tilemap.
        /// </param>
        void UpdateTileColorAt(int value, int column, int row);

        /// <summary>
        ///     Clears the entire tilemap including the tilemap cache.
        /// </summary>
        void ClearTilemap();

        /// <summary>
        ///     Reloads the default tilemap and replaces any changes made after it was loaded. Use this to 
        ///     revert the tilemap back to it's initial values.
        /// </summary>
        void RebuildMap();

        #endregion

        #region Text APIs

        /// <summary>
        ///     Use this method to draw text to the display using sprites. Each character will use a single sprite 
        ///     which counts against the total sprite count. If no more sprites can be displayed, they will not be
        ///     displayed.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="x">
        ///     X position to place sprite on the screen. 0 is left side of screen.
        /// </param>
        /// <param name="y">
        ///     Y position to place sprite on the screen. 0 is the top of the screen.
        /// </param>
        /// <param name="fontName">The name of the font.</param>
        /// <param name="colorOffset">
        ///     This value offsets all the color ID's of the sprite. Use this to simulate palette shifting.
        /// </param>
        /// <param name="spacing">The spacing in pixels between each character. The default value is 0.</param>
        void DrawSpriteText(string text, int x, int y, string fontName = "Default", int colorOffset = 0, int spacing = 0);

        /// <summary>
        ///     This method allows you to set individual tiles in the tilemap to a font characters. This is useful
        ///     when you want to add text to the background and not use sprites. Note that you can not change the
        ///     character spacing since this method only works with single tile wide characters.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="x">
        ///     X position to place sprite on the screen. 0 is left side of screen.
        /// </param>
        /// <param name="y">
        ///     Y position to place sprite on the screen. 0 is the top of the screen.
        /// </param>
        /// <param name="fontName">The name of the font.</param>
        /// <param name="colorOffset">
        ///     This value offsets all the color ID's of the sprite. Use this to simulate palette shifting.
        /// </param>
        void DrawTileText(string text, int column, int row, string fontName = "Default", int colorOffset = 0);

        /// <summary>
        ///     This method allows you set tiles to a font's characters in a box with word wrap. This is useful
        ///     when you want to add text to the background and not use sprites. Note that you can not change the
        ///     character spacing since this method only works with single tile wide characters.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="x">
        ///     X position to place sprite on the screen. 0 is left side of screen.
        /// </param>
        /// <param name="y">
        ///     Y position to place sprite on the screen. 0 is the top of the screen.
        /// </param>
        /// <param name="characterWidth">The total number of characters before the next line is created.</param>
        /// <param name="fontName">The name of the font.</param>
        /// <param name="colorOffset">
        ///     This value offsets all the color ID's of the sprite. Use this to simulate palette shifting.
        /// </param>
        void DrawTileTextBox(string text, int column, int row, int characterWidth, string fontName = "Default", int colorOffset = 0);

        #endregion

        #region Raw Drawing APIs

        /// <summary>
        ///     Draws raw sprite data to the display. This allows you to create dynamic sprites at run time
        ///     that are not stored in the SpriteChip.
        /// </summary>
        /// <param name="pixelData">An array of ints that represent colors.</param>
        /// <param name="x">
        ///     X position to place sprite on the screen. 0 is left side of screen.
        /// </param>
        /// <param name="y">
        ///     Y position to place sprite on the screen. 0 is the top of the screen.
        /// </param>
        /// <param name="width">The width of the pixel data.</param>
        /// <param name="height">The height of the pixel data.</param>
        /// <param name="flipH">
        ///     This flips the sprite horizontally. Set to false by default
        /// </param>
        /// <param name="flipV">
        ///     This flips the sprite vertically. Set to false by default.
        /// </param>
        /// <param name="aboveBG">
        ///     Defines if the sprite is above or below the background layer. Set to true by default.
        /// </param>
        /// <param name="colorOffset">
        ///     This value offsets all the color ID's of the sprite. Use this to simulate palette shifting.
        /// </param>
        /// <param name="id">
        ///     Index of the sprite inside of the sprite chip.
        /// </param>
        void DrawSpritePixelData(int[] pixelData, int x, int y, int width, int height, bool flipH = false, bool flipV = false, bool aboveBG = true, int colorOffset = 0);

        /// <summary>
        ///     This draws raw tile data into the TilemapChip's cache. As long as the underlying tiles don't
        ///     change, you can continue to draw raw pixel data into the cache layer and use it as a canvas.
        /// </summary>
        /// <param name="pixelData">An array of ints that represent colors.</param>
        /// <param name="column">
        ///     Column position to draw tile to. 0 is the left of the tilemap.
        /// </param>
        /// <param name="row">
        ///     Row position to draw tile to. 0 is the top of the tilemap.
        /// </param>
        /// <param name="width">The width of the pixel data.</param>
        /// <param name="height">The height of the pixel data.</param>
        /// <param name="offsetX">This offset allows you to draw pixel data off of the tilemap's column grid for pixel perfect layout.</param>
        /// <param name="offsetY">This offset allows you to draw pixel data off of the tilemap's row grid for pixel perfect layout.</param>
        void DrawTilePixelData(int[] pixelData, int column, int row, int width, int height, int offsetX = 0, int offsetY = 0);

        /// <summary>
        ///     This draws text into the TilemapChip's cache. It can be used to copy font sprite data and
        ///     render it in a tilemap. This also allows for letter spacing which is useful on fonts that
        ///     have character that are smaller than an individual tile. 
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="column">
        ///     Column position to draw tile to. 0 is the left of the tilemap.
        /// </param>
        /// <param name="row">
        ///     Row position to draw tile to. 0 is the top of the tilemap.
        /// </param>
        /// <param name="fontName">The name of the font.</param>
        /// <param name="colorOffset">
        ///     This value offsets all the color ID's of the sprite. Use this to simulate palette shifting.
        /// </param>
        /// <param name="spacing">The spacing in pixels between each character. The default value is 0.</param>
        /// <param name="offsetX">This offset allows you to draw pixel data off of the tilemap's column grid for pixel perfect layout.</param>
        /// <param name="offsetY">This offset allows you to draw pixel data off of the tilemap's row grid for pixel perfect layout.</param>
        void DrawTileTextPixelData(string text, int column, int row, string fontName = "Default", int colorOffset = 0, int spacing = 0, int offsetX = 0, int offsetY = 0);

        #endregion

        #region Display APIs

        /// <summary>
        ///     The width of the sprites in pixels.
        /// </summary>
        /// <returns>Returns an int.</returns>
        int ReadSpriteWidth();

        /// <summary>
        ///     The height of the sprites in pixels.
        /// </summary>
        /// <returns>Returns an int.</returns>
        int ReadSpriteHeight();

        /// <summary>
        ///     The width of the screen in pixels.
        /// </summary>
        /// <returns>Returns an int.</returns>
        int ReadDisplayWidth();

        /// <summary>
        ///     The height of the screen in pixels.
        /// </summary>
        /// <returns>Returns an int.</returns>
        int ReadDisplayHeight();

        /// <summary>
        ///     The horizontal scroll position of the screen buffer. 0 is the left
        ///     side of the screen.
        /// </summary>
        /// <returns>Returns an int.</returns>
        int ReadScrollX();

        /// <summary>
        ///     Updates the scroll x value.
        /// </summary>
        /// <param name="value">Accepts an int value.</param>
        void UpdateScrollX(int value);

        /// <summary>
        ///     The vertical scroll position of the screen buffer. 0 is the top side
        ///     of the screen.
        /// </summary>
        /// <returns>Returns an int.</returns>
        int ReadScrollY();

        /// <summary>
        ///     Updates the scroll y value.
        /// </summary>
        /// <param name="value">Accepts an int value.</param>
        void UpdateScrollY(int value);

        /// <summary>
        ///     Scrolls the tilemap to a specific position.
        /// </summary>
        /// <param name="x">Accepts an int for the x value.</param>
        /// <param name="y">Accepts an int for the y value.</param>
        void ScrollTo(int x, int y);

        /// <summary>
        ///     Clears the display with the current background color.
        /// </summary>
        void Clear();

        /// <summary>
        ///     Clears an area of the screen with a specific color.
        /// </summary>
        /// <param name="x">
        ///     X position to place sprite on the screen. 0 is left side of screen.
        /// </param>
        /// <param name="y">
        ///     Y position to place sprite on the screen. 0 is the top of the screen.
        /// </param>
        /// <param name="width">The width of the clear area.</param>
        /// <param name="height">The height of the clear area.</param>
        /// <param name="color">A color to use for the clear.</param>
        void ClearArea(int x, int y, int width, int height, int color = -1);

        /// <summary>
        ///     Changes the background color.
        /// </summary>
        /// <param name="id">
        ///     Accepts an int for the background color between 0 and 
        ///     the total colors in the ColorChip.</param>
        void UpdateBackgroundColor(int id);

        /// <summary>
        ///     Returns the current background color.
        /// </summary>
        /// <returns>Returns an int.</returns>
        int ReadBackgroundColor();

        /// <summary>
        ///     Draws a portion of the tilemap at a specific position on the display.
        ///     Simply call this method without any values to redraw the entire display
        ///     with the tilemap.
        /// </summary>
        /// <param name="startCol">The first tile column id to use. 0 is the far left side of the tilemap.</param>
        /// <param name="startRow">The first tile row id to use. 0 is the top of the tilemap.</param>
        /// <param name="columns">The total number of columns to draw to the display.</param>
        /// <param name="rows">The total number of rows to draw to the display.</param>
        /// <param name="offsetX">The x offset where to render the tilemap on the display. 0 is the far left side of the display.</param>
        /// <param name="offsetY">The y offset where to render the tilemap on the display. 0 is the top of the display.</param>
        void DrawTilemap(int startCol = 0, int startRow = 0, int columns = -1, int rows = -1, int offsetX = 0, int offsetY = 0);

        #endregion

        #region Sound APIs

        /// <summary>
        ///     Plays a sound from the sound chip.
        /// </summary>
        /// <param name="id">Play sound at index in the collection.</param>
        /// <param name="channel">
        ///     Define which channel to play the sound on. Each system has a limit
        ///     of number of sounds it can play at a single time.
        /// </param>
        void PlaySound(int id, int channel);

        /// <summary>
        ///     Loads a song into memory for playback.
        /// </summary>
        /// <param name="id"></param>
        void LoadSong(int id);

        /// <summary>
        ///     Plays a song that is loaded in memory. You can chose to have
        ///     the song loop.
        /// </summary>
        /// <param name="loop"></param>
        void PlaySong(bool loop = false);

        /// <summary>
        ///     Pauses the currently playing song.
        /// </summary>
        void PauseSong();

        /// <summary>
        ///     Stops the current song and auto rewinding it to the beginning.
        /// </summary>
        /// <param name="autoRewind"></param>
        void StopSong(bool autoRewind = true);

        /// <summary>
        ///     Rewinds a song to the beginning.
        /// </summary>
        void RewindSong();

        #endregion

        #region Input APIs

        /// <summary>
        ///     Determines a button's value on the current
        ///     frame. Each button has a unique ID.
        /// </summary>
        /// <param name="button">Button ID to test.</param>
        /// <param name="player">
        ///     Id for which player's controller to test. It's set to 0 by default
        ///     for single player game.
        /// </param>
        /// <returns>
        ///     Returns true if the button is currently down.
        /// </returns>
        bool ReadButton(int button, int player = 0);

        /// <summary>
        ///     Determines if a button is up on the current
        ///     frame. Each button has a unique ID.
        /// </summary>
        /// <param name="button">Button ID to test.</param>
        /// <param name="player">
        ///     Id for which player's controller to test. It's set to 0 by default
        ///     for single player game.
        /// </param>
        /// <returns>
        ///     Returns true if the button is currently up.
        /// </returns>
        bool ReadButtonUp(int button, int player = 0);

        /// <summary>
        ///     Determines if a button is down on the current
        ///     frame. Each button has a unique ID.
        /// </summary>
        /// <param name="button">Button ID to test.</param>
        /// <param name="player">
        ///     Id for which player's controller to test. It's set to 0 by default
        ///     for single player game.
        /// </param>
        /// <returns>
        ///     Returns true if the button is currently down.
        /// </returns>
        bool ReadButtonDown(int button, int player = 0);

        /// <summary>
        ///     Determines if a <paramref name="button" /> was just released on the
        ///     previous frame. Each <paramref name="button" /> has a unique ID.
        /// </summary>
        /// <param name="button">Button ID to test.</param>
        /// <param name="player">
        ///     Id for which player's controller to test. It's set to 0 by default
        ///     for single player game.
        /// </param>
        /// <returns>
        ///     Returns true if the button was released in the previous frame.
        /// </returns>
        bool ReadButtonReleased(int button, int player = 0);

        #endregion

        #region Utility APIs

        /// <summary>
        ///     Replaces a single color id in a set of PixelData to a new color
        /// </summary>
        /// <param name="pixelData">An array of ints that represent colors.</param>
        /// <param name="oldID">The old color ID to look for.</param>
        /// <param name="newID">The new color ID to replace.</param>
        /// <returns></returns>
        int[] ReplaceColorID(int[] pixelData, int oldID, int newID);

        /// <summary>
        ///     Replaces multiples colors in a set of PixelData.
        /// </summary>
        /// <param name="pixelData">An array of ints that represent colors.</param>
        /// <param name="oldIDs">The old color IDs to look for.</param>
        /// <param name="newIDs">The new color IDs to replace. Must match the order of the oldIDs array.</param>
        /// <returns></returns>
        int[] ReplaceColorIDs(int[] pixelData, int[] oldIDs, int[] newIDs);

        /// <summary>
        ///     This method converts a set of sprites into raw pixel data. This is
        ///     useful when trying to draw data to the display but need to modify
        ///     it before hand.
        /// </summary>
        /// <param name="ids">An array of sprite ids.</param>
        /// <param name="width">The width in sprites of the raw pixel data.</param>
        /// <returns></returns>
        int[] SpritesToRawData(int[] ids, int width);

        /// <summary>
        ///     Saves data based on a key value pair
        /// </summary>
        /// <param name="key">The key to use when storing the data.</param>
        /// <param name="value">The value associated with the key.</param>
        void UpdateGameData(string key, string value);

        /// <summary>
        ///     Reads saved data based on the supplied key.
        /// </summary>
        /// <param name="key">The key to use when storing the data.</param>
        /// <param name="defaultValue">The value associated with the key if no value is found.</param>
        /// <returns>Returns a string value of based on the key.</returns>
        string ReadGameData(string key, string defaultValue = "undefined");

        #endregion

        #region Deprecated

        /// <summary>
        ///     A flag for whether the engine is
        ///     <see cref="IPixelVisionAPI.paused" /> or not.
        /// </summary>
        bool paused { get; }
        int spriteWidth { get; }
        int spriteHeight { get; }
        int displayWidth { get; }
        int displayHeight { get; }
        bool displayWrap { get; }
        int mouseX { get; }
        int mouseY { get; }
        int scrollX { get; }
        int scrollY { get; }
        void SaveData(string key, string value);
        string ReadData(string key, string defaultValue = "undefined");

        /// <summary>
        ///     This draws pixel data directly to the display. It's the raw drawing
        ///     API that most display drawing methods use.
        /// </summary>
        /// <param name="pixelData">
        ///     Anint array of color data that will be converted
        ///     into pixels.
        /// </param>
        /// <param name="x">
        ///     X position to place pixel data on the screen. 0 is left side of
        ///     screen.
        /// </param>
        /// <param name="y">
        ///     Y position to place pixel data on the screen. 0 is the top of the
        ///     screen.
        /// </param>
        /// <param name="width">Width of the pixel data.</param>
        /// <param name="height">Height of the pixel data.</param>
        /// <param name="flipH">This flips the pixel data horizontally.</param>
        /// <param name="flipV">This flips the pixel data vertically.</param>
        /// <param name="flipY">
        ///     Flip the <paramref name="y" /> position. This corrects the issue
        ///     that Y is at the bottom of the screen.
        /// </param>
        /// <param name="layerOrder">
        ///     Defines if the sprite is above or below the background layer. -1 is
        ///     below and 0 is above. It's set to 0 by default.
        /// </param>
        /// <param name="masked">
        ///     Defines whether the transparent data should be ignored or filled in
        ///     with the background color.
        /// </param>
        /// <param name="colorOffset"></param>
        void DrawPixelData(int[] pixelData, int x, int y, int width, int height, bool flipH, bool flipV, bool flipY, int layerOrder = 0, bool masked = false, int colorOffset = 0);

        /// <summary>
        /// </summary>
        /// <param name="value"></param>
        void TogglePause(bool value);

        /// <summary>
        ///     This enables or disabled the display wrap which allows sprites
        ///     that go off-screen to be rendered on the opposite side.
        /// </summary>
        /// <param name="value"></param>
        void ToggleDisplayWrap(bool value);
        int ReadSpritesInRam();
        /// <summary>
        ///     Rebuilds the screen buffer from the tile map. This rendered the
        ///     entire tile map into the buffer allowing you to cache the tile map
        ///     before running the game for optimization.
        /// </summary>
        void RebuildScreenBuffer();
        /// <summary>
        ///     Draws the screen buffer to the display. The buffer uses its own view
        ///     port width and height as well as scroll x and y offsets to calculate
        ///     what is rendered.
        /// </summary>
        void DrawScreenBuffer(int x = 0, int y = 0, int width = -1, int height = -1, int offsetX = 0, int offsetY = 0);
        /// <summary>
        ///     This draws pixel data directly to the screen buffer. It's the raw
        ///     drawing API that most buffer drawing methods use.
        /// </summary>
        /// <param name="pixelData">
        ///     Anint array of color data that will be converted
        ///     into pixels.
        /// </param>
        /// <param name="x">
        ///     X position to place pixel data on the screen buffer. 0 is left side
        ///     of screen.
        /// </param>
        /// <param name="y">
        ///     Y position to place pixel data on the screen buffer. 0 is the top of
        ///     the screen.
        /// </param>
        /// <param name="width">Width of the pixel data.</param>
        /// <param name="height">Height of the pixel data.</param>
        void DrawBufferData(int[] pixelData, int x, int y, int width, int height);
        /// <summary>
        ///     Reformats text with word wrap.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="witdh"></param>
        /// <param name="wholeWords"></param>
        /// <returns></returns>
        string FormatWordWrap(string text, int witdh, bool wholeWords = false);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="characterWidth"></param>
        /// <returns></returns>
        int CalculateTextBoxHeight(string text, int characterWidth);

        /// <summary>
        ///     Draws a font to the display as sprites. This is an expensive draw
        ///     call since each character is an individual sprite. Use
        ///     DrawFontToBuffer() for better performance.
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
        /// <param name="fontName"></param>
        /// <param name="letterSpacing"></param>
        /// <param name="offset"></param>
        void DrawFont(string text, int x, int y, string fontName = "Default", int letterSpacing = 0, int offset = 0);


        void DrawFontTiles(string text, int column, int row, string fontName = "Default", int offset = 0);

        // Buffer Drawing API
        /// <summary>
        ///     Draws a tile to the screen buffer. The buffer represents the
        ///     rendered data from the tile map. Drawing to the buffer doesn't
        ///     change the tile map, it simply alters the cached tile map in memory.
        /// </summary>
        /// <param name="spriteID">Tile to draw to the buffer</param>
        /// <param name="column">
        ///     Column position to draw tile to the screen buffer. 0 is the left of
        ///     the screen.
        /// </param>
        /// <param name="row">
        ///     Row position to draw tile to the screen buffer. 0 is the top of the
        ///     screen.
        /// </param>
        /// <param name="colorOffset">Shift the color IDs by this value</param>
        void DrawTileToBuffer(int spriteID, int column, int row, int colorOffset = 0);

        /// <summary>
        ///     Draws multiple tiles to the screen buffer. The buffer represents the
        ///     rendered data from the tile map. Drawing to the buffer doesn't
        ///     change the tile map, it simply alters the cached tile map in memory.
        /// </summary>
        /// <param name="ids">Tile IDs to draw to the buffer</param>
        /// <param name="column">
        ///     Column position to draw tile to the screen buffer. 0 is the left of
        ///     the screen.
        /// </param>
        /// <param name="row">
        ///     Row position to draw tile to the screen buffer. 0 is the top of the
        ///     screen.
        /// </param>
        /// <param name="column">
        ///     The width of the tiles in columns being drawn to the
        ///     buffer.
        /// </param>
        /// <param name="colorOffset">Shift the color IDs by this value</param>
        void DrawTilesToBuffer(int[] ids, int column, int row, int columns, int colorOffset = 0);

        /// <summary>
        ///     Draws a font to the screen buffer. This allows you to display more
        ///     <paramref name="text" /> without adding extra draw calls. This may
        ///     be slow to render a lot of <paramref name="text" /> at once so don't
        ///     call during the draw method.
        /// </summary>
        /// <param name="text">
        ///     String that will be renderer to the screen buffer.
        /// </param>
        /// <param name="column">
        ///     Column position to draw tile to the screen buffer. 0 is the left of
        ///     the screen.
        /// </param>
        /// <param name="row">
        ///     Row position to draw tile to the screen buffer. 0 is the top of the
        ///     screen.
        /// </param>
        /// <param name="fontName"></param>
        /// <param name="letterSpacing"></param>
        /// <param name="offset"></param>
        void DrawFontToBuffer(string text, int column, int row, string fontName = "Default", int letterSpacing = 0, int offset = 0);

        /// <summary>
        ///     Draws text to the screen with each character being a sprite bound
        ///     by a specific width.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="witdh"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="fontName"></param>
        /// <param name="letterSpacing"></param>
        /// <param name="wholeWords"></param>
        void DrawTextBox(string text, int witdh, int x, int y, string fontName = "Default", int letterSpacing = 0,
            bool wholeWords = false);

        /// <summary>
        ///     Draws text to the buffer to a predefined width and word wraps.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="witdh"></param>
        /// <param name="column"></param>
        /// <param name="row"></param>
        /// <param name="fontName"></param>
        /// <param name="letterSpacing"></param>
        /// <param name="wholeWords"></param>
        void DrawTextBoxToBuffer(string text, int witdh, int column, int row, string fontName = "Default",
            int letterSpacing = 0,
            bool wholeWords = false);

        /// <summary>
        ///     Changes the background color.
        /// </summary>
        /// <param name="id">
        ///     <see cref="Color" /> id to render text with. Uses color ids from the
        ///     color chip.
        /// </param>
        void ChangeBackgroundColor(int id);


        bool ButtonDown(int button, int player = 0);
        bool ButtonReleased(int button, int player = 0);
        #endregion

    }
}