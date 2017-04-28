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
        //TODO need to make sure this is correctly sized when sprite sizes change
        private readonly int[] tmpSpriteData = new int[8 * 8];
        private int[] tmpPixelData = new int[0];
        protected bool _paused;


        /// <summary>
        /// </summary>
        /// <param name="enginechips"></param>
        public APIBridge(IEngineChips enginechips)
        {
            chips = enginechips;
        }

        /// <summary>
        /// </summary>
        /// <param name="enginechips"></param>
        public IEngineChips chips { get; set; }

        public int spriteWidth
        {
            get { return chips.spriteChip.width; }
        }

        public int spriteHeight
        {
            get { return chips.spriteChip.height; }
        }

        public int displayWidth
        {
            get { return chips.displayChip.width; }
        }

        public bool displayWrap
        {
            get { return chips.displayChip.wrapMode; }
        }

        public void ToggleDisplayWrap(bool value)
        {
            chips.displayChip.wrapMode = value;
        }

        public int displayHeight
        {
            get { return chips.displayChip.height; }
        }

        public int scrollX
        {
            get { return chips.displayChip.scrollX; }
        }

        public int scrollY
        {
            get { return chips.displayChip.scrollY; }
        }

        public void ScrollTo(int x, int y)
        {
            if (chips.displayChip == null)
                return;

            chips.displayChip.scrollX = x;
            chips.displayChip.scrollY = y;
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
            get { return chips.controllerChip.mousePosition.x; }
        }

        public int mouseY
        {
            get { return chips.controllerChip.mousePosition.y; }
        }

        public string inputString
        {
            get { return chips.controllerChip.inputString; }
        }

        public Vector mousePosition
        {
            get { return chips.controllerChip.mousePosition; }
        }

        public int backgroundColor
        {
            get { return chips.colorChip.backgroundColor; }
        }

        public void DrawSprite(int id, int x, int y, bool flipH = false, bool flipV = false, bool aboveBG = true,
            int colorOffset = 0)
        {
            //chips.displayChip.DrawSprite(id, x, y, flipH, flipV, aboveBG, colorOffset);

            if (!chips.displayChip.CanDraw())
                return;

            chips.spriteChip.ReadSpriteAt(id, tmpSpriteData);

            

            //chips.displayChip.NewDrawCall(tmpSpriteData, x, y, spriteWidth, spriteHeight, flipH, flipV, true, layerOrder, false, colorOffset);

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

        public void UpdateTile(int spriteID, int column, int row, int flag = -1, int colorOffset = 0)
        {
            chips.tileMapChip.UpdateTileAt(spriteID, column, row);
        }

        public void Clear()
        {
            chips.displayChip.ClearArea(0, 0, displayWidth, displayHeight);
        }

        public void ClearArea(int x, int y, int width, int height, int color = -1)
        {
            chips.displayChip.ClearArea(x, y, width, height, color);
        }

        public void ChangeBackgroundColor(int id)
        {
            id = id.Clamp(0, chips.colorChip.total - 1);
            chips.colorChip.backgroundColor = id;
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
        /// <param name="tileID"></param>
        /// <param name="column"></param>
        /// <param name="row"></param>
        /// <param name="colorOffset"></param>
        public void DrawTile(int tileID, int column, int row, int colorOffset = 0)
        {
            chips.tileMapChip.UpdateSpriteAt(column, row, tileID);
            chips.tileMapChip.UpdatePaletteAt(column, row, colorOffset);
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

//        public void DrawTextBoxTiles(string text, int width, int column, int row, string fontName = "Default", bool wholeWords = true)
//        {
//            text = FormatWordWrap(text, width, wholeWords);
//            var spriteIDs = 
//        }

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

        public void DrawTilePixelData(int[] pixelData, int x, int y, int width, int height)
        {
            chips.tileMapChip.UpdateCachedTilemap(pixelData, x, y, width, height);
        }

        public int ReadFlagAt(int column, int row)
        {
            return chips.tileMapChip.ReadFlagAt(column, row);
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
                tilemap.UpdatePaletteAt(c, r, offset);
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
            chips.tileMapChip.UpdatePaletteAt(column, row, colorOffset);
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
            //chips.tileMapChip.cachedTileMap.SetPixels(x, y, width, height, pixelData);
//            chips.screenBufferChip.UpdatePixelDataAt(x, y, width, height, pixelData);
        }
        
        public void DrawFontToBuffer(string text, int column, int row, string fontName = "Default", int letterSpacing = 0, int offset = 0)
        {
//            int width;
//            int height;
//
//            chips.fontChip.ConvertTextToPixelData(text, ref tmpPixelData, out width, out height, fontName, letterSpacing, offset);
//
//            var x = column;
//            var y = row;
//
//            DrawBufferData(tmpPixelData, x, y, width, height);
        }

        //TODO deprecate this in favor of DrawSpritePixelData
        public void DrawPixelData(int[] pixelData, int x, int y, int width, int height, bool flipH, bool flipV, bool flipY, int layerOrder = 0, bool masked = false, int colorOffset = 0)
        {

            //DrawSpritePixelData(pixelData, x, y, width, height, flipH, flipV, layerOrder ? 1 : 0, colorOffset: colorOffset);
        }
    }
}