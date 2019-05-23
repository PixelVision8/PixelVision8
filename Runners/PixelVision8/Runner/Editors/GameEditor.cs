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
using PixelVision8.Engine;
using PixelVision8.Engine.Chips;
using PixelVision8.Engine.Services;
using PixelVision8.Engine.Utils;
using PixelVision8.Runner.Chips;
using PixelVision8.Runner.Chips.Sfxr;
using PixelVision8.Runner.Exporters;
using PixelVision8.Runner.Importers;
using PixelVision8.Runner.Parsers;
using PixelVision8.Runner.Services;
using PixelVision8.Runner.Workspace;

namespace PixelVision8.Runner.Editors
{
    /// <summary>
    ///     This class allows you to edit the current sandbox game.
    /// </summary>
//    [MoonSharpUserData]
    public class GameEditor
    {
        protected bool _invalid;

        public ColorChip activeColorChip;

//        private GCControllerChip controllerChip;
        private ColorChip colorChip;

//        private ColorMapChip colorMapChip; // TODO this should have a GC version of the chip
        private DisplayChip displayChip;

//
//        public IEngine targetGame;
//
        private GameChip gameChip;
        private SfxrMusicGeneratorChip musicChip;
        protected PixelVision8Runner runner;
        private SfxrSoundChip soundChip;
        private SpriteChip spriteChip;
        private PixelVisionEngine targetGame;
        private TilemapChip tilemapChip;

        protected WorkspaceServicePlus workspace;

        protected IServiceLocator serviceManager;
        
//        private ColorChip colorPaletteChip;

        /// <summary>
        ///     Creates a new Game Editor instance and loads the game's system and meta data
        /// </summary>
        /// <param name="workspace"></param>
        /// <param name="luaService"></param>
//        [MoonSharpHidden]
        public GameEditor(PixelVision8Runner runner, IServiceLocator serviceManager)
        {
            this.runner = runner;
            this.serviceManager = serviceManager;
            // Get a reference to the workspace from the runner instance
            workspace = runner.workspaceService as WorkspaceServicePlus;
        }

        public virtual List<string> defaultChips
        {
            get
            {
                var chips = new List<string>
                {
                    typeof(ColorChip).FullName,
                    typeof(SpriteChip).FullName,
                    typeof(TilemapChip).FullName,
                    typeof(FontChip).FullName,
                    typeof(ControllerChip).FullName,
                    typeof(DisplayChip).FullName,
                    typeof(SfxrMusicGeneratorChip).FullName, // TODO this is a custom chip just for the editor
                    typeof(SfxrSoundChip).FullName,
                    typeof(LuaGameChip).FullName
                };

                return chips;
            }
        }

        public bool invalid => _invalid;

        //        [MoonSharpHidden]
        private SaveFlags BuildSaveFlags(SaveFlags[] flags)
        {
            // Since Lua doesn't know how to handle bit flags, we need to do the conversion on the C# side of things
            var saveFlags = SaveFlags.System;

            for (var i = 0; i < flags.Length; i++)
                if (flags[i] != SaveFlags.System)
                    saveFlags |= flags[i];

            return saveFlags;
        }

        /// <summary>
        ///     This allows you to load files inside of the Workspace/Sandbox/ directory into the Game Editor. It
        ///     accepts an array of SaveFlags enums which define what game files to load. Use this method to load only
        ///     what you need to edit to speed up parsing game data.
        /// </summary>
        /// <param name="flags">Supply an array of SaveFlags enums for each file you want to load into the Game Editor instance.</param>
        /// <returns>Returns a bool if the loading process was successful.</returns>
        public bool Load(string path, SaveFlags[] flags)
        {
//            // If the path is not valid, return null
//            if (path == null)
//            {
//                return false;
//            }
//            
            // TODO this is hard coded to the disks directory and should be from the bios or somewhere else?
            // Convert to a system path
            var filePath = WorkspacePath.Parse(path);

            // If the file doesn't exist, return false.
            if (!workspace.Exists(filePath))
                return false;

            var files = workspace.ConvertDiskFilesToBytes(workspace.ReadDisk(filePath));

            var saveFlags = BuildSaveFlags(flags);

            try
            {
                targetGame = new PixelVisionEngine(serviceManager, defaultChips.ToArray());

                runner.ParseFiles(files, targetGame, saveFlags);
            }
            catch
            {
//                Console.WriteLine("Game Editor Load Error:\n"+e.Message);

                return false;
            }


            Reset();


            // TODO this needs to be tied into the preload system and not imediatly loaded
//            var success = false;//workspace.ReadGameFromWorkspace(targetGame, saveFlags, true);

            return true;
        }

        public void Reset()
        {
//            targetGame = new PixelVisionEngine(null, null, runner.defaultChips.ToArray());

            // Configure the game editor now that all the chips have been loaded
            gameChip = targetGame.gameChip;

            // Since the game is not attached to a runner it will throw an error when trying to load lua serivce
            try
            {
                // Configure the game chip
                gameChip.Reset();
            }
            catch
            {
//                Console.WriteLine("Game Editor Reset Error:\n"+e.Message);
                // Do nothing with any missing service error
            }

//
//            // Get references to all of the chips
//            controllerChip = targetGame.controllerChip as GCControllerChip;
            colorChip = targetGame.colorChip;
//            colorMapChip = targetGame.chipManager.GetChip(ColorMapParser.chipName, false) as ColorChip;
//
//            colorMapChip = targetGame.colorMapChip;
            displayChip = targetGame.displayChip;
            spriteChip = targetGame.spriteChip;
            tilemapChip = targetGame.tilemapChip;
            soundChip = targetGame.soundChip as SfxrSoundChip;
            musicChip = targetGame.musicChip as SfxrMusicGeneratorChip; // TODO need to create a SfxrMusicChip
//            
//            colorPaletteChip = targetGame.chipManager.GetChip(ColorPaletteParser.chipName, false) as ColorChip;

            ChangeColorMode();
        }

        public string[] LibraryPaths()
        {
            var files = new Dictionary<string, byte[]>();

            // TODO need to go through and find all of the included libraries from the workspace

            workspace.IncludeLibDirectoryFiles(files);

            var fileList = new List<string>();

            // TODO need to get the real paths

            foreach (var file in files) fileList.Add(file.Key);

            return fileList.ToArray();
        }

//
//        /// <summary>
//        ///     Returns true if an engine is loaded
//        /// </summary>
//        public bool gameLoaded
//        {
//            get { return targetGame != null; }
//        }
//

//
        public void Invalidate()
        {
            _invalid = true;
        }

