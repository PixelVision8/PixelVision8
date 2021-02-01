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

using PixelVision8.Player;
using PixelVision8.Runner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PixelVision8.Runner.Exporters
{
    /// <summary>
    ///     Leverage the built in sprite parser to do the cutting up and indexing work for us
    /// </summary>
    ///  TODO this needs to extend Sprite Data Parser
    internal class SpriteDataParser : SpriteImageParser
    {
        public int[] ids;
        public int totalSpritesInTexture;

        public SpriteDataParser(IImageParser parser, ColorChip colorChip, SpriteChip spriteChip) : base("", parser,
            colorChip, spriteChip)
        {
        }

        public override void PrepareSprites()
        {
            // Get the total number of sprites
            //TODO this needs to be double checked at different size sprites
            var cols = (int)Math.Floor((double)ImageData.Width / SpriteChip.DefaultSpriteSize);
            var rows = (int)Math.Floor((double)ImageData.Height / SpriteChip.DefaultSpriteSize);
            totalSpritesInTexture = cols * rows;

            ids = Enumerable.Repeat(-1, totalSpritesInTexture).ToArray();

            base.PrepareSprites();
        }

        protected override void ProcessSpriteData()
        {
            // Save the index to the ids array
            ids[index] = spriteChip.FindSprite(spriteData, true);
        }
    }

    public class SpriteBuilderExporter : AbstractExporter
    {
        private readonly string endComment = "-- spritelib-end";

        private readonly Dictionary<string, byte[]> files;

        private readonly List<SpriteExportData> sprites = new List<SpriteExportData>();

        private readonly string startComment = "-- spritelib-start";

        private int currentTile;
        private int maxTilesPerLoop;

        public int spriteCount;
        private int totalTiles;
        private SpriteChip spriteChip;
        private ColorChip colorChip;

        public SpriteBuilderExporter(string fileName, ColorChip colorChip, SpriteChip spriteChip,
            Dictionary<string, byte[]> files) : base(fileName)
        {
            this.files = files;
            this.spriteChip = spriteChip;
            this.colorChip = colorChip;
        }

        public override void CalculateSteps()
        {
            base.CalculateSteps();

            spriteCount = 0;

            Steps.Add(ConvertFilesToTextures);

            maxTilesPerLoop = 10;
            totalTiles = files.Count;

            var loops = (int) Math.Ceiling((float) totalTiles / maxTilesPerLoop);

            for (var i = 0; i < loops; i++) Steps.Add(ParseSpriteData);

            Steps.Add(GenerateSpriteJSON);
        }


        private void ConvertFilesToTextures()
        {
            foreach (var file in files)
                // Add the sprite data to the list
                sprites.Add(new SpriteExportData(file.Key, file.Value));

            CurrentStep++;
        }


        private void ParseSpriteData()
        {
            for (var i = 0; i < maxTilesPerLoop; i++)
            {
                var spriteData = sprites[currentTile];

                var spriteParser = new SpriteDataParser(spriteData.imageParser, colorChip, spriteChip);

                spriteParser.CalculateSteps();

                while (spriteParser.Completed == false) spriteParser.NextStep();

                Array.Copy(spriteParser.ids, spriteData.ids, spriteParser.ids.Length);

                currentTile++;

                if (currentTile >= totalTiles) break;
            }

            CurrentStep++;
        }


        public void GenerateSpriteJSON()
        {
            spriteCount = 0;

            var sb = new StringBuilder();
            sb.Append(startComment);

            sb.AppendLine();

            var forbiddenChars = @"+-*/^=~<>(){}[];:,.".ToCharArray();
            var names = new List<string>();


            foreach (var sprite in sprites)
            {
                var name = sprite.fileName; //Path.GetFileNameWithoutExtension(file);

                // make sure we don't have duplicates
                if (names.IndexOf(name) == -1)
                {
                    // Test to make sure the sprite isn't empty
                    var total = sprite.ids.Length;
                    var emptyIDs = -1;
                    for (var i = 0; i < total; i++)
                        if (sprite.ids[i] == -1)
                            emptyIDs++;

                    // If all of the ids are -1 the spriteData is empty
                    var isEmpty = emptyIDs == total - 1;

                    if (!isEmpty)
                    {
                        sb.Append(new string(name.Where(c => !forbiddenChars.Contains(c)).ToArray()));
                        sb.Append("={");
                        sb.Append("width=" + sprite.width);
                        sb.Append(",");
                        sb.Append("spriteIDs={");

                        sb.Append(string.Join(",", sprite.ids.Select(element => element.ToString()).ToArray()));

                        sb.Append("}");
                        sb.Append("}");
                        sb.AppendLine();
                        spriteCount++;

                        names.Add(name);
                    }
                }
            }

            sb.Append(endComment);
            sb.AppendLine();

            Bytes = Encoding.UTF8.GetBytes(sb.ToString());

            CurrentStep++;
        }
    }
}