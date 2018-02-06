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
using PixelVisionSDK.Chips;
using PixelVisionSDK.Utils;
using PixelVisionSDK;

namespace PixelVisionRunner
{

    public class ImportUtil
    {

        /// <summary>
        ///     Finds the unique colors in a texture and returns
        ///     an array of them
        /// </summary>
        /// <param name="src"></param>
        /// <returns>
        /// </returns>
        public static IColor[] IndexColorsFromTexture(ITexture2D src, IColor magenta, bool unique = true, bool ignoreTransparent = true)
        {
            // Get the total pixels from the texture
            var pixels = src.GetPixels();
            var total = pixels.Length;

            // Create a new list to store the unique colors
            var colors = new List<IColor>();

            // tmp color used in the loop
            IColor tmpColor;

            int x, y;
            var width = src.width;
            var height = src.height;

            // Loop through each color and find the unique ones
            for (var i = 0; i < total; i++)
            {
                //PosUtil.CalculatePosition(i, width, out x, out y);

                x = i % width;
                y = i / width;

                y = height - y - 1;

                // Get the current color
                tmpColor = src.GetPixel(x, y); //pixels[i]);

                if (tmpColor.a < 1 && !ignoreTransparent)
                    tmpColor = magenta;

                // Look to see if the color is already in the list
                if (!colors.Contains(tmpColor) && unique)
                    colors.Add(tmpColor);
                else if (unique == false)
                    colors.Add(tmpColor);
            }

            // Return an array of the color list
            return colors.ToArray();
        }

        /// <summary>
        ///     Imports unique colors out of a texture. You can chose to
        ///     <see langword="override" /> the existing colors or add to them.
        /// </summary>
        /// <param name="src"></param>
        /// <param name="colorChip"></param>
        /// <param name="rebuild"></param>
//        public static void ImportColorsFromTexture(Texture2D src, IEngineChips chips, bool rebuild = true,
//            bool saveAsDeafult = true)
//        {
//            var colorChip = chips.colorChip;
//
//            // Get the unique colors from a texture
//            var indexedColors = IndexColorsFromTexture(src, false, false);
//
//            // Get the total colors loaded
//            var total = indexedColors.Length;
//
//            //Debug.Log("Total Colors Imported "+ total);
//            // Clear the colors first
//            colorChip.Clear();
//
//            // Update the color chip to support the number of colors found
//            colorChip.RebuildColorPages(total);
//
//            // Loop through new colors and add them to the chip
//            for (var i = 0; i < total; i++)
//            {
//                var tmpColor = indexedColors[i];
//                var hex = ColorData.ColorToHex(tmpColor.r, tmpColor.g, tmpColor.b);
//
//                colorChip.UpdateColorAt(i, hex);
//            }
//
//            // Update supported colors based on what was imported
//            colorChip.RecalculateSupportedColors();
//        }
//
//        /// <summary>
//        ///     Imports sprites form a texture. As the sprites are imported their
//        ///     colors are indexed and remapped to values the engine can use to map
//        ///     to the color chip. This means if you have more colors then the color
//        ///     chip can support they will be ignored.
//        /// </summary>
//        /// <param name="src"></param>
//        /// <param name="spriteChip"></param>
//        /// <param name="resizeRam"></param>
//        /// <param name="unique"></param>
//        public static void ImportSpritesFromTexture(Texture2D src, IEngineChips chips, bool resizeRam = false,
//            bool unique = true)
//        {
//            src.filterMode = FilterMode.Point;
//
//            var spriteChip = chips.spriteChip;
//            var sWidth = spriteChip.width;
//            var sHeight = spriteChip.height;
//
//            // Set up colors array
//            ColorData[] colorData;
//
//            // Test for color map, if not index the texture
//            var colorMapChipName = typeof(ColorMapChip).FullName;
//            if (chips.chipManager.HasChip(colorMapChipName))
//                colorData = ((ColorMapChip) chips.chipManager.GetChip(typeof(ColorMapChip).FullName)).colors;
//            else
//                colorData = chips.colorChip.colors;
//
//            var colors = new Color[colorData.Length];
//            ApplyColors(ref colors, colorData);
//
//            // Calculate values needed to cut out sprites
//            var srcWidth = src.width;
//            var srcHeight = src.height;
//
//            //var spriteSize = spriteChip.spriteSize;
//            var totalSprites = SpriteChipUtil.CalcualteTotalSprites(srcWidth, srcHeight, sWidth, sHeight);
//            var maxSprites = SpriteChipUtil.CalcualteTotalSprites(spriteChip.textureWidth, spriteChip.textureHeight,
//                sWidth, sHeight);
//
//            // Create tmp arrays for color and reference data
//            var totalPixels = spriteChip.width * spriteChip.height;
//            var tmpPixels = new Color[totalPixels];
//            var spriteData = new int[totalPixels];
//
//            // Keep track of number of sprites added
//            var spritesAdded = 0;
//
//            //var spriteCache = new List<string>();
//
//            // Loop through all the potential sprites and import them
//            for (var i = 0; i < totalSprites; i++)
//            {
//                // Cut out the sprite from the texture
//                tmpPixels = CutOutSpriteFromTexture2D(i, src, sWidth, sHeight);
//
//                // Convert sprite to color index
//                ConvertColorsToIndexes(ref spriteData, tmpPixels, colors, spriteChip.colorsPerSprite);
//
//                if (spritesAdded < maxSprites)
//                    if (!SpriteChipUtil.IsEmpty(spriteData))
//                        if (unique)
//                        {
//                            if (spriteChip.FindSprite(spriteData) == -1)
//                            {
//                                spriteChip.UpdateSpriteAt(spritesAdded, spriteData);
//                                spritesAdded++;
//                            }
//                        }
//                        else
//                        {
//                            spriteChip.UpdateSpriteAt(i, spriteData);
//                            spritesAdded++;
//                        }
//            }
//        }

