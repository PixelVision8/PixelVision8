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
            LuaScript.Globals["MetaSprite"] = new Func<object, SpriteCollection, SpriteCollection>(MetaSpriteRouter);
            LuaScript.Globals["DrawMetaSprite"] =
                new Action<object, int, int, bool, bool, DrawMode, int>(DrawMetaSpriteRouter);

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

        public void DrawMetaSpriteRouter(object metaSprite, int x, int y, bool flipH = false, bool flipV = false, DrawMode drawMode = DrawMode.Sprite, int colorOffset = 0)
        {
            
            if(metaSprite is string){
                DrawMetaSprite((string)metaSprite, x, y, flipH, flipV, drawMode, colorOffset);
            }else if(metaSprite is SpriteCollection){
                DrawMetaSprite((SpriteCollection)metaSprite, x, y, flipH, flipV, drawMode, colorOffset);
            }else{
                // Convert to an int or throw an error
                DrawMetaSprite(Convert.ToInt32(metaSprite), x, y, flipH, flipV, drawMode, colorOffset);
            }
            
        }

        public SpriteCollection MetaSpriteRouter(object id, SpriteCollection spriteCollection = null)
        {
            if(id is string)
            {
                return MetaSprite((string)id, spriteCollection);
            }

            return MetaSprite(Convert.ToInt32(id), spriteCollection);
        }
    }
}