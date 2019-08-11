//   
// Copyright (c) Jesse Freeman, Pixel Vision 8. All rights reserved.  
//  
// Licensed under the Microsoft Public License (MS-PL) except for a few
// portions of the code. See LICENSE file in the project root for full 
// license information. Third-party libraries used by Pixel Vision 8 are 
// under their own licenses. Please refer to those libraries for details 
// on the license they use.
// 
// Contributors
// --------------------------------------------------------
// This is the official list of Pixel Vision 8 contributors:
//  
// Jesse Freeman - @JesseFreeman
// Christina-Antoinette Neofotistou @CastPixel
// Christer Kaitila - @McFunkypants
// Pedro Medeiros - @saint11
// Shawn Rakowski - @shwany
//

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using MoonSharp.Interpreter;
using PixelVision8.Engine;
using PixelVision8.Engine.Utils;
using PixelVision8.Runner.Data;
using PixelVision8.Runner.Editors;
using PixelVision8.Runner.Exporters;
using PixelVision8.Runner.Importers;
using PixelVision8.Runner.Parsers;
using PixelVision8.Runner.Utils;

// TODO need to remove reference to this
using PixelVision8.Runner.Workspace;




namespace PixelVision8.Runner.Services
{

    // Wrapper for texture data that includes Hex color data to rebuild colors when exporting
    public class Image : TextureData
    {
        public string[] colors;
        
        public Image(int width, int height, string[] colors, int[] pixelData = null) : base(width, height)
        {
            this.colors = colors;
            
            if (pixelData == null)
            {
                var total = width * height;
                
                pixelData = new int[total];

                for (int i = 0; i < total; i++)
                {
                    pixelData[i] = -1;
                }
            }

            pixels = pixelData;
        }
    }
    
    public class LuaServicePlus : LuaService//, IPlatformAccessor // TODO need to map to these APIs
    {
        protected PixelVision8Runner desktopRunner;

        // TODO move GameEditor to the plus library
        protected GameEditor gameEditor;

        private readonly List<string> ignoreFiles = new List<string>();


        private readonly List<string> supportedExportFiles = new List<string>
        {
            "sprites.png",
            "tilemap.png",
            "tilemap.json",
            "info.json",
            "data.json",
            "colors.png",
            "color-map.png",
            "music.json",
            "sounds.json",
            "saves.json"
        };

        private WorkspaceServicePlus workspace;

        public LuaServicePlus(PixelVision8Runner runner) : base(runner)
        {
            desktopRunner = runner;

            workspace = runner.workspaceService as WorkspaceServicePlus;

        }

        //public WorkspaceService workspace => desktopRunner.workspaceService;

