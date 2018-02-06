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

namespace PixelVisionRunner.Utils
{

    public class JsonUtil
    {

        public static int indentLevel = 0;
        public static string lineBreak = "\n";
        public static string indentChar = "    ";

        public static bool compressJson { get; set; }

        public static void GetLineBreak(StringBuilder sb, int indent = 0)
        {
            if (compressJson)
                return;

            sb.Append(lineBreak);

            GetIndent(sb, indent);
        }

        public static void GetIndent(StringBuilder sb, int indent = 0)
        {
            indent += indentLevel;
            for (var i = 0; i < indent; i++)
                sb.Append(indentChar);
        }

    }

}