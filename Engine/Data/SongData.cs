using System;
using System.Text;
using PixelVision8.Engine.Utils;
using PixelVision8.Runner.Utils;

namespace PixelVision8.Engine
{
    public class SongData : AbstractData
    {
        public string name = "Untitled";
        private int[] _patterns = new int[100];
        
        public int[] patterns
        {
            get => _patterns;
            set
            {
                if (value.Length > totalPatternsPerSong)
                {
                    // TODO need to trim
                    
                    Array.Resize(ref value, totalPatternsPerSong);
                }
                
                _patterns = value;
                
                // TODO need to change start

                end = _patterns.Length;
                
                // TODO end should match up with the length of the song before it is resized or the end if it is bigger
                
                // TODO should we make a copy of the value?
                
                
            }
        }
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

//        public SongData(int[] patterns, int start, int end, bool loops = true)
//        {
//            
//        }
        
        public int NextPattern()
        {
            currentPos++;

            if (AtEnd())
                currentPos = 0;
    
//            Console.WriteLine("Load pattern " + currentPos);
            return _patterns[currentPos];
        }

        public bool AtEnd()
        {
            
//            Console.WriteLine("End "+ end + " " + currentPos);
            
            return currentPos >= end;
        }

        public void Rewind()
        {
           
            currentPos = -1;

//            return NextPattern();
        }

        public int SeekTo(int index)
        {
            currentPos = MathUtil.Clamp(index, -1, _patterns.Length - 1);
            
            return currentPos;
        }
        
        public void UpdatePatternAt(int index, int id)
        {
            _patterns[index] = id;
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
            
            sb.Append(string.Join(",", _patterns));

            sb.Append("]");

            JsonUtil.GetLineBreak(sb);
            
            sb.Append("}");

            return sb.ToString();
        }
    }
}