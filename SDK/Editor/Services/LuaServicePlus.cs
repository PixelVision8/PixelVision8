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
using System.Linq;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using MoonSharp.Interpreter;
using PixelVision8.Engine;
using PixelVision8.Engine.Utils;
using PixelVision8.Runner.Editors;
using PixelVision8.Runner.Exporters;
using PixelVision8.Runner.Importers;
using PixelVision8.Runner.Parsers;
using PixelVision8.Runner.Utils;
using PixelVision8.Runner.Workspace;
// TODO need to remove reference to this


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

                for (var i = 0; i < total; i++) pixelData[i] = -1;
            }

            pixels = pixelData;
        }
    }

    public class LuaServicePlus : LuaService //, IPlatformAccessor // TODO need to map to these APIs
    {
        private readonly PNGWriter _pngWriter = new PNGWriter();
        private readonly WorkspaceServicePlus workspace;

        private SoundEffectInstance currentSound;
        protected PixelVision8Runner desktopRunner;
        protected GameEditor gameEditor;

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
                workspace.GetEntities(path).OrderBy(o => o.EntityName, StringComparer.OrdinalIgnoreCase).ToList());
            luaScript.Globals["GetEntitiesRecursive"] = new Func<WorkspacePath, List<WorkspacePath>>(path =>
                workspace.GetEntitiesRecursive(path).ToList());

            luaScript.Globals["PlayWav"] = new Action<WorkspacePath>(PlayWav);
            luaScript.Globals["StopWav"] = new Action(StopWav);

            luaScript.Globals["CreateDisk"] =
                new Func<string, WorkspacePath[], WorkspacePath, int, string[], Dictionary<string, object>>(CreateDisk);
            luaScript.Globals["CreateExe"] =
                new Func<string, WorkspacePath[], WorkspacePath, WorkspacePath, string[], Dictionary<string, object>>(
                    CreateExe);
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

