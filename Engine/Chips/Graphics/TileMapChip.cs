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
using System.Security.Cryptography.X509Certificates;
using PixelVisionSDK.Utils;
using UnityEngine;

namespace PixelVisionSDK.Chips
{

    /// <summary>
    ///     The tile map chip represents a grid of sprites used to populate the background
    ///     layer of the game. These sprites are fixed and laid out in column and row
    ///     positions making it easier to create grids of tiles. The TileMapChip also
    ///     manages flag values per tile for use in collision detection. Finally, the TileMapChip
    ///     also stores a color offset per tile to simulate palette shifting.
    /// </summary>
    public class TileMapChip : AbstractChip, ILayer
    {

        protected int _columns;
        protected int _rows;
        protected int _scrollX;
        protected int _scrollY;
        protected SpriteChip _spriteChip;
        protected int _totalLayers = -1;
        protected TextureData cachedTileMap = new TextureData(0, 0);
        protected int[][] layers;
        protected int offscreenPadding = 0;
        protected int[] tiles = new int[0];
        protected int tmpIndex;
        protected int[] tmpPixelData = new int[8 * 8];
        protected int tmpX;
        protected int tmpY;

        /// <summary>
        ///     Total number of collision flags the chip will support.
        ///     The default value is 16.
        ///     The default value is 16.
        /// </summary>
        public int totalFlags = 16;

        protected SpriteChip spriteChip
        {
            get
            {
                if (_spriteChip == null)
                    _spriteChip = engine.spriteChip;

                return _spriteChip;
            }
        }

        public int tileWidth
        {
            get { return spriteChip == null ? 8 : engine.spriteChip.width; }
        }

        public int tileHeight
        {
            get { return spriteChip == null ? 8 : engine.spriteChip.height; }
        }

        public int realWidth
        {
            get { return tileWidth * columns; }
        }

        public int realHeight
        {
            get { return tileHeight * rows; }
        }

        /// <summary>
        ///     Returns the total number of data layers stored in the Tilemap. It uses the Layer enum and
        ///     caches the value the first time it is called.
        /// </summary>
        public int totalLayers
        {
            get
            {
                // Let's check to see if the value has been cached yet?
                if (_totalLayers == -1)
                    _totalLayers = Enum.GetNames(typeof(Layer)).Length;

                // Return the cached value
                return _totalLayers;
            }
        }

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
        public int columns { get; private set; }

        /// <summary>
        ///     The height of the tile map in tiles.
        /// </summary>
        public int rows { get; private set; }

        public bool invalid { get; private set; }

        public void Invalidate()
        {
            invalid = true;
        }

        public void InvalidateTileID(int id)
        {
            var tileLayer = layers[(int)Layer.Sprites];
            var invalidLayer = layers[(int)Layer.Invalid];

            var total = tileLayer.Length;
            for (int i = 0; i < total; i++)
            {
                if (tileLayer[i] == id)
                    invalidLayer[i] = -1;
            }

            Invalidate();
        }

        public void ResetValidation()
        {
            invalid = false;
            var invalidLayer = layers[(int) Layer.Invalid];
            Array.Clear(invalidLayer, 0, total);
        }

//        protected int[] _cachedTilemapPixels = new int[0];
//
//        public int[] cachedTilemapPixels
//        {
//            get
//            {
//                if (invalid)
//                {
//                    ReadPixelData(realWidth, realHeight, ref _cachedTilemapPixels);
//                }
//
//                return _cachedTilemapPixels;
//            }
//        }

        public void ReadCachedTilemap(ref int[] pixels)
        {
            var total = realWidth * realHeight;
            
            if (pixels.Length != total)
            {
                Array.Resize(ref pixels, total);
                Invalidate();
            }

            if (invalid)
            {
                ReadPixelData(realWidth, realHeight, ref pixels);
            }

        }

        public void UpdateCachedTilemap(int[] pixels, int x, int y, int blockWidth, int blockHeight, int colorOffset = 0)
        {
//            if (cachedTileMap.width != realWidth || cachedTileMap.height != realHeight)
//                cachedTileMap.Resize(realWidth, realHeight);

            cachedTileMap.SetPixels(x, y, blockWidth, blockHeight, pixels, colorOffset);
            Invalidate();
        }

