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

using System;
using System.Globalization;
using System.Text.RegularExpressions;
using PixelVisionRunner;
using PixelVisionSDK.Utils;

namespace PixelVisionSDK
{

    /// <summary>
    ///     The ColorData class is a wraper for color data in the engine.
    ///     It provides a simple interface for storing RBG color data as
    ///     well as converting that data in Hex or vise versa.
    /// </summary>
    public class ColorData : AbstractData, IColor
    {

        protected float _b;
        protected float _g;
        protected float _r;
        protected float _a = 1;
        
        /// <summary>
        ///     Use this constructor for setting the ColorData instance up
        ///     with RBG values.
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        public ColorData(float r = 0f, float g = 0f, float b = 0f)
        {
            FromRGB(r, g, b);
        }
        
//        public ColorData(float r = 0f, float g = 0f, float b = 0f, float a = 0f)
//        {
//            FromRGB(r, g, b);
//
//            // Clamp alpha if it's being used.
//            a = a < 1 ? 0 : 1;
//        }

        /// <summary>
        ///     Use this constructor for setting the ColorData instance up
        ///     with a HEX color value.
        /// </summary>
        /// <param name="hexColor"></param>
        public ColorData(string hexColor)
        {
            FromHex(hexColor);
        }

        /// <summary>
        ///     This flag is used to store extra data which can be read by
        ///     other chips that analyze the ColorData instance.
        /// </summary>
        public int flag { get; set; }

        /// <summary>
        ///     Alpha can be used as a masking flag. It accepts 0 for off or 1 for on.
        /// </summary>
        public float a
        {
            get { return _a;}
            set { _a = value < 1 ? 0 : 1; } 
        }

        /// <summary>
        ///     The red value of a color. This ranges from 0 to 255.
        /// </summary>
        public float r
        {
            get { return _r; }
            set { _r = value; }
        }

        /// <summary>
        ///     The green value of a color. This ranges from 0 to 255.
        /// </summary>
        public float g
        {
            get { return _g; }
            set { _g = value; }
        }

        /// <summary>
        ///     The blue value of a color. This ranges from 0 to 255.
        /// </summary>
        public float b
        {
            get { return _b; }
            set { _b = value; }
        }

        public void FromHex(string hexColor)
        {
            float tmpR, tmpG, tmpB;

            HexToColor(hexColor, out tmpR, out tmpG, out tmpB);

            r = tmpR;
            g = tmpG;
            b = tmpB;
        }

        public void FromRGB(float r = 0f, float g = 0f, float b = 0f)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }

        /// <summary>
        ///     Returns the HEX value of the color data.
        /// </summary>
        /// <returns name="String"></returns>
        public string ToHex()
        {
            return ColorToHex(r, g, b);
        }

        /// <summary>
        ///     Static method for converting RGB colors into HEX values
        /// </summary>
        /// <param name="r">Red value</param>
        /// <param name="g">Green value</param>
        /// <param name="b">Blue value</param>
        /// <returns></returns>
        public static string ColorToHex(float r, float g, float b)
        {
            var r1 = (byte) MathUtil.RoundToInt(r * byte.MaxValue).Clamp(0, byte.MaxValue);
            var g1 = (byte) MathUtil.RoundToInt(g * byte.MaxValue).Clamp(0, byte.MaxValue);
            var b1 = (byte) MathUtil.RoundToInt(b * byte.MaxValue).Clamp(0, byte.MaxValue);
            return "#" + string.Format("{0:X2}{1:X2}{2:X2}", r1, g1, b1);
        }

        /// <summary>
        ///     Static method for converting a HEX color into an RGB value.
        /// </summary>
        /// <param name="hex"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        public static void HexToColor(string hex, out float r, out float g, out float b)
        {
            if (hex == null)
                hex = "FF00FF";

            if (hex[0] == '#')
                hex = hex.Substring(1);

            r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber) / (float) byte.MaxValue;
            g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber) / (float) byte.MaxValue;
            b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber) / (float) byte.MaxValue;
        }

        /// <summary>
        ///     Static method to calculate the brightness of a ColorData instance
        /// </summary>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float Brightness(float r, float g, float b)
        {
            return (float) Math.Sqrt(
                r * r * .241 +
                g * g * .691 +
                b * b * .068);
        }

        /// <summary>
        ///     Static method to valide that a HEX color can be parsed. HEX colors need
        ///     to be in the correct format #FFFFFF or #FFF
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static bool ValidateColor(string color)
        {
            var regex = new Regex(@"^#(?:[0-9a-fA-F]{3}){1,2}$");
            var match = regex.Match(color);
            return match.Success;
        }
        
        public override bool Equals(object other)
        {
            if (!(other is IColor))
                return false;
            var color = (IColor)other;
            return this.r.Equals(color.r) && this.g.Equals(color.g) && this.b.Equals(color.b) && this.a.Equals(color.a);
        }

        public override string ToString()
        {
            return ToHex();
        }
    }

}