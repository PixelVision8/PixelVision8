////
//// Copyright (c) Jesse Freeman. All rights reserved.  
////
//// Licensed under the Microsoft Public License (MS-PL) License. 
//// See LICENSE file in the project root for full license information. 
////
//// Contributors
//// --------------------------------------------------------
//// This is the official list of Pixel Vision 8 contributors:
////
//// Jesse Freeman - @JesseFreeman
//// Christer Kaitila - @McFunkypants
//// Pedro Medeiros - @saint11
//// Shawn Rakowski - @shwany
//
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using PixelVisionRunner.Utils;
//using PixelVisionSDK;
//using PixelVisionSDK.Chips;
//
//namespace PixelVisionRunner.Exporters
//{
//    public class TileColorOffsetExporter : AbstractExporter
//    {
//
//        private IEngine targetEngine;
//        private StringBuilder sb;
//        
//        public TileColorOffsetExporter(string fileName, IEngine engine) : base(fileName)
//        {
//            targetEngine = engine;
//        }
//
//        public override void CalculateSteps()
//        {
//            base.CalculateSteps();
//            
//            // Create a new string builder
//            steps.Add(CreateStringBuilder);
//            
//            
//            steps.Add(SaveGameData);
//            
//            // Save the final string builder
//            steps.Add(CloseStringBuilder);
//        }
//        
//        private void SaveGameData()
//        {
////            var spriteChip = targetEngine.spriteChip;
//            var tilemapChip = targetEngine.tilemapChip;
//
//            var list = new List<string>
//            {
//                TilemapChip.Layer.Colors.ToString(),
////                TilemapChip.Layer.Flags.ToString(),
////                Layer.Palettes.ToString()
//            };
//
//            var totalLayers = list.Count;
//            
//            for (int i = 0; i < totalLayers; i++)
//            {
//
//                var layerName = list[i];
//
//                // name
//                sb.Append("\"name\":");
//                sb.Append("\""+layerName+"\"");
//                sb.Append(",");
//                JsonUtil.GetLineBreak(sb, 1);
//                
//                // width
//                sb.Append("\"width\":");
//                sb.Append(tilemapChip.columns);
//                sb.Append(",");
//                JsonUtil.GetLineBreak(sb, 1);
//                
//                // height
//                sb.Append("\"height\":");
//                sb.Append(tilemapChip.rows);
//                sb.Append(",");
//                JsonUtil.GetLineBreak(sb, 1);
//                
//                // layers start
//                sb.Append("\"data\": [");
//                JsonUtil.GetLineBreak(sb, 2);
//                
//                var layerEnum = (TilemapChip.Layer) Enum.Parse(typeof(TilemapChip.Layer), layerName);
//                    
//                // Need to join the layer array and add 1 to the sprite ID since tiled isn't 
////                sb.Append(string.Join(",", Array.ConvertAll(tilemapChip.layers[(int)layerEnum], x => (x == -1 ? 0 : x ).ToString())));
//                sb.Append(string.Join(",", tilemapChip.layers[(int)layerEnum].Select(x => (x == -1 ? 0 : x)).ToString()));
//
//                // tilesets end
//                JsonUtil.GetLineBreak(sb, 1);
//                sb.Append("]");
//
//            }
//            
//            currentStep++;
//        }
//        
//        private void CreateStringBuilder()
//        {
//            sb = new StringBuilder();
//            sb.Append("{");
//            
//            JsonUtil.GetLineBreak(sb, 1);
//
//            currentStep++;
//        }
//
//        private void CloseStringBuilder()
//        {
//
//            JsonUtil.indentLevel--;
//            JsonUtil.GetLineBreak(sb);
//            sb.Append("}");
//            
//            bytes = Encoding.UTF8.GetBytes(sb.ToString());
//            
//            currentStep++;
//        }
//    }
//}