        public void ResetValidation()
        {
            _invalid = false;
        }


//
////        [MoonSharpHidden]
////        private ArchiveFlags BuildArchiveFlags(ArchiveFlags[] flags)
////        {
////            // Since Lua doesn't know how to handle bit flags, we need to do the conversion on the C# side of things
////            var archiveFlags = ArchiveFlags.All;
////
////            for (int i = 0; i < flags.Length; i++)
////            {
////                if (flags[i] != ArchiveFlags.All)
////                    archiveFlags |= flags[i];
////            }
////
////            return archiveFlags;
////        }
//

//
//        // TODO this is not part of the GameChip API?
////        public Vector RealTilemapSize()
////        {
////            return new Vector(gameChip.realWidth, gameChip.realHeight);
////        }
//            
//        public void AddScript(string name, string code)
//        {
//            var gameChip = this.gameChip as LuaGameChip;
//
//            if (gameChip != null)
//            {
//                gameChip.AddScript(name, code);
//            }
//
//        }
//

//
        /// <summary>
        /// </summary>
        /// <param name="flags"></param>
        public void Save(string path, SaveFlags[] flags)
        {
            // TODO need to get the export service


            // TODO should only save flags that are supplied
            var saveFlags = SaveFlags.None;

            for (var i = 0; i < flags.Length; i++) saveFlags |= flags[i];

            targetGame.SetMetaData("version", runner.systemVersion);
//            gameChip.version = ;

            // TODO saving games doesn't work
            runner.SaveGameData(path, targetGame, saveFlags);
//            workspace.SaveCart(workspace.WorkspacePath(WorkspaceFolders.CurrentGameDir), targetGame, saveFlags);

            ResetValidation();
        }

//
        /// <summary>
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public int GameMaxSize(int? size = null)
        {
            if (size.HasValue)
            {
                gameChip.maxSize = size.Value;
                Invalidate();
            }

            return gameChip.maxSize;
        }

        public int GameSaveSlots(int? total = null)
        {
            if (total.HasValue)
            {
                gameChip.saveSlots = total.Value;
                Invalidate();
            }

            return gameChip.saveSlots;
        }

