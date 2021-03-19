using System;
using System.Collections.Generic;

namespace PixelVision8.Runner
{
    public partial class SystemParser
    {
        [ChipParser("FontChip")]
        public void ConfigureFontChip(Dictionary<string, object> data)
        {
            var fontChip = Target.FontChip;

            if (data.ContainsKey("pages")) fontChip.Pages = (int) (long) data["pages"];

            if (data.ContainsKey("unique")) fontChip.Unique = Convert.ToBoolean(data["unique"]);

            // fontChip.Resize(fontChip.pageWidth, fontChip.pageHeight * fontChip.pages);
        }
    }
}