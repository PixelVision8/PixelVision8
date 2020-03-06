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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using MoonSharp.Interpreter;
using PixelVision8.Engine.Utils;
using PixelVision8.Runner.Exporters;
using PixelVision8.Runner.Importers;
using PixelVision8.Runner.Parsers;
using PixelVision8.Runner.Utils;
using PixelVision8.Runner.Workspace;

// TODO need to remove reference to this


namespace PixelVision8.Runner.Services
{
    

    public class LuaServicePlus : LuaService //, IPlatformAccessor // TODO need to map to these APIs
    {
        private readonly PNGWriter _pngWriter = new PNGWriter();
        private readonly WorkspaceServicePlus workspace;

        private SoundEffectInstance currentSound;
        protected PixelVision8Runner desktopRunner;

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
                new Func<string, Dictionary<WorkspacePath, WorkspacePath>, WorkspacePath, int, Dictionary<string, object>>(CreateDisk);
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

            luaScript.Globals["AddExporter"] = new Action<WorkspacePath, Image>(SaveImage);

            luaScript.Globals["NewWorkspacePath"] = new Func<string, WorkspacePath>(WorkspacePath.Parse);

            UserData.RegisterType<WorkspacePath>();
            UserData.RegisterType<Image>();

            // Experimental
            luaScript.Globals["DebugLayers"] = new Action<bool>(runner.DebugLayers);
            luaScript.Globals["ToggleLayers"] = new Action<int>(runner.ToggleLayers);

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

            var colorRefs = reader.colorPalette.Select(c => ColorUtils.RgbToHex(c.R, c.G, c.B)).ToArray();

            // Convert all of the pixels into color ids
            var pixelIDs = reader.colorPixels.Select(c => Array.IndexOf(colorRefs, ColorUtils.RgbToHex(c.R, c.G, c.B))).ToArray();

            return new Image(reader.width, reader.height, colorRefs, pixelIDs);


            // var colors = reader.colorPalette;
            //
            // var pixels = 
            //
            // // TODO need to convert the image data to colors and pixel data
            //
            // // TODO need to finish this parser 
            // return new Image(reader.width, reader.height, new[] {"ff00ff"});
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
            if (workspace.Exists(workspacePath) == false) return -1;

            if (workspacePath.IsDirectory) return 0;

            return workspace.OpenFile(workspacePath, FileAccess.Read).ReadAllBytes().Length / 1024;
        }


        public Dictionary<string, object> CreateDisk(string name, Dictionary<WorkspacePath, WorkspacePath> files, WorkspacePath dest, int maxFileSize = 512)
        {
            var fileLoader = new WorkspaceFileLoadHelper(workspace);

            dest = workspace.UniqueFilePath(dest.AppendDirectory("Build")).AppendPath(name + ".pv8");
            var diskExporter = new DiskExporter(dest.Path, fileLoader, files, maxFileSize);

            diskExporter.CalculateSteps();

            while (diskExporter.completed == false)
            {
                diskExporter.NextStep();
            }

            try
            {
                if ((bool)diskExporter.Response["success"])
                {
                    workspace.SaveExporterFiles(new Dictionary<string, byte[]>() { { diskExporter.fileName, diskExporter.bytes } });

                    // Update the response
                    diskExporter.Response["success"] = true;
                    diskExporter.Response["message"] = "A new build was created in " + dest + ".";
                    diskExporter.Response["path"] = dest.Path;
                }


            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                diskExporter.Response["success"] = false;
                diskExporter.Response["message"] = "Unable to create a build for '" + name + "'. " + e.Message;

            }

            //
            // // Create a path to the temp directory for the builds
            // var tmpExportPath = WorkspacePath.Root.AppendDirectory("Tmp").AppendDirectory("Builds");
            //
            // // Make sure there is a builds folder in the Tmp directory
            // if (workspace.Exists(tmpExportPath) == false) workspace.CreateDirectory(tmpExportPath);
            //
            // // Create a folder with the timestamp
            // tmpExportPath = tmpExportPath.AppendDirectory(DateTime.Now.ToString("yyyyMMddHHmmss"));
            // workspace.CreateDirectory(tmpExportPath);
            //
            // // Add the zip filename to it
            // var tmpZipPath = tmpExportPath.AppendFile(name + ".pv8");
            //
            // // TODO make sure using is ok here so it closes out the stream when done
            // // var stream = workspace.CreateFile(tmpZipPath);
            //
            // var response = workspace.CreateZipFile(tmpZipPath, files);
            //
            // if ((bool)response["success"])
            // {
            //
            //     // TODO need to add lib files to zip (but they are going to have different source and destination paths)
            //
            //     // Copy the file to the right location
            //     var fileSize = (long) response["fileSize"];
            //     
            //     if (fileSize > maxFileSize)
            //     {
            //
            //         // Change the response message to reflect that the file is to big to save
            //         response["message"] =
            //             "The game is too big to compile. You'll need to reduce the file size or increase the game size to create a new build.";
            //         
            //         // Set the success back to false
            //         response["success"] = false;
            //
            //         // Delete the temp file size it is too big
            //         workspace.Delete(tmpZipPath);
            //
            //         // return the response
            //         return response;
            //     }
            //
            //     // Move the new build over 
            //     dest = workspace.UniqueFilePath(dest.AppendDirectory("Build"));
            //
            //     // Create the directory for the new file
            //     workspace.CreateDirectoryRecursive(dest);
            //
            //     dest = dest.AppendFile(tmpZipPath.EntityName);
            //
            //     // Copy over the file
            //     workspace.Copy(tmpZipPath, dest);
            //
            //     // Update the response
            //     response["success"] = true;
            //     response["message"] = "A new build was created in " + dest + ".";
            //     response["path"] = dest.Path;
            // }
            // else
            // {
            //     // Update the message for the failed build
            //     response["message"] = "Unable to create a build for '" + name + "'. " + (string)response["message"];
            // }

            return diskExporter.Response;
        }

