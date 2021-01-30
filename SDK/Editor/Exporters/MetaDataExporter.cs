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

namespace PixelVision8.Runner.Exporters
{
    public class MetadataExporter : AbstractExporter
    {
        private readonly PixelVision engine;
        private StringBuilder sb;
        private GameChip _gameChip;

        public MetadataExporter(string fileName, PixelVision engine) : base(fileName)
        {
            this.engine = engine;
            _gameChip = this.engine.GameChip;

            //            
            //            CalculateSteps();
        }

        public override void CalculateSteps()
        {
            base.CalculateSteps();

            // Create a new string builder
            Steps.Add(CreateStringBuilder);

            // Serialize Game
            if (engine.GameChip != null) Steps.Add(delegate { SerializeGameChip(_gameChip); });

            // Save the final string builder
            Steps.Add(CloseStringBuilder);
        }

        private void CreateStringBuilder()
        {
            sb = new StringBuilder();

            // Start the json string
            sb.Append("{");
            JsonUtil.GetLineBreak(sb, 1);

            CurrentStep++;
        }

        private void CloseStringBuilder()
        {
            JsonUtil.GetLineBreak(sb);
            sb.Append("}");

            Bytes = Encoding.UTF8.GetBytes(sb.ToString());

            CurrentStep++;
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
            var metaData = engine.MetaData;

            foreach (var data in metaData)
            {
                sb.Append("\"" + data.Key + "\":");
                sb.Append("\"");
                sb.Append(data.Value);
                sb.Append("\"");
                sb.Append(",");
                JsonUtil.GetLineBreak(sb, 1);
            }


            CurrentStep++;
        }
    }
}