        /// <summary>
        ///     Cut out a sprite from a texture
        /// </summary>
        /// <param name="index"></param>
        /// <param name="texture2d"></param>
        /// <param name="spriteSize"></param>
        /// <returns>
        /// </returns>
        public static IColor[] CutOutSpriteFromTexture2D(int index, ITexture2D texture2d, int spriteWidth,
            int spriteHeight)
        {
            int x, y;

            SpriteChipUtil.CalculateSpritePos(index, texture2d.width, texture2d.height, spriteWidth, spriteHeight, out x,
                out y);

            return texture2d.GetPixels(x, y, spriteWidth, spriteHeight);
        }

        public static void ConvertColorsToIndexes(ref int[] indexes, IColor[] pixels, IColor[] refColors, int totalColors)
        {
            // Calculate the total number of pixels
            var total = pixels.Length;

            // Adjust the size of the index array to match the pixel
            if (indexes.Length != total)
                Array.Resize(ref indexes, total);

            // Create a tmp color for the loop
            IColor tmpColor;
            int tmpRefID;

            var colorIndex = new List<int>();

            // Loop through all the pixels and match up to color references
            for (var i = 0; i < total; i++)
            {
                // Find the current color in the loop
                tmpColor = pixels[i];

                // if color is transparent set to -1, if not try to look up in the ref colors array
                tmpRefID = Array.IndexOf(refColors, tmpColor);

                // Look to see if color is not transparent
                if (tmpRefID > -1)
                {
                    // compare against the color index
                    var indexed = colorIndex.IndexOf(tmpRefID);

                    // if the color is not found let's index it
                    if (indexed == -1)
                        if (colorIndex.Count < totalColors)
                        {
                            // Add the color
                            colorIndex.Add(tmpRefID);

                            // Update the index
                            indexed = colorIndex.Count - 1;
                        }

                    // if the index is still empty (we ran out of colors, then make transparent)
                    if (indexed == -1)
                        tmpRefID = -1;
                }

                // Update the value in the indexes array
                indexes[i] = tmpRefID;
            }
        }

        /// <summary>
        ///     Quickly convert sprite data into a string to make it easier to
        ///     compare
        /// </summary>
        /// <param name="data"></param>
        /// <returns>
        /// </returns>
        public static void ImportTileMapFromTexture(ITexture2D src, IEngineChips chips, bool autoImport = true
        )
        {
            var spriteChip = chips.spriteChip;
            var tileMapChip = chips.tilemapChip;

            // Set up colors array

            var colorData = chips.colorMapChip != null ? chips.colorMapChip.colors : chips.colorChip.colors;
            var colors = new IColor[colorData.Length];
            ApplyColors(ref colors, colorData);

            //var sSize = spriteChip.spriteSize;

            var sWidth = spriteChip.width;
            var sHeight = spriteChip.height;

            var width = MathUtil.CeilToInt(src.width / sWidth);
            var height = MathUtil.CeilToInt(src.height / sHeight);

            // Make sure index is in range or create a new screen block
            tileMapChip.Resize(width, height);

            var totalTiles = width * height;
            var totalPixels = sWidth * sHeight;
            var colorRefs = new int[totalPixels];
            IColor[] pixels;
            int x, y;
            for (var i = 0; i < totalTiles; i++)
            {
                //TODO need a new way to convert textures into texture data
                pixels = CutOutSpriteFromTexture2D(i, src, sWidth, sHeight);

                ConvertColorsToIndexes(ref colorRefs, pixels, colors, spriteChip.colorsPerSprite);

                //PaletteUtil.ConvertColorsToIndexes(ref colorRefs, pixels, colorChip);

                //var spriteString = SpriteChip.SpriteDataToString(colorRefs);
                var id = spriteChip.FindSprite(colorRefs);

                //var spriteID = spriteCache.IndexOf(spriteString);
                if (id == -1 && autoImport)
                {
                    //spriteCache.Add(spriteString);
                    id = spriteChip.NextEmptyID(); //spriteCache.Count - 1;
                    spriteChip.UpdateSpriteAt(id, colorRefs);
                }

                //PosUtil.CalculatePosition(i, width, out x, out y);
                x = i % width;
                y = i / width;
                tileMapChip.UpdateTileAt(id, x, y);
            }
        }

