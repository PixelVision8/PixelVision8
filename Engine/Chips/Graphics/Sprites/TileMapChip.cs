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
// 

using System;
using System.Collections.Generic;
using System.Text;
using PixelVisionSDK.Engine.Chips.Data;
using PixelVisionSDK.Engine.Utils;

namespace PixelVisionSDK.Engine.Chips.Graphics.Sprites
{

    /// <summary>
    ///     The tile map chip represents a grid of sprites used to populate the background
    ///     layer of the game. These sprites are fixed and laid out in column and row
    ///     positions making it easier to create grids of tiles. The TileMapChip also
    ///     manages flag values per tile for use in collision detection. Finally, the TileMapChip
    ///     also stores a color offset per tile to simulate palette shifting.
    /// </summary>
    public class TileMapChip : AbstractChip
    {

        protected int[] flags = new int[0];
        protected bool[] invalid = new bool[0];
        protected int[] paletteIDs = new int[0];

        /// <summary>
        ///     A flag to toggle whether tile data should be serialized.
        /// </summary>
        public bool serializeTileData;

        protected int[] spriteIDs = new int[0];
        protected int tmpIndex;
        protected int[] tmpPixelData = new int[8 * 8];
        protected int tmpX;
        protected int tmpY;

        /// <summary>
        ///     Total number of collision <see cref="flags" /> the chip will support.
        ///     The default value is 16.
        /// </summary>
        public int totalFlags = 16;

        /// <summary>
        ///     The <see cref="total" /> tiles in the chip.
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

        /// <summary>
        ///     This creates a json string for the TileMapChip's properties. It will serialize the columns, rows & totalFlags.
        /// </summary>
        /// <param name="sb"></param>
        public override void CustomSerializedData(StringBuilder sb)
        {
            // Columns
            sb.Append("\"columns\":");
            sb.Append(columns);
            sb.Append(",");

            // Rows
            sb.Append("\"rows\":");
            sb.Append(rows);
            sb.Append(",");

            // Total Flags
            sb.Append("\"totalFlags\":");
            sb.Append(totalFlags);

            if (serializeTileData)
            {
                // Disabled for now while I break the files out

                sb.Append(",");

                sb.Append("\"spriteIDs\":[" + string.Join(",", Array.ConvertAll(spriteIDs, x => x.ToString())) + "],");
                sb.Append("\"paletteIDs\":[" + string.Join(",", Array.ConvertAll(paletteIDs, x => x.ToString())) + "],");
                sb.Append("\"flags\":[" + string.Join(",", Array.ConvertAll(flags, x => x.ToString())) + "]");
            }
        }

