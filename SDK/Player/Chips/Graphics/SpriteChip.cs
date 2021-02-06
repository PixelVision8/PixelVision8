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
using System;
using System.Linq;
using System.Text;

namespace PixelVision8.Player
{
    
    #region Sprite Chip Class
    
    /// <summary>
    ///     The <see cref="SpriteChip" /> represents a way to store and retrieve
    ///     sprite pixel data. Internally, the pixel data is stored in a
    ///     <see cref="TextureData" /> class allowing you to dynamically resize
    ///     sprites at run time.
    /// </summary>
    public class SpriteChip : AbstractChip, IDisplay
    {
        
        // TODO these are hard coded assuming the sprites are always 8x8
        private readonly int[] _emptySpriteData = Enumerable.Repeat(-1, 64).ToArray();

        private int _colorsPerSprite = 8;
        private int _pages;

        private ImageData _spriteMemory = new ImageData(128, 128);
        private static readonly StringBuilder tmpSB = new StringBuilder();

        /// <summary>
        ///     Internal <see cref="_cache" /> for faster lookup
        /// </summary>
        private string[] _cache;

        private const int PageHeight = 128;
        private const int PageWidth = 128;
        public bool Unique = false;

        /// <summary>
        ///     Sets the total number of sprite draw calls for the display.
        /// </summary>
        public int MaxSpriteCount { get; set; }

        public PixelData PixelData => _spriteMemory.PixelData;

        /// <summary>
        ///     Return's the <see cref="Sprite" /> Ram's internal
        ///     <see cref="texture" /> <see cref="SpriteWidth" />
        /// </summary>
        public int TextureWidth => _spriteMemory.Width;

        /// <summary>
        ///     Return's the <see cref="Sprite" /> Ram's internal
        ///     <see cref="texture" /> <see cref="SpriteWidth" />
        /// </summary>
        public int TextureHeight => _spriteMemory.Height;

        public const int DefaultSpriteSize = 8;
        /// <summary>
        ///     The virtual number of pages of sprite memory the SpriteChip
        ///     has. Each page contains a max number of sprites.
        /// </summary>
        public int Pages
        {
            get => _pages;
            set
            {
                if (_pages == value) return;

                _pages = MathHelper.Clamp(value, 1, 8);

                _spriteMemory.Resize(
                    (int) Math.Ceiling((float) PageWidth / DefaultSpriteSize) * DefaultSpriteSize,
                    (int) Math.Ceiling((float) PageHeight * Pages / DefaultSpriteSize) * DefaultSpriteSize
                );

                _cache = new string[_spriteMemory.TotalSprites];
            }
        }

        /// <summary>
        ///     Total number of sprites that can be stored in memory.
        /// </summary>
        public int TotalSprites => _spriteMemory.TotalSprites;

        /// <summary>
        ///     Total number of sprites that exist in memory.
        /// </summary>
        public int SpritesInMemory => _cache.Count(x => x != null);

        /// <summary>
        ///     Number of colors per sprite.
        /// </summary>
        public int ColorsPerSprite
        {
            get => _colorsPerSprite;
            set => _colorsPerSprite = MathHelper.Clamp(value, 1, 16);
        }

        public int Columns => _spriteMemory.Columns;
        
        /// <summary>
        ///     Tests to see if sprite <paramref name="data" /> is empty. This method
        ///     iterates over all the ints in the supplied <paramref name="data" />
        ///     array and looks for a value of -1. If all values are -1 then it
        ///     returns true.
        /// </summary>
        /// <param name="data">An array of ints</param>
        /// <returns>
        /// </returns>
        public static bool IsEmpty(int[] data)
        {
            var total = data.Length;
            for (var i = 0; i < total; i++)
                if (data[i] > -1)
                    return false;

            return true;
        }

        public bool IsEmptyAt(int index)
        {
            return string.IsNullOrEmpty(_cache[index]);
        }

