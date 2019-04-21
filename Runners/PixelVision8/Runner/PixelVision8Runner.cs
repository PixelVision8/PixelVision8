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
using PixelVision8.Runner.Workspace;
using Color = Microsoft.Xna.Framework.Color;

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
//        private MergedFileSystem osFileSystem;
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
        
        public List<KeyValuePair<string, Dictionary<string, string>>> loadHistory = new List<KeyValuePair<string, Dictionary<string, string>>>();
        
//        public List<string> loadHistory = new List<string>();
//        protected List<Dictionary<string, string>> metaDataHistory = new List<Dictionary<string, string>>();
        protected string Documents => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        public string sessionID { get; protected set; }

//        protected string rootPath;
        protected bool shutdown;
        public string systemName;
        public string systemVersion;
        
        public ExportService exportService { get; private set; }
        
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

        protected override void ConfigureRunner()
        {
            base.ConfigureRunner();
            
            // Save the session ID
            sessionID = DateTime.Now.ToString("yyyyMMddHHmmssffff");

            systemVersion = bios.ReadBiosData(BiosSettings.SystemVersion.ToString(), "0.0.0");
            systemName = bios.ReadBiosData("SystemName", "PixelVision8");
            
            screenshotService = new ScreenshotService(workspaceServicePlus);
            exportService = new ExportService(null); //TODO Need to create a new AudioClipAdaptor
            
            var actions = Enum.GetValues(typeof(ActionKeys)).Cast<ActionKeys>();
            foreach (var action in actions)
            {
                actionKeys[action] = (Keys)Convert.ToInt32(bios.ReadBiosData(action.ToString(), ((int)actionKeys[action]).ToString(), true));
            }
            
            // Replace title with version
            
            Window.Title =
                bios.ReadBiosData(BiosSettings.SystemName.ToString(), "Pixel Vision 8 Runner") + " " + 
                bios.ReadBiosData(BiosSettings.SystemVersion.ToString(), "0.0.0");
           
        }

        protected WorkspaceServicePlus workspaceServicePlus;
        
        protected override void CreateWorkspaceService()
        {
            workspaceServicePlus = new WorkspaceServicePlus(
                new KeyValuePair<WorkspacePath, IFileSystem>(
                    WorkspacePath.Root.AppendDirectory("App"),
                    new PhysicalFileSystem(rootPath)
                    )
                );

            // Pass the new service back to the base class    
            workspaceService = workspaceServicePlus;
            
            serviceManager.AddService(typeof(WorkspaceService).FullName, workspaceService);

        }

        protected override void ConfigureWorkspace()
        {

            // Call the base ConfigureWorkspace method to configure the workspace correctly
            base.ConfigureWorkspace();
            
            // Everything below is custom to PV8
            
            // Define PV8 disk extensions from the bios
            workspaceServicePlus.archiveExtensions =
                ((string) bios.ReadBiosData("ArchiveExtensions", "zip,pv8,pvt,pvs,pva")).Split(',')
                .ToList();
            
            //  Define the valid file extensions from the bios
            workspaceServicePlus.fileExtensions =
                ((string) bios.ReadBiosData("FileExtensions", "png,lua,json,txt")).Split(',')
                .ToList();

            // Define the files required to make a game valid from the bios
            workspaceServicePlus.requiredFiles =
                ((string) bios.ReadBiosData("RequiredFiles", "data.json,info.json")).Split(',')
                .ToList();

            // Include any library files in the OS mount point
            workspaceServicePlus.osLibPath = WorkspacePath.Root.AppendDirectory("PixelVisionOS")
                .AppendDirectory(bios.ReadBiosData("LibsDir", "Libs"));

            // PV8 Needs to access the documents folder so it can create the workspace drive
            var baseDir = bios.ReadBiosData("BaseDir", "PixelVision8") as string;

            // Create the real system path to the documents folder
            documentsPath = Path.Combine(Documents, baseDir);
            
            // Create a new physical file system mount
            workspaceServicePlus.AddMount(new KeyValuePair<WorkspacePath, IFileSystem>(
                WorkspacePath.Root.AppendDirectory("User"),
                new PhysicalFileSystem(documentsPath)));
            
            // Add trash
            // Create a trash folder
            
            
            // Look for the workspace name
            var workspaceName = bios.ReadBiosData("WorkspaceDir", "Workspace") as string;
        
            // Mount the workspace drive    
            workspaceServicePlus.MountWorkspace(workspaceName);
            
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
            luaScript.Globals["PreloaderComplete"] = new Action(RunGame);
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

            luaScript.Globals["DiskPaths"] = new Func<Dictionary<string, string>>(workspaceServicePlus.DiskPaths);
            luaScript.Globals["SaveActiveDisks"] = new Action(() =>
            {
                var disks = workspaceServicePlus.disks;

                foreach (var disk in disks) workspaceServicePlus.SaveDisk(disk);
            });
            luaScript.Globals["EjectDisk"] = new Action<string>(EjectDisk);
            
            luaScript.Globals["EnableAutoRun"] = new Action<bool>(EnableAutoRun);
            luaScript.Globals["EnableBackKey"] = new Action<bool>(EnableBackKey);
            luaScript.Globals["RebuildWorkspace"] = new Action(workspaceServicePlus.RebuildWorkspace);
            
            // Activate the game
            base.ActivateEngine(engine);
            
        }
        
        public void EjectDisk(string path)
        {
            workspaceServicePlus.EjectDisk(WorkspacePath.Parse(path));

            UpdateDiskInBios();

            

            AutoLoadDefaultGame();

        }
        
        public void EnableAutoRun(bool value)
        {
            autoRunEnabled = value;
        }

        public void EnableBackKey(bool value)
        {
            backKeyEnabled = value;
        }
        
        private delegate bool EnableCRTDelegator(bool? toggle);
        private delegate float BrightnessDelegator(float? brightness = null);
        private delegate float SharpnessDelegator(float? sharpness = null);
        private delegate bool LoadGameDelegator(string path, Dictionary<string, string> metaData = null);
        private delegate void QuitCurrentToolDelagator(Dictionary<string, string> metaData, string tool = null);

        
        
        protected bool LoadGame(string path, Dictionary<string, string> metaData = null)
        {
            var success = Load(path, RunnerMode.Loading, metaData);

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
            // When quitting, we should modify the history by removing the current game and if a new game is provided, add that to the history
            
            if (tool != null)
                // TODO need to remove the previous game from the history
                LoadGame(tool, metaData);
            else
            {
                Back();
            }
                
        }
        

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
//            if (activeEngine == null)
//                return;
            
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

            // Setup Drag and drop support
            Window.FileDropped += (o, e) => OnFileDropped(o, e);

            // Mount all of the disks from the bios
