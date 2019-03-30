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

using System.Text;
using PixelVision8.Engine;
using PixelVision8.Engine.Chips;
using PixelVision8.Runner.Utils;

namespace PixelVision8.Runner.Exporters
{
    public class MetadataExporter : AbstractExporter
    {
        private IEngine engine;
        private StringBuilder sb;

        public MetadataExporter(string fileName, IEngine engine) : base(fileName)
        {
            this.engine = engine;
//            
//            CalculateSteps();
        }
        
        public override void CalculateSteps()
        {
            base.CalculateSteps();
            
            // Create a new string builder
            steps.Add(CreateStringBuilder);
            
            // Serialize Game
            if (engine.gameChip != null)
            {
                steps.Add(delegate { SerializeGameChip(engine.gameChip); });
            }
            
            // Save the final string builder
            steps.Add(CloseStringBuilder);
            
        }
        
        private void CreateStringBuilder()
        {
            sb = new StringBuilder();
            
            // Start the json string
            sb.Append("{");
            JsonUtil.GetLineBreak(sb, 1);
            
            currentStep++;
        }

        private void CloseStringBuilder()
        {
            JsonUtil.GetLineBreak(sb);
            sb.Append("}");

            bytes = Encoding.UTF8.GetBytes(sb.ToString());
            
            currentStep++;
        }
        
        private void SerializeGameChip(GameChip gameChip)
        {
            
            // Name
//            sb.Append("\"gameName\":");
//            sb.Append("\"");
//            sb.Append(gameChip.name);
//            sb.Append("\"");
//            sb.Append(",");
//            JsonUtil.GetLineBreak(sb, 1);
//            
//            // Description
//            sb.Append("\"gameDescription\":");
//            sb.Append("\"");
//            sb.Append(gameChip.description);
//            sb.Append("\"");
//            sb.Append(",");
//            JsonUtil.GetLineBreak(sb, 1);
//            
//            // Version
//            sb.Append("\"gameVersion\":");
//            sb.Append("\"");
//            sb.Append(gameChip.version);
//            sb.Append("\"");
//            sb.Append(",");
//            JsonUtil.GetLineBreak(sb, 1);
//            
//            // ext
//            sb.Append("\"gameExt\":");
//            sb.Append("\"");
//            sb.Append(gameChip.ext);
//            sb.Append("\"");
//            sb.Append(",");
//            JsonUtil.GetLineBreak(sb, 1);
            
            // Loop through all the meta data and save it
            var metaData = engine.metaData;
            
            foreach (var data in metaData)
            {
                sb.Append("\""+data.Key+"\":");
                sb.Append("\"");
                sb.Append(data.Value);
                sb.Append("\"");
                sb.Append(",");
                JsonUtil.GetLineBreak(sb, 1);
            }
            
            
            currentStep++;
        }
    }
}