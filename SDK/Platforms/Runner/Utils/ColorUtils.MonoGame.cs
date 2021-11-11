using System;
using Microsoft.Xna.Framework;
using PixelVision8.Player;

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
        public static Color[] ConvertColors(string[] hexColors)//, string maskColor = Constants.MaskColor, bool debugMode = false, int backgroundColor = 1)
        {
            
            var t = hexColors.Length;
            var colors = new Color[t];

            for (var i = 0; i < t; i++)
            {
                var colorHex = hexColors[i];

                // if (colorHex == maskColor && debugMode == false) colorHex = hexColors[backgroundColor];

                // Make the first color match the background color
                // if (i == 0 && debugMode == false) colorHex = hexColors[backgroundColor];

                ColorData.HexToRgb(colorHex, out var r, out var g, out var b);

                colors[i] = new Color(r, g, b);
                 
            }

            return colors;
        }

        public static Color[] ConvertColors(string[] hexColors, int bgColorId, bool debugMode = false)
        {

            // var refColors = hexColors[Constants.EmptyPixel];

            if(debugMode == false)
            {
                // Set the default color to 1 if the bg color is out of range
                // hexColors[bgColorId] == refColors)

                hexColors[Constants.EmptyPixel] = hexColors[bgColorId];
                //     bgColorId = 1;

                // Console.WriteLine("BG Color " + bgColorId + " " + hexColors[bgColorId] + " vs " + hexColors[0]);

                // Loop through and replace all the colors matching the mask with the bg color
                // for (int i = 0; i < hexColors.Length; i++)
                // {
                //     if(hexColors[i] == refColors)
                //         hexColors[i] = hexColors[bgColorId];
                // }

            }
            
            return ConvertColors(hexColors);
        }
        
        public static string RgbToHex(ColorData colorData)
        {
            return "#" + string.Format("{0:X2}{1:X2}{2:X2}", colorData.R, colorData.G, colorData.B);
        }
        
        public static string RgbToHex(Color color)
        {
            return "#" + string.Format("{0:X2}{1:X2}{2:X2}", color.R, color.G, color.B);
        }
    }
}