//
//            // Expose Bios APIs
//            luaScript.Globals["ReadBiosData"] = new Func<string, string, string>((key, defaultValue) =>
//                desktopRunner.bios.ReadBiosData(key, defaultValue));
//            luaScript.Globals["WriteBiosData"] = new Action<string, string>(desktopRunner.bios.UpdateBiosData);

            //            luaScript.Globals["RemapKey"] = new Action<string, int>(RemapKey);

            luaScript.Globals["NewWorkspacePath"] = new Func<string, WorkspacePath>(WorkspacePath.Parse);

            UserData.RegisterType<WorkspacePath>();
            UserData.RegisterType<Image>();

            // Register the game editor with  the lua service
            gameEditor = new GameEditor(desktopRunner, locator);
            UserData.RegisterType<GameEditor>();
            luaScript.Globals["gameEditor"] = gameEditor;
        }

        public void PlayWav(WorkspacePath workspacePath)
        {
            if (workspace.Exists(workspacePath) && workspacePath.GetExtension() == ".wav")
            {
                if (currentSound != null) StopWav();

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

        public virtual void WriteBiosSafeMode(string key, string value)
        {
            // TODO should there be a set of safe keys and values types that can be accepted?
            desktopRunner.bios.UpdateBiosData(key, value);
        }
//        readonly PNGReader _pngReader = new PNGReader();

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
//            throw new NotImplementedException();

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

            var imageParser = new ImageParser(reader, maskHex);
            imageParser.CalculateSteps();

            while (imageParser.completed == false) imageParser.NextStep();

            // TODO need to convert the image data to colors and pixel data

            // TODO need to finish this parser 
            return new Image(reader.width, reader.height, new[] {"ff00ff"});
        }

        public void SaveImage(WorkspacePath dest, Image image)
        {
            var width = image.width;
            var height = image.height;
            var hexColors = image.colors;

            // convert colors
            var totalColors = hexColors.Length;
            var colors = new Color[totalColors];
            for (var i = 0; i < totalColors; i++) colors[i] = ColorUtils.HexToColor(hexColors[i]);

            var pixelData = image.pixels;

            var exporter =
                new PixelDataExporter(dest.EntityName, pixelData, width, height, colors, _pngWriter, "#FF00FF");
            exporter.CalculateSteps();

            while (exporter.completed == false) exporter.NextStep();

            var output = new Dictionary<string, byte[]>
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
                return -1;
            if (workspacePath.IsDirectory) return 0;

            return workspace.OpenFile(workspacePath, FileAccess.Read).ReadAllBytes().Length / 1024;
        }

        public Dictionary<string, object> CreateDisk(string gameName, WorkspacePath[] filePaths,
            WorkspacePath exportPath, int maxFileSize = 512, string[] libFileNames = null)
        {
            var response = new Dictionary<string, object>
            {
                {"success", false},
                {"message", ""}
            };

            try
            {
                // Create a path to the temp directory for the builds
                var tmpExportPath = WorkspacePath.Root.AppendDirectory("Tmp").AppendDirectory("Builds");

                // Make sure there is a builds folder in the Tmp directory
                if (workspace.Exists(tmpExportPath) == false)
                    workspace.CreateDirectory(tmpExportPath);

                // Create a folder with the timestamp
                tmpExportPath = tmpExportPath.AppendDirectory(DateTime.Now.ToString("yyyyMMddHHmmss"));
                workspace.CreateDirectory(tmpExportPath);

                // Add the zip filename to it
                var tmpZipPath = tmpExportPath.AppendFile(gameName + ".pv8");

                // 'using' statements guarantee the stream is closed properly which is a big source
                // of problems otherwise.  Its exception safe as well which is great.
                using (var OutputStream = new ZipOutputStream(workspace.CreateFile(tmpZipPath)))
                {
                    // Define the compression level
                    // 0 - store only to 9 - means best compression
                    OutputStream.SetLevel(4);

                    var buffer = new byte[4096];

                    foreach (var file in filePaths)
                        if (file.IsFile)
                        {
                            // Using GetFileName makes the result compatible with XP
                            // as the resulting path is not absolute.
                            var entry = new ZipEntry(file.EntityName) {DateTime = DateTime.Now};

                            // Setup the entry data as required.

                            // Crc and size are handled by the library for seakable streams
                            // so no need to do them here.

                            // Could also use the last write time or similar for the file.
                            OutputStream.PutNextEntry(entry);

                            using (var fs = workspace.OpenFile(file, FileAccess.Read) as FileStream)
                            {
                                // Using a fixed size buffer here makes no noticeable difference for output
                                // but keeps a lid on memory usage.
                                int sourceBytes;

                                do
                                {
                                    sourceBytes = fs.Read(buffer, 0, buffer.Length);
                                    OutputStream.Write(buffer, 0, sourceBytes);
                                } while (sourceBytes > 0);
                            }
                        }

                    // Copy all the lib files
                    if (libFileNames != null)
                    {
                        var libFileData = new Dictionary<string, byte[]>();

                        workspace.IncludeLibDirectoryFiles(libFileData);

                        var total = libFileNames.Length;

                        for (var i = 0; i < total; i++)
                        {
                            var fileName = libFileNames[i] + ".lua";

                            if (libFileData.ContainsKey(fileName))
                            {
//                                    var tmpPath = fileName;

                                var entry = new ZipEntry(fileName);

                                OutputStream.PutNextEntry(entry);

                                using (Stream fs = new MemoryStream(libFileData[fileName]))
                                {
                                    // Using a fixed size buffer here makes no noticeable difference for output
                                    // but keeps a lid on memory usage.
                                    int sourceBytes;

                                    do
                                    {
                                        sourceBytes = fs.Read(buffer, 0, buffer.Length);
                                        OutputStream.Write(buffer, 0, sourceBytes);
                                    } while (sourceBytes > 0);
                                }
                            }
                        }
                    }


                    // Copy the file to the right location
                    var fileSize = OutputStream.Length / 1024;

                    response.Add("fileSize", fileSize);

                    // Finish is important to ensure trailing information for a Zip file is appended.  Without this
                    // the created file would be invalid.
                    OutputStream.Finish();

                    // Close is important to wrap things up and unlock the file.
                    OutputStream.Close();


//                        Console.WriteLine("FileSize " + fileSize);

                    if (fileSize > maxFileSize)
                    {
                        response["message"] =
                            "The game is too big to compile. You'll need to reduce the file size or increase the game size to create a new build.";

                        return response;
                    }

                    // Move the new build over 
                    exportPath = workspace.UniqueFilePath(exportPath.AppendDirectory("Build"));

//                            workspace.CreateDirectory(exportPath);

                    workspace.CreateDirectoryRecursive(exportPath);

                    exportPath = exportPath.AppendFile(tmpZipPath.EntityName);
                    workspace.Copy(tmpZipPath, exportPath);

                    response["success"] = true;
                    response["message"] = "A new build was created in " + exportPath + ".";
                    response["path"] = exportPath.Path;
                }
            }
            catch (Exception ex)
            {
                // No need to rethrow the exception as for our purposes its handled.
                response["message"] = "Unable to create a build for " + gameName + " " + ex;

//                    Console.WriteLine("Exception during processing {0}", ex);
            }

            return response;
        }

        public Dictionary<string, object> CreateExe(string name, WorkspacePath[] files, WorkspacePath template,
            WorkspacePath exportPath, string[] libFileNames = null)
        {
            var response = new Dictionary<string, object>
            {
                {"success", false},
                {"message", ""}
            };

//            var buildFilePath = template.ParentPath.AppendFile("build.json");
//
//            if (workspace.Exists(buildFilePath))
//            {
//                var buildText = "";
//
//                using (var file = workspace.OpenFile(buildFilePath, FileAccess.Read))
//                {
//                    buildText = file.ReadAllText();
//                    file.Close();
//                    file.Dispose();
//                }

                var platform = template.EntityName.Split(' ')[1].Split('.')[0];

                var contentPath = platform == "Mac" ? name + ".app/Contents/Resources/Content/DefaultGame/" : "Content/DefaultGame/";

                // Make sure the source is a pv8 file
                if (workspace.Exists(template) && template.GetExtension() == ".pvr")
                {
                    workspace.CreateDirectoryRecursive(exportPath);

                    exportPath = exportPath.AppendFile(name + ".zip");
                    
                    // Remove platform from name
                    name = name.Split(' ')[0];

                    using (Stream fsIn = workspace.OpenFile(template, FileAccess.Read))
                    using (var zfIn = new ZipFile(fsIn))
                    {

                        using (Stream fsOut = workspace.CreateFile(exportPath))
                        {

                            using (var zfOut = new ZipOutputStream(fsOut))
                            {



                                // Copy over all of the contents of the template to a new Zip file
                                foreach (ZipEntry zipEntry in zfIn)
                                {
                                    if (!zipEntry.IsFile)
                                    {
                                        // Ignore directories
                                        continue;
                                    }

                                    var entryFileName = zipEntry.Name;

                                    if (!entryFileName.Contains(contentPath))
                                    {
                                        Stream fsInput = null;
                                        long size = 0;

                                        // Check to see if there is a bios file
                                        if (entryFileName.EndsWith("bios.json"))
                                        {
                                            // Create a reader from a new copy of the zipEntry
                                            StreamReader reader = new StreamReader(zfIn.GetInputStream(zipEntry));

                                            // Read out all of the text
                                            var text = reader.ReadToEnd();
                                            //  
                                            // Replace the base directory with the game name and no spaces
                                            text = text.Replace(@"GameRunner",
                                                name.Replace(" " + platform, " ").Replace(" ", ""));

                                            text = text.Replace(@"PV8 Game Runner",
                                                name.Replace(" " + platform, ""));

                                            // Create a new memory stream in place of the zip file entry
                                            fsInput = new MemoryStream();

                                            // Wrap the stream in a writer
                                            var writer = new StreamWriter(fsInput);

                                            // Write the text to the stream
                                            writer.Write(text);

                                            // Flush the stream and set it back to the begining
                                            writer.Flush();
                                            fsInput.Seek(0, SeekOrigin.Begin);

                                            // Get the size so we know how big it is later on
                                            size = fsInput.Length;
                                        }



                                        // Clean up path for mac builds
                                        if (platform == "Mac")
                                        {
                                            if (entryFileName.StartsWith("Runner.app"))
                                            {
                                                // We rename the default Runner.app path to the game name to rename everything in the exe
                                                entryFileName = entryFileName.Replace("Runner.app", name + ".app");
                                            }
                                        }
                                        else
                                        {

                                            //                                            fsInput = new MemoryStream();
                                            // We need to look for the launch script
                                            if (entryFileName == "Pixel Vision 8 Runner")
                                            {
                                                // Create a reader from a new copy of the zipEntry
                                                StreamReader reader = new StreamReader(zfIn.GetInputStream(zipEntry));

                                                // Read out all of the text
                                                string text = reader.ReadToEnd();
                                                //  
                                                // Replace the default name with the game name
                                                text = text.Replace(@"Pixel\ Vision\ 8\ Runner",
                                                    name.Replace(" ", @"\ "));

                                                // Create a new memory stream in place of the zip file entry
                                                fsInput = new MemoryStream();

                                                // Wrap the stream in a writer
                                                var writer = new StreamWriter(fsInput);

                                                // Write the text to the stream
                                                writer.Write(text);

                                                // Flush the stream and set it back to the begining
                                                writer.Flush();
                                                fsInput.Seek(0, SeekOrigin.Begin);

                                                // Get the size so we know how big it is later on
                                                size = fsInput.Length;
                                            }

                                            // Rename all the executibale files in the linux build
                                            entryFileName = entryFileName.Replace("Pixel Vision 8 Runner", name);

                                        }

                                        // Check to see if we have a stream
                                        if (fsInput == null)
                                        {
                                            // Get a stream from the current zip entry
                                            fsInput = zfIn.GetInputStream(zipEntry);
                                            size = zipEntry.Size;
                                        }

                                        using (fsInput)
                                        {
                                            ZipEntry newEntry = new ZipEntry(entryFileName)
                                            {
                                                DateTime = DateTime.Now, Size = size
                                            };


                                            zfOut.PutNextEntry(newEntry);

                                            var buffer = new byte[4096];

                                            StreamUtils.Copy(fsInput, zfOut, buffer);

                                            fsInput.Close();

                                            zfOut.CloseEntry();
                                        }

                                    }

                                }

                                // Copy over all of the game files
                                var list = from p in files
                                           where workspace.fileExtensions.Any(val => p.EntityName.EndsWith(val))
                                           select p;

                                // Readjust the content path
                                contentPath = platform == "Mac" ? name + ".app/Contents/Resources/Content/DefaultGame/" : "Content/DefaultGame/";
                                foreach (var file in list)
                                {
                                    var entryFileName = contentPath + file.EntityName;
                                    using (var fileStream = workspace.OpenFile(file, FileAccess.Read))
                                    {
                                        
                                        var newEntry = new ZipEntry(entryFileName)
                                        {
                                            DateTime = DateTime.Now,
                                            Size = fileStream.Length
                                        };

                                        zfOut.PutNextEntry(newEntry);

                                        var buffer = new byte[4096];

                                        StreamUtils.Copy(fileStream, zfOut, buffer);

                                        zfOut.CloseEntry();

                                        fileStream.Close();

                                    }
                                }

                                // Copy over all of the library files
                                if (libFileNames != null)
                                {
                                    var libFileData = new Dictionary<string, byte[]>();

                                    workspace.IncludeLibDirectoryFiles(libFileData);

                                    var total = libFileNames.Length;

                                    for (int i = 0; i < total; i++)
                                    {
                                        var fileName = libFileNames[i] + ".lua";

                                        if (libFileData.ContainsKey(fileName))
                                        {

                                            var entryFileName = contentPath + fileName;
                                            using (var fileStream = new MemoryStream(libFileData[fileName]))
                                            {


                                                var newEntry = new ZipEntry(entryFileName)
                                                {
                                                    DateTime = DateTime.Now,
                                                    Size = fileStream.Length
                                                };

                                                zfOut.PutNextEntry(newEntry);

                                                var buffer = new byte[4096];

                                                StreamUtils.Copy(fileStream, zfOut, buffer);

                                                zfOut.CloseEntry();

                                                fileStream.Close();
                                            }

                                        }
                                    }
                                }
                            }
                        }
                    }

//                }
            }

            return response;
        }

        private delegate bool SaveTextToFileDelegator(string filePath, string text, bool autoCreate = false);
    }
}