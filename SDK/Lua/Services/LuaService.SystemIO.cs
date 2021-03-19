using System;
using System.Collections.Generic;
using System.Linq;
using MoonSharp.Interpreter;
using PixelVision8.Player;
using PixelVision8.Runner.Workspace;

namespace PixelVision8.Runner
{
    public partial class LuaService
    {

        [RegisterLuaService]
        public void RegisterSystemIO(Script luaScript)
        {
            // File APIs
            luaScript.Globals["UniqueFilePath"] = new Func<WorkspacePath, WorkspacePath>(workspace.UniqueFilePath);
            luaScript.Globals["CreateDirectory"] = new Action<WorkspacePath>(workspace.CreateDirectoryRecursive);
            luaScript.Globals["MoveTo"] = new Action<WorkspacePath, WorkspacePath>(workspace.Move);
            luaScript.Globals["CopyTo"] = new Action<WorkspacePath, WorkspacePath>(workspace.Copy);
            luaScript.Globals["Delete"] = new Action<WorkspacePath>(workspace.Delete);
            luaScript.Globals["PathExists"] = new Func<WorkspacePath, bool>(workspace.Exists);
            luaScript.Globals["GetEntities"] = new Func<WorkspacePath, List<WorkspacePath>>(path =>
                workspace.GetEntities(path).OrderBy(o => o.EntityName, StringComparer.OrdinalIgnoreCase).ToList());
            luaScript.Globals["GetEntitiesRecursive"] = new Func<WorkspacePath, List<WorkspacePath>>(path =>
                workspace.GetEntitiesRecursive(path).ToList());
            luaScript.Globals["CreateDisk"] =
                new Func<string, Dictionary<WorkspacePath, WorkspacePath>, WorkspacePath, int,
                    Dictionary<string, object>>(CreateDisk);

            luaScript.Globals["ClearLog"] = new Action(workspace.ClearLog);
            luaScript.Globals["ReadLogItems"] = new Func<List<string>>(workspace.ReadLogItems);

            // TODO these are deprecated
            luaScript.Globals["ReadTextFile"] = new Func<string, string>(ReadTextFile);
            luaScript.Globals["SaveTextToFile"] = (SaveTextToFileDelegator) SaveTextToFile;
            
            // Save file helpers
            luaScript.Globals["SaveText"] = new Action<WorkspacePath, string>(SaveText);
            luaScript.Globals["SaveImage"] = new Action<WorkspacePath, ImageData>(SaveImage);
            
            // Read file helpers
            luaScript.Globals["ReadJson"] = new Func<WorkspacePath, Dictionary<string, object>>(ReadJson);
            luaScript.Globals["Save"] = new Action<WorkspacePath, Dictionary<string, object>>(SaveJson);
            luaScript.Globals["ReadText"] = new Func<WorkspacePath, string>(ReadText);
            luaScript.Globals["ReadImage"] = new Func<WorkspacePath, string, string[], ImageData>(ReadImage);
            
            luaScript.Globals["NewWorkspacePath"] = new Func<string, WorkspacePath>(WorkspacePath.Parse);
            
            // File helpers
            luaScript.Globals["NewImage"] = new Func<int, int, string[], int[], ImageData>(NewImage);
            
            luaScript.Globals["CurrentTime"] = new Func<string>(CurrentTime);
            
            UserData.RegisterType<WorkspacePath>();
            UserData.RegisterType<ImageData>();
            
            luaScript.Globals["SystemVersion"] = new Func<string>(() => runner.SystemVersion);
            luaScript.Globals["SystemName"] = new Func<string>(() => runner.systemName);
            luaScript.Globals["SessionID"] = new Func<string>(() => runner.SessionId);
            luaScript.Globals["ReadBiosData"] = new Func<string, string, string>((key, defaultValue) =>
                runner.bios.ReadBiosData(key, defaultValue));
            luaScript.Globals["WriteBiosData"] = new Action<string, string>(runner.bios.UpdateBiosData);

            luaScript.Globals["DocumentPath"] = new Func<string>(() => runner.documentsPath);
            luaScript.Globals["TmpPath"] = new Func<string>(() => runner.tmpPath);
            
            luaScript.Globals["OperatingSystem"] = new Func<string>(OperatingSystem);
            
            luaScript.Globals["ShutdownSystem"] = new Action(runner.ShutdownSystem);
            luaScript.Globals["QuitCurrentTool"] = (QuitCurrentToolDelagator) runner.QuitCurrentTool;
        }
        
        // TODO this should be a Func or Action
        private delegate void QuitCurrentToolDelagator(Dictionary<string, string> metaData, string tool = null);
        
        protected string OperatingSystem()
        {
            var os = Environment.OSVersion;
            var pid = os.Platform;
            switch (pid)
            {
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.WinCE:
                    return "Windows";

                case PlatformID.Unix:
                    return "Linux";
                case PlatformID.MacOSX:
                    return "Mac";
                default:
                    return "Unknown";
            }
        }
    }
}