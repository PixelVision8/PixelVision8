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

using Microsoft.Xna.Framework;
using PixelVision8.Engine.Utils;
using PixelVisionSDK.Engine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PixelVision8.Engine.Chips
{

    /// <summary>
    ///     The GameChip represents the foundation of a game class
    ///     with all the logic it needs to work correctly in the PixelVisionEngine.
    ///     The AbstractChip class manages configuring the game when created via the
    ///     chip life-cycle. The engine manages the game's state, the game's own life-cycle and
    ///     serialization/deserialization of the game's data.
    /// </summary>
    public class GameChip : GameChipLite
    {
        protected int _saveSlots;
        public Dictionary<string, string> savedData = new Dictionary<string, string>();
        // private int w;
        // private int h;
        protected int[] tilemapCachePixels;
        protected bool _tmpFlipH;
        protected bool _tmpFlipV;
        protected SpriteData _currentSpriteData;
        protected List<SpriteData> tmpSpritesData;
        protected int startX;
        protected int startY;
        private readonly string newline = "\n";
        protected SpriteCollection[] metaSprites;

        protected Rectangle metaSpriteMaxBounds = new Rectangle(0, 0, 64, 64);
        public int maxSize = 256;

        public bool lockSpecs = false;

        #region GameChip Properties

        /// <summary>
        ///     Used to limit the amount of data the game can save.
        /// </summary>
        public int SaveSlots
        {
            get => _saveSlots;
            set
            {
                value = MathHelper.Clamp(value, 2, 16);
                _saveSlots = value;

                // resize dictionary?
                for (var i = savedData.Count - 1; i >= 0; i--)
                {
                    var item = savedData.ElementAt(i);
                    if (i > value) savedData.Remove(item.Key);
                }
            }
        }

        #endregion

        #region Chip References

        protected MusicChip MusicChip => ((PixelVisionEngine)engine).MusicChip;

        protected int _TotalMetaSprites
        {
            // TODO do we need to save the previous values?
            set => Array.Resize(ref metaSprites, MathHelper.Clamp(value, 0, 96));
            get => metaSprites.Length;
        }

        public int TotalMetaSprites(int? total = null)
        {
            if (total.HasValue)
            {
                _TotalMetaSprites = total.Value;
            }

            return _TotalMetaSprites;
        }

        #endregion

        #region Lifecycle

        public override void Configure()
        {

            base.Configure();

            metaSprites = new SpriteCollection[96];
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

            Array.Clear(metaSprites, 0, metaSprites.Length);

            base.Reset();
        }

        #endregion


        #region File IO

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
            if (savedData.Count > SaveSlots) return;

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
        public string ReadSaveData(string key, string defaultValue = "undefined")
        {
            if (!savedData.ContainsKey(key)) WriteSaveData(key, defaultValue);

            return savedData[key];
        }

        #endregion

        #region Sound

        /// <summary>
        ///     This method allows your read and write raw sound data on the SoundChip.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="data"></param>
        public string Sound(int id, string data = null)
        {
            if (data != null) ((SfxrSoundChip)SoundChip).UpdateSound(id, data);

            return ((SfxrSoundChip)SoundChip).ReadSound(id).param;
        }

        /// <summary>
        ///     Plays a sing by it's ID. You can pass in a start position for it to being at a specific pattern ID in the song.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="loop"></param>
        /// <param name="startAt"></param>
        public void PlaySong(int id, bool loop = true, int startAt = 0)
        {
            MusicChip.PlaySong(id, loop, startAt);
        }

        /// <summary>
        ///     This helper method allows you to automatically load a set of patterns as a complete
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
        public void PlayPattern(int id, bool loop = true)
        {
            MusicChip.PlayPatterns(new[] { id }, loop);
        }

        /// <summary>
        ///     This helper method allows you to automatically load a set of patterns as a complete
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
        public void PlayPatterns(int[] loopIDs, bool loop = true)
        {
            MusicChip.PlayPatterns(loopIDs, loop);
        }

        /// <summary>
        ///     Returns a dictionary with information about the current state of the music chip.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, int> SongData()
        {
            return MusicChip.songData;
        }

        /// <summary>
        ///     Toggles the current playback state of the sequencer. If the song
        ///     is playing it will pause, if it is paused it will play.
        /// </summary>
        public void PauseSong()
        {
            MusicChip.PauseSong();
        }

        /// <summary>
        ///     Stops the sequencer.
        /// </summary>
        public void StopSong()
        {
            MusicChip.StopSong();
        }

        /// <summary>
        ///     Rewinds the sequencer to the beginning of the currently loaded song. You can define
        ///     the position in the loop and the loop where playback should begin. Calling this method
        ///     without any arguments will simply rewind the song to the beginning of the first loop.
        /// </summary>
        /// <param name="position">
        ///     Position in the loop to start playing at.
        /// </param>
        /// <param name="patternID">
        ///     The loop to rewind too.
        /// </param>
        public void RewindSong(int position = 0, int patternID = 0)
        {
            //TODO need to add in better support for rewinding a song across multiple loops
            MusicChip.RewindSong();
        }

        #endregion

        #region Tilemap Cache

        public void SaveTilemapCache()
        {
            tilemapCachePixels = PixelDataUtil.GetPixels(TilemapChip.PixelData);
        }

        public void RestoreTilemapCache()
        {
            if (tilemapCachePixels == null) return;

            PixelDataUtil.SetPixels(tilemapCachePixels, TilemapChip.PixelData);
        }

        #endregion


        

        #region Utils

        private StringBuilder _sb = new StringBuilder();

        // public int ReadFPS()
        // {
        //     return fps;
        // }
        //
        // public int ReadTotalSprites()
        // {
        //     return CurrentSprites;
        // }

        /// <summary>
        ///     This allows you to call the TextUtil's WordWrap helper to wrap a string of text to a specified character
        ///     width. Since the FontChip only knows how to render characters as sprites, this can be used to calculate
        ///     blocks of text then each line can be rendered with a DrawText() call.
        /// </summary>
        /// <param name="text">The string of text to wrap.</param>
        /// <param name="width">The width of characters to wrap each line of text.</param>
        /// <returns></returns>
        public string WordWrap(string text, int width)
        {

            if (text == null)
            {
                return "";
            }

            int pos, next;

            // Reset the string builder
            _sb.Clear();

            // Lucidity check
            if (width < 1) return text;

            // Parse each line of text
            for (pos = 0; pos < text.Length; pos = next)
            {
                // Find end of line
                var eol = text.IndexOf(newline, pos);
                if (eol == -1)
                    next = eol = text.Length;
                else
                    next = eol + newline.Length;

                // Copy this line of text, breaking into smaller lines as needed
                if (eol > pos)
                    do
                    {
                        var len = eol - pos;
                        if (len > width) len = BreakLine(text, pos, width);

                        _sb.Append(text, pos, len);
                        _sb.Append(newline);

                        // Trim whitespace following break
                        pos += len;
                        while (pos < eol && char.IsWhiteSpace(text[pos])) pos++;
                    } while (eol > pos);
                else
                    _sb.Append(newline); // Empty line
            }

            return _sb.ToString();
        }

        /// <summary>
        ///     Locates position to break the given line so as to avoid
        ///     breaking words.
        /// </summary>
        /// <param name="text">String that contains line of text</param>
        /// <param name="pos">Index where line of text starts</param>
        /// <param name="max">Maximum line length</param>
        /// <returns>The modified line length</returns>
        private int BreakLine(string text, int pos, int max)
        {
            // Find last whitespace in line
            var i = max;
            while (i >= 0 && !char.IsWhiteSpace(text[pos + i])) i--;

            // If no whitespace found, break at maximum length
            if (i < 0) return max;

            // Find start of whitespace
            while (i >= 0 && char.IsWhiteSpace(text[pos + i])) i--;

            // Return length of text before whitespace
            return i + 1;
        }

        /// <summary>
        ///     This calls the TextUtil's SplitLines() helper to convert text with line breaks (\n) into a collection of
        ///     lines. This can be used in conjunction with the WordWrap() helper to render large blocks of text line by
        ///     line with the DrawText() API.
        /// </summary>
        /// <param name="str">The string of text to split.</param>
        /// <returns>Returns an array of strings representing each line of text.</returns>
        public string[] SplitLines(string str)
        {
            var lines = str.Split(
                new[] { newline },
                StringSplitOptions.None
            );

            return lines;
        }

        

        public int[] BitArray(int value)
        {
            var bits = new BitArray(BitConverter.GetBytes(value));

            var intArray = new int[bits.Length];

            bits.CopyTo(intArray, 0);

            return intArray;
        }

        #endregion


        #region Experimental APIs

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
        public int TotalSprites(bool ignoreEmpty = false)
        {
            return ignoreEmpty ? SpriteChip.SpritesInMemory : SpriteChip.TotalSprites;
        }

        /// <summary>
        ///     This method returns the maximum number of sprites the Display Chip can render in a single frame. Use this
        ///     to better understand the limitations of the hardware your game is running on. This is a read only property
        ///     at runtime.
        /// </summary>
        /// <param name="total"></param>
        /// <returns>Returns an int representing the total number of sprites on the screen at once.</returns>
        public int MaxSpriteCount()
        {
            //            if (total.HasValue) spriteChip.maxSpriteCount = total.Value;

            return SpriteChip.maxSpriteCount;
        }

        public SpriteCollection MetaSprite(int id, SpriteCollection spriteCollection = null)
        {
            if (spriteCollection != null)
                metaSprites[id] = spriteCollection;
            else if (metaSprites[id] == null)
                metaSprites[id] =
                    new SpriteCollection(
                        "MetaSprite" + id.ToString().PadLeft(metaSprites.Length.ToString().Length, '0'))
                    {
                        SpriteWidth = SpriteSize().X,
                        SpriteHeight = SpriteSize().Y,
                        SpriteMax = TotalSprites(),
                        MaxBoundary = new Rectangle(metaSpriteMaxBounds.X, metaSpriteMaxBounds.Y,
                            metaSpriteMaxBounds.Width - SpriteSize().X,
                            metaSpriteMaxBounds.Height - SpriteSize().Y)
                    };

            return metaSprites[id];
        }


        public void DrawMetaSprite(int id, int x, int y, bool flipH = false, bool flipV = false,
            DrawMode drawMode = DrawMode.Sprite, int colorOffset = 0)
        {
            // This draw method doesn't support background or tile draw modes
            if (drawMode == DrawMode.TilemapCache || drawMode == DrawMode.Tile) return;

            // Get the sprite data for the meta sprite
            tmpSpritesData = metaSprites[id].Sprites;
            total = tmpSpritesData.Count;

            // Loop through each of the sprites
            for (var i = 0; i < total; i++)
            {
                _currentSpriteData = tmpSpritesData[i];

                if (!SpriteChip.IsEmptyAt(_currentSpriteData.Id))
                {
                    // Get sprite values
                    startX = _currentSpriteData.X;
                    startY = _currentSpriteData.Y;
                    _tmpFlipH = _currentSpriteData.FlipH;
                    _tmpFlipV = _currentSpriteData.FlipV;

                    // Get the width and height of the meta sprite's bounds
                    _width = metaSprites[id].Bounds.Width;
                    height = metaSprites[id].Bounds.Height;

                    if (flipH)
                    {
                        startX = _width - startX - SpriteSize().X;
                        _tmpFlipH = !_tmpFlipH;
                    }

                    if (flipV)
                    {
                        startY = height - startY - SpriteSize().Y;
                        _tmpFlipV = !_tmpFlipV;
                    }

                    startX += x;
                    startY += y;

                    DrawSprite(
                        _currentSpriteData.Id,
                        startX,
                        startY,
                        _tmpFlipH,
                        _tmpFlipV,
                        drawMode,
                        _currentSpriteData.ColorOffset + colorOffset
                    );
                }
            }
        }

        /// <summary>
        ///     Reads the meta data that was passed into the game when it was loaded.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public string ReadMetadata(string key, string defaultValue = "undefined")
        {
            return ((PixelVisionEngine)engine).GetMetadata(key, defaultValue);
        }

        /// <summary>
        ///     Writes meta data back into the game which can be read if the game reloads.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void WriteMetadata(string key, string value)
        {
            ((PixelVisionEngine)engine).SetMetadata(key, value);
        }

        public Dictionary<string, string> ReadAllMetadata()
        {
            var tmpMetadata = new Dictionary<string, string>();

            ((PixelVisionEngine)engine).ReadAllMetadata(tmpMetadata);

            return tmpMetadata;
        }

        #endregion

        #region Factories

        /// <summary>
        ///     A Rect is a Pixel Vision 8 primitive used for defining the bounds of an object on the display. It
        ///     contains an x, y, width and height property. The Rect class also has some additional methods to aid with
        ///     collision detection such as Intersect(rect, rect), IntersectsWidth(rect) and Contains(x,y).
        /// </summary>
        /// <param name="x">The x position of the rect as an int.</param>
        /// <param name="y">The y position of the rect as an int.</param>
        /// <param name="w">The width value of the rect as an int.</param>
        /// <param name="h">The height value of the rect as an int.</param>
        /// <returns>Returns a new instance of a Rect to be used as a Lua object.</returns>
        public Rectangle NewRect(int x = 0, int y = 0, int w = 0, int h = 0)
        {
            return new Rectangle(x, y, w, h);
        }

        /// <summary>
        ///     A Vector is a Pixel Vision 8 primitive used for defining a position on the display as an x,y value.
        /// </summary>
        /// <param name="x">The x position of the Vector as an int.</param>
        /// <param name="y">The y position of the Vector as an int.</param>
        /// <returns>Returns a new instance of a Vector to be used as a Lua object.</returns>
        public Point NewPoint(int x = 0, int y = 0)
        {
            return new Point(x, y);
        }

        public Canvas NewCanvas(int width, int height)
        {
            return new Canvas(width, height, this);
        }

        public SpriteData NewSpriteData(int id, int x = 0, int y = 0, bool flipH = false, bool flipV = false,
            int colorOffset = 0)
        {
            return new SpriteData(id, x, y, flipH, flipV, colorOffset);
        }

        public SpriteCollection NewSpriteCollection(string name, SpriteData[] sprites = null)
        {
            return new SpriteCollection(name, sprites);
        }

        public SpriteCollection NewMetaSprite(int id, string name, int[] spriteIDs, int width, int colorOffset = 0)
        {
            var collection = NewSpriteCollection(name);

            for (int i = 0; i < spriteIDs.Length; i++)
            {

                var pos = CalculatePosition(i, width);

                collection.AddSprite(spriteIDs[i], pos.X * spriteSize.X, pos.Y * spriteSize.Y, false, false, colorOffset);
            }

            // TODO need to figure out how to do this better where meta sprites 


            return MetaSprite(id, collection);
        }

        #endregion

    }


}