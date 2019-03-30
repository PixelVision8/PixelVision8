//   
// Copyright (c) Jesse Freeman. All rights reserved.  
//  
// Licensed under the Microsoft Public License (MS-PL) License. 
// See LICENSE file in the project root for full license information. 
// 
// Contributors
// --------------------------------------------------------
// This is the official list of Pixel Vision 8 contributors:
//  
// Jesse Freeman - @JesseFreeman
// Christer Kaitila - @McFunkypants
// Pedro Medeiros - @saint11
// Shawn Rakowski - @shwany

using System.Text.RegularExpressions;
using MoonSharp.Interpreter;
using System;
using Microsoft.Xna.Framework;
using PixelVision8.Runner;
using PixelVision8.Runner.Services;

namespace PixelVision8.Engine.Chips
{

    public class LuaGameChip : GameChip
    {

        //        public Dictionary<string, string> scripts = new Dictionary<string, string>();
//        public Script luaScript { get; protected set; }
        
        protected Script _luaScript;

        public Script luaScript
        {
            get
            {
                if (_luaScript == null)
                {
                    _luaScript = new Script(CoreModules.Preset_SoftSandbox);

//                    ConfigureScript(_luaScript);
                }
                    

                return _luaScript;
            }
        }
        

        #region Lifecycle

        /// <summary>
        ///     Init() is called when a game is loaded into memory and is ready to be played. Use this hook to initialize
        ///     your game's logic. It is only called once. 
        /// </summary>
        public override void Init()
        {
            if (luaScript?.Globals["Init"] == null)
                return;

            luaScript.Call(luaScript.Globals["Init"]);
        }

        public override void Update(float timeDelta)
        {

            base.Update(timeDelta);

            if (luaScript?.Globals["Update"] == null)
                return;

            luaScript.Call(luaScript.Globals["Update"], timeDelta);
        }

        public override void Draw()
        {
            if (luaScript?.Globals["Draw"] == null)
                return;

            luaScript.Call(luaScript.Globals["Draw"]);
        }

        public override void Shutdown()
        {
            if (luaScript?.Globals["Shutdown"] == null)
                return;

            luaScript.Call(luaScript.Globals["Shutdown"]);
        }

