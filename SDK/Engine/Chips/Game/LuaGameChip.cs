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
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using MoonSharp.Interpreter;
using PixelVision8.Engine.Audio;
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
                if (_luaScript == null) _luaScript = new Script(CoreModules.Preset_SoftSandbox);


                return _luaScript;
            }
        }

        protected virtual void RegisterLuaServices()
        {
            
        }

        #region Lifecycle

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

            luaScript.Globals["BackgroundColor"] = new Func<int?, int>(BackgroundColor);
            luaScript.Globals["Color"] = new Func<int, string, string>(Color);
            luaScript.Globals["ColorsPerSprite"] = new Func<int>(ColorsPerSprite);
            luaScript.Globals["TotalColors"] = new Func<bool, int>(TotalColors);
            luaScript.Globals["ReplaceColor"] = new Action<int, int>(ReplaceColor);

            #endregion

            #region Display APIs

            luaScript.Globals["Clear"] = new Action<int, int, int?, int?>(Clear);
            luaScript.Globals["Display"] = new Func<bool, Point>(Display);
            luaScript.Globals["DrawPixels"] = new Action<int[], int, int, int, int, bool, bool, DrawMode, int>(DrawPixels);
            luaScript.Globals["DrawSprite"] = new Action<int, int, int, bool, bool, DrawMode, int>(DrawSprite);
            luaScript.Globals["DrawSprites"] = new Action<int[], int, int, int, bool, bool, DrawMode, int, bool, bool, Rectangle?>(DrawSprites);
            luaScript.Globals["DrawSpriteBlock"] = new Action<int, int, int, int, int, bool, bool, DrawMode, int, bool, bool, Rectangle?>(DrawSpriteBlock);

            luaScript.Globals["DrawText"] = new Action<string, int, int, DrawMode, string, int, int>(DrawText);
            luaScript.Globals["DrawTilemap"] = new Action<int, int, int, int, int?, int?, DrawMode>(DrawTilemap);

            luaScript.Globals["DrawRect"] = new Action<int, int, int, int, int, DrawMode>(DrawRect);
            luaScript.Globals["RedrawDisplay"] = new Action(RedrawDisplay);
            luaScript.Globals["ScrollPosition"] = new Func<int?, int?, Point>(ScrollPosition);

            #endregion

            #region File IO APIs

            luaScript.Globals["LoadScript"] = new Action<string>(LoadScript);
            luaScript.Globals["ReadSaveData"] = new Func<string, string, string>(ReadSaveData);
            luaScript.Globals["WriteSaveData"] = new Action<string, string>(WriteSaveData);
            luaScript.Globals["ReadMetaData"] = new Func<string, string, string>(ReadMetaData);
            luaScript.Globals["WriteMetaData"] = new Action<string, string>(WriteMetaData);

            #endregion

            #region Input APIs

            luaScript.Globals["Key"] = new Func<Keys, InputState, bool>(Key);
            luaScript.Globals["Button"] = new Func<Buttons, InputState, int, bool>(Button);
            luaScript.Globals["MouseButton"] = new Func<int, InputState, bool>(MouseButton);
            luaScript.Globals["MousePosition"] = new Func<Point>(MousePosition);
            luaScript.Globals["InputString"] = new Func<string>(InputString);

            #endregion

            #region Math APIs

            luaScript.Globals["CalculateIndex"] = new Func<int, int, int, int>(CalculateIndex);
            luaScript.Globals["CalculatePosition"] = new Func<int, int, Point>(CalculatePosition);
            luaScript.Globals["Clamp"] = new Func<int, int, int, int>(Clamp);
            luaScript.Globals["Repeat"] = new Func<int, int, int>(Repeat);

            #endregion

            #region Sound APIs

            luaScript.Globals["PlaySound"] = new Action<int, int>(PlaySound);
            luaScript.Globals["PlayRawSound"] = new Action<string, int, float>((soundChip).PlayRawSound);
        
            luaScript.Globals["PlayPattern"] = new Action<int, bool>(PlayPattern);
            luaScript.Globals["PlayPatterns"] = new Action<int[], bool>(PlayPatterns);
            luaScript.Globals["PlaySong"] = new Action<int, bool, int>(PlaySong);
            luaScript.Globals["PauseSong"] = new Action(PauseSong);
            luaScript.Globals["RewindSong"] = new Action<int, int>(RewindSong);
            luaScript.Globals["StopSong"] = new Action(StopSong);
            luaScript.Globals["SongData"] = new Func<Dictionary<string, int>>(SongData);

            #endregion

            #region Sprite APIs

            luaScript.Globals["Sprite"] = new Func<int, int[], int[]>(Sprite);
            luaScript.Globals["Sprites"] = new Func<int[], int, int[]>(Sprites);
            luaScript.Globals["SpriteSize"] = new Func<int?, int?, Point>(SpriteSize);
            luaScript.Globals["TotalSprites"] = new Func<bool, int>(TotalSprites);

            #endregion

            #region Tilemap

            luaScript.Globals["RebuildTilemap"] = new Action(RebuildTilemap);
                
            luaScript.Globals["TilemapSize"] = new Func<int?, int?, bool, Point>(TilemapSize);
            luaScript.Globals["Tile"] = new Func<int, int, int?, int?, int?, bool?, bool?, TileData>(Tile);
            luaScript.Globals["UpdateTiles"] = new Action<int, int, int, int[], int?, int?>(UpdateTiles);
            luaScript.Globals["Flag"] = new Func<int, int, int?, int> (Flag);


            luaScript.Globals["SaveTilemapCache"] = new Action(SaveTilemapCache);
            luaScript.Globals["RestoreTilemapCache"] = new Action(RestoreTilemapCache);

            #endregion

            #region Utils

            luaScript.Globals["WordWrap"] = new Func<string, int, string>(WordWrap);
            luaScript.Globals["SplitLines"] = new Func<string, string[]>(SplitLines);
            luaScript.Globals["BitArray"] = new Func<int, int[]>(BitArray);
            luaScript.Globals["CalculateDistance"] = new Func<int, int, int, int,int>(CalculateDistance);

            #endregion

            #region Debug

            luaScript.Globals["ReadFPS"] = new Func<int>(()=>fps);
            luaScript.Globals["ReadTotalSprites"] = new Func<int>(()=>currentSprites);

            #endregion

            #region Text

            luaScript.Globals["ConvertTextToSprites"] = new Func<string, string, int[]>(ConvertTextToSprites);
            luaScript.Globals["ConvertCharacterToPixelData"] = new Func<char, string, int[]>(ConvertCharacterToPixelData);

            #endregion

            #region Experimental APIs

            luaScript.Globals["PaletteOffset"] = new Func<int, int>(PaletteOffset);


            luaScript.Globals["RegisterMetaSprite"] = new Action<string, int, MetaSpriteData[]>(RegisterMetaSprite);
            luaScript.Globals["DeleteMetaSprite"] = new Action<string>(DeleteMetaSprite);
            luaScript.Globals["DrawMetaSprite"] = new Action<string, int, int, bool, bool, DrawMode, int, bool, bool, Rectangle?>(DrawMetaSprite);

            UserData.RegisterType<MetaSpriteData>();
            luaScript.Globals["MetaSpriteData"] = UserData.CreateStatic<MetaSpriteData>();

            UserData.RegisterType<MetaSprite>();
            luaScript.Globals["MetaSprite"] = UserData.CreateStatic<MetaSprite>();

            #endregion

            // Enums
            UserData.RegisterType<WaveType>();
            luaScript.Globals["WaveType"] = UserData.CreateStatic<WaveType>();
            
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
            luaScript.Globals["NewPoint"] = new Func<int, int, Point>(NewPoint);

            // Register PV8's TileData type
            UserData.RegisterType<TileData>();

            // Register PV8's rect type
            UserData.RegisterType<Rectangle>();
            luaScript.Globals["NewRect"] = new Func<int, int, int, int, Rectangle>(NewRect);

            // Register PV8's rect type
            UserData.RegisterType<Canvas>();
            luaScript.Globals["NewCanvas"] = new Func<int, int, Canvas>((width, height) => new Canvas(width, height, this));

            // Load the default script
            LoadScript("code.lua");

            // Register any extra services
            RegisterLuaServices();

            // Reset the game
            if (luaScript.Globals["Reset"] != null)
                luaScript.Call(luaScript.Globals["Reset"]);
        }

        #endregion

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
                    var pattern = @"(\S+)\s*([+\-*/%])\s*=";
                    var replacement = "$1 = $1 $2 ";
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
                textFiles[name] = script;
            else
                textFiles.Add(name, script);
        }

        #endregion
    }
}