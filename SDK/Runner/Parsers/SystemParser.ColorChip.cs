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

            // Flag chip to export
            //colorChip.export = true;

            // Force the color chip to clear itself
            //            colorChip.Clear();

            //            if (data.ContainsKey("colorsPerPage"))
            //                colorChip.colorsPerPage = (int) (long) data["colorsPerPage"];

            if (data.ContainsKey("maskColor")) colorChip.MaskColor = (string) data["maskColor"];

            // if (data.ContainsKey("maxColors")) colorChip.maxColors = (int)(long)data["maxColors"];

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


            // TODO this is deprecated and only in for legacy support
            // If the data has a page count, resize the pages to match that value, even though there may be less colors than pages
            //            if (data.ContainsKey("pages"))
            //                colorChip.total = (int) (long) data["pages"] * 64;
            //
            //            if (data.ContainsKey("total"))
            //                colorChip.total = (int) (long) data["total"];

            if (data.ContainsKey("backgroundColor")) colorChip.BackgroundColor = (int) (long) data["backgroundColor"];

            if (data.ContainsKey("debug")) colorChip.DebugMode = Convert.ToBoolean(data["debug"]);

            // if (data.ContainsKey("unique")) colorChip.unique = Convert.ToBoolean(data["unique"]);

            //            if (data.ContainsKey("paletteMode"))
            //                colorChip.paletteMode = Convert.ToBoolean(data["paletteMode"]);
        }
    }
}