using System;
using System.Collections.Generic;

namespace PixelVision8.Runner
{
    public partial class SystemParser
    {
        [ChipParser("ColorChip")]
        public void ConfigureColorChip(Dictionary<string, object> data)
        {
            var colorChip = Target.ColorChip;

            if (data.ContainsKey("maskColor")) colorChip.MaskColor = (string) data["maskColor"];

            // Force the color chip to have 256 colors
            colorChip.Total = 256;

            // Make sure we have data to parse
            if (data.ContainsKey("colors"))
            {
                colorChip.Clear();

                // Pull out the color data
                var colors = (List<object>) data["colors"];

                // Clear the colors
                colorChip.Clear();

                // Add all the colors from the data
                for (var i = 0; i < colors.Count; i++) colorChip.UpdateColorAt(i, (string) colors[i]);
            }

            if (data.ContainsKey("backgroundColor")) colorChip.BackgroundColor = (int) (long) data["backgroundColor"];

            if (data.ContainsKey("debug")) colorChip.DebugMode = Convert.ToBoolean(data["debug"]);

        }
    }
}