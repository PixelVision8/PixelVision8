using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using MoonSharp.Interpreter;
using PixelVision8.Engine;
using PixelVision8.Engine.Chips;
using PixelVision8.Runner.Services;
using SharpFileSystem;
using SharpFileSystem.FileSystems;
using Color = Microsoft.Xna.Framework.Color;
using Directory = System.IO.Directory;
using File = System.IO.File;

namespace PixelVision8.Runner
{
    
    public enum ActionKeys  
    {
//            RunKey,
        ScreenShotKey,
        RecordKey,
        RestartKey,
        BackKey
    }
    
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class PixelVision8Runner : DesktopRunner
    {
        private MergedFileSystem osFileSystem;
        private ScreenshotService screenshotService;
        protected bool screenShotActive;
        protected float screenshotTime = 0;
        protected float screenshotDelay = .2f;
        protected IControllerChip controllerChip;
        protected Dictionary<string, string> nextMetaData;
        protected RunnerMode nextMode;
        protected string nextPathToLoad;
        private string documentsPath;
        public bool backKeyEnabled = true;
        public List<string> loadHistory = new List<string>();
        protected List<Dictionary<string, string>> metaDataHistory = new List<Dictionary<string, string>>();
        protected string Documents => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        public string sessionID { get; protected set; }

//        protected string rootPath;
        protected bool shutdown;
        public string systemName;
        public string systemVersion;
        
        public ExportService exportService { get; private set; }

        public override List<string> defaultChips {
            get
            {
                
                // Get the list of default chips
                var chips = base.defaultChips;
                
                // Add the custom Lua Game chip
                chips.Add(typeof(LuaGameChip).FullName);
                
                // Return the list of chips
                return chips;
            }
        }
        
        public readonly Dictionary<ActionKeys, Keys> actionKeys = new Dictionary<ActionKeys, Keys>()
        {
            // TODO need to add the other action keys here so they are mapped in the bios
            {ActionKeys.BackKey, Keys.Escape},
            {ActionKeys.ScreenShotKey, Keys.Alpha2},
            {ActionKeys.RecordKey, Keys.Alpha3},
            {ActionKeys.RestartKey, Keys.Alpha4},
        };
        
        // Default path to where PV8 workspaces will go
        public PixelVision8Runner(string rootPath) : base(rootPath)
        {
        }

//        private string Documents => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
//        private string rootPath
//        {
//            get
//            {
//                
//                var tmpRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content");
//                
//                // Need to change the rootPath if this is running on MacOS
//                if (tmpRoot.EndsWith("/MonoBundle/Content"))
//                {
//                    tmpRoot = tmpRoot.Replace("/MonoBundle/Content", "/Resources/Content");
//                }
//
//                return tmpRoot;
//            }
//        }
        
        
        

        protected override void ConfigureRunner()
        {
            base.ConfigureRunner();
            
            // Save the session ID
            sessionID = DateTime.Now.ToString("yyyyMMddHHmmssffff");

            systemVersion = (string) bios.ReadBiosData(BiosSettings.SystemVersion.ToString(), "0.0.0");
            systemName = (string) bios.ReadBiosData("SystemName", "PixelVision8");
            
            screenshotService = new ScreenshotService(workspaceServicePlus);
            exportService = new ExportService(null); //TODO Need to create a new AudioClipAdaptor
            
            var actions = Enum.GetValues(typeof(ActionKeys)).Cast<ActionKeys>();
            foreach (var action in actions)
            {
                actionKeys[action] = (Keys)Convert.ToInt32((long)bios.ReadBiosData(action.ToString(), (long)actionKeys[action], true));
            }
            
            
            // TODO need to fix the path to /PixelVisionOS/Effects/
//            var shaderPath = Path.Combine(rootPath, "crt-lottes-mg.ogl.mgfxo");


            
            
            // Set the display bios values here but this should be in the base runner?
//            Brightness(Convert.ToSingle((long) workspaceServicePlus.ReadBiosData(BiosSettings.Brightness.ToString(), 100L))/100F);
//            Sharpness(Convert.ToSingle((long) workspaceServicePlus.ReadBiosData(BiosSettings.Sharpness.ToString(), -6L)));

        }

        /// <summary>
        ///     Reads the current session ID which is a time stamp of when the Game Creator was booted up. This can be
        ///     used to help identify the state a tool should return too based on if it was last used in the same session.
        /// </summary>
        /// <returns>Returns a string timestamp formatting as yyyyMMddHHmmssffff.</returns>
//        public string ReadSessionID()
//        {
//            return sessionID;
//        }
//
//        public string SystemVersion()
//        {
//            return systemVersion;
//        }
//
//        public string SystemName()
//        {
//            return systemName;
//        }

//        public string ReadDocumentPath()
//        {
//            return _documentsPath;
//        }
//
//        public string ReadTmpPath()
//        {
//            return _tmpPath;
//        }

        /// <summary>
        ///     Override the base initialize() method and setup the file system for PV8 to run on the desktop.
        /// </summary>
//        protected override void Initialize()
//        {
//
//            // Create a workspace
//            workspaceServicePlus = new workspaceServicePlusPlus(this);
//                
//            // Add root path
//            workspaceServicePlus.fileSystem.Mounts.Add(new KeyValuePair<FileSystemPath, IFileSystem>(
//                FileSystemPath.Root.AppendDirectory("App"),
//                new PhysicalFileSystem(rootPath)));
//            
//            
//            // Create a path to the system bios
//            var biosPath = FileSystemPath.Root.AppendDirectory("App").AppendFile("bios.json");//Path.Combine(rootPath, "bios.json")));
//
//            // Test if a bios file exists
//            if (workspaceServicePlus.fileSystem.Exists(biosPath))
//            {
//                
//                // Read the bios text
//                var biosText = workspaceServicePlus.ReadTextFromFile(biosPath);
////                
//                bios = new BiosService();
//
//                try
//                {
//                    bios.ParseBiosText(biosText);
//                }
//                catch
//                {
////                    DisplayBootErrorScreen("Error parsing system bios.");
//                }
//                
//                ConfigureWorkspace();
//                
//                
////                if (workspaceServicePlus.fileSystem.Exists(FileSystemPath.Root.AppendDirectory("PixelVisionOS")))
////                {
//             
//                // Initialize the runner
//                ConfigureRunner();
//
//                LoadDefaultGame();
////                }
////                else
////                {
////                    // TODO No OS Found
////                }
//
//            }
//            else
//            {
//                // TODO no bios found
////                DisplayError(ErrorCode.LoadError, new Dictionary<string, string>(){{"LoadError", biosPath.ToString()}});
//            }
//            
//        }

        protected WorkspaceServicePlus workspaceServicePlus;
        
        protected override void CreateWorkspaceService()
        {
            workspaceServicePlus = new WorkspaceServicePlus(this, new KeyValuePair<FileSystemPath, IFileSystem>(
                FileSystemPath.Root.AppendDirectory("App"),
                new PhysicalFileSystem(rootPath)));

            // Pass the new service back to the base class
            workspaceService = workspaceServicePlus;
            
            serviceManager.AddService(typeof(WorkspaceService).FullName, workspaceService);

        }

        protected override void ConfigureWorkspace()
        {
//            var mounts = new Dictionary<FileSystemPath, IFileSystem>();
//
//            // Create the base directory in the documents and local storage folder
//                
//            // Get the base directory from the bios or use Pixel Vision 8 as the default name
//            var baseDir = bios.ReadBiosData("BaseDir", "PixelVision8") as string;
//                
//            tmpPath = Path.Combine(LocalStorage, baseDir, "Tmp");
//                
//            // Create an array of required directories
//            var requiredDirectories = new Dictionary<string, string>()
//            {
//                {"Storage", Path.Combine(LocalStorage, baseDir)},
//                {"Tmp", tmpPath}
//            };
//                
//            // Loop through the list of directories, make sure they exist and create them
//            foreach (var directory in requiredDirectories)
//            {
//                if (!Directory.Exists(directory.Value))
//                {
//
//                    Directory.CreateDirectory(directory.Value);
//                        
//                }
//                    
//                // Add directories to mount points
//                mounts.Add(FileSystemPath.Root.AppendDirectory(directory.Key), new PhysicalFileSystem(directory.Value));
//                    
//            }
//                
//            // Mount the filesystem
//            workspaceServicePlus.MountFileSystems(mounts);
//            
//            // Load bios from the user's storage folder
//            LoadBios(new[] {userBiosPath});
//                
            
            base.ConfigureWorkspace();
            
            workspaceServicePlus.archiveExtensions =
                ((string) bios.ReadBiosData("ArchiveExtensions", "zip,pv8,pvt,pvs,pva")).Split(',')
                .ToList(); //new List<string> {"zip", "pv8", "pvt", "pvs", "pva"});
            workspaceServicePlus.fileExtensions =
                ((string) bios.ReadBiosData("FileExtensions", "png,lua,json,txt")).Split(',')
                .ToList(); //new List<string> {"png", "lua", "json", "txt"};
//            gameFolders = ((string)ReadBiosData("GameFolders", "Games,Systems,Tools")).Split(',').ToList();//new List<string> {"zip", "pv8", "pvt", "pvs", "pva"});

            workspaceServicePlus.requiredFiles =
                ((string) bios.ReadBiosData("RequiredFiles", "data.json,info.json")).Split(',')
                .ToList(); //new List<string> {"zip", "pv8", "pvt", "pvs", "pva"});
            
            workspaceServicePlus.osLibPath = FileSystemPath.Root.AppendDirectory("PixelVisionOS")
                .AppendDirectory((string) bios.ReadBiosData("LibsDir", "Libs"));
            workspaceServicePlus.workspaceLibPath = FileSystemPath.Root.AppendDirectory("Workspace")
                .AppendDirectory((string) bios.ReadBiosData("LibsDir", "Libs"));
        
            
            var baseDir = bios.ReadBiosData("BaseDir", "PixelVision8") as string;

            // Custom to PV8
            
            documentsPath = Path.Combine(Documents, baseDir);

            
            workspaceServicePlus.fileSystem.Mounts.Add(new KeyValuePair<FileSystemPath, IFileSystem>(
                FileSystemPath.Root.AppendDirectory("User"),
                new PhysicalFileSystem(documentsPath)));
            
            // Build the OS Folder
    
            osFileSystem = new MergedFileSystem();

            osFileSystem.FileSystems = osFileSystem.FileSystems.Concat(new[] { new SubFileSystem(workspaceServicePlus.fileSystem,
                FileSystemPath.Root.AppendDirectory("App").AppendDirectory("PixelVisionOS")) });
                
            // Mount the PixelVisionOS directory
            workspaceServicePlus.fileSystem.Mounts.Add(new KeyValuePair<FileSystemPath, IFileSystem>(FileSystemPath.Root.AppendDirectory("PixelVisionOS"), osFileSystem));
                
            
            var workspaceName = bios.ReadBiosData("WorkspaceDir", "") as string;
        
            // Only create the workspace if there is a workspace directory in the bios
            if (workspaceName != "")
            {
                // Create the workspace
                workspaceServicePlus.MountWorkspace(workspaceName);
                
                var path = FileSystemPath.Root.AppendDirectory("Workspace").AppendDirectory("System");

                try
                {
                    if (workspaceServicePlus.fileSystem.Exists(path))
                    {
                        Console.WriteLine("Found Workspace system folder");
                        
                        osFileSystem.FileSystems = osFileSystem.FileSystems.Concat(
                            new[] { 
                                new SubFileSystem
                                (
                                    workspaceServicePlus.fileSystem, 
                                    path
                                ) 
                            }
                        );
                    }
                    
                }
                catch 
                {
                    Console.WriteLine("No system folder");
                }
                
                
            }
//            workspaceServicePlus.SetupLogFile(FileSystemPath.Parse(bios.ReadBiosData("LogFilePath", "/Tmp/Log.txt") as string));

        }
        
        protected override void LoadDefaultGame()
        {
            // Boot the game
            Load((string) bios.ReadBiosData("BootTool", "/PixelVisionOS/Tools/BootTool/"), RunnerMode.Booting);

        }

        public override void ActivateEngine(IEngine engine)
        {
            // At this point this game is fully configured so all chips are accessible for extra configuring
            
            // Save a reference to the controller chip so we can listen for special key events
            controllerChip = engine.controllerChip;
            
            // Get a reference to the Lua game
            var game = engine.gameChip as LuaGameChip;

            // Get the script
            var luaScript = game.luaScript;
            
            // Inject the PV8 runner special global functions
            luaScript.Globals["StartNextPreload"] = new Action(StartNextPreload);
            luaScript.Globals["PreloaderComplete"] = new Action(PreLoaderComplete);
            luaScript.Globals["IsExporting"] = new Func<bool>(exportService.IsExporting);
            luaScript.Globals["ReadExportPercent"] = new Func<int>(exportService.ReadExportPercent);
            luaScript.Globals["ReadExportMessage"] = new Func<string>(exportService.ReadExportMessage);
            luaScript.Globals["EnableCRT"] = (EnableCRTDelegator) EnableCRT;
            luaScript.Globals["Brightness"] = (BrightnessDelegator)Brightness;
            luaScript.Globals["Sharpness"] = (SharpnessDelegator)Sharpness;
            luaScript.Globals["BootDone"] = new Action(BootDone);
            luaScript.Globals["ReadPreloaderPercent"] = new Func<int>(() => (int) (loadService.percent * 100));
            luaScript.Globals["ShutdownSystem"] = new Action(ShutdownSystem);
            luaScript.Globals["QuitCurrentTool"] = (QuitCurrentToolDelagator) QuitCurrentTool;
            luaScript.Globals["LoadGame"] = (LoadGameDelegator) LoadGame;
            
            luaScript.Globals["DocumentPath"] = new Func<string>(() => documentsPath);
            luaScript.Globals["TmpPath"] = new Func<string>(() => tmpPath);
            
            luaScript.Globals["SystemVersion"] = new Func<string>(() => systemVersion);
            luaScript.Globals["SystemName"] = new Func<string>(() => systemName);
            luaScript.Globals["SessionID"] = new Func<string>(() => sessionID);
            
            // Activate the game
            base.ActivateEngine(engine);
            
        }
        
        private delegate bool EnableCRTDelegator(bool? toggle);
        private delegate float BrightnessDelegator(float? brightness = null);
        private delegate float SharpnessDelegator(float? sharpness = null);
        private delegate bool LoadGameDelegator(string path, Dictionary<string, string> metaData = null);
        private delegate void QuitCurrentToolDelagator(Dictionary<string, string> metaData, string tool = null);

        
        
        protected bool LoadGame(string path, Dictionary<string, string> metaData = null)
        {
            var success = Load(path, RunnerGame.RunnerMode.Loading, metaData);

            return success;
        }

        /// <summary>
        ///     This quits the current tool and returns to the default tool which should be the workspace explorer.
        /// </summary>
        /// <param name="metaData">
        ///     An optional argument to supply additional metadata back to the tool that is loaded when
        ///     quitting.
        /// </param>
        /// <param name="tool">
        ///     An optional argument that allows you to supply a string name for a built-in tool to load instead of
        ///     the default workspace explorer.
        /// </param>
        public void QuitCurrentTool(Dictionary<string, string> metaData, string tool = null)
        {
            if (tool != null)
                LoadGame(tool, metaData);
            else
                Back();
        }
        
//        public bool EnableCRT(bool? toggle)
//        {
//            return EnableCRT(toggle);
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

        protected override void Update(GameTime gameTime)
        {
            
            if (activeEngine == null || shutdown == true)
                return;
            
            // TODO make sure this order is correct or maybe it can be cleaned up
            if (screenShotActive)
            {
                screenshotTime += timeDelta;

                if (screenshotTime > screenshotDelay)
                {
                    screenShotActive = false;
                    screenshotTime = 0;
                }
            }
            
            if (controllerChip.GetKeyDown(Keys.LeftControl) ||
                controllerChip.GetKeyDown(Keys.LeftControl))
            {

                    if (controllerChip.GetKeyUp(actionKeys[ActionKeys.ScreenShotKey]))
                    {

                        // Only take a screenshot when one isn't being saved
                        if (!screenShotActive)
                        {
//                            Console.WriteLine("Take Picture");

                            screenShotActive = screenshotService.TakeScreenshot(activeEngine);

                        }

                    }else if (controllerChip.GetKeyUp(actionKeys[ActionKeys.RecordKey]))
                    {
//                        Console.WriteLine("Toggle Recoding");
                    }else if (controllerChip.GetKeyUp(actionKeys[ActionKeys.RestartKey]))
                    {
//                        Console.WriteLine("Reset Game");
                        ResetGame();

                    }
            }else if (controllerChip.GetKeyUp(Keys.Escape) && backKeyEnabled)
            {
                if (mode == RunnerMode.Booting)
                {
                    BootDone();
                }
                else
                {
                    Back();
                }
                
            }
            else
            {
                if (controllerChip.GetKeyUp(Keys.Alpha1))
                    ToggleLayers(1);
                else if (controllerChip.GetKeyUp(Keys.Alpha2))
                    ToggleLayers(2);
                else if (controllerChip.GetKeyUp(Keys.Alpha3))
                    ToggleLayers(3);
                else if (controllerChip.GetKeyUp(Keys.Alpha4))
                    ToggleLayers(4);
                else if (controllerChip.GetKeyUp(Keys.Alpha5))
                    ToggleLayers(5);
                else if (controllerChip.GetKeyUp(Keys.Alpha6))
                    ToggleLayers(6);
                else if (controllerChip.GetKeyUp(Keys.Alpha0)) ToggleLayers(Enum.GetNames(typeof(DrawMode)).Length - 1);
            }

            // Capture Script errors
            try
            {

                base.Update(gameTime);

            }
            catch (Exception e)
            {
//                Console.WriteLine("Update Error:\n"+e.Message);

                var error = e as ScriptRuntimeException;

                DisplayError(ErrorCode.Exception,
                    new Dictionary<string, string> {{"@{error}", error != null ? error.DecoratedMessage : e.Message}},
                    e);
            }
            
        }

        protected override void Draw(GameTime gameTime)
        {
            if (activeEngine == null)
                return;
            
            try
            {
                // If we are taking a screenshot, clear with white and don't draw the runner.
                if (screenShotActive)
                {
                    graphics.GraphicsDevice.Clear(Color.White);
                }
                else
                {
                    base.Draw(gameTime);    
                }

            }
            catch (Exception e)
            {
                
                var error = e as ScriptRuntimeException;

                DisplayError(ErrorCode.Exception,
                    new Dictionary<string, string> {{"@{error}", error != null ? error.DecoratedMessage : e.Message}}, e);
            }
            
            
        }

        public bool autoRunEnabled = true;
        
        public void BootDone()
        {
            
            // Only call BootDone when the runner is booting.
            if (mode != RunnerMode.Booting)
                return;
            
            var totalDisks = workspaceServicePlus.totalDisks;
        
            var loadedDisks = 0;

            // Disable auto run when loading up the default disks
            autoRunEnabled = false;
            
            for (int i = 0; i < totalDisks; i++)
            {
                var diskPath = (string) bios.ReadBiosData("Disk" + i, "none");
                if (diskPath != "none")
                {
//                        Console.WriteLine("Boot Mount "+ diskPath + " AutoRun " + (i == 0) + " AR Enabled " + autoRunEnabled);
                    MountDisk(diskPath, false);
                    loadedDisks++;
                }
            }

            string[] args = Environment.GetCommandLineArgs();
            
            bios.UpdateBiosData("TmpDisks", args.Length);

            var count = 0;
        
            foreach (string arg in args.Skip(1))
            {
                bios.UpdateBiosData("TmpDisk"+count, arg);
//                    workspaceServicePlus.MountDisk(arg, false);
                count++;

            }
        
            // Setup Drag and drop support
            Window.FileDropped += (o, e) => OnFileDropped(o, e);
            
            autoRunEnabled = true;
        
            // Look to see if we have the bios default tool in the OS folder
            try
            {
                var biosAutoRun = FileSystemPath.Parse((string) bios.ReadBiosData("AutoRun", ""));
                
                if (workspaceServicePlus.fileSystem.Exists(biosAutoRun))
                {

                    if (workspaceServicePlus.ValidateGameInDir(biosAutoRun))
                    {
                        Load(biosAutoRun.Path, RunnerMode.Loading);
                        return;
                    }
                    
                }
                
            }
            catch
            {
            }

            // Look to see if we can boot off a disk
            if (loadedDisks == 0)
            {
                DisplayError(ErrorCode.NoAutoRun);
            }
            else
            {
                // When all the disks are loaded, auto run the first disk
                var diskName = workspaceServicePlus.AutoRunFirstDisk();

                if (diskName != null)
                {
                    AutoRunGameFromDisk(diskName);
                }
                else
                {
                    DisplayError(ErrorCode.NoAutoRun);
                }
                
            }
                
        }
        
        public void MountDisk(string path, bool updateBios = true)
        {
//            Console.WriteLine("Load File - " + path + " Auto Run " + autoRunEnabled);
            try
            {
                var diskName = workspaceServicePlus.MountDisk(path, updateBios);

                // Only try to auto run a game if this is enabled in the runner
                if (autoRunEnabled) AutoRunGameFromDisk(diskName);
            }
            catch
            {
//                autoRunEnabled = true;
                // TODO need to make sure we show a better error to explain why the disk couldn't load
                 DisplayError(RunnerGame.ErrorCode.NoAutoRun);
            }

            // Only update the bios when we need  to
//            if (updateBios) UpdateDiskInBios();
        }
        
        public void AutoRunGameFromDisk(string diskName)
        {
            var diskPath = workspaceServicePlus.AutoRunGameFromDisk(diskName);

            if(diskPath != null)
            {
                
                // Create new meta data for the game. We wan to display the disk insert animation.
                var metaData = new Dictionary<string, string>
                {
                    {"showDiskAnimation", "true"}
                };
                
                // Load the disk path and play the game
                Load(diskPath, RunnerMode.Playing, metaData);
            }
            else
            {
                // If the new auto run path can't be found, throw an error
                DisplayError(RunnerGame.ErrorCode.NoAutoRun);
            }
        }
        
        public override void ShutdownActiveEngine()
        {
            // Look to see if there is an active engine
//            if (activeEngine == null)
//                return;

            try
            {
                base.ShutdownActiveEngine();
                
//                if (activeEngine.gameChip.saveSlots > 0)
//                {
//                    //Print("Active Engine To Save", activeEngine.name);
//
//                    SaveGameData(workspaceServicePlus.FindValidSavePath(activeEngine.name), activeEngine, SaveFlags.SaveData, false);
//                }

                // Save the active disk
                workspaceServicePlus.SaveActiveDisk();

            }
            catch (Exception e)
            {

                var error = e as ScriptRuntimeException;

                DisplayError(ErrorCode.Exception,
                    new Dictionary<string, string> {{"@{error}", error != null ? error.DecoratedMessage : e.Message}}, e);
            }

        }
        
        

        public bool Load(string path, RunnerMode newMode = RunnerMode.Playing, Dictionary<string, string> metaData = null)
        {

            try
            {
                
                if (newMode == RunnerMode.Loading)
                {
                    nextPathToLoad = path;
                    nextMetaData = metaData;
                    nextMode = RunnerMode.Playing;

                    // Create new meta data for the pre-loader
                    metaData = new Dictionary<string, string>
                    {
                        {"nextMode", nextMode.ToString()},
                        {"showDiskAnimation", "false"}
                    };

                    // Look to see if the game's meta data changes the disk animation flag
                    if (nextMetaData != null && nextMetaData.ContainsKey("showDiskAnimation"))
                        metaData["showDiskAnimation"] = nextMetaData["showDiskAnimation"];

                    // Get the default path to the load tool from the bios
                    path = (string) bios.ReadBiosData("LoadTool", "/PixelVisionOS/Tools/LoadTool/");

                    // Change the mode to loading
                    newMode = RunnerMode.Loading;
                }
                
                 // TODO This should be moved into the desktop runner?
                 autoRunEnabled = true;
    
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
                var files = workspaceServicePlus.LoadGame(path);
    
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
//                return base.Load(path, newMode, metaData);
            }
            catch (Exception e)
            {
                
                // Console.WriteLine("Load Error:\n"+e.Message);
                
                var error = e as ScriptRuntimeException;

                DisplayError(ErrorCode.Exception,
                    new Dictionary<string, string> {{"@{error}", error != null ? error.DecoratedMessage : e.Message}}, e);
            }


            return false;

        }
    
        public virtual void StartNextPreload()
        {
//            Print("Start Next Preload");

            if (nextPathToLoad != null)
            {
                var success = Load(nextPathToLoad, nextMode, nextMetaData);

                //TODO this needs to be tested more
                if (success == false)
                {
//                    loading = false;
                    DisplayError(ErrorCode.LoadError, new Dictionary<string, string> {{"@{path}", nextPathToLoad}});
                }
                else
                {
                    // Clear the old data
                    nextPathToLoad = null;
                    nextMetaData = null;

                    loadService.StartLoading();
                }
            }
        }
        
        public void PreLoaderComplete()
        {
//            Console.WriteLine("Preloading complete " + ReadPreloaderPercent());
//            loading = false;
            RunGame();
        }

        
        
        
        public override void SaveGameData(string path, IEngine engine, SaveFlags saveFlags, bool useSteps = true)
        {
            
//            Console.WriteLine("Save Game Date " + path + " /Game/ Exist " + workspaceServicePlus.fileSystem.Exists(FileSystemPath.Parse("/Game/")));
            
            // TODO testing saving all files to the rootPath of the game directory
            
//            path = "/Game/";
            
// TODO this finds the tmp directory to write to
//                    var basePath = engine.name.StartsWith("/PixelVisionOS/")
//                        ? "/Tmp" + engine.name
//                        : "/Game/";
//                    
//                    

            // Export the current game
            
            // TODO exporter needs a callback when its completed
            exportService.ExportGame(path, engine, saveFlags);
            
            // TODO this should be moved into the ExportGame class
            exportService.StartExport(useSteps);

//
        }

        public override void ConfigureServices()
        {

            base.ConfigureServices();
            
            var luaService = new LuaServicePlus(this);
            
            // Register Lua Service
            tmpEngine.AddService(typeof(LuaService).FullName, luaService);
            
            
            tmpEngine.AddService(typeof(ExportService).FullName, exportService);

        }
        
        public void OnFileDropped(object gameWindow, string path)
        {
            if(shutdown == false)
            MountDisk(path);

            UpdateDiskInBios();
            
            // TODO need to make sure this is the right time to do this
            
            ResetGame();
        }
        
        public void ResetGame(bool showBoot = true)
        {
            // Make sure we don't reload when booting or loading
            if (mode == RunnerMode.Booting || mode == RunnerMode.Loading || loadHistory.Count == 0)
                return;

            //TODO this nees to also pass in the last metaData state
            // Load the last game from the history
            var lastURI = loadHistory.Last();
            var metaData = metaDataHistory.Last();

            // Clear the load history if it is loading the first item
            if (loadHistory.Count == 1)
            {
                // This insures you can't quit the first game that is loaded.
                loadHistory.Clear();
            }
            
            if (metaData != null)
            {
                if (metaData.ContainsKey("reset"))
                    metaData["reset"] = "true";
                else
                    metaData.Add("reset", "true");
                
                if (metaData.ContainsKey("showDiskAnimation"))
                    metaData["showDiskAnimation"] = "false";
                else
                    metaData.Add("showDiskAnimation", "false");
                
            }

            // Reload the game
            Load(lastURI, RunnerMode.Loading, metaData);
        }
        
                public void UpdateDiskInBios()
        {
            var paths = workspaceServicePlus.diskDrives.physicalPaths;

            for (var i = 0; i < paths.Length; i++) bios.UpdateBiosData("Disk" + i, paths[i]);
        }

//        public void Dispose()
//        {
////            throw new NotImplementedException();
//        }

        public override void DisplayWarning(string message)
        {
            workspaceServicePlus.UpdateLog(message);
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
//                workspaceServicePlus.EjectDisk();
//
//                UpdateDiskInBios();
            }
//            }
        }
        
        public override void ShutdownSystem()
        {
            // We only want to call this once so don't run if shutdown is true
            if (shutdown)
                return;

            // Shutdown the active game
//            ShutdownActiveEngine();

            // Toggle the shutdown flag
            shutdown = true;
            
            base.ShutdownSystem();

//            UpdateDiskInBios();
//            SaveBiosChanges();
//            
//            // Save any changes to the bios to the user's custom bios file
//            workspaceServicePlus.ShutdownSystem();
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

        

        protected string GetErrorMessage(ErrorCode code)
        {
            return (string) bios.ReadBiosData(code.ToString(), "Error code " + (int) code);
        }

        protected virtual void LoadError(Dictionary<string, string> metaData)
        {
            var tool = (string) bios.ReadBiosData("ErrorTool", "/PixelVisionOS/Tools/ErrorTool/");

            workspaceServicePlus.UpdateLog(metaData["errorMessage"], LogType.Error,
                metaData.ContainsKey("exceptionMessage") ? metaData["exceptionMessage"] : null);

            Load(tool, RunnerMode.Error, metaData);
        }
    }
    
}
