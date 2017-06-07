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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PixelVisionSDK.Chips
{

    /// <summary>
    ///     The font chip allows you to render text to the display. It is built on
    ///     top of the same APIs as the <see cref="SpriteChip" /> but has custom
    ///     methods for converting text into sprites.
    /// </summary>
    public class FontChip : AbstractChip
    {

        public static string newline = "\r\n";

        protected static int charOffset = 32;

        protected Dictionary<string, int[]> fonts = new Dictionary<string, int[]>();

        public int[] tmpPixels = new int[0];
        protected TextureData tmpTextureData = new TextureData(1, 1, false);

        /// <summary>
        ///     This method configures the FontChip. It registers itself with the
        ///     engine, sets the default width and height to 8 and resizes the
        ///     <see cref="TextureData" /> to 96 x 64.
        /// </summary>
        public override void Configure()
        {
            engine.fontChip = this;
        }

        public override void Deactivate()
        {
            base.Deactivate();
            engine.fontChip = null;
        }

        /// <summary>
        ///     Adds a font to the FontChip. Each font requires a name and an array
        ///     of IDs for the sprites to be used. Each sprite should refer to their
        ///     character's ASCII code minus 32 since the font map starts at the empty
        ///     space character
        /// </summary>
        /// <param name="name"></param>
        /// <param name="fontMap"></param>
        public void AddFont(string name, int[] fontMap)
        {
            if (fonts.ContainsKey(name))
                fonts[name] = fontMap;
            else
                fonts.Add(name, fontMap);
        }

        /// <summary>
        ///     This method converts text into pixel data. It leverages the
        ///     GenerateTextData() method. The supplied <paramref name="pixels" />
        ///     int array, <paramref name="width" /> and
        ///     <paramref name="height" /> values will be set with the data generated
        ///     by the method.
        /// </summary>
        /// <param name="value">The string to convert.</param>
        /// <param name="pixels">An array to set the new pixel data on.</param>
        /// <param name="width">The returned width of the text.</param>
        /// <param name="height">The returned height of the text.</param>
        /// <param name="fontName"></param>
        /// <param name="letterSpacing"></param>
        /// <param name="offset"></param>
        /// <param name="colorID">
        ///     The color id to use when rendering the text. This should match up
        ///     the colors in the ColorChip.
        /// </param>
        public void ConvertTextToPixelData(string value, ref int[] pixels, out int width, out int height, string fontName = "Default", int letterSpacing = 0, int offset = 0)
        {
            if (fontName == "Default")
                fontName = fonts.Keys.First();

            // if no font exists, exit out of the draw call
            if (fonts.ContainsKey(fontName))
            {
                var fontMap = fonts[fontName];
                GenerateTextData(value, engine.spriteChip, fontMap, tmpTextureData, false, letterSpacing, offset);
            }
            else
            {
                tmpTextureData.Resize(1, 1);
            }

            tmpTextureData.CopyPixels(ref pixels);
            width = tmpTextureData.width;
            height = tmpTextureData.height;
        }

        /// <summary>
        ///     Use this method to get the raw pixel data of the font text that is generated.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="spriteChip"></param>
        /// <param name="fontName"></param>
        /// <param name="pixels"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="letterSpacing"></param>
        public void ConvertTextToPixelData(string value, SpriteChip spriteChip, string fontName, ref int[] pixels, out int width, out int height, int letterSpacing = 0)
        {
            if (fontName == "Default")
                fontName = fonts.Keys.First();

            var fontMap = fonts[fontName];

            GenerateTextData(value, spriteChip, fontMap, tmpTextureData, false, letterSpacing);
            tmpTextureData.CopyPixels(ref pixels);
            width = tmpTextureData.width;
            height = tmpTextureData.height;
        }

        /// <summary>
        ///     This method is responsible for converting a string of text into
        ///     TextureData. This is done by converting each character in the string
        ///     into an id that maps to the sprites stored in the FontChip. The ids
        ///     are based on ASCII values. The font class starts at ASCII
        ///     <paramref name="value" /> for an empty space and will scale up from
        ///     there based on how many sprites are in the FontChip. By default it
        ///     supports up to ASCII 128 but can support more as long as the sprites
        ///     exist.
        /// </summary>
        /// <param name="value">
        ///     The string to be converted into pixel data.
        /// </param>
        /// <param name="spriteChip"></param>
        /// <param name="fontMap"></param>
        /// <param name="textureData">
        ///     A reference to a <see cref="TextureData" /> class to store the pixel
        ///     data in.
        /// </param>
        /// <param name="stripEmptyLines">
        ///     Ignore empty lines in the supplied text. By default this is set to
        ///     false.
        /// </param>
        /// <param name="letterSpacing"></param>
        /// <param name="offset"></param>
        public void GenerateTextData(string value, SpriteChip spriteChip, int[] fontMap, TextureData textureData, bool stripEmptyLines = false, int letterSpacing = 0, int offset = 0)
        {
            // Strip out any tabs
            value = value.Replace("\t", "     ");

            var result = value.Split(new[] {"\n", "\r\n"},
                stripEmptyLines ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);

            Array.Reverse(result);

            var totalLines = result.Length;

            //spriteChip = this;

            var cWidth = spriteChip.width;
            var cHeight = spriteChip.height;

            // index text
            var tWidth = 0;
            var tHeight = totalLines;

            for (var i = 0; i < totalLines; i++)
                tWidth = Math.Max(tWidth, result[i].Length);

            var realWidth = (cWidth + letterSpacing) * tWidth;
            var realHeight = cHeight * tHeight;

            if (textureData.width != realWidth || textureData.height != realHeight)
                textureData.Resize(realWidth, realHeight);

            textureData.Clear();

            // convert each line into a sprite id

            var charOffset = 32;

            var tmpData = new int[spriteChip.width * spriteChip.height];
            for (var i = 0; i < totalLines; i++)
            {
                var line = result[i];
                var characters = line.Length;
                for (var j = 0; j < characters; j++)
                {
                    var character = line[j];
                    var spriteID = Convert.ToInt32(character) - charOffset;

                    //Debug.Log("Char " + character + " " + spriteID);
                    spriteChip.ReadSpriteAt(fontMap[spriteID], tmpData);

                    textureData.MergePixels(j * (cWidth + letterSpacing), i * cHeight, cWidth, cHeight, tmpData);
                }
            }
        }

        public static string WordWrap(string text, int width)
        {
            int pos, next;
            var sb = new StringBuilder();

            // Lucidity check
            if (width < 1)
                return text;

            // Parse each line of text
            for (pos = 0; pos < text.Length; pos = next)
            {
                // Find end of line
                var eol = text.IndexOf(newline, pos);

                if (eol == -1)
                    next = eol = text.Length;
                else
                    next = eol + newline.Length;

                // Copy this line of text, breaking into smaller lines as needed
                if (eol > pos)
                    do
                    {
                        var len = eol - pos;

                        if (len > width)
                            len = BreakLine(text, pos, width);

                        sb.Append(text, pos, len);
                        sb.Append(newline);

                        // Trim whitespace following break
                        pos += len;

                        while (pos < eol && char.IsWhiteSpace(text[pos]))
                            pos++;
                    }
                    while (eol > pos);
                else
                    sb.Append(newline); // Empty line
            }

            return sb.ToString();
        }

        /// <summary>
        ///     Locates position to break the given line so as to
        ///     avoid breaking words.
        /// </summary>
        /// <param name="text">String that contains line of text</param>
        /// <param name="pos">
        ///     Index where line of <paramref name="text" /> starts
        /// </param>
        /// <param name="max">Maximum line length</param>
        /// <returns>
        ///     The modified line length
        /// </returns>
        public static int BreakLine(string text, int pos, int max)
        {
            // Find last whitespace in line
            var i = max - 1;
            while (i >= 0 && !char.IsWhiteSpace(text[pos + i]))
                i--;

            if (i < 0)
                return max; // No whitespace found; break at maximum length

            // Find start of whitespace
            while (i >= 0 && char.IsWhiteSpace(text[pos + i]))
                i--;

            // Return length of text before whitespace
            return i + 1;
        }

        public static string Split(string text, int maxLineLength)
        {
            return string.Join("\n", CalcualteSplit(text, maxLineLength));
        }

        public static string[] CalcualteSplit(string text, int maxLineLength)
        {
            var list = new List<string>();

            int currentIndex;
            var lastWrap = 0;
            var whitespace = new[] {' ', '\r', '\n', '\t'};
            do
            {
                currentIndex = lastWrap + maxLineLength > text.Length
                    ? text.Length
                    : text.LastIndexOfAny(new[] {' ', ',', '.', '?', '!', ':', ';', '-', '\n', '\r', '\t'},
                          Math.Min(text.Length - 1, lastWrap + maxLineLength)) + 1;
                if (currentIndex <= lastWrap)
                    currentIndex = Math.Min(lastWrap + maxLineLength, text.Length);
                list.Add(text.Substring(lastWrap, currentIndex - lastWrap).Trim(whitespace));
                lastWrap = currentIndex;
            }
            while (currentIndex < text.Length);

            return list.ToArray();
        }

        internal int[] ConvertTextToSprites(string text, string fontName = "Default")
        {
            var total = text.Length;

            var spriteIDs = new int[total];

            char character;

//            var charOffset = 32;
            int spriteID, index;

            // Test to make sure font exists
            if (!fonts.ContainsKey(fontName))
                throw new Exception("Font '" + fontName + "' not found.");

            var fontMap = fonts[fontName];
            var totalCharacters = fontMap.Length;

            for (var i = 0; i < total; i++)
            {
                character = text[i];
                index = Convert.ToInt32(character) - charOffset;
                spriteID = -1;

                if (index < totalCharacters && index > -1)
                    spriteID = fontMap[index];

                spriteIDs[i] = spriteID;
            }

            return spriteIDs;
        }

        public int[] ConvertCharacterToPixelData(char character, string fontName)
        {
            var spriteChip = engine.spriteChip;

            // Test to make sure font exists
            if (!fonts.ContainsKey(fontName))
                throw new Exception("Font '" + fontName + "' not found.");

            var index = Convert.ToInt32(character) - charOffset;

            var fontMap = fonts[fontName];
            var totalCharacters = fontMap.Length;
            var spriteID = -1;

            if (index < totalCharacters && index > -1)
                spriteID = fontMap[index];

            if (spriteID > -1)
            {
                var totalPixels = spriteChip.width * spriteChip.height;

                if (tmpPixels.Length != totalPixels)
                    Array.Resize(ref tmpPixels, totalPixels);

                spriteChip.ReadSpriteAt(spriteID, tmpPixels);

                return tmpPixels;
            }

            return null;
        }

    }

}