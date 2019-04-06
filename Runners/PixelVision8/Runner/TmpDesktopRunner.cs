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
using PixelVision8.Runner.Data;
using PixelVision8.Runner.Services;
using SharpFileSystem;
using SharpFileSystem.FileSystems;
using PixelVision8.Runner.Exporters;
using PixelVision8.Runner.Utils;
using Directory = System.IO.Directory;

namespace PixelVision8.Runner
{
    /// <summary>
    ///     This is the main type for your game.
    /// </summary>
    public class TmpDesktopRunner : RunnerGame
    {
//        protected string autoRunPath;
        public bool backKeyEnabled = true;
        public List<string> loadHistory = new List<string>();
        protected List<Dictionary<string, string>> metaDataHistory = new List<Dictionary<string, string>>();
        
        protected string rootPath;
        protected bool shutdown;
        public string systemName;
        public string systemVersion;
        public WorkspaceService workspaceService;

        public TmpDesktopRunner(string rootPath, string autoRunPath = null)
        {
            this.rootPath = rootPath;
//            this.autoRunPath = autoRunPath;
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
            Volume(MathHelper.Clamp(Convert.ToInt32((long) bios.ReadBiosData(BiosSettings.Volume.ToString(), 40L)), 0, 100));

            Mute(Convert.ToBoolean(bios.ReadBiosData(BiosSettings.Mute.ToString(), "False") as string));

            systemVersion = (string) bios.ReadBiosData(BiosSettings.SystemVersion.ToString(), "0.0.0");
            systemName = (string) bios.ReadBiosData("SystemName", "PixelVision8");

            Window.Title =
                (string) bios.ReadBiosData(BiosSettings.SystemName.ToString(), "Pixel Vision 8") + " " +
                systemVersion;
        }

        public override void ConfigureDisplayTarget()
        {
            // Get the virtual monitor resolution
            var tmpRes = ((string) bios.ReadBiosData(BiosSettings.Resolution.ToString(), "512x480"))
                .Split('x').Select(int.Parse)
                .ToArray();

            // TODO this may be a string
            try
            {
                Scale(Convert.ToInt32((long) bios.ReadBiosData(BiosSettings.Scale.ToString(), 1)));
            }
            catch
            {
                Scale(Convert.ToInt32((string) bios.ReadBiosData(BiosSettings.Scale.ToString(), "1")));
            }


            Fullscreen(Convert.ToBoolean(
                bios.ReadBiosData(BiosSettings.FullScreen.ToString(), "False") as string));
            StretchScreen(
                Convert.ToBoolean(
                    bios.ReadBiosData(BiosSettings.StretchScreen.ToString(), "False") as string));
            CropScreen(Convert.ToBoolean(
                bios.ReadBiosData(BiosSettings.CropScreen.ToString(), "False") as string));
            // Create the default display target

            displayTarget = new DisplayTarget(graphics, tmpRes[0], tmpRes[1], Fullscreen());
        }

        public BiosService bios;

        protected virtual void CreateWorkspaceService()
        {
            workspaceService = new WorkspaceService(new KeyValuePair<FileSystemPath, IFileSystem>(
                FileSystemPath.Root.AppendDirectory("App"),
                new PhysicalFileSystem(rootPath)));
        }
        
        /// <summary>
        ///     Override the base initialize() method and setup the file system for PV8 to run on the desktop.
        /// </summary>
        protected override void Initialize()
        {
            // Create a workspace
            CreateWorkspaceService();
                
            // Add root path
//            workspaceService.fileSystem.Mounts.Add();
            
            
            // Create a path to the system bios
            var biosPath = FileSystemPath.Root.AppendDirectory("App").AppendFile("bios.json");//Path.Combine(rootPath, "bios.json")));

            // Test if a bios file exists
            if (workspaceService.fileSystem.Exists(biosPath))
            {
                
                // Read the bios text
                var biosText = workspaceService.ReadTextFromFile(biosPath);
//                
                bios = new BiosService();

                try
                {
                    bios.ParseBiosText(biosText);
                }
                catch
                {
//                    DisplayBootErrorScreen("Error parsing system bios.");
                }
                
                ConfigureWorkspace();
                
                
//                if (workspaceService.fileSystem.Exists(FileSystemPath.Root.AppendDirectory("PixelVisionOS")))
//                {
             
                // Initialize the runner
                ConfigureRunner();

                LoadDefaultGame();  
//                }
//                else
//                {
//                    // TODO No OS Found
//                }

            }
            else
            {
                // TODO no bios found
//                DisplayError(ErrorCode.LoadError, new Dictionary<string, string>(){{"LoadError", biosPath.ToString()}});
            }
        }
        
