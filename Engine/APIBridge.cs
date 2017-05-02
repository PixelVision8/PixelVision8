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
using PixelVisionSDK.Chips;
using PixelVisionSDK.Services;
using PixelVisionSDK.Utils;

namespace PixelVisionSDK
{
    /// <summary>
    ///     This is the communication layer between the games
    ///     and the engine's chips. It's designed to provide a clean and safe API
    ///     for games to use without exposing the rest of the underpinnings of the
    ///     engine.<br />
    /// </summary>
    /// <remarks>
    ///     This is the class diagram<br />
    ///     <img src="Content/images/apibridge.png" />
    /// </remarks>
    public class APIBridge : IAPIBridge
    {
        private readonly int[] tmpSpriteData = new int[8 * 8];
        private int[] tmpPixelData = new int[0];
        protected bool _paused;
        protected IEngineChips chips { get; set; }

        /// <summary>
        ///     Returns a reference to the current game instance.
        /// </summary>
        public GameChip currentGame
        {
            get { return chips.currentGame; }
        }

        /// <summary>
        ///     Offers access to the underlying service manager to expose internal
        ///     service APIs to any class referencing the APIBridge.
        /// </summary>
        /// <param name="id">Name of the service.</param>
        /// <returns>Returns an IService instance associated with the supplied ID.</returns>
        public IService GetService(string id)
        {
            return chips.chipManager.GetService(id);
        }

        /// <summary>
        ///     The APIBridge represents the public facing methods used to control
        ///     the PixelVisionEngine class and run games. The goal of this class
        ///     is to have a common interface to code against to insure that the
        ///     core of the engine remains hidden from the game's logic.
        /// </summary>
        /// <param name="enginechips">Reference to all of the chips.</param>
        public APIBridge(IEngineChips enginechips)
        {
            chips = enginechips;
        }




        public string inputString
        {
            get { return ReadInputString(); }
        }
        public void UpdateScrollY(int value)
        {
            chips.displayChip.scrollY = value;
        }

        public void ScrollTo(int x, int y)
        {
            chips.displayChip.scrollX = x;
            chips.displayChip.scrollY = y;
        }

        public Vector ReadMousePosition()
        {
            return chips.controllerChip.mousePosition;
        }

        /// <summary>
        ///     Determines if the mouse button is down.
        /// </summary>
        /// <param name="id">
        ///     The id of the mouse button. Its set to 0 by default. 0 is the left
        ///     mouse and 1 is the right.
        /// </param>
        /// <returns>
        /// </returns>
        public bool ReadMouseButton(int button)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     <para>Returns true during the frame the user pressed the given mouse button.</para>
        /// </summary>
        /// <param name="button"></param>
        public bool ReadMouseButtonDown(int button)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Determines if the state of the mouse button.
        /// </summary>
        /// <param name="id">
        ///     The id of the mouse button. Its set to 0 by default. 0 is the left
        ///     mouse and 1 is the right.
        /// </param>
        /// <returns>
        /// </returns>
        public bool ReadMouseButtonUp(int button)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Returns the keyboard input entered in the this frame.
        /// </summary>
        /// <returns></returns>
        public string ReadInputString()
        {
            return chips.controllerChip.inputString;
        }

        /// <summary>
        ///     <para>Returns true while the user holds down the key identified by the key KeyCode enum parameter.</para>
        /// </summary>
        /// <param name="key"></param>
        public bool ReadKey(int key)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     <para>
        ///         Returns true during the frame the user starts pressing down the key identified by the key KeyCode enum
        ///         parameter.
        ///     </para>
        /// </summary>
        /// <param name="key"></param>
        public bool ReadKeyDown(int key)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     <para>Returns true during the frame the user releases the key identified by name.</para>
        /// </summary>
        /// <param name="key"></param>
        public bool ReadKeyUp(int key)
        {
            throw new NotImplementedException();
        }

        public int backgroundColor
        {
            get { return chips.colorChip.backgroundColor; }
        }

        public void DrawSprite(int id, int x, int y, bool flipH = false, bool flipV = false, bool aboveBG = true,
            int colorOffset = 0)
        {

            if (!chips.displayChip.CanDraw())
                return;

            chips.spriteChip.ReadSpriteAt(id, tmpSpriteData);

            DrawSpritePixelData(tmpSpriteData, x, y, spriteWidth, spriteHeight, flipH, flipV, aboveBG, colorOffset);
        }

