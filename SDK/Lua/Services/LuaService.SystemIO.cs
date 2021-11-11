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
            
            luaScript.Globals["Zip"] = new Func<string, Dictionary<WorkspacePath, WorkspacePath>, WorkspacePath, int,
                    Dictionary<string, object>>(Zip);
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
            luaScript.Globals["NewImage"] = new Func<int, int, int[], string[], ImageData>(NewImage);
            
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

        protected Dictionary<string, object> OnZip(string name, Dictionary<WorkspacePath, WorkspacePath> files,
            WorkspacePath dest, int maxFileSize = 512)
        {
            var fileLoader = new WorkspaceFileLoadHelper(workspace);

            dest = workspace.UniqueFilePath(dest.AppendDirectory("Build")).AppendPath(name);
            
            var zipExporter = new ZipExporter(dest.Path, fileLoader, files, maxFileSize);

            if (((DesktopRunner) runner).ExportService is GameDataExportService exportService)
            {
                exportService.Clear();

                exportService.AddExporter(zipExporter);

                exportService.StartExport();

                // Update the response
                zipExporter.Response["success"] = true;
                zipExporter.Response["message"] = "A new build was created in " + dest + ".";
                zipExporter.Response["path"] = dest.Path;
            }
            else
            {
                zipExporter.Response["success"] = false;
                zipExporter.Response["message"] = "Couldn't find the service to save a disk.";
            }

            return zipExporter.Response;
        }

        public Dictionary<string, object> Zip(string name, Dictionary<WorkspacePath, WorkspacePath> files,
            WorkspacePath dest, int maxFileSize = 512)
        {
            
            if(name.EndsWith(".zip") == false)
                name = name + ".zip";

            return OnZip(name, files, dest, maxFileSize);

        }
        
        public Dictionary<string, object> CreateDisk(string name, Dictionary<WorkspacePath, WorkspacePath> files,
            WorkspacePath dest, int maxFileSize = 512)
        {

            if(name.EndsWith(".pv8") == false)
                name = name + ".pv8";

            // TODO need to do a size check
            return OnZip(name, files, dest, maxFileSize);

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
            Console.WriteLine("Save Image " + dest);

            // imageData = new ImageData(100, 100, null, new string[]{"FF00FF"});

            var width = imageData.Width;
            var height = imageData.Height;
            
            var cachedColors = ColorUtils.ConvertColors(imageData.Colors).Select(c=> new ColorData(c.R, c.G, c.B)).ToArray();

            var exporter = new PixelDataExporter(dest.EntityName, imageData.GetPixels(), width, height, cachedColors, _pngWriter);
            exporter.CalculateSteps();

            while (exporter.Completed == false) exporter.NextStep();

            var output = new Dictionary<string, byte[]>
            {
                {dest.Path, exporter.Bytes}
            };

            workspace.SaveExporterFiles(output);
        }
        
        // TODO need to change arguments and make colors mandatory with pixel data being optional
        public ImageData NewImage(int width, int height, int[] pixelData = null, string[] colors = null)
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

        // TODO need to remove unused mask here
        public ImageData ReadImage(WorkspacePath src, string maskHex = Constants.MaskColor, string[] colorRefs = null)
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

            List<string> finalColors; 

            // If no colors are passed in, used the image's palette
            if (colorRefs == null)
            {
                finalColors = reader.ColorPalette.Select(c => c.ToString()).ToList();
            }
            else
            {
                finalColors = colorRefs.ToList();
            }

            if(finalColors.IndexOf(maskHex) > -1)
                finalColors.RemoveAt(finalColors.IndexOf(maskHex));

            finalColors.Insert(0, maskHex);

            // Resize the color chip
            tmpColorChip.Total = finalColors.Count;

            // Add the colors
            for (int i = 0; i < finalColors.Count; i++)
            {
                tmpColorChip.UpdateColorAt(i, finalColors[i]);
            }

            // Parse the image with the new colors
            imageParser.CreateImage();

            // Return the new image from the parser
            return imageParser.ImageData;
        }
    }
}