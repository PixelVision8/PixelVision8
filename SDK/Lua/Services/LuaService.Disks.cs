using System;
using System.IO;
using MoonSharp.Interpreter;
using PixelVision8.Workspace;

namespace PixelVision8.Runner
{
    public partial class LuaService
    {

        [RegisterLuaService]
        public void RegisterDisks(Script luaScript)
        {
            
            // if (runner.mode == RunnerMode.Loading)
            // {
                luaScript.Globals["DiskPaths"] = new Func<WorkspacePath[]>(() => runner.workspaceServicePlus.Disks);
                luaScript.Globals["SharedLibPaths"] = new Func<WorkspacePath[]>(() => runner.workspaceServicePlus.SharedLibDirectories().ToArray());
                    
                luaScript.Globals["EjectDisk"] = new Action<string>(runner.EjectDisk);
                luaScript.Globals["RebuildWorkspace"] = new Action(runner.workspaceServicePlus.RebuildWorkspace);
                luaScript.Globals["MountDisk"] = new Action<WorkspacePath>(path =>
                {
                    var segments = path.GetDirectorySegments();

                    var systemPath = Path.PathSeparator.ToString();

                    if (segments[0] == "Disk")
                    {
                    }
                    else if (segments[0] == "Workspace")
                    {
                        // TODO the workspace could have a different name so we should check the bios
                        systemPath = Path.Combine(runner.documentsPath, segments[0]);
                    }

                    for (var i = 1; i < segments.Length; i++) systemPath = Path.Combine(systemPath, segments[i]);

                    systemPath = Path.Combine(systemPath,
                        path.IsDirectory ? Path.PathSeparator.ToString() : path.EntityName);


                    //                Console.WriteLine("Mount Disk From " + systemPath);

                    runner.MountDisk(systemPath);
                });
            // }
        }
    }
}