        /// <summary>
        ///     Returns the next empty id in the <see cref="_cache" /> index. Used for
        ///     building tools to modify the <see cref="SpriteChip" /> like importers
        ///     or if you want to add sprite dynamically at runtime and need to know
        ///     the next open index.
        /// </summary>
        /// <returns>
        /// </returns>
        public int NextEmptyId()
        {
            return Array.FindIndex(_cache, string.IsNullOrEmpty);
        }

        /// <summary>
        ///     This configures the <see cref="SpriteChip" /> when it is activated.
        ///     It registers itself with the engine as the default SpriteChip, it
        ///     calls Clear() to clear any data and also sets the defaut size to 8 x
        ///     8.
        /// </summary>
        protected override void Configure()
        {
            Player.SpriteChip = this;
            
            Pages = 4;
            
        }

        public void Clear() => _spriteMemory.Clear();

        /// <summary>
        ///     Returns an array of ints that represent a sprite. Each
        ///     int should be mapped to a color
        ///     <paramref name="index" /> to display in the renderer.
        /// </summary>
        /// <param name="index">
        ///     Anint representing the location in memory of the
        ///     sprite.
        /// </param>
        /// <param name="pixelData"></param>
        /// <returns>
        /// </returns>
        public void ReadSpriteAt(int index, ref int[] pixelData)
        {
            // TODO check to see if the cache doesn't exist and return the empty sprite as well
            if (index == -1)
            {
                var size = DefaultSpriteSize * DefaultSpriteSize;

                if (pixelData.Length < size) Array.Resize(ref pixelData, size);

                Array.Copy(_emptySpriteData, pixelData, size);
            }
            else
            {
                // TODO need to remove the additional array copy from the Image to the Array
                Array.Copy(_spriteMemory.GetSpriteData(index, ColorsPerSprite), pixelData, pixelData.Length);
            }
        }

        /// <summary>
        ///     Updates the sprite data at a given position in the
        ///     <see cref="texture" /> data.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="pixels"></param>
        public void UpdateSpriteAt(int index, int[] pixels)
        {
            _spriteMemory.WriteSpriteData(index, pixels);

            CacheSprite(index, pixels);
        }

        /// <summary>
        /// </summary>
        /// <param name="data"></param>
        /// <returns>
        /// </returns>
        private string SpriteDataToString(int[] data)
        {
            tmpSB.Length = 0;
            var total = data.Length;

            for (var i = 0; i < total; i++) tmpSB.Append(data[i]);

            return tmpSB.ToString();
        }

        /// <summary>
        ///     This caches a sprite for easier look up and duplicate detection.
        ///     Each sprite is cached as a string.
        /// </summary>
        /// <param name="index">
        ///     Index where the sprite's cached value should be stored in the
        ///     <see cref="_cache" /> array.
        /// </param>
        /// <param name="data">
        ///     An array of ints that represents the sprite's color data.
        /// </param>
        private void CacheSprite(int index, int[] data)
        {
            if (index < 0 || index >= _cache.Length) return;

            // TODO need to test to see if the sprite is empty first (no need to cache)

            _cache[index] = SpriteDataToString(data);

            var totalPixels = DefaultSpriteSize * DefaultSpriteSize;

            var tmpPixels = new int[totalPixels];
            Array.Copy(data, tmpPixels, totalPixels);
            //            pixelDataCache[index] = tmpPixels;
        }

        /// <summary>
        ///     Finds a sprite by looking it up against the cache. Returns -1 if no
        ///     sprite is found. This is used for insuring duplicate sprites aren't
        ///     added to the TextureData.
        /// </summary>
        /// <param name="pixels">
        ///     An array of ints representing the sprite's color data.
        /// </param>
        /// <returns>
        /// </returns>
        public int FindSprite(int[] pixels, bool emptyCheck = false)
        {
            if (emptyCheck)
                if (IsEmpty(pixels))
                    return -1;

            var sprite = SpriteDataToString(pixels);

            return Array.IndexOf(_cache, sprite);
        }

        // TODO don't forget to add 'typeof(SpriteChip).FullName' to the Chip list in the GameRunner.Activate.cs class
    }
    
    #endregion

    #region Modify PixelVision
    
    public partial class PixelVision
    {
        public SpriteChip SpriteChip { get; set; }
    }
    
    #endregion
}