        public void ReadPixelData(int width, int height, ref int[] pixelData, int offsetX = 0, int offsetY = 0)
        {
            // Test if we need to rebuild the cached tilemap
            if (invalid)
            {

                // Make sure the cached tilemap is the correct width and height
//                if (cachedTileMap.width != realWidth || cachedTileMap.height != realHeight)
//                    cachedTileMap.Resize(realWidth, realHeight);

                // Get a local reference to the layers we need
                var tmpSpriteIDs = layers[(int) Layer.Sprites];
                var tmpPaletteIDs = layers[(int) Layer.Palettes];
                var invalideLayer = layers[(int) Layer.Invalid];

                // Create tmp variables for loop
                int x, y, spriteID;

                // Get a local reference to the total number of tiles
                var totalTiles = total;

                // Loop through all of the tiles in the tilemap
                for (var i = 0; i < totalTiles; i++)
                    if (invalideLayer[i] != 0)
                    {
                        // Get the sprite id
                        spriteID = tmpSpriteIDs[i];

                        // Make sure there is a sprite
                        if (spriteID > -1)
                        {

                            // Read the sprite data
                            spriteChip.ReadSpriteAt(spriteID, tmpPixelData);

                            // Calculate the new position of the tile;
                            x = i % columns * tileWidth;
                            y = i / columns;
                            //x *= tileWidth;
                            y = (rows - 1 - y) * tileHeight;
                            //y *= tileHeight;

                            // Draw the pixel data into the cachedTilemap
                            cachedTileMap.SetPixels(x, y, tileWidth, tileHeight, tmpPixelData, tmpPaletteIDs[i]);
                        }
                    }

                // Reset the invalidation state
                ResetValidation();
            }

            // Return the requested pixel data
            cachedTileMap.GetPixels(offsetX, offsetY, width, height, ref pixelData);

        }
        
        public void Invalidate(int index)
        {
            Invalidate();
            layers[(int) Layer.Invalid][index] = -1;
        }

        /// <summary>
        ///     Reads the current tile and output the spriteID,
        ///     <paramref name="paletteID" /> and <paramref name="flag" /> value. Use
        ///     this to get access to the underlying tile map data structure.
        /// </summary>
        /// <param name="column">
        ///     The column position of the tile. 0 is the left of the tile map.
        /// </param>
        /// <param name="row">
        ///     The row position of the tile. 0 is the top of the tile map.
        /// </param>
        /// <param name="spriteID">The id of the sprite to use.</param>
        /// <param name="paletteID">
        ///     The color offset to use when rendering the sprite.
        /// </param>
        /// <param name="flag">The flag value used for collision.</param>
        public void ReadTile(int column, int row, out int spriteID, out int paletteID, out int flag)
        {
            spriteID = ReadDataAt(Layer.Sprites, column, row);
            paletteID = ReadDataAt(Layer.Palettes, column, row);
            flag = ReadDataAt(Layer.Flags, column, row);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="column"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        public int ReadTileAt(int column, int row)
        {
            return ReadDataAt(Layer.Sprites, column, row);
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
            var index = column + row * columns;
            
            return index > layers[id].Length ? -1 : layers[id][index];
        }

        protected void UpdateDataAt(Layer name, int column, int row, int value)
        {
            UpdateDataAt((int) name, column, row, value);
        }

        protected void UpdateDataAt(int id, int column, int row, int value)
        {
            if (column >= columns)
                column = MathUtil.Repeat(column, columns);
            var index = column + row * columns;
            
            layers[id][index] = value;
            Invalidate(index);
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
        public void UpdateTileAt(int spriteID, int column, int row, int flag = 0, int paletteID = 0)
        {

            UpdateDataAt(Layer.Sprites, column, row, spriteID);
            UpdateDataAt(Layer.Palettes, column, row, paletteID);
            UpdateDataAt(Layer.Flags, column, row, flag);

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
            return ReadDataAt(Layer.Palettes, column, row);
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
            UpdateDataAt(Layer.Palettes, column, row, paletteID);
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

            cachedTileMap.Resize(realWidth, realHeight);

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
                layers[j][i] = -1;
            
            cachedTileMap.Clear();
            Invalidate();



        }

        /// <summary>
        ///     This method converts the tile map into pixel data that can be
        ///     rendered by the engine. It's an expensive operation and should only
        ///     be called when the game or level is loading up. This data can be
        ///     passed into the ScreenBufferChip to allow cached rendering of the
        ///     tile map as well as scrolling of the tile map if it is larger then
        ///     the screen's resolution.
        /// </summary>
        /// <param name="textureData">
        ///     A reference to a <see cref="TextureData" /> class to populate with
        ///     tile map pixel data.
        /// </param>
        /// <param name="clearColor">
        ///     The transparent color to use when a tile is set to -1. The default
        ///     value is -1 for transparent.
        /// </param>
        public void ConvertToTextureData(TextureData textureData, int clearColor = -1)
        {
            if (spriteChip == null)
                return;

            //TODO need to reconnect this so you can export tilemap
            //ReadPixelData(textureData, 0, 0, columns, rows);
        }

        /// <summary>
        ///     Configured the TileMapChip. This method sets the
        ///     <see cref="TileMapChip" /> as the default tile map for the engine. It
        ///     also resizes the tile map to its default size of 32 x 30 which is a
        ///     resolution of 256 x 240.
        /// </summary>
        public override void Configure()
        {
            //ppu.tileMap = this;
            engine.tileMapChip = this;

            // Resize to default nes resolution
            Resize(32, 30);
        }

        public override void Deactivate()
        {
            base.Deactivate();
            engine.tileMapChip = null;
        }

        protected enum Layer
        {

            Sprites,
            Palettes,
            Flags,
            Invalid

        }

    }

}