//            var totalDisks = ;

//            var loadedDisks = 0;

            // Disable auto run when loading up the default disks
            autoRunEnabled = false;

            for (int i = 0; i < workspaceServicePlus.totalDisks; i++)
            {
                var diskPath =  bios.ReadBiosData("Disk" + i, "none");
                if (diskPath != "none")
                {
//                    Console.WriteLine("Mount disk " + diskPath);

                    // manually mount each disk since we are not going to load from them
                    workspaceServicePlus.MountDisk(diskPath);
//                    loadedDisks++;
                }
            }

            AutoLoadDefaultGame();

        }
        
        public void AutoLoadDefaultGame()
        {

            loadHistory.Clear();
//            metaDataHistory.Clear();
            
        // Enable auto run by default
            autoRunEnabled = true;
            
            // Look to see if we have the bios default tool in the OS folder
            try
            {
                var biosAutoRun = bios.ReadBiosData("AutoRun", "");
                
                // Check to see if this path exists
                if (workspaceServicePlus.Exists(WorkspacePath.Parse(biosAutoRun)))
                {

                    // Validate that the path is actually a game
                    if (workspaceServicePlus.ValidateGameInDir(WorkspacePath.Parse(biosAutoRun)))
                    {
                        // Attempt to load the game
                        Load(biosAutoRun, RunnerMode.Loading);
                        return;
                    }
                    
                }
                
            }
            catch
            {
                // ignored
            }

            // Try to boot from the first disk
            try
            {
                var firstDiskPath = workspaceServicePlus.disks.First().EntityName;

                AutoRunGameFromDisk(firstDiskPath);

                return;
            }
            catch
            {
                // ignored
            }

            DisplayError(ErrorCode.NoAutoRun);
            
        }
        
        public void MountDisk(string path)
        {
//            Console.WriteLine("Load File - " + path + " Auto Run " + autoRunEnabled);
            try
            {
                var diskName = workspaceServicePlus.MountDisk(path);

                // Only try to auto run a game if this is enabled in the runner
                if (autoRunEnabled) 
                    AutoRunGameFromDisk(diskName);
                
            }
            catch
            {
//                autoRunEnabled = true;
                // TODO need to make sure we show a better error to explain why the disk couldn't load
                 DisplayError(ErrorCode.NoAutoRun);
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
                Load(diskPath, RunnerMode.Loading, metaData);
            }
            else
            {
                // If the new auto run path can't be found, throw an error
                DisplayError(ErrorCode.NoAutoRun);
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
                    path =  bios.ReadBiosData("LoadTool", "/PixelVisionOS/Tools/LoadTool/");

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

                    // Only add the history if the last item is not the same
//                    if(loadHistory.Last().Key != path)
                        loadHistory.Add(new KeyValuePair<string, Dictionary<string, string>>(path, metaData));

//                    loadHistory.Add(path);
//                    metaDataHistory.Add(metaData);
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

        
//        public void PreLoaderComplete()
//        {
////            Console.WriteLine("Preloading complete " + ReadPreloaderPercent());
////            loading = false;
//            RunGame();
//        }

        
        
        
        public override void SaveGameData(string path, IEngine engine, SaveFlags saveFlags, bool useSteps = true)
        {
            
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

//            UpdateDiskInBios();
            
            // TODO need to make sure this is the right time to do this
            
            ResetGame();
        }
        
        public override void ResetGame()
        {
            // Make sure we don't reload when booting or loading
            if (mode == RunnerMode.Booting || mode == RunnerMode.Loading || loadHistory.Count == 0)
                return;

            //TODO this nees to also pass in the last metaData state
            // Load the last game from the history

            var lastGameRef = loadHistory.Last();
            
            var lastURI = WorkspacePath.Parse(lastGameRef.Key);
            var metaData = lastGameRef.Value;

            // Clear the load history if it is loading the first item
            if (loadHistory.Count == 1)
            {
                // This insures you can't quit the first game that is loaded.
                loadHistory.Clear();
            }
            
            // Make sure this is still a valid path before we try to load it
            if (workspaceService.Exists(lastURI) && workspaceService.ValidateGameInDir(lastURI))
            {
            
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
                Load(lastURI.Path, RunnerMode.Loading, metaData);
                return;
            }
            
            DisplayError(ErrorCode.LoadError, new Dictionary<string, string>(){{"@{path}", lastURI.Path}});
            
        }
        
        public virtual void Back()
        {
            if (mode == RunnerMode.Loading)
                return;

            if (loadHistory.Count > 1)
            {
                // Remvoe the last game that was running from the history
                loadHistory.RemoveAt(loadHistory.Count - 1);
                
                // Get the previous game
                var lastGameRef = loadHistory.Last();
                
                // Remove that game from history since we are about to load it
                loadHistory.RemoveAt(loadHistory.Count - 1);
                
                // Load the last game
                Load(lastGameRef.Key, RunnerMode.Loading, lastGameRef.Value);
            }
            else
            {
                DisplayError(ErrorCode.NoAutoRun);
            }

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

            UpdateDiskInBios();
            
            base.ShutdownSystem();

//            UpdateDiskInBios();
//            SaveBiosChanges();
//            
//            // Save any changes to the bios to the user's custom bios file
//            workspaceServicePlus.ShutdownSystem();
        }

        public void UpdateDiskInBios()
        {
            var paths = workspaceServicePlus.physicalPaths;

            for (int i = 0; i < paths.Length; i++)
            {
                bios.UpdateBiosData("Disk" + i, paths[i]);
            }
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

            if (exception != null) metaData["exceptionMessage"] = exception.StackTrace;


            // TODO need to be able to pass in an error
            LoadError(metaData);
        }

        

        protected string GetErrorMessage(ErrorCode code)
        {
            return  bios.ReadBiosData(code.ToString(), "Error code " + (int) code);
        }

        protected virtual void LoadError(Dictionary<string, string> metaData)
        {
            var tool =  bios.ReadBiosData("ErrorTool", "/PixelVisionOS/Tools/ErrorTool/");

            workspaceServicePlus.UpdateLog(metaData["errorMessage"], LogType.Error,
                metaData.ContainsKey("exceptionMessage") ? metaData["exceptionMessage"] : null);

            Load(tool, RunnerMode.Error, metaData);
        }
    }
    
}
