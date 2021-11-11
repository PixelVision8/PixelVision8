using System;
using MoonSharp.Interpreter;

namespace PixelVision8.Player
{
    public partial class LuaGameChip
    {
        [LuaGameChipAPI]
        public void RegisterTilemap()
        {
            #region Tilemap

            // LuaScript.Globals["RebuildTilemap"] = new Action(RebuildTilemap);

            LuaScript.Globals["TilemapSize"] = new Func<Point>(TilemapSize);
            LuaScript.Globals["Tile"] = new Func<int, int, int?, int?, int?, bool?, bool?, TileData>(Tile);
            LuaScript.Globals["UpdateTiles"] = new Action<int[], int?, int?>(UpdateTiles);
            LuaScript.Globals["Flag"] = new Func<int, int, int?, int>(Flag);


            LuaScript.Globals["SaveTilemapCache"] = new Action(SaveTilemapCache);
            LuaScript.Globals["RestoreTilemapCache"] = new Action(RestoreTilemapCache);

            LuaScript.Globals["TilemapName"] = new Func<string, string>(TilemapName);
            LuaScript.Globals["ReadTilemap"] = new Func<string, TilemapData>(ReadTilemap);
            LuaScript.Globals["LoadTilemap"] = new Func<string, bool>(LoadTilemap);
            LuaScript.Globals["SaveTilemap"] = new Action<string>(SaveTilemap);
            LuaScript.Globals["ClearTilemap"] = new Action(ClearTilemap);

            #endregion
            
            // Register PV8's TilemapData type
            UserData.RegisterType<PixelData<int>>();

            // Register PV8's TilemapData type
            UserData.RegisterType<TilemapData>();

            // Register PV8's TileData type
            UserData.RegisterType<TileData>();
            
            
        }
    }
}