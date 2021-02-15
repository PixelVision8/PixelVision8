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
using Microsoft.Xna.Framework.Input;
using PixelVision8.Player;
using PixelVision8.Player.Audio;
using PixelVision8.Runner.Chips;
using PixelVision8.Runner.Exporters;
using PixelVision8.Runner;
using PixelVision8.Runner.Workspace;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Buttons = PixelVision8.Player.Buttons;

namespace PixelVision8.Runner.Editors
{
    /// <summary>
    ///     This class allows you to edit the current sandbox game.
    /// </summary>
    //    [MoonSharpUserData]
    public class GameEditor
    {
        protected bool _invalid;

        // public ColorChip activeColorChip;

        //        private GCControllerChip controllerChip;
        private ColorChip colorChip;

        //        private ColorMapChip colorMapChip; // TODO this should have a GC version of the chip
        private DisplayChip displayChip;
        private FontChip fontChip;

        //
        //        public IEngine targetGame;
        //
        private GameChip _gameChip;
        private MusicChip musicChip;
        protected DesktopRunner runner;

        protected IServiceLocator serviceManager;
        private SfxrSoundChip soundChip;
        private SpriteChip spriteChip;
        private PixelVision targetGame;
        private TilemapChip tilemapChip;

        protected WorkspaceServicePlus workspace;

        //        private ColorChip colorPaletteChip;

        /// <summary>
        ///     Creates a new Game Editor instance and loads the game's system and meta data
        /// </summary>
        /// <param name="workspace"></param>
        /// <param name="luaService"></param>
        //        [MoonSharpHidden]
        public GameEditor(DesktopRunner runner, IServiceLocator serviceManager)
        {
            this.runner = runner;
            this.serviceManager = serviceManager;
            // Get a reference to the workspace from the runner instance
            workspace = runner.workspaceService as WorkspaceServicePlus;
        }

        public virtual List<string> DefaultChips
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
                    typeof(SfxrSoundChip).FullName,
                    typeof(MusicChip).FullName,
                    typeof(LuaGameChip).FullName
                };

