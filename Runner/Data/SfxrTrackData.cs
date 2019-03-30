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

using System.Linq;
using System.Text;
using PixelVision8.Engine;
using PixelVision8.Runner.Utils;

namespace PixelVision8.Runner.Data
{

    public class SfxrTrackData : TrackData
    {

//        /// <summary>
//        /// </summary>
//        /// <param name="data"></param>
//        public void DeserializeData(Dictionary<string, object> data)
//        {
//            if (data.ContainsKey("sfxID"))
//                sfxID = Convert.ToInt32((long) data["sfxID"]);
//
//            if (data.ContainsKey("notes"))
//            {
//                var noteData = (List<object>) data["notes"];
//                var totalNotes = noteData.Count;
//                notes = new int[totalNotes];
//                for (var k = 0; k < totalNotes; k++)
//                    notes[k] = (int) (long) noteData[k];
//            }
//        }

//        public bool ignore { get; private set; }

        /// <summary>
        /// </summary>
        /// <param name="sb"></param>
        public string SerializeData()
        {
            var sb = new StringBuilder();
            JsonUtil.GetLineBreak(sb);
            sb.Append("{");
            JsonUtil.GetLineBreak(sb, 1);

            sb.Append("\"sfxID\":");
            sb.Append(sfxID);
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 1);

            sb.Append("\"notes\":[");

            sb.Append(string.Join(",", notes.Select(x => x.ToString()).ToArray()));
            sb.Append("]");

            JsonUtil.GetLineBreak(sb);
            sb.Append("}");

            return sb.ToString();
        }

    }

}