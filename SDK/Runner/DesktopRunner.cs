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
using System.Text;
using Microsoft.Xna.Framework;
using PixelVision8.Engine;
using PixelVision8.Engine.Chips;
using PixelVision8.Engine.Utils;
using PixelVision8.Runner.Data;
using PixelVision8.Runner.Services;
using SharpFileSystem;
using SharpFileSystem.FileSystems;
using PixelVision8.Runner.Exporters;

using Directory = System.IO.Directory;
using File = System.IO.File;

namespace PixelVision8.Runner
{
    /// <summary>
    ///     This is the main type for your game.
    /// </summary>
    public class DesktopRunner : RunnerGame
    {
        protected string autoRunPath;
        public bool backKeyEnabled = true;
        public List<string> loadHistory = new List<string>();
        protected List<Dictionary<string, string>> metaDataHistory = new List<Dictionary<string, string>>();
        
        protected string rootPath;
        protected bool shutdown;
        public string systemName;
        public string systemVersion;
        public WorkspaceService workspaceService;

        public DesktopRunner(string rootPath, string autoRunPath = null)
        {
            this.rootPath = rootPath;
            this.autoRunPath = autoRunPath;
        }

        // Default path to where PV8 workspaces will go
        protected string Documents => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        public string sessionID { get; protected set; }


        protected override void ConfigureRunner()
        {
            base.ConfigureRunner();

            // Save the session ID
            sessionID = DateTime.Now.ToString("yyyyMMddHHmmssffff");

            // TODO This may be a string
            Volume(MathHelper.Clamp(Convert.ToInt32((long) workspaceService.ReadBiosData(BiosSettings.Volume.ToString(), 40L)), 0, 100));

            Mute(Convert.ToBoolean(workspaceService.ReadBiosData(BiosSettings.Mute.ToString(), "False") as string));

            systemVersion = (string) workspaceService.ReadBiosData(BiosSettings.SystemVersion.ToString(), "0.0.0");
            systemName = (string) workspaceService.ReadBiosData("SystemName", "PixelVision8");

            Window.Title =
                (string) workspaceService.ReadBiosData(BiosSettings.SystemName.ToString(), "Pixel Vision 8") + " " +
                systemVersion;
        }

        public override void ConfigureDisplayTarget()
        {
            // Get the virtual monitor resolution
            var tmpRes = ((string) workspaceService.ReadBiosData(BiosSettings.Resolution.ToString(), "512x480"))
                .Split('x').Select(int.Parse)
                .ToArray();

            // TODO this may be a string
            try
            {
                Scale(Convert.ToInt32((long) workspaceService.ReadBiosData(BiosSettings.Scale.ToString(), 1)));
            }
            catch
            {
                Scale(Convert.ToInt32((string) workspaceService.ReadBiosData(BiosSettings.Scale.ToString(), "1")));
            }


            Fullscreen(Convert.ToBoolean(
                workspaceService.ReadBiosData(BiosSettings.FullScreen.ToString(), "False") as string));
            StretchScreen(
                Convert.ToBoolean(
                    workspaceService.ReadBiosData(BiosSettings.StretchScreen.ToString(), "False") as string));
            CropScreen(Convert.ToBoolean(
                workspaceService.ReadBiosData(BiosSettings.CropScreen.ToString(), "False") as string));
            // Create the default display target

            displayTarget = new DisplayTarget(graphics, tmpRes[0], tmpRes[1], Fullscreen());
        }

