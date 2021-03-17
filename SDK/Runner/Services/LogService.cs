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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PixelVision8.Runner
{
    public enum LogType
    {
        Error,
        Assert,
        Warning,
        Log,
        Exception
    }
}

namespace PixelVision8.Runner
{
    public static class Log
    {
        private static readonly string[] logCharacters =
        {
            "¡", // 129 Error
            "¢", // 130 Assert
            "£", // 131 Warning
            "¤", // 132 Log
            "¥", // 133 Exception
            "¥",
            "¥",
            "¥"
        };

        private static readonly StringBuilder sb = new StringBuilder();

        public static List<string> logBuffer = new List<string>();

        // TODO need to add a way to change this and modify the length of the List
        public static int totalItems = 500;

        // public LogService(int total)
        // {
        //     totalItems = total;

        // }

        public static void Print(string logString, LogType type = LogType.Log, string stackTrace = "")
        {
            var typeCharacter = logCharacters[(int) type];

            // Clear string builder
            sb.Clear();
            sb.Append(typeCharacter);

            sb.Append(" ");
            sb.Append(logString);
            sb.Append("\n");

            if (!string.IsNullOrEmpty(stackTrace))
            {
                sb.Append(stackTrace);
                sb.Append("\n");
            }

            // Push message into the log buffer

            if (logBuffer.Count > totalItems) logBuffer.RemoveAt(0);

            // Debug.WriteLine(sb.ToString());

            Console.WriteLine(sb.ToString());

            logBuffer.Add(sb.ToString());
        }

        public static List<string> ReadLogItems(int start = 0, int end = -1)
        {
            var tmpList = new List<string>();

            if (end == -1) end = logBuffer.Count;

            var total = end - start;

            for (var i = start; i < total; i++) tmpList.Add(logBuffer[i]);

            return tmpList;
        }

        public static string ReadLog(int start = 0, int end = -1)
        {
            return string.Join("\n", ReadLogItems(start, end).ToArray());
        }

        public static string ReadLastLogItem()
        {
            return logBuffer.Last();
        }


        public static void Clear()
        {
            logBuffer.Clear();
        }
    }
}