using System;
using System.Collections.Generic;

namespace PixelVision8.Runner
{
    public partial class SystemParser
    {
        [ChipParser("TilemapChip")]
        public void ConfigureTilemapChip(Dictionary<string, object> data)
        {
            var tilemapChip = Target.TilemapChip;

            // Flag chip to export
            //tilemapChip.export = true;

            var columns = tilemapChip.Columns;
            var rows = tilemapChip.Rows;

            if (data.ContainsKey("columns")) columns = (int) (long) data["columns"];

            if (data.ContainsKey("rows")) rows = (int) (long) data["rows"];

            // if (data.ContainsKey("totalFlags")) tilemapChip.totalFlags = (int) (long) data["totalFlags"];

            if (data.ContainsKey("autoImport")) tilemapChip.autoImport = Convert.ToBoolean(data["autoImport"]);

            tilemapChip.Resize(columns, rows);
        }
    }
}