        public override void ConfigureScript(Script luaScript)
        {
            base.ConfigureScript(luaScript);

            // File APIs
            luaScript.Globals["UniqueFilePath"] = new Func<WorkspacePath, WorkspacePath>(workspace.UniqueFilePath);
            luaScript.Globals["CreateDirectory"] = new Action<WorkspacePath>(workspace.CreateDirectoryRecursive);
            luaScript.Globals["MoveTo"] = new Action<WorkspacePath, WorkspacePath>(workspace.Move);
            luaScript.Globals["CopyTo"] = new Action<WorkspacePath, WorkspacePath>(workspace.Copy);
            luaScript.Globals["Delete"] = new Action<WorkspacePath>(workspace.Delete);
            luaScript.Globals["PathExists"] = new Func<WorkspacePath, bool>(workspace.Exists);
            luaScript.Globals["GetEntities"] = new Func<WorkspacePath, List<WorkspacePath>>(path =>
                workspace.GetEntities(path).OrderBy(o => o.EntityName, new OrdinalStringComparer()).ToList());
            luaScript.Globals["GetEntitiesRecursive"] = new Func<WorkspacePath, List<WorkspacePath>>(path =>
                workspace.GetEntitiesRecursive(path).OrderBy(o => o.EntityName, new OrdinalStringComparer()).ToList());
            
            luaScript.Globals["PlayWav"] = new Action<WorkspacePath>(PlayWav);
            luaScript.Globals["StopWav"] = new Action(StopWav);
            
            luaScript.Globals["CreateDisk"] = new Func<string, WorkspacePath[], WorkspacePath, int, Dictionary<string, object>> (CreateDisk);
            luaScript.Globals["CreateExe"] = new Func<string, WorkspacePath[], WorkspacePath, WorkspacePath, Dictionary<string, object>> (CreateExe);
            luaScript.Globals["ClearLog"] = new Action(workspace.ClearLog);
            luaScript.Globals["ReadLogItems"] = new Func<List<string>>(workspace.ReadLogItems);
            
            // TODO these are deprecated
            luaScript.Globals["ReadTextFile"] = new Func<string, string>(ReadTextFile);
            luaScript.Globals["SaveTextToFile"] = (SaveTextToFileDelegator) SaveTextToFile;
            
            // File helpers
            luaScript.Globals["NewImage"] = new Func<int, int, string[], int[], Image>(NewImage);
            
            // Read file helpers
            luaScript.Globals["ReadJson"] = new Func<WorkspacePath, Dictionary<string, object>>(ReadJson);
            luaScript.Globals["ReadText"] = new Func<WorkspacePath, string>(ReadText);
            luaScript.Globals["ReadImage"] = new Func<WorkspacePath, string, Image>(ReadImage);
            
            // Save file helpers
            luaScript.Globals["SaveText"] = new Action<WorkspacePath, string>(SaveText);
            luaScript.Globals["SaveImage"] = new Action<WorkspacePath, Image>(SaveImage);
            

            
            // Expose Bios APIs
            luaScript.Globals["ReadBiosData"] = new Func<string, string>(ReadBiosSafeMode);
            luaScript.Globals["WriteBiosData"] = new Action<string, string>(WriteBiosSafeMode);

            //            luaScript.Globals["RemapKey"] = new Action<string, int>(RemapKey);

            luaScript.Globals["NewWorkspacePath"] = new Func<string, WorkspacePath>(WorkspacePath.Parse);
            
            UserData.RegisterType<WorkspacePath>();
            UserData.RegisterType<Image>();

            // Register the game editor with  the lua service
            gameEditor = new GameEditor(desktopRunner, locator);
            UserData.RegisterType<GameEditor>();
            luaScript.Globals["gameEditor"] = gameEditor;

        }

        private SoundEffectInstance currentSound;
        
        public void PlayWav(WorkspacePath workspacePath)
        {

            if (workspace.Exists(workspacePath) && workspacePath.GetExtension() == ".wav")
            {

                if (currentSound != null)
                {
                    StopWav();
                }
                
                using (var stream = workspace.OpenFile(workspacePath, FileAccess.Read))
                {
                    currentSound = SoundEffect.FromStream(stream).CreateInstance();
                }
                
                currentSound.Play();

            }
        }

        public void StopWav()
        {
            if (currentSound != null)
            {
                currentSound.Stop();
                currentSound = null;
            }
            
        }
        
        public virtual string ReadBiosSafeMode(string key)
        {
            return desktopRunner.bios.ReadBiosData(key, null);
        }


        public virtual void WriteBiosSafeMode(string key, string value)
        {
            // TODO should there be a set of safe keys and values types that can be accepted?
            desktopRunner.bios.UpdateBiosData(key, value);
        }
        
        PNGWriter _pngWriter = new PNGWriter();
        PNGReader _pngReader = new PNGReader();

        public Image NewImage(int width, int height, string[] colors, int[] pixelData = null)
        {
            return new Image(width, height, colors, pixelData);
        }
        
