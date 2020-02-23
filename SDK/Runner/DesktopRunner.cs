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
using MoonSharp.Interpreter;
using PixelVision8.Engine;
using PixelVision8.Engine.Chips;
using PixelVision8.Runner.Data;
using PixelVision8.Runner.Exporters;
using PixelVision8.Runner.Services;
using PixelVision8.Runner.Utils;
using PixelVision8.Runner.Workspace;

namespace PixelVision8.Runner
{
    public enum CRTBiosSettings
    {
        CRT,
        Brightness,
        Sharpness,
        CRTEffectPath
    }

    /// <summary>
    ///     This is the main type for your game.
    /// </summary>
    public class DesktopRunner : GameRunner
    {
        // Store the path to the game's files
        //        private IControllerChip controllerChip;
        public BiosService bios;
        protected WorkspacePath biosPath = WorkspacePath.Root.AppendDirectory("App").AppendFile("bios.json");

        public List<KeyValuePair<string, Dictionary<string, string>>> loadHistory =
            new List<KeyValuePair<string, Dictionary<string, string>>>();

        protected Dictionary<string, string> nextMetaData;
        protected RunnerMode nextMode;
        protected string nextPathToLoad;
        protected string rootPath;
        public string systemName;
        public string SystemVersion;
        protected string tmpPath;
        public WorkspaceService workspaceService;

        /// <summary>
        ///     This constructor saves the path to the game's files and kicks off the base constructor
        /// </summary>
        /// <param name="gamePath"></param>
        public DesktopRunner(string rootPath)
        {
            this.rootPath = rootPath;
        }

        public DesktopRunner()
        {
            throw new NotImplementedException();
        }

        public string SessionId { get; protected set; }

        protected WorkspacePath userBiosPath => WorkspacePath.Parse("/Storage/user-bios.json");

        /// <summary>
        ///     The base runner contains a list of the core chips. Here you'll want to add the game chip to the list so it can run.
        ///     This is called when a new game is created by the runner.
        /// </summary>
        public override List<string> DefaultChips
        {
            get
            {
                // Get the list of default chips
                var chips = base.DefaultChips;

                // Add the custom C# game chip
                chips.Add(typeof(LuaGameChip).FullName);

                // Return the list of chips
                return chips;
            }
        }

        protected override void ConfigureRunner()
        {
            // Save the session ID
            SessionId = DateTime.Now.ToString("yyyyMMddHHmmssffff");

            SystemVersion = bios.ReadBiosData(BiosSettings.SystemVersion.ToString(), "0.0.0");
            systemName = bios.ReadBiosData("SystemName", "PixelVision8");

            base.ConfigureRunner();

            // TODO This may be a string
            Volume(MathHelper.Clamp(
                Convert.ToInt32(bios.ReadBiosData(BiosSettings.Volume.ToString(), "40")),
                0, 100));

            Mute(Convert.ToBoolean(bios.ReadBiosData(BiosSettings.Mute.ToString(), "False")));

            Window.Title =
                bios.ReadBiosData(BiosSettings.SystemName.ToString(), "Pixel Vision 8 Runner");
        }

        public override void CreateLoadService()
        {
            
            loadService = new LoadService(new WorkspaceFileLoadHelper(workspaceService));
        
            Script.DefaultOptions.ScriptLoader = new ScriptLoaderUtil(workspaceService);
        }

        public override void ActivateEngine(IEngine engine)
        {
            
            // Activate the game
            BaseActivateEngine(engine);

            
        }

        public virtual void BaseActivateEngine(IEngine engine)
        {
            if (engine == null) return;

            // Make the loaded engine active
            ActiveEngine = engine;

            LuaGameChip tempQualifier = ((LuaGameChip)ActiveEngine.GameChip);

            tempQualifier.LoadScript(tempQualifier.DefaultScriptPath);

            ActiveEngine.ResetGame();

            // After loading the game, we are ready to run it.
            ActiveEngine.RunGame();

            // Reset the game's resolution
            ResetResolution();

            // Make sure that the first frame is cleared with the default color
            ActiveEngine.GameChip.Clear();
        }

        //        protected void LoadDefaultGame()
        //        {
        //            // Boot the game
        //        }