                return chips;
            }
        }

        public bool Invalid => _invalid;


        /// <summary>
        ///     Helper method to build save flags.
        /// </summary>
        /// <param name="flags"></param>
        /// <returns></returns>
        private FileFlags BuildSaveFlags(FileFlags[] flags)
        {
            // Since Lua doesn't know how to handle bit flags, we need to do the conversion on the C# side of things
            var saveFlags = FileFlags.System;

            for (var i = 0; i < flags.Length; i++)
                if (flags[i] != FileFlags.System)
                    saveFlags |= flags[i];

            return saveFlags;
        }

        /// <summary>
        ///     This allows you to load game files inside of the Workspace directory into the Game Editor. It
        ///     accepts an array of SaveFlags enums which define what game files to load. Use this method to load only
        ///     what you need to edit to speed up parsing game data.
        /// </summary>
        /// <param name="flags">Supply an array of SaveFlags enums for each file you want to load into the Game Editor instance.</param>
        /// <returns>Returns a bool if the loading process was successful.</returns>

        // TODO should use workspace path as argument
        public bool Load(string path, FileFlags[] flags)
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
            if (!workspace.Exists(filePath)) return false;

            var files = workspace.GetGameEntities(filePath);

            var saveFlags = BuildSaveFlags(flags);

            try
            {
                targetGame = new PixelVision(DefaultChips.ToArray(), "GameEditor")
                {
                    ServiceLocator = serviceManager
                };
                // targetGame.Init();

                runner.ParseFiles(files, targetGame, saveFlags);
            }
            catch (Exception e)
            {
                Console.WriteLine("Game Editor Load Error:\n" + e.Message);

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
            _gameChip = targetGame.GameChip;

            // Since the game is not attached to a runner it will throw an error when trying to load lua serivce
            try
            {
                // Configure the game chip
                _gameChip.Reset();
            }
            catch
            {
                //                Console.WriteLine("Game Editor Reset Error:\n"+e.Message);
                // Do nothing with any missing service error
            }

            //
            //            // Get references to all of the chips
            //            controllerChip = targetGame.controllerChip as GCControllerChip;
            colorChip = targetGame.ColorChip;
            //            colorMapChip = targetGame.chipManager.GetChip(ColorMapParser.chipName, false) as ColorChip;
            //
            //            colorMapChip = targetGame.colorMapChip;
            displayChip = targetGame.DisplayChip;
            spriteChip = targetGame.SpriteChip;
            fontChip = targetGame.FontChip;
            tilemapChip = targetGame.TilemapChip;
            soundChip = targetGame.SoundChip as SfxrSoundChip;
            musicChip = targetGame.MusicChip; // TODO need to create a SfxrMusicChip

            //            Console.WriteLine("MC Tracks " + musicChip.totalTracks);
            songGenerator = new SfxrMusicGeneratorChip();

            //            
            //            colorPaletteChip = targetGame.chipManager.GetChip(ColorPaletteParser.chipName, false) as ColorChip;

            // ChangeColorMode();
        }

        /// <summary>
        ///     Returns a list of library paths
        /// </summary>
        /// <returns></returns>
        // public string[] LibraryPaths()
        // {
        //
        //     var sharedLibPaths = workspace.SharedLibDirectories();
        //
        //     var luaFiles = new List<string>();
        //
        //     for (int i = 0; i < sharedLibPaths.Count; i++)
        //     {
        //         
        //         var files = from p in workspace.GetEntities(sharedLibPaths[i])
        //                    where p.EntityName.EndsWith("lua") select p;
        //
        //         foreach (var file in files)
        //         {
        //             if (luaFiles.IndexOf(file.Path) == -1)
        //             {
        //                 var text = workspace.ReadTextFromFile(file);
        //
        //                 luaFiles.Add(file.Path);
        //             }
        //             
        //         }
        //
        //     }
        //
        //     return luaFiles.ToArray();
        // }

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
        /// <summary>
        ///     Allows you to save the current game you are editing. Pass in SaveFlags to define which files should be exported.
        /// </summary>
        /// <param name="flags"></param>
        public void Save(string path, FileFlags[] flags, bool useSteps = false)
        {
            // TODO need to get the export service


            // TODO should only save flags that are supplied
            var saveFlags = FileFlags.None;

            for (var i = 0; i < flags.Length; i++) saveFlags |= flags[i];

            targetGame.SetMetadata("version", runner.SystemVersion);
            //            gameChip.version = ;

            // TODO saving games doesn't work
            runner.SaveGameData(path, targetGame, saveFlags, useSteps);
            //            workspace.SaveCart(workspace.WorkspacePath(WorkspaceFolders.CurrentGameDir), targetGame, saveFlags);

            ResetValidation();
        }

        //
        /// <summary>
        ///     Change the game's max size setting.
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public int GameMaxSize(int? size = null)
        {
            if (size.HasValue)
            {
                _gameChip.maxSize = size.Value;
                Invalidate();
            }

            return _gameChip.maxSize;
        }

        /// <summary>
        ///     Change the game's save slot settings.
        /// </summary>
        /// <param name="total"></param>
        /// <returns></returns>
        public int GameSaveSlots(int? total = null)
        {
            if (total.HasValue)
            {
                _gameChip.SaveSlots = total.Value;
                Invalidate();
            }

            return _gameChip.SaveSlots;
        }

        /// <summary>
        ///     Lock or unlock the game specs.
        /// </summary>
        /// <param name="isLocked"></param>
        /// <returns></returns>
        public bool GameSpecsLocked(bool? isLocked = null)
        {
            if (isLocked.HasValue)
            {
                _gameChip.lockSpecs = isLocked.Value;
                Invalidate();
            }

            return _gameChip.lockSpecs;
        }

        /// <summary>
        ///     Change the name of the game.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string Name(string name = null)
        {
            // If a new name is supplied, set it on the game chip
            if (name != null)
            {
                targetGame.GetMetadata("name", name);
                Invalidate();
            }


            // Return the latest name value from the gameChip
            return targetGame.GetMetadata("name", GetType().Name);
        }

        /// <summary>
        ///     Get the current version of the runner.
        /// </summary>
        /// <returns></returns>
        public string Version()
        {
            return targetGame.GetMetadata("version", runner.SystemVersion);
        }

        public string Ext(string value = null)
        {
            if (value != null)
            {
                targetGame.SetMetadata("ext", value);
                Invalidate();
            }

            return targetGame.GetMetadata("ext", ".pv8");
        }

        public int BackgroundColor(int? id = null)
        {
            return _gameChip.BackgroundColor(id);
        }

        public string BackgroundColorHex()
        {
            return colorChip.ReadColorAt(BackgroundColor());
        }

        public string Color(int id, string value = null)
        {
            if (value == null) return colorChip.ReadColorAt(id);

            colorChip.UpdateColorAt(id, value);

            return value;
        }

        public void ClearColors(string color = null)
        {
            colorChip.Clear(color);
        }

        public void ClearSprites()
        {
            spriteChip.Clear();
        }

        /// <summary>
        ///     Get the TotalDisks colors or change the limit for how many colors the color chip can store.
        /// </summary>
        /// <param name="ignoreEmpty"></param>
        /// <param name="total"></param>
        /// <returns></returns>
        public int TotalColors(bool ignoreEmpty = false)
        {
            //            if (TotalDisks.HasValue)
            //                activeColorChip.maxColors = TotalDisks.Value;

            return ignoreEmpty ? colorChip.TotalUsedColors : colorChip.Total;
        }

        // public int MaximumColors(int? value)
        // {
        //     if (value.HasValue) colorChip.maxColors = value.Value;
        //
        //     return colorChip.maxColors;
        // }


        // Since we want to be able to edit this value but the interface doesn't allow it, we hide it in lua and use the overload instead
        //        [MoonSharpHidden]
        public int ColorsPerSprite()
        {
            return _gameChip.ColorsPerSprite();
        }

        /// <summary>
        ///     Get the maximum number or sprites or change it.
        /// </summary>
        /// <param name="total"></param>
        /// <returns></returns>
        public int MaxSpriteCount(int? total)
        {
            if (total.HasValue) targetGame.SpriteChip.MaxSpriteCount = total.Value;

            return _gameChip.MaxSpriteCount();
        }

        /// <summary>
        ///     Get the display size or change it.
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
                size.X = displayChip.Width;
            }

            if (height.HasValue)
            {
                size.Y = height.Value;
                resize = true;
            }
            else
            {
                size.Y = displayChip.Height;
            }

            if (resize) displayChip.ResetResolution(size.X, size.Y);

            // TODO need a flag to tell the runner to change the resolution

            return new Point(displayChip.Width, displayChip.Height);
        }

        /// <summary>
        ///     Get the over scan value or change it
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        // public Point OverscanBorder(int? x, int? y)
        // {
        //     if (x.HasValue) displayChip.OverscanX = x.Value;
        //
        //     if (y.HasValue) displayChip.OverscanY = y.Value;
        //
        //     return new Point(displayChip.OverscanX, displayChip.OverscanY);
        // }
        public void RedrawDisplay()
        {
            throw new NotImplementedException();
        }

        public Point ScrollPosition(int? x = null, int? y = null)
        {
            return _gameChip.ScrollPosition(x, y);
        }

        public void WriteSaveData(string key, string value)
        {
            _gameChip.WriteSaveData(key, value);
        }

        public string ReadSaveData(string key, string defaultValue = "undefined")
        {
            return _gameChip.ReadSaveData(key, defaultValue);
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


        /// <summary>
        ///     Get the TotalDisks number of songs or change it
        /// </summary>
        /// <param name="total"></param>
        /// <returns></returns>
        public int TotalSongs(int? total = null)
        {
            if (total.HasValue) musicChip.totalSongs = total.Value;

            return musicChip.totalSongs;
        }

        public void PlaySong(int id, bool loop = true, int startAt = 0)
        {
            _gameChip.PlaySong(id, loop, startAt);
        }

        public void PlayPattern(int id, bool loop = true)
        {
            _gameChip.PlayPattern(id);
        }

        /// <summary>
        ///     Get a song's name or change it
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <returns></returns>
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

        /// <summary>
        ///     Return the patterns in a song or change them.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="patterns"></param>
        /// <returns></returns>
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

        /// <summary>
        ///     Get the pattern at a position in the song or change it.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="position"></param>
        /// <param name="newPattern"></param>
        /// <returns></returns>
        public int SongPatternAt(int id, int position, int? newPattern)
        {
            if (newPattern.HasValue) musicChip.songs[id].UpdatePatternAt(position, newPattern.Value);

            return musicChip.songs[id].patterns[position];
        }

        /// <summary>
        ///     Get the end position of a song or change it.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pos"></param>
        /// <returns></returns>
        public int SongEnd(int id, int? pos = null)
        {
            if (pos.HasValue) musicChip.songs[id].end = pos.Value;

            return musicChip.songs[id].end;
        }

        public void PauseSong()
        {
            _gameChip.PauseSong();
        }

        public void StopSong()
        {
            _gameChip.StopSong();
        }

        public void RewindSong(int position = 0, int loopID = 0)
        {
            _gameChip.RewindSong();
        }

        public Point SpriteSize()
        {
            return _gameChip.SpriteSize();
        }

        public int[] Sprite(int id, int[] data = null)
        {
            return _gameChip.Sprite(id, data);
        }

        /// <summary>
        ///     Helper utility for flipping pixel data.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="flipH"></param>
        /// <param name="flipV"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public int[] FlipPixelData(int[] data, bool flipH = false, bool flipV = false, int width = 8, int height = 8)
        {
            Utilities.FlipPixelData(ref data, width, height, flipH, flipV);

            return data;
        }

        //        public int[] Sprites(int[] ids, int width)
        //        {
        //            return gameChip.Sprites(ids, width);
        //        }

        public int TotalSprites(bool ignoreEmpty = false)
        {
            return _gameChip.TotalSprites(ignoreEmpty);
        }

        public int Flag(int column, int row, byte? value = null)
        {
            return _gameChip.Flag(column, row, value);
        }

        public TileData Tile(int column, int row, int? spriteID = null, int? colorOffset = null,
            byte? flag = null)
        {
            var tileData = _gameChip.Tile(column, row, spriteID, colorOffset, flag);

            // RenderTile(tileData, column, row);

            return tileData;
        }

        // public void RebuildTilemap()
        // {
        //     gameChip.RebuildTilemap();
        // }

        public Point TilemapSize(int? width = null, int? height = null, bool clear = false)
        {
            return _gameChip.TilemapSize(width, height, clear);
        }

        public void UpdateTiles(int[] ids, int? colorOffset = null, byte? flag = null)
        {
            _gameChip.UpdateTiles(ids, colorOffset, flag);
        }

        /// <summary>
        ///     This allows you to fill a tilemap with a specific tile ID. It's used in the Tilemap editor. Define the position you
        ///     want to start at and any tile matching that ID touching will be replaced.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="column"></param>
        /// <param name="row"></param>
        /// <param name="mode"></param>
        /// <param name="scaleX"></param>
        /// <param name="scaleY"></param>
        /// <param name="colorOffset"></param>
        public void FloodFillTilemap(int value, int column, int row, int mode = 0, int scaleX = 1, int scaleY = 1,
            int colorOffset = -1)
        {
            var canvas = NewCanvas(tilemapChip.Columns, tilemapChip.Rows);

            var tiles = tilemapChip.tiles;

            var total = canvas.Pixels.Length;

            // Loop through all the tiles and build the pixels
            for (var i = 0; i < total; i++) canvas.Pixels[i] = mode == 0 ? tiles[i].SpriteId : tiles[i].Flag;

            if (mode == 0)
            {
                // Build the pattern
                var size = scaleX * scaleY;
                var pattern = new int[size];
                var spriteCols = 16;

                var spritePos = value == -1 ? new Point(-1, -1) : _gameChip.CalculatePosition(value, spriteCols);

                for (var i = 0; i < size; i++)
                {
                    var offset = _gameChip.CalculatePosition(i, scaleX);

                    // TODO this is not right
                    offset.X += spritePos.X;
                    offset.Y += spritePos.Y;
                    pattern[i] = value == -1
                        ? value
                        : (_gameChip.CalculateIndex(offset.X, offset.Y, spriteCols) + 1) * -100;
                }

                canvas.SetPattern(pattern, scaleX, scaleY);
            }
            else if (mode == 1)
            {
                canvas.SetPattern(new[] {value}, 1, 1);
            }

            canvas.FloodFill(column, row);

            // Copy the pixel data back to the tilemap
            for (var i = 0; i < total; i++)
                if (mode == 0)
                {
                    var tile = tiles[i];

                    if (canvas.Pixels[i] < -1)
                    {
                        var pixel = canvas.Pixels[i] / -100 - 1;

                        tile.SpriteId = pixel;

                        if (colorOffset > -1) tile.ColorOffset = colorOffset;
                    }
                }
                else if (mode == 1)
                {
                    tiles[i].Flag = unchecked((byte) canvas.Pixels[i]);
                }

            tilemapChip.Invalidate();
        }


        public int[] ConvertTextToSprites(string text, string fontName = "default")
        {
            throw new NotImplementedException();
        }

        public int[] ConvertCharacterToPixelData(char character, string fontName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Exports a song to a wav file.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="id"></param>
        public void ExportSong(string path, int id)
        {
            workspace.ExportSong(path, musicChip, soundChip, id);
        }

        public void ExportPattern(WorkspacePath path, int id)
        {
            workspace.ExportPattern(path, musicChip, soundChip, id);
        }

        public bool RunBackgroundScript(string scriptName, string[] args = null)
        {
            try
            {
                // filePath = UniqueFilePath(filePath.AppendFile("pattern+" + id + ".wav"));

                // TODO exporting sprites doesn't work
                if (serviceManager.GetService(typeof(GameDataExportService).FullName) is GameDataExportService
                    exportService)
                {
                    exportService.Restart();

                    exportService.AddExporter(new BackgroundScriptRunner(scriptName,
                        serviceManager.GetService(typeof(LuaService).FullName) as LuaService, args));
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

        /// <summary>
        ///     Change the value if unique sprites are loaded in
        /// </summary>
        /// <param name="flag"></param>
        /// <returns></returns>
        public bool UniqueSprites(bool? flag = null)
        {
            if (flag.HasValue) spriteChip.Unique = flag.Value;

            return spriteChip.Unique;
        }

        public string[] Colors()
        {
            return colorChip.HexColors;
        }

        /// <summary>
        ///     Change the flag for importing tiles if they are not found in the Sprite Chip
        /// </summary>
        /// <param name="flag"></param>
        /// <returns></returns>
        public bool ImportTiles(bool? flag = null)
        {
            if (flag.HasValue) tilemapChip.autoImport = flag.Value;

            return tilemapChip.autoImport;
        }

        /// <summary>
        ///     Toggle the flag for debug color allowing the mask color to be shown instead of defaulting to the background color.
        /// </summary>
        /// <param name="flag"></param>
        /// <returns></returns>
        public bool DebugColor(bool? flag = null)
        {
            if (flag.HasValue) colorChip.DebugMode = flag.Value;

            return colorChip.DebugMode;
        }

        /// <summary>
        ///     Change the color mode of the game to edit specific color spaces in the Color Chip.
        /// </summary>
        /// <param name="mode"></param>
        // public void ChangeColorMode(int mode = 0)
        // {
        //     if (mode == 1)
        //     {
        //         // Create a new color map chip based on the exsiting color chip 
        //         if (targetGame.GetChip(ColorMapParser.chipName, false) == null)
        //         {
        //             // Since we need pagincation and other values only on the color chip we'll create one here
        //             // Create new color map chip
        //             var colorMapChip = new ColorChip();
        //
        //             // Add the chip to the engine
        //             targetGame.ActivateChip(ColorMapParser.chipName, colorMapChip, false);
        //
        //             // Register the temporary color chip as a ColorMapChip
        //             //                    targetGame.chipManager.ActivateChip(typeof(ColorMapChip).FullName, colorMapChip, false);
        //
        //             var colors = colorChip.hexColors;
        //
        //             colorMapChip.total = colors.Length;
        //
        //             for (var i = 0; i < colors.Length; i++) colorMapChip.UpdateColorAt(i, colors[i]);
        //
        //             //                    Debug.Log("Create New Color Map Chip");
        //         }
        //
        //         //                Debug.Log("Color Map Chip Exists " + (targetGame.colorMapChip != null));
        //
        //         // Since we are using a color chip we need to make sure we call the rigt chip because its not registered with the engine
        //         activeColorChip = targetGame.GetChip(ColorMapParser.chipName, false) as ColorChip;
        //     }
        //     //            else if (mode == 2)
        //     //            {
        //     //                activeColorChip = targetGame.GetChip(FlagColorParser.FlagColorChipName, false) as ColorChip;
        //     //            }
        //     else
        //     {
        //         activeColorChip = targetGame.ColorChip;
        //     }
        // }


        /// <summary>
        ///     This will go through the Sprite Chip memory and remove any duplicate sprites it finds.
        /// </summary>
        // public void OptimizeSprites()
        // {
        //     //            Console.WriteLine("Optimize sprites " + spriteChip.width + " " + spriteChip.height);
        //
        //     var tmpSpriteChip = new SpriteChip { width = 8, height = 8 };
        //
        //
        //     tmpSpriteChip.Resize(spriteChip.textureWidth, spriteChip.textureHeight);
        //
        //     // Loop through all the sprites and copy them to the new chip
        //     var total = spriteChip.TotalSprites;
        //
        //     var tmpPixelData = new int[8 * 8];
        //     var nextSpriteID = 0;
        //     
        //     // Copy the sprites to the temp chip
        //     for (var i = 0; i < total; i++)
        //     {
        //         spriteChip.ReadSpriteAt(i, ref tmpPixelData);
        //
        //         if (tmpSpriteChip.FindSprite(tmpPixelData) == -1)
        //         {
        //             tmpSpriteChip.UpdateSpriteAt(nextSpriteID, tmpPixelData);
        //
        //             nextSpriteID++;
        //         }
        //     }
        //
        //     spriteChip.Clear();
        //
        //     total = tmpSpriteChip.SpritesInMemory;
        //
        //     for (var i = 0; i < total; i++)
        //     {
        //         tmpSpriteChip.ReadSpriteAt(i, ref tmpPixelData);
        //         spriteChip.UpdateSpriteAt(i, tmpPixelData);
        //     }
        //
        // }

        /// <summary>
        ///     Returns the TotalDisks number of sprites in memory.
        /// </summary>
        /// <returns></returns>
        public int SpritesInRam()
        {
            return spriteChip.SpritesInMemory;
        }

        /// <summary>
        ///     Loads an image from a path in the Workspace.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        // public bool LoadImage(string path)
        // {
        //     // Convert to a system path
        //     var filePath = WorkspacePath.Parse(path);
        //
        //     // If the file doesn't exist, return false.
        //     if (!workspace.Exists(filePath)) return false;
        //
        //     byte[] imageBytes = null;
        //
        //     try
        //     {
        //         // Read bytes from image file
        //         using (var memoryStream = new MemoryStream())
        //         {
        //             workspace.OpenFile(filePath, FileAccess.Read).CopyTo(memoryStream);
        //
        //             imageBytes = memoryStream.ToArray();
        //         }
        //     }
        //     catch
        //     {
        //         runner.DisplayWarning("Unable to read image file.");
        //     }
        //
        //     try
        //     {
        //         // var saveFlags = BuildSaveFlags(new[] { SaveFlags.Colors, SaveFlags.Tilemap });
        //
        //         // var files = new Dictionary<string, byte[]>
        //         // {
        //         //     {"colors.png", imageBytes},
        //         //     {"tilemap.png", imageBytes}
        //         // };
        //
        //         // We only need a few chips to make this work
        //         string[] chips =
        //         {
        //             typeof(ColorChip).FullName,
        //             typeof(SpriteChip).FullName,
        //             typeof(TilemapChip).FullName,
        //             typeof(DisplayChip).FullName,
        //             typeof(GameChip).FullName
        //         };
        //         
        //
        //         targetGame = new PixelVisionEngine(serviceManager, chips)
        //         {
        //             ColorChip = {unique = true},
        //             SpriteChip = {unique = true, colorsPerSprite = 16, pages = 8},
        //             TilemapChip = {autoImport = true}
        //         };
        //
        //         targetGame.ColorChip.Clear();
        //
        //         // var tmpParser = new PNGReader(imageBytes);
        //         var pngReader = new PNGReader(imageBytes, targetGame.ColorChip.maskColor);
        //         
        //         // Resize the tilemap
        //         targetGame.TilemapChip.Resize(pngReader.width / 8, pngReader.height / 8);
        //
        //         var loadService = runner.loadService;
        //
        //         loadService.Reset();
        //
        //         loadService.targetEngine = targetGame;
        //
        //         loadService.AddParser(new ColorParser(pngReader, targetGame.ColorChip));
        //         loadService.AddParser(new TilemapParser(pngReader, targetGame));
        //
        //         loadService.LoadAll();
        //
        //     }
        //     catch
        //     {
        //         //                Console.WriteLine("Game Editor Load Error:\n"+e.Message);
        //
        //         return false;
        //     }
        //
        //
        //     Reset();
        //
        //     return true;
        // }

        /// <summary>
        ///     Load a font from a path in the Workspace
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool LoadFont(string path)
        {
            // Convert to a system path
            var filePath = WorkspacePath.Parse(path);

            // If the file doesn't exist, return false.
            if (!workspace.Exists(filePath)) return false;

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
                // We only need a few chips to make this work
                string[] chips =
                {
                    typeof(ColorChip).FullName,
                    typeof(SpriteChip).FullName,
                    typeof(FontChip).FullName,
                    typeof(TilemapChip).FullName,
                    typeof(DisplayChip).FullName,
                    typeof(GameChip).FullName
                };

                targetGame = new PixelVision(chips, "GameEditor")
                {
                    ServiceLocator = serviceManager,
                    FontChip = {Unique = false, Pages = 1},
                    Name = path
                };

                var pngReader = new PNGReader(imageBytes);
                // { FileName = filePath.EntityName };

                var loadService = runner.loadService;

                loadService.Reset();

                loadService.targetEngine = targetGame;

                loadService.AddParser(new FontParser("", pngReader, targetGame.ColorChip, targetGame.FontChip));

                loadService.LoadAll();
            }
            catch
            {
                return false;
            }

            Reset();

            return true;
        }

        /// <summary>
        ///     Change a font's sprite data
        /// </summary>
        /// <param name="id"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public int[] FontSprite(int id, int[] data = null)
        {
            if (data != null)
            {
                fontChip.UpdateSpriteAt(id, data);

                return data;
            }

            var tmpSpriteData = new int[64];

            fontChip.ReadSpriteAt(id, ref tmpSpriteData);

            return tmpSpriteData;
        }

        /// <summary>
        ///     Save a font
        /// </summary>
        /// <param name="fontName"></param>
        /// <param name="oldName"></param>
        public string SaveFont(string fontName)
        {
            var engineName = targetGame.Name;

            var oldName = fontChip.Fonts.Keys.First();

            var parentFilePath = WorkspacePath.Parse(engineName).ParentPath;

            var fontPath = parentFilePath.AppendFile(fontName + ".font.png");

            if (fontName != oldName)
            {
                var oldPath = parentFilePath.AppendFile(oldName + ".font.png");

                if (workspace.Exists(oldPath)) workspace.Delete(oldPath);

                var value = fontChip.Fonts[oldName];
                fontChip.Fonts.Remove(oldName);

                fontPath = workspace.UniqueFilePath(fontPath);

                fontName = fontPath.EntityName.Split('.')[0];

                fontChip.Fonts[fontName] = value;
            }

            //            var fontPath = workspace.UniqueFilePath(parentFilePath.AppendFile(fontName + ".font.png"));

            var pngWriter = new PNGWriter();

            var exporter = new FontExporter(fontPath.EntityName, targetGame, pngWriter);
            exporter.CalculateSteps();

            while (exporter.Completed == false) exporter.NextStep();

            var files = new Dictionary<string, byte[]>
            {
                {fontPath.Path, exporter.Bytes}
            };

            workspace.SaveExporterFiles(files);

            // Return just the name of the font without the extension
            return fontPath.EntityName;
        }

        public string ReadMetadata(string key, string defaultValue = "")
        {
            return targetGame.GetMetadata(key, defaultValue);
        }

        public void WriteMetadata(string key, string value)
        {
            targetGame.SetMetadata(key, value);
        }


        #region Sound Editor APIs

        public string Sound(int id, string data = null)
        {
            if (data != null) soundChip.UpdateSound(id, data);
            //
            return soundChip.ReadSound(id).param;
            // return gameChip.Sound(id, data);
        }

        /// <summary>
        ///     Get the channel type or change it.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="typeID"></param>
        /// <returns></returns>
        public int ChannelType(int id, int? typeID = null)
        {
            WaveType? type = null;

            if (typeID.HasValue) type = (WaveType) typeID.Value;

            //            Console.WriteLine("Set Wave Type " + type  + (typeID.HasValue ? " - " + typeID.Value.ToString() : ""));

            return (int) soundChip.ChannelType(id, type);
        }

        #endregion

        #region Colors

        /// <summary>
        ///     Convert sprites in memory to palette colors, clamping the total colors in each sprite to 16
        /// </summary>
        public void ReindexSprites()
        {
            // TODO each sprite needs to be clamped between the max colors per sprite

            // var colorMap = new List<int>();
            //
            // // var total = rawSpriteData.Length;
            //
            // var spritePixelTotal = gameChip.SpriteSize().X * gameChip.SpriteSize().Y;
            //
            // var totalSprites = spriteChip.totalSprites;
            //
            // var tmpPixelData = new int[spritePixelTotal];
            // int pixel,tmpIndex, i, j;
            //
            // // Loop through each sprite
            // for (i = 0; i < totalSprites; i++)
            // {
            //     // Read the sprite data
            //     spriteChip.ReadSpriteAt(i, ref tmpPixelData);
            //
            //     // Clear the color map
            //     //                colorMap.Clear();
            //
            //     // Index the pixel data
            //     for (j = 0; j < spritePixelTotal; j++)
            //     {
            //         pixel = tmpPixelData[j];
            //
            //         if (pixel > -1)
            //         {
            //             tmpIndex = colorMap.IndexOf(pixel);
            //             if (tmpIndex == -1)
            //             {
            //                 colorMap.Add(pixel);
            //                 tmpIndex = colorMap.Count - 1;
            //             }
            //
            //             tmpPixelData[j] = tmpIndex;
            //         }
            //     }
            //
            //     spriteChip.UpdateSpriteAt(i, tmpPixelData);
            // }
            //
            // // Update the CPS to reflect the indexed colors
            // spriteChip.colorsPerSprite = colorMap.Count == 0 ? spriteChip.colorsPerSprite : colorMap.Count;
            //
            // // Copy the colors to the first palette
            // for (i = 0; i < spriteChip.colorsPerSprite; i++)
            //     colorChip.UpdateColorAt(128 + i, colorChip.ReadColorAt(colorMap.Count == 0 ? i : colorMap[i]));
            //
            // // Create the 16 colors the sprites will be remapped to
            // string[] colorMapColors =
            // {
            //     "#000000",
            //     "#111111",
            //     "#222222",
            //     "#333333",
            //     "#444444",
            //     "#555555",
            //     "#666666",
            //     "#777777",
            //     "#888888",
            //     "#999999",
            //     "#AAAAAA",
            //     "#BBBBBB",
            //     "#CCCCCC",
            //     "#DDDDDD",
            //     "#EEEEEE",
            //     "#FFFFFF"
            // };
            //
            // // Set the new color TotalDisks
            // total = colorMapColors.Length;
            //
            // // Create a color map chip
            // var colorMapChip = new ColorChip { total = colorMapColors.Length };
            //
            // // Clear the color map chip and rebuild the pages
            // //            colorMapChip.TotalDisks = TotalDisks;
            // colorMapChip.Clear();
            //
            // // Add the colors to the color map chip
            // for (i = 0; i < total; i++) colorMapChip.UpdateColorAt(i, colorMapColors[i]);
            //
            // //            colorChip.paletteMode = true;
            //
            // // Add the chip to the engine
            // targetGame.ActivateChip(ColorMapParser.chipName, colorMapChip, false);

            // Set the pixels back into the sprite texture
            //            spriteChip.texture.SetPixels(rawSpriteData);
        }

        /// <summary>
        ///     Special method to resize a tool's memory to allow it to store colors for the tool
        ///     and the game itself. Should make the color chip memory store 512 colors
        /// </summary>
        public void ResizeToolColorMemory()
        {
            runner.ActiveEngine.ColorChip.Total = 512;
            runner.ActiveEngine.ColorChip.Invalidate();
        }

        /// <summary>
        ///     Change a game's mask color
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string MaskColor(string value = null)
        {
            if (value != null) colorChip.MaskColor = value;

            return colorChip.MaskColor;
        }

        #endregion


        #region Rendering

        public Dictionary<string, int> TmpPos { get; } = new Dictionary<string, int>
        {
            {"x", 0},
            {"y", 0}
        };

        /// <summary>
        ///     Get or change the TotalDisks sprite pages in memory
        /// </summary>
        /// <param name="total"></param>
        /// <returns></returns>
        public int SpritePages(int? total = null)
        {
            if (total.HasValue) spriteChip.Pages = total.Value;

            return spriteChip.Pages;
        }

        /// <summary>
        ///     Get or change the colors per sprite
        /// </summary>
        /// <param name="total"></param>
        /// <returns></returns>
        public int ColorsPerSprite(int? total)
        {
            // 
            if (total.HasValue) spriteChip.ColorsPerSprite = total.Value;

            return spriteChip.ColorsPerSprite;
        }

        /// <summary>
        /// </summary>
        /// <param name="total"></param>
        /// <returns></returns>
        public int DrawCalls(int? total = null)
        {
            if (total.HasValue) spriteChip.MaxSpriteCount = total.Value;

            return spriteChip.MaxSpriteCount;
        }

        #endregion

        #region Sounds

        public void PlaySound(int id, int channel = 0)
        {
            _gameChip.PlaySound(id, channel);
        }

        public bool IsChannelPlaying(int channel = 0)
        {
            return _gameChip.IsChannelPlaying(channel);
        }

        public void StopSound(int channel = 0)
        {
            _gameChip.StopSound(channel);
        }

        public void DrawSpriteBlock(int id, int x, int y, int width = 1, int height = 1, bool flipH = false,
            bool flipV = false,
            DrawMode drawMode = DrawMode.Sprite, int colorOffset = 0, bool onScreen = true, bool useScrollPos = true,
            Rectangle? bounds = null)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Get or change the TotalDisks number of sounds
        /// </summary>
        /// <param name="total"></param>
        /// <returns></returns>
        public int TotalSounds(int? total = null)
        {
            if (total.HasValue) soundChip.TotalSounds = total.Value;

            return soundChip.TotalSounds;
        }

        /// <summary>
        ///     Get or change the TotalDisks channels
        /// </summary>
        /// <param name="total"></param>
        /// <returns></returns>
        public int TotalChannels(int? total = null)
        {
            if (total.HasValue) soundChip.TotalChannels = total.Value;

            return soundChip.TotalChannels;
        }

        /// <summary>
        ///     Create a new sound from a template at a specific position in the sound chip's memory
        /// </summary>
        /// <param name="index"></param>
        /// <param name="template"></param>
        public void GenerateSound(int index, int template)
        {
            // Create a tmp synth parameter
            var settings = new SfxrSynthChannel().parameters;

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

            soundChip.UpdateSound(index, settings.GetSettingsString());
        }

        /// <summary>
        ///     Mutate a sound at a specific ID
        /// </summary>
        /// <param name="id"></param>
        public void Mutate(int id)
        {
            var param = new SfxrParams();
            param.SetSettingsString(soundChip.ReadSound(id).param);
            param.Mutate();
            soundChip.UpdateSound(id, param.GetSettingsString());
            //            soundChip.ReadSound(id).Mutate();
        }

        /// <summary>
        ///     Create a new sound and clear an existing one.
        /// </summary>
        /// <param name="id"></param>
        public void NewSound(int id)
        {
            //            var settings = new SfxrSynth().parameters;
            // TODO I don't like that this is a static value on the SoundData class
            soundChip.UpdateSound(id, ""); //settings.GetSettingsString());
        }

        /// <summary>
        ///     Get or change the sound label
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public string SoundLabel(int index, string value = null)
        {
            if (value != null)
            {
                soundChip.UpdateLabel(index, value);

                // soundChip.RefreshSamples();
            }

            return soundChip.ReadLabel(index);
        }

        #endregion

        #region Music

        /// <summary>
        ///     Get or change the TotalDisks tracks
        /// </summary>
        /// <param name="total"></param>
        /// <returns></returns>
        public int TotalTracks(int? total)
        {
            //            if (TotalDisks.HasValue) musicChip.totalTracks = TotalDisks.Value;

            return musicChip.totalTracks;
        }

        /// <summary>
        ///     Get or change the TotalDisks loops
        /// </summary>
        /// <param name="total"></param>
        /// <returns></returns>
        public int TotalLoops(int? total)
        {
            if (total.HasValue) musicChip.TotalLoops = total.Value;

            return musicChip.TotalLoops;
        }

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
        ///     Check to see if a sound is a wav file being read form the workspace.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool IsWav(int id)
        {
            return soundChip.ReadSound(id).isWav;
        }

        /// <summary>
        ///     Modify a note
        /// </summary>
        /// <param name="track"></param>
        /// <param name="beat"></param>
        /// <param name="note"></param>
        /// <returns></returns>
        public int Note(int track, int beat, int? note)
        {
            if (track < 0 || track >= musicChip.totalTracks) return 0;

            var notes = musicChip.ActiveTrackerData.tracks[track].notes;

            if (beat > notes.Length || beat < 0) return 0;

            if (note.HasValue) notes[beat] = note.Value;

            return notes[beat];
        }

        /// <summary>
        ///     Get or change the tempo value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public int Tempo(int? value)
        {
            if (value.HasValue)
            {
                musicChip.ActiveTrackerData.speedInBPM = value.Value;
                musicChip.UpdateNoteTickLengths();
            }

            return musicChip.ActiveTrackerData.speedInBPM;

            //workspace.InvalidateSave();
        }

        /// <summary>
        ///     Mute a track
        /// </summary>
        /// <param name="track"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool MuteTrack(int track, bool? value = null)
        {
            if (track >= musicChip.ActiveTrackerData.tracks.Length) return false;

            if (value.HasValue) musicChip.ActiveTrackerData.tracks[track].mute = value.Value;

            return musicChip.ActiveTrackerData.tracks[track].mute;
        }

        /// <summary>
        ///     Change a track's SFX ID in the song generator
        /// </summary>
        /// <param name="track"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public int ConfigTrackSFX(int track, int? id)
        {
            if (id.HasValue) songGenerator.trackSettings[track].SfxId = id.Value;

            return songGenerator.trackSettings[track].SfxId;
        }

        /// <summary>
        ///     Configure a track's instrument in the song generator
        /// </summary>
        /// <param name="track"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public int ConfigTrackInstrument(int track, int? id)
        {
            if (id.HasValue) songGenerator.trackSettings[track].InstrumentType = (InstrumentType) id.Value;

            return (int) songGenerator.trackSettings[track].InstrumentType;
        }

        /// <summary>
        ///     Change the track octave in the song generator
        /// </summary>
        /// <param name="track"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public Point ConfigTrackOctaveRange(int track, Point? range = null)
        {
            if (range.HasValue) songGenerator.trackSettings[track].OctaveRange = range.Value;

            return songGenerator.trackSettings[track].OctaveRange;
        }

        /// <summary>
        ///     Change the track instrument in the song generator
        /// </summary>
        /// <param name="track"></param>
        /// <param name="sfxID"></param>
        /// <returns></returns>
        public int TrackInstrument(int track, int? sfxID)
        {
            if (sfxID.HasValue)
                musicChip.ActiveTrackerData.tracks[track].sfxID =
                    MathHelper.Clamp(sfxID.Value, 0, soundChip.TotalSounds);

            return musicChip.ActiveTrackerData.tracks[track].sfxID;
        }

        /// <summary>
        ///     Get or change the notes per track
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public int NotesPerTrack(int? value = null)
        {
            if (value.HasValue) musicChip.NotesPerTrack = 32;

            return musicChip.NotesPerTrack;
        }

        /// <summary>
        ///     Play instrument
        /// </summary>
        /// <param name="id"></param>
        public void PreviewInstrument(int id)
        {
            // Just need to get a reference to any track setting for this data
            var soundData = songGenerator.trackSettings[0].ReadInstrumentSoundData(id);

            if (soundData != null) soundChip.PlayRawSound(soundData);
        }

        /// <summary>
        ///     Configures the music generator
        /// </summary>
        public void ConfigureGenerator()
        {
            songGenerator.ConfigureGenerator(musicChip.totalTracks);
        }


        /// <summary>
        ///     Generate a new song
        /// </summary>
        public void GenerateSong()
        {
            songGenerator.GenerateSong(targetGame);
        }

        /// <summary>
        ///     Change the octave range in the song generator
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public Point OctaveRange(int? min, int? max)
        {
            if (min.HasValue) songGenerator.octaveRange.X = min.Value;

            if (max.HasValue) songGenerator.octaveRange.Y = max.Value;

            return songGenerator.octaveRange;
        }

        /// <summary>
        ///     Manually update the sequencer
        /// </summary>
        /// <param name="timeDelta"></param>
        public void UpdateSequencer(int timeDelta)
        {
            musicChip.Update(timeDelta);
        }

        /// <summary>
        ///     Manually start the sequencer
        /// </summary>
        /// <param name="loop"></param>
        public void StartSequencer(bool loop = false)
        {
            LoopSong(loop);
            musicChip.songCurrentlyPlaying = true;
        }

        /// <summary>
        ///     Loop the current song
        /// </summary>
        /// <param name="value"></param>
        public void LoopSong(bool value)
        {
            musicChip.loopSong = value;
        }

        /// <summary>
        ///     Manually stop the sequencer
        /// </summary>
        public void StopSequencer()
        {
            musicChip.songCurrentlyPlaying = false;
        }

        public bool SongPlaying()
        {
            return musicChip.songCurrentlyPlaying;
        }

        public int CurrentPattern()
        {
            return musicChip.currentPattern;
        }

        public Dictionary<string, int> ReadSongData()
        {
            return musicChip.songData;
        }

        /// <summary>
        ///     Get the current beat value or change it
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public int CurrentBeat(int? pos)
        {
            if (pos.HasValue) musicChip.SequencerBeatNumber = pos.Value;

            return musicChip.SequencerBeatNumber;
        }

        /// <summary>
        ///     Manually reset the tracker
        /// </summary>
        public void ResetSong()
        {
            musicChip.ResetTracker();
        }

        /// <summary>
        ///     Play a note
        /// </summary>
        /// <param name="track"></param>
        /// <param name="beat"></param>
        public void PlayNote(int track, int beat)
        {
            //            musicChip.PlayNote(track, beat);

            var sfxID = songGenerator.trackSettings[track].SfxId;

            // play the sound
            soundChip.ReadSound(sfxID);

            soundChip.PlaySound(sfxID, track, musicChip.noteStartFrequency[beat]);
        }

        /// <summary>
        ///     Return the instrument ID in the song generator
        /// </summary>
        /// <param name="track"></param>
        /// <returns></returns>
        public int ReadInstrumentID(int track)
        {
            return (int) songGenerator.trackSettings[track].InstrumentType;
        }

        /// <summary>
        /// </summary>
        public int PcgDensity
        {
            get => songGenerator.pcgDensity;
            set => songGenerator.pcgDensity = value;
        }

        /// <summary>
        /// </summary>
        public int PcgFunk
        {
            get => songGenerator.pcgFunk;
            set => songGenerator.pcgFunk = value;
        }

        /// <summary>
        /// </summary>
        public int PcgLayering
        {
            get => songGenerator.pcgLayering;
            set => songGenerator.pcgLayering = value;
        }

        /// <summary>
        /// </summary>
        public int PcgMinTempo
        {
            get => songGenerator.pcgMinTempo;
            set => songGenerator.pcgMinTempo = value;
        }

        /// <summary>
        /// </summary>
        public int PcgMaxTempo
        {
            get => songGenerator.pcgMaxTempo;
            set => songGenerator.pcgMaxTempo = value;
        }

        /// <summary>
        /// </summary>
        public int Scale
        {
            get => songGenerator.scale;
            set => songGenerator.scale = value;
        }

        /// <summary>
        /// </summary>
        /// <param name="track"></param>
        /// <param name="instrument"></param>
        /// <param name="sfxID"></param>
        public void SetTrack(int track, int instrument, int sfxID)
        {
            songGenerator.trackSettings[track].InstrumentType = (InstrumentType) instrument;
            songGenerator.trackSettings[track].SfxId = sfxID;
        }

        #endregion


        #region Sprites

        /// <summary>
        ///     Read game sprite pixel data directly from the sprite chip's memory
        /// </summary>
        /// <param name="id"></param>
        /// <param name="flipH"></param>
        /// <param name="flipV"></param>
        /// <returns></returns>
        public int[] ReadGameSpriteData(int id, int scaleX = 1, int scaleY = 1, bool flipH = false, bool flipV = false)
        {
            return ReadSpriteData(spriteChip, id, scaleX, scaleY, flipH, flipV);
        }

        /// <summary>
        ///     Read tool sprite pixel data directly from the sprite chip's memory
        /// </summary>
        /// <param name="id"></param>
        /// <param name="scaleX"></param>
        /// <param name="scaleY"></param>
        /// <param name="flipH"></param>
        /// <param name="flipV"></param>
        /// <returns></returns>
        public int[] ReadToolSpriteData(int id, int scaleX = 1, int scaleY = 1, bool flipH = false, bool flipV = false)
        {
            return ReadSpriteData(runner.ActiveEngine.SpriteChip, id, scaleX, scaleY, flipH, flipV);
        }

        private int[] ReadSpriteData(SpriteChip spriteChip, int id, int scaleX = 1, int scaleY = 1, bool flipH = false,
            bool flipV = false)
        {
            //            var scale = 1;

            var blockSizeX = scaleX * SpriteChip.DefaultSpriteSize;
            var blockSizeY = scaleY * SpriteChip.DefaultSpriteSize;

            // var pixelData = new int[blockSizeX * blockSizeY];

            var pos = _gameChip.CalculatePosition(id, spriteChip.TextureWidth / SpriteChip.DefaultSpriteSize);

            var pixelData = Utilities.GetPixels(spriteChip.PixelData, pos.X * 8, pos.Y * 8, blockSizeX, blockSizeY);
            // spriteChip.texture.CopyPixels(ref pixelData, pos.X * 8, pos.Y * 8, blockSizeX, blockSizeY);

            //            var pixelData = Sprite(id);

            if (flipH || flipV) Utilities.FlipPixelData(ref pixelData, blockSizeX, blockSizeY, flipH, flipV);

            return pixelData;
        }

        /// <summary>
        ///     Write sprite data directly into the Sprite Chip's memory
        /// </summary>
        /// <param name="id"></param>
        /// <param name="flipH"></param>
        /// <param name="flipV"></param>
        public void WriteSpriteData(int id, int[] pixelData, int scaleX = 1, int scaleY = 1)
        {
            var blockSizeX = scaleX * SpriteChip.DefaultSpriteSize;
            var blockSizeY = scaleY * SpriteChip.DefaultSpriteSize;

            var pos = _gameChip.CalculatePosition(id, spriteChip.TextureWidth / SpriteChip.DefaultSpriteSize);

            //            var pos = gameChip.CalculatePosition(id, spriteChip.textureWidth / spriteChip.width);

            //            Console.WriteLine("Write sprite " + pos.X +" "+pos.Y);
            // spriteChip.texture.SetPixels(pos.X * 8, pos.Y * 8, blockSizeX, blockSizeY, pixelData);
            Utilities.SetPixels(pixelData, pos.X * 8, pos.Y * 8, blockSizeX, blockSizeY, spriteChip.PixelData);

            // TODO need to invalidate the cached sprite data


            //            spriteChip.texture.CopyPixels(ref pixelData, pos.X * 8, pos.Y * 8, blockSize, blockSize);
            //            
            ////            var pixelData = Sprite(id);
            //
            //            if (flipH || flipV)
            //                SpriteChipUtil.FlipSpriteData(ref pixelData, blockSize, blockSize, flipH, flipV);

            // TODO need to invalidate all the sprites that are within the bounds of the update

            // TODO need to make sure the sprite data is flipped correctly before being saved
        }

        /// <summary>
        ///     Run the sprite builder
        /// </summary>
        /// <returns></returns>
        public int RunSpriteBuilder(string path)
        {
            // Make sure we are editing a game
            if (targetGame == null) return -1;


            // Generate the sprites
            return workspace.GenerateSprites(path, targetGame);
        }

        #endregion

        #region Render Tilemap Layer

        /// <summary>
        ///     Create a new Canvas using the edited game as a source
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public Canvas NewCanvas(int width, int height)
        {
            return new Canvas(width, height, _gameChip);
        }

        // private Canvas tmpCanvas;
        // public bool RenderingMap { get; private set; }
        // private int[] tmpPixelData = new int[0];
        // private int totalTiles;
        // private int totalLoops;
        // private readonly int maxPerLoop = 100;
        // private int currentLoop;
        //
        // private readonly Canvas[] layerCache = new Canvas[2];
        private SfxrMusicGeneratorChip songGenerator;
        //
        // public int RenderPercent => MathHelper.Clamp((int)(currentLoop / (float)totalLoops * 100), 0, 100);
        //
        // /// <summary>
        // ///     Draws the map to a layer
        // /// </summary>
        // /// <param name="mode"></param>
        // public void RenderMapLayer(int mode)
        // {
        //     var realWidth = spriteChip.width * tilemapChip.columns;
        //     var realHeight = spriteChip.height * tilemapChip.rows;
        //
        //
        //     if (layerCache[mode] == null)
        //     {
        //         layerCache[mode] = new Canvas(realWidth, realHeight, gameChip);
        //         RenderingMap = true;
        //     }
        //
        //     // Set the tmpCanvas to the cache
        //     tmpCanvas = layerCache[mode];
        //
        //     // Rebuild the map if it hasn't been rendered yet.
        //     if (RenderingMap)
        //     {
        //         tmpCanvas.Clear(mode == 0 ? colorChip.backgroundColor : -2);
        //
        //         totalTiles = tilemapChip.total;
        //
        //         Array.Resize(ref tmpPixelData, spriteChip.width * spriteChip.height);
        //
        //         RenderingMap = true;
        //
        //         totalLoops = MathUtil.CeilToInt(tilemapChip.total / (float)maxPerLoop);
        //
        //         currentLoop = 0;
        //     }
        // }
        //
        // /// <summary>
        // ///     Moves to the next render step
        // /// </summary>
        // public void NextRenderStep()
        // {
        //     var offset = currentLoop * maxPerLoop;
        //
        //     for (var i = 0; i < maxPerLoop; i++)
        //     {
        //         var index = i + offset;
        //         if (index >= totalTiles)
        //         {
        //             RenderingMap = false;
        //             break;
        //         }
        //
        //         var pos = gameChip.CalculatePosition(index, tilemapChip.columns);
        //
        //         var tileData = gameChip.Tile(pos.X, pos.Y);
        //
        //         RenderTile(tileData, pos.X, pos.Y);
        //     }
        //
        //     currentLoop++;
        // }
        //
        // /// <summary>
        // ///     Draw a specific tile
        // /// </summary>
        // /// <param name="tileData"></param>
        // /// <param name="col"></param>
        // /// <param name="row"></param>
        // private void RenderTile(TileData tileData, int col, int row)
        // {
        //     int[] spriteData;
        //     int[] flagData;
        //
        //     col *= spriteChip.width;
        //     row *= spriteChip.height;
        //
        //     var totalPixels = spriteChip.width * spriteChip.height;
        //
        //     if (layerCache[0] != null)
        //     {
        //         // 
        //
        //         if (tileData.SpriteId == -1)
        //             spriteData = Enumerable.Repeat(-1, totalPixels).ToArray();
        //         else
        //             spriteData = gameChip.Sprite(tileData.SpriteId);
        //
        //         // Shift the pixel data
        //         for (var j = 0; j < spriteData.Length; j++) spriteData[j] += tileData.ColorOffset;
        //
        //         //
        //         layerCache[0].SetPixels(col, row, spriteChip.width, spriteChip.height, spriteData);
        //     }
        //
        //     if (layerCache[1] != null)
        //     {
        //         flagData = Enumerable.Repeat((int)tileData.Flag, spriteChip.width * spriteChip.height).ToArray();
        //
        //         layerCache[1].SetPixels(col, row, spriteChip.width, spriteChip.height, flagData);
        //     }
        // }

        /// <summary>
        ///     Fast copy of render to the display
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="colorOffset"></param>
        /// <param name="maskColor"></param>
        // public void CopyRenderToDisplay(int x, int y, int width, int height, int colorOffset, int maskColor = -1)
        // {
        //     // Only render when a canvas exists
        //     if (tmpCanvas == null) return;
        //
        //     // Should have some kind of invalidation test
        //
        //     // Get scroll position for tmpX/Y position
        //     var scrollPos = gameChip.ScrollPosition();
        //     var tmpX = scrollPos.X;
        //     var tmpY = scrollPos.Y;
        //
        //     // Copy the pixels from the canvas
        //     tmpCanvas.CopyPixels(ref tmpPixelData, tmpX, tmpY, width, height);
        //
        //     //            if (useBGColor)
        //     //            {
        //     var size = tmpPixelData.Length;
        //
        //     // Replace empty colors with the background
        //     for (var i = 0; i < size; i++)
        //         if (tmpPixelData[i] < 0)
        //             tmpPixelData[i] = maskColor; //useBGColor ? colorChip.backgroundColor : -1 - colorOffset;
        //     //            }
        //
        //     // Copy to the active game's tilemap layer
        //     runner.ActiveEngine.GameChip.DrawPixels(tmpPixelData, x, y, width, height, false, false,
        //         DrawMode.TilemapCache, colorOffset);
        // }

        /// <summary>
        ///     Fast canvas copy to the display
        /// </summary>
        /// <param name="srcCanvas"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="colorOffset"></param>
        /// <param name="maskColor"></param>
        /// <param name="offsetX"></param>
        /// <param name="offsetY"></param>
        /// <param name="useScrollPos"></param>
        // public void CopyCanvasToDisplay(Canvas srcCanvas, int x, int y, int width, int height, int colorOffset,
        //     int maskColor = -1, int offsetX = 0, int offsetY = 0, bool useScrollPos = true)
        // {
        //     // Only render when a canvas exists
        //     //            if (tmpCanvas == null)
        //     //                return;
        //
        //     // Should have some kind of invalidation test
        //
        //     // Get scroll position for tmpX/Y position
        //     var scrollPos = gameChip.ScrollPosition();
        //     var tmpX = scrollPos.X + offsetX;
        //     var tmpY = scrollPos.Y + offsetY;
        //
        //     // Copy the pixels from the canvas
        //     srcCanvas.CopyPixels(ref tmpPixelData, tmpX, tmpY, width, height);
        //
        //     //            if (useBGColor)
        //     //            {
        //     var size = tmpPixelData.Length;
        //
        //     // Replace empty colors with the background
        //     for (var i = 0; i < size; i++)
        //         if (tmpPixelData[i] < 0)
        //             tmpPixelData[i] = maskColor; //useBGColor ? colorChip.backgroundColor : -1 - colorOffset;
        //     //            }
        //
        //     // Copy to the active game's tilemap layer
        //     runner.ActiveEngine.GameChip.DrawPixels(tmpPixelData, x, y, width, height, false, false,
        //         DrawMode.TilemapCache, colorOffset);
        // }

        #endregion


        // TODO need a way to reduce the palette for sprites?

        #region Tool APIs

        /// <summary>
        ///     Get the tool's colors from the active engine.
        /// </summary>
        /// <returns></returns>
        public string[] ToolColors()
        {
            return runner.ActiveEngine.ColorChip.HexColors;
        }

        /// <summary>
        ///     Resize the tool's color pages in the active engine
        /// </summary>
        /// <param name="total"></param>
        public void ToolRebuildColorPages(int total)
        {
            runner.ActiveEngine.ColorChip.Total = total;
        }

        /// <summary>
        ///     Export a SFX as a wav
        /// </summary>
        /// <param name="id"></param>
        /// <param name="workspacePath"></param>
        public void ExportSFX(int id, WorkspacePath workspacePath)
        {
            var sfx = soundChip.ReadSound(id);

            workspacePath = workspacePath.AppendFile(sfx.name + ".wav");

            // TODO need to wire this up
            var synth = new SfxrSynthChannel();
            synth.parameters.SetSettingsString(sfx.param);
            //            var stream = workspace.CreateFile(path);

            var files = new Dictionary<string, byte[]>
            {
                {workspacePath.Path, synth.GenerateWav()}
            };

            workspace.SaveExporterFiles(files);
            //            
            //            File.WriteAllBytes(path, synth.GetWavFile());
        }

        #endregion


        // TODO adding some extra APIs to work directly with the chip but maybe these should be opened up?

        public int FindSprite(int[] pixels, bool emptyCheck)
        {
            return spriteChip.FindSprite(pixels, emptyCheck);
        }

        public bool IsSpriteEmpty(int index)
        {
            return spriteChip.IsEmptyAt(index);
        }
    }
}