        /// <summary>
        ///     Override the base initialize() method and setup the file system for PV8 to run on the desktop.
        /// </summary>
        protected override void Initialize()
        {
            // Create a path to the system bios
            var biosPath = Path.Combine(rootPath, "bios.json");

            // Test if a bios file exists
            if (File.Exists(biosPath))
            {
                // Read the bios text
                var biosText = File.ReadAllText(biosPath);

                var bios = new BiosService();

                try
                {
                    bios.ParseBiosText(biosText);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
//                    DisplayBootErrorScreen("Error parsing system bios.");
                }

                // Create a workspace
                workspaceService = new WorkspaceService(bios, this);
                
                
                
                
                var mounts = new Dictionary<FileSystemPath, IFileSystem>();

                // Get the base directory from the bios or use Pixel Vision 8 as the default name
                var baseDir = bios.ReadBiosData("BaseDir", "PixelVision8") as string;

                // Create an array of required directories
                var requiredDirectories = new Dictionary<string, string>
                {
                    {"App", rootPath},
                    {"User", Path.Combine(Documents, baseDir)},
                    {"Storage", Path.Combine(LocalStorage, baseDir)},
                    {"Tmp", Path.Combine(LocalStorage, baseDir, "Tmp")}
                };

                // Loop through the list of directories, make sure they exist and create them
                foreach (var directory in requiredDirectories)
                {
                    if (!Directory.Exists(directory.Value)) Directory.CreateDirectory(directory.Value);

                    // Add directories to mount points
                    mounts.Add(FileSystemPath.Root.AppendDirectory(directory.Key),
                        new PhysicalFileSystem(directory.Value));
                }

                // Mount the filesystem
                workspaceService.MountFileSystems(mounts);

                // Mount the PixelVisionOS directory
//                workspaceService.fileSystem.Mounts.Add(new KeyValuePair<FileSystemPath, IFileSystem>(
//                    FileSystemPath.Root.AppendDirectory("PixelVisionOS"),
//                    new SubFileSystem(workspaceService.fileSystem,
//                        FileSystemPath.Root.AppendDirectory("App").AppendDirectory("PixelVisionOS"))));


                workspaceService.SetupLogFile(
                    FileSystemPath.Parse(workspaceService.ReadBiosData("LogFilePath", "/Tmp/Log.txt") as string));

//                if (workspaceService.fileSystem.Exists(FileSystemPath.Root.AppendDirectory("PixelVisionOS")))
//                {
                    // Initialize the runner
                    ConfigureRunner();

                    
                    var biosAutoRun = FileSystemPath.Parse((string) workspaceService.ReadBiosData("AutoRun", ""));
                
                    if (workspaceService.fileSystem.Exists(biosAutoRun))
                    {

                        if (workspaceService.ValidateGameInDir(biosAutoRun))
                        {
                            Load(biosAutoRun.Path);
                            return;
                        }
                    
                    }
                    
                    // Boot the game
                    Load(autoRunPath);
//                }
//                else
//                {
//                    Console.WriteLine("No OS found.");
//                }
            }
            else
            {
                Console.WriteLine("Error: No Bios file found.");
            }
        }

        public override void ConfigureServices()
        {
            base.ConfigureServices();

            tmpEngine.AddService(typeof(WorkspaceService).FullName, workspaceService);
        }

        protected override void Update(GameTime gameTime)
        {
            if (shutdown)
                return;

            base.Update(gameTime);
            
        }

        protected override void ConfigureKeyboard()
        {
            // Pass input mapping
            foreach (var keyMap in defaultKeys)
            {
                var rawValue = workspaceService.ReadBiosData(keyMap.Key.ToString(), keyMap.Value, true);
                if (rawValue is long)
                    rawValue = Convert.ToInt32(rawValue);

                var keyValue = rawValue;

                tmpEngine.SetMetaData(keyMap.Key.ToString(), keyValue.ToString());
            }

            tmpEngine.controllerChip.RegisterKeyInput();
        }

//        public virtual void LoadDefaultGame()
//        {
//            
//            // TODO this should just load the default game path?
//            
//        }

        

        protected override void OnExiting(object sender, EventArgs args)
        {
            ShutdownSystem();

            base.OnExiting(sender, args);
        }

        public void ShutdownSystem()
        {
            // We only want to call this once so don't run if shutdown is true
            if (shutdown)
                return;

            // Shutdown the active game
            ShutdownActiveEngine();

            // Toggle the shutdown flag
            shutdown = true;

            // Save any changes to the bios to the user's custom bios file
            workspaceService.ShutdownSystem();
        }

        public override void DisplayWarning(string message)
        {
            workspaceService.UpdateLog(message);
        }

        public virtual void Back()
        {
            if (mode == RunnerMode.Loading)
                return;

//            if (mode == RunnerMode.Booting)
//            {
//                BootDone();
//            }
//            else
//            {
            if (loadHistory.Count > 1)
            {
                var path = loadHistory.First();

                loadHistory.Clear();

                // TODO need to see if its a tool or a game?
                Load(path, RunnerMode.Loading);
            }
            else
            {
                loadHistory.Clear();

                // Eject the fist disk
                workspaceService.EjectDisk();
            }
//            }
        }

