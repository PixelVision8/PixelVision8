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
using PixelVisionRunner.Utils;
using PixelVisionSDK;

namespace GameCreator
{

    public class SfxrTrackerData : TrackerData//, ISave
    {

        public SfxrTrackerData(string name = "Untitled", int tracks = 4) : base(name, tracks)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="sb"></param>
        public string SerializeData()
        {
            var sb = new StringBuilder();
            JsonUtil.GetLineBreak(sb);
            sb.Append("{");
            JsonUtil.GetLineBreak(sb, 1);

            sb.Append("\"patternName\":\"");
            sb.Append(songName);
            sb.Append("\",");
            JsonUtil.GetLineBreak(sb, 1);

            sb.Append("\"speedInBPM\":");
            sb.Append(speedInBPM);
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 1);

            sb.Append("\"tracks\":");
            JsonUtil.GetLineBreak(sb, 1);
            sb.Append("[");
            JsonUtil.indentLevel++;
            var total = tracks.Length;
            for (var i = 0; i < total; i++)
                if (tracks[i] != null)
                {
                    JsonUtil.indentLevel++;
                    var track = tracks[i] as SfxrTrackData;

                    if (track != null)
                        sb.Append(track.SerializeData());

                    if (i < total - 1)
                        sb.Append(",");
                    JsonUtil.indentLevel--;
                }

            JsonUtil.indentLevel--;
            JsonUtil.GetLineBreak(sb, 1);
            sb.Append("]");

            JsonUtil.GetLineBreak(sb);
            sb.Append("}");

            return sb.ToString();
        }

        public override TrackData CreateNewTrack()
        {
            return new SfxrTrackData();
        }

    }

}