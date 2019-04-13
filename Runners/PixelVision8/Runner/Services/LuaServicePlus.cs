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
using MoonSharp.Interpreter;
using PixelVision8.Engine;
using PixelVision8.Engine.Utils;
using PixelVision8.Runner.Data;
using PixelVision8.Runner.Editors;
using PixelVision8.Runner.Exporters;
using PixelVision8.Runner.Utils;

// TODO need to remove reference to this
using PixelVision8.Runner.Workspace;

namespace PixelVision8.Runner.Services
{
    public class LuaServicePlus : LuaService//, IPlatformAccessor // TODO need to map to these APIs
    {
        protected PixelVision8Runner desktopRunner;

        private List<DirectoryItem> directoryItems = new List<DirectoryItem>();

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

        private readonly List<string> validFiles = new List<string>
        {
            ".png",
            ".json",
            ".txt",
            ".lua",
            ".pv8" //,
//            ".pvt",
//            ".pva",
//            ".pvs",
//            ".p8"
        };

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

            luaScript.Globals["FindEditors"] = new Func<Dictionary<string, string>>(FindEditors);

            
            luaScript.Globals["ImportColorsFromGameEditor"] =
                (ImportColorsFromGameEditorDelegator) ImportColorsFromGameEditor;
            
            
            
            
            // Filesystem
            
            luaScript.Globals["NewFile"] = (NewFileDelegator) NewFile;
            luaScript.Globals["UniqueFilePath"] = (UniqueFilePathDelegator) UniqueFilePath;
            luaScript.Globals["NewFolder"] = (NewFolderDelegator) NewFolder;
            luaScript.Globals["CopyFile"] = (CopyFileDelegator) CopyFile;
            luaScript.Globals["MoveFile"] = (MoveFileDelegator) MoveFile;
            luaScript.Globals["DeleteFile"] = (DeleteFileDelegator) DeleteFile;
            luaScript.Globals["ParentDirectory"] = (ParentDirectoryDelegator) ParentDirectory;
            luaScript.Globals["GetDirectoryContents"] = (GetDirectoryContentsDelegator) GetDirectoryContents;
            luaScript.Globals["PathExists"] = (PathExistsDelegator) PathExists;
            

//            luaScript.Globals["IsExporting"] = new Func<bool>(IsExporting);
            luaScript.Globals["ExportGame"] = (ExportGameDelegator) ExportGame;
            luaScript.Globals["GetSystemPath"] = (GetSystemPathDelegator) GetSystemPath;

            


//            luaScript.Globals["ReadFPS"] = new Func<int>(runner.activeEngine.gameChip.ReadFPS);
            
            luaScript.Globals["ClearLog"] = new Action(workspace.ClearLog);

//            luaScript.Globals["ReadPreloaderMessage"] = new Func<string>(runner.ReadPreLoaderMessage);


//            luaScript.Globals["EnableAutoRun"] = new Action<bool>(EnableAutoRun);
//            luaScript.Globals["EnableBackKey"] = new Action<bool>(EnableBackKey);


            // Get the export service

//            var exportService =
//                runner.activeEngine.GetService(typeof(ExportService).FullName) as ExportService;

            // Manage exporting
            


            // TODO need to expose other screen scale modes


            luaScript.Globals["ReadLogItems"] = new Func<List<string>>(workspace.ReadLogItems);
//            luaScript.Globals["ReadMetaData"] = (ReadMetaDataDelegator) desktopRunner.ReadMetaData;
//            luaScript.Globals["WriteMetaData"] = (WriteMetaDataDelegator) desktopRunner.WriteMetaData;
            luaScript.Globals["ReadTextFile"] = new Func<string, string>(ReadTextFile);
            luaScript.Globals["ReadBiosData"] = (ReadBiosSafeModeDelegator) ReadBiosSafeMode;
            luaScript.Globals["WriteBiosData"] = (WriteBiosSafeModeDelegator) WriteBiosSafeMode;
            
//            luaScript.Globals["ValidateGameInDir"] = (ValidateGameInDirDelegator) ValidateGameInDir;

            luaScript.Globals["RemapKey"] = new Action<string, int>(RemapKey);
            luaScript.Globals["SaveTextToFile"] = (SaveTextToFileDelegator) SaveTextToFile;


            