        public Dictionary<string, object> ReadJson(WorkspacePath src)
        {

            var text = ReadText(src);
            
            return Json.Deserialize(text) as Dictionary<string, object>;
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

        public Image ReadImage(WorkspacePath src, string maskHex = "#ff00ff")
        {

            throw new NotImplementedException();
            
            PNGReader reader = null;
            
            using (var memoryStream = new MemoryStream())
            {
                using (var fileStream = workspace.OpenFile(src, FileAccess.Read))
                {
                    fileStream.CopyTo(memoryStream);
                    fileStream.Close();
                }
    
                reader = new PNGReader(memoryStream.ToArray(), maskHex);
                
            }
            
            var imageParser = new ImageParser(_pngReader, maskHex);

            // TODO need to finish this parser 
            return new Image(reader.width, reader.height, new []{"ff00ff"});
            
        }
        
        public void SaveImage(WorkspacePath dest, Image image)
        {

            int width = image.width;
            int height = image.height;
            string[] hexColors = image.colors;
            
            // convert colors
            var totalColors = hexColors.Length;
            var colors = new Color[totalColors];
            for (int i = 0; i < totalColors; i++)
            {
                colors[i] = ColorUtils.HexToColor(hexColors[i]);
            }

            var pixelData = image.pixels;
            
            var exporter = new PixelDataExporter(dest.EntityName, pixelData, width, height, colors, _pngWriter);
            exporter.CalculateSteps();

            while (exporter.completed == false)
            {
                exporter.NextStep();
            }

            var output = new Dictionary<string, byte[]>()
            {
                {dest.Path, exporter.bytes}
            };
            
            workspace.SaveExporterFiles(output);

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
            if (workspace.Exists(workspacePath) == false)
            {
                return -1;
            }else if (workspacePath.IsDirectory)
            {
                return 0;
            }

            return workspace.OpenFile(workspacePath, FileAccess.Read).ReadAllBytes().Length / 1024;
        }
        
        public Dictionary<string, object> CreateDisk(string gameName, WorkspacePath[] filePaths, WorkspacePath exportPath, int maxFileSize = 512)
        {
            var response = new Dictionary<string, object>
            {
                {"success", false},
                {"message", ""}
            };
            
//            var gamePath = WorkspacePath.Parse(path);

            try
            {
                
                    using (var memoryStream = new MemoryStream())
                    {
                        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                        {
                            // Copy all the core files
                            foreach (var file in filePaths)
                            {
                                
                            
                                try
                                {
                                    if (file.IsFile)
                                    {
                                        var fileName = file.EntityName;

                                        if (supportedExportFiles.IndexOf(fileName) > -1 || fileName.EndsWith(".lua") ||
                                            fileName.EndsWith(".font.png"))
                                        {
                                            var tmpPath = file.EntityName;
                                            
                                            Console.WriteLine("Adding File " + file.Path);

                                            var tmpFile = archive.CreateEntry(tmpPath);

                                            using (var entryStream = tmpFile.Open())
                                            {
                                                workspace.OpenFile(file, FileAccess.ReadWrite).CopyTo(entryStream);
                                            }
                                        }
                                    }
                                }
                                catch(Exception e)
                                {
                                Console.WriteLine("Archive Error: "+ e);
                                }

                            }
                            
                        }

                        var fileSize = memoryStream.Length / 1024;

                        response.Add("fileSize", fileSize);

                        Console.WriteLine("FileSize " + fileSize);

                        if (fileSize > maxFileSize)
                        {
                            response["message"] =
                                "The game is too big to compile. You'll need to reduce the file size or increase the game size to create a new build.";

                            return response;
                        }


                        var tmpExportPath = WorkspacePath.Root.AppendDirectory("Tmp").AppendDirectory("Builds");

                        WorkspacePath tmpZipPath;

                        try
                        {
                            // Make sure there is a builds folder in the Tmp directory
                            if (workspace.Exists(tmpExportPath) == false)
                                workspace.CreateDirectory(tmpExportPath);

                            // Create a folder with the timestamp
                            tmpExportPath = tmpExportPath.AppendDirectory(DateTime.Now.ToString("yyyyMMddHHmmss"));
                            workspace.CreateDirectory(tmpExportPath);

                            // Add the zip filename to it
                            tmpZipPath = tmpExportPath.AppendFile(gameName + ".pv8");


                            using (var fileStream = workspace.CreateFile(tmpZipPath) as FileStream)
                            {
                                memoryStream.Seek(0, SeekOrigin.Begin);
                                memoryStream.CopyTo(fileStream);
                            }

                            // Make sure we close the stream
                            memoryStream.Close();
                        }
                        catch
                        {
                            response["message"] = "Unable to create a temporary build for " + gameName +
                                                  " in " + tmpExportPath;

                            return response;
                        }

                        // Move the new build over 
//                        var exportPath = gamePath.AppendDirectory("Builds");

                        try
                        {
                            
                            exportPath = workspace.UniqueFilePath(exportPath.AppendDirectory("Build"));
                            
//                            workspace.CreateDirectory(exportPath);
                            
                            workspace.CreateDirectoryRecursive(exportPath);

                            exportPath = exportPath.AppendFile(tmpZipPath.EntityName);
                            workspace.Copy(tmpZipPath,exportPath );

                            response["success"] = true;
                            response["message"] = "A new build was created in " + exportPath + ".";
                            response["path"] = exportPath.Path;
                        }
                        catch
                        {
                            response["message"] = "Unable to create path for " + gameName +
                                                  " in " + exportPath;

                            return response;
                        }

//                        return true;
                    }

                    // Create a zip file 
//                }
            }
            catch
            {
                response["message"] = "Unable to create a build for " + gameName;


//                Console.WriteLine(e);
//                throw;
            }

            return response;
        }

        public Dictionary<string, object> CreateExe(string name, WorkspacePath[] files, WorkspacePath template, WorkspacePath exportPath)
        {
            
            var response = new Dictionary<string, object>
            {
                {"success", false},
                {"message", ""}
            };
            
            // Make sure the source is a pv8 file
            if (workspace.Exists(template) && template.GetExtension() == ".pvr")
            {

                workspace.CreateDirectoryRecursive(exportPath);

                exportPath = exportPath.AppendFile(name + ".zip");
                
                workspace.Copy(template, exportPath);
                
                // Read template into memory
                var disk = workspace.ReadDisk(exportPath) as ZipFileSystem;

                var buildFilePath = WorkspacePath.Root.AppendFile("build.json");
                var buildText = "";

//                var diskFiles = disk.GetEntities(WorkspacePath.Root);

                if (disk.Exists(buildFilePath))
                {
                    using (var file = disk.OpenFile(buildFilePath, FileAccess.Read))
                    {
                        
                        buildText = file.ReadAllText();
                        file.Close();
                        file.Dispose();
                    }
                }
                
                
                var buildJson = Json.Deserialize(buildText) as Dictionary<string, object>;

                Console.WriteLine("ContentDir " + (buildJson["ContentDir"] as String));

                var contentPath = WorkspacePath.Parse(buildJson["ContentDir"] as String);
                
                var executables = buildJson["Executables"] as List<object>;

                for (int i = 0; i < executables.Count; i++)
                {
                    var exePath = WorkspacePath.Root.AppendFile(executables[i] as string);
                    if (disk.Exists(exePath))
                    {
                        var newExePath = exePath.ParentPath.AppendFile(name + exePath.GetExtension());
                        
//                        Console.WriteLine(exePath + " " + newExePath);
                        disk.Move(exePath, disk, newExePath);
                    }
                }
                
                // TODO need to look into how to rename launcher files on linux.

                disk.Delete(contentPath);
                
                disk.CreateDirectoryRecursive(contentPath);
                
                // Delete the build script
                disk.Delete(buildFilePath);
                
//                var total = gameFiles.Length;

//                var gameFiles = new Dictionary<string, byte[]>();
//                var files = disk.GetEntities(WorkspacePath.Root);

                var list = from p in files
                    where workspace.fileExtensions.Any(val => p.EntityName.EndsWith(val))
                    select p;

                foreach (var file in list)
                {

//                    var newFile = disk.CreateFile(contentPath.AppendFile(file.EntityName));
                    
                    // TODO Track if file is critical
                    using (var memoryStream = disk.CreateFile(contentPath.AppendFile(file.EntityName)))
                    {

                        Console.WriteLine("Include " + file.EntityName);

                        using (var fileStream = workspace.OpenFile(file, FileAccess.Read))
                        {
                            fileStream.CopyTo(memoryStream);
                            fileStream.Close();
                        }

//                        gameFiles.Add(file.EntityName, memoryStream.ToArray());
                    }
                }

                
                disk.Save();

            }


            return response;
        }
        
        private delegate bool SaveTextToFileDelegator(string filePath, string text, bool autoCreate = false);


    }
}