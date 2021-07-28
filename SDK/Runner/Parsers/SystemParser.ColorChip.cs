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

            // Force the color chip to have 256 colors
            colorChip.Total = 256;

            // TODO need to deprecate this
            // if (data.ContainsKey("backgroundColor"))
            // {
                
            //     // TODO this should use PV8's error system
            //     Console.WriteLine("Reading the Background Color from the Color Chip Data is deprecated and will be removed in a future release.");
                  
            //     Target.GameChip.BackgroundColor((int) (long) data["backgroundColor"]);
            // }
            
            if (data.ContainsKey("debug")) colorChip.DebugMode = Convert.ToBoolean(data["debug"]);

        }
    }
}