        public virtual bool Load(string path, RunnerMode newMode = RunnerMode.Playing,
            Dictionary<string, string> metaData = null)
        {
            try
            {
                if (newMode == RunnerMode.Loading)
                {
                    nextPathToLoad = path;

                    // Save a copy of the meta data
                    nextMetaData = metaData?.ToDictionary(entry => entry.Key,
                        entry => entry.Value);

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


                    // TODO this was where the eject animation flag was set

                    // Get the default path to the load tool from the bios
                    path = bios.ReadBiosData("LoadTool", "/PixelVisionOS/Tools/LoadTool/");

                    // Change the mode to loading
                    newMode = RunnerMode.Loading;
                }

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

                    var metaDataCopy = metaData.ToDictionary(entry => entry.Key,
                        entry => entry.Value);

                    if (loadHistory.Count > 0)
                    {
                        //                        Console.WriteLine("History " + loadHistory.Last().Key + " " + path);
                        // Only add the history if the last item is not the same

                        // Loop through the history and see if the path already exists
                        for (var i = loadHistory.Count - 1; i >= 0; i--)
                            if (loadHistory[i].Key == path)
                                loadHistory.RemoveAt(i);

                        if (loadHistory.Last().Key != path)
                            loadHistory.Add(new KeyValuePair<string, Dictionary<string, string>>(path, metaDataCopy));
                    }
                    else
                    {
                        loadHistory.Add(new KeyValuePair<string, Dictionary<string, string>>(path, metaDataCopy));
                    }
                }

                // Create a new tmpEngine
                ConfigureEngine(metaData);


                // Path the full path to the engine's name
                tmpEngine.Name = path;

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

                // TODO need to remove old load disk animation?
                //                if (metaDataCopy.ContainsKey("showDiskAnimation"))
                //                {
                //                    metaData["showDiskAnimation"] = "false";
                //                }

                // If the game is unable to run, display an error
                //            if (success == false) 


                // Create new FileSystemPath
                return success;
                //                return base.Load(path, newMode, metaData);
            }
            catch (Exception e)
            {
                // Console.WriteLine("Load Error:\n"+e.Message);


                DisplayError(ErrorCode.Exception,
                    new Dictionary<string, string>
                        {{"@{error}", e is ScriptRuntimeException error ? error.DecoratedMessage : e.Message}}, e);
            }


            return false;
        }

        public override void ConfigureEngine(Dictionary<string, string> metaData = null)
        {
            base.ConfigureEngine(metaData);

            // Get a reference to the Lua game
            var game = tmpEngine.GameChip as LuaGameChip;

            // Get the script
            var luaScript = game.LuaScript;

            luaScript.Globals["StartNextPreload"] = new Action(StartNextPreload);
            luaScript.Globals["PreloaderComplete"] = new Action(RunGame);
            luaScript.Globals["EnableCRT"] = (EnableCRTDelegator)EnableCRT;
            luaScript.Globals["Brightness"] = (BrightnessDelegator)Brightness;
            luaScript.Globals["Sharpness"] = (SharpnessDelegator)Sharpness;
            luaScript.Globals["BootDone"] = new Action<bool>(BootDone);
            luaScript.Globals["ReadPreloaderPercent"] = new Func<int>(() => (int)(loadService.Percent * 100));
            luaScript.Globals["LoadGame"] =
                new Func<string, Dictionary<string, string>, bool>((path, metadata) =>
                    Load(path, RunnerMode.Loading, metadata));
            luaScript.Globals["SystemVersion"] = new Func<string>(() => SystemVersion);
            luaScript.Globals["SystemName"] = new Func<string>(() => systemName);
            luaScript.Globals["SessionID"] = new Func<string>(() => SessionId);

            // Expose Bios APIs
            luaScript.Globals["ReadBiosData"] = new Func<string, string, string>((key, defaultValue) =>
                bios.ReadBiosData(key, defaultValue));
            luaScript.Globals["WriteBiosData"] = new Action<string, string>(bios.UpdateBiosData);

        }

        public virtual void DisplayError(ErrorCode code, Dictionary<string, string> tokens = null,
            Exception exception = null)
        {
            if (mode == RunnerMode.Error) return;

            //            // TODO should this only work on special cases?
            //            autoRunEnabled = true;

            var sb = new StringBuilder();

            // Pull the error message from the bios
            var message = GetErrorMessage(code);

            sb.Append(message);

            if (tokens != null)
                foreach (var entry in tokens)
                    sb.Replace(entry.Key, entry.Value);

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
            return bios.ReadBiosData(code.ToString(), "Error code " + (int) code);
        }