            UserData.RegisterType<DirectoryItem>();


            // Register the game editor with  the lua service
            gameEditor = new GameEditor(desktopRunner, locator);
            UserData.RegisterType<GameEditor>();
            luaScript.Globals["gameEditor"] = gameEditor;


//            luaScript.Globals["EnableCRT"] = (EnableCRTDelegator) EnableCRT;
//            luaScript.Globals["Brightness"] = (BrightnessDelegator)Brightness;
//            luaScript.Globals["Sharpness"] = (SharpnessDelegator)Sharpness;
        }

        

        public void RemapKey(string mapKey, int keyCode)
        {
            if (keyCode == -1)
                return;

            // Check to see if this is an action key and update it.
//            var success = Enum.TryParse(mapKey, out RunnerGame.ActionKeys actinoKey);
//            if (success)
//            {
//                if (runner.actionKeys.ContainsKey(actinoKey))
//                {
//                    runner.actionKeys[actinoKey] = (Keys)keyCode;
//                }
//            }


// TODO need to rewire mapping action keys since this should be more generic

            // Save the new mapped key to the bios
            WriteBiosSafeMode(mapKey, keyCode);
        }

        public virtual object ReadBiosSafeMode(string key)
        {
            return desktopRunner.bios.ReadBiosData(key, null);
        }

        public virtual void WriteBiosSafeMode(string key, object value)
        {
            // TODO should there be a set of safe keys and values types that can be accepted?
            desktopRunner.bios.UpdateBiosData(key, value);
        }

        /// <summary>
        ///     Returns the meta data from a game folder. This containes the name, ext, version and description.
        /// </summary>
        /// <param name="path">A valid workspace path to the PV8 archive or game directory.</param>
        /// <returns>Returns a table with the metadata from the pv8 project.</returns>
//        public Dictionary<string, object> ReadGameMetaData(string path = null)
//        {
//            return desktopRunner.workspaceService.ReadGameMetaData(FileSystemPath.Parse(path).AppendFile("info.json"));
//        }


        

        /// <summary>
        ///     This will read a text file from a valid workspace path and return it as a string. This can read .txt, .json and
        ///     .lua files.
        /// </summary>
        /// <param name="path">A valid workspace path.</param>
        /// <returns>Returns the contents of the file as a string.</returns>
        public string ReadTextFile(string path)
        {
            var filePath = FileSystemPath.Parse(path);

            return workspace.ReadTextFromFile(filePath);
        }

        public bool SaveTextToFile(string filePath, string text, bool autoCreate = false)
        {
            var path = FileSystemPath.Parse(filePath);

            return workspace.SaveTextToFile(path, text, autoCreate);
        }


        // TODO move all of this into the Runner Game
//        public bool EnableCRT(bool? toggle)
//        {
//            return runner.EnableCRT(toggle);
//        }
//
//        public float Brightness(float? brightness = null)
//        {
//            return runner.Brightness(brightness);
//        }
//        
//        public float Sharpness(float? sharpness = null)
//        {
//            return runner.Sharpness(sharpness);
//        }

//        public void CreateGameEditor()
//        {
//            gameEditor = new GameEditor(desktopRunner);
//            // Register the game editor with  the lua service
//            RegisterType("gameEditor", gameEditor);
//        }