        public Dictionary<string, object> CreateExe(string name, WorkspacePath[] files, WorkspacePath template,
            WorkspacePath exportPath, string[] libFileNames = null)
        {
            var response = new Dictionary<string, object>
            {
                {"success", false},
                {"message", ""}
            };

            var platform = template.EntityName.Split(' ')[1].Split('.')[0];

            var contentPath = platform == "Mac"
                ? name + ".app/Contents/Resources/Content/DefaultGame/"
                : "Content/DefaultGame/";

            // Make sure the source is a pv8 file

            // TODO need to use the CreateZipFile and AddFilesToZip methods on the workspace
            // if (workspace.Exists(template) && template.GetExtension() == ".pvr")
            // {
            //     workspace.CreateDirectoryRecursive(exportPath);
            //
            //     exportPath = exportPath.AppendFile(name + ".zip");
            //
            //     // Remove platform from name
            //     name = name.Split(' ')[0];
            //
            //     using (var fsIn = workspace.OpenFile(template, FileAccess.Read))
            //     using (var zfIn = new ZipFile(fsIn))
            //     {
            //         using (var fsOut = workspace.CreateFile(exportPath))
            //         {
            //             using (var zfOut = new ZipOutputStream(fsOut))
            //             {
            //                 // Copy over all of the contents of the template to a new Zip file
            //                 foreach (ZipEntry zipEntry in zfIn)
            //                 {
            //                     if (!zipEntry.IsFile)
            //                         // Ignore directories
            //                         continue;
            //
            //                     var entryFileName = zipEntry.Name;
            //
            //                     if (!entryFileName.Contains(contentPath))
            //                     {
            //                         Stream fsInput = null;
            //                         long size = 0;
            //
            //                         // Check to see if there is a bios file
            //                         if (entryFileName.EndsWith("bios.json"))
            //                         {
            //                             // Create a reader from a new copy of the zipEntry
            //                             var reader = new StreamReader(zfIn.GetInputStream(zipEntry));
            //
            //                             // Read out all of the text
            //                             var text = reader.ReadToEnd();
            //                             //  
            //                             // Replace the base directory with the game name and no spaces
            //                             text = text.Replace(@"GameRunner",
            //                                 name.Replace(" " + platform, " ").Replace(" ", ""));
            //
            //                             text = text.Replace(@"PV8 Game Runner",
            //                                 name.Replace(" " + platform, ""));
            //
            //                             // Create a new memory stream in place of the zip file entry
            //                             fsInput = new MemoryStream();
            //
            //                             // Wrap the stream in a writer
            //                             var writer = new StreamWriter(fsInput);
            //
            //                             // Write the text to the stream
            //                             writer.Write(text);
            //
            //                             // Flush the stream and set it back to the begining
            //                             writer.Flush();
            //                             fsInput.Seek(0, SeekOrigin.Begin);
            //
            //                             // Get the size so we know how big it is later on
            //                             size = fsInput.Length;
            //                         }
            //
            //
            //                         // Clean up path for mac builds
            //                         if (platform == "Mac")
            //                         {
            //                             if (entryFileName.StartsWith("Runner.app"))
            //                                 // We rename the default Runner.app path to the game name to rename everything in the exe
            //                                 entryFileName = entryFileName.Replace("Runner.app", name + ".app");
            //                         }
            //                         else
            //                         {
            //                             //                                            fsInput = new MemoryStream();
            //                             // We need to look for the launch script
            //                             if (entryFileName == "Pixel Vision 8 Runner")
            //                             {
            //                                 // Create a reader from a new copy of the zipEntry
            //                                 var reader = new StreamReader(zfIn.GetInputStream(zipEntry));
            //
            //                                 // Read out all of the text
            //                                 var text = reader.ReadToEnd();
            //                                 //  
            //                                 // Replace the default name with the game name
            //                                 text = text.Replace(@"Pixel\ Vision\ 8\ Runner",
            //                                     name.Replace(" ", @"\ "));
            //
            //                                 // Create a new memory stream in place of the zip file entry
            //                                 fsInput = new MemoryStream();
            //
            //                                 // Wrap the stream in a writer
            //                                 var writer = new StreamWriter(fsInput);
            //
            //                                 // Write the text to the stream
            //                                 writer.Write(text);
            //
            //                                 // Flush the stream and set it back to the begining
            //                                 writer.Flush();
            //                                 fsInput.Seek(0, SeekOrigin.Begin);
            //
            //                                 // Get the size so we know how big it is later on
            //                                 size = fsInput.Length;
            //                             }
            //
            //                             // Rename all the executibale files in the linux build
            //                             entryFileName = entryFileName.Replace("Pixel Vision 8 Runner", name);
            //                         }
            //
            //                         // Check to see if we have a stream
            //                         if (fsInput == null)
            //                         {
            //                             // Get a stream from the current zip entry
            //                             fsInput = zfIn.GetInputStream(zipEntry);
            //                             size = zipEntry.Size;
            //                         }
            //
            //                         using (fsInput)
            //                         {
            //                             var newEntry = new ZipEntry(entryFileName)
            //                             {
            //                                 DateTime = DateTime.Now,
            //                                 Size = size
            //                             };
            //
            //
            //                             zfOut.PutNextEntry(newEntry);
            //
            //                             var buffer = new byte[4096];
            //
            //                             StreamUtils.Copy(fsInput, zfOut, buffer);
            //
            //                             fsInput.Close();
            //
            //                             zfOut.CloseEntry();
            //                         }
            //                     }
            //                 }
            //
            //                 // Copy over all of the game files
            //                 var list = from p in files
            //                     where workspace.fileExtensions.Any(val => p.EntityName.EndsWith(val))
            //                     select p;
            //
            //                 // Readjust the content path
            //                 contentPath = platform == "Mac"
            //                     ? name + ".app/Contents/Resources/Content/DefaultGame/"
            //                     : "Content/DefaultGame/";
            //                 foreach (var file in list)
            //                 {
            //                     var entryFileName = contentPath + file.EntityName;
            //                     using (var fileStream = workspace.OpenFile(file, FileAccess.Read))
            //                     {
            //                         var newEntry = new ZipEntry(entryFileName)
            //                         {
            //                             DateTime = DateTime.Now,
            //                             Size = fileStream.Length
            //                         };
            //
            //                         zfOut.PutNextEntry(newEntry);
            //
            //                         var buffer = new byte[4096];
            //
            //                         StreamUtils.Copy(fileStream, zfOut, buffer);
            //
            //                         zfOut.CloseEntry();
            //
            //                         fileStream.Close();
            //                     }
            //                 }
            //
            //                 // Copy over all of the library files
            //                 if (libFileNames != null)
            //                 {
            //                     var libFileData = new Dictionary<string, byte[]>();
            //
            //                     // TODO NEED TO INCLUDE LIB FILES HERE 
            //                     //workspace.IncludeLibDirectoryFiles(libFileData);
            //
            //                     var total = libFileNames.Length;
            //
            //                     for (var i = 0; i < total; i++)
            //                     {
            //                         var fileName = libFileNames[i] + ".lua";
            //
            //                         if (libFileData.ContainsKey(fileName))
            //                         {
            //                             var entryFileName = contentPath + fileName;
            //                             using (var fileStream = new MemoryStream(libFileData[fileName]))
            //                             {
            //                                 var newEntry = new ZipEntry(entryFileName)
            //                                 {
            //                                     DateTime = DateTime.Now,
            //                                     Size = fileStream.Length
            //                                 };
            //
            //                                 zfOut.PutNextEntry(newEntry);
            //
            //                                 var buffer = new byte[4096];
            //
            //                                 StreamUtils.Copy(fileStream, zfOut, buffer);
            //
            //                                 zfOut.CloseEntry();
            //
            //                                 fileStream.Close();
            //                             }
            //                         }
            //                     }
            //                 }
            //             }
            //         }
            //     }
            //
            //     //                }
            // }

            return response;
        }

        private delegate bool SaveTextToFileDelegator(string filePath, string text, bool autoCreate = false);
    }
}