        protected virtual void LoadError(Dictionary<string, string> metaData)
        {
            var tool = bios.ReadBiosData("ErrorTool", "/PixelVisionOS/Tools/ErrorTool/");

            workspaceService.UpdateLog(metaData["errorMessage"], LogType.Error,
                metaData.ContainsKey("exceptionMessage") ? metaData["exceptionMessage"] : null);

            Load(tool, RunnerMode.Error, metaData);
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

        public virtual void BootDone(bool safeMode = false)
        {
            // Only call BootDone when the runner is booting.
            if (mode != RunnerMode.Booting) return;

            // Test to see if we are in save mode before loading the bios
            if (safeMode)
            {
                // Clear the current bios
                bios.Clear();

                // Read the bios text
                var biosText = workspaceService.ReadTextFromFile(biosPath);

                // Reparse the bios text
                bios.ParseBiosText(biosText);

                // Reset all the default values from the bios
                ConfigureDisplayTarget();
            }

            AutoLoadDefaultGame();
        }

        public virtual void AutoLoadDefaultGame()
        {
            loadHistory.Clear();
            //            metaDataHistory.Clear();

            // Look to see if we have the bios default tool in the OS folder
            try
            {
                var biosAutoRun = bios.ReadBiosData("AutoRun");

                // Check to see if this path exists
                if (biosAutoRun != "" && workspaceService.Exists(WorkspacePath.Parse(biosAutoRun)))
                    // Validate that the path is actually a game
                    if (workspaceService.ValidateGameInDir(WorkspacePath.Parse(biosAutoRun)))
                    {
                        // Attempt to load the game
                        Load(biosAutoRun, RunnerMode.Loading);
                        return;
                    }
            }
            catch
            {
                // ignored
            }

            DisplayError(ErrorCode.NoAutoRun);
        }

        public override void ConfigureDisplayTarget()
        {
            // Get the virtual monitor resolution
            var tmpRes = bios.ReadBiosData(BiosSettings.Resolution.ToString(), "512x480")
                .Split('x').Select(int.Parse)
                .ToArray();

            if (displayTarget == null) displayTarget = new DisplayTarget(graphics, tmpRes[0], tmpRes[1]);

            Fullscreen(Convert.ToBoolean(
                bios.ReadBiosData(BiosSettings.FullScreen.ToString(), "False")));
            StretchScreen(
                Convert.ToBoolean(
                    bios.ReadBiosData(BiosSettings.StretchScreen.ToString(), "False")));
            CropScreen(Convert.ToBoolean(
                bios.ReadBiosData(BiosSettings.CropScreen.ToString(), "True")));

            Scale(Convert.ToInt32(bios.ReadBiosData(BiosSettings.Scale.ToString(), "1")));

            // Configure CRT shader
            var shaderPath = WorkspacePath.Parse(bios.ReadBiosData(CRTBiosSettings.CRTEffectPath.ToString(),
                "/App/Effects/crt-lottes-mg.ogl.mgfxo"));

            if (workspaceService.Exists(shaderPath))
            {
                displayTarget.shaderPath = workspaceService.OpenFile(shaderPath, FileAccess.Read);

                // Force the display to load the shader
                displayTarget.useCRT = true;
            }

            displayTarget.ResetResolution(tmpRes[0], tmpRes[1]);

            // Configure the shader from the bios
            EnableCRT(Convert.ToBoolean(bios.ReadBiosData(CRTBiosSettings.CRT.ToString(), "False")));
            Brightness(Convert.ToSingle(bios.ReadBiosData(CRTBiosSettings.Brightness.ToString(), "100")) / 100F);
            Sharpness(Convert.ToSingle(bios.ReadBiosData(CRTBiosSettings.Sharpness.ToString(), "-6")));
        }

        protected override void ConfigureKeyboard()
        {
            // Pass input mapping
            foreach (var keyMap in defaultKeys)
            {
                var rawValue = Convert.ToInt32(bios.ReadBiosData(keyMap.Key.ToString(), keyMap.Value.ToString(), true));
                //                if (rawValue is long)
                //                    rawValue = Convert.ToInt32(rawValue);

                var keyValue = rawValue;

                tmpEngine.SetMetadata(keyMap.Key.ToString(), keyValue.ToString());
            }

            tmpEngine.ControllerChip.RegisterKeyInput();
        }

        protected virtual void CreateWorkspaceService()
        {
            workspaceService = new WorkspaceService(new KeyValuePair<WorkspacePath, IFileSystem>(
                WorkspacePath.Root.AppendDirectory("App"),
                new PhysicalFileSystem(rootPath)));

            serviceManager.AddService(typeof(WorkspaceService).FullName, workspaceService);
        }

        /// <summary>
        ///     This is called when the runner first starts up.
        /// </summary>
        protected override void Initialize()
        {
            // Create the workspace starting at the App's directory
            CreateWorkspaceService();

            //            var biosPath = WorkspacePath.Root.AppendDirectory("App").AppendFile("bios.json");//Path.Combine(rootPath, "bios.json")));

            // Test if a bios file exists
            if (workspaceService.Exists(biosPath))
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

                // Configure the runner
                ConfigureRunner();

                // Load the game
                LoadDefaultGame();
            }
        }