        public static void ImportFlagsFromTexture(ITexture2D src, IEngineChips chips)
        {
            var spriteChip = chips.spriteChip;
            var tileMap = chips.tilemapChip;

            //TODO need to remap these since they are bottom left aligned

            //var sSize = spriteChip.spriteSize;

            var sWidth = spriteChip.width;
            var sHeight = spriteChip.height;

            var columns = MathUtil.CeilToInt(src.width / sWidth);
            var rows = MathUtil.CeilToInt(src.height / sHeight);

            if (columns != tileMap.columns || rows != tileMap.rows)
                return;

            var total = SpriteChipUtil.CalculateTotalSprites(src.width, src.height, sWidth, sHeight);
            int x, y;
            var tmpWidth = tileMap.columns;

            for (var i = 0; i < total; i++)
            {
                var pixels = CutOutSpriteFromTexture2D(i, src, sWidth, sHeight);
                var color = pixels[0];

                //PosUtil.CalculatePosition(i, tileMap.columns, out x, out y);
                x = i % tmpWidth;
                y = i / tmpWidth;

                //Debug.Log(color.r);
                var flag = color.a == 1 ? (int) (color.r * 256) / tileMap.totalFlags : -1;

                tileMap.UpdateFlagAt(x, y, flag);
            }
        }

        public static void ImportColorMapFromTexture(ITexture2D src, IEngineChips engine, IColor magenta)
        {
            // Create a new color map chip to store data
            var colorMapChip = new ColorMapChip();

            engine.chipManager.ActivateChip(colorMapChip.GetType().FullName, colorMapChip);

            // Load Texture
            var mapColors = IndexColorsFromTexture(src, magenta, true, false);

            //var colors = MapColors(mapColors, engine.colorChip.colors);
            var totalColors = mapColors.Length;

            colorMapChip.total = totalColors;
            for (var i = 0; i < totalColors; i++)
            {
                var tmpColor = mapColors[i];
                var hex = ColorData.ColorToHex(tmpColor.r, tmpColor.g, tmpColor.b);

                colorMapChip.UpdateColorAt(i, hex);
            }
        }

        public static void ImportFontFromTexture(ITexture2D src, IEngineChips chips, string name = "Default",
            bool autoImport = true)
        {
            if (src == null)
                throw new Exception("Source texture was null, can't import font.");

            var spriteChip = chips.spriteChip;
            var fontChip = chips.fontChip;

            if (fontChip == null)
            {
                // Create a new color map chip to store data
                fontChip = new FontChip();

                chips.chipManager.ActivateChip(fontChip.GetType().FullName, fontChip);
            }

            // Set up colors array
            var colorData = chips.colorMapChip != null ? chips.colorMapChip.colors : chips.colorChip.colors;
            var colors = new IColor[colorData.Length];
            ApplyColors(ref colors, colorData);

            var sWidth = spriteChip.width;
            var sHeight = spriteChip.height;

            var width = MathUtil.CeilToInt(src.width / sWidth);
            var height = MathUtil.CeilToInt(src.height / sHeight);

            var totalTiles = width * height;
            var totalPixels = sWidth * sHeight;
            var colorRefs = new int[totalPixels];
            IColor[] pixels;

            var fontMap = new int[totalTiles];

            for (var i = 0; i < totalTiles; i++)
            {
                //TODO need a new way to convert textures into texture data
                pixels = CutOutSpriteFromTexture2D(i, src, sWidth, sHeight);

                ConvertColorsToIndexes(ref colorRefs, pixels, colors, spriteChip.colorsPerSprite);

                var id = spriteChip.FindSprite(colorRefs);

                if (id == -1 && autoImport)
                {
                    id = spriteChip.NextEmptyID();
                    spriteChip.UpdateSpriteAt(id, colorRefs);
                }

                fontMap[i] = id;
            }

            fontChip.AddFont(name, fontMap);
        }

        public static void ApplyColor(ref IColor c1, ColorData c2)
        {
            c1.r = c2.r;
            c1.g = c2.g;
            c1.b = c2.b;
            c1.a = 1;
        }

        public static void ApplyColors(ref IColor[] c1, ColorData[] c2)
        {
            var total1 = c1.Length;
            var total2 = c2.Length;
            if (total1 != total2)
                return;

            for (var i = 0; i < total1; i++)
                ApplyColor(ref c1[i], c2[i]);
        }

    }

}