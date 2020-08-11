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
using Microsoft.Xna.Framework.Input;
using MoonSharp.Interpreter;
using PixelVision8.Engine;
using PixelVision8.Engine.Chips;
using PixelVision8.Runner.Services;
using PixelVision8.Runner.Workspace;

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
    ///     This is the main type for your game.
    /// </summary>
    public class PixelVision8Runner : DesktopRunner
    {
        public readonly Dictionary<ActionKeys, Keys> actionKeys = new Dictionary<ActionKeys, Keys>
        {
            // TODO need to add the other action keys here so they are mapped in the bios
            {ActionKeys.BackKey, Keys.Escape},
            {ActionKeys.ScreenShotKey, Keys.D2},
            {ActionKeys.RecordKey, Keys.D3},
            {ActionKeys.RestartKey, Keys.D4}
        };

        // private readonly List<GifExporter> gifEncoders = new List<GifExporter>();
        //
        // private readonly WorkspacePath tmpGifPath =
        //     WorkspacePath.Root.AppendDirectory("Tmp").AppendFile("tmp-recording.gif");

        public bool autoRunEnabled = true;
        public bool backKeyEnabled = true;
        protected IControllerChip controllerChip;
        private string documentsPath;
        private bool ejectingDisk;

        protected GifExporter gifEncoder;

        
        protected bool screenShotActive;

        protected float screenshotDelay = 200f;

//        private MergedFileSystem osFileSystem;
        private ScreenshotService screenshotService;
        protected float screenshotTime;

//        protected string rootPath;
        protected bool shutdown;
        

        private string windowTitle;

        protected WorkspaceServicePlus workspaceServicePlus;

        // Default path to where PV8 workspaces will go
        public PixelVision8Runner(string rootPath) : base(rootPath)
        {
        }

        public PixelVision8Runner()
        {
//            throw new NotImplementedException();
        }

//        public List<string> loadHistory = new List<string>();
//        protected List<Dictionary<string, string>> metaDataHistory = new List<Dictionary<string, string>>();
        protected string Documents => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        public ExportService ExportService { get; private set; }
        public bool recording { get; set; }

        protected override void ConfigureRunner()
        {
            base.ConfigureRunner();
            
            screenshotService = new ScreenshotService(workspaceServicePlus);
            ExportService = new ExportService(); //TODO Need to create a new AudioClipAdaptor

            autoShutdown = bios.ReadBiosData("AutoShutdown", "True") == "True";

            RefreshActionKeys();

            // Replace title with version

            windowTitle = bios.ReadBiosData(BiosSettings.SystemName.ToString(), "Pixel Vision 8 Runner") + " " +
                          bios.ReadBiosData(BiosSettings.SystemVersion.ToString(), "0.0.0");

            // Offset the title by 2 for the record icon
            Window.Title = windowTitle;
        }

        public void RefreshActionKeys()
        {
            var actions = Enum.GetValues(typeof(ActionKeys)).Cast<ActionKeys>();
            foreach (var action in actions)
                actionKeys[action] =
                    (Keys) Convert.ToInt32(bios.ReadBiosData(action.ToString(), ((int) actionKeys[action]).ToString(),
                        true));
        }

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
                bios.ReadBiosData("ArchiveExtensions", "zip,pv8,pvt,pvs,pva,pvr").Split(',')
                    .ToList();

            //  Define the valid file extensions from the bios
            workspaceServicePlus.fileExtensions =
                bios.ReadBiosData("FileExtensions", "png,lua,json,txt,wav").Split(',')
                    .ToList();

            // Define the files required to make a game valid from the bios
            workspaceServicePlus.requiredFiles =
                bios.ReadBiosData("RequiredFiles", "data.json,info.json").Split(',')
                    .ToList();

            // Include any library files in the OS mount point
            workspaceServicePlus.osLibPath = WorkspacePath.Root.AppendDirectory("PixelVisionOS")
                .AppendDirectory(bios.ReadBiosData("LibsDir", "Libs"));


            // Look for the workspace name
            var workspaceName = bios.ReadBiosData("WorkspaceDir", "Workspace");

            // PV8 Needs to access the documents folder so it can create the workspace drive
            var baseDir = bios.ReadBiosData("BaseDir", "PixelVision8");

            // Set the TotalDisks disks
            workspaceServicePlus.MaxDisks = int.Parse(bios.ReadBiosData("MaxDisks", "2"));

            // Create the real system path to the documents folder
            documentsPath = Path.Combine(Documents, baseDir);

            if (Directory.Exists(documentsPath) == false) Directory.CreateDirectory(documentsPath);

            // Create a new physical file system mount
            workspaceServicePlus.AddMount(new KeyValuePair<WorkspacePath, IFileSystem>(
                WorkspacePath.Root.AppendDirectory("User"),
                new PhysicalFileSystem(documentsPath)));

            // Mount the workspace drive    
            workspaceServicePlus.MountWorkspace(workspaceName);

            workspaceServicePlus.RebuildWorkspace();

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

            // Inject the PV8 runner special global function
            luaScript.Globals["IsExporting"] = new Func<bool>(ExportService.IsExporting);
            luaScript.Globals["ReadExportPercent"] = new Func<int>(ExportService.ReadExportPercent);
            luaScript.Globals["ReadExportMessage"] = new Func<string>(ExportService.ReadExportMessage);
            luaScript.Globals["ShutdownSystem"] = new Action(ShutdownSystem);
            luaScript.Globals["QuitCurrentTool"] = (QuitCurrentToolDelagator) QuitCurrentTool;
            luaScript.Globals["RefreshActionKeys"] = new Action(RefreshActionKeys);
            luaScript.Globals["DocumentPath"] = new Func<string>(() => documentsPath);
            luaScript.Globals["TmpPath"] = new Func<string>(() => tmpPath);
            luaScript.Globals["DiskPaths"] = new Func<WorkspacePath[]>(() => workspaceServicePlus.Disks);
            luaScript.Globals["SaveActiveDisks"] = new Action(() =>
            {
                var disks = workspaceServicePlus.Disks;

                foreach (var disk in disks) workspaceServicePlus.SaveDisk(disk);
            });
            luaScript.Globals["EjectDisk"] = new Action<string>(EjectDisk);
            luaScript.Globals["EnableAutoRun"] = new Action<bool>(EnableAutoRun);
            luaScript.Globals["EnableBackKey"] = new Action<bool>(EnableBackKey);
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

            // Activate the game
            base.ActivateEngine(engine);

            // Force the lua script to use this boot done logic instead
            luaScript.Globals["BootDone"] = new Action<bool>(BootDone);
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
            if (activeEngine == null || shutdown)
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

            // Save any gifs that have been encoded
            // if (gifEncoders.Count > 0)
            //     for (var i = gifEncoders.Count - 1; i >= 0; i--)
            //     {
            //         var encoder = gifEncoders[i];
            //
            //         // Do processing here, then...
            //         if (encoder.ExportingFinished)
            //         {
            //             // Get the gif directory
            //             var gifDirectory = WorkspacePath.Root.AppendDirectory("Workspace")
            //                 .AppendDirectory("Recordings");
            //
            //             // Create the directory if it doesn't exist
            //             if (!workspaceService.Exists(gifDirectory))
            //                 workspaceServicePlus.CreateDirectoryRecursive(gifDirectory);
            //
            //             // Get the path to the new file
            //             var path = workspaceService.UniqueFilePath(gifDirectory.AppendFile("recording.gif"));
            //
            //             // Create a new file in the workspace
            //             var fileStream = workspaceService.CreateFile(path);
            //
            //             // Get the memory stream from the gif encoder
            //             var memoryStream = new MemoryStream(encoder.bytes); 
            //             
            //             memoryStream.Position = 0;
            //             memoryStream.CopyTo(fileStream);
            //
            //             // Close the file stream
            //             fileStream.Close();
            //             fileStream.Dispose();
            //
            //             // Close the memory stream
            //             memoryStream.Close();
            //             memoryStream.Dispose();
            //
            //             // Remove the encoder
            //             gifEncoders.RemoveAt(i);
            //         }
            //     }

            if (controllerChip.GetKeyDown(Keys.LeftControl) ||
                controllerChip.GetKeyDown(Keys.LeftControl))
            {
                if (controllerChip.GetKeyUp(actionKeys[ActionKeys.ScreenShotKey]))
                {
                    // Only take a screenshot when one isn't being saved
                    if (!screenShotActive)
                        //                            Console.WriteLine("Take Picture");

                        screenShotActive = screenshotService.TakeScreenshot(activeEngine);
                }
                else if (controllerChip.GetKeyUp(actionKeys[ActionKeys.RecordKey]))
                {
                    if (recording)
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
            else
            {
                if (controllerChip.GetKeyUp(Keys.D1))
                    ToggleLayers(2);
                else if (controllerChip.GetKeyUp(Keys.D2))
                    ToggleLayers(3);
                else if (controllerChip.GetKeyUp(Keys.D3))
                    ToggleLayers(4);
                else if (controllerChip.GetKeyUp(Keys.D4))
                    ToggleLayers(5);
                else if (controllerChip.GetKeyUp(Keys.D5))
                    ToggleLayers(6);
                else if (controllerChip.GetKeyUp(Keys.D6))
                    ToggleLayers(7);
                else if (controllerChip.GetKeyUp(Keys.D7)) ToggleLayers(Enum.GetNames(typeof(DrawMode)).Length);
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

                    if (recording) gifEncoder.AddFrame(timeDelta/1000f);
                }

            }
            catch (Exception e)
            {
                DisplayError(ErrorCode.Exception,
                    new Dictionary<string, string>
                        {{"@{error}", e is ScriptRuntimeException error ? error.DecoratedMessage : e.Message}}, e);
            }
        }

        // Custom method to use instaed of the default BootDone
        public override void BootDone(bool safeMode = false)
        {
            // Only call BootDone when the runner is booting.
            if (mode != RunnerMode.Booting)
                return;

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
            if (bios.ReadBiosData("FileDiskMounting", "True") != "False")
            {
                // Setup Drag and drop support
                // Window.FileDropped += (o, e) => OnFileDropped(o, e);

                // Disable auto run when loading up the default disks
                autoRunEnabled = false;

                for (var i = 0; i < workspaceServicePlus.MaxDisks; i++)
                {
                    var diskPath = bios.ReadBiosData("Disk" + i, "none");
                    if (diskPath != "none" && diskPath != "")
                        // manually mount each disk since we are not going to load from them
                        workspaceServicePlus.MountDisk(diskPath);
                }
            }

            AutoLoadDefaultGame();
            
        }

        public override void DisplayError(ErrorCode code, Dictionary<string, string> tokens = null, Exception exception = null)
        {

            if (mode == RunnerMode.Error)
                return;

            // TODO should this only work on special cases?
            autoRunEnabled = true;

            base.DisplayError(code, tokens, exception);
        }

        // TODO need to make sure this still works correctly
        public override void AutoLoadDefaultGame()
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

            return;
        }

        public void MountDisk(string path)
        {
//            Console.WriteLine("Load File - " + path + " Auto Run " + autoRunEnabled);
            try
            {
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
                    // Need to force the disk animation to show
                    var lastGameRef = loadHistory.Last();

                    var metaData = lastGameRef.Value;

                    if (metaData.ContainsKey("showDiskAnimation"))
                        metaData["showDiskAnimation"] = "true";
                    else
                        metaData.Add("showDiskAnimation", "true");

                    // TODO sometimes we don't wan to do this
                    ResetGame();
                }
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

            if (diskPath != null)
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


        public override void RunGame()
        {
            // TODO This should be moved into the desktop runner?
            autoRunEnabled = true;

            // Re-enable back when loading a new game
            backKeyEnabled = true;

            base.RunGame();
        }

        public virtual bool Load(string path, RunnerMode newMode = RunnerMode.Playing,
            Dictionary<string, string> metaData = null)
        {

            // Make sure we stop recording when loading a new game
            if (recording) StopRecording();
            
            if (newMode == RunnerMode.Loading)
            {
                // Create metadata if it doesn't exists so we can store the eject value for the loader
                if (metaData == null)
                {
                    metaData = new Dictionary<string, string>();
                }

                // Tell the loader to show eject animation
                if (metaData.ContainsKey("showEjectAnimation"))
                    metaData["showEjectAnimation"] = ejectingDisk.ToString().ToLower();
                else
                    metaData.Add("showEjectAnimation", ejectingDisk.ToString().ToLower());

            }

            return base.Load(path, newMode, metaData);

        }

        public override void SaveGameData(string path, IEngine engine, SaveFlags saveFlags, bool useSteps = true)
        {
            // Export the current game

            // TODO exporter needs a callback when its completed
            ExportService.ExportGame(path, engine, saveFlags);

            // TODO this should be moved into the ExportGame class
            ExportService.StartExport(useSteps);
        }

        public override void ConfigureServices()
        {
            base.ConfigureServices();

            var luaService = new LuaServicePlus(this);

            // Register Lua Service
            tmpEngine.AddService(typeof(LuaService).FullName, luaService);

            tmpEngine.AddService(typeof(ExportService).FullName, ExportService);
        }

        public void OnFileDropped(object gameWindow, string path)
        {
            if (shutdown == false)
                MountDisk(path);

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

            // Merge values from the active game
            foreach (var entry in activeEngine.metaData)
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

                if (metaData != null)
                {
                    if (metaData.ContainsKey("showDiskAnimation"))
                        metaData["showDiskAnimation"] = "false";
                    else
                        metaData.Add("showDiskAnimation", "false");
                }

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

                    // Clear the disk animation
                    if (lastGameRef.Value.ContainsKey("showDiskAnimation"))
                        lastGameRef.Value["showDiskAnimation"] = "false";

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

        public override void ShutdownSystem()
        {
            // We only want to call this once so don't run if shutdown is true
            if (shutdown)
                return;

            // Toggle the shutdown flag
            shutdown = true;

            UpdateDiskInBios();

            base.ShutdownSystem();
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
            if (!recording)
            {
                recording = true;

                // if (workspaceService.Exists(tmpGifPath)) workspaceServicePlus.Delete(tmpGifPath);

                var gifDirectory = WorkspacePath.Root.AppendDirectory("Workspace")
                    .AppendDirectory("Recordings");

                // Create the directory if it doesn't exist
                if (!workspaceService.Exists(gifDirectory))
                    workspaceServicePlus.CreateDirectoryRecursive(gifDirectory);

                // Get the path to the new file
                var destPath = workspaceService.UniqueFilePath(gifDirectory.AppendFile("recording.gif"));

                gifEncoder = new GifExporter(destPath.Path, activeEngine);

                // gifEncoder.Start();
                // gifEncoder = new AnimatedGifEncoder();
                // gifEncoder.SetDelay(1000 / 60);
                //
                // gifEncoder.CreatePalette(activeEngine.displayChip, activeEngine.colorChip);

                Window.Title = windowTitle + " (REC)";
            }
        }

        public void StopRecording()
        {
            if (!recording || gifEncoder == null)
                return;

            recording = false;

            ExportService.Reset();
            ExportService.AddExporter(gifEncoder);
            ExportService.StartExport();
            
            // gifEncoder.Finish();

            // Add the encoder to the list to watch for exporting
            // gifEncoders.Add(gifEncoder);
            //
            // // Clear the current encoder
            // gifEncoder = null;

            // Change the title
            Window.Title = windowTitle;
        }

        private delegate void QuitCurrentToolDelagator(Dictionary<string, string> metaData, string tool = null);
    }
}