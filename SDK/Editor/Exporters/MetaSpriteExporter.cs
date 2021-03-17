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
using System.Text;
using PixelVision8.Runner.Exporters;

namespace PixelVision8.Editor
{
    public class MetaSpriteExporter : AbstractExporter
    {
        private readonly PixelVision targetEngine;
        private StringBuilder sb;
        private GameChip _gameChip;

        public MetaSpriteExporter(string fileName, PixelVision targetEngine) : base(fileName)
        {
            this.targetEngine = targetEngine;

            _gameChip = this.targetEngine.GameChip;

            CalculateSteps();
        }

        public override void CalculateSteps()
        {
            if (_gameChip.TotalMetaSprites() < 1) return;

            base.CalculateSteps();

            // Create a new string builder
            Steps.Add(CreateStringBuilder);


            Steps.Add(MetaSpriteData);

            // Save the final string builder
            Steps.Add(CloseStringBuilder);
        }

        private void MetaSpriteData()
        {
            // var gameChip = targetEngine.GameChip;

            // Save Data
            sb.Append("\"metaSprites\":");

            sb.Append("[");

            JsonUtil.indentLevel++;
            JsonUtil.GetLineBreak(sb, 1);

            // var savedData = gameChip.savedData;

            for (var i = 0; i < _gameChip.TotalMetaSprites(); i++)
            {
                var metaSprite = _gameChip.MetaSprite(i);
                var childrenSprites = metaSprite.Sprites;
                var totalChildrenSprites = childrenSprites.Count;

                if (totalChildrenSprites > 0)
                {
                    sb.Append("{");
                    JsonUtil.indentLevel++;
                    JsonUtil.GetLineBreak(sb, 1);

                    // var item = savedData.ElementAt(i);
                    sb.Append($"\"name\": \"{metaSprite.Name}\",");
                    JsonUtil.GetLineBreak(sb, 1);

                    sb.Append("\"sprites\":[");
                    JsonUtil.indentLevel++;
                    for (var j = 0; j < totalChildrenSprites; j++)
                    {
                        JsonUtil.GetLineBreak(sb, 1);
                        var childSprite = childrenSprites[j];
                        sb.Append("{");
                        sb.Append(
                            $"\"id\":{childSprite.Id},\"x\":{childSprite.X},\"y\":{childSprite.Y},\"flipH\":{childSprite.FlipH.ToString().ToLower()},\"flipV\":{childSprite.FlipV.ToString().ToLower()},\"colorOffset\":{childSprite.ColorOffset}");
                        sb.Append("},");
                    }

                    // Hack to remove the last comma from the sprite list
                    sb.Length -= 1;

                    JsonUtil.GetLineBreak(sb, 1);
                    JsonUtil.indentLevel--;
                    // sb.Append(item.Key);
                    // sb.Append("\": \"");
                    // sb.Append(item.Value);
                    // sb.Append("\"");
                    // if (i > 0)
                    // {
                    //     sb.Append(",");
                    //     JsonUtil.GetLineBreak(sb, 1);
                    // }

                    // TODO need to add all the sprites and end this with the correct comma

                    sb.Append("]");
                    JsonUtil.indentLevel--;
                    JsonUtil.GetLineBreak(sb, 1);
                    sb.Append("},");
                }
            }

            // Hack to remove the last comma
            sb.Length -= 1;

            CurrentStep++;
        }

        private void CreateStringBuilder()
        {
            sb = new StringBuilder();

            sb.Append("{");
            JsonUtil.GetLineBreak(sb, 1);

            JsonUtil.indentLevel++;


            sb.Append("\"GameChip\":");

            JsonUtil.GetLineBreak(sb);
            sb.Append("{");
            JsonUtil.GetLineBreak(sb, 1);

            CurrentStep++;
        }

        private void CloseStringBuilder()
        {
            JsonUtil.indentLevel--;
            JsonUtil.GetLineBreak(sb, 1);
            sb.Append("]");

            JsonUtil.indentLevel--;
            JsonUtil.GetLineBreak(sb, 1);
            sb.Append("}");

            JsonUtil.GetLineBreak(sb);
            sb.Append("}");

            Bytes = Encoding.UTF8.GetBytes(sb.ToString());

            CurrentStep++;
        }
    }
}