using System.Collections.Generic;
using System.Text;
using PixelVisionRunner.Utils;
using PixelVisionSDK.Utils;

namespace PixelVisionSDK
{
    public class SongData : AbstractData
    {
        public string name = "Untitled";
        public int[] patterns = new int[100];
        
        public int start = 0;

        private int _end;

        public int end
        {
            get => _end;
            set
            {
                // TODO need to make sure end is not before start
                _end = value;
            }
        }
        
        
        public int currentPos = -1;
        public int totalPatternsPerSong = 100;

        public int NextPattern()
        {
            currentPos++;

            if (currentPos > end)
                currentPos = 0;

            return patterns[currentPos];
        }

        public int Rewind()
        {
            currentPos = -1;

            return NextPattern();
        }

        public int SeekTo(int index)
        {
            currentPos = index.Clamp(0, patterns.Length - 1);
            
            return NextPattern();
        }
        
        public void UpdatePatternAt(int index, int id)
        {
            patterns[index] = id;
        }

        public string SerializeData()
        {
            var sb = new StringBuilder();
            JsonUtil.GetLineBreak(sb);
            sb.Append("{");
            JsonUtil.GetLineBreak(sb, 1);

            sb.Append("\"songName\":\"");
            sb.Append(name);
            sb.Append("\",");
            JsonUtil.GetLineBreak(sb, 1);
            
            sb.Append("\"start\":");
            sb.Append(start);
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 1);
            
            sb.Append("\"end\":");
            sb.Append(end);
            sb.Append(",");
            JsonUtil.GetLineBreak(sb, 1);
            
            sb.Append("\"patterns\":");
            JsonUtil.GetLineBreak(sb, 1);
            sb.Append("[");
            
            sb.Append(string.Join(",", patterns));

            sb.Append("]");

            JsonUtil.GetLineBreak(sb);
            
            sb.Append("}");

            return sb.ToString();
        }
    }
}