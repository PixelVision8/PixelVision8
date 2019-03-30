using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PixelVision8.Engine.Services;


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
namespace PixelVision8.Runner.Services
{
    
    public class LogService: AbstractService
    {
        public int totalItems = 100;
        
        public List<string> logBuffer = new List<string>();
        
        
        
        private readonly string[] logCharacters =
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
    
        private readonly StringBuilder sb = new StringBuilder();
        
        public LogService(int total)
        {
            this.totalItems = total;
        }

        public void UpdateLog(string logString, LogType type = LogType.Log, string stackTrace = "")
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

            if (logBuffer.Count > totalItems)
            {
                // Remove the last item
                logBuffer.RemoveAt(0);
            }
            
            Console.WriteLine(sb.ToString());
            
            logBuffer.Add(sb.ToString());
            
        }

        public List<string> ReadLogItems(int start = 0, int end = -1)
        {

            var tmpList = new List<string>();
            
            if (end == -1)
            {
                end = logBuffer.Count;
            }

            var total = end - start;
            
            for (int i = start; i < total; i++)
            {
                tmpList.Add(logBuffer[i]);
            }

            return tmpList;

        }

        public string ReadLog(int start = 0, int end = -1)
        {
            return String.Join("\n", ReadLogItems(start, end).ToArray());
        }

        public string ReadLastLogItem()
        {
            return logBuffer.Last();
        }


        public void Clear()
        {
            logBuffer.Clear();
        }
    }
}