        protected virtual void LoadDefaultGame()
        {

            var gamePath = (string) bios.ReadBiosData("AutoRun", "/App/DefaultGame/");

            var filePath = FileSystemPath.Parse(gamePath);

            var files = workspaceService.fileSystem.GetEntities(filePath);
            
            
            // Boot the game
            Load((string) bios.ReadBiosData("AutoRun", "/App/DefaultGame/"), RunnerMode.Booting);

        }

        protected string tmpPath;
        
        protected virtual void ConfigureWorkspace()
        {
            var mounts = new Dictionary<FileSystemPath, IFileSystem>();

            // Create the base directory in the documents and local storage folder
                
            // Get the base directory from the bios or use Pixel Vision 8 as the default name
            var baseDir = bios.ReadBiosData("BaseDir", "PixelVision8") as string;
                
            tmpPath = Path.Combine(LocalStorage, baseDir, "Tmp");
                
            // Create an array of required directories
            var requiredDirectories = new Dictionary<string, string>()
            {
                {"Storage", Path.Combine(LocalStorage, baseDir)},
                {"Tmp", tmpPath}
            };
                
            // Loop through the list of directories, make sure they exist and create them
            foreach (var directory in requiredDirectories)
            {
                if (!Directory.Exists(directory.Value))
                {

                    Directory.CreateDirectory(directory.Value);
                        
                }
                    
                // Add directories to mount points
                mounts.Add(FileSystemPath.Root.AppendDirectory(directory.Key), new PhysicalFileSystem(directory.Value));
                    
            }
                
            // Mount the filesystem
            workspaceService.MountFileSystems(mounts);
            
            // Load bios from the user's storage folder
            LoadBios(new[] {userBiosPath});
                
            
            
//            // Custom to PV8
//            
//            documentsPath = Path.Combine(Documents, baseDir);
//
//            
//            workspaceService.fileSystem.Mounts.Add(new KeyValuePair<FileSystemPath, IFileSystem>(
//                FileSystemPath.Root.AppendDirectory("User"),
//                new PhysicalFileSystem(documentsPath)));
//            
//            // Build the OS Folder
//    
//            osFileSystem = new MergedFileSystem();
//
//            osFileSystem.FileSystems = osFileSystem.FileSystems.Concat(new[] { new SubFileSystem(workspaceService.fileSystem,
//                FileSystemPath.Root.AppendDirectory("App").AppendDirectory("PixelVisionOS")) });
//                
//            // Mount the PixelVisionOS directory
//            workspaceService.fileSystem.Mounts.Add(new KeyValuePair<FileSystemPath, IFileSystem>(FileSystemPath.Root.AppendDirectory("PixelVisionOS"), osFileSystem));
//                
            workspaceService.SetupLogFile(FileSystemPath.Parse(bios.ReadBiosData("LogFilePath", "/Tmp/Log.txt") as string));

        }
        
        public void LoadBios(FileSystemPath[] paths)
        {
            for (var i = 0; i < paths.Length; i++)
            {
                var path = paths[i];

                if (workspaceService.fileSystem.Exists(path))
                {
                    var json = workspaceService.ReadTextFromFile(path);

                    bios.ParseBiosText(json);
                }
            }

            ConfigureWorkspaceSettings();
        }
        