        protected virtual void ConfigureWorkspace()
        {
            var mounts = new Dictionary<WorkspacePath, IFileSystem>();

            // Create the base directory in the documents and local storage folder

            // Get the base directory from the bios or use Pixel Vision 8 as the default name
            var baseDir = bios.ReadBiosData("BaseDir", "PixelVision8");

            tmpPath = Path.Combine(LocalStorage, baseDir, "Tmp");

            // Create an array of required directories
            var requiredDirectories = new Dictionary<string, string>
            {
                {"Storage", Path.Combine(LocalStorage, baseDir)},
                {"Tmp", tmpPath}
            };

            // Loop through the list of directories, make sure they exist and create them
            foreach (var directory in requiredDirectories)
            {
                if (!Directory.Exists(directory.Value)) Directory.CreateDirectory(directory.Value);

                // Add directories as mount points
                mounts.Add(WorkspacePath.Root.AppendDirectory(directory.Key), new PhysicalFileSystem(directory.Value));
            }

            // Mount the filesystem
            workspaceService.MountFileSystems(mounts);

            var userBios =
                workspaceService.ReadTextFromFile(userBiosPath);

            bios.ParseBiosText(userBios);

            workspaceService.SetupLogFile(WorkspacePath.Parse(bios.ReadBiosData("LogFilePath", "/Tmp/Log.txt")));
        }

        public override void ConfigureServices()
        {
            if (luaService == null)
                CreateLuaService();
        }

        protected LuaService luaService;

        public virtual void CreateLuaService()
        {
            
            luaService = new LuaService(this);

            // Register Lua Service
            serviceManager.AddService(typeof(LuaService).FullName, luaService);
        }

        /// <summary>
        ///     This mthod manually loads the game file's binary data then configures the engine and processes the files.
        /// </summary>
        protected virtual void LoadDefaultGame()
        {
            Load(bios.ReadBiosData("BootTool", "/PixelVisionOS/Tools/BootTool/"), RunnerMode.Booting);
            //
            //            var autoRunPath = bios.ReadBiosData("AutoRun", "/App/DefaultGame/");
            //
            //            // Create a new dictionary to store the file binary data
            //            var gameFiles = workspaceService.LoadGame(autoRunPath); //gamePath)));
            //
            //            // Configure a new PV8 engine to play the game
            //            ConfigureEngine();
            //
            //            // Process the files
            //            ProcessFiles(tmpEngine, gameFiles);
            //
            //            tmpEngine.name = autoRunPath;
        }

        public virtual void SaveGameData(string path, IEngine engine, SaveFlags saveFlags, bool useSteps = true)
        {
            // Simple save game exporter

            var saveExporter = new SavedDataExporter(path, engine);
            saveExporter.CalculateSteps();
            while (saveExporter.completed == false) saveExporter.NextStep();

            // Save file
            var saveFile = new Dictionary<string, byte[]>
            {
                {saveExporter.fileName, saveExporter.bytes}
            };

            workspaceService.SaveExporterFiles(saveFile);
        }

        //        public override void ResetGame()
        //        {
        //            LoadDefaultGame();
        //        }


