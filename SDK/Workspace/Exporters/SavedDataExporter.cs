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

using System.Linq;
using System.Text;
using PixelVision8.Engine;
using PixelVision8.Runner.Utils;

namespace PixelVision8.Runner.Exporters
{
    public class SavedDataExporter : AbstractExporter
    {
        private readonly IEngine targetEngine;
        private StringBuilder sb;

        public SavedDataExporter(string fileName, IEngine targetEngine) : base(fileName)
        {
            this.targetEngine = targetEngine;

            CalculateSteps();
        }

        public override void CalculateSteps()
        {
            if (targetEngine.GameChip.SaveSlots < 1) return;

            base.CalculateSteps();

            // Create a new string builder
            steps.Add(CreateStringBuilder);


            steps.Add(SaveGameData);

            // Save the final string builder
            steps.Add(CloseStringBuilder);
        }

        private void SaveGameData()
        {
            var gameChip = targetEngine.GameChip;

            // Save Data
            sb.Append("\"savedData\":");

            sb.Append("{");

            JsonUtil.indentLevel++;
            JsonUtil.GetLineBreak(sb, 1);

            var savedData = gameChip.savedData;

            for (var i = savedData.Count - 1; i >= 0; i--)
            {
                var item = savedData.ElementAt(i);
                sb.Append("\"");
                sb.Append(item.Key);
                sb.Append("\": \"");
                sb.Append(item.Value);
                sb.Append("\"");
                if (i > 0)
                {
                    sb.Append(",");
                    JsonUtil.GetLineBreak(sb, 1);
                }
            }

            currentStep++;
        }

        private void CreateStringBuilder()
        {
            sb = new StringBuilder();

            sb.Append("{");
            JsonUtil.GetLineBreak(sb, 1);

            JsonUtil.indentLevel++;

            JsonUtil.GetLineBreak(sb);

            sb.Append("\"GameChip\":");

            JsonUtil.GetLineBreak(sb);
            sb.Append("{");
            JsonUtil.GetLineBreak(sb, 1);

            currentStep++;
        }

        private void CloseStringBuilder()
        {
            JsonUtil.indentLevel--;
            JsonUtil.GetLineBreak(sb, 1);
            sb.Append("}");

            JsonUtil.indentLevel--;
            JsonUtil.GetLineBreak(sb, 1);
            sb.Append("}");

            JsonUtil.GetLineBreak(sb);
            sb.Append("}");

            bytes = Encoding.UTF8.GetBytes(sb.ToString());

            currentStep++;
        }
    }
}