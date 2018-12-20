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

using System;
using PixelVisionSDK.Utils;

namespace PixelVisionSDK.Chips
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
        public enum Layer
        {
            Sprites,
            Colors,
            Flags,
            FlipH,
            FlipV,
            Invalid
        }

        protected readonly int totalLayers = Enum.GetNames(typeof(Layer)).Length;

        public int[][] layers;

        protected int tmpIndex;

        /// <summary>
        ///     Total number of collision flags the chip will support.
        ///     The default value is 16.
        ///     The default value is 16.
        /// </summary>
        public int totalFlags = 16;

        public bool autoImport;

        /// <summary>
        ///     The total tiles in the chip.
        /// </summary>
        public int total
        {
            get { return columns * rows; }
        }

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
            var tileLayer = layers[(int) Layer.Sprites];
            var invalidLayer = layers[(int) Layer.Invalid];

            var total = tileLayer.Length;
            for (var i = 0; i < total; i++)
                if (tileLayer[i] == id)
                    invalidLayer[i] = -1;

            Invalidate();
        }

        public void InvalidateAll()
        {
//            cachedTileMap.Clear();

            for (int i = 0; i < total; i++)
            {
                layers[(int) Layer.Invalid][i] = -1;
            }
            
            Invalidate();
        }

        public void ResetValidation()
        {
            invalid = false;
            Array.Clear(layers[(int) Layer.Invalid], 0, total);

        }

        public void Invalidate(int index)
        {
            // Get the invalid layer
//            var layer = layers[(int) Layer.Invalid];
//
//            // Make sure the index is within range
//            if (index >= layer.Length || index < 0)
//                return;

            // change the tile flag to -1 so we know it needs to be redrawn
            layers[(int) Layer.Invalid][index] = -1;

            // Tell the map there was a change
            Invalidate();
        }

        /// <summary>
        ///     Returns the value in a given Tilemap layer. Accepts a layer enum and automatically converts is to a layer id.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="column"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        protected int ReadDataAt(Layer name, int column, int row)
        {
            return ReadDataAt((int) name, column, row);
        }

        protected int ReadDataAt(int id, int column, int row)
        {
            // Note: + size and the second modulo operation are required to get wrapped values between 0 and +size
            int size = rows;
            row = ((row % size) + size) % size;
            size = columns;
            column = ((column % size) + size) % size;
            // size is still == columns from the previous operation - let's reuse the local

            var index = column + size * row;
            
//            var index = column + row * columns;

//            if (index >= layers[id].Length || index < 0)
//                return -1;
            return layers[id][index];
            
        }

        protected void UpdateDataAt(Layer name, int column, int row, int value)
        {
            UpdateDataAt((int) name, column, row, value);
        }

        protected void UpdateDataAt(int id, int column, int row, int value)
        {
            // Note: + size and the second modulo operation are required to get wrapped values between 0 and +size
            int size = rows;
            row = ((row % size) + size) % size;
            size = columns;
            column = ((column % size) + size) % size;
            // size is still == columns from the previous operation - let's reuse the local

            // Calculate the index
            var index = column + size * row;
            
//            column = (int) (column - Math.Floor(column / (float) columns) * columns);
            
//            if (column >= columns)
//            {
//                int max = columns;
//                column = (int) (column - Math.Floor(column / (float) max) * max);
//            }

//            var index = column + row * columns;

//            if (index > -1 && index < layers[id].Length)
//            {
            
            // If the layer value is not equil to the new value, update it and invalidate it
//            if (layers[id][index] != value)
//            {
                layers[id][index] = value;
                Invalidate(index);
//            }
                
//            }
        }

        /// <summary>
        ///     Updates a tile's data in the tile map. A tile consists of 3 values,
        ///     the sprite id, the palette id and the flag. Each value is an int.
        /// </summary>
        /// <param name="spriteID">The id of the sprite to use.</param>
        /// <param name="column">
        ///     The column position of the tile. 0 is the left of the tile map.
        /// </param>
        /// <param name="row">
        ///     The row position of the tile. 0 is the top of the tile map.
        /// </param>
        /// <param name="flag">The flag value used for collision.</param>
        /// <param name="paletteID">
        ///     The color offset to use when rendering the sprite.
        /// </param>
        /// <param name="flipH"></param>
        /// <param name="flipV"></param>
        public void UpdateTileAt(int spriteID, int column, int row, int flag = -1, int paletteID = 0,
            bool flipH = false, bool flipV = false)
        {
            UpdateDataAt(Layer.Sprites, column, row, spriteID);
            UpdateDataAt(Layer.Colors, column, row, paletteID);
            UpdateDataAt(Layer.Flags, column, row, flag);
//            UpdateDataAt(Layer.FlipH, column, row, Convert.ToInt32(flipH));
//            UpdateDataAt(Layer.FlipV, column, row, Convert.ToInt32(flipV));
        }

        /// <summary>
        ///     Returns the value of a sprite at a given position in the tile map.
        /// </summary>
        /// <param name="column">
        ///     The column position of the tile. 0 is the left of the tile map.
        /// </param>
        /// <param name="row">
        ///     The row position of the tile. 0 is the top of the tile map.
        /// </param>
        /// <returns>
        ///     Returns anint for the sprite id set at the
        ///     specified position. If the tile is empty it will return -1.
        /// </returns>
        public int ReadSpriteAt(int column, int row)
        {
            return ReadDataAt(Layer.Sprites, column, row);
        }

        /// <summary>
        ///     Updates a sprite id for a tile at a given position. Set this value
        ///     to -1 if you want it to be empty. Empty tiles will automatically be
        ///     filled in with the engine's transparent color when rendered to the
        ///     ScreenBufferChip.
        /// </summary>
        /// <param name="column">
        ///     The column position of the tile. 0 is the left of the tile map.
        /// </param>
        /// <param name="row">
        ///     The row position of the tile. 0 is the top of the tile map.
        /// </param>
        /// <param name="spriteID">
        ///     The index of the sprite to use for the tile.
        /// </param>
        public void UpdateSpriteAt(int column, int row, int spriteID)
        {
            UpdateDataAt(Layer.Sprites, column, row, spriteID);
        }

        /// <summary>
        ///     Reads the palette offset at a give position in the tile map. When
        ///     reading the pixel data of a sprite from the tile map, the palette
        ///     value will be added to all of the pixel data ints to shift the
        ///     colors of the tile.
        /// </summary>
        /// <param name="column">
        ///     The column position of the tile. 0 is the left of the tile map.
        /// </param>
        /// <param name="row">
        ///     The row position of the tile. 0 is the top of the tile map.
        /// </param>
        /// <returns>
        ///     Returns the color int offset.
        /// </returns>
        public int ReadTileColorAt(int column, int row)
        {
            return ReadDataAt(Layer.Colors, column, row);
        }

        /// <summary>
        ///     Used to offset the pixel data of a tile sprite. Set the value which
        ///     is added to all the ints in a requested tile's data when being
        ///     rendered to the ScreenBufferChip.
        /// </summary>
        /// <param name="column">
        ///     The column position of the tile. 0 is the left of the tile map.
        /// </param>
        /// <param name="row">
        ///     The row position of the tile. 0 is the top of the tile map.
        /// </param>
        /// <param name="paletteID">
        ///     A color int offset.
        /// </param>
        public void UpdateTileColorAt(int column, int row, int paletteID)
        {
            int width = columns;
            tmpIndex = column + row * width;

            UpdateDataAt(Layer.Colors, column, row, paletteID);

            
        }

        /// <summary>
        ///     Returns the flag value at a specific position. The flag can be used
        ///     for collision detection on the tile map.
        /// </summary>
        /// <param name="column">
        ///     The column position of the tile. 0 is the left of the tile map.
        /// </param>
        /// <param name="row">
        ///     The row position of the tile. 0 is the top of the tile map.
        /// </param>
        /// <returns>
        ///     Returns an int for the flag value.
        /// </returns>
        public int ReadFlagAt(int column, int row)
        {
            return ReadDataAt(Layer.Flags, column, row);
        }

        /// <summary>
        ///     This method updates the <paramref name="flag" /> value at a given
        ///     position. -1 means there is no <paramref name="flag" /> and the
        ///     maximum value is capped by the totalFlag field.
        /// </summary>
        /// <param name="column">
        ///     The column position of the tile. 0 is the left of the tile map.
        /// </param>
        /// <param name="row">
        ///     The row position of the tile. 0 is the top of the tile map.
        /// </param>
        /// <param name="flag">The value of the flag as an int.</param>
        public void UpdateFlagAt(int column, int row, int flag)
        {
            UpdateDataAt(Layer.Flags, column, row, flag);
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
            this.columns = columns;
            this.rows = rows;

            // Get the total number of layers we are working with
            //var totalLayers = Enum.GetNames(typeof(Layer)).Length;

            // Make sure we have the layers we need
            if (layers == null)
                layers = new int[totalLayers][];

            var totalTiles = total;

            // Loop through each data layer and resize it
            for (var i = 0; i < totalLayers; i++)
                if (layers[i] == null)
                    layers[i] = new int[totalTiles]; // (columns, rows);
                else
                    Array.Resize(ref layers[i], totalTiles);

//            cachedTileMap.Resize(realWidth, realHeight);

            // Clear flags
            var flagLayer = layers[(int) Layer.Flags];
            for (var i = 0; i < flagLayer.Length; i++)
                flagLayer[i] = -1;

            if (clear)
                Clear();

            Invalidate();
        }

        /// <summary>
        ///     This clears all the tile map data. The spriteID and flag arrays are
        ///     set to -1 as their default value and the palette array is set to 0.
        /// </summary>
        public void Clear()
        {
            // Get the total number of layers we are working with
            var totalLayers = Enum.GetNames(typeof(Layer)).Length;

            var totalTiles = total;
            for (var i = 0; i < totalTiles; i++)
            for (var j = 0; j < totalLayers; j++)
                layers[j][i] = j == (int)Layer.Colors ? 0 : -1;

//            cachedTileMap.Clear();
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
            engine.tilemapChip = this;

            // Resize to default nes resolution
            Resize(32, 30);
        }

        public override void Deactivate()
        {
            base.Deactivate();
            engine.tilemapChip = null;
        }

        #region Tile APIs

        
        

        #endregion
    }
}