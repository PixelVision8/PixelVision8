using System.Collections.Generic;

namespace PixelVision8.Runner
{
    public partial class SystemParser
    {
        [ChipParser("DisplayChip")]
        public void ConfigureDisplayChip(Dictionary<string, object> data)
        {
            var displayChip = Target.DisplayChip;

            // Flag chip to export
            //displayChip.export = true;

            var _width = displayChip.Width;
            var _height = displayChip.Height;

            if (data.ContainsKey("width")) _width = (int) (long) data["width"];

            if (data.ContainsKey("height")) _height = (int) (long) data["height"];

            // if (data.ContainsKey("overscanX")) displayChip.OverscanX = (int) (long) data["overscanX"];

            // if (data.ContainsKey("overscanY")) displayChip.OverscanY = (int) (long) data["overscanY"];

            // if (data.ContainsKey("layers")) displayChip.layers = (int) (long) data["layers"];

            displayChip.ResetResolution(_width, _height);
        }
    }
}