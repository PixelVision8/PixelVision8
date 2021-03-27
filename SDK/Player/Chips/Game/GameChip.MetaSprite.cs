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

using System;

namespace PixelVision8.Player
{

    public partial class Utilities
    {
        public static void FlipPixelData(ref int[] pixelData, int sWidth, int sHeight, bool flipH = false,
            bool flipV = false)
        {
            var total = pixelData.Length;
            
            var pixels = new int[total];

            // if (pixels.Length < total) Array.Resize(ref pixels, total);

            Array.Copy(pixelData, pixels, total);

            for (var ix = 0; ix < sWidth; ix++)
            for (var iy = 0; iy < sHeight; iy++)
            {
                var newx = ix;
                var newY = iy;
                if (flipH) newx = sWidth - 1 - ix;

                if (flipV) newY = sHeight - 1 - iy;

                pixelData[ix + iy * sWidth] = pixels[newx + newY * sWidth];
            }
        }
    }
    
    /// <summary>
    ///     The GameChip represents the foundation of a game class
    ///     with all the logic it needs to work correctly in the PixelVisionEngine.
    ///     The AbstractChip class manages configuring the game when created via the
    ///     chip life-cycle. The engine manages the game's state, the game's own life-cycle and
    ///     serialization/deserialization of the game's data.
    /// </summary>
    public partial class GameChip
    {
        
        protected SpriteData _currentSpriteData;
        protected SpriteCollection[] metaSprites = new SpriteCollection[0];
        protected int[] tmpIDs = new int[0];
        public int maxSize = 256;

        // private int _totalMetaSprites => metaSprites.Length;

        public int TotalMetaSprites(int? total = null)
        {
            if (total.HasValue)
            {
                // Console.WriteLine("Change meta sprite total");
                Array.Resize(ref metaSprites, Utilities.Clamp(total.Value, 0, 256));
                for (int i = 0; i < total.Value; i++)
                {
                    if (metaSprites[i] == null)
                        metaSprites[i] = new SpriteCollection("EmptyMetaSprite");
                }
            }

            return metaSprites.Length;
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

            return SpriteChip.MaxSpriteCount;
        }

        public SpriteCollection MetaSprite(string name, SpriteCollection spriteCollection = null)
        {
            return MetaSprite(FindMetaSpriteId(name), spriteCollection);
        }

        public SpriteCollection MetaSprite(int id, SpriteCollection spriteCollection = null)
        {
            if (id < 0 || id > metaSprites.Length)
                return null;

            if (spriteCollection != null)
                metaSprites[id] = spriteCollection;
            else if (metaSprites[id] == null)
            {
                Console.WriteLine("New Meta Sprite");
                metaSprites[id] =
                    new SpriteCollection(
                        "MetaSprite" + id.ToString().PadLeft(metaSprites.Length.ToString().Length, '0'))
                    {
                        SpriteWidth = SpriteSize().X,
                        SpriteHeight = SpriteSize().Y,
                        SpriteMax = TotalSprites(),
                        // MaxBoundary = new Rectangle(metaSpriteMaxBounds.X, metaSpriteMaxBounds.Y,
                        //     metaSpriteMaxBounds.Width - SpriteSize().X,
                        //     metaSpriteMaxBounds.Height - SpriteSize().Y)
                    };
            }
                

            return metaSprites[id];
        }

        protected int FindMetaSpriteId(string name)
        {
            var total = metaSprites.Length;
            
            // Loop through all of the meta sprites and find a name that matches
            for (int i = 0; i < total; i++)
            {
                if (metaSprites[i].Name == name)
                    return i;
            }

            // If no match is found
            return -1;
        }

        public void DrawMetaSprite(string name, int x, int y, bool flipH = false, bool flipV = false,
            DrawMode drawMode = DrawMode.Sprite, int colorOffset = 0)
        {
            DrawMetaSprite(FindMetaSpriteId(name), x, y, flipH, flipV, drawMode, colorOffset);
        }

