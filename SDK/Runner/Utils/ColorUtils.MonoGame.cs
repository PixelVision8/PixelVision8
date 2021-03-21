using Microsoft.Xna.Framework;

namespace PixelVision8.Runner
{
    public class ColorUtils
    {
        /// <summary>
        ///     The display target is in charge of converting system colors into MonoGame colors. This utility method
        ///     can be used by any external runner class to correctly convert hex colors into Colors.
        /// </summary>
        /// <param name="hexColors"></param>
        /// <param name="maskColor"></param>
        /// <param name="debugMode"></param>
        /// <param name="backgroundColor"></param>
        /// <returns></returns>
        public static Color[] ConvertColors(string[] hexColors, string maskColor = "#FF00FF", bool debugMode = false, int backgroundColor = 0)
        {
            var t = hexColors.Length;
            var colors = new Color[t];

            for (var i = 0; i < t; i++)
            {
                var colorHex = hexColors[i];

                if (colorHex == maskColor && debugMode == false) colorHex = hexColors[backgroundColor];

                ColorData.HexToRgb(colorHex, out var r, out var g, out var b);

                colors[i] = new Color(r, g, b);
                 
            }

            return colors;
        }
    }
}