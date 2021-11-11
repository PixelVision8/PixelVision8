using System;

namespace PixelVision8.Player
{
    public partial class GameChip
    {
        protected FontChip FontChip => Player.FontChip;

        /// <summary>
        ///     A helper method to convert a string of characters into an array of sprite IDs.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="fontName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public SpriteCollection ConvertTextToSprites(string text, string fontName = "default", int spacing = 0, int[] colorOffsets = null)
        {
            var total = text.Length;

            colorOffsets ??= new int[1];

            var spriteCollection = new SpriteCollection(text);

            // Test to make sure font exists
            if (FontChip.ReadFont(fontName) == null) throw new Exception("Font '" + fontName + "' not found.");

            var nextX = 0;
            
            var offset = Constants.SpriteSize + spacing;
        
            var lastColorOffset = 0; 

            for (var i = 0; i < total; i++)
            {
                if(i < colorOffsets.Length)
                    lastColorOffset = colorOffsets[i];

                spriteCollection.AddSprite(FindCharSpriteId(text[i], fontName), nextX, 0, false, false, lastColorOffset);

                nextX += offset;
            }

            return spriteCollection;
        }

        // Write pixel data directly into the font chip just like Sprite()
        public int[] Char(int id, int[] data = null) => Sprite(id, data);
        
        public int FindCharSpriteId(char character, string fontName)
        {
            var fontMap = FontChip.ReadFont(fontName);

            var index = Convert.ToInt32(character);

            if(fontMap == null || (index >= fontMap.Length || index < 0))
                return -1;

            return fontMap[index];

        }

        // Write pixel data into a font's char sprite
        public int[] FontChar(char character, string fontName, int[] data = null)
        {
            var fontMap = FontChip.ReadFont(fontName);

            // Test to make sure font exists
            if (fontMap == null) throw new Exception("Font '" + fontName + "' not found.");

            return Char(FindCharSpriteId(character, fontName), data);
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
            int colorOffset = 0, int spacing = 0, bool flipH = false, bool flipV = false) => DrawColoredText(text, x, y, drawMode, font, new int[]{colorOffset}, spacing, flipH, flipV);

        public void DrawColoredText(string text, int x, int y, DrawMode drawMode = DrawMode.Sprite, string font = "large",
            int[] colorOffset = null, int spacing = 0, bool flipH = false, bool flipV = false)
        {

            var sprites = ConvertTextToSprites(text, font, spacing, colorOffset);
            DrawMetaSprite(sprites, x, y, flipH, flipV, drawMode);

        }
    }
}