        /// <summary>
        ///     Attempts to rebuild the TileMap from a Dictionary with a string as
        ///     the key and an object for the value. It looks for columns, rows,
        ///     <see cref="totalFlags" /> and an array of sprite ids which represent
        ///     the tiles.
        /// </summary>
        /// <param name="data">
        ///     A Dictionary with a string for the key and an object for the value.
        /// </param>
        public override void DeserializeData(Dictionary<string, object> data)
        {
            var columns = 32;
            var rows = 30;

            if (data.ContainsKey("columns"))
                columns = (int) (long) data["columns"];

            if (data.ContainsKey("rows"))
                rows = (int) (long) data["rows"];

            if (data.ContainsKey("totalFlags"))
                rows = (int) (long) data["totalFlags"];

            Resize(columns, rows);

            if (data.ContainsKey("spriteIDs") && data.ContainsKey("paletteIDs") && data.ContainsKey("flags"))
            {
                var sIDs = (List<object>) data["spriteIDs"];
                var pIDs = (List<object>) data["paletteIDs"];
                var f = (List<object>) data["flags"];

                //Debug.Log("Deserilize Tile Map");
                var total = columns * rows;
                for (var i = 0; i < total; i++)
                {
                    spriteIDs[i] = (int) (long) sIDs[i];
                    paletteIDs[i] = (int) (long) pIDs[i];
                    flags[i] = (int) (long) f[i];
                }
            }
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
        public void ReadTileAt(int column, int row, out int spriteID, out int paletteID, out int flag)
        {
            PosUtil.CalculateIndex(column, row, columns, out tmpIndex);

            spriteID = spriteIDs[tmpIndex];
            paletteID = paletteIDs[tmpIndex];
            flag = flags[tmpIndex];
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
            PosUtil.CalculateIndex(column, row, columns, out tmpIndex);
            spriteIDs[tmpIndex] = spriteID;
            paletteIDs[tmpIndex] = paletteID;
            flags[tmpIndex] = flag;
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
            PosUtil.CalculateIndex(column, row, columns, out tmpIndex);
            return spriteIDs[tmpIndex];
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
            PosUtil.CalculateIndex(column, row, columns, out tmpIndex);
            spriteIDs[tmpIndex] = spriteID;
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
        ///     Returns the colorint offset.
        /// </returns>
        public int ReadPaletteAt(int column, int row)
        {
            PosUtil.CalculateIndex(column, row, columns, out tmpIndex);
            return paletteIDs[tmpIndex];
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
        ///     A colorint offset.
        /// </param>
        public void UpdatePaletteAt(int column, int row, int paletteID)
        {
            PosUtil.CalculateIndex(column, row, columns, out tmpIndex);
            paletteIDs[tmpIndex] = paletteID;
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
        ///     Returns anint for the flag value.
        /// </returns>
        public int ReadFlagAt(int column, int row)
        {
            PosUtil.CalculateIndex(column, row, columns, out tmpIndex);

            if (tmpIndex < 0 || tmpIndex >= flags.Length)
                return -1;

            return flags[tmpIndex];
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
            PosUtil.CalculateIndex(column, row, columns, out tmpIndex);

            //if(tmpIndex < flags.Length)
            flags[tmpIndex] = flag.Clamp(-1, totalFlags);
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

            var size = total;

            //Debug.Log("Resize Tile Map "+ size);
            if (spriteIDs.Length != size)
                Array.Resize(ref spriteIDs, size);

            if (paletteIDs.Length != size)
                Array.Resize(ref paletteIDs, size);

            if (flags.Length != size)
                Array.Resize(ref flags, size);

            if (clear)
                Clear();
        }

        /// <summary>
        ///     This clears all the tile map data. The spriteID and flag arrays are
        ///     set to -1 as their default value and the palette array is set to 0.
        /// </summary>
        public void Clear()
        {
            //Debug.Log("Clear Map");
            var size = total;
            for (var i = 0; i < size; i++)
            {
                spriteIDs[i] = -1;
                paletteIDs[i] = 0;
                flags[i] = -1;
            }
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
            if (engine.spriteChip == null)
                return;

            var spriteWidth = engine.spriteChip.width;
            var spriteHeight = engine.spriteChip.height;
            var realWidth = columns * spriteWidth;
            var realHeight = rows * spriteHeight;

            if (textureData.width != realWidth || textureData.height != realWidth)
            {
                textureData.Resize(realWidth, realHeight);
            }

            textureData.Clear(clearColor);

            var total = columns * rows;
            var spriteRam = engine.spriteChip;

            int x, y, spriteID;

            for (var i = 0; i < total; i++)
            {
                spriteID = spriteIDs[i];

                if (spriteID > -1)
                {
                    spriteRam.ReadSpriteAt(spriteID, tmpPixelData);

                    // TODO need to adjust for palette

                    PosUtil.CalculatePosition(i, columns, out x, out y);

                    x *= spriteWidth;
                    y = rows - 1 - y;
                    y *= spriteHeight;

                    textureData.SetPixels(x, y, spriteWidth, spriteHeight, tmpPixelData);
                }
            }
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

            //tmpPixelData = new int[engine.spriteChip.width*engine.spriteChip.height];
            // Resize to default nes resolution
            Resize(32, 30);
        }

        public override void Deactivate()
        {
            base.Deactivate();
            engine.tileMapChip = null;
        }

    }

}