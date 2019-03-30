using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;

namespace PixelVision8.Engine.Utils
{
    public class ColorUtils
    {
        /// <summary>
        ///     Static method for converting RGB colors into HEX values
        /// </summary>
        /// <param name="r">Red value</param>
        /// <param name="g">Green value</param>
        /// <param name="b">Blue value</param>
        /// <returns></returns>
        public static string RgbToHex(byte r, byte g, byte b)
        {
//            var r1 = 0;//(byte) MathUtil.Clamp(MathUtil.RoundToInt(r * byte.MaxValue), 0, byte.MaxValue);
//            var g1 = 0;//(byte) MathUtil.Clamp(MathUtil.RoundToInt(g * byte.MaxValue), 0, byte.MaxValue);
//            var b1 = 0;//(byte) MathUtil.Clamp(MathUtil.RoundToInt(b * byte.MaxValue), 0, byte.MaxValue);
            return "#" + string.Format("{0:X2}{1:X2}{2:X2}", r, g, b);
        }

        /// <summary>
        ///     Static method for converting a HEX color into an RGB value.
        /// </summary>
        /// <param name="hex"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        public static void HexToRgb(string hex, out byte r, out byte g, out byte b)
        {
            if (hex == null)
                hex = "FF00FF";

            if (hex[0] == '#')
                hex = hex.Substring(1);

            r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);// / (float) byte.MaxValue;
            g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);// / (float) byte.MaxValue;
            b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);// / (float) byte.MaxValue;
        }

        /// <summary>
        ///     Static method to valide that a HEX color can be parsed. HEX colors need
        ///     to be in the correct format #FFFFFF or #FFF
        /// </summary>
        /// <param name="hexColor"></param>
        /// <returns></returns>
        public static bool ValidateHexColor(string hexColor)
        {
            var regex = new Regex(@"^#(?:[0-9a-fA-F]{3}){1,2}$");
            var match = regex.Match(hexColor);
            return match.Success;
        }

        public static Color HexToColor(string hex)
        {
            byte r, g, b;
            
            HexToRgb(hex, out r, out g, out b);
            
            return new Color(r, g, b);
        }
        
        public static void HexToColor(string hex, Color color)
        {
            byte r, g, b;
            
            HexToRgb(hex, out r, out g, out b);

            color.R = r;
            color.G = g;
            color.B = b;
            
        }
    }
}