using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Platforms;
using PixelVision8.Player;
using PixelVision8.Runner;
using PixelVision8.Workspace;
using System;
using System.IO;
using System.Text;

namespace PixelVision8.Editor
{
    public class LuaPlatformAccessorService : AbstractService, IPlatformAccessor
    {
        private readonly WorkspaceService workspace;

        public LuaPlatformAccessorService()
        {
            // Get reference to the workspace
            workspace = locator.GetService(typeof(WorkspaceService).FullName) as WorkspaceService;
        }


        public CoreModules FilterSupportedCoreModules(CoreModules module)
        {
            throw new NotImplementedException();
        }

        public string GetEnvironmentVariable(string envvarname)
        {
            throw new NotImplementedException();
        }

        public bool IsRunningOnAOT()
        {
            throw new NotImplementedException();
        }

        public string GetPlatformName()
        {
            throw new NotImplementedException();
        }

        public void DefaultPrint(string content)
        {
            // Write print statements directly to the log
            workspace.UpdateLog(content);

            throw new NotImplementedException();
        }

        public string DefaultInput(string prompt)
        {
            throw new NotImplementedException();
        }

        public Stream IO_OpenFile(Script script, string filename, Encoding encoding, string mode)
        {
            // TODO need to convert mode to file system mode

            return workspace.OpenFile(WorkspacePath.Parse(filename), FileAccess.Read);
        }

        public Stream IO_GetStandardStream(StandardFileType type)
        {
            // TODO need to look into what this is

            throw new NotImplementedException();
        }

        public string IO_OS_GetTempFilename()
        {
            throw new NotImplementedException();
        }

        public void OS_ExitFast(int exitCode)
        {
            throw new NotImplementedException();
        }

        public bool OS_FileExists(string file)
        {
            return workspace.Exists(WorkspacePath.Parse(file));
        }

        public void OS_FileDelete(string file)
        {
            workspace.Delete(WorkspacePath.Parse(file));
        }

        public void OS_FileMove(string src, string dst)
        {
            workspace.Move(WorkspacePath.Parse(src), WorkspacePath.Parse(dst));
        }

        public int OS_Execute(string cmdline)
        {
            throw new NotImplementedException();
        }
    }
}