namespace PixelVision8.Engine.Data
{
    public class SoundData : AbstractData
    {

        public static readonly string DEFAULT_SOUND_PARAM = "0,,.0185,.4397,.1783,.8434,,,,,,,,,,,,,1,,,,,.5";

        public string name { get; set; }
        public string param { get; set; }

        public bool isWav
        {
            get { return _bytes != null; }
        }

        private byte[] _bytes;

        public byte[] bytes
        {
            get { return _bytes; }
            set
            {
                _bytes = value;
            }
        }

        public SoundData(string name, string param = "")
        {

            this.name = name;

            // Make sure the param is always set to a default beep sound
            this.param = param == "" ? DEFAULT_SOUND_PARAM : param;

            //            Console.WriteLine(name + " " + this.param + " | " + param);
        }

        public SoundData(string name, byte[] bytes)
        {
            this.name = name;
            this.bytes = bytes;
        }

    }
}