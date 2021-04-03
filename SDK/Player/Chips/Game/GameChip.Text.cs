using System;

namespace PixelVision8.Player
{
    public partial class GameChip
    {
        protected FontChip FontChip => Player.FontChip;

        private const int CharOffset = 32;

        /// <summary>
        ///     A helper method to convert a string of characters into an array of sprite IDs.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="fontName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public int[] ConvertTextToSprites(string text, string fontName = "default")
        {
            var total = text.Length;

            // TODO convert this to a list
            var spriteIDs = new int[total];

            var fontMap = FontChip.ReadFont(fontName);

            // Test to make sure font exists
            if (fontMap == null) throw new Exception("Font '" + fontName + "' not found.");

            var totalCharacters = fontMap.Length;

            for (var i = 0; i < total; i++)
            {
                var character = text[i];
                var index = Convert.ToInt32(character) - CharOffset;
                var spriteId = -1;

                if (index < totalCharacters && index > -1) spriteId = fontMap[index];

                spriteIDs[i] = spriteId;
            }

            return spriteIDs;
        }

        public int[] FontChar(char character, string fontName, int[] data = null)
        {
            var fontMap = FontChip.ReadFont(fontName);

            // Test to make sure font exists
            if (fontMap == null) throw new Exception("Font '" + fontName + "' not found.");

            // TODO need to test this out
            var id = ConvertTextToSprites(character.ToString(), fontName)[0];

            if (data != null)
            {
                FontChip.UpdateSpriteAt(id, data);

                return data;
            }

            FontChip.ReadSpriteAt(id, ref _tmpSpriteData);

            return _tmpSpriteData;
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
        public void DrawText(string text, int x, int y, DrawMode drawMode = DrawMode.Sprite, string font = "large",
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
                nextX *= Constants.SpriteSize;
                nextY *= Constants.SpriteSize;

                // spacing is disabled when in tilemode
                spacing = 0;
            }

            var offset = Constants.SpriteSize + spacing;

            // Save the current sprite mode
            var oldSpriteWidth = SpriteWidth;
            var oldSpriteHeight = SpriteHeight;

            // Change the sprite width and height to the default value
            SpriteWidth = Constants.SpriteSize;
            SpriteHeight = Constants.SpriteSize;

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

                DrawSprite(spriteIDs[j], nextX, nextY, false, false, drawMode, colorOffset, FontChip);

                nextX += offset;
            }

            // Restore the previous sprite width and height
            SpriteWidth = oldSpriteWidth;
            SpriteHeight = oldSpriteHeight;

        }
    }
}