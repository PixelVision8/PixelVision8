using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MoonSharp.Interpreter;
using PixelVision8.Editor;
using PixelVision8.Player;
using PixelVision8.Workspace;

namespace PixelVision8.Runner
{
    public partial class LuaService
    {
        private readonly PNGWriter _pngWriter = new PNGWriter();
        
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
        private delegate bool SaveTextToFileDelegator(string filePath, string text, bool autoCreate = false);
        
        public string CurrentTime()
        {
            return DateTime.Now.ToString("HH:mmtt");
        }
        
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
        
        public Dictionary<string, object> CreateDisk(string name, Dictionary<WorkspacePath, WorkspacePath> files,
            WorkspacePath dest, int maxFileSize = 512)
        {
            var fileLoader = new WorkspaceFileLoadHelper(workspace);

            dest = workspace.UniqueFilePath(dest.AppendDirectory("Build")).AppendPath(name + ".pv8");
            var diskExporter = new DiskExporter(dest.Path, fileLoader, files, maxFileSize);

            if (((DesktopRunner) runner).ExportService is GameDataExportService exportService)
            {
                exportService.Clear();

                exportService.AddExporter(diskExporter);

                exportService.StartExport();

                // Update the response
                diskExporter.Response["success"] = true;
                diskExporter.Response["message"] = "A new build was created in " + dest + ".";
                diskExporter.Response["path"] = dest.Path;
            }
            else
            {
                diskExporter.Response["success"] = false;
                diskExporter.Response["message"] = "Couldn't find the service to save a disk.";
            }

            return diskExporter.Response;
        }
        
        /// <summary>
        ///     This will read a text file from a valid workspace path and return it as a string. This can read .txt, .json and
        ///     .lua files.
        /// </summary>
        /// <param name="path">A valid workspace path.</param>
        /// <returns>Returns the contents of the file as a string.</returns>
        public string ReadTextFile(string path)
        {
            var filePath = WorkspacePath.Parse(path);

            return workspace.ReadTextFromFile(filePath);
        }

        public bool SaveTextToFile(string filePath, string text, bool autoCreate = false)
        {
            var path = WorkspacePath.Parse(filePath);

            return workspace.SaveTextToFile(path, text, autoCreate);
        }

        public long FileSize(WorkspacePath workspacePath)
        {
            if (workspace.Exists(workspacePath) == false) return -1;

            if (workspacePath.IsDirectory) return 0;

            return workspace.OpenFile(workspacePath, FileAccess.Read).ReadAllBytes().Length / 1024;
        }
        
        public void SaveImage(WorkspacePath dest, ImageData imageData)
        {
            var width = imageData.Width;
            var height = imageData.Height;
            var hexColors = imageData.Colors;

            // convert colors
            var totalColors = hexColors.Length;
            var colors = new ColorData[totalColors];
            for (var i = 0; i < totalColors; i++) colors[i] = new ColorData(hexColors[i]);

            var pixelData = imageData.GetPixels();

            var exporter =
                new PixelDataExporter(dest.EntityName, pixelData, width, height, colors, _pngWriter, "#FF00FF");
            exporter.CalculateSteps();

            while (exporter.Completed == false) exporter.NextStep();

            var output = new Dictionary<string, byte[]>
            {
                {dest.Path, exporter.Bytes}
            };

            workspace.SaveExporterFiles(output);
        }
        
        public ImageData NewImage(int width, int height, string[] colors, int[] pixelData = null)
        {
            return new ImageData(width, height, pixelData, colors);
        }

        public Dictionary<string, object> ReadJson(WorkspacePath src)
        {
            var text = ReadText(src);

            return Json.Deserialize(text) as Dictionary<string, object>;
        }

        public void SaveJson(WorkspacePath dest, Dictionary<string, object> data)
        {
            SaveText(dest, Json.Serialize(data));
        }

        public string ReadText(WorkspacePath src)
        {
            return workspace.ReadTextFromFile(src);
        }

        /// <summary>
        ///     Helper function to create a new text file.
        /// </summary>
        /// <param name="dest"></param>
        /// <param name="defaultText"></param>
        public void SaveText(WorkspacePath dest, string defaultText = "")
        {
            workspace.SaveTextToFile(dest, defaultText, true);
        }

        public ImageData ReadImage(WorkspacePath src, string maskHex = "#ff00ff", string[] colorRefs = null)
        {
            PNGReader reader = null;

            using (var memoryStream = new MemoryStream())
            {
                using (var fileStream = workspace.OpenFile(src, FileAccess.Read))
                {
                    fileStream.CopyTo(memoryStream);
                    fileStream.Close();
                }

                reader = new PNGReader(memoryStream.ToArray());
            }

            var tmpColorChip = new ColorChip();


            var imageParser = new SpriteImageParser("", reader, tmpColorChip);

            // Manually call each step
            imageParser.ParseImageData();

            // If no colors are passed in, used the image's palette
            if (colorRefs == null)
            {
                colorRefs = reader.ColorPalette.Select(c => c.ToString()).ToArray();
            }

            // Resize the color chip
            tmpColorChip.Total = colorRefs.Length;

            // Add the colors
            for (int i = 0; i < colorRefs.Length; i++)
            {
                tmpColorChip.UpdateColorAt(i, colorRefs[i]);
            }

            // Parse the image with the new colors
            imageParser.CreateImage();

            // Return the new image from the parser
            return imageParser.ImageData;
        }
    }
}