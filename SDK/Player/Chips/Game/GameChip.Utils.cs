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

using System;
using System.Collections;
using System.Text;

namespace PixelVision8.Player
{
    /// <summary>
    ///     The GameChip represents the foundation of a game class
    ///     with all the logic it needs to work correctly in the PixelVisionEngine.
    ///     The AbstractChip class manages configuring the game when created via the
    ///     chip life-cycle. The engine manages the game's state, the game's own life-cycle and
    ///     serialization/deserialization of the game's data.
    /// </summary>
    public partial class GameChip
    {
        private readonly string newline = "\n";

        private StringBuilder _sb = new StringBuilder();

        /// <summary>
        ///     This allows you to call the TextUtil's WordWrap helper to wrap a string of text to a specified character
        ///     width. Since the FontChip only knows how to render characters as sprites, this can be used to calculate
        ///     blocks of text then each line can be rendered with a DrawText() call.
        /// </summary>
        /// <param name="text">The string of text to wrap.</param>
        /// <param name="width">The width of characters to wrap each line of text.</param>
        /// <returns></returns>
        public string WordWrap(string text, int width)
        {
            if (text == null)
            {
                return "";
            }

            int pos, next;

            // Reset the string builder
            _sb.Clear();

            // Lucidity check
            if (width < 1) return text;

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
                        if (len > width) len = BreakLine(text, pos, width);

                        _sb.Append(text, pos, len);
                        _sb.Append(newline);

                        // Trim whitespace following break
                        pos += len;
                        while (pos < eol && char.IsWhiteSpace(text[pos])) pos++;
                    } while (eol > pos);
                else
                    _sb.Append(newline); // Empty line
            }

            return _sb.ToString();
        }

        /// <summary>
        ///     Locates position to break the given line so as to avoid
        ///     breaking words.
        /// </summary>
        /// <param name="text">String that contains line of text</param>
        /// <param name="pos">Index where line of text starts</param>
        /// <param name="max">Maximum line length</param>
        /// <returns>The modified line length</returns>
        private int BreakLine(string text, int pos, int max)
        {
            // Find last whitespace in line
            var i = max;
            while (i >= 0 && !char.IsWhiteSpace(text[pos + i])) i--;

            // If no whitespace found, break at maximum length
            if (i < 0) return max;

            // Find start of whitespace
            while (i >= 0 && char.IsWhiteSpace(text[pos + i])) i--;

            // Return length of text before whitespace
            return i + 1;
        }

        /// <summary>
        ///     This calls the TextUtil's SplitLines() helper to convert text with line breaks (\n) into a collection of
        ///     lines. This can be used in conjunction with the WordWrap() helper to render large blocks of text line by
        ///     line with the DrawText() API.
        /// </summary>
        /// <param name="str">The string of text to split.</param>
        /// <returns>Returns an array of strings representing each line of text.</returns>
        public string[] SplitLines(string str)
        {
            var lines = str.Split(
                new[] {newline},
                StringSplitOptions.None
            );

            return lines;
        }

        public int[] BitArray(int value)
        {
            var bits = new BitArray(BitConverter.GetBytes(value));

            var intArray = new int[bits.Length];

            bits.CopyTo(intArray, 0);

            return intArray;
        }
    }
}