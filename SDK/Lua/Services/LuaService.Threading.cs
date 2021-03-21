using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using PixelVision8.Editor;

namespace PixelVision8.Runner
{
    public partial class LuaService
    {

        [RegisterLuaService]
        public void RegisterThreading(Script luaScript)
        {
            luaScript.Globals["RunBackgroundScript"] = new Func<string, string[], bool>(RunBackgroundScript);
            luaScript.Globals["BackgroundScriptData"] = new Func<string, string, string>(BackgroundScriptData);
            luaScript.Globals["CancelExport"] = new Action(CancelExport);
        }
        
        private Dictionary<string, string> bgScriptData = new Dictionary<string, string>();

        public string BackgroundScriptData(string key, string value = null)
        {
            if (value != null)
            {
                if (bgScriptData.ContainsKey(key))
                {
                    bgScriptData[key] = value;
                }
                else
                {
                    bgScriptData.Add(key, value);
                }
            }

            if (bgScriptData.ContainsKey(key))
            {
                return bgScriptData[key];
            }

            return "undefined";
        }
        
        public bool RunBackgroundScript(string scriptName, string[] args = null)
        {
            try
            {
                // filePath = UniqueFilePath(filePath.AppendFile("pattern+" + id + ".wav"));

                // TODO exporting sprites doesn't work
                if (runner.ServiceManager.GetService(typeof(GameDataExportService).FullName) is GameDataExportService
                    exportService)
                {
                    exportService.Restart();

                    bgScriptData.Clear();

                    exportService.AddExporter(new BackgroundScriptRunner(scriptName, this, args));
                    //
                    exportService.StartExport();

                    return true;
                }
            }
            catch (Exception e)
            {
                // TODO this needs to go through the error system?
                Console.WriteLine(e);
            }

            return false;
        }
        
        public void CancelExport()
        {
            if (runner.ServiceManager.GetService(typeof(GameDataExportService).FullName) is GameDataExportService
                exportService)
            {
                exportService.Cancel();
            }
        }
    }
}