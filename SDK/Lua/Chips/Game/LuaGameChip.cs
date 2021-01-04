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
using MoonSharp.Interpreter;
using PixelVision8.Engine.Audio;
using PixelVision8.Runner;
using PixelVision8.Runner.Services;
using PixelVisionSDK.Engine;
using System;
using System.Collections.Generic;

namespace PixelVision8.Engine.Chips
{
    public class LuaGameChip : GameChip
    {
        protected Script _luaScript;
        public string DefaultScriptPath = "code";

        public Script LuaScript
        {
            get
            {
                if (_luaScript == null) _luaScript = new Script(CoreModules.Preset_SoftSandbox);

                return _luaScript;
            }
        }

        protected virtual void RegisterLuaServices()
        {
            try
            {
                // Look to see if there are any lua services registered in the game engine
                var luaService = ((PixelVisionEngine)engine).GetService(typeof(LuaService).FullName) as LuaService;

                // Run the lua service to configure the script
                luaService.ConfigureScript(LuaScript);
            }
            catch
            {
                // Do nothing if the lua service isn't found
            }

            #region Color APIs

            LuaScript.Globals["BackgroundColor"] = new Func<int?, int>(BackgroundColor);
            LuaScript.Globals["Color"] = new Func<int, string, string>(Color);
            LuaScript.Globals["ColorsPerSprite"] = new Func<int>(ColorsPerSprite);
            LuaScript.Globals["TotalColors"] = new Func<bool, int>(TotalColors);
            LuaScript.Globals["ReplaceColor"] = new Action<int, int>(ReplaceColor);

            #endregion

            #region Display APIs

            LuaScript.Globals["Clear"] = new Action(Clear);
            LuaScript.Globals["Display"] = new Func<Point>(Display);
            LuaScript.Globals["DrawPixels"] =
                new Action<int[], int, int, int, int, bool, bool, DrawMode, int>(DrawPixels);
            LuaScript.Globals["DrawSprite"] =
                new Action<int, int, int, bool, bool, DrawMode, int>(DrawSingleSprite);
            LuaScript.Globals["DrawSprites"] =
                new Action<int[], int, int, int, bool, bool, DrawMode, int>(DrawSprites);
            LuaScript.Globals["DrawSpriteBlock"] =
                new Action<int, int, int, int, int, bool, bool, DrawMode, int>(DrawSpriteBlock);

            LuaScript.Globals["DrawText"] = new Action<string, int, int, DrawMode, string, int, int>(DrawText);
            LuaScript.Globals["DrawTilemap"] = new Action<int, int, int, int, int?, int?>(DrawTilemap);

            LuaScript.Globals["DrawRect"] = new Action<int, int, int, int, int, DrawMode>(DrawRect);
            LuaScript.Globals["RedrawDisplay"] = new Action(RedrawDisplay);
            LuaScript.Globals["ScrollPosition"] = new Func<int?, int?, Point>(ScrollPosition);

            #endregion

            #region File IO APIs

            LuaScript.Globals["AddScript"] = new Action<string, string>(AddScript);
            LuaScript.Globals["LoadScript"] = new Action<string>(LoadScript);
            LuaScript.Globals["ReadSaveData"] = new Func<string, string, string>(ReadSaveData);
            LuaScript.Globals["WriteSaveData"] = new Action<string, string>(WriteSaveData);
            LuaScript.Globals["ReadMetadata"] = new Func<string, string, string>(ReadMetadata);
            LuaScript.Globals["WriteMetadata"] = new Action<string, string>(WriteMetadata);
            LuaScript.Globals["ReadAllMetadata"] = new Func<Dictionary<string, string>>(ReadAllMetadata);

            #endregion

            #region Input APIs

            LuaScript.Globals["Key"] = new Func<Keys, InputState, bool>(Key);
            LuaScript.Globals["Button"] = new Func<Buttons, InputState, int, bool>(Button);
            LuaScript.Globals["MouseButton"] = new Func<int, InputState, bool>(MouseButton);
            LuaScript.Globals["MousePosition"] = new Func<Point>(MousePosition);
            LuaScript.Globals["MouseWheel"] = new Func<Point>(MouseWheel);
            LuaScript.Globals["InputString"] = new Func<string>(InputString);

            #endregion

            #region Math APIs

            LuaScript.Globals["CalculateIndex"] = new Func<int, int, int, int>(CalculateIndex);
            LuaScript.Globals["CalculatePosition"] = new Func<int, int, Point>(CalculatePosition);
            LuaScript.Globals["Clamp"] = new Func<int, int, int, int>(Clamp);
            LuaScript.Globals["Repeat"] = new Func<int, int, int>(Repeat);

            #endregion

            #region Sound APIs

            // LuaScript.Globals["Sound"] = new Func<int, string, string>(Sound);
            LuaScript.Globals["PlaySound"] = new Action<int, int>(PlaySound);
            LuaScript.Globals["StopSound"] = new Action<int>(StopSound);
            LuaScript.Globals["PlayRawSound"] = new Action<string, int, float>(((SfxrSoundChip)SoundChip).PlayRawSound);

            LuaScript.Globals["IsChannelPlaying"] = new Func<int, bool>(IsChannelPlaying);
            LuaScript.Globals["PlayPattern"] = new Action<int, bool>(PlayPattern);
            LuaScript.Globals["PlayPatterns"] = new Action<int[], bool>(PlayPatterns);
            LuaScript.Globals["PlaySong"] = new Action<int, bool, int>(PlaySong);
            LuaScript.Globals["PauseSong"] = new Action(PauseSong);
            LuaScript.Globals["RewindSong"] = new Action<int, int>(RewindSong);
            LuaScript.Globals["StopSong"] = new Action(StopSong);
            LuaScript.Globals["SongData"] = new Func<Dictionary<string, int>>(SongData);

            #endregion

            #region Sprite APIs

            LuaScript.Globals["Sprite"] = new Func<int, int[], int[]>(Sprite);
            //            luaScript.Globals["Sprites"] = new Func<int[], int, int[]>(Sprites);
            LuaScript.Globals["SpriteSize"] = new Func<Point>(SpriteSize);
            LuaScript.Globals["TotalSprites"] = new Func<bool, int>(TotalSprites);
            LuaScript.Globals["MaxSpriteCount"] = new Func<int>(MaxSpriteCount);

            #endregion

            #region Tilemap

            // LuaScript.Globals["RebuildTilemap"] = new Action(RebuildTilemap);

            LuaScript.Globals["TilemapSize"] = new Func<int?, int?, bool, Point>(TilemapSize);
            LuaScript.Globals["Tile"] = new Func<int, int, int?, int?, int?, bool?, bool?, TileData>(Tile);
            LuaScript.Globals["UpdateTiles"] = new Action<int[], int?, int?>(UpdateTiles);
            LuaScript.Globals["Flag"] = new Func<int, int, int?, int>(Flag);


            LuaScript.Globals["SaveTilemapCache"] = new Action(SaveTilemapCache);
            LuaScript.Globals["RestoreTilemapCache"] = new Action(RestoreTilemapCache);

            #endregion

            #region Utils

            LuaScript.Globals["WordWrap"] = new Func<string, int, string>(WordWrap);
            LuaScript.Globals["SplitLines"] = new Func<string, string[]>(SplitLines);
            LuaScript.Globals["BitArray"] = new Func<int, int[]>(BitArray);
            LuaScript.Globals["CalculateDistance"] = new Func<int, int, int, int, int>(CalculateDistance);

            #endregion

            #region Debug

            LuaScript.Globals["ReadFPS"] = new Func<int>(ReadFPS);
            LuaScript.Globals["ReadTotalSprites"] = new Func<int>(ReadTotalSprites);

            #endregion

            #region Text

            //            luaScript.Globals["ConvertTextToSprites"] = new Func<string, string, int[]>(ConvertTextToSprites);
            // LuaScript.Globals["CharacterToPixelData"] =
            //     new Func<char, string, int[]>(CharacterToPixelData);

            #endregion

            #region Experimental APIs

            LuaScript.Globals["PaletteOffset"] = new Func<int, int, int>(PaletteOffset);

            LuaScript.Globals["TotalMetaSprites"] = new Func<int?, int>(TotalMetaSprites);
            LuaScript.Globals["MetaSprite"] = new Func<int, SpriteCollection, SpriteCollection>(MetaSprite);
            LuaScript.Globals["DrawMetaSprite"] =
                new Action<int, int, int, bool, bool, DrawMode, int>(DrawMetaSprite);

            UserData.RegisterType<SpriteData>();
            LuaScript.Globals["SpriteData"] = UserData.CreateStatic<SpriteData>();

            // Create new meta sprites
            UserData.RegisterType<SpriteCollection>();
            LuaScript.Globals["SpriteCollection"] = UserData.CreateStatic<SpriteCollection>();

            #endregion

            // Enums
            UserData.RegisterType<WaveType>();
            LuaScript.Globals["WaveType"] = UserData.CreateStatic<WaveType>();

            UserData.RegisterType<DrawMode>();
            LuaScript.Globals["DrawMode"] = UserData.CreateStatic<DrawMode>();

            UserData.RegisterType<Buttons>();
            LuaScript.Globals["Buttons"] = UserData.CreateStatic<Buttons>();

            UserData.RegisterType<Keys>();
            LuaScript.Globals["Keys"] = UserData.CreateStatic<Keys>();

            UserData.RegisterType<InputState>();
            LuaScript.Globals["InputState"] = UserData.CreateStatic<InputState>();

            UserData.RegisterType<SaveFlags>();
            LuaScript.Globals["SaveFlags"] = UserData.CreateStatic<SaveFlags>();

            // Register PV8's vector type
            UserData.RegisterType<Point>();
            LuaScript.Globals["NewPoint"] = new Func<int, int, Point>(NewPoint);

            // Register PV8's TileData type
            UserData.RegisterType<TileData>();

            // Register PV8's rect type
            UserData.RegisterType<Rectangle>();
            LuaScript.Globals["NewRect"] = new Func<int, int, int, int, Rectangle>(NewRect);

            // Register PV8's rect type
            UserData.RegisterType<Canvas>();
            LuaScript.Globals["NewCanvas"] =
                new Func<int, int, Canvas>(NewCanvas);

            UserData.RegisterType<SpriteData>();
            LuaScript.Globals["NewSpriteData"] =
                new Func<int, int, int, bool, bool, int, SpriteData>(NewSpriteData);

            UserData.RegisterType<SpriteCollection>();
            LuaScript.Globals["NewSpriteCollection"] =
                new Func<string, SpriteData[], SpriteCollection>(NewSpriteCollection);

            LuaScript.Globals["NewMetaSprite"] =
                new Func<int, string, int[], int, int, SpriteCollection>(NewMetaSprite);


        }

