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

namespace PixelVision8.Engine.Chips
{
    /// <summary>
    ///     The tile map chip represents a grid of sprites used to populate the background
    ///     layer of the game. These sprites are fixed and laid out in column and row
    ///     positions making it easier to create grids of tiles. The TileMapChip also
    ///     manages flag values per tile for use in collision detection. Finally, the TileMapChip
    ///     also stores a color offset per tile to simulate palette shifting.
    /// </summary>
    public class TilemapChip : AbstractChip
    {
        public bool autoImport;

        public TileData[] tiles;

        /// <summary>
        ///     Total number of collision flags the chip will support.
        ///     The default value is 16.
        ///     The default value is 16.
        /// </summary>
        public int totalFlags = 16;

        /// <summary>
        ///     The total tiles in the chip.
        /// </summary>
        public int total => columns * rows;

        /// <summary>
        ///     The width of the tile map by tiles.
        /// </summary>
        public int columns { get; protected set; }

        /// <summary>
        ///     The height of the tile map in tiles.
        /// </summary>
        public int rows { get; protected set; }

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

        public void InvalidateAll()
        {
            //            cachedTileMap.Clear();

            for (var i = 0; i < total; i++) tiles[i].Invalidate();
            //                layers[(int) Layer.Invalid][i] = -1;

            Invalidate();
        }

        public void ResetValidation()
        {
            invalid = false;
            for (var i = 0; i < total; i++) tiles[i].ResetValidation();
        }

        public TileData GetTile(int column, int row)
        {
            return tiles[CalculateTileIndex(column, row)];
        }

        public int CalculateTileIndex(int column, int row)
        {
            // Note: + size and the second modulo operation are required to get wrapped values between 0 and +size
            var size = rows;
            row = (row % size + size) % size;
            size = columns;
            column = (column % size + size) % size;
            // size is still == columns from the previous operation - let's reuse the local

            return column + size * row;
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
        public void Resize(int columns, int rows, bool clear = true)
        {
            // Create a new array for each tile's data
            var newTiles = new TileData[columns * rows];

            var row = 0;

            for (var i = 0; i < newTiles.Length; i++)
            {
                var c = i % columns;
                var r = row;

                if (c < this.columns && r < this.rows && clear == false)
                {
                    newTiles[i] = GetTile(c, r);
                    newTiles[i].Index = i;
                }
                else
                {
                    newTiles[i] = new TileData(i);
                }

                if (c == columns - 1) row++;
            }

            

            // Save the new tilemap data
            this.columns = columns;
            this.rows = rows;
            tiles = newTiles;

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

            // Resize to default nes resolution
            Resize(32, 30);
        }

        public override void Deactivate()
        {
            base.Deactivate();
            engine.TilemapChip = null;
        }
    }
}