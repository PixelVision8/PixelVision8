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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MoonSharp.Interpreter;
using MoonSharp.VsCodeDebugger;
using MoonSharp.VsCodeDebugger.DebuggerLogic;
using PixelVision8.Engine;
using PixelVision8.Engine.Chips;
using PixelVision8.Runner.Data;
using PixelVision8.Runner.Editors;
using PixelVision8.Runner.Services;
using PixelVision8.Runner.Utils;
using PixelVision8.Runner.Workspace;

namespace PixelVision8.Runner

{
    /// <summary>
    ///     This is the main type for your game.
    /// </summary>
    public class DesktopRunner : GameRunner
    {
        public readonly Dictionary<ActionKeys, Keys> actionKeys = new Dictionary<ActionKeys, Keys>
        {
            // TODO need to add the other action keys here so they are mapped in the bios
            {ActionKeys.BackKey, Keys.Escape},
            {ActionKeys.ScreenShotKey, Keys.D2},
            {ActionKeys.RecordKey, Keys.D3},
            {ActionKeys.RestartKey, Keys.D4}
        };

        private GameEditor _editor;
        public bool autoRunEnabled = true;
        public bool backKeyEnabled = true;
        public BiosService bios;
        protected WorkspacePath biosPath = WorkspacePath.Root.AppendDirectory("App").AppendFile("bios.json");
        protected IControllerChip controllerChip;
        protected string documentsPath;
        protected bool ejectingDisk;
        public string[] GameFiles;
        protected GifExporter gifEncoder;

        public List<KeyValuePair<string, Dictionary<string, string>>> loadHistory =
            new List<KeyValuePair<string, Dictionary<string, string>>>();

        public bool LuaMode = true;
        protected LuaService luaService;
        protected bool mountingDisk;
        protected Dictionary<string, string> nextMetaData;
        protected RunnerMode nextMode;
        protected string nextPathToLoad;
        protected string rootPath;
        protected bool screenShotActive;
        protected float screenshotDelay = 200f;
        private ScreenshotService screenshotService;
        protected float screenshotTime;
        protected bool shutdown;
        public string systemName;
        public string SystemVersion;
        protected string tmpPath;
        public WorkspaceService workspaceService;
        protected WorkspaceServicePlus workspaceServicePlus;

        private string bootDisk;
        
        // Default path to where PV8 workspaces will go
        public DesktopRunner(string rootPath, string bootDisk = null)
        {
            // Fix a bug related to parsing numbers in Europe, among other things
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
            
            this.rootPath = rootPath;
            server = new MoonSharpVsCodeDebugServer(1985);
            server.Start();

            if (bootDisk != null && bootDisk.EndsWith(".pv8") ? File.Exists(bootDisk) : Directory.Exists(bootDisk))
            {
                this.bootDisk = bootDisk;
            }
        }

        protected MoonSharpVsCodeDebugServer server;
        private bool attachScript = true;

        public string SessionId { get; protected set; }
        protected WorkspacePath userBiosPath => WorkspacePath.Parse("/Storage/user-bios.json");

        public string LocalStorage
        {
            get
            {
                var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

                // TODO replace with something better
                DisplayWarning("Local Storage Located at " + localAppData);

                return localAppData;
            }
        }

        public override List<string> DefaultChips
        {
            get
            {
                // Get the list of default chips
                var chips = base.DefaultChips;

                // Add the custom C# game chip
                chips.Add(typeof(LuaDebugGameChip).FullName);

                // Return the list of chips
                return chips;
            }
        }


        protected string Documents => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        public GameDataExportService ExportService { get; protected set; }
        public bool Recording { get; set; }

        protected string DefaultWindowTitle =>
            bios.ReadBiosData(BiosSettings.SystemName.ToString(), "Pixel Vision 8 Runner") + " " +
            bios.ReadBiosData(BiosSettings.SystemVersion.ToString(), "0.0.0");

        public GameEditor Editor
        {
            get
            {
                if (_editor == null) _editor = new GameEditor(this, ServiceManager);

                return _editor;
            }
        }