        #region Lifecycle

        public override void Init()
        {
            if (LuaScript?.Globals["Init"] == null) return;

            LuaScript.Call(LuaScript.Globals["Init"]);
        }

        public override void Update(int timeDelta)
        {
            base.Update(timeDelta);

            if (LuaScript?.Globals["Update"] == null) return;

            LuaScript.Call(LuaScript.Globals["Update"], timeDelta);
        }

        public override void Draw()
        {
            if (LuaScript?.Globals["Draw"] == null) return;

            LuaScript.Call(LuaScript.Globals["Draw"]);
        }

        public override void Shutdown()
        {
            if (LuaScript?.Globals["Shutdown"] == null) return;

            LuaScript.Call(LuaScript.Globals["Shutdown"]);

        }

        public override void Reset()
        {
            // Setup the GameChip
            base.Reset();

            if (LuaScript.Globals["Reset"] != null) LuaScript.Call(LuaScript.Globals["Reset"]);

        }

        public override void Configure()
        {
            base.Configure();

            // Register any extra services
            RegisterLuaServices();
        }

        // public virtual void LoadDefaultScript()
        // {
        //     // Kick off the first game script file
        //     LoadScript(DefaultScriptPath);
        //
        //     // Reset the game
        //
        // }

        #endregion

        #region Scripts

        // TODO need to update the docs on both of these APIs. Load is for a file and Add is for a string. Both automatically parse the script.

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
        public virtual void LoadScript(string name)
        {
            LuaScript.DoFile(name);
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
            LuaScript.DoString(script, null, name);
        }

        public void DrawSingleSprite(int id, int x, int y, bool flipH = false, bool flipV = false,
            DrawMode drawMode = DrawMode.Sprite, int colorOffset = 0)
        {
            DrawSprite(id, x, y, flipH, flipV, drawMode, colorOffset, SpriteChip);
        }

        #endregion
    }
}