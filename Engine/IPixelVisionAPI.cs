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
// Jesse Freeman
// 

using PixelVisionSDK.Engine.Chips.IO.Controller;

namespace PixelVisionSDK.Engine
{
    /// <summary>
    ///     This interface represents the main API for the
    ///     engine. This is exposed to games so they can interact with the sub
    ///     systems of the engine.
    /// </summary>
    public interface IPixelVisionAPI : IKeyInput, IMouseInput
    {
        /// <summary>
        ///     The time difference between the last frame.
        /// </summary>
        float timeDelta { get; }

        /// <summary>
        ///     A flag for whether the engine is
        ///     <see cref="IPixelVisionAPI.paused" /> or not.
        /// </summary>
        bool paused { get; }

        /// <summary>
        ///     The width of the sprites in pixels.
        /// </summary>
        int spriteWidth { get; }

        /// <summary>
        ///     The height of the sprites in pixels.
        /// </summary>
        int spriteHeight { get; }

        /// <summary>
        ///     The width of the screen in pixels.
        /// </summary>
        int displayWidth { get; }

        /// <summary>
        ///     The height of the screen in pixels.
        /// </summary>
        int displayHeight { get; }

        bool displayWrap { get; }

        /// <summary>
        ///     Current x position of the mouse on the screen.
        /// </summary>
        int mouseX { get; }

        /// <summary>
        ///     Current y position of the mouse on the screen.
        /// </summary>
        int mouseY { get; }

        // Screen Buffer
        /// <summary>
        ///     The horizontal scroll position of the screen buffer. 0 is the left
        ///     side of the screen.
        /// </summary>
        int scrollX { get; }

        /// <summary>
        ///     The vertical scroll position of the screen buffer. 0 is the top side
        ///     of the screen.
        /// </summary>
        int scrollY { get; }

        // Drawing APIs

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
        void DrawSprite(int id, int x, int y, bool flipH = false, bool flipV = false, bool aboveBG = true,
            int colorOffset = 0);

        /// <summary>
        ///     Draws a group of sprites in a grid. This is useful when trying to
        ///     draw sprites larger than 8x8. Each sprite in the
        ///     <paramref name="ids" /> array still counts as an indivdual sprite so
        ///     it will only render as many sprites that are remaning during the
        ///     draw pass.
        /// </summary>
        /// <param name="ids">Ids of the sprites to draw</param>
        /// <param name="x">
        ///     The upper left corner of where the first sprite should be drawn.
        /// </param>
        /// <param name="y">The top of where the sprite should be drawn.</param>
        /// <param name="width">
        ///     The width of the larger sprite in columns.
        /// </param>
        /// <param name="flipH"></param>
        /// <param name="flipV"></param>
        /// <param name="aboveBG"></param>
        /// <param name="colorOffset"></param>
        void DrawSprites(int[] ids, int x, int y, int width, bool flipH = false, bool flipV = false, bool aboveBG = true,
            int colorOffset = 0);

        /// <summary>
        ///     Draws a tile into the tile map. Tiles are simply stored in the tile
        ///     map, you need to render the tile map to the screen buffer in order
        ///     to display it.
        /// </summary>
        /// <param name="spriteID">
        ///     Index of the sprite to use from the sprite chip.
        /// </param>
        /// <param name="column">
        ///     Column position to draw tile to. 0 is the left of the screen.
        /// </param>
        /// <param name="row">
        ///     Row position to draw tile to. 0 is the top of the screen.
        /// </param>
        /// <param name="flag"></param>
        /// <param name="colorOffset"></param>
        void UpdateTile(int spriteID, int column, int row, int flag = -1, int colorOffset = 0);

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
        void DrawFont(string text, int x, int y, string fontName = "Default", int letterSpacing = 0);

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
        void DrawFontToBuffer(string text, int column, int row, string fontName = "Default", int letterSpacing = 0);

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
        ///     Returns the value of a flag set in the tile map. Used mostly for
        ///     collision detection.
        /// </summary>
        /// <param name="column">
        ///     Column in the tile map to read from. 0 is the left side of the map.
        /// </param>
        /// <param name="row">
        ///     Row in the tile map to read from. 0 is the top side of the map.
        /// </param>
        /// <returns>
        ///     Returns a bit value based on the total number of flags set in the
        ///     tile map chip.
        /// </returns>
        int ReadFlagAt(int column, int row);