        /// <summary>
        /// </summary>
        /// <param name="isLocked"></param>
        /// <returns></returns>
        public bool GameSpecsLocked(bool? isLocked = null)
        {
            if (isLocked.HasValue)
            {
                gameChip.lockSpecs = isLocked.Value;
                Invalidate();
            }

            return gameChip.lockSpecs;
        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <returns></returns>
//        public bool ValidateSize()
//        {
////            var size = workspace.CalculateProjectSize() / 1024;
////
//            // TODO need to figure out a better way to validate the project size
//            return true; //size <= GameMaxSize();
//        }
//
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="safeDelete"></param>
//        /// <returns></returns>
//        public string Archive(bool safeDelete = true)
//        {
//            var newFileName = workspace.ArchiveCurrentGame(safeDelete);
//
//            return newFileName;
//        }
//
        /// <summary>
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string Name(string name = null)
        {
            // If a new name is supplied, set it on the game chip
            if (name != null)
            {
                targetGame.GetMetaData("name", name);
                Invalidate();
            }


            // Return the latest name value from the gameChip
            return targetGame.GetMetaData("name", GetType().Name);
        }

        /// <summary>
        ///     This is read only, shouldn't be allowed to manually change a game's version since that only happens when saving
        /// </summary>
        /// <returns></returns>
        public string Version()
        {
            return targetGame.GetMetaData("version", runner.systemVersion);
        }

        /// <summary>
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string Ext(string value = null)
        {
            if (value != null)
            {
                targetGame.SetMetaData("ext", value);
                Invalidate();
            }

            return targetGame.GetMetaData("ext", ".pv8");
        }

        public int BackgroundColor(int? id = null)
        {
            return gameChip.BackgroundColor(id);
        }

        public string BackgroundColorHex()
        {
            return colorChip.ReadColorAt(BackgroundColor());
        }

        public string Color(int id, string value = null)
        {
            if (value == null)
                return activeColorChip.ReadColorAt(id);

            activeColorChip.UpdateColorAt(id, value);

            return value;
        }

        public void ClearColors()
        {
            colorChip.Clear();
        }

        public int TotalColors(bool ignoreEmpty = false, int? total = null)
        {
            if (total.HasValue)
                activeColorChip.total = total.Value;

            return ignoreEmpty ? activeColorChip.totalUsedColors : activeColorChip.total;
        }

        // Since we want to be able to edit this value but the interface doesn't allow it, we hide it in lua and use the overload instead
//        [MoonSharpHidden]
        public int ColorsPerSprite()
        {
            return gameChip.ColorsPerSprite();
        }

        public int MaxSpriteCount(int? total)
        {
            if (total.HasValue) gameChip.MaxSpriteCount(total.Value);

            return gameChip.MaxSpriteCount();
        }

        /// <summary>
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public Point DisplaySize(int? width = null, int? height = null)
        {
            var size = new Point();
            var resize = false;

            if (width.HasValue)
            {
                size.X = width.Value;
                resize = true;
            }
            else
            {
                size.X = displayChip.width;
            }

            if (height.HasValue)
            {
                size.Y = height.Value;
                resize = true;
            }
            else
            {
                size.Y = displayChip.height;
            }

            if (resize) displayChip.ResetResolution(size.X, size.Y);

            // TODO need a flag to tell the runner to change the resolution

            return new Point(displayChip.width, displayChip.height);
        }

        /// <summary>
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public Point OverscanBorder(int? x, int? y)
        {
            if (x.HasValue) displayChip.overscanX = x.Value;

            if (y.HasValue) displayChip.overscanY = y.Value;

            return new Point(displayChip.overscanX, displayChip.overscanY);
        }


        public void RedrawDisplay()
        {
            throw new NotImplementedException();
        }

        public Point ScrollPosition(int? x = null, int? y = null)
        {
            return gameChip.ScrollPosition(x, y);
        }

        public void WriteSaveData(string key, string value)
        {
            gameChip.WriteSaveData(key, value);
        }

        public string ReadSaveData(string key, string defaultValue = "undefined")
        {
            return gameChip.ReadSaveData(key, defaultValue);
        }

        public bool Key(Keys key, InputState state = InputState.Down)
        {
            throw new NotImplementedException();
        }

        public bool MouseButton(int button, InputState state = InputState.Down)
        {
            throw new NotImplementedException();
        }

        public bool Button(Buttons button, InputState state = InputState.Down, int controllerID = 0)
        {
            throw new NotImplementedException();
        }

        public Point MousePosition()
        {
            throw new NotImplementedException();
        }

        public string InputString()
        {
            throw new NotImplementedException();
        }


        public int TotalSongs(int? total = null)
        {
            if (total.HasValue)
                musicChip.totalSongs = total.Value;

            return musicChip.totalSongs;
        }

        public void PlaySong(int id, bool loop = false, int startAt = 0)
        {
            gameChip.PlaySong(id, loop, startAt);
        }

        public void PlayPattern(int id, bool loop = true)
        {
            gameChip.PlayPattern(id, true);
        }

        public string SongName(int id, string value = null)
        {
            if (value != null) musicChip.songs[id].name = value;

            return musicChip.songs[id].name;
        }

        public int SongStart(int id, int? pos = null)
        {
            if (pos.HasValue) musicChip.songs[id].start = pos.Value;

            return musicChip.songs[id].start;
        }

        public int[] SongPatterns(int id, int[] patterns = null)
        {

            if (patterns != null)
            {

                var newPatterns = new int[patterns.Length];
                
                Array.Copy(patterns, newPatterns, patterns.Length);
                
                musicChip.songs[id].patterns = newPatterns;
            }
            
            return musicChip.songs[id].patterns;
        }

        public int TotalSongs()
        {
            return musicChip.totalSongs;
        }

        public int SongPatternAt(int id, int position, int? newPattern)
        {

            if (newPattern.HasValue)
            {
                musicChip.songs[id].UpdatePatternAt(position, newPattern.Value);
            }

            return musicChip.songs[id].patterns[position];
        }

        public int SongEnd(int id, int? pos = null)
        {
            if (pos.HasValue) musicChip.songs[id].end = pos.Value;

            return musicChip.songs[id].end;
        }

        public void PauseSong()
        {
            gameChip.PauseSong();
        }

        public void StopSong()
        {
            gameChip.StopSong();
        }

        public void RewindSong(int position = 0, int loopID = 0)
        {
            gameChip.RewindSong();
        }

        public Point SpriteSize(int? width, int? height)
        {
            return gameChip.SpriteSize(width, height);
        }

        public int[] Sprite(int id, int[] data = null)
        {
            return gameChip.Sprite(id, data);
        }

        public int[] FlipPixelData(int[] data, bool flipH = false, bool flipV = false, int width = 8, int height = 8)
        {
            SpriteChipUtil.FlipSpriteData(ref data, width, height, flipH, flipV);

            return data;
        }

        public int[] Sprites(int[] ids, int width)
        {
            return gameChip.Sprites(ids, width);
        }

        public int TotalSprites(bool ignoreEmpty = true)
        {
            return gameChip.TotalSprites(ignoreEmpty);
        }

        public int Flag(int column, int row, int? value = null)
        {
            return gameChip.Flag(column, row, value);
        }

        public TileData Tile(int column, int row, int? spriteID = null, int? colorOffset = null,
            int? flag = null)
        {
            var tileData = gameChip.Tile(column, row, spriteID, colorOffset, flag);

            RenderTile(tileData, column, row);

            return tileData;
        }

        public void RebuildTilemap()
        {
            gameChip.RebuildTilemap();
        }

        public Point TilemapSize(int? width = null, int? height = null)
        {
            return gameChip.TilemapSize(width, height);
        }

        public void UpdateTiles(int column, int row, int columns, int[] ids, int? colorOffset = null, int? flag = null)
        {
            gameChip.UpdateTiles(column, row, columns, ids, colorOffset, flag);
        }

        public int[] ConvertTextToSprites(string text, string fontName = "default")
        {
            throw new NotImplementedException();
        }

        public int[] ConvertCharacterToPixelData(char character, string fontName)
        {
            throw new NotImplementedException();
        }


        #region Sound Editor APIs

        public string Sound(int id, string data = null)
        {
            return gameChip.Sound(id, data);
        }

        #endregion

//        
//        #region APIs not availible to the Game Editor
//
//        // TODO remove this from the Interface?
//        public void ReplaceColor(int index, int id)
//        {
//            throw new System.NotImplementedException();
//        }
//
//        // TODO remove this from the interface
//        public void Clear(int x = 0, int y = 0, int? width = null, int? height = null)
//        {
//            throw new System.NotImplementedException();
//        }
//
//        public Vector Display(bool visible = true)
//        {
//            return gameChip.Display(visible);
//        }
//
//        public Rect VisibleBounds()
//        {
//            throw new NotImplementedException();
//        }
//
//        // TODO route the drawing APIs back through the current chip and not the game chip
//        public void DrawPixels(int[] pixelData, int x, int y, int width, int height,
//            DrawMode drawMode = DrawMode.Sprite,
//            bool flipH = false, bool flipV = false, int colorOffset = 0)
//        {
//            throw new System.NotImplementedException();
//        }
//
//        public void DrawPixel(int x, int y, int colorRef)
//        {
//            throw new System.NotImplementedException();
//        }
//
//        public void DrawSprite(int id, int x, int y, bool flipH = false, bool flipV = false,
//            DrawMode drawMode = DrawMode.Sprite,
//            int colorOffset = 0)
//        {
//            throw new System.NotImplementedException();
//        }
//
//        public void DrawSprites(int[] ids, int x, int y, int width, bool flipH = false, bool flipV = false,
//            DrawMode drawMode = DrawMode.Sprite, int colorOffset = 0, bool onScreen = true, bool useScrollPos = true,
//            Rect bounds = null)
//        {
//            throw new System.NotImplementedException();
//        }
//
//        public void DrawText(string text, int x, int y, DrawMode drawMode = DrawMode.Sprite, string font = "Default",
//            int colorOffset = 0, int spacing = 0)
//        {
//            throw new System.NotImplementedException();
//        }
//
//        public void DrawTilemap(int x = 0, int y = 0, int columns = 0, int rows = 0, int? offsetX = null,
//            int? offsetY = null, DrawMode drawMode = DrawMode.Tile)
//        {
//            throw new System.NotImplementedException();
//        }
//
//        public void DrawRect(int x, int y, int width, int height, int color = -1,
//            DrawMode drawMode = DrawMode.Background)
//        {
//            throw new NotImplementedException();
//        }
//
//        #endregion
//
//
//
//        #region Builds
//
//        public bool CreateExportArchive()
//        {
//            
//            var path = workspace.WorkspacePath(WorkspaceFolders.TmpDir) + fullName;
//            
//            var files = workspace.FindValidGameFiles(workspace.WorkspacePath(WorkspaceFolders.CurrentGameDir));
//
//            // Create a zip
//            workspace.CreateZip(workspace.ConvertToSystemPath(path), files);
//                
////            var archiveFlags = BuildArchiveFlags(flags);
//
////            var name = fullName;
////
////            var src = workspace.WorkspacePath(WorkspaceFolders.CurrentGameDir);
////            var dest = workspace.WorkspacePath(WorkspaceFolders.TmpDir);
////
////            workspace.ArchiveDirectory(name, src, dest, "PixelVision8 Archive File - " + name);
//
////            var path = workspace.CreateArchive(name, workspace.WorkspacePath(WorkspaceFolders.TmpDir), archiveFlags);
//
//            return workspace.FileExists(path);
//
//        }
//
//        public string buildPath
//        {
//            get { return workspace.WorkspacePath(WorkspaceFolders.BuildsDir) + Name() + "/"; }
//        }
//
//        public string tmpArchivePath
//        {
//            get
//            {
//                var path = workspace.WorkspacePath(WorkspaceFolders.TmpDir) + fullName;
//
//                return path;
//            }
//        }
//
//        /// <summary>
//        /// 
//        /// </summary>
//        public void ClearBuilds()
//        {
//            if (workspace.DirectoryExists(buildPath))
//            {
//                workspace.DeleteDirectory(buildPath);
//            }
//        }
//
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="target"></param>
//        /// <returns></returns>
//        public bool Export(string target)
//        {
////            var gameName = workspace.ReadGameData("name", "Untitled");
//
//            if (!workspace.DirectoryExists(buildPath))
//            {
//                workspace.CreateDirectory(buildPath);
//            }
//
//            bool success;
//
//            if (!workspace.FileExists(tmpArchivePath))
//            {
//                success = CreateExportArchive();
//
//                if (!success)
//                {
//                    return false;
//                }
//
//            }
//
//            try
//            {
//                switch (target)
//                {
//                    case "Archive":
//                        workspace.CopyFile(tmpArchivePath, buildPath + fullName);
//                        success = true;
//                        break;
//                    default:
//
//                        success = CreateExport(target);
//
//                        break;
//                }
//            }
//            catch (Exception e)
//            {
////                Debug.Log("Export Error: " + e);
//                success = false;
//            }
//
////            if(success)
////                Debug.Log("Build '"+target+"' for " + gameName );
//
//            return success;
//        }
//
//        private bool CreateExport(string template)
//        {
//            
//            // TODO this needs to use the new logic to only include files that are critical for the game to run
//
//            for (int i = 0; i < runnerPaths.Length; i++)
//            {
//                var fullPath = workspace.ConvertToSystemPath(runnerPaths[i]);
//
//                if (workspace.DirectoryExists(fullPath))
//                {
//                    var tempDir = fullPath + template + "/";
//                    var destPath = buildPath + template + "/";
//                    if (workspace.DirectoryExists(tempDir))
//                    {
//                        workspace.CopyAll(tempDir, destPath);
//
//                        var copyDest = destPath;
//                        var fileName = fullName;
//
//                        var buildFile = destPath + "build.json";
//
//                        var exes = new string[0];
//
//                        if (workspace.FileExists(buildFile))
//                        {
//                            var buildJson =
//                                Json.Deserialize(workspace.ReadTextFromFile(destPath + "build.json")) as
//                                    Dictionary<string, object>;
//
//                            if (buildJson.ContainsKey("StreamingAssets"))
//                            {
//                                copyDest += buildJson["StreamingAssets"] as string;
//                            }
//
//                            if (buildJson.ContainsKey("defaultName"))
//                            {
//                                fileName = buildJson["defaultName"] as string;
//                            }
//
//                            if (buildJson.ContainsKey("executables"))
//                            {
//
//                                var values = buildJson["executables"] as List<object>;
//
//                                exes = values.ConvertAll(x => (string) x).ToArray();
//                            }
//
//                            workspace.FileDelete(buildFile);
//
//                        }
//
//                        // Create the path for the final file
//                        copyDest += fileName;
//
//                        // If the file exits, delete the old one
//                        if (workspace.FileExists(copyDest))
//                        {
//                            workspace.FileDelete(copyDest);
//                        }
//
//                        // Copy over the new archive
//                        workspace.CopyFile(buildPath + fullName, copyDest);
//
//                        if (exes != null)
//                        {
//                            foreach (var exe in exes)
//                            {
//
//                                // TODO need to get extension
//
//                                var exePath = destPath + exe;
//
//
////                                if (workspace.FileExists(exePath))
////                                {
//
//                                var ext = workspace.GetFileExtension(exePath);
//
////                                    Debug.Log("Rename " + exe + " " + ext);
//
//                                if (ext == ".app")
//                                {
//                                    workspace.MoveDirectory(exePath, destPath + Name() + ext);
//                                }
//                                else
//                                {
//                                    workspace.MoveFile(exePath, destPath + Name() + ext);
//
//                                }
////                                }
//
//                            }
//                        }
//
//
//                        return true;
//                    }
//
//                }
//
//            }
//
//            return false;
//        }
//
//        #endregion
//
//
//        private string fullName
//        {
//            get { return Name() + Ext(); }
//        }
//
//        //TODO this should be part of the bios. SHould be ordered in priority
//        // Find runner folders
//        string[] runnerPaths = new[]
//        {
//            "Workspace/Runners/",
//            "System/GameCreatorPro/Runners/"
//        };
//
//        public int TotalFlags(int? total)
//        {
//            if (total.HasValue)
//            {
//                tilemapChip.totalFlags = total.Value;
//            }
//
//            return tilemapChip.totalFlags;
//        }
//
        public void ExportSong(string path)
        {
            workspace.ExportSong(path, musicChip, soundChip);
        }

//
        public bool UniqueSprites(bool? flag = null)
        {
            if (flag.HasValue)
                spriteChip.unique = flag.Value;

            return spriteChip.unique;
        }

//        public bool UniqueColors(bool? flag = null)
//        {
//            if (flag.HasValue)
//                colorChip.unique = flag.Value;
//
//            return colorChip.unique;
//
//        }

        public string[] Colors()
        {
            return colorChip.hexColors;
        }

        public bool ImportTiles(bool? flag = null)
        {
            if (flag.HasValue)
                tilemapChip.autoImport = flag.Value;

            return tilemapChip.autoImport;
        }


        public bool DebugColor(bool? flag = null)
        {
            if (flag.HasValue)
                colorChip.debugMode = flag.Value;

            return colorChip.debugMode;
        }

//        public long CalculateProjectSize(bool compressed = true)
//        {
//
//            var files = workspace.FindValidGameFiles(workspace.WorkspacePath(WorkspaceFolders.CurrentGameDir));
//
//            var size = 0L;
//            
//            if (compressed)
//            {
//                // Figure out the tmp path for the directory
//                var path = workspace.WorkspacePath(WorkspaceFolders.TmpDir) + "TmpSizeArchive.zip";
//                
//                // Create a zip
//                workspace.CreateZip(workspace.ConvertToSystemPath(path), files);
//                
//                // Get the size of the zip
//                size = workspace.GetFileSize(path);
//                
//                // Delete the tmp zip
//                workspace.FileDelete(path);
//
//                // TODO need to zip up the files in the tmp directory to see the final size
//            }
//            else
//            {
//                
//                foreach (var file in files)
//                {
//                    size += workspace.GetFileSize(file);
//                }
//                
//            }
//            
//            
//            
//            return size;
//        }
//
//        public string ConvertSizeToString(long value)
//        {
//            return workspace.ConvertSizeToString(value);
//        }

        public string FlagColor(int id)
        {
            var flagColors = ((ColorChip) targetGame.GetChip(FlagColorParser.flagColorChipName, false)).hexColors;

            return flagColors[id];
        }


        public void ChangeColorMode(int mode = 0)
        {
            if (mode == 1)
            {
                // Create a new color map chip based on the exsiting color chip 
                if (targetGame.GetChip(ColorMapParser.chipName, false) == null)
                {
                    // Since we need pagincation and other values only on the color chip we'll create one here
                    // Create new color map chip
                    var colorMapChip = new ColorChip();

                    // Add the chip to the engine
                    targetGame.ActivateChip(ColorMapParser.chipName, colorMapChip, false);

                    // Register the temporary color chip as a ColorMapChip
//                    targetGame.chipManager.ActivateChip(typeof(ColorMapChip).FullName, colorMapChip, false);

                    var colors = colorChip.hexColors;

                    colorMapChip.total = colors.Length;

                    for (var i = 0; i < colors.Length; i++) colorMapChip.UpdateColorAt(i, colors[i]);

//                    Debug.Log("Create New Color Map Chip");
                }

//                Debug.Log("Color Map Chip Exists " + (targetGame.colorMapChip != null));

                // Since we are using a color chip we need to make sure we call the rigt chip because its not registered with the engine
                activeColorChip = targetGame.GetChip(ColorMapParser.chipName, false) as ColorChip;
            }
            else if (mode == 2)
            {
                activeColorChip = targetGame.GetChip(FlagColorParser.flagColorChipName, false) as ColorChip;
            }
            else
            {
                activeColorChip = targetGame.colorChip;
            }
        }


        public void OptimizeSprites()
        {
//            Console.WriteLine("Optimize sprites " + spriteChip.width + " " + spriteChip.height);

            var tmpSpriteChip = new SpriteChip();

            tmpSpriteChip.width = 8;
            tmpSpriteChip.height = 8;

            tmpSpriteChip.Resize(spriteChip.textureWidth, spriteChip.textureHeight);

            // Loop through all the sprites and copy them to the new chip
            var total = spriteChip.totalSprites;

            var tmpPixelData = new int[8 * 8];
            var nextSpriteID = 0;
            var i = 0;


            // Copy the sprites to the temp chip
            for (i = 0; i < total; i++)
            {
                spriteChip.ReadSpriteAt(i, tmpPixelData);

                if (tmpSpriteChip.FindSprite(tmpPixelData) == -1)
                {
                    tmpSpriteChip.UpdateSpriteAt(nextSpriteID, tmpPixelData);

                    nextSpriteID++;
                }
            }

            spriteChip.Clear();

            total = tmpSpriteChip.spritesInRam;

            for (i = 0; i < total; i++)
            {
                tmpSpriteChip.ReadSpriteAt(i, tmpPixelData);
                spriteChip.UpdateSpriteAt(i, tmpPixelData);
            }

//            Console.WriteLine("Optimized sprites " + i + " to " + total);

//            spriteChip.texture = tmpSpriteChip.texture;
        }

        public int SpritesInRam()
        {
            return spriteChip.spritesInRam;
        }

        public bool LoadImage(string path)
        {
            // Convert to a system path
            var filePath = WorkspacePath.Parse(path);

            // If the file doesn't exist, return false.
            if (!workspace.Exists(filePath))
                return false;

            byte[] imageBytes = null;

            try
            {
                // Read bytes from image file
                using (var memoryStream = new MemoryStream())
                {
                    workspace.OpenFile(filePath, FileAccess.Read).CopyTo(memoryStream);

                    imageBytes = memoryStream.ToArray();
                }
            }
            catch
            {
                runner.DisplayWarning("Unable to read image file.");
            }

            try
            {
                var saveFlags = BuildSaveFlags(new[] {SaveFlags.Colors, SaveFlags.Tilemap});

                var files = new Dictionary<string, byte[]>
                {
                    {"colors.png", imageBytes},
                    {"tilemap.png", imageBytes}
                };

                // We only need a few chips to make this work
                var chips = new[]
                {
                    typeof(ColorChip).FullName,
                    typeof(SpriteChip).FullName,
                    typeof(TilemapChip).FullName,
                    typeof(DisplayChip).FullName,
                    typeof(GameChip).FullName
                };

                targetGame = new PixelVisionEngine(serviceManager, chips);

//                var imageTexture = runner.loadService.textureFactory.NewTexture2D(1,1);
//    
//                imageTexture.LoadImage(imageBytes);


                var tmpParser = new PNGReader(imageBytes);


                // Only save unique colors
                targetGame.colorChip.unique = true;

                // 
                targetGame.colorChip.total = 256;
                targetGame.colorChip.Clear();

                // Make sure we only have unique sprites
                targetGame.spriteChip.unique = true;
                targetGame.spriteChip.colorsPerSprite = 16;
                targetGame.spriteChip.pages = 8;

                // Set this flag so it auto imports all tiles into the sprite memeory
                targetGame.tilemapChip.autoImport = true;

                // Resize the tilemap
                targetGame.tilemapChip.Resize(tmpParser.width / 8, tmpParser.height / 8);


                runner.ParseFiles(files, targetGame, saveFlags, true);
            }
            catch
            {
//                Console.WriteLine("Game Editor Load Error:\n"+e.Message);

                return false;
            }

            Reset();

            return true;
        }

        public bool LoadFont(string path)
        {
            // Convert to a system path
            var filePath = WorkspacePath.Parse(path);

            // If the file doesn't exist, return false.
            if (!workspace.Exists(filePath))
                return false;

            byte[] imageBytes = null;

            try
            {
                // Read bytes from image file
                using (var memoryStream = new MemoryStream())
                {
                    workspace.OpenFile(filePath, FileAccess.Read).CopyTo(memoryStream);

                    imageBytes = memoryStream.ToArray();
                }
            }
            catch
            {
                runner.DisplayWarning("Unable to read image file.");
            }

            try
            {
                var saveFlags = BuildSaveFlags(new[] {SaveFlags.Colors, SaveFlags.Fonts});

                var files = new Dictionary<string, byte[]>
                {
                    {"colors.png", imageBytes},
                    {filePath.EntityName, imageBytes}
                };

                // We only need a few chips to make this work
                var chips = new[]
                {
                    typeof(ColorChip).FullName,
                    typeof(SpriteChip).FullName,
                    typeof(TilemapChip).FullName,
                    typeof(DisplayChip).FullName,
                    typeof(GameChip).FullName
                };

                targetGame = new PixelVisionEngine(serviceManager, chips);

                // Only save unique colors
                targetGame.colorChip.unique = true;

                // 
                targetGame.colorChip.total =
                    2; // TODO need to make sure there are enough colors for the font but technically it should be 1 bit (b&w)

                // Make sure we only have unique sprites
                targetGame.spriteChip.unique = false;

                targetGame.spriteChip.pages = 1;

                targetGame.name = path;

                runner.ParseFiles(files, targetGame, saveFlags, true);
            }
            catch
            {
//                Console.WriteLine("Game Editor Load Error:\n"+e.Message);

                return false;
            }

            Reset();

            return true;
        }

        public void SaveFont(string fontName, string oldName = null)
        {
            var engineName = targetGame.name;

            var parentFilePath = WorkspacePath.Parse(engineName).ParentPath;

            if (fontName != oldName)
            {
                var oldPath = parentFilePath.AppendFile(oldName);

                if (workspace.Exists(oldPath)) workspace.Delete(oldPath);
            }

            var fontPath = parentFilePath.AppendFile(fontName);

            var pngWriter = new PNGWriter();

            var exporter = new FontExporter(fontPath.EntityName, targetGame, pngWriter);
            exporter.CalculateSteps();

            while (exporter.completed == false) exporter.NextStep();

            var files = new Dictionary<string, byte[]>
            {
                {fontPath.Path, exporter.bytes}
            };

            workspace.SaveExporterFiles(files);
        }

        public string ReadMetaData(string key, string defaultValue = "")
        {
            return targetGame.GetMetaData(key, defaultValue);
        }

        public void WriteMetaData(string key, string value)
        {
            targetGame.SetMetaData(key, value);
        }

//
//        #region Controller APIs
//
//        public int CaptureKey(int[] values)
//        {
//            return controllerChip.CaptureKey(values);
//        }
//
//        public int[] ReadButtonKeys(int player)
//        {
//            return controllerChip.ReadControllerKeys(player);
//        }
//
//        public void UpdateControllerKey(int controllerID, int buttonID, int key)
//        {
//            var button = (Buttons) buttonID;
//
//            //workspace.UpdateControllerKey(controllerID, button, key);
//            controllerChip.UpdateControllerKey(controllerID, new KeyboardButtonInput(button, key));
//        }
//
//        public int ReadControllerKey(int controllerID, int buttonID)
//        {
//            return controllerChip.ReadControllerKey(controllerID, (Buttons) buttonID);
//        }
//
//        public void Revert()
//        {
//            controllerChip.RevertControllerMapping();
//        }
//
//        public void CaptureButton()
//        {
//            // TODO need to figure out how to connect this to the imput class and get the native system's controller values
//            throw new NotImplementedException();
////            var value = Input.GetAxis("Joystick 1 Up");
////
////            if (value > 0)
////                Debug.Log("UP : " + value);
//        }
//
//        public int ReadSystemKey(int id)
//        {
//            return controllerChip.ReadSystemKey(id);
//        }
//
//        public void SaveController()
//        {
//            controllerChip.UpdateKeysInBios();
//        }
//
//        #endregion
//
//
//

        #region Colors

        /// <summary>
        /// </summary>
        /// <param name="total"></param>
        /// <returns></returns>
        public int ColorPages(int? total = null)
        {
            // TODO this is deprecated and the API needs to be updated

            if (total.HasValue) activeColorChip.total = total.Value * 64;

            return MathUtil.CeilToInt(activeColorChip.total / 64);
        }

//        public bool PaletteMode(bool? active)
//        {
//            if (active.HasValue)
//                colorChip.paletteMode = active.Value;
//            
//            return colorChip.paletteMode;
//        }

        public void ReindexSprites()
        {
            // TODO each sprite needs to be clamped between the max colors per sprite

            var rawSpriteData = spriteChip.texture.GetPixels();

            var colorMap = new List<int> {-1};

            var total = rawSpriteData.Length;

            var i = 0;

            // The first pass creates a color map
            for (i = 0; i < total; i++)
            {
                var pixel = rawSpriteData[i];
//                if (pixel > -1)
//                {
                if (colorMap.IndexOf(pixel) == -1) colorMap.Add(pixel);
//                }
            }

            // Loop back through the pixels and remap them
            for (i = 0; i < total; i++)
            {
                var pixel = rawSpriteData[i];

//                if (pixel > -1)
//                {
                rawSpriteData[i] = colorMap.IndexOf(pixel);
//                }
            }

            // Create the 16 colors the sprites will be remapped to
            var colorMapColors = new[]
            {
                "#000000",
                "#111111",
                "#222222",
                "#333333",
                "#444444",
                "#555555",
                "#666666",
                "#777777",
                "#888888",
                "#999999",
                "#AAAAAA",
                "#BBBBBB",
                "#CCCCCC",
                "#DDDDDD",
                "#EEEEEE",
                "#FFFFFF"
            };

            // Set the new color total
            total = colorMapColors.Length;

            // Create a color map chip
            var colorMapChip = new ColorChip();

            // Clear the color map chip and rebuild the pages
            colorMapChip.total = total;
            colorMapChip.Clear();

            // Add the colors to the color map chip
            for (i = 0; i < total; i++) colorMapChip.UpdateColorAt(i, colorMapColors[i]);

//            colorChip.paletteMode = true;

            // Add the chip to the engine
            targetGame.ActivateChip(ColorMapParser.chipName, colorMapChip, false);

            // Set the pixels back into the sprite texture
            spriteChip.texture.SetPixels(rawSpriteData);
        }

//        public int colorsPerPalette(int? total)
//        {
//            if (total.HasValue)
//            {
//                colorChip.colorsPerPalette = total.Value;
//            }    
//
//            return colorChip.colorsPerPalette;
//        }

        /// <summary>
        ///     Special method to resize a tool's memory to allow it to store colors for the tool
        ///     and the game itself. Should make the color chip memory store 512 colors
        /// </summary>
        public void ResizeToolColorMemory()
        {
            runner.activeEngine.colorChip.total = 512;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
//        public string[] SupportedColors()
//        {
//            try
//            {
//                return colorChip.supportedColors;
//            }
//            catch (Exception e)
//            {
//                var tmpColors = colorChip.hexColors.Distinct().ToList();
//
//                tmpColors.Remove(colorChip.maskColor);
//                
//                return tmpColors.ToArray();
//            }
//        }

//        public int TotalSupportedColors()
//        {
//            return SupportedColors().Length;
//        }

        /// <summary>
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string MaskColor(string value = null)
        {
            if (value != null) colorChip.maskColor = value;

            return colorChip.maskColor;
        }

        #endregion


        #region Rendering

        private readonly Dictionary<string, int> tmpPos = new Dictionary<string, int>
        {
            {"x", 0},
            {"y", 0}
        };

        /// <summary>
        /// </summary>
        /// <param name="total"></param>
        /// <returns></returns>
        public int SpritePages(int? total = null)
        {
            if (total.HasValue) spriteChip.pages = total.Value;

            return spriteChip.pages;
        }

        /// <summary>
        /// </summary>
        /// <param name="total"></param>
        /// <returns></returns>
        public int ColorsPerSprite(int? total)
        {
            // 
            if (total.HasValue) spriteChip.colorsPerSprite = total.Value;

            return spriteChip.colorsPerSprite;
        }

        /// <summary>
        /// </summary>
        /// <param name="total"></param>
        /// <returns></returns>
        public int DrawCalls(int? total = null)
        {
            if (total.HasValue) spriteChip.maxSpriteCount = total.Value;

            return spriteChip.maxSpriteCount;
        }

        #endregion

        #region Sounds

        public void PlaySound(int id, int channel = 0)
        {
            gameChip.PlaySound(id, channel);
        }

        public bool IsChannelPlaying(int channel = 0)
        {
            return gameChip.IsChannelPlaying(channel);
        }

        public void StopSound(int channel = 0)
        {
            gameChip.StopSound(channel);
        }

        public void DrawSpriteBlock(int id, int x, int y, int width = 1, int height = 1, bool flipH = false,
            bool flipV = false,
            DrawMode drawMode = DrawMode.Sprite, int colorOffset = 0, bool onScreen = true, bool useScrollPos = true,
            Rectangle? bounds = null)
        {
            throw new NotImplementedException();
        }

//        public void DrawTile(int id, int c, int r, DrawMode drawMode = DrawMode.Tile, int colorOffset = 0)
//        {
//            gameChip.DrawTile(id, c, r, drawMode, colorOffset);
//        }
//
//        public void DrawTiles(int[] ids, int c, int r, int width, DrawMode drawMode = DrawMode.Tile,
//            int colorOffset = 0)
//        {
//            gameChip.DrawTiles(ids, c, r, width, drawMode, colorOffset);
//        }

        /// <summary>
        /// </summary>
        /// <param name="total"></param>
        /// <returns></returns>
        public int TotalSounds(int? total = null)
        {
            if (total.HasValue) soundChip.totalSounds = total.Value;

            return soundChip.totalSounds;
        }

        /// <summary>
        /// </summary>
        /// <param name="total"></param>
        /// <returns></returns>
        public int TotalChannels(int? total = null)
        {
            if (total.HasValue) soundChip.totalChannels = total.Value;

            return soundChip.totalChannels;
        }

        /// <summary>
        /// </summary>
        /// <param name="index"></param>
        /// <param name="template"></param>
        public void GenerateSound(int index, int template)
        {
            // Create a tmp synth parameter
            var settings = new SfxrSynth().parameters;

            // Apply sound template
            switch (template)
            {
                case 1:
                    settings.GeneratePickupCoin();
                    break;
                case 2:
                    settings.GenerateLaserShoot();
                    break;
                case 3:
                    settings.GenerateExplosion();
                    break;
                case 4:
                    settings.GeneratePowerup();
                    break;
                case 5:
                    settings.GenerateHitHurt();
                    break;
                case 6:
                    settings.GenerateJump();
                    break;
                case 7:
                    settings.GenerateBlipSelect();
                    break;
                default:
                    settings.Randomize();
                    break;
            }

            gameChip.Sound(index, settings.GetSettingsString());
        }

        /// <summary>
        /// </summary>
        /// <param name="id"></param>
        public void Mutate(int id)
        {
            soundChip.ReadSound(id).Mutate();
        }

        /// <summary>
        /// </summary>
        /// <param name="id"></param>
        public void NewSound(int id)
        {
            var settings = new SfxrSynth().parameters;
            gameChip.Sound(id, settings.GetSettingsString());
        }

        /// <summary>
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public string SoundLabel(int index, string value = null)
        {
            if (value != null) soundChip.UpdateLabel(index, value);

            return soundChip.ReadLabel(index);
        }

        #endregion

        #region Music

        /// <summary>
        /// </summary>
        /// <param name="total"></param>
        /// <returns></returns>
        public int TotalTracks(int? total)
        {
            if (total.HasValue) musicChip.totalTracks = total.Value;

            return musicChip.totalTracks;
        }

        /// <summary>
        /// </summary>
        /// <param name="total"></param>
        /// <returns></returns>
        public int TotalLoops(int? total)
        {
            if (total.HasValue) musicChip.totalLoops = total.Value;

            return musicChip.totalLoops;
        }

        /// <summary>
        /// </summary>
        /// <param name="id"></param>
        public void LoadLoop(int id)
        {
            //TODO change this to load loop, all APIs should be loop based
            musicChip.LoadPattern(id);
        }

//        public string LoopName(string value = null)
//        {
//            if (value != null) musicChip.activeTrackerData.songName = value;
//
//            return musicChip.activeTrackerData.songName;
//        }

        /// <summary>
        /// </summary>
        /// <param name="track"></param>
        /// <param name="beat"></param>
        /// <param name="note"></param>
        /// <returns></returns>
        public int Note(int track, int beat, int? note)
        {
            if (track < 0 || track >= musicChip.totalTracks)
                return 0;

            var notes = musicChip.activeTrackerData.tracks[track].notes;

            if (beat > notes.Length || beat < 0)
                return 0;

            if (note.HasValue) notes[beat] = note.Value;

            return notes[beat];
        }

        /// <summary>
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public int Tempo(int? value)
        {
            if (value.HasValue)
            {
                musicChip.activeTrackerData.speedInBPM = value.Value;
                musicChip.UpdateNoteTickLengths();
            }

            return musicChip.activeTrackerData.speedInBPM;

            //workspace.InvalidateSave();
        }

        public bool MuteTrack(int track, bool? value = null)
        {
            if (value.HasValue) musicChip.activeTrackerData.tracks[track].mute = value.Value;

            return musicChip.activeTrackerData.tracks[track].mute;
        }

        public int ConfigTrackSFX(int track, int? id)
        {
            if (id.HasValue) musicChip.trackSettings[track].sfxID = id.Value;

            return musicChip.trackSettings[track].sfxID;
        }

        public int ConfigTrackInstrument(int track, int? id)
        {
            if (id.HasValue) musicChip.trackSettings[track].instrumentType = (InstrumentType) id.Value;

            return (int) musicChip.trackSettings[track].instrumentType;
        }

        public Point ConfigTrackOctaveRange(int track, Point? range = null)
        {
            if (range.HasValue) musicChip.trackSettings[track].octaveRange = range.Value;

            return musicChip.trackSettings[track].octaveRange;
        }

        /// <summary>
        /// </summary>
        /// <param name="track"></param>
        /// <param name="sfxID"></param>
        /// <returns></returns>
        public int TrackInstrument(int track, int? sfxID)
        {
            if (sfxID.HasValue)
                musicChip.activeTrackerData.tracks[track].sfxID = MathHelper.Clamp(sfxID.Value, 0, soundChip.totalSounds);

            return musicChip.activeTrackerData.tracks[track].sfxID;
        }

        public int NotesPerTrack(int? value = null)
        {
            if (value.HasValue) musicChip.notesPerTrack = 32;

            return musicChip.notesPerTrack;
        }

        /// <summary>
        /// </summary>
        /// <param name="id"></param>
        public void PreviewInstrument(int id)
        {
            // Just need to get a reference to any track setting for this data
            var soundData = musicChip.trackSettings[0].ReadInstrumentSoundData(id);

            if (soundData != null)
                soundChip.PlayRawSound(soundData);
        }

        /// <summary>
        /// </summary>
        public void ConfigureGenerator()
        {
            musicChip.ConfigureGenerator();
        }


        /// <summary>
        /// </summary>
        public void GenerateSong()
        {
            musicChip.GenerateSong();
        }

        /// <summary>
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public Point OctaveRange(int? min, int? max)
        {
            if (min.HasValue) musicChip.octaveRange.X = min.Value;

            if (max.HasValue) musicChip.octaveRange.Y = max.Value;

            return musicChip.octaveRange;
        }

        /// <summary>
        /// </summary>
        /// <param name="timeDelta"></param>
        public void UpdateSequencer(float timeDelta)
        {
            musicChip.Update(timeDelta);
        }

        /// <summary>
        /// </summary>
        /// <param name="loop"></param>
        public void StartSequencer(bool loop = false)
        {
            LoopSong(loop);
            musicChip.StartSequencer();
        }

        /// <summary>
        /// </summary>
        /// <param name="value"></param>
        public void LoopSong(bool value)
        {
            musicChip.loopSong = value;
        }

        /// <summary>
        /// </summary>
        public void StopSequencer()
        {
            musicChip.StopSequencer();
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public bool SongPlaying()
        {
            return musicChip.songCurrentlyPlaying;
        }

        /// <summary>
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public int CurrentBeat(int? pos)
        {
            if (pos.HasValue) musicChip.MoveToBeat(pos.Value);

            return musicChip.currentBeat;
        }

        /// <summary>
        /// </summary>
        public void ResetSong()
        {
            musicChip.ResetTracker();
        }

        /// <summary>
        /// </summary>
        /// <param name="track"></param>
        /// <param name="beat"></param>
        public void PlayNote(int track, int beat)
        {
            musicChip.PlayNote(track, beat);
        }

        /// <summary>
        /// </summary>
        /// <param name="track"></param>
        /// <returns></returns>
        public int ReadInstrumentID(int track)
        {
            return (int) musicChip.trackSettings[track].instrumentType;
        }

        /// <summary>
        /// </summary>
        public int pcgDensity
        {
            get => musicChip.pcgDensity;
            set => musicChip.pcgDensity = value;
        }

        /// <summary>
        /// </summary>
        public int pcgFunk
        {
            get => musicChip.pcgFunk;
            set => musicChip.pcgFunk = value;
        }

        /// <summary>
        /// </summary>
        public int pcgLayering
        {
            get => musicChip.pcgLayering;
            set => musicChip.pcgLayering = value;
        }

        /// <summary>
        /// </summary>
        public int pcgMinTempo
        {
            get => musicChip.pcgMinTempo;
            set => musicChip.pcgMinTempo = value;
        }

        /// <summary>
        /// </summary>
        public int pcgMaxTempo
        {
            get => musicChip.pcgMaxTempo;
            set => musicChip.pcgMaxTempo = value;
        }

        /// <summary>
        /// </summary>
        public int scale
        {
            get => musicChip.scale;
            set => musicChip.scale = value;
        }

        /// <summary>
        /// </summary>
        /// <param name="track"></param>
        /// <param name="instrument"></param>
        /// <param name="sfxID"></param>
        public void SetTrack(int track, int instrument, int sfxID)
        {
            musicChip.trackSettings[track].instrumentType = (InstrumentType) instrument;
            musicChip.trackSettings[track].sfxID = sfxID;
        }

        #endregion


        #region Sprites

        /// <summary>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="flipH"></param>
        /// <param name="flipV"></param>
        /// <returns></returns>
        public int[] ReadSpriteData(int id, bool flipH, bool flipV)
        {
            var pixelData = Sprite(id);

            if (flipH || flipV)
                SpriteChipUtil.FlipSpriteData(ref pixelData, spriteChip.width, spriteChip.height, flipH, flipV);

            return pixelData;
        }

        /// <summary>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="flipH"></param>
        /// <param name="flipV"></param>
        public void WriteSpriteData(int id, bool flipH, bool flipV)
        {
            // TODO need to make sure the sprite data is flipped correctly before being saved
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public bool SpriteBuilderActive(string rootPath)
        {
            var filePath = WorkspacePath.Parse(rootPath);

            // Check to see if the sprite builder folder exists
            return workspace.ValidateSpriteBuilderFolder(filePath);
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public int RunSpriteBuilder(string path)
        {
            // Make sure we are editing a game
            if (targetGame == null) return -1;


            // Generate the sprites
            return workspace.GenerateSprites(path, targetGame);
        }
//
//        /// <summary>
//        /// 
//        /// </summary>
//        /// <returns></returns>
//        public bool AreSpritesCompressed()
//        {
//            return workspace.AreSpritesCompressed();
//        }

        /// <summary>
        /// </summary>
        public void CompressSprites()
        {
        }

        #endregion

        #region Render Tilemap Layer

        public Canvas NewCanvas(int width, int height)
        {
            return new Canvas(width, height, gameChip);
        }

        private Canvas tmpCanvas;
        public bool renderingMap { get; private set; }
        private int[] tmpPixelData = new int[0];
        private int mapRenderMode;
        private int totalTiles;
        private int totalLoops;
        private readonly int maxPerLoop = 100;
        private int currentLoop;

        private readonly Canvas[] layerCache = new Canvas[2];

        public int renderPercent => MathHelper.Clamp(((int) (currentLoop / (float) totalLoops * 100)), 0, 100);

        public void RenderMapLayer(int mode)
        {
            var realWidth = spriteChip.width * tilemapChip.columns;
            var realHeight = spriteChip.height * tilemapChip.rows;

            mapRenderMode = mode;

            if (layerCache[mode] == null)
            {
                layerCache[mode] = new Canvas(realWidth, realHeight, gameChip);
                renderingMap = true;
            }

            // Set the tmpCanvas to the cache
            tmpCanvas = layerCache[mode];

            // Rebuild the map if it hasn't been rendered yet.
            if (renderingMap)
            {
                tmpCanvas.Clear(mode == 0 ? colorChip.backgroundColor : -2);

                totalTiles = tilemapChip.total;

                Array.Resize(ref tmpPixelData, spriteChip.width * spriteChip.height);

                renderingMap = true;

                totalLoops = MathUtil.CeilToInt(tilemapChip.total / (float) maxPerLoop);

                currentLoop = 0;
            }
        }

        public void NextRenderStep()
        {
            var offset = currentLoop * maxPerLoop;

            for (var i = 0; i < maxPerLoop; i++)
            {
                var index = i + offset;
                if (index >= totalTiles)
                {
                    renderingMap = false;
                    break;
                }

                var pos = gameChip.CalculatePosition(index, tilemapChip.columns);

                var tileData = gameChip.Tile(pos.X, pos.Y);

                RenderTile(tileData, pos.X, pos.Y);
            }

            currentLoop++;
        }

        private void RenderTile(TileData tileData, int col, int row)
        {
            int[] spriteData;
            int[] flagData;

            col *= spriteChip.width;
            row *= spriteChip.height;

            var totalPixels = spriteChip.width * spriteChip.height;

            if (layerCache[0] != null)
            {
                // 

                if (tileData.spriteID == -1)
                    spriteData = Enumerable.Repeat(-1, totalPixels).ToArray();
                else
                    spriteData = gameChip.Sprite(tileData.spriteID);

                // Shift the pixel data
                for (var j = 0; j < spriteData.Length; j++) spriteData[j] += tileData.colorOffset;

                //
                layerCache[0].SetPixels(col, row, spriteChip.width, spriteChip.height, spriteData);
            }

            if (layerCache[1] != null)
            {
                flagData = Enumerable.Repeat(tileData.flag, spriteChip.width * spriteChip.height).ToArray();

                layerCache[1].SetPixels(col, row, spriteChip.width, spriteChip.height, flagData);
            }
        }


        public void CopyRenderToDisplay(int x, int y, int width, int height, int colorOffset, int maskColor = -1)
        {
            // Only render when a canvas exists
            if (tmpCanvas == null)
                return;

            // Should have some kind of invalidation test

            // Get scroll position for tmpX/Y position
            var scrollPos = gameChip.ScrollPosition();
            var tmpX = scrollPos.X;
            var tmpY = scrollPos.Y;

            // Copy the pixels from the canvas
            tmpCanvas.CopyPixels(ref tmpPixelData, tmpX, tmpY, width, height);

//            if (useBGColor)
//            {
            var size = tmpPixelData.Length;

            // Replace empty colors with the background
            for (var i = 0; i < size; i++)
                if (tmpPixelData[i] < 0)
                    tmpPixelData[i] = maskColor; //useBGColor ? colorChip.backgroundColor : -1 - colorOffset;
//            }

            // Copy to the active game's tilemap layer
            runner.activeEngine.gameChip.DrawPixels(tmpPixelData, x, y, width, height, false, false,
                DrawMode.TilemapCache, colorOffset);
        }

        public void CopyCanvasToDisplay(Canvas srcCanvas, int x, int y, int width, int height, int colorOffset,
            int maskColor = -1)
        {
            // Only render when a canvas exists
//            if (tmpCanvas == null)
//                return;

            // Should have some kind of invalidation test

            // Get scroll position for tmpX/Y position
            var scrollPos = gameChip.ScrollPosition();
            var tmpX = scrollPos.X;
            var tmpY = scrollPos.Y;

            // Copy the pixels from the canvas
            srcCanvas.CopyPixels(ref tmpPixelData, tmpX, tmpY, width, height);

//            if (useBGColor)
//            {
            var size = tmpPixelData.Length;

            // Replace empty colors with the background
            for (var i = 0; i < size; i++)
                if (tmpPixelData[i] < 0)
                    tmpPixelData[i] = maskColor; //useBGColor ? colorChip.backgroundColor : -1 - colorOffset;
//            }

            // Copy to the active game's tilemap layer
            runner.activeEngine.gameChip.DrawPixels(tmpPixelData, x, y, width, height, false, false,
                DrawMode.TilemapCache, colorOffset);
        }

        #endregion

        #region Palette APIs

//        public bool UsePalettes()
//        {
//            return colorPaletteChip != null;
//        }

//        public int ColorsPerPalette(int? total = null)
//        {
//
//            if (total.HasValue)
//            {
//                // TODO need to rebuild the pages?
//                colorPaletteChip.colorsPerPage = total.Value.Clamp(2, 8);
//            }
//            
//            return UsePalettes() ? colorPaletteChip.total : -1;
//
//        }

        #endregion

        // TODO need a way to reduce the palette for sprites?

        #region Tool APIs

        /// <summary>
        ///     Get the tool's colors from the active engine.
        /// </summary>
        /// <returns></returns>
        public string[] ToolColors()
        {
            return runner.activeEngine.colorChip.hexColors;
        }

        /// <summary>
        ///     Resize the tool's color pages in the active engine
        /// </summary>
        /// <param name="total"></param>
        public void ToolRebuildColorPages(int total)
        {
            runner.activeEngine.colorChip.total = total;
        }

        public void ExportSFX(int id, WorkspacePath workspacePath)
        {
            var sfx = soundChip.ReadSound(id);

            workspacePath = workspacePath.AppendFile(sfx.name + ".wav");
            
            // TODO need to wire this up
            var synth = new SfxrSynth();
            synth.parameters.SetSettingsString(sfx.ReadSettings());
//            var stream = workspace.CreateFile(path);
            
            var files = new Dictionary<string, byte[]>()
            {
                {workspacePath.Path, synth.GenerateWav()}
            };
            
            workspace.SaveExporterFiles(files);
//            
//            File.WriteAllBytes(path, synth.GetWavFile());

        }

        #endregion
    }
}