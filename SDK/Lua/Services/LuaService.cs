//   
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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using MoonSharp.Interpreter;
using PixelVision8.Runner.Exporters;
using PixelVision8.Runner;
using PixelVision8.Runner.Workspace;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PixelVision8.Player;

namespace PixelVision8.Runner
{
    public class LuaService : AbstractService
    {
        protected DesktopRunner runner;
        private readonly PNGWriter _pngWriter = new PNGWriter();
        private readonly WorkspaceServicePlus workspace;

        private SoundEffectInstance currentSound;
        public DateTime now = DateTime.Now;

        /// <summary>
        ///     The LuaService exposes core Runner APIs to the Lua Game Chip
        /// </summary>
        /// <param name="runner"></param>
        public LuaService(DesktopRunner runner)
        {
            this.runner = runner;

            workspace = runner.workspaceService as WorkspaceServicePlus;
        }

        private Dictionary<string, string> bgScriptData = new Dictionary<string, string>();

        public string BackgroundScriptData(string key, string value = null)
        {
            if (value != null)
            {
                if (bgScriptData.ContainsKey(key))
                {
                    bgScriptData[key] = value;
                }
                else
                {
                    bgScriptData.Add(key, value);
                }
            }

            if (bgScriptData.ContainsKey(key))
            {
                return bgScriptData[key];
            }

            return "undefined";
        }

        /// <summary>
        ///     This service exposes some of the runner's APIs to Lua Games.
        /// </summary>
        /// <param name="luaScript"></param>
        public virtual void ConfigureScript(Script luaScript)
        {
            luaScript.Options.DebugPrint = runner.DisplayWarning;

            luaScript.Globals["Volume"] = new Func<int?, int>(runner.Volume);
            luaScript.Globals["Mute"] = new Func<bool?, bool>(runner.Mute);

            luaScript.Globals["Scale"] = new Func<int?, int>(runner.Scale);
            luaScript.Globals["Fullscreen"] = new Func<bool?, bool>(runner.Fullscreen);
            luaScript.Globals["CropScreen"] = new Func<bool?, bool>(runner.CropScreen);
            luaScript.Globals["StretchScreen"] = new Func<bool?, bool>(runner.StretchScreen);
            luaScript.Globals["EnableCRT"] = new Func<bool?, bool>(runner.EnableCRT);

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

            luaScript.Globals["RunBackgroundScript"] = new Func<string, string[], bool>(RunBackgroundScript);
            luaScript.Globals["BackgroundScriptData"] = new Func<string, string, string>(BackgroundScriptData);
            luaScript.Globals["CancelExport"] = new Action(CancelExport);

            luaScript.Globals["PlayWav"] = new Action<WorkspacePath>(PlayWav);
            luaScript.Globals["StopWav"] = new Action(StopWav);

            luaScript.Globals["CreateDisk"] =
                new Func<string, Dictionary<WorkspacePath, WorkspacePath>, WorkspacePath, int,
                    Dictionary<string, object>>(CreateDisk);

            luaScript.Globals["ClearLog"] = new Action(workspace.ClearLog);
            luaScript.Globals["ReadLogItems"] = new Func<List<string>>(workspace.ReadLogItems);

            // TODO these are deprecated
            luaScript.Globals["ReadTextFile"] = new Func<string, string>(ReadTextFile);
            luaScript.Globals["SaveTextToFile"] = (SaveTextToFileDelegator) SaveTextToFile;

            // File helpers
            luaScript.Globals["NewImage"] = new Func<int, int, string[], int[], ImageData>(NewImage);

            // Read file helpers
            luaScript.Globals["ReadJson"] = new Func<WorkspacePath, Dictionary<string, object>>(ReadJson);
            luaScript.Globals["ReadText"] = new Func<WorkspacePath, string>(ReadText);
            luaScript.Globals["ReadImage"] = new Func<WorkspacePath, string, string[], ImageData>(ReadImage);

            // Save file helpers
            luaScript.Globals["SaveText"] = new Action<WorkspacePath, string>(SaveText);
            luaScript.Globals["SaveImage"] = new Action<WorkspacePath, ImageData>(SaveImage);

            // luaScript.Globals["AddExporter"] = new Action<WorkspacePath, ImageData>(SaveImage);

            luaScript.Globals["NewWorkspacePath"] = new Func<string, WorkspacePath>(WorkspacePath.Parse);

            luaScript.Globals["CurrentTime"] = new Func<string>(CurrentTime);


            UserData.RegisterType<WorkspacePath>();
            UserData.RegisterType<ImageData>();

            // Experimental
            // luaScript.Globals["DebugLayers"] = new Action<bool>(runner.DebugLayers);
            // luaScript.Globals["ToggleLayers"] = new Action<int>(runner.ToggleLayers);
            luaScript.Globals["ResizeColorMemory"] = new Action<int, int>(ResizeColorMemory);
        }

        public void ResizeColorMemory(int newSize, int maxColors = -1)
        {
            // runner.ActiveEngine.ColorChip.maxColors = maxColors;
            runner.ActiveEngine.ColorChip.Total = newSize;
        }

        public string CurrentTime()
        {
            return now.ToString("HH:mmtt");
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

        public bool RunBackgroundScript(string scriptName, string[] args = null)
        {
            try
            {
                // filePath = UniqueFilePath(filePath.AppendFile("pattern+" + id + ".wav"));

                // TODO exporting sprites doesn't work
                if (runner.ServiceManager.GetService(typeof(GameDataExportService).FullName) is GameDataExportService
                    exportService)
                {
                    exportService.Restart();

                    bgScriptData.Clear();

                    exportService.AddExporter(new BackgroundScriptRunner(scriptName, this, args));
                    //
                    exportService.StartExport();

                    return true;
                }
            }
            catch (Exception e)
            {
                // TODO this needs to go through the error system?
                Console.WriteLine(e);
            }

            return false;
        }

        public void CancelExport()
        {
            if (runner.ServiceManager.GetService(typeof(GameDataExportService).FullName) is GameDataExportService
                exportService)
            {
                exportService.Cancel();
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

        public ImageData NewImage(int width, int height, string[] colors, int[] pixelData = null)
        {
            return new ImageData(width, height, pixelData, colors);
        }

        public Dictionary<string, object> ReadJson(WorkspacePath src)
        {
            var text = ReadText(src);

            return Json.Deserialize(text) as Dictionary<string, object>;
        }

        public void Write(WorkspacePath dest, Dictionary<string, object> data)
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
                colorRefs = reader.ColorPalette.Select(c => SpriteImageParser.RgbToHex(c.R, c.G, c.B)).ToArray();
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

        public void SaveImage(WorkspacePath dest, ImageData imageData)
        {
            var width = imageData.Width;
            var height = imageData.Height;
            var hexColors = imageData.Colors;

            // convert colors
            var totalColors = hexColors.Length;
            var colors = new Color[totalColors];
            for (var i = 0; i < totalColors; i++) colors[i] = DisplayTarget.HexToColor(hexColors[i]);

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

        private delegate bool SaveTextToFileDelegator(string filePath, string text, bool autoCreate = false);
    }
}