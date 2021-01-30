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
    }
}