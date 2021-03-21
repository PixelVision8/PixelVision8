using MoonSharp.Interpreter;
using PixelVision8.Editor;

namespace PixelVision8.Runner
{
    
    public partial class LuaService
    {
        private GameEditor _editor;

        [RegisterLuaService]
        public void RegisterEditor(Script luaScript)
        {
            // Register the game editor with  the lua service
            UserData.RegisterType<GameEditor>();
            luaScript.Globals["gameEditor"] = Editor;
        }
        
        public GameEditor Editor
        {
            get
            {
                if (_editor == null) _editor = new GameEditor(runner);

                return _editor;
            }
        }
    }
}