        public void DrawMetaSprite(int id, int x, int y, bool flipH = false, bool flipV = false,
            DrawMode drawMode = DrawMode.Sprite, int colorOffset = 0)
        {
            // This draw method doesn't support background or tile draw modes
            if (id == -1) return;

            DrawMetaSprite(metaSprites[id], x, y, flipH, flipV, drawMode, colorOffset);

        }

        public void DrawMetaSprite(SpriteCollection spriteCollection, int x, int y, bool flipH = false, bool flipV = false,
            DrawMode drawMode = DrawMode.Sprite, int colorOffset = 0)
        {

            // Get the sprite data for the meta sprite
            var tmpSpritesData = spriteCollection.Sprites;
            var total = tmpSpritesData.Count;
            var id = 0;

            // // When rendering in Tile Mode, switch to grid layout
            // if (drawMode == DrawMode.Tile)
            // {
            //     // TODO added this so C# code isn't corrupted, need to check performance impact
            //     if (tmpIDs.Length != total) Array.Resize(ref tmpIDs, total);

            //     var i = 0;

            //     for (i = 0; i < total; i++)
            //     {
            //         tmpIDs[i] = tmpSpritesData[i].Id;
            //     }

            //     var width = (int)Math.Ceiling((double)spriteCollection.Bounds.Width / SpriteChip.DefaultSpriteSize);

            //     var height = (int)Math.Ceiling((double)total / width);

            //     if (flipH || flipV) Utilities.FlipPixelData(ref tmpIDs, width, height, flipH, flipV);

            //     // TODO need to offset the bounds based on the scroll position before testing against it
            //     for (i = 0; i < total; i++)
            //     {
            //         // Set the sprite id
            //         id = tmpIDs[i];

            //         // TODO should also test that the sprite is not greater than the total sprites (from a cached value)
            //         // Test to see if the sprite is within range
            //         if (id > -1)
            //         {
            //             var pos = CalculatePosition(i, width);

            //             DrawSprite(
            //                 id,
            //                 pos.X + x,
            //                 pos.Y + y,
            //                 flipH,
            //                 flipV,
            //                 drawMode,
            //                 _currentSpriteData.ColorOffset + colorOffset);
            //         }
            //     }
            // }
            // else
            // {
                
                int startX, startY;
                bool tmpFlipH, tmpFlipV;

                var spriteSize = SpriteChip.DefaultSpriteSize;
                var width = spriteCollection.Bounds.Width;
                var height = spriteCollection.Bounds.Height;
                
                // Loop through each of the sprites
                for (var i = 0; i < total; i++)
                {
                    _currentSpriteData = tmpSpritesData[i];

                    if (!SpriteChip.IsEmptyAt(_currentSpriteData.Id))
                    {
                        // Get sprite values
                        startX = _currentSpriteData.X;
                        startY = _currentSpriteData.Y;
                        tmpFlipH = _currentSpriteData.FlipH;
                        tmpFlipV = _currentSpriteData.FlipV;

                        if (flipH)
                        {
                            startX = width - startX - spriteSize;
                            tmpFlipH = !tmpFlipH;
                        }

                        if (flipV)
                        {
                            startY = height - startY - spriteSize;
                            tmpFlipV = !tmpFlipV;
                        }

                        if (drawMode == DrawMode.Tile){
                        
                            // Get sprite values
                            startX = (int)Math.Ceiling((double)startX/ spriteSize);
                            startY = (int)Math.Ceiling((double)startY / spriteSize);

                        }

                        startX += x;
                        startY += y;

                        DrawSprite(
                            _currentSpriteData.Id,
                            startX,
                            startY,
                            tmpFlipH,
                            tmpFlipV,
                            drawMode,
                            _currentSpriteData.ColorOffset + colorOffset
                        );
                    }
                }
            // }
        }
    }
}