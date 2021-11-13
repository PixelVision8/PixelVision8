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
    public class SoundExporter : AbstractExporter
    {
        private readonly PixelVision _targetPlay;
        private StringBuilder sb;

        public SoundExporter(string fileName, PixelVision targetPlay) : base(fileName)
        {
            _targetPlay = targetPlay;

            //            CalculateSteps();
        }

        public override void CalculateSteps()
        {
            base.CalculateSteps();

            // Create a new string builder
            Steps.Add(CreateStringBuilder);


            Steps.Add(SaveGameData);

            // Save the final string builder
            Steps.Add(CloseStringBuilder);
        }

        private void SaveGameData()
        {
            var soundChip = _targetPlay.SoundChip;

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

            Bytes = Encoding.UTF8.GetBytes(sb.ToString());

            CurrentStep++;
        }
    }
}