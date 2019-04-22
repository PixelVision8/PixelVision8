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
        }
    }
    
    public class LuaServicePlus : LuaService//, IPlatformAccessor // TODO need to map to these APIs
    {
        protected PixelVision8Runner desktopRunner;

//        private List<DirectoryItem> directoryItems = new List<DirectoryItem>();

//        protected IFileSystem fileSystem;

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

//        private readonly List<string> validFiles = new List<string>
//        {
//            ".png",
//            ".json",
//            ".txt",
//            ".lua",
//            ".pv8" //,
////            ".pvt",
////            ".pva",
////            ".pvs",
////            ".p8"
//        };

        private WorkspaceServicePlus workspace;

        public LuaServicePlus(PixelVision8Runner runner) : base(runner)
        {
            desktopRunner = runner;

            workspace = runner.workspaceService as WorkspaceServicePlus;

            // TODO need to use service locator for this
//            workspace = locator.GetService(typeof(WorkspaceService).FullName) as WorkspaceService;
//            
//            fileSystem = workspace;
        }

        //public WorkspaceService workspace => desktopRunner.workspaceService;

        public override void ConfigureScript(Script luaScript)
        {
            base.ConfigureScript(luaScript);

//            luaScript.Globals["FindEditors"] = new Func<Dictionary<string, string>>(FindEditors);

            
//            luaScript.Globals["ImportColorsFromGameEditor"] =
//                (ImportColorsFromGameEditorDelegator) ImportColorsFromGameEditor;
            
            
            
            
            // Filesystem
            
            luaScript.Globals["NewFile"] = (NewFileDelegator) NewFile;
            
            // File APIs
            luaScript.Globals["UniqueFilePath"] = new Func<WorkspacePath, WorkspacePath>(workspace.UniqueFilePath);
            luaScript.Globals["CreateDirectory"] = new Action<WorkspacePath>(workspace.CreateDirectoryRecursive);
            luaScript.Globals["MoveTo"] = new Action<WorkspacePath, WorkspacePath>(workspace.Move);
            luaScript.Globals["CopyTo"] = new Action<WorkspacePath, WorkspacePath>(workspace.Copy);
            luaScript.Globals["Delete"] = new Action<WorkspacePath>(workspace.Delete);
            luaScript.Globals["PathExists"] = new Func<WorkspacePath, bool>(workspace.Exists);
            luaScript.Globals["GetEntities"] = new Func<WorkspacePath, List<WorkspacePath>>(path =>
                workspace.GetEntities(path).OrderBy(o => o.EntityName).ToList());
            
//            luaScript.Globals["CopyFile"] = (CopyFileDelegator) CopyFile;
//            luaScript.Globals["MoveFile"] = (MoveFileDelegator) MoveFile;
            
//            luaScript.Globals["DeleteFile"] = (DeleteFileDelegator) DeleteFile;
//            luaScript.Globals["ParentDirectory"] = (ParentDirectoryDelegator) ParentDirectory;
//            luaScript.Globals["GetDirectoryContents"] = (GetDirectoryContentsDelegator) GetDirectoryContents;
            luaScript.Globals["OldPathExists"] = (PathExistsDelegator) PathExists;
            
            luaScript.Globals["ExportGame"] = (ExportGameDelegator) ExportGame;
//            luaScript.Globals["GetSystemPath"] = (GetSystemPathDelegator) GetSystemPath;
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
//            UserData.RegisterType<DirectoryItem>();


            // Register the game editor with  the lua service
            gameEditor = new GameEditor(desktopRunner, locator);
            UserData.RegisterType<GameEditor>();
            luaScript.Globals["gameEditor"] = gameEditor;


//            luaScript.Globals["EnableCRT"] = (EnableCRTDelegator) EnableCRT;
//            luaScript.Globals["Brightness"] = (BrightnessDelegator)Brightness;
//            luaScript.Globals["Sharpness"] = (SharpnessDelegator)Sharpness;
        }

//        private List<WorkspacePath> GetEntities(WorkspacePath path)
//        {
//            return workspace.GetEntities(path).ToList();
//        }

//        public FileSystemPath NewFileSystemPath(string path)
//        {
//            return FileSystemPath.Parse(path);
//        }
//        
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
            return Json.Deserialize(ReadText(src)) as Dictionary<string, object>;
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

//        public int ImportColorsFromGameEditor(int? resetIndex = null)
//        {
//            var colorChip = runner.activeEngine.colorChip;
//
//            if (resetIndex.HasValue && resetIndex.Value > 0) colorChip.total = resetIndex.Value;
//
////            var totalPages = colorChip.pages;
//
//            // The current color chip's total becomes the start index for the new colors
//            var colorIndex = runner.activeEngine.gameChip.TotalColors(false);
//
//            // Set the color chip's page total to the current pages plus the editor's pages
////            colorChip.pages = totalPages + gameEditor.ColorPages();
//
//            var colors = gameEditor.activeColorChip.hexColors;
//
//            for (var i = colorIndex; i < colorChip.total; i++)
//                colorChip.UpdateColorAt(i, colors[i - colorIndex]);
//
//            return colorIndex;
//        }

//        private bool IsExporting()
//        {
//            return desktopRunner.IsExporting();
//        }
        public Dictionary<string, string> FindEditors()
        {
//            Console.WriteLine("Searching for editors");

            var editors = new Dictionary<string, string>();


//            var fileSystem = workspace;


            var paths = new List<WorkspacePath>
            {
                WorkspacePath.Parse("/PixelVisionOS/System/Tools/"),
//                WorkspacePath.Parse("/Workspace/System/Tools/")
            };

            // Add disk paths
            var disks = workspace.DiskPaths();
            foreach (var disk in disks) paths.Add(WorkspacePath.Parse(disk.Value).AppendDirectory("System").AppendDirectory("Tools"));

            // TODO loop through the workspace and the disks to add new paths

            var total = paths.Count;

            for (var i = 0; i < total; i++)
            {
                var path = paths[i];

                try
                {
                    if (workspace.Exists(path))
                    {
//                    Console.WriteLine("Look for editors in " + path);

                        var folders = workspace.GetEntities(path);

                        foreach (var folder in folders)
                            if (folder.IsDirectory)
                                if (workspace.ValidateGameInDir(folder))
                                {
//                            Console.WriteLine("Reading from game folder " + folder);


//                                    var metaData = workspace.ReadGameMetaData(folder.AppendFile("info.json"));

                                    var data = workspace.ReadTextFromFile(folder.AppendFile("info.json")); //ReadTextFromFile(filePath);

                                    // parse the json data into a dictionary the engine can use
                                    var jsonData =  Json.Deserialize(data) as Dictionary<string, object>;
                                    
                                    if (jsonData.ContainsKey("editType"))
                                    {
                                        var split = ((string) jsonData["editType"]).Split(',');

                                        var totalTypes = split.Length;

                                        for (var j = 0; j < totalTypes; j++)
                                        {
                                            var key = split[j];

                                            if (!editors.ContainsKey(key))
                                                editors.Add(key, folder.Path);
                                            else
                                                editors[key] = folder.Path;

//                                        Console.WriteLine("Editor Found " +  key + " " + folder.Path);
                                        }
                                    }
                                }
                    }
                }
                catch
                {
//                    runner.DisplayWarning("Couldn't find editor path " + path);
                }
            }

            return editors;
        }

        public bool NewFile(string path)
        {
            var success = false;

            try
            {
                var filePath = workspace.UniqueFilePath(WorkspacePath.Parse(path));

                var fileName = filePath.EntityName;
                var fileExt = filePath.GetExtension();

                if (fileExt == ".json" || fileExt == ".txt" || fileExt == ".lua")
                {
                    var sb = new StringBuilder();

                    if (fileExt == ".json")
                    {
                        if (fileName == "tilemap.json")
                        {
                            // TODO need a template for the tilemap

                            sb.AppendLine("{");
                            sb.AppendLine("    \"width\":33,");
                            sb.AppendLine("    \"height\":31,");
                            sb.AppendLine("    \"nextobjectid\":1,");
                            sb.AppendLine("    \"orientation\":\"orthogonal\",");
                            sb.AppendLine("    \"renderorder\":\"right-down\",");
                            sb.AppendLine("    \"tiledversion\":\"1.0.3\",");
                            sb.AppendLine("    \"tilewidth\":8,");
                            sb.AppendLine("    \"tileheight\":8,");
                            sb.AppendLine("    \"type\":\"map\",");
                            sb.AppendLine("    \"version\":1,");
                            sb.AppendLine("    \"backgroundcolor\":\"#FF00FF\",");
                            sb.AppendLine("    \"tilesets\": [");
                            sb.AppendLine("    {");
                            sb.AppendLine("        \"columns\":16,");
                            sb.AppendLine("        \"firstgid\":1,");
                            sb.AppendLine("        \"image\":\"sprites.png\",");
                            sb.AppendLine("        \"imagewidth\":128,");
                            sb.AppendLine("        \"imageheight\":1024,");
                            sb.AppendLine("        \"margin\":0,");
                            sb.AppendLine("        \"name\":\"sprites\",");
                            sb.AppendLine("        \"spacing\":0,");
                            sb.AppendLine("        \"tilewidth\":8,");
                            sb.AppendLine("        \"tileheight\":8,");
                            sb.AppendLine("        \"tilecount\":2048,");
                            sb.AppendLine("        \"transparentcolor\":\"#FF00FF\"");
                            sb.AppendLine("    }");
                            sb.AppendLine("    ],\"layers\": [");
                            sb.AppendLine("    {");
                            sb.AppendLine("        \"draworder\":\"topdown\",");
                            sb.AppendLine("        \"name\":\"Tilemap\",");
                            sb.AppendLine("        \"id\":1,");
                            sb.AppendLine("        \"type\":\"objectgroup\",");
                            sb.AppendLine("        \"opacity\":1,");
                            sb.AppendLine("        \"visible\":true,");
                            sb.AppendLine("        \"x\":0,");
                            sb.AppendLine("        \"y\":0,");
                            sb.AppendLine("        \"objects\": [");
                            sb.AppendLine("");
                            sb.AppendLine("            ]");
                            sb.AppendLine("    }");
                            sb.AppendLine("    ]");
                            sb.AppendLine("}");
                        }
                        else if (fileName == "data.json")
                        {
                            sb.AppendLine("{");
                            sb.AppendLine("    \"ColorChip\":");
                            sb.AppendLine("    {");
                            sb.AppendLine("        \"pages\":4,");
                            sb.AppendLine("        \"colorsPerPage\":64,");
                            sb.AppendLine("        \"maxColors\":64,");
                            sb.AppendLine("    },");
                            sb.AppendLine("    \"DisplayChip\":");
                            sb.AppendLine("    {");
                            sb.AppendLine("        \"width\":256,");
                            sb.AppendLine("        \"height\":240");
                            sb.AppendLine("    },");
                            sb.AppendLine("    \"GameChip\":");
                            sb.AppendLine("    {");
                            sb.AppendLine("        \"maxSize\":256,");
                            sb.AppendLine("        \"saveSlots\":8");
                            sb.AppendLine("    },");
                            sb.AppendLine("    \"MusicChip\":");
                            sb.AppendLine("    {");
                            sb.AppendLine("        \"totalTracks\":4,");
                            sb.AppendLine("        \"notesPerTrack\":127,");
                            sb.AppendLine("        \"totalLoop\":16");
                            sb.AppendLine("    },");
                            sb.AppendLine("    \"SoundChip\":");
                            sb.AppendLine("    {");
                            sb.AppendLine("        \"totalChannels\":4,");
                            sb.AppendLine("        \"totalSounds\":16");
                            sb.AppendLine("    },");
                            sb.AppendLine("    \"SpriteChip\":{");
                            sb.AppendLine("        \"maxSpriteCount\":0,");
                            sb.AppendLine("        \"unique\":false,");
                            sb.AppendLine("        \"pages\":4,");
                            sb.AppendLine("        \"cps\":8");
                            sb.AppendLine("    },");
                            sb.AppendLine("    \"TilemapChip\":");
                            sb.AppendLine("    {");
                            sb.AppendLine("        \"columns\":32,");
                            sb.AppendLine("        \"rows\":30,");
                            sb.AppendLine("        \"totalFlags\":16");
                            sb.AppendLine("    }");
                            sb.AppendLine("}");
                        }
                        else if (fileName == "info.json")
                        {
                            sb.AppendLine("{");
                            sb.AppendLine("    \"name\":\"untitled\",");
                            sb.AppendLine("    \"version\":\"v0.9.0\",");
                            sb.AppendLine("}");
                        }
                        else if (fileName == "sounds.json")
                        {
                            sb.AppendLine("{");
                            sb.AppendLine("    \"SoundChip\":");
                            sb.AppendLine("    {");
                            sb.AppendLine("        \"sounds\": []");
                            sb.AppendLine("    }");
                            sb.AppendLine("}");
                        }
                        else if (fileName == "music.json")
                        {
                            sb.AppendLine("{");
                            sb.AppendLine("    \"MusicChip\":");
                            sb.AppendLine("    {");
                            sb.AppendLine("        \"songs\": []");
                            sb.AppendLine("    }");
                            sb.AppendLine("}");
                        }
                        else
                        {
                            sb.AppendLine("{");
                            sb.AppendLine("}");
                        }
                    }
                    else if (fileExt == ".txt")
                    {
                        sb.AppendLine("Empty text file.");
                    }
                    else if (fileExt == ".lua")
                    {
                        sb.AppendLine("-- Empty code file.");
                    }

                    workspace.SaveTextToFile(filePath, sb.ToString(), true);
                }
                else if (fileExt == ".png" || fileExt == "font.png")
                {
                    TextureData textureData = null;

                    if (fileName.EndsWith("font.png"))
                    {
                        textureData = new TextureData(96, 64);
                    }
                    else if (fileName == "colors.png")
                    {
                        textureData = new TextureData(64, 64);
                        textureData.SetPixel(0, 0, 1);
                        textureData.SetPixel(0, 0, 2);
                    }
                    else if (fileName == "sprites.png")
                    {
                        textureData = new TextureData(128, 128);
                    }

                    if (textureData != null)
                    {
                        textureData.Clear(0);

                        var exporter = new PixelDataExporter(fileName, textureData.pixels, textureData.width,
                            textureData.height, new[]
                            {
                                // TODO why is this hard coded here?
                                ColorUtils.HexToColor("#ff00ff"),
                                ColorUtils.HexToColor("#000000"),
                                ColorUtils.HexToColor("#ffffff")
                            }, new PNGWriter());

                        exporter.CalculateSteps();

                        while (exporter.completed == false) exporter.NextStep();

                        workspace.SaveExporterFiles(new Dictionary<string, byte[]> {{filePath.Path, exporter.bytes}});
                    }
                }

                success = true;
            }
            catch (Exception e)
            {
                runner.DisplayWarning(e.Message);
            }

            return success;
        }

        public Dictionary<string, object> ExportGame(string path, int maxFileSize = 512)
        {
            var response = new Dictionary<string, object>
            {
                {"success", false},
                {"message", ""}
            };

            var filePath = WorkspacePath.Parse(path);

            try
            {
                if (workspace.Exists(filePath) && filePath.IsDirectory)
                {
                    var files = workspace.GetEntities(filePath);

                    using (var memoryStream = new MemoryStream())
                    {
                        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                        {
                            // Copy all the core files
                            foreach (var file in files)
                                try
                                {
                                    if (file.IsFile)
                                    {
                                        var fileName = file.EntityName;

                                        if (supportedExportFiles.IndexOf(fileName) > -1 || fileName.EndsWith(".lua") ||
                                            fileName.EndsWith(".font.png"))
                                        {
                                            var tmpPath = file.EntityName;

                                            var tmpFile = archive.CreateEntry(tmpPath);

                                            using (var entryStream = tmpFile.Open())
                                            {
                                                workspace.OpenFile(file, FileAccess.ReadWrite)
                                                    .CopyTo(entryStream);

                                                // TODO need a way to only include lua files we reference
//                                                if (fileName.EndsWith(".lua"))
//                                                {
//                                                    string fileContents;
//                                                    using (StreamReader reader = new StreamReader(entryStream))
//                                                    {
//                                                        fileContents = reader.ReadToEnd();
//                                                        
//                                                        Console.WriteLine("Script\n"+fileContents);
//                                                        
//                                                    }
//                                                }
                                            }
                                        }
                                    }
                                }
                                catch
                                {
//                                Console.WriteLine("Archive Error: "+ e);
                                }


                            // Copy the lib files

                            var libFiles = new Dictionary<string, byte[]>();

                            workspace.IncludeLibDirectoryFiles(libFiles);

                            foreach (var libFile in libFiles)
                            {
                                var tmpPath = "Libs/" + libFile.Key;

                                var zipEntry = archive.CreateEntry(tmpPath);

                                //Get the stream of the attachment
                                using (var originalFileStream = new MemoryStream(libFile.Value))
                                using (var zipEntryStream = zipEntry.Open())
                                {
                                    //Copy the attachment stream to the zip entry stream
                                    originalFileStream.CopyTo(zipEntryStream);
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
                            if (workspace.Exists(tmpExportPath) == false)
                                workspace.CreateDirectory(tmpExportPath);

                            // Create a folder with the timestamp
                            tmpExportPath = tmpExportPath.AppendDirectory(DateTime.Now.ToString("yyyyMMddHHmmss"));
                            workspace.CreateDirectory(tmpExportPath);


                            // Add the zip filename to it
                            tmpZipPath = tmpExportPath.AppendFile(filePath.EntityName + ".pv8");


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
                            response["message"] = "Unable to create a temporary build for " + filePath.EntityName +
                                                  " in " + tmpExportPath;

                            return response;
                        }

                        // Move the new build over 
                        var exportPath = filePath.AppendDirectory("Builds");

                        try
                        {
                            if (workspace.Exists(exportPath) == false)
                                workspace.CreateDirectory(exportPath);
                        }
                        catch
                        {
                        }

                        exportPath = workspace.UniqueFilePath(exportPath.AppendDirectory("Build"));
                        workspace.CreateDirectory(exportPath);

                        workspace.Copy(tmpZipPath, exportPath);

                        response["success"] = true;
                        response["message"] = "A new build was created in " + exportPath + ".";
                        response["path"] = exportPath.Path;

//                        return true;
                    }

                    // Create a zip file 
                }
            }
            catch
            {
                response["message"] = "Unable to create a build for " + filePath.EntityName;

                return response;

//                Console.WriteLine(e);
//                throw;
            }

            return response;
        }

        public bool PathExists(string path)
        {
            var filePath = WorkspacePath.Parse(path);

            try
            {
                return workspace.Exists(filePath);
            }
            catch
            {
                return false;
            }
        }

//        public string ParentDirectory(string path)
//        {
//            var filePath = WorkspacePath.Parse(path).ParentPath.Path;
//
//            return filePath;
//        }

        /// <summary>
        ///     This method returns the contents of a workspace directory. It scans the directory and builds an array
        ///     of DirectoryItems which can be used to populate a file picker component.
        /// </summary>
        /// <param name="path">String workspace path to the directory.</param>
        /// <param name="testIfGame">
        ///     An optional bool to test if the folders within the path are also games and convert their types. This is
        ///     set to false by default.
        /// </param>
        /// <param name="ignoreDirectories">
        ///     An optional bool to ignore directories and just return files in a given path. This is set to false by
        ///     default.
        /// </param>
        /// <param name="fileFilter">
        ///     This optional array can contain file extension to loop for. Extensions in this array should contain a
        ///     period before the extension name itself like ".pv8" or ".png." By default, this is set to null so all
        ///     file types will be returned.
        /// </param>
        /// <returns></returns>
//        public List<DirectoryItem> GetDirectoryContents(string path, bool testIfGame = false,
//            bool ignoreDirectories = false, string[] fileFilter = null)
//        {
//            directoryItems.Clear();
//
//            var filePath = WorkspacePath.Parse(path);
//
//            if (workspace.Exists(filePath))
//            {
//                var entities = workspace.GetEntities(filePath);
//
////                var validGamePath = ValidateGameInDir(path);
//
//                foreach (var entity in entities)
//                    if (entity.IsDirectory || validFiles.IndexOf(entity.GetExtension()) != -1 &&
//                        ignoreFiles.IndexOf(entity.EntityName) <= -1)
//                    {
//                        var tmpItem = new DirectoryItem(entity);
//
//                        // Make sure we only include 
//                        if (tmpItem.name.Length > 0)
//                            directoryItems.Add(tmpItem);
//                    }
//
//                directoryItems = directoryItems.OrderBy(o => o.name).ToList();
//            }
//
//            return directoryItems;
//        }

        private delegate bool PathExistsDelegator(string path);

        private delegate bool NewFileDelegator(string path);

        private delegate Dictionary<string, object> ExportGameDelegator(string path, int fileSize = 512);

//        private delegate string ParentDirectoryDelegator(string path);

//        private delegate List<DirectoryItem> GetDirectoryContentsDelegator(string path, bool testIfGame = false,
//            bool ignoreDirectories = false, string[] validFiles = null);

        private delegate bool SaveTextToFileDelegator(string filePath, string text, bool autoCreate = false);


    }
}