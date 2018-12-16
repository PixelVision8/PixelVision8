﻿//
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
using PixelVisionRunner.Utils;
using PixelVisionSDK;
using PixelVisionSDK.Chips;
using PixelVisionSDK.Utils;

namespace PixelVisionRunner.Exporters
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
        private IEngine targetEngine;
        private StringBuilder sb;
        
        public TilemapJsonExporter(string fileName, IEngine targetEngine) : base(fileName)
        {
            this.targetEngine = targetEngine;
            
        }

        public override void CalculateSteps()
        {
            base.CalculateSteps();
            
            // Create a new string builder
            steps.Add(CreateStringBuilder);
            
            
            steps.Add(SaveGameData);
            
            // Save the final string builder
            steps.Add(CloseStringBuilder);
        }
        
        private void SaveGameData()
        {
            var spriteChip = targetEngine.spriteChip;
            var tilemapChip = targetEngine.tilemapChip;
            var colorChip = targetEngine.colorChip;
            var gameChip = targetEngine.gameChip;

            var spriteSize = gameChip.SpriteSize();
            
            // Width
            sb.Append("\"width\":");
            sb.Append(tilemapChip.columns);
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 1);    
                
            // Height
            sb.Append("\"height\":");
            sb.Append(tilemapChip.rows);
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
            sb.Append(spriteSize.x);
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 1); 
            
            // tileheight
            sb.Append("\"tileheight\":");
            sb.Append(spriteSize.y);
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
            sb.Append("\""+ colorChip.maskColor + "\"");
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 1);


// tilesets start
            sb.Append("\"tilesets\": [");
            JsonUtil.GetLineBreak(sb, 2);
            
            var sheets = new Dictionary<string, SpriteVector>()
            {
                {"sprites.png", new SpriteVector(spriteChip.texture.width, spriteChip.texture.height, spriteChip.totalSprites)},
//                {"flags.png", new SpriteVector(8, MathUtil.CeilToInt((float) tilemapChip.totalFlags / 8), tilemapChip.totalFlags)}
            };
            
            var totalSheets = sheets.Count;
            var index = 0;
            var offset = 1;
            
            foreach(KeyValuePair<string, SpriteVector> entry in sheets)
            {
                
                sb.Append("{");
                JsonUtil.GetLineBreak(sb, 2);
                
                // columns
                sb.Append("\"columns\":");
                sb.Append(spriteChip.texture.width / spriteSize.x);
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
                sb.Append("\""+entry.Key+"\"");
                sb.Append(",");
                JsonUtil.GetLineBreak(sb, 2);
                
                // imagewidth
                sb.Append("\"imagewidth\":");
                sb.Append((int)entry.Value.x);
                sb.Append(",");
                JsonUtil.GetLineBreak(sb, 2);
                
                // imageheight
                sb.Append("\"imageheight\":");
                sb.Append((int)entry.Value.y);
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
                sb.Append(spriteSize.x);
                sb.Append(",");
                JsonUtil.GetLineBreak(sb, 2);
                
                // tileheight
                sb.Append("\"tileheight\":");
                sb.Append(spriteSize.y);
                sb.Append(",");
                JsonUtil.GetLineBreak(sb, 2);
                
                // tilecount
                sb.Append("\"tilecount\":");
                sb.Append((int)entry.Value.z);
                sb.Append(",");
                JsonUtil.GetLineBreak(sb, 2);
                
                // transparentcolor
                sb.Append("\"transparentcolor\":");
                sb.Append("\""+targetEngine.colorChip.maskColor+"\"");
                JsonUtil.GetLineBreak(sb, 2);
                
                // tilesets end
                JsonUtil.GetLineBreak(sb, 2);
                sb.Append("}");
                    
                if (index < totalSheets - 1 && totalSheets > 1)
                {
                    sb.Append(",");
                }

                offset += (int)entry.Value.z;
                index++;

            }
                
            JsonUtil.GetLineBreak(sb, 1);
            sb.Append("],");    
            
            // layers start
            sb.Append("\"layers\": [");
            JsonUtil.GetLineBreak(sb, 2);
            
            
            
            // TODO need to do this as a loop


//            var list = new List<string>
//            {
//                TilemapChip.Layer.Sprites.ToString(),
////                TilemapChip.Layer.Flags.ToString(),
////                Layer.Palettes.ToString()
//            };

//            var totalLayers = list.Count;
            
//            for (int i = 0; i < totalLayers; i++)
//            {

//                var layerName = list[i];

                var idOffset = 1 + spriteChip.totalSprites;
                
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
                
                // height
