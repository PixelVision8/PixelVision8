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

namespace PixelVision8.Player
{
    public partial class GameChip
    {
        // TODO shared id with GameChip_Sprite
        protected TilemapChip TilemapChip => Player.TilemapChip;
        protected Point _tilemapSize = Point.Zero;
        
        /// <summary>
        ///     By default, the tilemap renders to the display by simply calling DrawTilemap(). This automatically fills the entire
        ///     display with the visible portion of the tilemap. To have more granular control over how to render the tilemap, you
        ///     can supply an optional X and Y position to change where it draws on the screen. You can also modify the width
        ///     (columns) and height (rows) that are displayed too. This is useful if you want to show a HUD or some other kind of
        ///     image on the screen that is not overridden by the tilemap. To scroll the tilemap, you need to call the
        ///     ScrollPosition() and supply a new scroll X and Y value.
        /// </summary>
        /// <param name="x">
        ///     An optional int value representing the X position to render the tilemap on the display. If set to 0, it
        ///     renders on the far left-hand side of the screen.
        /// </param>
        /// <param name="y">
        ///     An optional int value representing the Y position to render the tilemap on the display. If set to 0, it
        ///     renders on the top of the screen.
        /// </param>
        /// <param name="columns">
        ///     An optional int value representing how many horizontal tiles to include when drawing the map. By default, this is
        ///     0 which automatically uses the full visible width of the display, while taking into account the X position offset.
        /// </param>
        /// <param name="rows">
        ///     An optional int value representing how many vertical tiles to include when drawing the map. By default, this is 0
        ///     which automatically uses the full visible height of the display, while taking into account the Y position offset.
        /// </param>
        /// <param name="offsetX">
        ///     An optional int value to override the scroll X position. This is useful when you need to change the left x position
        ///     from where to sample the tilemap data from.
        /// </param>
        /// <param name="offsetY">
        ///     An optional int value to override the scroll Y position. This is useful when you need to change the top y position
        ///     from where to sample the tilemap data from.
        /// </param>
        public void DrawTilemap(int x = 0, int y = 0, int columns = 0, int rows = 0, int? offsetX = null,
            int? offsetY = null)
        {

            DisplayChip.NewDrawCall(
                TilemapChip,
                x,
                y,
                columns == 0 ? DisplayChip.Width : columns * SpriteChip.width,
                rows == 0 ? DisplayChip.Height : rows * SpriteChip.height,
                (byte)DrawMode.Tile,
                false,
                false,
                0,
                offsetX ?? _scrollPos.X,
                offsetY ?? _scrollPos.Y
            );


        }

        /// <summary>
        ///     You can use RedrawDisplay to make clearing and drawing the tilemap easier. This is a helper method automatically
        ///     calls both Clear() and DrawTilemap() for you.
        /// </summary>
        public void RedrawDisplay()
        {
            Clear();
            DrawTilemap();
        }

        /// <summary>
        ///     You can scroll the tilemap by calling the ScrollPosition() method and supplying a new scroll X and Y position.
        ///     By default, calling ScrollPosition() with no arguments returns a vector with the current scroll X and Y values.
        ///     If you supply an X and Y value, it updates the tilemap's scroll position the next time you call the
        ///     DrawTilemap() method.
        /// </summary>
        /// <param name="x">
        ///     An optional int value representing the scroll X position of the tilemap. If set to 0, it starts on the far
        ///     left-hand side of the tilemap.
        /// </param>
        /// <param name="y">
        ///     An optional int value representing the scroll Y position of the tilemap. If set to 0, it starts on the top of
        ///     the tilemap.
        /// </param>
        /// <returns>
        ///     By default, this method returns a vector with the current scroll X and Y position.
        /// </returns>
        public Point ScrollPosition(int? x = null, int? y = null)
        {

            if (x.HasValue)
            {
                _scrollPos.X = x.Value;
            }

            if (y.HasValue)
            {
                _scrollPos.Y = y.Value;
            }

            return _scrollPos;
        }
        #region Tilemap

        /// <summary>
        ///     This allows you to quickly access just the flag value of a tile. This is useful when trying
        ///     to the caluclate collision on the tilemap. By default, you can call this method and return
        ///     the flag value. If you supply a new value, it will be overridden on the tile. Changing a
        ///     tile's flag value does not force the tile to be redrawn to the tilemap cache.
        /// </summary>
        /// <param name="column">
        ///     The X position of the tile in the tilemap. The 0 position is on the far left of the tilemap.
        /// </param>
        /// <param name="row">
        ///     The Y position of the tile in the tilemap. The 0 position is on the top of the tilemap.
        /// </param>
        /// <param name="value">
        ///     The new value for the flag. Setting the flag to -1 means no collision.
        /// </param>
        /// <returns></returns>
        public int Flag(int column, int row, int? value = null)
        {
            var tile = TilemapChip.GetTile(column, row);

            if (value.HasValue) tile.Flag = value.Value;

            return tile.Flag;
        }