        public override void CreateLoadService()
        {
            loadService = new LoadService(new WorkspaceFileLoadHelper(workspaceService));

            Script.DefaultOptions.ScriptLoader = new ScriptLoaderUtil(workspaceService);
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

            if (displayTarget.HasShader() == false)
            {
                // Configure CRT shader
                var shaderPath = WorkspacePath.Parse(bios.ReadBiosData(CRTSettings.CRTEffectPath.ToString(),
                    "/App/Effects/crt-lottes-mg.ogl.mgfxo"));

                if (workspaceService.Exists(shaderPath))
                    displayTarget.shaderPath = workspaceService.OpenFile(shaderPath, FileAccess.Read);
            }


            displayTarget.ResetResolution(tmpRes[0], tmpRes[1]);

            // Configure the shader from the bios
            Brightness(Convert.ToSingle(bios.ReadBiosData(CRTSettings.Brightness.ToString(), "100")) / 100F);
            Sharpness(Convert.ToSingle(bios.ReadBiosData(CRTSettings.Sharpness.ToString(), "-6")));
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

        public override void ActivateEngine(IEngine engine)
        {
            // At this point this game is fully configured so all chips are accessible for extra configuring

            if (mode == RunnerMode.Loading)
                ((LuaGameChip) engine.GameChip).LuaScript.Globals["DebuggerAttached"] =
                    new Func<bool>(AwaitDebuggerAttach);

            // Save a reference to the controller chip so we can listen for special key events
            controllerChip = engine.ControllerChip;

            // Activate the game
            BaseActivateEngine(engine);
        }

        private bool AwaitDebuggerAttach()
        {
            var connected = server.Connected;

            if (connected == false && attachScript)
            {
                var tempQualifier = (LuaGameChip) tmpEngine.GameChip;
                // Kick off the first game script file
                tempQualifier.LoadScript(tempQualifier.DefaultScriptPath);

                var scriptPath = workspaceService.GetPhysicalPath(WorkspacePath.Parse(ActiveEngine.Name + "code.lua"));

                server.AttachToScript(tempQualifier.LuaScript, scriptPath);

                attachScript = false;
            }

            UpdateTitle();

            return connected;
        }

        protected void OnTextInput(object sender, TextInputEventArgs e)
        {
            // Pass this to the input chip
            controllerChip.SetInputText(e.Character, e.Key);
        }

        public virtual void BaseActivateEngine(IEngine engine)
        {
            if (engine == null) return;

            // Make the loaded engine active
            ActiveEngine = engine;

            if (ActiveEngine?.GameChip is LuaGameChip)
            {
                var tempQualifier = (LuaGameChip) ActiveEngine.GameChip;
                tempQualifier.LoadScript(tempQualifier.DefaultScriptPath);
            }

            ActiveEngine.ResetGame();

            // After loading the game, we are ready to run it.
            ActiveEngine.RunGame();

            // Reset the game's resolution
            ResetResolution();

            // Make sure that the first frame is cleared with the default color
            ActiveEngine.GameChip.Clear();

            // No ned to  call the base method since the logic is copied here
        }

        protected override void ConfigureRunner()
        {
            // Save the session ID
            SessionId = DateTime.Now.ToString("yyyyMMddHHmmssffff");

            SystemVersion = bios.ReadBiosData(BiosSettings.SystemVersion.ToString(), "0.0.0", true);
            systemName = bios.ReadBiosData("SystemName", "PixelVision8", true);

            base.ConfigureRunner();

            // TODO This may be a string
            Volume(MathHelper.Clamp(
                Convert.ToInt32(bios.ReadBiosData(BiosSettings.Volume.ToString(), "40", true)),
                0, 100));

            Mute(Convert.ToBoolean(bios.ReadBiosData(BiosSettings.Mute.ToString(), "False", true)));

            Window.Title =
                bios.ReadBiosData(BiosSettings.SystemName.ToString(), "Pixel Vision 8 Runner", true);

            // Capture text input from the window
            Window.TextInput += OnTextInput;

            screenshotService = new ScreenshotService(workspaceServicePlus);
            ExportService = new GameDataExportService(); //TODO Need to create a new AudioClipAdaptor

            autoShutdown = bios.ReadBiosData("AutoShutdown", "True") == "True";

            RefreshActionKeys();

            UpdateTitle();
        }

        public void RefreshActionKeys()
        {
            var actions = Enum.GetValues(typeof(ActionKeys)).Cast<ActionKeys>();
            foreach (var action in actions)
                actionKeys[action] =
                    (Keys) Convert.ToInt32(bios.ReadBiosData(action.ToString(), ((int) actionKeys[action]).ToString(),
                        true));
        }

        protected void CreateWorkspaceService()
        {
            // workspaceService = new WorkspaceService(new KeyValuePair<WorkspacePath, IFileSystem>(
            //         WorkspacePath.Root.AppendDirectory("App"),
            //         new PhysicalFileSystem(rootPath)));
            //
            //     ServiceManager.AddService(typeof(WorkspaceService).FullName, workspaceService);

            // TODO use partial class to add in support for workspace APIs needed by tools
            workspaceServicePlus = new WorkspaceServicePlus(
                new KeyValuePair<WorkspacePath, IFileSystem>(
                    WorkspacePath.Root.AppendDirectory("App"),
                    new PhysicalFileSystem(rootPath)
                )
            );

            // Pass the new service back to the base class
            workspaceService = workspaceServicePlus;

            ServiceManager.AddService(typeof(WorkspaceService).FullName, workspaceService);
        }

        /// <summary>
        ///     This is called when the runner first starts up.
        /// </summary>
        protected override void Initialize()
        {
            // Create the workspace starting at the App's directory
            CreateWorkspaceService();

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

                // Reset the filter based from bios after everything loads up
                EnableCRT(Convert.ToBoolean(bios.ReadBiosData(CRTSettings.CRT.ToString(), "False")));
            }
        }