        protected void ConfigureWorkspaceSettings()
        {
            workspaceService.archiveExtensions =
                ((string) bios.ReadBiosData("ArchiveExtensions", "zip,pv8,pvt,pvs,pva")).Split(',')
                .ToList(); //new List<string> {"zip", "pv8", "pvt", "pvs", "pva"});
            workspaceService.fileExtensions =
                ((string) bios.ReadBiosData("FileExtensions", "png,lua,json,txt")).Split(',')
                .ToList(); //new List<string> {"png", "lua", "json", "txt"};
//            gameFolders = ((string)ReadBiosData("GameFolders", "Games,Systems,Tools")).Split(',').ToList();//new List<string> {"zip", "pv8", "pvt", "pvs", "pva"});

            workspaceService.requiredFiles =
                ((string) bios.ReadBiosData("RequiredFiles", "data.json,info.json")).Split(',')
                .ToList(); //new List<string> {"zip", "pv8", "pvt", "pvs", "pva"});
            
            workspaceService.osLibPath = FileSystemPath.Root.AppendDirectory("PixelVisionOS")
                .AppendDirectory((string) bios.ReadBiosData("LibsDir", "Libs"));
            workspaceService.workspaceLibPath = FileSystemPath.Root.AppendDirectory("Workspace")
                .AppendDirectory((string) bios.ReadBiosData("LibsDir", "Libs"));

//            workspaceService.spriteBuilderFolderName = (string) bios.ReadBiosData("SpriteBuilderDir", "SpriteBuilder");
        }
        
        protected FileSystemPath userBiosPath => FileSystemPath.Parse("/Storage/user-bios.json");

        public void SaveBiosChanges()
        {
            // TODO need to update this
//            var path = FileSystemPath.Parse(userBiosPath);

            // Look for changes
            if (bios.userBiosChanges != null)
            {
                // Get the path to where the user's bios should be saved
//                var path = FileSystemPath.Parse("/User/").AppendFile("user-bios.json");

                if (!workspaceService.fileSystem.Exists(userBiosPath))
                {
                    var newBios = workspaceService.fileSystem.CreateFile(userBiosPath);
                    newBios.Close();
                }

                // Create a user data dictionary
                var userData = new Dictionary<string, object>();

                // Load the raw data for ther user's bio
                var json = workspaceService.ReadTextFromFile(userBiosPath);

                // If the json file isn't empty, deserialize it
                if (json != "") userData = Json.Deserialize(json) as Dictionary<string, object>;

                // Loop through each of the items in the uerBiosChanges dictionary
                foreach (var pair in bios.userBiosChanges)
                    // Set the changed values over any existing values from the json
                    if (userData.ContainsKey(pair.Key))
                        userData[pair.Key] = pair.Value;
                    else
                        userData.Add(pair.Key, pair.Value);


                // Save the new bios data back to the user's bios file.
                workspaceService.SaveTextToFile(userBiosPath, Json.Serialize(userData), true);
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
                var rawValue = bios.ReadBiosData(keyMap.Key.ToString(), keyMap.Value, true);
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

//            UpdateDiskInBios();
            SaveBiosChanges();
            
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

                // TODO need to add back in disk logic
                // Eject the fist disk
//                workspaceService.EjectDisk();
//
//                UpdateDiskInBios();
            }
//            }
        }
        
//        public void UpdateDiskInBios()
//        {
//            var paths = workspaceService.diskDrives.physicalPaths;
//
//            for (var i = 0; i < paths.Length; i++) bios.UpdateBiosData("Disk" + i, paths[i]);
//        }

        public virtual bool Load(string path, RunnerMode newMode = RunnerMode.Playing,
            Dictionary<string, string> metaData = null)
        {
            // Reset auto run to be true each time a disk is loaded
            workspaceService.autoRunEnabled = true;

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
            var files = workspaceService.LoadGame(path);

            if (files != null)
            {
                // Read and Run the disk
                ProcessFiles(tmpEngine, files, displayProgress);
                success = true;
            }
            else
            {
                DisplayError(ErrorCode.LoadError, new Dictionary<string, string> {{"@{path}", path}});
                success = false;
            }
            
            // If the game is unable to run, display an error
//            if (success == false) 

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
            return (string) bios.ReadBiosData(code.ToString(), "Error code " + (int) code);
        }

        protected virtual void LoadError(Dictionary<string, string> metaData)
        {
            var tool = (string) bios.ReadBiosData("ErrorTool", "/PixelVisionOS/Tools/ErrorTool/");

            workspaceService.UpdateLog(metaData["errorMessage"], LogType.Error,
                metaData.ContainsKey("exceptionMessage") ? metaData["exceptionMessage"] : null);

            Load(tool, RunnerMode.Error, metaData);
        }
    }
}