        public virtual bool Load(string path, RunnerMode newMode = RunnerMode.Playing,
            Dictionary<string, string> metaData = null)
        {
            // Reset auto run to be true each time a disk is loaded
            workspaceService.autoRunEnabled = true;

            
            // TODO need to depricate the preloader argument
//            if (preload == true)
//            {
//                newMode = RunnerMode.Loading;
//            }
            
            
            // If we are going to preload, save the current load call and change the values for the preloader tool
//            if (newMode == RunnerMode.Loading)
//            {
//                nextPathToLoad = path;
//                nextMetaData = metaData;
//                nextMode = RunnerMode.Playing;
//
//                // Create new meta data for the pre-loader
//                metaData = new Dictionary<string, string>
//                {
//                    {"nextMode", nextMode.ToString()},
//                    {"showDiskAnimation", "false"}
//                };
//
//                // Look to see if the game's meta data changes the disk animation flag
//                if (nextMetaData != null && nextMetaData.ContainsKey("showDiskAnimation"))
//                    metaData["showDiskAnimation"] = nextMetaData["showDiskAnimation"];
//
//                // Get the default path to the load tool from the bios
//                path = (string) workspaceService.ReadBiosData("LoadTool", "/PixelVisionOS/Tools/LoadTool/");
//
//                // Change the mode to loading
//                newMode = RunnerMode.Loading;
//            }

            // Create a new meta data dictionary if one doesn't exist yet
            if (metaData == null) metaData = new Dictionary<string, string>();

            // Spit path and convert to a list
            var splitPath = path.Split('/').ToList();
            var gameName = "";
            var rootPath = "/";
            var total = splitPath.Count - 1;

            // Loop through each item in the path and get the game name and root path
            for (var i = 1; i < total; i++)
                if (i < total - 1)
                    rootPath += splitPath[i] + "/";
                else
                    gameName = splitPath[i];

//            Console.WriteLine("Load "+ gameName + " in " + rootPath);

            // Add the game's name and root path to the meta data
            if (metaData.ContainsKey("GameName"))
                metaData["GameName"] = gameName;
            else
                metaData.Add("GameName", gameName);

            // Change the root path for the game's meta data
            if (metaData.ContainsKey("RootPath"))
                metaData["RootPath"] = rootPath;
            else
                metaData.Add("RootPath", rootPath);

            // Update the runner mode
            mode = newMode;

            // When playing a game, save it to history and the meta data so it can be reloaded correctly
            if (mode == RunnerMode.Playing)
            {
                lastMode = mode;
                loadHistory.Add(path);
                metaDataHistory.Add(metaData);
            }

            // Create a new tmpEngine
            ConfigureEngine(metaData);

            // Path the full path to the engine's name
            tmpEngine.name = path;

            bool success;

            // Have the workspace run the game from the current path
            success = workspaceService.LoadGame(path, tmpEngine, this, displayProgress);

            // If the game is unable to run, display an error
            if (success == false) DisplayError(ErrorCode.NoAutoRun, new Dictionary<string, string> {{"@{path}", path}});

            // Re-enable back when loading a new game
            backKeyEnabled = true;

            // Create new FileSystemPath
            return success;
        }

        public virtual void DisplayError(ErrorCode code, Dictionary<string, string> tokens = null,
            Exception exception = null)
        {
//			Debug.Log("Display Error");

            if (mode == RunnerMode.Error)
                return;

            // var id = (int) code;

            var sb = new StringBuilder();

            // Pull the error message from the bios
            var message = GetErrorMessage(code);

            sb.Append(message);

            if (tokens != null)
                foreach (var entry in tokens)
                    sb.Replace(entry.Key, entry.Value);

            // Make sure we stop preloading if we crash during the process
//            loading = false;

            var messageString = sb.ToString();

            var metaData = new Dictionary<string, string>
            {
                {"errorMessage", messageString}
            };

            string exceptionMessage = null;

            if (exception != null) metaData["exceptionMessage"] = exception.StackTrace;


            // TODO need to be able to pass in an error
            LoadError(metaData);
        }

        public virtual void SaveGameData(string path, IEngine engine, SaveFlags saveFlags, bool useSteps = true)
        {
            
            // Simple save game exporter

            var saveExporter = new SavedDataExporter(path, engine);
            saveExporter.CalculateSteps();
            while (saveExporter.completed == false)
            {
                saveExporter.NextStep();
            }
            
            // Save file
            var saveFile = new Dictionary<string, byte[]>()
            {
                {saveExporter.fileName, saveExporter.bytes}
            };
            
            workspaceService.SaveExporterFiles(saveFile);
            
        }

        protected string GetErrorMessage(ErrorCode code)
        {
            return (string) workspaceService.ReadBiosData(code.ToString(), "Error code " + (int) code);
        }

        protected virtual void LoadError(Dictionary<string, string> metaData)
        {
            var tool = (string) workspaceService.ReadBiosData("ErrorTool", "/PixelVisionOS/Tools/ErrorTool/");

            workspaceService.UpdateLog(metaData["errorMessage"], LogType.Error,
                metaData.ContainsKey("exceptionMessage") ? metaData["exceptionMessage"] : null);

            Load(tool, RunnerMode.Error, metaData);
        }
    }
}