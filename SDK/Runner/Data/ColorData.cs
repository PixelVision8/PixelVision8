using System;
using System.Globalization;
using System.Text;

namespace PixelVision8.Runner
{
    
    public readonly struct ColorData
    {
        public readonly byte B;
        public readonly byte G;
        public readonly byte R;
        public readonly byte A;
        
        /// <summary>
        ///     Static method for converting a HEX color into an RGB value.
        /// </summary>
        /// <param name="hex"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        public static void HexToRgb(string hex, out byte r, out byte g, out byte b)
        {
            hex ??= "FF00FF";

            if (hex[0] == '#') hex = hex.Substring(1);

            r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
            g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
            b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
        }
        
        public ColorData(byte r, byte g, byte b, byte alpha = Byte.MaxValue)
        {
            R = r;
            G = g;
            B = b;
            A = alpha;
        }

        public ColorData(string hex)
        {
            HexToRgb(hex, out R, out G, out B);
            
            A = Byte.MaxValue;
        }
        
        public override string ToString ()
        {
            return "#" + string.Format("{0:X2}{1:X2}{2:X2}", R, G, B);
        }
	
    }
}