        protected virtual void ConfigureWorkspace()
        {
            // Call the base ConfigureWorkspace method to configure the workspace correctly
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
           

            var userBios = workspaceService.ReadTextFromFile(userBiosPath);

            bios.ParseBiosText(userBios);

            workspaceService.SetupLogFile(WorkspacePath.Parse(bios.ReadBiosData("LogFilePath", "/Tmp/Log.txt")));

            // Everything below is custom to PV8

            // Define PV8 disk extensions from the bios
            workspaceServicePlus.archiveExtensions =
                bios.ReadBiosData("ArchiveExtensions", "zip,pv8,pvt,pvs,pva,pvr", true).Split(',')
                    .ToList();

            //  Define the valid file extensions from the bios
            workspaceServicePlus.fileExtensions =
                bios.ReadBiosData("FileExtensions", "png,lua,json,txt,wav,cs", true).Split(',')
                    .ToList();

            // Define the files required to make a game valid from the bios
            workspaceServicePlus.requiredFiles =
                bios.ReadBiosData("RequiredFiles", "data.json,info.json", true).Split(',')
                    .ToList();

            // Include any library files in the OS mount point
            workspaceServicePlus.osLibPath = WorkspacePath.Root.AppendDirectory("PixelVisionOS")
                .AppendDirectory(bios.ReadBiosData("LibsDir", "Libs", true));

                var createWorkspace = bios.ReadBiosData("CreateWorkspace", "True");

            if (createWorkspace == "True")
            {
                // Look for the workspace name
                var workspaceName = bios.ReadBiosData("WorkspaceDir", "Workspace", true);
            
                // Set the TotalDisks disks
                workspaceServicePlus.MaxDisks = int.Parse(bios.ReadBiosData("MaxDisks", "2", true));

                // Create the real system path to the documents folder
                documentsPath = Path.Combine(Documents, baseDir);

                if (Directory.Exists(documentsPath) == false) Directory.CreateDirectory(documentsPath);

                // Create a new physical file system mount
                workspaceServicePlus.AddMount(new KeyValuePair<WorkspacePath, IFileSystem>(
                    WorkspacePath.Root.AppendDirectory("User"),
                    new PhysicalFileSystem(documentsPath)));

                // Mount the workspace drive
                workspaceServicePlus.MountWorkspace(workspaceName);
                
            }
            
            workspaceServicePlus.RebuildWorkspace();
        }

        public void EjectDisk(string path)
        {
            ejectingDisk = true;

            workspaceServicePlus.EjectDisk(WorkspacePath.Parse(path));

            UpdateDiskInBios();

            AutoLoadDefaultGame();

            ejectingDisk = false;
        }

        public void EnableAutoRun(bool value)
        {
            autoRunEnabled = value;
        }

        public void EnableBackKey(bool value)
        {
            backKeyEnabled = value;
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
                Load(tool, RunnerMode.Loading, metaData);
            else
                Back(metaData);
        }

