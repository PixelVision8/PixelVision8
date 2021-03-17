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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PixelVision8.Runner.Exporters;

namespace PixelVision8.Editor
{
    internal class SpriteVector
    {
        public int x;
        public int y;
        public int z;

        public SpriteVector(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }

    public class TilemapJsonExporter : AbstractExporter
    {
        private readonly PixelVision targetEngine;
        private StringBuilder sb;

        public TilemapJsonExporter(string fileName, PixelVision targetEngine) : base(fileName)
        {
            this.targetEngine = targetEngine;
        }

        public override void CalculateSteps()
        {
            base.CalculateSteps();

            // Create a new string builder
            Steps.Add(CreateStringBuilder);

            // TODO need to see if there is a legacy flag
            Steps.Add(SaveMapDataV1);
            // _steps.Add(SaveMapDataV2);

            // _steps.Add(SaveMapDataV1);

            // Save the final string builder
            Steps.Add(CloseStringBuilder);
        }


        private void SaveMapDataV2()
        {
            JsonUtil.compressJson = true;

            var spriteChip = targetEngine.SpriteChip;
            var tilemapChip = targetEngine.TilemapChip;
            var colorChip = targetEngine.ColorChip;
            var gameChip = targetEngine.GameChip;

            var spriteSize = gameChip.SpriteSize();

            // version
            sb.Append("\"version\":");
            sb.Append(2);
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 1);

            // layers start
            sb.Append("\"layers\": [");
            JsonUtil.GetLineBreak(sb, 2);

            var idOffset = 1 + spriteChip.TotalSprites;

            // Layer start
            sb.Append("{");
            JsonUtil.GetLineBreak(sb, 3);

            // type
            sb.Append("\"type\":");
            sb.Append("\"objectgroup\"");
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 3);

            // layers start
            sb.Append("\"objects\": [");
            JsonUtil.GetLineBreak(sb, 4);


            var total = tilemapChip.Total;
            var cols = tilemapChip.Columns;
            var tileCounter = 0;

            for (var i = 0; i < total; i++)
            {
                var pos = Utilities.CalculatePosition(i, cols);

                var tile = gameChip.Tile(pos.X, pos.Y);

                // Only save a tile if it exists
                if (tile.SpriteId > 0 || tile.Flag > 0 || tile.ColorOffset > 0)
                {
                    sb.Append("{");
                    JsonUtil.GetLineBreak(sb, 5);

                    // TODO need to add in flip values to this sprite ID
                    sb.Append("\"gid\":");
                    sb.Append(CreateGID(tile.SpriteId + 1, tile.FlipH, tile.FlipV));
                    sb.Append(",");
                    JsonUtil.GetLineBreak(sb, 5);

                    sb.Append("\"x\":");
                    sb.Append(pos.X * spriteSize.X);
                    sb.Append(",");
                    JsonUtil.GetLineBreak(sb, 5);

                    // Tiled Y pos is 1 based, so offset the x position by 1 tile
                    sb.Append("\"y\":");
                    sb.Append((pos.Y + 1) * spriteSize.Y);
                    sb.Append(",");
                    JsonUtil.GetLineBreak(sb, 5);

                    // layers start
                    sb.Append("\"properties\": [");
                    JsonUtil.GetLineBreak(sb, 6);

                    // Save flag id
                    JsonUtil.GetLineBreak(sb, 7);
                    sb.Append("{");

                    sb.Append("\"name\":");
                    sb.Append("\"flagID\"");
                    sb.Append(",");
                    JsonUtil.GetLineBreak(sb, 7);

                    sb.Append("\"value\":");
                    sb.Append(tile.Flag);

                    // property end
                    JsonUtil.GetLineBreak(sb, 6);
                    sb.Append("},");

                    // Save color offset
                    JsonUtil.GetLineBreak(sb, 7);
                    sb.Append("{");

                    sb.Append("\"name\":");
                    sb.Append("\"colorOffset\"");
                    sb.Append(",");
                    JsonUtil.GetLineBreak(sb, 7);

                    sb.Append("\"value\":");
                    sb.Append(tile.ColorOffset);

                    // property end
                    JsonUtil.GetLineBreak(sb, 6);
                    sb.Append("}");


                    JsonUtil.GetLineBreak(sb, 5);
                    sb.Append("]");
                    JsonUtil.GetLineBreak(sb, 4);

                    sb.Append("}");

                    sb.Append(",");

                    tileCounter++;
                }
            }