        //TODO need to draw sprites to buffer
        // Direct Drawing API

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
        void DrawPixelData(int[] pixelData, int x, int y, int width, int height, bool flipH, bool flipV, bool flipY,
            int layerOrder = 0, bool masked = false);

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
        ///     Clears the display with the current background color.
        /// </summary>
        void Clear();

        /// <summary>
        ///     Changes the background color.
        /// </summary>
        /// <param name="id">
        ///     <see cref="Color" /> id to render text with. Uses color ids from the
        ///     color chip.
        /// </param>
        void ChangeBackgroundColor(int id);

        /// <summary>
        ///     Draws the screen buffer to the display. The buffer uses its own view
        ///     port width and height as well as scroll x and y offsets to calculate
        ///     what is rendered.
        /// </summary>
        void DrawScreenBuffer();

        /// <summary>
        ///     Rebuilds the screen buffer from the tile map. This rendered the
        ///     entire tile map into the buffer allowing you to cache the tile map
        ///     before running the game for optimization.
        /// </summary>
        void RebuildScreenBuffer();

        // Sound APIs

        /// <summary>
        ///     Plays a sound from the sound chip.
        /// </summary>
        /// <param name="id">Play sound at index in the collection.</param>
        /// <param name="channel">
        ///     Define which channel to play the sound on. Each system has a limit
        ///     of number of sounds it can play at a single time.
        /// </param>
        void PlaySound(int id, int channel);

        // Input APIs

        /// <summary>
        ///     Determines if a <paramref name="button" /> is down on the current
        ///     frame. Each <paramref name="button" /> has a unique ID.
        /// </summary>
        /// <param name="button">Button ID to test.</param>
        /// <param name="player">
        ///     Id for which player's controller to test. It's set to 0 by default
        ///     for single player game.
        /// </param>
        /// <returns>
        /// </returns>
        bool ButtonDown(int button, int player = 0);

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
        /// </returns>
        bool ButtonReleased(int button, int player = 0);

        /// <summary>
        ///     This method converts a set of sprites into raw pixel data. This is
        ///     useful when trying to draw data to the display but need to modify
        ///     it before hand.
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        int[] SpritesToRawData(int[] ids, int width);

        /// <summary>
        /// </summary>
        /// <param name="value"></param>
        void TogglePause(bool value);

        /// <summary>
        ///     This enables or dissabled the display wrap which allows sprites
        ///     that go off-screen to be rendered on the opposite side.
        /// </summary>
        /// <param name="value"></param>
        void ToggleDisplayWrap(bool value);

        /// <summary>
        ///     Scrolls the screen buffer to a specific position.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        void ScrollTo(int x, int y);

        /// <summary>
        ///     Saves data based on a key value pair
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void SaveData(string key, string value);

        /// <summary>
        ///     Reads saved data based on the spupplied key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        string ReadData(string key, string defaultValue = "undefined");

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

        /// <summary>
        ///     Replaces a single color id in a set of PixelData to a new color
        /// </summary>
        /// <param name="pixelData"></param>
        /// <param name="oldID"></param>
        /// <param name="newID"></param>
        /// <returns></returns>
        int[] ReplaceColorID(int[] pixelData, int oldID, int newID);

        /// <summary>
        ///     Replaces multiples colors in a set of PixelData.
        /// </summary>
        /// <param name="pixelData"></param>
        /// <param name="oldIDs"></param>
        /// <param name="newIDs"></param>
        /// <returns></returns>
        int[] ReplaceColorIDs(int[] pixelData, int[] oldIDs, int[] newIDs);
    }
}