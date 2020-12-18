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

using PixelVision8.Engine;
using PixelVision8.Engine.Chips;
using PixelVision8.Runner.Utils;
using System.Text;

namespace PixelVision8.Runner.Exporters
{
    public class SoundExporter : AbstractExporter
    {
        private readonly IEngine targetEngine;
        private StringBuilder sb;

        public SoundExporter(string fileName, IEngine targetEngine) : base(fileName)
        {
            this.targetEngine = targetEngine;

            //            CalculateSteps();
        }

        public override void CalculateSteps()
        {
            base.CalculateSteps();

            // Create a new string builder
            _steps.Add(CreateStringBuilder);


            _steps.Add(SaveGameData);

            // Save the final string builder
            _steps.Add(CloseStringBuilder);
        }

        private void SaveGameData()
        {
            var soundChip = targetEngine.SoundChip as SfxrSoundChip;

            sb.Append("\"version\":\"v2\",");
            JsonUtil.GetLineBreak(sb, 1);

            JsonUtil.indentLevel++;
            sb.Append("\"sounds\": [");

            var total = soundChip.TotalSounds;
            for (var i = 0; i < total; i++)
            {
                var sound = soundChip.ReadSound(i);
                //                if (sound != null)
                //                {
                JsonUtil.indentLevel++;


                //                {
                //                    "name":"Melody",
                //                    "settings":"0,.5,,.2,,.2,.3,.1266,,,,,,,,,,,,,,,,,,1,,,,,,"
                //                },


                sb.Append("{");
                JsonUtil.GetLineBreak(sb, 1);

                sb.Append("\"name\":\"");
                sb.Append(sound.name);
                sb.Append("\",");
                JsonUtil.GetLineBreak(sb, 1);
                sb.Append("\"settings\":");
                sb.Append("\"" + sound.param + "\"");
                JsonUtil.GetLineBreak(sb, 1);
                sb.Append("}");

                //                    sb.Append(sound.ReadSettings());
                if (i < total - 1) sb.Append(",");

                JsonUtil.GetLineBreak(sb, 1);
                JsonUtil.indentLevel--;
                //                }
            }

            JsonUtil.indentLevel--;
            JsonUtil.GetLineBreak(sb, 1);
            sb.Append("]");

            CurrentStep++;
        }

        private void CreateStringBuilder()
        {
            sb = new StringBuilder();

            sb.Append("{");
            JsonUtil.indentLevel++;

            JsonUtil.GetLineBreak(sb);
            sb.Append("\"SoundChip\":");

            JsonUtil.GetLineBreak(sb);
            sb.Append("{");
            JsonUtil.GetLineBreak(sb, 1);

            //            JsonUtil.indentLevel++;

            CurrentStep++;
        }

        private void CloseStringBuilder()
        {
            JsonUtil.indentLevel--;
            JsonUtil.GetLineBreak(sb);
            sb.Append("}");

            JsonUtil.indentLevel--;
            JsonUtil.GetLineBreak(sb, 1);
            sb.Append("}");

            bytes = Encoding.UTF8.GetBytes(sb.ToString());

            CurrentStep++;
        }
    }
}