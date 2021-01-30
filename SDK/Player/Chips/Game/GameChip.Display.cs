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
        private PixelData _tmpPixelData = new PixelData();
        public int CurrentSprites => Player.SpriteCounter;

        #region Display

        /// <summary>
        ///     The display's size defines the visible area where pixel data exists on the screen. Calculating this is
        ///     important for knowing how to position sprites on the screen. The Display() method allows you to get
        ///     the resolution of the display at run time. By default, this will return the visble screen area based on
        ///     the overscan value set on the display chip. To calculate the exact overscan in pixels, you must subtract
        ///     the full size from the visible size. Simply supply false as an argument to get the full display dimensions.
        /// </summary>
        public Point Display()
        {
            return new Point(DisplayChip.Width, DisplayChip.Height);
        }

        /// <summary>
        ///     This method allows you to draw raw pixel data directly to the display. Depending on which draw mode you
        ///     use, the pixel data could be rendered as a sprite or drawn directly onto the tilemap cache. Sprites drawn
        ///     with this method still count against the total number the display can render but you can draw irregularly
        ///     shaped sprites by defining a custom width and height. For drawnig into the tilemap cache directly, you can
        ///     use this to change the way the tilemap looks at run-time without having to modify a sprite's pixel data.
        ///     It is important to note that when you change a tile's sprite ID or color offset, the tilemap redraws it
        ///     back to the cache overwriting any pixel data that was previously there.
        /// </summary>
        /// <param name="pixelData">
        ///     The pixelData argument accepts an int array representing references to color IDs. The pixelData array length
        ///     needs to be the same size as the supplied width and height, or it will throw an error.
        /// </param>
        /// <param name="x">
        ///     The x position where to display the new pixel data. The display's horizontal 0 position is on the far left-hand
        ///     side.
        ///     When using DrawMode.TilemapCache, the pixel data is drawn into the tilemap's cache instead of directly on
        ///     the display when using DrawMode.Sprite.
        /// </param>
        /// <param name="y">
        ///     The Y position where to display the new pixel data. The display's vertical 0 position is on the top. When using
        ///     DrawMode.TilemapCache, the pixel data is drawn into the tilemap's cache instead of directly on the display
        ///     when using DrawMode.Sprite.
        /// </param>
        /// <param name="blockWidth">
        ///     The width of the pixel data to use when rendering to the display.
        /// </param>
        /// <param name="blockHeight">
        ///     The height of the pixel data to use when rendering to the display.
        /// </param>
        /// <param name="drawMode">
        ///     This argument accepts the DrawMode enum. You can use Sprite, SpriteBelow, and TilemapCache to change where the
        ///     pixel data is drawn to. By default, this value is DrawMode.Sprite.
        /// </param>
        /// <param name="flipH">
        ///     This is an optional argument which accepts a bool. The default value is set to false but passing in true flips
        ///     the pixel data horizontally.
        /// </param>
        /// <param name="flipV">
        ///     This is an optional argument which accepts a bool. The default value is set to false but passing in true flips
        ///     the pixel data vertically.
        /// </param>
        /// <param name="colorOffset">
        ///     This optional argument accepts an int that offsets all the color IDs in the pixel data array. This value is added
        ///     to each int, in the pixel data array, allowing you to simulate palette shifting.
        /// </param>
        public void DrawPixels(int[] pixelData, int x, int y, int blockWidth, int blockHeight, bool flipH = false,
            bool flipV = false, DrawMode drawMode = DrawMode.Sprite, int colorOffset = 0)
        {
            switch (drawMode)
            {
                case DrawMode.Tile:
                    // Do nothing since we can't set raw pixel data to a tile
                    break;

                case DrawMode.TilemapCache:

                    if (_tmpPixelData.Width != blockWidth || _tmpPixelData.Height != blockHeight)
                        _tmpPixelData.Resize(blockWidth, blockHeight);

                    _tmpPixelData.Pixels = pixelData;

                    // Copy pixel data directly into the tilemap chip's texture
                    Utilities.MergePixels(_tmpPixelData, 0, 0, blockWidth, blockHeight, TilemapChip.PixelData, x, y,
                        flipH, flipV, colorOffset);

                    break;

                default:

                    DisplayChip.NewDrawCall(pixelData, x, y, blockWidth, blockHeight, (byte) drawMode, flipH, flipV,
                        colorOffset);

                    break;
            }
        }

        /// <summary>
        ///     Sprites represent individual collections of pixel data at a fixed size. By default, Pixel Vision 8 sprites are
        ///     8 x 8 pixels and have a set limit of visible colors. You can use the DrawSprite() method to render any sprite
        ///     stored in the Sprite Chip. The display also has a limitation on how many sprites can be on the screen at one time.
        ///     Each time you call DrawSprite(), the sprite counts against the total amount the display can render. If you attempt
        ///     to
        ///     draw more sprites than the display can handle, the call is ignored. One thing to keep in mind when drawing sprites
        ///     is that their x and y position wraps if they reach the right or bottom border of the screen. You need to change
        ///     the overscan border to hide sprites offscreen.
        /// </summary>
        /// <param name="id">
        ///     The unique ID of the sprite to use in the SpriteChip.
        /// </param>
        /// <param name="x">
        ///     An int value representing the X position to place sprite on the display. If set to 0, it renders on the far
        ///     left-hand side of the screen.
        /// </param>
        /// <param name="y">
        ///     An int value representing the Y position to place sprite on the display. If set to 0, it renders on the top
        ///     of the screen.
        /// </param>
        /// <param name="flipH">
        ///     This is an optional argument which accepts a bool. The default value is set to false but passing in true flips
        ///     the pixel data horizontally.
        /// </param>
        /// <param name="flipV">
        ///     This is an optional argument which accepts a bool. The default value is set to false but passing in true flips
        ///     the pixel data vertically.
        /// </param>
        /// <param name="drawMode"></param>
        /// <param name="colorOffset">
        ///     This optional argument accepts an int that offsets all the color IDs in the pixel data array. This value is added
        ///     to each int, in the pixel data array, allowing you to simulate palette shifting.
        /// </param>
        /// <param name="srcChip"></param>
        /// <param name="aboveBG">
        ///     An optional bool that defines if the sprite is above or below the tilemap. Sprites are set to render above the
        ///     tilemap by default. When rendering below the tilemap, the sprite is visible in the transparent area of the tile
        ///     above the background color.
        /// </param>
        public void DrawSprite(int id, int x, int y, int columns = 1, int rows = 1, bool flipH = false,
            bool flipV = false,
            DrawMode drawMode = DrawMode.Sprite, int colorOffset = 0, SpriteChip srcChip = null)
        {
            // Only apply the max sprite count to sprite draw modes

            srcChip ??= SpriteChip;

            if (drawMode == DrawMode.Tile)
            {
                Tile(x, y, id, colorOffset);
            }
            else
            {
                
                if (drawMode == DrawMode.TilemapCache)
                {
                    srcChip.ReadSpriteAt(id, ref _tmpSpriteData);

                    DrawPixels(_tmpSpriteData, x, y, SpriteChip.SpriteWidth, SpriteChip.SpriteHeight, flipH, flipV, drawMode,
                        colorOffset);
                }
                else
                {
                    if (SpriteChip.MaxSpriteCount > 0 && CurrentSprites >= SpriteChip.MaxSpriteCount) return;

                    var pos = Utilities.CalculatePosition(id, srcChip.Columns);
                    pos.X *= SpriteChip.SpriteWidth;
                    pos.Y *= SpriteChip.SpriteHeight;

                    DisplayChip.NewDrawCall(srcChip, x, y, SpriteChip.SpriteWidth * columns, SpriteChip.SpriteHeight * rows,
                        (byte) drawMode, flipH, flipV, colorOffset, pos.X, pos.Y);

                    Player.SpriteCounter++;
                    // }
                }
            }
        }

        public virtual void DrawSprites(int[] ids, int x, int y, int width, bool flipH = false, bool flipV = false,
            DrawMode drawMode = DrawMode.Sprite, int colorOffset = 0)
        {
            // TODO need to delete this after the refactoring
        }

        /// <summary>
        ///     The DrawText() method allows you to render text to the display. By supplying a custom DrawMode, you can render
        ///     characters as individual sprites (DrawMode.Sprite), tiles (DrawMode.Tile) or drawn directly into the tilemap
        ///     cache (DrawMode.TilemapCache). When drawing text as sprites, you have more flexibility over position, but each
        ///     character counts against the displays' maximum sprite count. When rendering text to the tilemap, more characters
        ///     are shown and also increase performance when rendering large amounts of text. You can also define the color offset,
        ///     letter spacing which only works for sprite and tilemap cache rendering, and a width in characters if you want the
        ///     text to wrap.
        /// </summary>
        /// <param name="text">
        ///     A text string to display on the screen.
        /// </param>
        /// <param name="x">
        ///     An int value representing the X position to start the text on the display. If set to 0, it renders on the far
        ///     left-hand side of the screen.
        /// </param>
        /// <param name="y">
        ///     An int value representing the Y position to place sprite on the display. If set to 0, it renders on the top
        ///     of the screen.
        /// </param>
        /// <param name="drawMode">
        ///     This argument accepts the DrawMode enum. You can use Sprite, SpriteBelow, and TilemapCache to change where the
        ///     pixel data is drawn to. By default, this value is DrawMode.Sprite.
        /// </param>
        /// <param name="font">
        ///     The name of the font to use. You do not need to add the font's file extension. If the file is called
        ///     The name of the font to use. You do not need to add the font's file extension. If the file is called
        ///     default.font.png,
        ///     you can simply refer to it as "default" when supplying an argument value.
        /// </param>
        /// <param name="colorOffset">
        ///     This optional argument accepts an int that offsets all the color IDs in the pixel data array. This value is added
        ///     to each color ID in the font's pixel data, allowing you to simulate palette shifting.
        /// </param>
        /// <param name="spacing">
        ///     This optional argument sets the number of pixels between each character when rendering text. This value is ignored
        ///     when rendering text as tiles. This value can be positive or negative depending on your needs. By default, it is 0.
        /// </param>
        /// <returns></returns>
        public void DrawText(string text, int x, int y, DrawMode drawMode = DrawMode.Sprite, string font = "Default",
            int colorOffset = 0, int spacing = 0)
        {
            
            var nextX = x;
            var nextY = y;

            var spriteIDs = ConvertTextToSprites(text, font);
            var total = spriteIDs.Length;

            var clearTiles = false;

            if (drawMode == DrawMode.Tile)
            {
                // Set the clear tile flag
                clearTiles = true;

                // Change to Tilemap Cache since we are not actually drawing the tiles
                drawMode = DrawMode.TilemapCache;

                // Need to adjust the position since tile mode is in columns,rows
                nextX *= 8;
                nextY *= 8;

                // spacing is disabled when in tilemode
                spacing = 0;
            }

            var offset = SpriteChip.SpriteWidth + spacing;

            for (var j = 0; j < total; j++)
            {
                // Clear the background when in tile mode
                if (clearTiles) Tile(nextX / 8, nextY / 8, -1);

                // Manually increase the sprite counter if drawing to one of the sprite layers
                if (drawMode == DrawMode.Sprite || drawMode == DrawMode.SpriteAbove ||
                    drawMode == DrawMode.SpriteBelow)
                {
                    // If the sprite counter has been met, exit out of the draw call
                    if (SpriteChip.MaxSpriteCount > 0 && CurrentSprites >= SpriteChip.MaxSpriteCount) return;

                    Player.SpriteCounter++;
                }

                DrawSprite(spriteIDs[j], nextX, nextY, 1, 1, false, false, drawMode, colorOffset, FontChip);

                // }

                nextX += offset;
            }
        }

        /// <summary>
        ///     This method allows you to draw a rectangle with a fill color. By default, this method is used to clear the screen
        ///     but you can supply a color offset to change the color value and use it to fill a rectangle area with a specific
        ///     color instead.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="color"></param>
        /// <param name="drawMode"></param>
        public void DrawRect(int x, int y, int width, int height, int color = -1,
            DrawMode drawMode = DrawMode.TilemapCache)
        {
            var pixels = new int[width * height];
            // TODO is there a faster way to do this?
            DrawPixels(pixels, x, y, width, height, false, false, drawMode, color);
        }


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
                columns == 0 ? DisplayChip.Width : columns * SpriteChip.SpriteWidth,
                rows == 0 ? DisplayChip.Height : rows * SpriteChip.SpriteHeight,
                (byte) DrawMode.Tile,
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

        #endregion
    }
}