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
using PixelVision8.Player;
using PixelVision8.Runner;
using System.Text;
using PixelVision8.Runner.Exporters;

namespace PixelVision8.Editor
{
    public class MusicExporter : AbstractExporter
    {
        private readonly PixelVision targetEngine;
        private StringBuilder sb;

        public MusicExporter(string fileName, PixelVision targetEngine) : base(fileName)
        {
            this.targetEngine = targetEngine;

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
        
        public string SerializeData(SongData songData)
        {
            var sb = new StringBuilder();
            JsonUtil.GetLineBreak(sb);
            sb.Append("{");
            JsonUtil.GetLineBreak(sb, 1);

            sb.Append("\"songName\":\"");
            sb.Append(songData.name);
            sb.Append("\",");
            JsonUtil.GetLineBreak(sb, 1);

            sb.Append("\"start\":");
            sb.Append(songData.start);
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 1);

            sb.Append("\"end\":");
            sb.Append(songData.end);
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 1);

            sb.Append("\"patterns\":");
            JsonUtil.GetLineBreak(sb, 1);
            sb.Append("[");

            sb.Append(string.Join(",", songData.patterns));

            sb.Append("]");

            JsonUtil.GetLineBreak(sb);

            sb.Append("}");

            return sb.ToString();
        }

        private void SaveGameData()
        {
            var musicChip = targetEngine.MusicChip;
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
                    sb.Append(SerializeData(songData));
                    JsonUtil.indentLevel--;
                }

                if (i < total - 1) sb.Append(",");
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
                    sb.Append(SerializeTrackerData(songData));
                    JsonUtil.indentLevel--;
                }

                if (i < total - 1) sb.Append(",");
            }

            JsonUtil.indentLevel--;
            JsonUtil.GetLineBreak(sb, 1);
            sb.Append("]");

            CurrentStep++;
        }
        
        public string SerializeTrackerData(TrackerData trackerData)
        {
            var sb = new StringBuilder();
            JsonUtil.GetLineBreak(sb);
            sb.Append("{");
            JsonUtil.GetLineBreak(sb, 1);

            //            sb.Append("\"patternName\":\"");
            //            sb.Append(songName);
            //            sb.Append("\",");
            //            JsonUtil.GetLineBreak(sb, 1);

            sb.Append("\"speedInBPM\":");
            sb.Append(trackerData.speedInBPM);
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 1);

            sb.Append("\"tracks\":");
            JsonUtil.GetLineBreak(sb, 1);
            sb.Append("[");
            JsonUtil.indentLevel++;
            var total = trackerData.tracks.Length;
            for (var i = 0; i < total; i++)
                if (trackerData.tracks[i] != null)
                {
                    JsonUtil.indentLevel++;
                    var track = trackerData.tracks[i];

                    if (track != null) sb.Append(SerializeTrackData(track));

                    if (i < total - 1) sb.Append(",");

                    JsonUtil.indentLevel--;
                }

            JsonUtil.indentLevel--;
            JsonUtil.GetLineBreak(sb, 1);
            sb.Append("]");

            JsonUtil.GetLineBreak(sb);
            sb.Append("}");

            return sb.ToString();
        }
        
        public string SerializeTrackData(TrackData trackData)
        {
            var sb = new StringBuilder();
            JsonUtil.GetLineBreak(sb);
            sb.Append("{");
            JsonUtil.GetLineBreak(sb, 1);

            sb.Append("\"SfxId\":");
            sb.Append(trackData.sfxID);
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 1);

            sb.Append("\"notes\":[");

            sb.Append(string.Join(",", trackData.notes.Select(x => x.ToString()).ToArray()));
            sb.Append("]");

            JsonUtil.GetLineBreak(sb);
            sb.Append("}");

            return sb.ToString();
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

            CurrentStep++;
        }

        private void CloseStringBuilder()
        {
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