        public override void Reset()
        {
            // Setup the GameChip
            base.Reset();

            if (luaScript == null)
                return;

            try
            {
                // Look to see if there are any lua services registered in the game engine
                var luaService = engine.GetService(typeof(LuaService).FullName) as LuaService;

                // Run the lua service to configure the script
                luaService.ConfigureScript(luaScript);
            }
            catch
            {
                // ignored
            }


            #region Color APIs

            luaScript.Globals["BackgroundColor"] = (BackgroundColorDelegate)BackgroundColor;
            luaScript.Globals["Color"] = (ColorDelegate)Color;
            luaScript.Globals["ColorsPerSprite"] = (ColorsPerSpriteDelegate)ColorsPerSprite;
            luaScript.Globals["TotalColors"] = (TotalColorsDelegate)TotalColors;
            luaScript.Globals["ReplaceColor"] = (ReplaceColorDelegate)ReplaceColor;

            #endregion

            #region Display APIs

            luaScript.Globals["Clear"] = (ClearDelegate)Clear;
            luaScript.Globals["Display"] = (DisplayDelegate)Display;
            luaScript.Globals["DrawPixels"] = (DrawPixelsDelegate)DrawPixels;
//            luaScript.Globals["DrawPixel"] = (DrawPixelDelegate)((x, y, colorRef, drawMode) => DrawPixel(x, y, colorRef));
            luaScript.Globals["DrawSprite"] = (DrawSpriteDelegate)DrawSprite;
            luaScript.Globals["DrawSprites"] = (DrawSpritesDelegate)DrawSprites;
            luaScript.Globals["DrawSpriteBlock"] = (DrawSpriteBlockDelegate)DrawSpriteBlock;

//            luaScript.Globals["DrawTile"] = (DrawTileDelegate)DrawTile;
//            luaScript.Globals["DrawTiles"] = (DrawTilesDelegate)DrawTiles;
            luaScript.Globals["DrawText"] = (DrawTextDelegate)DrawText;
            luaScript.Globals["DrawTilemap"] = (DrawTilemapDelegate)DrawTilemap;

            luaScript.Globals["DrawRect"] = (DrawRectDelegate)DrawRect;
            luaScript.Globals["RedrawDisplay"] = (RedrawDisplayDelegate)RedrawDisplay;
            luaScript.Globals["ScrollPosition"] = (ScrollPositionDelegate)((x, y) => ScrollPosition(x, y));

            #endregion

            #region File IO APIs

            luaScript.Globals["LoadScript"] = (LoadScriptDelegate)LoadScript;
            luaScript.Globals["ReadSaveData"] = (ReadDataDelegate)ReadSaveData;
            luaScript.Globals["WriteSaveData"] = (WriteDataDelegate)WriteSaveData;

            #endregion

            #region Input APIs

            luaScript.Globals["Key"] = (KeyDelegate)Key;
            luaScript.Globals["Button"] = (ButtonDelegate)Button;
            luaScript.Globals["MouseButton"] = (MouseButtonDelegate)MouseButton;
            luaScript.Globals["MousePosition"] = (MousePositionDelegate)MousePosition;
            luaScript.Globals["InputString"] = (InputStringDelegate)InputString;

            #endregion

            #region Math APIs

            luaScript.Globals["CalculateIndex"] = (CalculateIndexDelegate)CalculateIndex;
            luaScript.Globals["CalculatePosition"] = (CalculatePositionDelegate)CalculatePosition;
            luaScript.Globals["Clamp"] = (ClampDelegate)Clamp;
            luaScript.Globals["Repeat"] = (RepeatDelegate)Repeat;

            #endregion

            #region Sound APIs

            luaScript.Globals["PlaySound"] = (PlaySoundDelegate)PlaySound;
            luaScript.Globals["PlayRawSound"] = (PlayRawSoundDelegate)PlayRawSound;
            luaScript.Globals["PlayPattern"] = (PlayPatternDelegate)PlayPattern;
            luaScript.Globals["PlayPatterns"] = (PlayPatternsDelegate)PlayPatterns;
            luaScript.Globals["PlaySong"] = (PlaySongDelegate)PlaySong;
            luaScript.Globals["PauseSong"] = (PauseSongDelegate)PauseSong;
            luaScript.Globals["RewindSong"] = (RewindSongDelegate)RewindSong;
            luaScript.Globals["StopSong"] = (StopSongDelegate)StopSong;

            #endregion

            #region Sprite APIs

            luaScript.Globals["Sprite"] = (SpriteDelegate)Sprite;
            luaScript.Globals["Sprites"] = (SpritesDelegate)Sprites;
            luaScript.Globals["SpriteSize"] = (SpriteSizeDelegate)SpriteSize;
            luaScript.Globals["TotalSprites"] = (TotalSpritesDelegate)TotalSprites;

            #endregion

            #region Tilemap

            luaScript.Globals["RebuildTilemap"] = (RebuildMapDelegate)((columns, rows, spriteIDs1, colorOffsets, flags) => RebuildTilemap());
            luaScript.Globals["TilemapSize"] = (TilemapSizeDelegate)TilemapSize;
            luaScript.Globals["Tile"] = (TileDelegate)Tile;
            luaScript.Globals["UpdateTiles"] = (UpdateTilesDelegate)UpdateTiles;
            luaScript.Globals["Flag"] = (FlagDelegate)Flag;
            
            
            luaScript.Globals["SaveTilemapCache"] = new Action(SaveTilemapCache);
            luaScript.Globals["RestoreTilemapCache"] = new Action(RestoreTilemapCache);
            
            #endregion

            #region Utils
            luaScript.Globals["WordWrap"] = (WordWrapDelegate)WordWrap;
            luaScript.Globals["SplitLines"] = (SplitLinesDelegate)SplitLines;
            luaScript.Globals["BitArray"] = (BitArrayDelegate)BitArray;
            luaScript.Globals["CalculateDistance"] = (CalcualteDistanceDelegate)CalculateDistance;

            #endregion

            #region Debug
            luaScript.Globals["ReadFPS"] = new Func<int>(ReadFPS);
            luaScript.Globals["ReadTotalSprites"] = new Func<int>(ReadTotalSprites);
            #endregion

            #region Text

            luaScript.Globals["ConvertTextToSprites"] = (ConvertTextToSpritesDelegate)ConvertTextToSprites;
            luaScript.Globals["ConvertCharacterToPixelData"] = (ConvertCharacterToPixelDataDelegate)ConvertCharacterToPixelData;

            #endregion

            #region Experimental APIs



            luaScript.Globals["PaletteOffset"] = (PaletteOffsetDelegator)PaletteOffset;
            
            
            luaScript.Globals["RegisterMetaSprite"] = (RegisterMetaSpriteDelegator)RegisterMetaSprite;
            luaScript.Globals["DeleteMetaSprite"] = (DeleteMetaSpriteDelegator)DeleteMetaSprite;
            luaScript.Globals["DrawMetaSprite"] = (DrawMetaSpriteDelegator)DrawMetaSprite;
            
            luaScript.Globals["DrawMetaSprite"] = (DrawMetaSpriteDelegator)DrawMetaSprite;
            luaScript.Globals["DrawMetaSprite"] = (DrawMetaSpriteDelegator)DrawMetaSprite;
            
            UserData.RegisterType<MetaSpriteData>();
            luaScript.Globals["MetaSpriteData"] = UserData.CreateStatic<MetaSpriteData>();
            
            UserData.RegisterType<MetaSprite>();
            luaScript.Globals["MetaSprite"] = UserData.CreateStatic<MetaSprite>();

            #endregion

            // Enums
            UserData.RegisterType<DrawMode>();
            luaScript.Globals["DrawMode"] = UserData.CreateStatic<DrawMode>();

            UserData.RegisterType<Buttons>();
            luaScript.Globals["Buttons"] = UserData.CreateStatic<Buttons>();

            UserData.RegisterType<Keys>();
            luaScript.Globals["Keys"] = UserData.CreateStatic<Keys>();

            UserData.RegisterType<InputState>();
            luaScript.Globals["InputState"] = UserData.CreateStatic<InputState>();

            UserData.RegisterType<SaveFlags>();
            luaScript.Globals["SaveFlags"] = UserData.CreateStatic<SaveFlags>();

            // Register PV8's vector type
            UserData.RegisterType<Point>();
            luaScript.Globals["NewPoint"] = (NewPointDelegator)NewPoint;

            // TODO need to depricate this
//            luaScript.Globals["NewVector"] = (NewPointDelegator)NewPoint;


            // Register PV8's TileData type
            UserData.RegisterType<TileData>();

            // Register PV8's rect type
            UserData.RegisterType<Rectangle>();
            luaScript.Globals["NewRect"] = (NewRectDelegator)NewRect;

            //            // Register PV8's rect type
            //            UserData.RegisterType<TextureData>();
            //            luaScript.Globals["NewTextureData"] = (NewTextureDataDelegate)NewTextureData;

            // Register PV8's rect type
            UserData.RegisterType<Canvas>();
            luaScript.Globals["NewCanvas"] = (NewCanvasDelegator)NewCanvas;

            // Load the deafult script
            LoadScript("code.lua");

            // Register any extra services
            RegisterLuaServices();

            // Reset the game
            if (luaScript.Globals["Reset"] != null)
                luaScript.Call(luaScript.Globals["Reset"]);
        }