        protected override void OnExiting(object sender, EventArgs args)
        {
            ShutdownSystem();

            base.OnExiting(sender, args);
        }

        public virtual void ShutdownSystem()
        {
            // We only want to call this once so don't run if shutdown is true
            //            if (shutdown)
            //                return;

            // Shutdown the active game
            ShutdownActiveEngine();

            // Toggle the shutdown flag
            //            shutdown = true;

            //            UpdateDiskInBios();
            SaveBiosChanges();

            // Save any changes to the bios to the user's custom bios file
            workspaceService.ShutdownSystem();
        }

        public override void ShutdownActiveEngine()
        {
            // Look to see if there is an active engine
            if (ActiveEngine == null) return;

            base.ShutdownActiveEngine();

            if (ActiveEngine.GameChip.SaveSlots > 0)
                //Print("Active Engine To Save", activeEngine.name);

                SaveGameData("/Game/", ActiveEngine, SaveFlags.SaveData,
                    false);

            // Save the active disk
            //                workspaceService.SaveActiveDisk();
        }

        public void SaveBiosChanges()
        {
            // Look for changes
            if (bios.userBiosChanges != null)
            {
                // Get the path to where the user's bios should be saved
                //                var path = FileSystemPath.Parse("/User/").AppendFile("user-bios.json");

                if (!workspaceService.Exists(userBiosPath))
                {
                    var newBios = workspaceService.CreateFile(userBiosPath);
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

        public override void DisplayWarning(string message)
        {
            workspaceService.UpdateLog(message);
        }

        protected delegate bool EnableCRTDelegator(bool? toggle);

        protected delegate float BrightnessDelegator(float? brightness = null);

        protected delegate float SharpnessDelegator(float? sharpness = null);

        #region Runner settings

        /// <summary>
        ///     Override Volume so it saves changes to the bios.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override int Volume(int? value = null)
        {
            var vol = base.Volume(value);

            bios.UpdateBiosData(BiosSettings.Volume.ToString(), vol.ToString());

            return vol;
        }

        /// <summary>
        ///     Change the mute value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override bool Mute(bool? value = null)
        {
            var mute = base.Mute(value);

            bios.UpdateBiosData(BiosSettings.Mute.ToString(), mute.ToString());

            return mute;
        }


        /// <summary>
        ///     Scale the resolution.
        /// </summary>
        /// <param name="scale"></param>
        /// <param name="fullScreen"></param>
        public override int Scale(int? scale = null)
        {
            var value = base.Scale(scale);

            bios.UpdateBiosData(BiosSettings.Scale.ToString(), value.ToString());

            return value;
        }


        public override bool Fullscreen(bool? value = null)
        {
            var full = base.Fullscreen(value);

            bios.UpdateBiosData(BiosSettings.FullScreen.ToString(), full.ToString());

            return full;
        }

        public override bool StretchScreen(bool? value = null)
        {
            var stretch = base.StretchScreen(value);

            bios.UpdateBiosData(BiosSettings.StretchScreen.ToString(), stretch.ToString());

            return stretch;
        }

        public override bool CropScreen(bool? value = null)
        {
            var crop = base.CropScreen(value);

            bios.UpdateBiosData(BiosSettings.CropScreen.ToString(), crop.ToString());

            return crop;
        }

        #endregion

        #region CRT Filter Settings

        public bool EnableCRT(bool? toggle)
        {
            if (toggle.HasValue)
            {
                displayTarget.useCRT = toggle.Value;
                bios.UpdateBiosData(CRTBiosSettings.CRT.ToString(), toggle.Value.ToString());
                InvalidateResolution();
            }

            return displayTarget.useCRT;
        }

        public float Brightness(float? brightness = null)
        {
            if (brightness.HasValue)
            {
                displayTarget.brightness = brightness.Value;
                bios.UpdateBiosData(CRTBiosSettings.Brightness.ToString(), (brightness * 100).ToString());
            }

            return displayTarget.brightness;
        }

        public float Sharpness(float? sharpness = null)
        {
            if (sharpness.HasValue)
            {
                displayTarget.sharpness = sharpness.Value;
                bios.UpdateBiosData(CRTBiosSettings.Sharpness.ToString(), sharpness.ToString());
            }

            return displayTarget.sharpness;
        }

        #endregion
    }
}