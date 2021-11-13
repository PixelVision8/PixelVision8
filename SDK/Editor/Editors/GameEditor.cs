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
using PixelVision8.Runner;
using PixelVision8.Workspace;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Buttons = PixelVision8.Player.Buttons;
using Point = PixelVision8.Player.Point;

namespace PixelVision8.Editor
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
        private SoundChip soundChip;
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
        public GameEditor(DesktopRunner runner)
        {
            this.runner = runner;
            this.serviceManager = runner.ServiceManager;
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
                    typeof(SoundChip).FullName,
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

        public void ImportFromCanvas(Canvas canvas, bool resetChips = true)
        {
            
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
            soundChip = targetGame.SoundChip;
            musicChip = targetGame.MusicChip; // TODO need to create a SfxrMusicChip

            //            Console.WriteLine("MC Tracks " + musicChip.totalTracks);
            songGenerator = new SfxrMusicGeneratorChip();

            //            
            //            colorPaletteChip = targetGame.chipManager.GetChip(ColorPaletteParser.chipName, false) as ColorChip;

            // ChangeColorMode();
        }

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

        public void ClearColors() => colorChip.Clear(_gameChip.MaskColor());
        // {
        //     colorChip.Clear();
        // }

        public void ClearSprites() => spriteChip.Clear();
        // {
        //     spriteChip.Clear();
        // }

        /// <summary>
        ///     Get the TotalDisks colors or change the limit for how many colors the color chip can store.
        /// </summary>
        /// <param name="ignoreEmpty"></param>
        /// <param name="total"></param>
        /// <returns></returns>
        public int TotalColors(bool ignoreEmpty = false) => _gameChip.TotalColors(ignoreEmpty);
        // {
            //            if (TotalDisks.HasValue)
            //                activeColorChip.maxColors = TotalDisks.Value;

            // return ignoreEmpty ? colorChip.TotalUsedColors : colorChip.Total;
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

        public Point TilemapSize(int? width = null, int? height = null, bool clear = false)
        {

            var _tilemapSize = new Point(tilemapChip.Columns, tilemapChip.Rows);
            
            var resize = false;

            if (width.HasValue)
            {
                _tilemapSize.X = width.Value;
                resize = true;
            }

            if (height.HasValue)
            {
                _tilemapSize.Y = height.Value;
                resize = true;
            }

            if (resize) tilemapChip.Resize(_tilemapSize.X, _tilemapSize.Y, clear);

            return _tilemapSize;
        }

        public void UpdateTiles(int[] ids, int? colorOffset = null, byte? flag = null)
        {
            _gameChip.UpdateTiles(ids, colorOffset, flag);
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

        public string[] Colors()
        {
            return colorChip.HexColors;
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
        ///     Returns the TotalDisks number of sprites in memory.
        /// </summary>
        /// <returns></returns>
        public int SpritesInRam()
        {
            return spriteChip.SpritesInMemory;
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
        ///     Change a game's mask color
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public string MaskColor(string value = null)// => Constants.MaskColor;// : _gameChip.MaskColor(value);
        {
            if (value != null) _gameChip.MaskColor(value);

            return _gameChip.MaskColor();
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
            var settings = new SoundChannel().parameters;

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

            soundChip.UpdateSound(index, settings.param);
        }

        /// <summary>
        ///     Mutate a sound at a specific ID
        /// </summary>
        /// <param name="id"></param>
        public void Mutate(int id)
        {
            var data = new SoundData("Untitled", soundChip.ReadSound(id).param);
            // param.SetSettingsString();
            data.Mutate();
            soundChip.UpdateSound(id, data.param);
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
        public void GenerateSong(bool resetInstruments = false)
        {
            songGenerator.GenerateSong(targetGame, resetInstruments);
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

        #region Render Tilemap Layer

        private SfxrMusicGeneratorChip songGenerator;
        
        #endregion


        // TODO need a way to reduce the palette for sprites?

        #region Tool APIs

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
            var synth = new SoundChannel();
            synth.parameters.param = sfx.param;
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

    }
}