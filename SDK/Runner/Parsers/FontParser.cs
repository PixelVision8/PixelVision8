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
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using PixelVision8.Engine.Chips;

namespace PixelVision8.Runner.Parsers
{
    public class FontParser : SpriteParser
    {
        //        private readonly bool autoImport;

        private readonly FontChip fontChip;
        private readonly string name;
        private readonly List<Color> uniqueFontColors = new List<Color>();
        private int[] fontMap;

        public FontParser(IImageParser imageParser, IEngineChips chips) : base(imageParser,
            chips, true, chips.FontChip)
        {
            fontChip = chips.FontChip;
            imageParser.ReadStream();
            name = imageParser.FileName.Split('.').First();
        }

        public override void PrepareSprites()
        {
            base.PrepareSprites();

            fontMap = new int[totalSprites];
            //            base.PreCutOutSprites();
        }

        protected override void PostCutOutSprites()
        {
            fontChip.AddFont(name, fontMap);
            base.PostCutOutSprites();
        }

        public override bool IsEmpty(Color[] pixels)
        {
            // Hack to make sure if the space is empty we still save it
            if (index == 0) return false;

            return base.IsEmpty(pixels);
        }

        protected override void ProcessSpriteData()
        {
            var id = -1;

            // If the sprite chip has unique sprites, try to find an existing sprite first
            if (spriteChip.unique) id = spriteChip.FindSprite(spriteData);

            // If the sprite ID is -1 look for an empty sprite
            if (id == -1) id = spriteChip.NextEmptyID();

            // Add the font character sprite data
            spriteChip.UpdateSpriteAt(id, spriteData);

            // Set the id to the font map
            fontMap[index] = id;
        }


        public override void ConvertColorsToIndexes(int totalColors)
        {
            // Calculate the total number of pixels
            var total = tmpPixels.Length;

            // Adjust the size of the index array to match the pixel
            if (spriteData.Length != total) Array.Resize(ref spriteData, total);

            // Create a tmp color for the loop     
            Color tmpColor;

            var colorIndex = new List<int>();

            // Loop through all the pixels and match up to color references
            for (var i = 0; i < total; i++)
            {
                // Find the current color in the loop
                tmpColor = tmpPixels[i];

                var tmpRefID = -1;

                if (!Equals(tmpColor, maskColor))
                {
                    var test = Equals(tmpColor, maskColor);

                    tmpRefID = uniqueFontColors.IndexOf(tmpColor);

                    if (tmpRefID == -1)
                    {
                        tmpRefID = uniqueFontColors.Count;
                        uniqueFontColors.Add(tmpColor);
                    }
                }

                //                tmpRefID = Equals(tmpColor, maskColor) ? -1 : Array.IndexOf(colorData, tmpColor);

                // Look to see if color is not transparent
                //                if (tmpRefID > -1)
                //                {
                //                    // compare against the color index
                //                    var indexed = colorIndex.IndexOf(tmpRefID);
                //
                //                    // if the color is not found let's index it
                //                    if (indexed == -1)
                //                        if (colorIndex.Count < totalColors)
                //                        {
                //                            // Add the color
                //                            colorIndex.Add(tmpRefID);
                //
                //                            // Update the index
                //                            indexed = colorIndex.Count - 1;
                //                        }
                //
                //                    // if the index is still empty (we ran out of colors, then make transparent)
                //                    if (indexed == -1)
                //                        tmpRefID = -1;
                //                }

                // Update the value in the indexes array
                spriteData[i] = tmpRefID;
            }
        }
    }
}