        public int ImportColorsFromGameEditor(int? resetIndex = null)
        {
            var colorChip = runner.activeEngine.colorChip;

            if (resetIndex.HasValue && resetIndex.Value > 0) colorChip.total = resetIndex.Value;

//            var totalPages = colorChip.pages;

            // The current color chip's total becomes the start index for the new colors
            var colorIndex = runner.activeEngine.gameChip.TotalColors(false);

            // Set the color chip's page total to the current pages plus the editor's pages
//            colorChip.pages = totalPages + gameEditor.ColorPages();

            var colors = gameEditor.activeColorChip.hexColors;

            for (var i = colorIndex; i < colorChip.total; i++)
                colorChip.UpdateColorAt(i, colors[i - colorIndex]);

            return colorIndex;
        }

//        private bool IsExporting()
//        {
//            return desktopRunner.IsExporting();
//        }
        public Dictionary<string, string> FindEditors()
        {
//            Console.WriteLine("Searching for editors");

            var editors = new Dictionary<string, string>();


//            var fileSystem = workspace;


            var paths = new List<FileSystemPath>
            {
                FileSystemPath.Parse("/PixelVisionOS/System/Tools/"),
                FileSystemPath.Parse("/Workspace/System/Tools/")
            };

            // Add disk paths
            var disks = workspace.DiskPaths();
            foreach (var disk in disks) paths.Add(FileSystemPath.Parse(disk.Value).AppendDirectory("System").AppendDirectory("Tools"));

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
                var filePath = workspace.UniqueFilePath(FileSystemPath.Parse(path));

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

            var filePath = FileSystemPath.Parse(path);

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


                        var tmpExportPath = FileSystemPath.Root.AppendDirectory("Tmp").AppendDirectory("Builds");

                        FileSystemPath tmpZipPath;

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

                        CopyFile(tmpZipPath.Path, exportPath.Path);

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

        public string GetSystemPath(string path)
        {
            try
            {
                var filePath = FileSystemPath.Parse(path);

                if (workspace.Exists(filePath))
                {
                    var file = workspace.OpenFile(filePath, FileAccess.Read) as FileStream;

                    return file.Name;
                }
            }
            catch
            {
                // ignored
            }

            return "";
        }

        public bool PathExists(string path)
        {
            var filePath = FileSystemPath.Parse(path);

            try
            {
                return workspace.Exists(filePath);
            }
            catch
            {
                return false;
            }
        }

        public Dictionary<string, string> DisksPaths()
        {
            return workspace.DiskPaths();
        }

        public string ParentDirectory(string path)
        {
            var filePath = FileSystemPath.Parse(path).ParentPath.Path;

            return filePath;
        }

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
        public List<DirectoryItem> GetDirectoryContents(string path, bool testIfGame = false,
            bool ignoreDirectories = false, string[] fileFilter = null)
        {
            directoryItems.Clear();

            var filePath = FileSystemPath.Parse(path);

            if (workspace.Exists(filePath))
            {
                var entities = workspace.GetEntities(filePath);

//                var validGamePath = ValidateGameInDir(path);

                foreach (var entity in entities)
                    if (entity.IsDirectory || validFiles.IndexOf(entity.GetExtension()) != -1 &&
                        ignoreFiles.IndexOf(entity.EntityName) <= -1)
                    {
                        var tmpItem = new DirectoryItem(entity);


                        // Make sure we only include 
                        if (tmpItem.name.Length > 0)
                            directoryItems.Add(tmpItem);
                    }

                directoryItems = directoryItems.OrderBy(o => o.name).ToList();
            }

            return directoryItems;
        }

        private delegate int ImportColorsFromGameEditorDelegator(int? resetIndex = null);

        private delegate bool PathExistsDelegator(string path);

        private delegate bool NewFolderDelegator(string path);

        private delegate bool CopyFileDelegator(string src, string dest);

        private delegate bool MoveFileDelegator(string src, string dest);

        private delegate bool DeleteFileDelegator(string src, bool autoDelete = true);

        private delegate string UniqueFilePathDelegator(string path);

        private delegate bool NewFileDelegator(string path);

        private delegate Dictionary<string, object> ExportGameDelegator(string path, int fileSize = 512);

        private delegate string GetSystemPathDelegator(string path);

        private delegate string ParentDirectoryDelegator(string path);

        private delegate List<DirectoryItem> GetDirectoryContentsDelegator(string path, bool testIfGame = false,
            bool ignoreDirectories = false, string[] validFiles = null);

        private delegate Dictionary<string, string> DisksPathsDelegator();

//        private delegate bool EnableCRTDelegator(bool? toggle);
//
//        private delegate float BrightnessDelegator(float? brightness = null);
//
//        private delegate float SharpnessDelegator(float? sharpness = null);

//        private delegate string ReadMetaDataDelegator(string key, string defaultValue = "undefined");
//
//        private delegate void WriteMetaDataDelegator(string key, string value);

        private delegate object ReadBiosSafeModeDelegator(string key);

        private delegate void WriteBiosSafeModeDelegator(string key, object value);
        
        private delegate bool SaveTextToFileDelegator(string filePath, string text, bool autoCreate = false);


        #region File System API

//        public void EjectDisk(string path)
//        {
//            workspace.EjectDisk(FileSystemPath.Parse(path));
//        }

//        public void EnableAutoRun(bool value)
//        {
//            desktopRunner.autoRunEnabled = value;
//        }
//
//        public void EnableBackKey(bool value)
//        {
//            desktopRunner.backKeyEnabled = value;
//        }

        public bool NewFolder(string path)
        {
            var filePath = workspace.UniqueFilePath(FileSystemPath.Parse(path));

            // Need to make sure we don't create a new file over an existing one
            if (filePath.IsDirectory && !workspace.Exists(filePath))
                try
                {
                    workspace.CreateDirectory(filePath);

                    return true;
                }
                catch
                {
                    runner.DisplayWarning("Could not create a new directory at '" + path + "'.");
                }

            return false;
        }

        public string UniqueFilePath(string path)
        {
            return workspace.UniqueFilePath(FileSystemPath.Parse(path)).Path;
        }

        public bool CopyFile(string src, string dest)
        {
            // Create new paths for the source and destination
            var srcPath = FileSystemPath.Parse(src);
            var destPath = FileSystemPath.Parse(dest);

            Console.WriteLine("Copy from "+src + " to " + destPath);
            
            try
            {

                // Get the parent directory
                var parent = destPath.ParentPath;

                // Check that the path exists
                if (!workspace.Exists(parent)) workspace.CreateDirectoryRecursive(parent);

                // Ignore files with the same src and dest path but the copy action is still successful
                if (srcPath != destPath) workspace.Copy(srcPath, destPath);

                return true;
            }
            catch (Exception e)
            {
//                Console.WriteLine(e);
                runner.DisplayWarning("Unable to copy '" + srcPath.Path + "' to '" + destPath.Path + "'.\n" +
                                      e.Message);
            }

            return false;
        }

        public bool MoveFile(string src, string dest)
        {
            // Create new paths for the source and destination
            var srcPath = FileSystemPath.Parse(src);
            var destPath = FileSystemPath.Parse(dest);

            // Copy fails if an existing file is in the destination directory with the same name
            if (workspace.Exists(destPath))
            {
                runner.DisplayWarning("Move failed because '" + destPath + "' already exists.");

                return false;
            }


            // TODO there is an issue copying nested folders with files in it, so this hack is a bit safer.

            // If this is a single file we are just going to use move.
            if (srcPath.IsFile)
                try
                {
                    workspace.Move(srcPath, destPath);

                    return true;
                }
                catch
                {
                    runner.DisplayWarning("Unable to move '" + src + "' to '" + destPath + "'.");
                }
            // For directories we need to use a little hack since move doesn't work on nested folders with files for some reason.
            else
                try
                {
                    // Create the new directory
                    workspace.CreateDirectory(destPath);

                    // Get the old directory's entities
                    var entities = workspace.GetEntities(srcPath);

                    // Copy each entity over
                    foreach (var entity in entities)
                        workspace.Copy(entity, destPath.AppendFile(entity.EntityName));


                    if (DeleteFile(src, true)) return true;

                    // If the original couldn't be delete, something went wrong and delete the new directory
                    DeleteFile(dest, true);
                }
                catch
                {
                    runner.DisplayWarning("Unable to move '" + src + "'.");
                }

            return false;
        }

        public bool DeleteFile(string src, bool autoDelete = false)
        {
            var srcPath = FileSystemPath.Parse(src);

            try
            {
                // Check to see if the file should be moved to the trash
                if (autoDelete == false)
                {
                    // Make sure there is a trash
                    var destPath = FileSystemPath.Parse("/Tmp/Trash/");

                    // Create trash if its missing
                    if (!workspace.Exists(destPath)) workspace.CreateDirectory(destPath);

                    CopyFile(src, destPath.Path);
                }

                // Delete the file from the disk system
                workspace.Delete(srcPath);

                return true;
            }
            catch
            {
                runner.DisplayWarning("Unable to delete '" + src + "'.");
            }

            return false;
        }

        #endregion
    }
}