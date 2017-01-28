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
    ///     The <see cref="APIBridge" /> is the communication layer between the games
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
        private static readonly int[] tmpPixelData = new int[8 * 8];
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

        /// <summary>
        /// </summary>
        public float timeDelta
        {
            get { return chips.chipManager.timeDelta; }
        }

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
            get { return chips.screenBufferChip.scrollX; }
        }

        public int scrollY
        {
            get { return chips.screenBufferChip.scrollY; }
        }

        public void ScrollTo(int x, int y)
        {
            if (chips.screenBufferChip == null)
                return;

            chips.screenBufferChip.scrollX = x;
            chips.screenBufferChip.scrollY = y;
        }

        public bool paused
        {
            get { return _paused; }
        }

        public void TogglePause(bool value)
        {
            if (_paused != value)
            {
                if (chips.displayChip != null)
                    chips.displayChip.paused = value;
            }
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

        public void DrawSprite(int id, int x, int y, bool flipH = false, bool flipV = false, bool aboveBG = true,
            int colorOffset = 0)
        {
            if (!chips.displayChip.CanDraw())
            {
                //Debug.Log("Out of draw calls");
                return;
            }

            chips.spriteChip.ReadSpriteAt(id, tmpPixelData);

            var layerOrder = aboveBG ? 1 : -1;

            DrawPixelData(tmpPixelData, x, y, spriteWidth, spriteHeight, flipH, flipV, true, layerOrder);
        }

        public void DrawSprites(int[] ids, int x, int y, int width, bool flipH = false, bool flipV = false,
            bool aboveBG = true, int colorOffset = 0)
        {
            //TODO need to allow flipping and match the draw sprite API

            //Debug.Log("Draw "+ids.Length + " sprites.");

            var total = ids.Length;

            for (var i = 0; i < total; i++)
            {
                var id = Convert.ToInt32(ids[i]);
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
            chips.displayChip.Clear();
        }

        public void ChangeBackgroundColor(int id)
        {
            id = id.Clamp(0, chips.colorChip.total - 1);
            chips.screenBufferChip.backgroundColor = id;
        }

        public void PlaySound(int id, int channel = 0)
        {
            chips.soundChip.PlaySound(id, channel);
        }

        public void RebuildScreenBuffer()
        {
            chips.screenBufferChip.RefreshScreenBlock();
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

        public virtual void DrawScreenBuffer()
        {
            chips.displayChip.CopyScreenBlockBuffer();
        }

        public void DrawTileToBuffer(int spriteID, int column, int row, int colorOffset = 0)
        {
            chips.spriteChip.ReadSpriteAt(spriteID, tmpPixelData);

            DrawBufferData(tmpPixelData, column * spriteWidth, row * spriteHeight, spriteWidth, spriteHeight);
        }

        public void DrawTilesToBuffer(int[] ids, int column, int row, int columns, int colorOffset = 0)
        {
            //TODO need to allow flipping and match the draw sprite API

            //Debug.Log("Draw "+ids.Length + " sprites.");

            var total = ids.Length;

            for (var i = 0; i < total; i++)
            {
                var id = Convert.ToInt32(ids[i]);
                if (id > -1)
                {
                    var newX = MathUtil.FloorToInt(i % columns) + column;
                    var newY = MathUtil.FloorToInt(i / columns) + row;

                    DrawTileToBuffer(id, newX, newY, 0);
                }
            }
        }

        public void DrawFont(string text, int x, int y, string fontName = "Default", int letterSpacing = 0)
        {
            if (chips.fontChip != null)
            {
                int[] pixels;
                int width;
                int height;

                chips.fontChip.ConvertTextToPixelData(text, out pixels, out width, out height, fontName, letterSpacing);

                DrawPixelData(pixels, x, y, width, height, false, false, true);
            }
        }

        public void DrawFontToBuffer(string text, int column, int row, string fontName = "Default",
            int letterSpacing = 0)
        {
            int[] pixels;
            int width;
            int height;

            chips.fontChip.ConvertTextToPixelData(text, out pixels, out width, out height
                , fontName, letterSpacing);

            var x = column;
            var y = row;

            chips.screenBufferChip.MergePixelDataAt(x, y, width, height, pixels);
        }

        public void DrawTextBox(string text, int witdh, int x, int y, string fontName = "Default", int letterSpacing = 0,
            bool wholeWords = false)
        {
            text = wholeWords ? FontChip.WordWrap(text, witdh) : FontChip.Split(text, witdh);

            DrawFont(text, x, y, fontName, letterSpacing);
        }

        public void DrawTextBoxToBuffer(string text, int witdh, int column, int row, string fontName = "Default",
            int letterSpacing = 0, bool wholeWords = false)
        {
            text = wholeWords ? FontChip.WordWrap(text, witdh) : FontChip.Split(text, witdh);

            DrawFontToBuffer(text, column, row, fontName, letterSpacing);
        }

        public void DrawBufferData(int[] pixelData, int x, int y, int width, int height)
        {
            chips.screenBufferChip.UpdatePixelDataAt(x, y, width, height, pixelData);
        }

        public void DrawPixelData(int[] pixelData, int x, int y, int width, int height, bool flipH, bool flipV,
            bool flipY, int layerOrder = 0, bool masked = false)
        {
            chips.displayChip.NewDrawCall(pixelData, x, y, width, height, flipH, flipV, flipY, layerOrder, masked);
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
            {
                if (pixelData[i] == oldID)
                    pixelData[i] = newID;
            }

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
            {
                for (var j = 0; j < colorTotal; j++)
                {
                    if (pixelData[i] == oldIDs[j])
                        pixelData[i] = newIDs[j];
                }
            }

            return pixelData;
        }

    }

}