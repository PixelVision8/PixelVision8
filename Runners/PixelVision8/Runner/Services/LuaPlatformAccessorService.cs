using System.IO;
using System.Text;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Platforms;
using PixelVision8.Engine.Services;
using PixelVision8.Runner.Services;
using SharpFileSystem;

namespace PixelVision8Runner.Runner.Services
{
    public class LuaPlatformAccessorService: AbstractService, IPlatformAccessor
    {
        private WorkspaceService workspace;
        
        public LuaPlatformAccessorService()
        {

            // Get reference to the workspace
            workspace = locator.GetService(typeof(WorkspaceService).FullName) as WorkspaceService;

        }


        public CoreModules FilterSupportedCoreModules(CoreModules module)
        {
            throw new System.NotImplementedException();
        }

        public string GetEnvironmentVariable(string envvarname)
        {
            throw new System.NotImplementedException();
        }

        public bool IsRunningOnAOT()
        {
            throw new System.NotImplementedException();
        }

        public string GetPlatformName()
        {
            throw new System.NotImplementedException();
        }

        public void DefaultPrint(string content)
        {
            
            // Write print statements directly to the log
            workspace.UpdateLog(content);
            
            throw new System.NotImplementedException();
        }

        public string DefaultInput(string prompt)
        {
            throw new System.NotImplementedException();
        }

        public Stream IO_OpenFile(Script script, string filename, Encoding encoding, string mode)
        {

            // TODO need to convert mode to file system mode
            
            return workspace.OpenFile(FileSystemPath.Parse(filename), FileAccess.Read);
            
            
        }

        public Stream IO_GetStandardStream(StandardFileType type)
        {
            
            // TODO need to look into what this is
            
            throw new System.NotImplementedException();
        }

        public string IO_OS_GetTempFilename()
        {
            
            
            throw new System.NotImplementedException();
        }

        public void OS_ExitFast(int exitCode)
        {
            throw new System.NotImplementedException();
        }

        public bool OS_FileExists(string file)
        {

            return workspace.Exists(FileSystemPath.Parse(file));
            
        }

        public void OS_FileDelete(string file)
        {
            workspace.Delete(FileSystemPath.Parse(file));
        }

        public void OS_FileMove(string src, string dst)
        {
            workspace.Move(FileSystemPath.Parse(src), FileSystemPath.Parse(dst));
            
        }

        public int OS_Execute(string cmdline)
        {
            throw new System.NotImplementedException();
        }
    }
}