        /// <summary>
        ///     Defines a meta sprite from a collection of sprites. This API is not working yet.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="flipH"></param>
        /// <param name="flipV"></param>
        /// <param name="colorOffset"></param>
        /// <returns></returns>
        public MetaSpriteData NewMetaSpriteData(int id, bool flipH = false, bool flipV = false, int colorOffset = 0)
        {
            return new MetaSpriteData(id, flipH, flipV, colorOffset);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="width"></param>
        /// <param name="sprites"></param>
        /// <returns></returns>
        public MetaSprite NewMetaSprite(string name, int width, MetaSpriteData[] sprites)
        {
            return new MetaSprite(name, width, sprites);
        }
        
        private delegate MetaSpriteData NewMetaSpriteDataDelegator(int id, bool flipH = false, bool flipV = false, int colorOffset = 0);
        private delegate MetaSprite NewMetaSpriteDelegator(string name, int width, MetaSpriteData[] sprites);


        #endregion


        protected virtual void RegisterLuaServices()
        {
            // Override to add your own Lua services before the game starts
        }

        #region Scripts

        /// <summary>
        ///     This allows you to load a script into memory. External scripts can be located in the System/Libs/,
        ///     Workspace/Libs/ or Workspace/Sandbox/ directory. All scripts, including built-in ones from the Game
        ///     Creator, are accessible via their file name (with or without the extension). You can keep additional
        ///     scripts in your game folder and load them up. Call this method before Init() in your game's Lua file to
        ///     have access to any external code loaded by the Game Creator or Runner.
        /// </summary>
        /// <param name="name">
        ///     Name of the Lua file. You can drop the .lua extension since only Lua files will be accessible to this
        ///     method.
        /// </param>
        public void LoadScript(string name)
        {

            if (!name.EndsWith(".lua"))
                name += ".lua";
            //            var split = name.Split('.');
            //            
            //            if (split.Last() != "lua")
            //                name += ".lua";

            if (textFiles.ContainsKey(name))
            {
                var script = textFiles[name];
                if (script != "")
                {
                    // Patch script to run in vanilla lua vm

                    // Replace short hand math oporators
                    string pattern = @"(\S+)\s*([+\-*/%])\s*=";
                    string replacement = "$1 = $1 $2 ";
                    script = Regex.Replace(script, pattern, replacement, RegexOptions.Multiline);

                    // Replace != conditions
                    pattern = @"!\s*=";
                    replacement = "~=";
                    script = Regex.Replace(script, pattern, replacement, RegexOptions.Multiline);

                    luaScript.DoString(script, null, name);

                }
            }
        }

        /// <summary>
        ///     This allows you to add your Lua scripts at runtime to a game from a string. This could be useful for
        ///     dynamically generating code such as level data or other custom Lua objects in memory. Simply give the
        ///     script a name and pass in a string with valid Lua code. If a script with the same name exists, this will
        ///     override it. Make sure to call LoadScript() after to parse it.
        /// </summary>
        /// <param name="name">Name of the script. This should contain the .lua extension.</param>
        /// <param name="script">The string text representing the Lua script data.</param>
        public void AddScript(string name, string script)
        {

            if (!name.EndsWith(".lua"))
                name += ".lua";

            if (textFiles.ContainsKey(name))
            {
                textFiles[name] = script;
            }
            else
            {
                textFiles.Add(name, script);
            }

        }
        

        #endregion

        #region Sound

        /// <summary>
        ///     This helper method allows you to pass raw SFXR string data to the sound chip for playback. It works just
        ///     like the normal PlaySound() API but accepts a string instead of a sound ID. Calling PlayRawSound() could
        ///     be expensive since the sound effect data is not cached by the engine. It is mostly used for sound effects
        ///     in tools and shouldn't be called when playing a game.
        /// </summary>
        /// <param name="data">Raw string data representing SFXR sound properties in a comma-separated list.</param>
        /// <param name="channel">
        ///     The channel the sound should play back on. Channel 0 is set by default.
        /// </param>
        /// <param name="frequency">
        ///     An optional float argument to change the frequency of the raw sound. The default setting is 0.1266.
        /// </param>
        public void PlayRawSound(string data, int channel = 0, float frequency = 0.1266f)
        {
            var soundChip = engine.soundChip as SfxrSoundChip;

            if (soundChip != null)
            {
                soundChip.PlayRawSound(data, channel, frequency);
            }
        }

        #endregion

        private delegate Rectangle NewRectDelegator(int x = 0, int y = 0, int w = 0, int h = 0);
        private delegate Point NewPointDelegator(int x = 0, int y = 0);

//        private delegate void DrawPixelDelegate(int x, int y, int colorRef, DrawMode drawMode = DrawMode.UI);

        private delegate int BackgroundColorDelegate(int? id = null);

        private delegate bool KeyDelegate(Keys key, InputState state = InputState.Down);

        private delegate bool ButtonDelegate(Buttons buttons, InputState state = InputState.Down, int player = 0);

        private delegate void ReplaceColorDelegate(int index, int id);

        private delegate void ClearDelegate(int x = 0, int y = 0, int? width = null, int? height = null);

        private delegate Point DisplayDelegate(bool visible = true);

        private delegate void LoadScriptDelegate(string name);

        private delegate void DrawPixelsDelegate(int[] pixelData, int x, int y, int width, int height, bool flipH = false, bool flipV = false, 
            DrawMode mode = DrawMode.Sprite, int colorOffset = 0);

        private delegate void DrawSpriteDelegate(int id, int x, int y, bool flipH = false, bool flipV = false,
            DrawMode drawMode = DrawMode.Sprite, int colorOffset = 0);

//        private delegate void DrawTileDelegate(int id, int c, int r, DrawMode drawMode = DrawMode.Tile, int colorOffset = 0, bool flipH = false, bool flipV = false);
//
//        private delegate void DrawTilesDelegate(int[] ids, int c, int r, int width, DrawMode drawMode = DrawMode.Tile, int colorOffset = 0);

        private delegate void DrawSpritesDelegate(int[] ids, int x, int y, int width, bool flipH = false, bool flipV = false, DrawMode drawMode = DrawMode.Sprite, int colorOffset = 0, bool onScreen = true, bool useScrollPos = true, Rectangle? bounds = null);

        private delegate void DrawSpriteBlockDelegate(int id, int x, int y, int width = 1, int height = 1, bool flipH = false, bool flipV = false, DrawMode drawMode = DrawMode.Sprite, int colorOffset = 0, bool onScreen = true, bool useScrollPos = true, Rectangle? bounds = null);

        private delegate void DrawTextDelegate(string text, int x, int y, DrawMode mode = DrawMode.Sprite,
            string font = "Default", int colorOffset = 0, int spacing = 0);

        private delegate void DrawTilemapDelegate(int x = 0, int y = 0, int columns = 0, int rows = 0, int? offsetX = null, int? offsetY = null, DrawMode drawMode = DrawMode.Tile);

        private delegate void DrawRectDelegate(int x, int y, int width, int height, int color = -1, DrawMode drawMode = DrawMode.UI);
        private delegate bool MouseButtonDelegate(int button, InputState state = InputState.Down);

        private delegate Point MousePositionDelegate();

        private delegate string InputStringDelegate();

        private delegate string ReadDataDelegate(string key, string defaultValue = "undefined");

        private delegate void RebuildMapDelegate(int? columns = null, int? rows = null, int[] spriteIDs = null, int[] colorOffsets = null, int[] flags = null);

        private delegate void RedrawDisplayDelegate();

        private delegate void RewindSongDelegate(int position, int loop);

        private delegate Point ScrollPositionDelegate(int? x, int? y);

        private delegate void PlaySoundDelegate(int id, int channel = 0);

        private delegate void PlayRawSoundDelegate(string data, int channel = 0, float frequency = 0.1266f);

        private delegate void PlayPatternDelegate(int id, bool loop = true);
        private delegate void PlayPatternsDelegate(int[] trackIDs, bool loop = true);
        private delegate void PlaySongDelegate(int id, bool loop = true, int startAt = 0);

        private delegate void PauseSongDelegate();

        private delegate Point SpriteSizeDelegate(int? x = 8, int? y = 8);

        private delegate void StopSongDelegate();

        private delegate Point TilemapSizeDelegate(int? columns = null, int? rows = null);

        private delegate TileData TileDelegate(int column, int row, int? spriteID, int? colorOffset,
            int? flag, bool? flipH = null, bool? flipV = null);

        private delegate int[] SpriteDelegate(int id, int[] data = null);
        private delegate int[] SpritesDelegate(int[] id, int width);

        private delegate int FlagDelegate(int column, int row, int? value = null);

        private delegate void UpdateTilesDelegate(int column, int row, int columns, int[] ids, int? colorOffset = null,
            int? flag = null);

        private delegate void WriteDataDelegate(string key, string value);

        private delegate string ColorDelegate(int id, string value = null);

        private delegate int ColorsPerSpriteDelegate();

        private delegate int TotalSpritesDelegate(bool ignoreEmpty = true);

        private delegate int TotalColorsDelegate(bool ignoreEmpty = true);

        private delegate int ClampDelegate(int val, int min, int max);

        private delegate int RepeatDelegate(int val, int max);

        private delegate int CalculateIndexDelegate(int x, int y, int width);

        private delegate Point CalculatePositionDelegate(int index, int width);

        private delegate string WordWrapDelegate(string text, int width);

        private delegate string[] SplitLinesDelegate(string txt);

        private delegate int[] BitArrayDelegate(int value);
        private delegate int CalcualteDistanceDelegate(int x0, int y0, int x1, int y1);

        private delegate int[] ConvertTextToSpritesDelegate(string text, string fontName = "default");
        private delegate int[] ConvertCharacterToPixelDataDelegate(char character, string fontName);


        //        private delegate TextureData NewTextureDataDelegate(int width, int height, bool wrapMode = true);

        private delegate Canvas NewCanvasDelegator(int width, int height);
        
        private delegate int PaletteOffsetDelegator(int value);
        private delegate void RegisterMetaSpriteDelegator(string name, int width, MetaSpriteData[] sprites);
        private delegate void DeleteMetaSpriteDelegator(string name);
        private delegate void DrawMetaSpriteDelegator(string name, int x, int y, bool flipH, bool flipV, DrawMode drawMode = DrawMode.Sprite, int colorOffset = 0, bool onScreen = true, bool useScrollPos = true, Rectangle? bounds = null);
    }
}