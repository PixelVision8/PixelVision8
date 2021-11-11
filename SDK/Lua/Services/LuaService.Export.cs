using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

namespace PixelVision8.Runner
{
    public partial class LuaService
    {

        [RegisterLuaService]
        public void RegisterExport(Script luaScript)
        {
            
            // if (runner.mode == RunnerMode.Loading)
            // {
                // Inject the PV8 runner special global function
                luaScript.Globals["IsExporting"] = new Func<bool>(runner.ExportService.IsExporting);
                luaScript.Globals["ReadExportPercent"] = new Func<int>(runner.ExportService.ReadExportPercent);
                luaScript.Globals["ReadExportMessage"] = new Func<Dictionary<string, object>>(runner.ExportService.ReadExportMessage);
            // }
        }
    }
}