using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private string tmpPath;
        
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
        public PixelVision8Runner(string rootPath, string autoRunPath = null) : base(rootPath, autoRunPath)
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
        
        public bool EnableCRT(bool? toggle)
        {

            if (toggle.HasValue)
            {
                displayTarget.useCRT = toggle.Value;
                workspaceService.UpdateBiosData(BiosSettings.CRT.ToString(), toggle.Value.ToString());
                ResetResolution();
            }

            return displayTarget.useCRT;

        }

        public float Brightness(float? brightness = null)
        {
            if (brightness.HasValue)
            {
                displayTarget.brightness = brightness.Value;
                workspaceService.UpdateBiosData(BiosSettings.Brightness.ToString(), (long) (brightness * 100));
            }

            return displayTarget.brightness;
        }
        
        public float Sharpness(float? sharpness = null)
        {
            if (sharpness.HasValue)
            {
                displayTarget.sharpness = sharpness.Value;
                workspaceService.UpdateBiosData(BiosSettings.Sharpness.ToString(), sharpness);
            }

            return displayTarget.sharpness;
        }
        

        protected override void ConfigureRunner()
        {
            base.ConfigureRunner();
            
            screenshotService = new ScreenshotService(workspaceService);
            exportService = new ExportService(null); //TODO Need to create a new AudioClipAdaptor
            
            var actions = Enum.GetValues(typeof(ActionKeys)).Cast<ActionKeys>();
            foreach (var action in actions)
            {
                actionKeys[action] = (Keys)Convert.ToInt32((long)workspaceService.ReadBiosData(action.ToString(), (long)actionKeys[action], true));
            }
            
            
            // TODO need to fix the path to /PixelVisionOS/Effects/
            var shaderPath = Path.Combine(rootPath, "crt-lottes-mg.ogl.mgfxo");

            if (File.Exists(shaderPath))
            {
                displayTarget.shaderPath = shaderPath;
                EnableCRT(Convert.ToBoolean(workspaceService.ReadBiosData(BiosSettings.CRT.ToString(), "True") as string));

            }
            
            // Set the display bios values here but this should be in the base runner?
//            Brightness(Convert.ToSingle((long) workspaceService.ReadBiosData(BiosSettings.Brightness.ToString(), 100L))/100F);
//            Sharpness(Convert.ToSingle((long) workspaceService.ReadBiosData(BiosSettings.Sharpness.ToString(), -6L)));

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
                catch
                {
//                    DisplayBootErrorScreen("Error parsing system bios.");
                }
                
                // Create a workspace
                workspaceService = new WorkspaceServicePlus(bios, this);
                var mounts = new Dictionary<FileSystemPath, IFileSystem>();

                // Create the base directory in the documents and local storage folder
                
                // Get the base directory from the bios or use Pixel Vision 8 as the default name
                var baseDir = bios.ReadBiosData("BaseDir", "PixelVision8") as string;
                
                documentsPath = Path.Combine(Documents, baseDir);
                tmpPath = Path.Combine(LocalStorage, baseDir, "Tmp");
                
                // Create an array of required directories
                var requiredDirectories = new Dictionary<string, string>()
                {
                    {"App", rootPath},
                    {"User", documentsPath},
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
                
                // Build the OS Folder
    
                osFileSystem = new MergedFileSystem();

                osFileSystem.FileSystems = osFileSystem.FileSystems.Concat(new[] { new SubFileSystem(workspaceService.fileSystem,
                    FileSystemPath.Root.AppendDirectory("App").AppendDirectory("PixelVisionOS")) });
                
                // Mount the PixelVisionOS directory
                workspaceService.fileSystem.Mounts.Add(new KeyValuePair<FileSystemPath, IFileSystem>(FileSystemPath.Root.AppendDirectory("PixelVisionOS"), osFileSystem));
                
                workspaceService.SetupLogFile(FileSystemPath.Parse(workspaceService.ReadBiosData("LogFilePath", "/Tmp/Log.txt") as string));
                
                if (workspaceService.fileSystem.Exists(FileSystemPath.Root.AppendDirectory("PixelVisionOS")))
                {
             
                    // Initialize the runner
                    ConfigureRunner();

                    // Boot the game
                    Load(workspaceService.DefaultBootTool(), RunnerMode.Booting);
                    
                }
                else
                {
                    // TODO No OS Found
                }

            }
            else
            {
                // TODO no bios found
            }
            
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

        public void BootDone()
        {
            
            // Only call BootDone when the runner is booting.
            if (mode != RunnerMode.Booting)
                return;
            
            var totalDisks = workspaceService.totalDisks;
        
            var loadedDisks = 0;

            // Disable auto run when loading up the default disks
            workspaceService.autoRunEnabled = false;
            
            for (int i = 0; i < totalDisks; i++)
            {
                var diskPath = (string) workspaceService.ReadBiosData("Disk" + i, "none");
                if (diskPath != "none")
                {
//                        Console.WriteLine("Boot Mount "+ diskPath + " AutoRun " + (i == 0) + " AR Enabled " + workspaceService.autoRunEnabled);
                    workspaceService.MountDisk(diskPath, false);
                    loadedDisks++;
                }
            }

            string[] args = Environment.GetCommandLineArgs();
            
            workspaceService.WriteBiosData("TmpDisks", args.Length);

            var count = 0;
        
            foreach (string arg in args.Skip(1))
            {
                workspaceService.WriteBiosData("TmpDisk"+count, arg);
//                    workspaceService.MountDisk(arg, false);
                count++;

            }

            var workspaceName = workspaceService.ReadBiosData("WorkspaceDir", "") as string;
        
            // Only create the workspace if there is a workspace directory in the bios
            if (workspaceName != "")
            {
                // Create the workspace
                workspaceService.MountWorkspace(workspaceName);
                
                var path = FileSystemPath.Root.AppendDirectory("Workspace").AppendDirectory("System");

                try
                {
                    if (workspaceService.fileSystem.Exists(path))
                    {
                        Console.WriteLine("Found Workspace system folder");
                        
                        osFileSystem.FileSystems = osFileSystem.FileSystems.Concat(
                            new[] { 
                                new SubFileSystem
                                (
                                    workspaceService.fileSystem, 
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
        
            // Setup Drag and drop support
            Window.FileDropped += (o, e) => OnFileDropped(o, e);
            
            workspaceService.autoRunEnabled = true;
        
            // Look to see if we have the bios default tool in the OS folder
            try
            {
                var biosAutoRun = FileSystemPath.Parse((string) workspaceService.ReadBiosData("AutoRun", ""));
                
                if (workspaceService.fileSystem.Exists(biosAutoRun))
                {

                    if (workspaceService.ValidateGameInDir(biosAutoRun))
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
                workspaceService.AutoRunFirstDisk();
            }
                
        }
        
        public override void ShutdownActiveEngine()
        {
            // Look to see if there is an active engine
            if (activeEngine == null)
                return;

            try
            {
                base.ShutdownActiveEngine();
                
                if (activeEngine.gameChip.saveSlots > 0)
                {
                    //Print("Active Engine To Save", activeEngine.name);

                    SaveGameData(workspaceService.FindValidSavePath(activeEngine.name), activeEngine, SaveFlags.SaveData, false);
                }

                // Save the active disk
                workspaceService.SaveActiveDisk();

            }
            catch (Exception e)
            {

                var error = e as ScriptRuntimeException;

                DisplayError(ErrorCode.Exception,
                    new Dictionary<string, string> {{"@{error}", error != null ? error.DecoratedMessage : e.Message}}, e);
            }

        }
        
        

        public override bool Load(string path, RunnerMode newMode = RunnerMode.Playing, Dictionary<string, string> metaData = null)
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
                    path = (string) workspaceService.ReadBiosData("LoadTool", "/PixelVisionOS/Tools/LoadTool/");

                    // Change the mode to loading
                    newMode = RunnerMode.Loading;
                }
                
                
                return base.Load(path, newMode, metaData);
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
            
//            Console.WriteLine("Save Game Date " + path + " /Game/ Exist " + workspaceService.fileSystem.Exists(FileSystemPath.Parse("/Game/")));
            
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
            workspaceService.MountDisk(path);
            
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

//        public void Dispose()
//        {
////            throw new NotImplementedException();
//        }
    }
    
}