        protected override void Update(GameTime gameTime)
        {
            
            
            // while (Sdl.PollEvent(out ev) == 1)
            // {
            if (ActiveEngine == null || shutdown) return;

            // TODO make sure this order is correct or maybe it can be cleaned up
            if (screenShotActive)
            {
                screenshotTime += timeDelta;

                if (screenshotTime > screenshotDelay)
                {
                    screenShotActive = false;
                    screenshotTime = 0;
                }

                UpdateTitle();
            }

            if (controllerChip.GetKeyDown(Keys.LeftControl) ||
                controllerChip.GetKeyDown(Keys.LeftControl))
            {
                if (controllerChip.GetKeyUp(actionKeys[ActionKeys.ScreenShotKey]))
                {
                    // Only take a screenshot when one isn't being saved
                    if (!screenShotActive)
                        //                            Console.WriteLine("Take Picture");

                        screenShotActive = screenshotService.TakeScreenshot(ActiveEngine);
                }
                else if (controllerChip.GetKeyUp(actionKeys[ActionKeys.RecordKey]))
                {
                    if (Recording)
                        StopRecording();
                    else
                        StartRecording();
                }
                else if (controllerChip.GetKeyUp(actionKeys[ActionKeys.RestartKey]))
                {
                    if (controllerChip.GetKeyDown(Keys.LeftShift) || controllerChip.GetKeyDown(Keys.RightShift))
                        AutoLoadDefaultGame();
                    else
                        ResetGame();
                }
            }
            else if (controllerChip.GetKeyUp(Keys.Escape) && backKeyEnabled)
            {
                Back();
            }


            // Capture Script errors
            try
            {
                base.Update(gameTime);
            }
            catch (Exception e)
            {
                DisplayError(ErrorCode.Exception,
                    new Dictionary<string, string>
                        {{"@{error}", e is ScriptRuntimeException error ? error.DecoratedMessage : e.Message}},
                    e);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            try
            {
                // If we are taking a screenshot, clear with white and don't draw the runner.
                if (screenShotActive)
                {
                    // TODO need to figure out why this is not displaying white when a screen shot happens
                    graphics.GraphicsDevice.Clear(Color.White);
                }
                else
                {
                    base.Draw(gameTime);

                    if (Recording) gifEncoder.AddFrame(timeDelta / 1000f);
                }

                workspaceServicePlus.SaveLog();
            }
            catch (Exception e)
            {
                DisplayError(ErrorCode.Exception,
                    new Dictionary<string, string>
                        {{"@{error}", e is ScriptRuntimeException error ? error.DecoratedMessage : e.Message}}, e);
            }
        }

        // Custom method to use instaed of the default BootDone
        public void BootDone(bool safeMode = false)
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

            // We only activate this if there is not bios setting to disable it
            if (bios.ReadBiosData("FileDiskMounting", "True", true) != "False")
            {
                // Setup Drag and drop support
                // Window.FileDropped += (o, e) => OnFileDropped(o, e);
                // Window.FileDropped += (o, e) => OnFileDropped(o, e);

                // Disable auto run when loading up the default disks
                autoRunEnabled = false;

                for (var i = 0; i < workspaceServicePlus.MaxDisks; i++)
                {
                    var diskPath = bios.ReadBiosData("Disk" + i, "none", true);
                    if (diskPath != "none" && diskPath != "")
                        // manually mount each disk since we are not going to load from them
                        workspaceServicePlus.MountDisk(diskPath);
                }
            }

            if (bootDisk == null)
            {
                AutoLoadDefaultGame();
            }
            else
            {
                // Force runner to auto run disk
                autoRunEnabled = true;
                
                // Mount the disk
                MountDisk(bootDisk);
            }
            
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

        // public virtual void BootDone(bool safeMode = false)
        // {
        //     // Only call BootDone when the runner is booting.
        //     if (mode != RunnerMode.Booting) return;
        //
        //     // Test to see if we are in save mode before loading the bios
        //     if (safeMode)
        //     {
        //         // Clear the current bios
        //         bios.Clear();
        //
        //         // Read the bios text
        //         var biosText = workspaceService.ReadTextFromFile(biosPath);
        //
        //         // Reparse the bios text
        //         bios.ParseBiosText(biosText);
        //
        //     }
        //
        //     AutoLoadDefaultGame();
        // }

        /// <summary>
        ///     This mthod manually loads the game file's binary data then configures the engine and processes the files.
        /// </summary>
        protected virtual void LoadDefaultGame()
        {
            Load(bios.ReadBiosData("BootTool", "/PixelVisionOS/Tools/BootTool/"), RunnerMode.Booting);
        }

        public virtual void DisplayError(ErrorCode code, Dictionary<string, string> tokens = null,
            Exception exception = null)
        {
            if (mode == RunnerMode.Error) return;

            // TODO should this only work on special cases?
            autoRunEnabled = true;

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

        // TODO need to make sure this still works correctly
        public virtual void AutoLoadDefaultGame()
        {
            // This doesn't call the base AutoLoadDefaultGame becasue it needs to check for disks

            // Enable auto run by default
            autoRunEnabled = true;

            loadHistory.Clear();
            //            metaDataHistory.Clear();

            // Look to see if we have the bios default tool in the OS folder
            try
            {
                var biosAutoRun = bios.ReadBiosData("AutoRun");

                // Check to see if this path exists
                if (workspaceServicePlus.Exists(WorkspacePath.Parse(biosAutoRun)))
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
                // Do nothing
            }

            // Try to boot from the first disk
            try
            {
                var firstDiskPath = workspaceServicePlus.Disks.First().EntityName;

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
            try
            {
                mountingDisk = true;

                var diskName = workspaceServicePlus.MountDisk(path);

                // Only try to auto run a game if this is enabled in the runner
                if (autoRunEnabled)
                {
                    // If we are running this disk clear the previous history
                    loadHistory.Clear();

                    // Run the disk
                    AutoRunGameFromDisk(diskName);
                }
                else
                {
                    ResetGame();
                }

                mountingDisk = false;
            }
            catch
            {
                // TODO need to make sure we show a better error to explain why the disk couldn't load
                DisplayError(ErrorCode.NoAutoRun);
            }
        }

        public void AutoRunGameFromDisk(string diskName)
        {
            var diskPath = workspaceServicePlus.AutoRunGameFromDisk(diskName);

            if (diskPath != null)
            {
                // Create new meta data for the game. We wan to display the disk insert animation.
                var metaData = new Dictionary<string, string>
                {
                    {"showDiskAnimation", mountingDisk.ToString().ToLower()},
                    {"showEjectAnimation", ejectingDisk.ToString().ToLower()}
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
            if (server.Current != null) server.Detach(server.Current);

            attachScript = true;

            try
            {
                // Look to see if there is an active engine
                if (ActiveEngine == null) return;

                base.ShutdownActiveEngine();

                if (ActiveEngine.GameChip.SaveSlots > 0)
                    SaveGameData("/Game/", ActiveEngine, SaveFlags.SaveData,
                        false);

                // Save the active disk
                workspaceServicePlus.SaveActiveDisk();
            }
            catch (Exception e)
            {
                DisplayError(ErrorCode.Exception,
                    new Dictionary<string, string>
                        {{"@{error}", e is ScriptRuntimeException error ? error.DecoratedMessage : e.Message}}, e);
            }
        }

        public override void ConfigureEngine(Dictionary<string, string> metaData = null)
        {
            LuaMode = Array.IndexOf(GameFiles, "code.cs") == -1;
            if (LuaMode)
            {
                CreateLuaService();

                var chips = DefaultChips;

                // Add the Lua game chip
                // if(LuaMode)
                chips.Add(typeof(LuaGameChip).FullName);

                // TODO need to move this to a base config engine method so the parent can be called

                // Had to disable the active game manually before this is called so copied base logic here
                tmpEngine = CreateNewEngine(DefaultChips);

                ConfigureServices();

                // Pass all meta data into the engine instance
                if (metaData != null)
                    foreach (var entry in metaData)
                        tmpEngine.SetMetadata(entry.Key, entry.Value);

                ConfigureKeyboard();
                ConfiguredControllers();

                // Get a reference to the    Lua game
                var game = tmpEngine.GameChip as LuaGameChip;

                // Get the script
                var luaScript = game.LuaScript;

                // Limit which APIs are exposed based on the mode for security
                // if (mode == RunnerMode.Loading)
                // {
                luaScript.Globals["StartNextPreload"] = new Action(StartNextPreload);
                luaScript.Globals["PreloaderComplete"] = new Action(RunGame);
                luaScript.Globals["ReadPreloaderPercent"] =
                    new Func<int>(() => (int) MathHelper.Clamp(loadService.Percent * 100, 0, 100));

                // }else
                if (mode == RunnerMode.Booting)
                    luaScript.Globals["BootDone"] = new Action<bool>(BootDone);
                else
                    luaScript.Globals["LoadGame"] =
                        new Func<string, Dictionary<string, string>, bool>((path, metadata) =>
                            Load(path, RunnerMode.Loading, metadata));

                // Global System APIs
                luaScript.Globals["EnableCRT"] = new Func<bool?, bool>(EnableCRT);
                luaScript.Globals["Brightness"] = new Func<float?, float>(Brightness);
                luaScript.Globals["Sharpness"] = new Func<float?, float>(Sharpness);
                luaScript.Globals["SystemVersion"] = new Func<string>(() => SystemVersion);
                luaScript.Globals["SystemName"] = new Func<string>(() => systemName);
                luaScript.Globals["SessionID"] = new Func<string>(() => SessionId);
                luaScript.Globals["ReadBiosData"] = new Func<string, string, string>((key, defaultValue) =>
                    bios.ReadBiosData(key, defaultValue));
                luaScript.Globals["WriteBiosData"] = new Action<string, string>(bios.UpdateBiosData);

                luaScript.Globals["ControllerConnected"] = new Func<int, bool>(tmpEngine.ControllerChip.IsConnected);


                // Get a reference to the Lua game
                // var game = tmpEngine.GameChip as LuaGameChip;

                // Get the script
                // var luaScript = game.LuaScript;

                luaScript.Globals["EnableAutoRun"] = new Action<bool>(EnableAutoRun);
                luaScript.Globals["EnableBackKey"] = new Action<bool>(EnableBackKey);


                if (mode == RunnerMode.Playing)
                {
                    // Inject the PV8 runner special global function
                    luaScript.Globals["IsExporting"] = new Func<bool>(ExportService.IsExporting);
                    luaScript.Globals["ReadExportPercent"] = new Func<int>(ExportService.ReadExportPercent);
                    luaScript.Globals["ReadExportMessage"] =
                        new Func<Dictionary<string, object>>(ExportService.ReadExportMessage);
                    luaScript.Globals["ShutdownSystem"] = new Action(ShutdownSystem);
                    luaScript.Globals["QuitCurrentTool"] = (QuitCurrentToolDelagator) QuitCurrentTool;
                    luaScript.Globals["RefreshActionKeys"] = new Action(RefreshActionKeys);
                    luaScript.Globals["DocumentPath"] = new Func<string>(() => documentsPath);
                    luaScript.Globals["TmpPath"] = new Func<string>(() => tmpPath);
                    luaScript.Globals["DiskPaths"] = new Func<WorkspacePath[]>(() => workspaceServicePlus.Disks);
                    luaScript.Globals["SharedLibPaths"] =
                        new Func<WorkspacePath[]>(() => workspaceServicePlus.SharedLibDirectories().ToArray());
                    // luaScript.Globals["SaveActiveDisks"] = new Action(() =>
                    // {
                    //     var disks = workspaceServicePlus.Disks;
                    //
                    //     foreach (var disk in disks) workspaceServicePlus.SaveDisk(disk);
                    // });
                    luaScript.Globals["EjectDisk"] = new Action<string>(EjectDisk);
                    luaScript.Globals["RebuildWorkspace"] = new Action(workspaceServicePlus.RebuildWorkspace);
                    luaScript.Globals["MountDisk"] = new Action<WorkspacePath>(path =>
                    {
                        var segments = path.GetDirectorySegments();

                        var systemPath = Path.PathSeparator.ToString();

                        if (segments[0] == "Disk")
                        {
                        }
                        else if (segments[0] == "Workspace")
                        {
                            // TODO the workspace could have a different name so we should check the bios
                            systemPath = Path.Combine(documentsPath, segments[0]);
                        }

                        for (var i = 1; i < segments.Length; i++) systemPath = Path.Combine(systemPath, segments[i]);

                        systemPath = Path.Combine(systemPath,
                            path.IsDirectory ? Path.PathSeparator.ToString() : path.EntityName);


                        //                Console.WriteLine("Mount Disk From " + systemPath);

                        MountDisk(systemPath);
                    });

                    luaScript.Globals["OperatingSystem"] = new Func<string>(OperatingSystem);

                    // Register the game editor with  the lua service
                    UserData.RegisterType<GameEditor>();
                    luaScript.Globals["gameEditor"] = Editor;
                }

                if (mode == RunnerMode.Booting)
                    // Force the lua script to use this boot done logic instead
                    luaScript.Globals["BootDone"] = new Action<bool>(BootDone);

                if (mode == RunnerMode.Loading)
                {
                    luaScript.Globals["StartUnload"] = new Action(StartUnload);
                    luaScript.Globals["UnloadProgress"] = new Func<int>(UnloadProgress);
                    luaScript.Globals["EndUnload"] = new Action(EndUnload);
                }
            }
            else
            {
                // Had to disable the active game manually before this is called so copied base logic here
                tmpEngine = CreateNewEngine(DefaultChips);

                // Pass all meta data into the engine instance
                if (metaData != null)
                    foreach (var entry in metaData)
                        tmpEngine.SetMetadata(entry.Key, entry.Value);

                ConfigureKeyboard();
                ConfiguredControllers();
            }
        }

        protected string OperatingSystem()
        {
            var os = Environment.OSVersion;
            var pid = os.Platform;
            switch (pid)
            {
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                case PlatformID.WinCE:
                    return "Windows";

                case PlatformID.Unix:
                    return "Linux";
                case PlatformID.MacOSX:
                    return "Mac";
                default:
                    return "Unknown";
            }
        }

        protected void StartUnload()
        {
            Console.WriteLine("StartUnload");
        }

        protected int UnloadProgress()
        {
            Console.WriteLine("UnloadProgress");

            return 0;
        }

        protected void EndUnload()
        {
            Console.WriteLine("EndUnload");
        }

        public override void RunGame()
        {
            // TODO This should be moved into the desktop runner?
            autoRunEnabled = true;

            // Re-enable back when loading a new game
            backKeyEnabled = true;

            base.RunGame();
        }

        public bool Load(string path, RunnerMode newMode = RunnerMode.Playing,
            Dictionary<string, string> metaData = null)
        {
            // Make sure we stop recording when loading a new game
            if (Recording) StopRecording();

            if (newMode == RunnerMode.Loading)
            {
                // Create metadata if it doesn't exists so we can store the eject value for the loader
                if (metaData == null) metaData = new Dictionary<string, string>();

                // Tell the loader to show eject animation
                if (metaData.ContainsKey("showEjectAnimation"))
                    metaData["showEjectAnimation"] = ejectingDisk.ToString().ToLower();
                else
                    metaData.Add("showEjectAnimation", ejectingDisk.ToString().ToLower());

                if (metaData.ContainsKey("showDiskAnimation"))
                    metaData["showDiskAnimation"] = mountingDisk.ToString().ToLower();
                else
                    metaData.Add("showDiskAnimation", mountingDisk.ToString().ToLower());
            }

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
                        {"nextMode", nextMode.ToString()}
                    };

                    // Copy over any previous properties
                    if (nextMetaData != null)
                        foreach (var property in nextMetaData)
                            if (metaData.ContainsKey(property.Key))
                                metaData[property.Key] = property.Value;
                            else
                                metaData.Add(property.Key, property.Value);

                    // Get the default path to the load tool from the bios
                    path = bios.ReadBiosData("LoadTool", "/PixelVisionOS/Tools/LoadTool/", true);

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
                    // lastMode = mode;

                    var metaDataCopy = metaData.ToDictionary(entry => entry.Key,
                        entry => entry.Value);

                    if (loadHistory.Count > 0)
                    {
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

                bool success;

                // Detect if we should be preloading the archive
                if (mode == RunnerMode.Booting || mode == RunnerMode.Error || mode == RunnerMode.Loading)
                    displayProgress = false;
                else
                    displayProgress = true;

                if (ActiveEngine != null) ShutdownActiveEngine();

                // TODO find a better way to do this

                // Have the workspace run the game from the current path
                GameFiles = workspaceService.LoadGame(path);

                // Create a new tmpEngine
                ConfigureEngine(metaData);

                // Path the full path to the engine's name
                tmpEngine.Name = path;

                if (GameFiles != null)
                {
                    // Read and Run the disk
                    ProcessFiles(tmpEngine, GameFiles, displayProgress);
                    success = true;
                }
                else
                {
                    DisplayError(ErrorCode.LoadError, new Dictionary<string, string> {{"@{path}", path}});
                    success = false;
                }

                GameFiles = null;

                // Create new FileSystemPath
                return success;
            }
            catch (Exception e)
            {
                DisplayError(ErrorCode.Exception,
                    new Dictionary<string, string>
                        {{"@{error}", e is ScriptRuntimeException error ? error.DecoratedMessage : e.Message}}, e);
            }


            return false;
        }

        public void SaveGameData(string path, IEngine engine, SaveFlags saveFlags, bool useSteps = true)
        {
            // Export the current game

            // TODO exporter needs a callback when its completed
            ExportService.ExportGame(path, engine, saveFlags, useSteps);
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            ShutdownSystem();

            base.OnExiting(sender, args);
        }

        public override void ConfigureServices()
        {
            CreateLuaService();

            ServiceManager.AddService(typeof(GameDataExportService).FullName, ExportService);
        }

        public override void ProcessFiles(IEngine tmpEngine, string[] files, bool displayProgress = false)
        {
            // Look for a CS file
            var csFilePaths = files.Where(p => p.EndsWith(".cs")).ToArray();
            if (csFilePaths.Length > 0)
                //Roslyn mode. Build the game. TODO: correct to use workspace paths. Hardcoded for Proof-Of-Concept
                CompileFromSource(csFilePaths);

            base.ProcessFiles(tmpEngine, files, displayProgress);
        }

        public void CompileFromSource(string[] files)
        {
            var total = files.Length;

            var syntaxTrees = new SyntaxTree[total];

            for (var i = 0;
                i < total;
                i++)
            {
                var path = WorkspacePath.Parse(files[i]);

                var data = workspaceService.ReadTextFromFile(path);

                syntaxTrees[i] = CSharpSyntaxTree.ParseText(data);
            }

            //Compilation options, should line up 1:1 with Visual Studio since it's the same underlying compiler.
            var options = new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                optimizationLevel: OptimizationLevel.Release,
                moduleName: "RoslynGame");

            //All of these are mandatory. This appears to the be minimum needed. Uncertain as of initial PoC if anything else outside of this needs referenced.
            var references = new MetadataReference[]
            {
                MetadataReference.CreateFromFile(typeof(object).Assembly.Location), //System.Private.CoreLib
                MetadataReference.CreateFromFile(typeof(Console).Assembly.Location), //System.Console
                MetadataReference.CreateFromFile(typeof(System.Runtime.AssemblyTargetedPatchBandAttribute).Assembly
                    .Location), //System.Runtime
                MetadataReference.CreateFromFile(typeof(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo).Assembly
                    .Location), //Microsoft.CSharp
                MetadataReference.CreateFromFile(typeof(GameChip).Assembly.Location), //PixelVision8Runner
                MetadataReference.CreateFromFile(typeof(Game).Assembly.Location), //MonoGameFramework
                MetadataReference.CreateFromFile(Assembly.Load("netstandard")
                    .Location) //Required due to a .NET Standard 2.0 dependency somewhere.
            };

            var compiler = CSharpCompilation.Create("LoadedGame", syntaxTrees, references, options);

            //Compile the existing file into memory, or error out.
            var stream = new MemoryStream();

            var compileResults = compiler.Emit(stream);
            if (compileResults.Success)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }

            else
            {
                var errors = compileResults.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();

                // TODO Need to get the error from the compiler
                // var e = "Code could not be compiled";
                DisplayError(ErrorCode.Exception,
                    new Dictionary<string, string>
                    {
                        {
                            "@{error}",
                            errors.Count > 0
                                ? errors[0].GetMessage()
                                : "There was an unknown errror trying to compile a C# file."
                        }
                    });
                //TODO: error handling, use data from compileResults to show user what's wrong.
                return;
            }

            //Get the DLL into active memory so we can use it.
            var roslynassembly = stream.ToArray();
            var loadedAsm = Assembly.Load(roslynassembly);

            var roslynGameChipType =
                loadedAsm.GetType("PixelVisionRoslyn.RoslynGameChip"); //This type much match what's in code.cs.
            //Could theoretically iterate over types until once that inherits from GameChip is found, but this Proof of Concept demonstrates the baseline feature.

            // tmpEngine.GameChip.Deactivate(); //Remove the previous LuaGameChip.
            tmpEngine.ActivateChip("GameChip", (AbstractChip) Activator.CreateInstance(roslynGameChipType))
                ; //Inserts the DLL's GameChip descendent into the engine.
        }

        public virtual void CreateLuaService()
        {
            // Make sure we only have one instance of the lua service
            if (luaService != null)
                return;

            luaService = new LuaServicePlus(this);

            // Register Lua Service
            ServiceManager.AddService(typeof(LuaService).FullName, luaService);
        }

        public void OnFileDropped(object gameWindow, string path)
        {
            if (shutdown == false) MountDisk(path);
        }

        public void ResetGame()
        {
            // Make sure we don't reload when booting or loading
            if (mode == RunnerMode.Booting || mode == RunnerMode.Loading || loadHistory.Count == 0) return;

            //TODO this nees to also pass in the last metaData state
            // Load the last game from the history

            var lastGameRef = loadHistory.Last();

            var lastURI = WorkspacePath.Parse(lastGameRef.Key);

            var metaData = lastGameRef.Value;

            // Merge values from the active game
            foreach (var entry in ActiveEngine.MetaData)
                if (metaData.ContainsKey(entry.Key))
                    metaData[entry.Key] = entry.Value;
                else
                    metaData.Add(entry.Key, entry.Value);

            // Clear the load history if it is loading the first item
            if (loadHistory.Count == 1)
                // This insures you can't quit the first game that is loaded.
                loadHistory.Clear();

            // Make sure this is still a valid path before we try to load it
            if (workspaceService.Exists(lastURI) && workspaceService.ValidateGameInDir(lastURI))
            {
                if (metaData != null)
                {
                    if (metaData.ContainsKey("reset"))
                        metaData["reset"] = "true";
                    else
                        metaData.Add("reset", "true");
                }

                // Reload the game
                Load(lastURI.Path, RunnerMode.Loading, metaData);

                return;
            }

            DisplayError(ErrorCode.LoadError, new Dictionary<string, string> {{"@{path}", lastURI.Path}});
        }

        public virtual void Back(Dictionary<string, string> metaData = null)
        {
            if (loadHistory.Count > 0)
                try
                {
                    // Remvoe the last game that was running from the history
                    loadHistory.RemoveAt(loadHistory.Count - 1);

                    // Get the previous game
                    var lastGameRef = loadHistory.Last();

                    // Copy the new meta data over to the last game ref before passing in
                    if (metaData != null)
                        foreach (var key in metaData.Keys)
                            if (lastGameRef.Value.ContainsKey(key))
                                lastGameRef.Value[key] = metaData[key];
                            else
                                lastGameRef.Value.Add(key, metaData[key]);

                    // Remove that game from history since we are about to load it
                    loadHistory.RemoveAt(loadHistory.Count - 1);

                    // Load the last game
                    Load(lastGameRef.Key, RunnerMode.Loading, lastGameRef.Value);

                    return;
                }
                catch
                {
                    // ignored
                }

            // Make sure all disks are ejected
            workspaceServicePlus.EjectAll();

            AutoLoadDefaultGame();
        }

        public void ShutdownSystem()
        {
            // We only want to call this once so don't run if shutdown is true
            if (shutdown) return;

            // Toggle the shutdown flag
            shutdown = true;

            UpdateDiskInBios();

            // Shutdown the active game
            ShutdownActiveEngine();

            SaveBiosChanges();

            // Save any changes to the bios to the user's custom bios file
            workspaceService.ShutdownSystem();
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

            if (server?.GetDebugger() is AsyncDebugger debugger)
                ((MoonSharpDebugSession) debugger.Client)?.SendText(message);
        }

        public void UpdateDiskInBios()
        {
            var total = workspaceServicePlus.MaxDisks;

            var disks = workspaceServicePlus.Disks;
            var totalDisks = disks.Length;

            for (var i = 0; i < total; i++)
                bios.UpdateBiosData("Disk" + i,
                    i < totalDisks ? workspaceServicePlus.DiskPhysicalRoot(disks[i]) : "none");
        }

        public void StartRecording()
        {
            if (!Recording)
            {
                Recording = true;

                // if (workspaceService.Exists(tmpGifPath)) workspaceServicePlus.Delete(tmpGifPath);

                var gifDirectory = WorkspacePath.Root.AppendDirectory("Workspace")
                    .AppendDirectory("Recordings");

                // Create the directory if it doesn't exist
                if (!workspaceService.Exists(gifDirectory))
                    workspaceServicePlus.CreateDirectoryRecursive(gifDirectory);

                // Get the path to the new file
                var destPath = workspaceService.UniqueFilePath(gifDirectory.AppendFile("recording.gif"));

                gifEncoder = new GifExporter(destPath.Path, ActiveEngine);

                UpdateTitle();
            }
        }

        public virtual void UpdateTitle()
        {
            Window.Title = DefaultWindowTitle + (server != null && server.Connected ? " 🐞" : "") +
                           (Recording ? " ⚫" : "") + (screenShotActive ? " 📷" : "");
        }

        public void StopRecording()
        {
            if (!Recording || gifEncoder == null) return;

            Recording = false;

            ExportService.Clear();
            ExportService.AddExporter(gifEncoder);
            ExportService.StartExport();

            // Change the title
            UpdateTitle();
        }

        private delegate void QuitCurrentToolDelagator(Dictionary<string, string> metaData, string tool = null);

        #region Runner settings

        /// <summary>
        ///     Override Volume so it saves changes to the bios.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public override int Volume(int? value = null)
        {
            var vol = base.Volume(value);

            // only save if not muted
            if (Mute() == false)
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

            // Only save if the value has changed
            if (value != null)
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
                bios.UpdateBiosData(CRTSettings.CRT.ToString(), toggle.Value.ToString());
                InvalidateResolution();
            }

            return displayTarget.useCRT;
        }

        public float Brightness(float? brightness = null)
        {
            if (brightness.HasValue)
            {
                displayTarget.brightness = brightness.Value;
                bios.UpdateBiosData(CRTSettings.Brightness.ToString(), (brightness * 100).ToString());
            }

            return displayTarget.brightness;
        }

        public float Sharpness(float? sharpness = null)
        {
            if (sharpness.HasValue)
            {
                displayTarget.sharpness = sharpness.Value;
                bios.UpdateBiosData(CRTSettings.Sharpness.ToString(), sharpness.ToString());
            }

            return displayTarget.sharpness;
        }

        #endregion
    }
}