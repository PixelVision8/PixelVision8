﻿//   
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
using Microsoft.Xna.Framework;
using PixelVision8.Engine.Utils;

namespace PixelVision8.Engine.Chips
{
    /// <summary>
    ///     The tile map chip represents a grid of sprites used to populate the background
    ///     layer of the game. These sprites are fixed and laid out in column and row
    ///     positions making it easier to create grids of tiles. The TileMapChip also
    ///     manages flag values per tile for use in collision detection. Finally, the TileMapChip
    ///     also stores a color offset per tile to simulate palette shifting.
    /// </summary>
    public class TilemapChip : AbstractChip, IDisplay
    {
        private SpriteChip SpriteChip;
        
        public bool autoImport;

        public TileData[] tiles;
        private int _columns;
        private int _rows;
        private int _i;
        private TileData _tile;
        private Point _pos;

        /// <summary>
        ///     The total tiles in the chip.
        /// </summary>
        public int total => columns * rows;
        
        // public int width => viewPort.Width;
        //
        // public int height => viewPort.Height;

        private PixelData _tilemapCache = new PixelData();

        public PixelData PixelData
        {
           get {
                if (invalid) 
                    RebuildCache();
                return _tilemapCache;
            }
        }
        
        private Rectangle _tileSize;
        private PixelData tmpPixelData;
        
        /// <summary>
        ///     The width of the tile map by tiles.
        /// </summary>
        public int columns
        {
            get => _columns;
            protected set => _columns = MathUtil.Clamp(value, 0, 255);
        }

        /// <summary>
        ///     The height of the tile map in tiles.
        /// </summary>
        public int rows
        {
            get => _rows;
            protected set => _rows = MathUtil.Clamp(value, 0, 255);
        }

        public bool invalid { get; protected set; }

        public void Invalidate()
        {
            invalid = true;
        }

        /// <summary>
        ///     This goes flags all tiles with a given sprite ID that it has changed and should be redrawn
        /// </summary>
        /// <param name="id"></param>
        public void InvalidateTileID(int id)
        {
            //            GetTile(id).invalid = true;

            for (var i = 0; i < total; i++)
            {
                var tile = tiles[i];
                if (tile.spriteID == id) tile.Invalidate();
            }

            Invalidate();
        }


        public void ResetValidation()
        {
            invalid = false;
            for (var i = 0; i < total; i++) tiles[i].ResetValidation();
        }

        public TileData GetTile(int column, int row)
        {
            return tiles[MathUtil.CalculateIndex(column, row, columns)];
        }

        /// <summary>
        ///     Resizes the tile map. When a tile map is resized, all of the sprite,
        ///     palette and flag data is destroyed.
        /// </summary>
        /// <param name="column">
        ///     The column position of the tile. 0 is the left of the tile map.
        /// </param>
        /// <param name="row">
        ///     The row position of the tile. 0 is the top of the tile map.
        /// </param>
        /// <param name="clear">
        ///     A optional value to perform a clear on the resized spriteID,
        ///     paletteID and <see cref="flags" /> arrays to return their values to
        ///     -1. This is set to true by default.
        /// </param>
        public void Resize(int newColumns, int newRows, bool clear = true)
        {

            // Make sure we keep the value in range
            columns = MathUtil.Clamp(newColumns, 1, 256);
            rows = MathUtil.Clamp(newRows, 1, 256);
            
            // Resize the tile array
            Array.Resize(ref tiles, columns * rows);

            // Loop through all of the tiles
            for (_i = 0; _i < tiles.Length; _i++)
            {
                
                // Create a new tile if it doesn't exist
                if (tiles[_i] == null)
                {
                    tiles[_i] = new TileData(_i);
                }
                else
                {
                    tiles[_i].Index = _i;
                    
                    if (clear)
                    {
                        tiles[_i].spriteID = -1;
                        tiles[_i].flag = -1;
                        tiles[_i].colorOffset = 0;
                    }
                }
            }
            
            _tilemapCache.Resize(this.columns * SpriteChip.width, this.rows * SpriteChip.height);
            
            Invalidate();
        }

        /// <summary>
        ///     This clears all the tile map data. The spriteID and flag arrays are
        ///     set to -1 as their default value and the palette array is set to 0.
        /// </summary>
        public void Clear()
        {
            for (var i = 0; i < total; i++) tiles[i].Clear();

            Invalidate();
        }

        /// <summary>
        ///     Configured the TileMapChip. This method sets the
        ///     <see cref="TilemapChip" /> as the default tile map for the engine. It
        ///     also resizes the tile map to its default size of 32 x 30 which is a
        ///     resolution of 256 x 240.
        /// </summary>
        public override void Configure()
        {
            //ppu.tileMap = this;
            engine.TilemapChip = this;

            // Get a reference to the Sprite Chip
            SpriteChip = engine.SpriteChip;
            
            _tileSize = new Rectangle(0, 0, SpriteChip.width, SpriteChip.height);
            
            tmpPixelData = new PixelData(_tileSize.Width, _tileSize.Height);
            
            // Resize to default nes resolution
            Resize(32, 30);
        }

        public override void Deactivate()
        {
            base.Deactivate();
            engine.TilemapChip = null;
        }

        // public PixelData texture
        // {
        //     get
        //     {
        //         if (invalid) 
        //             RebuildCache();
        //         return _tilemapCache;
        //     }
        // }

        protected void RebuildCache()
        {
            
            // Loop through all of the tiles in the tilemap
            for (_i = 0; _i < total; _i++)
            {
                _tile = tiles[_i];

                if (_tile.invalid)
                {
                    // Get the sprite id
                    _pos = MathUtil.CalculatePosition(_i, columns );
                    
                    SpriteChip.ReadSpriteAt(_tile.spriteID, ref tmpPixelData.Pixels);

                    if (_tile.flipH || _tile.flipV)
                        SpriteChipUtil.FlipSpriteData(ref tmpPixelData.Pixels, _tileSize.Width, _tileSize.Height, _tile.flipH,
                            _tile.flipH);

                    // Draw the pixel data into the cachedTilemap
                    PixelDataUtil.MergePixels(tmpPixelData, 0, 0, _tileSize.Width, _tileSize.Height, _tilemapCache, _pos.X * _tileSize.Width, _pos.Y * _tileSize.Height, false, false, _tile.colorOffset, false);

                }
            }

            // Reset the invalidation state
            ResetValidation();
        }
        
    }
}