        public void DrawSprites(int[] ids, int x, int y, int width, bool flipH = false, bool flipV = false,
            bool aboveBG = true, int colorOffset = 0)
        {
            //TODO need to allow flipping and match the draw sprite API

            //Debug.Log("Draw "+ids.Length + " sprites.");

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
                    var newX = MathUtil.FloorToInt(i % width) * spriteWidth + x;
                    var newY = MathUtil.FloorToInt(i / width) * spriteWidth + y;
                    DrawSprite(id, newX, newY, flipH, flipV, aboveBG, colorOffset);
                }
            }
        }

        public void UpdateTile(int id, int column, int row, int flag = -1, int colorOffset = 0)
        {
            chips.tileMapChip.UpdateTileAt(id, column, row);
        }

        public void Clear()
        {
            chips.displayChip.ClearArea(0, 0, displayWidth, displayHeight);
        }

        public void ClearArea(int x, int y, int width, int height, int color = -1)
        {
            chips.displayChip.ClearArea(x, y, width, height, color);
        }

        public void UpdateBackgroundColor(int id)
        {
            chips.colorChip.backgroundColor = id;
        }

        public int ReadBackgroundColor()
        {
            return chips.colorChip.backgroundColor;
        }

        

        public void PlaySound(int id, int channel = 0)
        {
            chips.soundChip.PlaySound(id, channel);
        }

        public bool ButtonDown(int button, int player = 0)
        {
            var totalButtons = Enum.GetNames(typeof(Buttons)).Length;

            if (button >= totalButtons)
                return false;

            return chips.controllerChip.ButtonDown(button, player);
        }

        public bool ButtonReleased(int button, int player = 0)
        {
            var totalButtons = Enum.GetNames(typeof(Buttons)).Length;

            if (button >= totalButtons)
                return false;

            return chips.controllerChip.ButtonReleased(button, player);
        }

        public int ReadMouseX()
        {
            return chips.controllerChip.mousePosition.x;
        }

        public int ReadMouseY()
        {
            return chips.controllerChip.mousePosition.y;
        }

        public bool GetMouseButtonDown(int id = 0)
        {
            return chips.controllerChip.GetMouseButtonDown(id);
        }

        public bool GetMouseButtonUp(int id = 0)
        {
            return chips.controllerChip.GetMouseButtonUp(id);
        }

        public bool GetMouseButton(int id = 0)
        {
            return chips.controllerChip.GetMouseButton(id);
        }

        public void UpdateTileColorAt(int value, int column, int row)
        {
            chips.tileMapChip.UpdateTileColorAt(value, column, row);
        }

        /// <summary>
        ///     Reloads the default tilemap and replaces any changes made after it was loaded. Use this to 
        ///     revert the tilemap back to it's initial values.
        /// </summary>
        public void RealoadTilemap()
        {
            throw new NotImplementedException();
        }

        public void DrawSpriteText(string text, int x, int y, string fontName = "Default", int colorOffset = 0, int spacing = 0)
        {
            var width = spriteWidth;
            var nextX = x;

            var spriteIDs = chips.fontChip.ConvertTextToSprites(text, fontName);
            var total = spriteIDs.Length;

            // Draw each character
            for (int i = 0; i < total; i++)
            {
                DrawSprite(spriteIDs[i], nextX, y, false, false, true, colorOffset);
                nextX += width + spacing;
            }

        }

        public void DrawTileText(string text, int column, int row, string fontName = "Default", int colorOffset = 0)
        {
            
            var spriteIDs = chips.fontChip.ConvertTextToSprites(text, fontName);
            var total = spriteIDs.Length;

            // Draw each character as a tile
            for (int i = 0; i < total; i++)
            {
                DrawTile(spriteIDs[i], column + i, row, colorOffset);
            }
        }

        public void DrawTileTextPixelData(string text, int column, int row, string fontName = "Default", int colorOffset = 0, int spacing = 0, int offsetX = 0, int offsetY = 0)
        {
            int width;
            int height;

            chips.fontChip.ConvertTextToPixelData(text, ref tmpPixelData, out width, out height, fontName, spacing, colorOffset);

            DrawTilePixelData(tmpPixelData, column, row, width, height, offsetX, offsetY);
        }

        public void DrawSpriteTextBox(string text, int x, int y, int characterWidth, string fontName = "Default", int colorOffset = 0, int letterSpacing = 0)
        {
            text = FontChip.WordWrap(text, characterWidth);
            var result = text.Split(new[] { "\n", "\r\n" }, StringSplitOptions.None);
            var total = result.Length;
            for (int i = 0; i < total; i++)
            {
                DrawSpriteText(result[i], x, y + i, fontName, colorOffset, letterSpacing);
            }
        }

        public void DrawTileTextBox(string text, int column, int row, int characterWidth, string fontName = "Default", int colorOffset = 0)
        {
            text = FontChip.WordWrap(text, characterWidth);
            var result = text.Split(new[] { "\n", "\r\n" }, StringSplitOptions.None);
            var total = result.Length;
            for (int i = 0; i < total; i++)
            {
                DrawTileText(result[i], column, row + i, fontName, colorOffset);
            }
        }

        public int CalculateTextBoxHeight(string text, int characterWidth)
        {
            return FontChip.WordWrap(text, characterWidth).Split(new[] {"\n", "\r\n"}, StringSplitOptions.None).Length;
        }

        /// <summary>
        ///     This method allows you to quickly update a Tile's visuals (sprite & color offset) without changing,
        ///     the flag value.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="column"></param>
        /// <param name="row"></param>
        /// <param name="colorOffset"></param>
        public void DrawTile(int id, int column, int row, int colorOffset = 0)
        {
            chips.tileMapChip.UpdateSpriteAt(column, row, id);
            chips.tileMapChip.UpdateTileColorAt(column, row, colorOffset);
        }

        public void DrawTiles(int[] ids, int column, int row, int columns, int colorOffset = 0)
        {
            var total = ids.Length;

            for (var i = 0; i < total; i++)
            {
                var id = ids[i];
                if (id > -1)
                {
                    var newX = MathUtil.FloorToInt(i % columns) + column;
                    var newY = MathUtil.FloorToInt(i / columns) + row;

                    DrawTile(id, newX, newY, colorOffset);
                }
            }
        }

        public void DrawTextBox(string text, int witdh, int x, int y, string fontName = "Default", int letterSpacing = 0,
            bool wholeWords = false)
        {
            text = FormatWordWrap(text, witdh, wholeWords);

            DrawFont(text, x, y, fontName, letterSpacing);
        }

        public string FormatWordWrap(string text, int witdh, bool wholeWords = false)
        {
            return wholeWords ? FontChip.WordWrap(text, witdh) : FontChip.Split(text, witdh);
        }

        public void DrawSpritePixelData(int[] pixelData, int x, int y, int width, int height, bool flipH = false, bool flipV = false, bool aboveBG = true, int colorOffset = 0)
        {
            var layerOrder = aboveBG ? 1 : -1;

            y += height - chips.spriteChip.height;

            chips.displayChip.NewDrawCall(pixelData, x, y, width, height, flipH, flipV, true, layerOrder, false, colorOffset);
        }

        public void DrawTilePixelData(int[] pixelData, int column, int row, int width, int height, int offsetX = 0, int offsetY = 0)
        {
            var x = (column * ReadSpriteWidth()) + offsetX;

            row = chips.tileMapChip.rows - row - 1;

            var y = (row * ReadSpriteHeight()) + offsetY;

            chips.tileMapChip.UpdateCachedTilemap(pixelData, x, y, width, height);
        }

        public int ReadSpriteWidth()
        {
            return chips.spriteChip.width;
        }

        public int ReadSpriteHeight()
        {
            return chips.spriteChip.height;
        }

        public int ReadDisplayWidth()
        {
            return chips.displayChip.width;
        }

        public int ReadDisplayHeight()
        {
            return chips.displayChip.height;
        }

        public int ReadScrollX()
        {
            return chips.displayChip.scrollX;
        }

        public void UpdateScrollX(int value)
        {
            chips.displayChip.scrollX = value;
        }

        public int ReadScrollY()
        {
            return chips.displayChip.scrollY;
        }

        public int ReadFlagAt(int column, int row)
        {
            return chips.tileMapChip.ReadFlagAt(column, row);
        }

        public void UpdateFlagAt(int flag, int column, int row)
        {
            chips.tileMapChip.UpdateFlagAt(column, row, flag);
        }

        public int ReadTile(int column, int row)
        {
            return chips.tileMapChip.ReadTileAt(column, row);
        }

        public int ReadTileColorAt(int column, int row)
        {
            return chips.tileMapChip.ReadTileColorAt(column, row);
        }

        public bool GetKey(int key)
        {
            return chips.controllerChip.GetKey(key);
        }

        public bool GetKeyDown(int key)
        {
            return chips.controllerChip.GetKeyDown(key);
        }

        public bool GetKeyUp(int key)
        {
            return chips.controllerChip.GetKeyUp(key);
        }


        public int[] ReadSpriteAt(int id)
        {
            chips.spriteChip.ReadSpriteAt(id, tmpSpriteData);

            return tmpSpriteData;
        }

        public void UpdateSpriteAt(int id, int[] pixels)
        {
            chips.spriteChip.UpdateSpriteAt(id, pixels);
            chips.tileMapChip.InvalidateTileID(id);
        }

        public int ReadSpritesInRam()
        {
            return chips.spriteChip.spritesInRam;
        }

        public int[] SpritesToRawData(int[] ids, int width)
        {
            var spriteChip = chips.spriteChip;
            var spriteWidth = spriteChip.width;
            var spriteHeight = spriteChip.height;
            var realHeight = spriteHeight * MathUtil.CeilToInt(ids.Length / width);
            var realWidth = spriteWidth * width;

            var pixelData = new int[realWidth * realHeight];

            SpriteChipUtil.CovertSpritesToRawData(ref pixelData, ids, width,
                chips);

            return pixelData;
        }

        /// <summary>
        ///     Saves data based on a key value pair
        /// </summary>
        /// <param name="key">The key to use when storing the data.</param>
        /// <param name="value">The value associated with the key.</param>
        public void UpdateGameData(string key, string value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Reads saved data based on the supplied key.
        /// </summary>
        /// <param name="key">The key to use when storing the data.</param>
        /// <param name="defaultValue">The value associated with the key if no value is found.</param>
        /// <returns>Returns a string value of based on the key.</returns>
        public string ReadGameData(string key, string defaultValue = "undefined")
        {
            throw new NotImplementedException();
        }

        public void SaveData(string key, string value)
        {
            chips.currentGame.SaveData(key, value);
        }

        public string ReadData(string key, string defaultValue = "undefined")
        {
            return chips.currentGame.GetData(key, defaultValue);
        }

        public void LoadSong(int id)
        {
            chips.musicChip.LoadSong(id);
        }

        public void PlaySong(bool loop = false)
        {
            chips.musicChip.PlaySong(loop);
        }

        public void PauseSong()
        {
            chips.musicChip.PauseSong();
        }

        public void StopSong(bool autoRewind = true)
        {
            chips.musicChip.StopSong();
            if (autoRewind)
                RewindSong();
            RewindSong();
        }

        public void RewindSong()
        {
            chips.musicChip.RewindSong();
        }

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
        public bool ReadButton(int button, int player = 0)
        {
            throw new NotImplementedException();
        }

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
        public bool ReadButtonUp(int button, int player = 0)
        {
            throw new NotImplementedException();
        }

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
        public bool ReadButtonDown(int button, int player = 0)
        {
            throw new NotImplementedException();
        }

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
        public bool ReadButtonReleased(int button, int player = 0)
        {
            throw new NotImplementedException();
        }

        public int[] ReplaceColorID(int[] pixelData, int oldID, int newID)
        {
            var total = pixelData.Length;
            for (var i = 0; i < total; i++)
                if (pixelData[i] == oldID)
                    pixelData[i] = newID;

            return pixelData;
        }

        public int[] ReplaceColorIDs(int[] pixelData, int[] oldIDs, int[] newIDs)
        {
            var total = pixelData.Length;
            var colorTotal = oldIDs.Length;

            // Make sure both arrays are the same length
            if (colorTotal != newIDs.Length)
                return pixelData;

            //TODO this needs to be optimized
            for (var i = 0; i < total; i++)
            for (var j = 0; j < colorTotal; j++)
                if (pixelData[i] == oldIDs[j])
                    pixelData[i] = newIDs[j];

            return pixelData;
        }

        // Buffer Drawing API
        public virtual void DrawScreenBuffer(int x = 0, int y = 0, int width = -1, int height = -1, int offsetX = 0, int offsetY = 0)
        {
            if (width == -1)
            {
                width = displayWidth;
            }

            if (height == -1)
            {
                height = displayHeight;
            }

            chips.displayChip.ClearArea(offsetX, offsetY, width, height);
            chips.displayChip.DrawTilemap(offsetX, offsetY, MathUtil.FloorToInt((float)width / spriteWidth), MathUtil.FloorToInt((float)height / spriteHeight));
        }

        public virtual void DrawTilemap(int startCol = 0, int startRow = 0, int columns = -1, int rows = -1, int offsetX = 0, int offsetY = 0)
        {
            chips.displayChip.DrawTilemap(startCol, startRow, columns, rows);
        }

        public void ClearTilemap()
        {
            chips.tileMapChip.Clear();    
        }

        #region Deprecated APIs

        public void DrawFontTiles(string text, int column, int row, string fontName = "Default", int offset = 0)
        {
            var spriteIDs = chips.fontChip.ConvertTextToSprites(text, fontName);

            var total = spriteIDs.Length;

            var tilemap = chips.tileMapChip;
            int c, r;
            for (int i = 0; i < total; i++)
            {
                c = column + i;
                r = row;
                tilemap.UpdateSpriteAt(c, r, spriteIDs[i]);
                tilemap.UpdateTileColorAt(c, r, offset);
            }

        }

        // Deprecated These Methods

        public void DrawFont(string text, int x, int y, string fontName = "Default", int letterSpacing = 0, int offset = 0)
        {
            DrawSpriteText(text, x, y, fontName, offset, letterSpacing);
        }

        public void RebuildScreenBuffer()
        {
            //TODO this should clear the cache completely?
            chips.tileMapChip.Invalidate();
            //chips.screenBufferChip.RebuildScreenBuffer();
        }

        public void DrawTileToBuffer(int spriteID, int column, int row, int colorOffset = 0)
        {
            //TODO need to deprecate this method
            chips.tileMapChip.UpdateSpriteAt(column, row, spriteID);
            chips.tileMapChip.UpdateTileColorAt(column, row, colorOffset);
        }

        public void DrawTilesToBuffer(int[] ids, int column, int row, int columns, int colorOffset = 0)
        {
            //TODO need to deprecate this method

            var total = ids.Length;

            for (var i = 0; i < total; i++)
            {
                var id = ids[i];
                if (id > -1)
                {
                    var newX = MathUtil.FloorToInt(i % columns) + column;
                    var newY = MathUtil.FloorToInt(i / columns) + row;

                    DrawTileToBuffer(id, newX, newY, colorOffset);
                }
            }
        }

        public void DrawTextBoxToBuffer(string text, int witdh, int column, int row, string fontName = "Default",
            int letterSpacing = 0, bool wholeWords = false)
        {
            //            text = wholeWords ? FontChip.WordWrap(text, witdh) : FontChip.Split(text, witdh);
            //
            //            DrawFontToBuffer(text, column, row, fontName, letterSpacing);
        }

        public void DrawBufferData(int[] pixelData, int x, int y, int width, int height)
        {
            chips.tileMapChip.UpdateCachedTilemap(pixelData, x, y, width, height);
        }

        [Obsolete]
        public void DrawFontToBuffer(string text, int column, int row, string fontName = "Default", int letterSpacing = 0, int offset = 0)
        {
            DrawTileTextPixelData(text, column, row, fontName, offset, letterSpacing);

        }

        //TODO deprecate this in favor of DrawSpritePixelData
        public void DrawPixelData(int[] pixelData, int x, int y, int width, int height, bool flipH, bool flipV, bool flipY, int layerOrder = 0, bool masked = false, int colorOffset = 0)
        {

            DrawSpritePixelData(pixelData, x, y, width, height, flipH, flipV, layerOrder == 1, colorOffset);
        }

        public bool displayWrap
        {
            get
            {
                return chips.displayChip.wrapMode;
            }
        }

        public void ToggleDisplayWrap(bool value)
        {
            chips.displayChip.wrapMode = value;
        }

        public int spriteWidth
        {
            get { return ReadSpriteWidth(); }
        }

        public int spriteHeight
        {
            get { return ReadSpriteHeight(); }
        }

        public int displayWidth
        {
            get { return ReadDisplayWidth(); }
        }


        public int displayHeight
        {
            get { return ReadDisplayHeight(); }
        }

        public int scrollX
        {
            get { return ReadScrollX(); }
        }

        public int scrollY
        {
            get { return ReadScrollY(); }
        }


        public bool paused
        {
            get { return _paused; }
        }


        public void TogglePause(bool value)
        {
            if (_paused != value)
                if (chips.displayChip != null)
                    chips.displayChip.paused = value;
        }

        public int mouseX
        {
            get { return ReadMouseX(); }
        }

        public int mouseY
        {
            get { return ReadMouseY(); }
        }

        public Vector mousePosition
        {
            get { return chips.controllerChip.mousePosition; }
        }

        public void ChangeBackgroundColor(int id)
        {
            id = id.Clamp(0, chips.colorChip.total - 1);
            chips.colorChip.backgroundColor = id;
        }

        

        
        #endregion


    }
}