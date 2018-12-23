using System.Collections.Generic;
using System.Text;
using PixelVisionRunner.Utils;
using PixelVisionSDK.Utils;

namespace PixelVisionSDK
{
    public class SongData : AbstractData
    {
        public string name = "Untitled";
        public List<int> patterns = new List<int>();
        public int start = 0;
        public int end => patterns.Count;
        public int currentPos = -1;

        public int NextPattern()
        {
            currentPos++;

            if (currentPos > patterns.Count)
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
            currentPos = index.Clamp(0, patterns.Count - 1);
            
            return NextPattern();
        }
        
        public void AddPattern(int id, int? index = null)
        {
            if (index.HasValue)
            {
                var pos = index.Value.Clamp(0, patterns.Count);
                
                patterns.Insert(pos, id);
            }
            else
            {
                patterns.Add(id);
            }
        }

        public void RemovePatternAt(int index)
        {
            patterns.RemoveAt(index);
        }
        
        public string SerializeData()
        {
            var sb = new StringBuilder();
            JsonUtil.GetLineBreak(sb);
            sb.Append("{");
            JsonUtil.GetLineBreak(sb, 1);

            sb.Append("\"patternName\":\"");
            sb.Append(name);
            sb.Append("\",");
            JsonUtil.GetLineBreak(sb, 1);
            
            sb.Append("\"patterns\":");
            JsonUtil.GetLineBreak(sb, 1);
            sb.Append("[");
            JsonUtil.indentLevel++;
            sb.Append(string.Join(",", patterns));

            JsonUtil.indentLevel--;
            JsonUtil.GetLineBreak(sb, 1);
            sb.Append("]");

            JsonUtil.GetLineBreak(sb);
            
            sb.Append("}");

            return sb.ToString();
        }
    }
}