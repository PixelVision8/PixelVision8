using System;
using MoonSharp.Interpreter;

namespace PixelVision8.Player
{
    public partial class LuaGameChip
    {
        [LuaGameChipAPI]
        public void RegisterMetaSprite()
        {
            
            LuaScript.Globals["FindMetaSpriteId"] = new Func<string, int>(FindMetaSpriteId);
            LuaScript.Globals["TotalMetaSprites"] = new Func<int?, int>(TotalMetaSprites);
            LuaScript.Globals["MetaSprite"] = new Func<int, SpriteCollection, SpriteCollection>(MetaSprite);
            LuaScript.Globals["DrawMetaSprite"] =
                new Action<int, int, int, bool, bool, DrawMode, int>(DrawMetaSprite);

            UserData.RegisterType<SpriteData>();
            LuaScript.Globals["SpriteData"] = UserData.CreateStatic<SpriteData>();

            // Create new meta sprites
            UserData.RegisterType<SpriteCollection>();
            LuaScript.Globals["SpriteCollection"] = UserData.CreateStatic<SpriteCollection>();

            UserData.RegisterType<SpriteData>();
            LuaScript.Globals["NewSpriteData"] =
                new Func<int, int, int, bool, bool, int, SpriteData>(NewSpriteData);

            UserData.RegisterType<SpriteCollection>();
            LuaScript.Globals["NewSpriteCollection"] =
                new Func<string, SpriteData[], SpriteCollection>(NewSpriteCollection);

            LuaScript.Globals["NewMetaSprite"] =
                new Func<int, string, int[], int, int, SpriteCollection>(NewMetaSprite);
        }
    }
}