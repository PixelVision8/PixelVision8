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

            LuaScript.Globals["TilemapSize"] = new Func<int?, int?, bool, Point>(TilemapSize);
            LuaScript.Globals["Tile"] = new Func<int, int, int?, int?, int?, bool?, bool?, TileData>(Tile);
            LuaScript.Globals["UpdateTiles"] = new Action<int[], int?, int?>(UpdateTiles);
            LuaScript.Globals["Flag"] = new Func<int, int, int?, int>(Flag);


            LuaScript.Globals["SaveTilemapCache"] = new Action(SaveTilemapCache);
            LuaScript.Globals["RestoreTilemapCache"] = new Action(RestoreTilemapCache);

            #endregion
            
            // Register PV8's TileData type
            UserData.RegisterType<TileData>();
            
            
        }
    }
}