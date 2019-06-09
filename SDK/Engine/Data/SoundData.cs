using System.Text;

namespace PixelVision8.Engine.Data
{
    public class SoundData : AbstractData
    {
        public string name { get; set; }
        public string param { get; set; }

        public bool isWav { get; private set; }

        private byte[] _bytes;
        
        public byte[] bytes
        {
            get { return _bytes; }
            set
            {
                isWav = true;
                _bytes = value;
            }
        }
        
        public SoundData(string name, string param = "")
        {
            this.name = name;
            this.param = param;
        }

        public SoundData(string name, byte[] bytes)
        {
            this.name = name;
            this.bytes = bytes;
        }
        
    }
}