        /// <summary>
        ///     This allows you to get the current sprite id, color offset and flag values associated with
        ///     a given tile. You can optionally supply your own if you want to change the tile's values.
        ///     Changing a tile's sprite id or color offset will for the tilemap to redraw it to the cache
        ///     on the next frame. If you are drawing raw pixel data into the tilemap cache in the same
        ///     position, it will be overwritten with the new tile's pixel data.
        /// </summary>
        /// <param name="column">
        ///     The X position of the tile in the tilemap. The 0 position is on the far left of the tilemap.
        /// </param>
        /// <param name="row">
        ///     The Y position of the tile in the tilemap. The 0 position is on the top of the tilemap.
        /// </param>
        /// <param name="spriteID">
        ///     The sprite id to use for the tile.
        /// </param>
        /// <param name="colorOffset">
        ///     Shift the color IDs by this value.
        /// </param>
        /// <param name="flag">
        ///     An int value between -1 and 16 used for collision detection.
        /// </param>
        /// <param name="flipH"></param>
        /// <param name="flipV"></param>
        /// <returns>
        ///     Returns a dictionary containing the spriteID, colorOffset, and flag for an individual tile.
        /// </returns>
        //TODO this should return a custom class not a Dictionary
        public TileData Tile(int column, int row, int? spriteID = null, int? colorOffset = null, int? flag = null,
            bool? flipH = null, bool? flipV = null)
        {
            var invalidateTileMap = false;

            var tile = TilemapChip.GetTile(column, row);

            if (spriteID.HasValue)
            {
                tile.SpriteId = spriteID.Value;
                invalidateTileMap = true;
            }

            if (colorOffset.HasValue)
            {
                tile.ColorOffset = colorOffset.Value;
                invalidateTileMap = true;
            }

            if (flag.HasValue)
            {
                tile.Flag = flag.Value;
                invalidateTileMap = true;
            }

            if (flipH.HasValue)
            {
                tile.FlipH = flipH.Value;
                invalidateTileMap = true;
            }


            if (flipV.HasValue)
            {
                tile.FlipV = flipV.Value;
                invalidateTileMap = true;
            }

            if (invalidateTileMap) TilemapChip.Invalidate();

            return tile;
            //            return new TileData(CalculateIndex(column, row, tilemapChip.columns), tilemapChip.ReadSpriteAt(column, row), tilemapChip.ReadTileColorAt(column, row), tilemapChip.ReadFlagAt(column, row));
        }

        /// <summary>
        ///     This will return a vector representing the size of the tilemap in columns (x) and rows (y).
        ///     To find the size in pixels, you will need to multiply the returned vectors x and y values by
        ///     the sprite size's x and y. This method also allows you to resize the tilemap by passing in an
        ///     optional new width and height. Resizing the tile map is destructive, so any changes will
        ///     automatically clear the tilemap's sprite ids, color offsets, and flag values.
        /// </summary>
        /// <param name="width">
        ///     An optional parameter for the width in tiles of the map.
        /// </param>
        /// <param name="height">
        ///     An option parameter for the height in tiles of the map.
        /// </param>
        /// <returns>
        ///     Returns a vector of the tile maps size in tiles where x and y are the columns and rows of the tilemap.
        /// </returns>
        public Point TilemapSize(int? width = null, int? height = null, bool clear = false)
        {

            // Update with the latest value
            _tilemapSize.X = TilemapChip.columns;
            _tilemapSize.Y = TilemapChip.rows;

            var resize = false;

            if (width.HasValue)
            {
                _tilemapSize.X = width.Value;
                resize = true;
            }

            if (height.HasValue)
            {
                _tilemapSize.Y = height.Value;
                resize = true;
            }

            if (resize) TilemapChip.Resize(_tilemapSize.X, _tilemapSize.Y, clear);

            return _tilemapSize;
        }

        /// <summary>
        ///     A helper method which allows you to update several tiles at once. Simply define the start column
        ///     and row position, the width of the area to update in tiles and supply a new int array of sprite
        ///     IDs. You can also modify the color offset and flag value of the tiles via the optional parameters.
        ///     This helper method uses calls the Tile() method to update each tile, so any changes to a tile
        ///     will be automatically redrawn to the tilemap's cache.
        /// </summary>
        /// <param name="column">
        ///     Start column of the first tile to update. The 0 column is on the far left of the tilemap.
        /// </param>
        /// <param name="row">
        ///     Start row of the first tile to update. The 0 row is on the top of the tilemap.
        /// </param>
        /// <param name="columns">
        ///     The width of the area in tiles to update.
        /// </param>
        /// <param name="ids">
        ///     An array of sprite IDs to use for each tile being updated.
        /// </param>
        /// <param name="colorOffset">
        ///     An optional color offset int value to be applied to each updated tile.
        /// </param>
        /// <param name="flag">
        ///     An optional flag int value to be applied to each updated tile.
        /// </param>
        public void UpdateTiles(int[] ids, int? colorOffset = null, int? flag = null)
        {
            total = ids.Length;
            _width = TilemapSize().X;

            //TODO need to get offset and flags working

            for (var i = 0; i < total; i++)
            {
                id = ids[i];

                var pos = Utilities.CalculatePosition(id, _width);

                Tile(pos.X, pos.Y, null, colorOffset, flag);
            }
        }

        #endregion
    }
}