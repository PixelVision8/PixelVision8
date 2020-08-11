﻿//   
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

using System.Text;
using PixelVision8.Engine;
using PixelVision8.Runner.Utils;

namespace PixelVision8.Runner.Exporters
{
    public class MusicExporter : AbstractExporter
    {
        private readonly IEngine targetEngine;
        private StringBuilder sb;

        public MusicExporter(string fileName, IEngine targetEngine) : base(fileName)
        {
            this.targetEngine = targetEngine;

//            CalculateSteps();
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
            var musicChip = targetEngine.musicChip;
            sb.Append("\"version\":\"v2\",");
            JsonUtil.GetLineBreak(sb, 1);

            sb.Append("\"songs\":[");

            var total = musicChip.songs.Length;
            for (var i = 0; i < total; i++)
            {
                var songData = musicChip.songs[i];
                if (songData != null)
                {
                    JsonUtil.indentLevel++;
                    sb.Append(songData.SerializeData());
                    JsonUtil.indentLevel--;
                }

                if (i < total - 1)
                    sb.Append(",");
            }

            JsonUtil.indentLevel--;
            JsonUtil.GetLineBreak(sb, 1);
            sb.Append("],");

            JsonUtil.GetLineBreak(sb, 1);
            sb.Append("\"patterns\":[");

            total = musicChip.trackerDataCollection.Length;
            for (var i = 0; i < total; i++)
            {
                var songData = musicChip.trackerDataCollection[i];
                if (songData != null)
                {
                    JsonUtil.indentLevel++;
                    sb.Append(songData.SerializeData());
                    JsonUtil.indentLevel--;
                }

                if (i < total - 1)
                    sb.Append(",");
            }

            JsonUtil.indentLevel--;
            JsonUtil.GetLineBreak(sb, 1);
            sb.Append("]");

            currentStep++;
        }

        private void CreateStringBuilder()
        {
            sb = new StringBuilder();

            sb.Append("{");
            JsonUtil.indentLevel++;

            JsonUtil.GetLineBreak(sb);
            sb.Append("\"MusicChip\":");

            JsonUtil.GetLineBreak(sb);
            sb.Append("{");
            JsonUtil.GetLineBreak(sb, 1);

            JsonUtil.indentLevel++;

            currentStep++;
        }

        private void CloseStringBuilder()
        {
            JsonUtil.GetLineBreak(sb);
            sb.Append("}");

            JsonUtil.indentLevel--;
            JsonUtil.GetLineBreak(sb, 1);
            sb.Append("}");

            bytes = Encoding.UTF8.GetBytes(sb.ToString());

            currentStep++;
        }
    }
}