            if (tileCounter > 0)
                // Remove the last comma
                sb.Length--;

            // tilesets end
            JsonUtil.GetLineBreak(sb, 3);
            sb.Append("]");

            // Layer end
            JsonUtil.GetLineBreak(sb, 2);
            sb.Append("}");

            // layers end
            JsonUtil.GetLineBreak(sb, 1);
            sb.Append("]");

            StepCompleted();
        }

        private void SaveMapDataV1()
        {
            JsonUtil.compressJson = true;

            var spriteChip = targetEngine.SpriteChip;
            var tilemapChip = targetEngine.TilemapChip;
            var colorChip = targetEngine.ColorChip;
            var gameChip = targetEngine.GameChip;

            var spriteSize = gameChip.SpriteSize();

            // Width
            sb.Append("\"width\":");
            sb.Append(tilemapChip.Columns);
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 1);

            // Height
            sb.Append("\"height\":");
            sb.Append(tilemapChip.Rows);
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 1);

            // nextobjectid
            sb.Append("\"nextobjectid\":");
            sb.Append(1);
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 1);

            // orientation
            sb.Append("\"orientation\":");
            sb.Append("\"orthogonal\"");
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 1);

            // renderorder
            sb.Append("\"renderorder\":");
            sb.Append("\"right-down\"");
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 1);

            // tiledversion
            sb.Append("\"tiledversion\":");
            sb.Append("\"1.0.3\"");
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 1);

            // tilewidth
            sb.Append("\"tilewidth\":");
            sb.Append(spriteSize.X);
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 1);

            // tileheight
            sb.Append("\"tileheight\":");
            sb.Append(spriteSize.Y);
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 1);

            // type
            sb.Append("\"type\":");
            sb.Append("\"map\"");
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 1);

            // version
            sb.Append("\"version\":");
            sb.Append(1);
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 1);

            // background color
            sb.Append("\"backgroundcolor\":");
            sb.Append("\"" + colorChip.MaskColor + "\"");
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 1);

            sb.Append("\"tilesets\": [");
            JsonUtil.GetLineBreak(sb, 2);

            var sheets = new Dictionary<string, SpriteVector>
            {
                {
                    "sprites.png",
                    new SpriteVector(spriteChip.TextureWidth, spriteChip.TextureHeight, spriteChip.TotalSprites)
                }
            };

            var totalSheets = sheets.Count;
            var index = 0;
            var offset = 1;

            foreach (var entry in sheets)
            {
                sb.Append("{");
                JsonUtil.GetLineBreak(sb, 2);

                // columns
                sb.Append("\"columns\":");
                sb.Append(spriteChip.TextureWidth / spriteSize.X);
                sb.Append(",");
                JsonUtil.GetLineBreak(sb, 2);

                // firstgid
                sb.Append("\"firstgid\":");
                sb.Append(offset);
                sb.Append(",");
                JsonUtil.GetLineBreak(sb, 2);

                // image
                sb.Append("\"image\":");
                // TODO need to test if this is going to be loaded from the cache or the sprite sheet itself
                sb.Append("\"" + entry.Key + "\"");
                sb.Append(",");
                JsonUtil.GetLineBreak(sb, 2);

                // imagewidth
                sb.Append("\"imagewidth\":");
                sb.Append(entry.Value.x);
                sb.Append(",");
                JsonUtil.GetLineBreak(sb, 2);

                // imageheight
                sb.Append("\"imageheight\":");
                sb.Append(entry.Value.y);
                sb.Append(",");
                JsonUtil.GetLineBreak(sb, 2);

                // margin
                sb.Append("\"margin\":");
                sb.Append(0);
                sb.Append(",");
                JsonUtil.GetLineBreak(sb, 2);

                // name
                sb.Append("\"name\":");
                sb.Append("\"" + entry.Key.Split('.').First() + "\"");
                sb.Append(",");
                JsonUtil.GetLineBreak(sb, 2);

                // spacing
                sb.Append("\"spacing\":");
                sb.Append(0);
                sb.Append(",");
                JsonUtil.GetLineBreak(sb, 2);

                // tilewidth
                sb.Append("\"tilewidth\":");
                sb.Append(spriteSize.X);
                sb.Append(",");
                JsonUtil.GetLineBreak(sb, 2);

                // tileheight
                sb.Append("\"tileheight\":");
                sb.Append(spriteSize.Y);
                sb.Append(",");
                JsonUtil.GetLineBreak(sb, 2);

                // tilecount
                sb.Append("\"tilecount\":");
                sb.Append(entry.Value.z);
                sb.Append(",");
                JsonUtil.GetLineBreak(sb, 2);

                // transparentcolor
                sb.Append("\"transparentcolor\":");
                sb.Append("\"" + targetEngine.ColorChip.MaskColor + "\"");
                JsonUtil.GetLineBreak(sb, 2);

                // tilesets end
                JsonUtil.GetLineBreak(sb, 2);
                sb.Append("}");

                if (index < totalSheets - 1 && totalSheets > 1) sb.Append(",");

                offset += entry.Value.z;
                index++;
            }

            JsonUtil.GetLineBreak(sb, 1);
            sb.Append("],");

            // layers start
            sb.Append("\"layers\": [");
            JsonUtil.GetLineBreak(sb, 2);

            var idOffset = 1 + spriteChip.TotalSprites;

            // Layer start
            sb.Append("{");
            JsonUtil.GetLineBreak(sb, 3);

            // name
            sb.Append("\"draworder\":");
            sb.Append("\"topdown\"");
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 3);

            sb.Append("\"name\":");
            sb.Append("\"Tilemap\"");
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 3);

            // width
            sb.Append("\"id\":");
            sb.Append(1);
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 3);

            // type
            sb.Append("\"type\":");
            sb.Append("\"objectgroup\"");
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 3);

            // opacity
            sb.Append("\"opacity\":");
            sb.Append(1);
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 3);

            // visible
            sb.Append("\"visible\":");
            sb.Append("true");
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 3);

            // x
            sb.Append("\"x\":");
            sb.Append(0);
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 3);

            // y
            sb.Append("\"y\":");
            sb.Append(0);
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 3);

            // layers start
            sb.Append("\"objects\": [");
            JsonUtil.GetLineBreak(sb, 4);


            var total = tilemapChip.Total;
            var cols = tilemapChip.Columns;
            var tileCounter = 0;

            for (var i = 0; i < total; i++)
            {
                var pos = Utilities.CalculatePosition(i, cols);

                var tile = gameChip.Tile(pos.X, pos.Y);

                // Only save a tile if it exists
                if (tile.SpriteId + tile.Flag >= -1)
                {
                    sb.Append("{");
                    JsonUtil.GetLineBreak(sb, 5);

                    sb.Append("\"name\":");
                    sb.Append("\"Tile:" + pos.X + "," + pos.Y + "\"");
                    sb.Append(",");
                    JsonUtil.GetLineBreak(sb, 5);

                    // TODO need to add in flip values to this sprite ID
                    sb.Append("\"gid\":");
                    sb.Append(CreateGID(tile.SpriteId + 1, tile.FlipH, tile.FlipV));
                    sb.Append(",");
                    JsonUtil.GetLineBreak(sb, 5);

                    sb.Append("\"width\":");
                    sb.Append(spriteSize.X);
                    sb.Append(",");
                    JsonUtil.GetLineBreak(sb, 5);

                    sb.Append("\"height\":");
                    sb.Append(spriteSize.Y);
                    sb.Append(",");
                    JsonUtil.GetLineBreak(sb, 5);


                    sb.Append("\"x\":");
                    sb.Append(pos.X * spriteSize.X);
                    sb.Append(",");
                    JsonUtil.GetLineBreak(sb, 5);

                    // Tiled Y pos is 1 based, so offset the x position by 1 tile
                    sb.Append("\"y\":");
                    sb.Append((pos.Y + 1) * spriteSize.Y);
                    sb.Append(",");
                    JsonUtil.GetLineBreak(sb, 5);

                    // layers start
                    sb.Append("\"properties\": [");
                    JsonUtil.GetLineBreak(sb, 6);

                    // Save flag id
                    JsonUtil.GetLineBreak(sb, 7);
                    sb.Append("{");

                    sb.Append("\"name\":");
                    sb.Append("\"flagID\"");
                    sb.Append(",");
                    JsonUtil.GetLineBreak(sb, 7);

                    sb.Append("\"type\":");
                    sb.Append("\"int\"");
                    sb.Append(",");
                    JsonUtil.GetLineBreak(sb, 7);

                    sb.Append("\"value\":");
                    sb.Append(tile.Flag);

                    // property end
                    JsonUtil.GetLineBreak(sb, 6);
                    sb.Append("},");


                    // Save color offset
                    JsonUtil.GetLineBreak(sb, 7);
                    sb.Append("{");

                    sb.Append("\"name\":");
                    sb.Append("\"colorOffset\"");
                    sb.Append(",");
                    JsonUtil.GetLineBreak(sb, 7);

                    sb.Append("\"type\":");
                    sb.Append("\"int\"");
                    sb.Append(",");
                    JsonUtil.GetLineBreak(sb, 7);

                    sb.Append("\"value\":");
                    sb.Append(tile.ColorOffset);

                    // property end
                    JsonUtil.GetLineBreak(sb, 6);
                    sb.Append("}");


                    JsonUtil.GetLineBreak(sb, 5);
                    sb.Append("]");
                    JsonUtil.GetLineBreak(sb, 4);

                    sb.Append("}");

                    sb.Append(",");

                    tileCounter++;
                }
            }

            //            sb.Append(String.Join("", new List<int>(layers[(int)Layer.Sprites]).ConvertAll(i => i.ToString()).ToArray()));

            //                var layerEnum = (TilemapChip.Layer) Enum.Parse(typeof(TilemapChip.Layer), layerName);
            //                    
            //                // Need to join the layer array and add 1 to the sprite ID since tiled isn't 
            //                sb.Append(string.Join(",", tilemapChip.layers[(int)layerEnum].Select(x => (x == -1 ? 0 : x + idOffset).ToString())));

            if (tileCounter > 0)
                // Remove the last comma
                sb.Length--;

            // tilesets end
            JsonUtil.GetLineBreak(sb, 3);
            sb.Append("]");

            // Layer end
            JsonUtil.GetLineBreak(sb, 2);
            sb.Append("}");

            // layers end
            JsonUtil.GetLineBreak(sb, 1);
            sb.Append("]");

            CurrentStep++;
        }

        private void CreateStringBuilder()
        {
            sb = new StringBuilder();
            sb.Append("{");

            JsonUtil.GetLineBreak(sb, 1);

            CurrentStep++;
        }

        private void CloseStringBuilder()
        {
            JsonUtil.indentLevel--;
            JsonUtil.GetLineBreak(sb);
            sb.Append("}");

            Bytes = Encoding.UTF8.GetBytes(sb.ToString());

            CurrentStep++;
        }

        public uint CreateGID(int id, bool flipH, bool flipV)
        {
            var gid = (uint) id;

            if (flipH) gid |= 1U << 31;

            if (flipV) gid |= 1U << 30;

            return gid;
        }
    }
}