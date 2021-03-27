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
    
    #region Tilemap Chip Class

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
        private int[] pixels;
        
        /// <summary>
        ///     The total tiles in the chip.
        /// </summary>
        public int Total => Columns * Rows;

        private readonly PixelData _tilemapCache = new PixelData();

        public PixelData PixelData
        {
            get
            {
                if (_invalid)
                    RebuildCache();
                return _tilemapCache;
            }
        }

        private Rectangle _tileSize;
        private PixelData _tmpPixelData;

        /// <summary>
        ///     The width of the tile map by tiles.
        /// </summary>
        public int Columns
        {
            get => _columns;
            private set => _columns = Utilities.Clamp(value, 0, 255);
        }

        /// <summary>
        ///     The height of the tile map in tiles.
        /// </summary>
        public int Rows
        {
            get => _rows;
            private set => _rows = Utilities.Clamp(value, 0, 255);
        }

        private bool _invalid;

        public void Invalidate()
        {
            _invalid = true;
        }

        /// <summary>
        ///     This goes flags all tiles with a given sprite ID that it has changed and should be redrawn
        /// </summary>
        /// <param name="id"></param>
        public void InvalidateTileId(int id)
        {
            //            GetTile(id).invalid = true;

            for (var i = 0; i < Total; i++)
            {
                var tile = tiles[i];
                if (tile.SpriteId == id) tile.Invalidate();
            }

            Invalidate();
        }


        private void ResetValidation()
        {
            _invalid = false;
            for (var i = 0; i < Total; i++) tiles[i].ResetValidation();
        }

        public TileData GetTile(int column, int row)
        {
            return tiles[Utilities.CalculateIndex(column, row, Columns)];
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
            Columns = Utilities.Clamp(newColumns, 1, 256);
            Rows = Utilities.Clamp(newRows, 1, 256);

            // Resize the tile array
            Array.Resize(ref tiles, Columns * Rows);

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
                        tiles[_i].SpriteId = -1;
                        tiles[_i].Flag = -1;
                        tiles[_i].ColorOffset = 0;
                    }
                }
            }

            _tilemapCache.Resize(this.Columns * 8, this.Rows * 8);

            Invalidate();
        }

        /// <summary>
        ///     This clears all the tile map data. The spriteID and flag arrays are
        ///     set to -1 as their default value and the palette array is set to 0.
        /// </summary>
        public void Clear()
        {
            for (var i = 0; i < Total; i++) tiles[i].Clear();

            Invalidate();
        }

        /// <summary>
        ///     Configured the TileMapChip. This method sets the
        ///     <see cref="TilemapChip" /> as the default tile map for the engine. It
        ///     also resizes the tile map to its default size of 32 x 30 which is a
        ///     resolution of 256 x 240.
        /// </summary>
        protected override void Configure()
        {
            //ppu.tileMap = this;
            Player.TilemapChip = this;

            // Get a reference to the Sprite Chip
            SpriteChip = Player.SpriteChip;

            _tileSize = new Rectangle(0, 0, SpriteChip.DefaultSpriteSize, SpriteChip.DefaultSpriteSize);

            _tmpPixelData = new PixelData(_tileSize.Width, _tileSize.Height);
            
            pixels = new int[SpriteChip.DefaultSpriteSize * SpriteChip.DefaultSpriteSize];
            
            // Resize to default nes resolution
            Resize(32, 30);
        }

        private void RebuildCache()
        {
            
            // Loop through all of the tiles in the tilemap
            for (_i = 0; _i < Total; _i++)
            {
                _tile = tiles[_i];

                if (_tile.Invalid)
                {
                    // Get the sprite id
                    _pos = Utilities.CalculatePosition(_i, Columns);
                    
                    SpriteChip.ReadSpriteAt(_tile.SpriteId, ref pixels);

                    _tmpPixelData.SetPixels(pixels, _tileSize.Width, _tileSize.Height);
                    
                    // Draw the pixel data into the cachedTilemap
                    Utilities.MergePixels(_tmpPixelData, 0, 0, _tileSize.Width, _tileSize.Height, _tilemapCache,
                        _pos.X * _tileSize.Width, _pos.Y * _tileSize.Height, _tile.FlipH,
                        _tile.FlipV, _tile.ColorOffset, false);
                }
            }

            // Reset the invalidation state
            ResetValidation();
        }

        // TODO don't forget to add 'typeof(TilemapChip).FullName' to the Chip list in the GameRunner.Activate.cs class
    }
    
    
    #endregion
    
    #region Modify PixelVision
    
    public partial class PixelVision
    {
        public TilemapChip TilemapChip { get; set; }
    }

    #endregion
    
}