//                sb.Append("\"height\":");
//                sb.Append(tilemapChip.rows);
//                sb.Append(",");
//                JsonUtil.GetLineBreak(sb, 3);
                
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
            
            


            var total = tilemapChip.total;
            var cols = tilemapChip.columns;
            var tileCounter = 0;
            
            for (int i = 0; i < total; i++)
            {

                var pos = gameChip.CalculatePosition(i, cols);
                
                var tile = gameChip.Tile(pos.x, pos.y);
                
                // Only save a tile if it exists
                if (tile.spriteID > -1)
                {
                    sb.Append("{");
                    JsonUtil.GetLineBreak(sb, 5);
                
                    sb.Append("\"id\":");
                    sb.Append(tileCounter);
                    sb.Append(",");
                    JsonUtil.GetLineBreak(sb, 5);
                    
                    sb.Append("\"name\":");
                    sb.Append("\"Tile:"+pos.x+","+pos.y+"\"");
                    sb.Append(",");
                    JsonUtil.GetLineBreak(sb, 5);
                    
                    sb.Append("\"type\":");
                    sb.Append("\""+tile.spriteID+"\"");
                    sb.Append(",");
                    JsonUtil.GetLineBreak(sb, 5);
                    
                    // TODO need to add in flip values to this sprite ID
                    sb.Append("\"gid\":");
                    sb.Append(tile.spriteID + 1);
                    sb.Append(",");
                    JsonUtil.GetLineBreak(sb, 5);
                    
                    sb.Append("\"width\":");
                    sb.Append(spriteSize.x);
                    sb.Append(",");
                    JsonUtil.GetLineBreak(sb, 5);
                    
                    sb.Append("\"height\":");
                    sb.Append(spriteSize.y);
                    sb.Append(",");
                    JsonUtil.GetLineBreak(sb, 5);
                    
                    
                    sb.Append("\"x\":");
                    sb.Append(pos.x * spriteSize.x);
                    sb.Append(",");
                    JsonUtil.GetLineBreak(sb, 5);
                    
                    sb.Append("\"y\":");
                    sb.Append((pos.y + 1) * spriteSize.y);  // Tiled Y pos is 1 based, so offset the x position by 1 tile
                    sb.Append(",");
                    JsonUtil.GetLineBreak(sb, 5);
                    
                    sb.Append("\"rotation\":");
                    sb.Append(0);
                    sb.Append(",");
                    JsonUtil.GetLineBreak(sb, 5);
                    
                    // visible
                    sb.Append("\"visible\":");
                    sb.Append("true");
                    sb.Append(",");
                    JsonUtil.GetLineBreak(sb, 5);
                    
                    // layers start
                    sb.Append("\"properties\": [");
                    JsonUtil.GetLineBreak(sb, 6);
                    
                    // Save flag id
                    
                    // 
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
                    sb.Append(tile.flag);
                    
                    // property end
                    JsonUtil.GetLineBreak(sb, 6);
                    sb.Append("},");
                    
                    
                    // Save flag id
                    
                    // 
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
                    sb.Append(tile.colorOffset);
                    
                    // property end
                    JsonUtil.GetLineBreak(sb, 6);
                    sb.Append("}");
                    
                    
                    
                    
                    JsonUtil.GetLineBreak(sb, 5);
                    sb.Append("]");
                    JsonUtil.GetLineBreak(sb, 4);
                    
                    sb.Append("}");

                    Console.WriteLine(i + " Add Comma " + total + " " + tileCounter);
                    
                    if (i < total-1)
                    {
                        sb.Append(",");
                    }
                    
                    JsonUtil.GetLineBreak(sb, 4);
                    
                    tileCounter++;
                }
                
            }
            
            
            
            
    //            sb.Append(String.Join("", new List<int>(layers[(int)Layer.Sprites]).ConvertAll(i => i.ToString()).ToArray()));

//                var layerEnum = (TilemapChip.Layer) Enum.Parse(typeof(TilemapChip.Layer), layerName);
//                    
//                // Need to join the layer array and add 1 to the sprite ID since tiled isn't 
//                sb.Append(string.Join(",", tilemapChip.layers[(int)layerEnum].Select(x => (x == -1 ? 0 : x + idOffset).ToString())));
                
                // tilesets end
                JsonUtil.GetLineBreak(sb, 3);
                sb.Append("]");
                
                // Layer end
                JsonUtil.GetLineBreak(sb, 2);
                sb.Append("}");
                
//                if (i < totalLayers - 1 && totalLayers > 1)
//                {
//                    sb.Append(",");
//                }
                
//            }
            
            // layers end
            JsonUtil.GetLineBreak(sb, 1);
            sb.Append("]");
            
            currentStep++;
        }
        
        private void CreateStringBuilder()
        {
            sb = new StringBuilder();
            sb.Append("{");
            
            JsonUtil.GetLineBreak(sb, 1);

            currentStep++;
        }

        private void CloseStringBuilder()
        {
//            JsonUtil.indentLevel--;
//            JsonUtil.GetLineBreak(sb);
//            sb.Append("}");
//            
            JsonUtil.indentLevel--;
            JsonUtil.GetLineBreak(sb);
            sb.Append("}");
            
            bytes = Encoding.UTF8.GetBytes(sb.ToString());
            
            currentStep++;
        }
    }
}