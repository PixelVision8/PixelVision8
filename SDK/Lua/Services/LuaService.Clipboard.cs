using System;
using MoonSharp.Interpreter;
using PixelVision8.Workspace;
using TextCopy;

namespace PixelVision8.Runner
{
    public partial class LuaService
    {

        [RegisterLuaService]
        public void RegisterClipboard(Script luaScript)
        {
            luaScript.Globals["SetClipboardText"] = new Action<string>(SetClipboardText);
            luaScript.Globals["GetClipboardText"] = new Func<string>(GetClipboardText);
            luaScript.Globals["ClearClipboardText"] = new Action(() => { SetClipboardText("");});
        }
        
        private string _clipboard;
        
        private void SetClipboardText(string value)
        {
            try
            {
                ClipboardService.SetText(value);
            }
            catch
            {
                // ignored
            }

            _clipboard = value;
        }
        
        private string GetClipboardText()
        {
            
            try
            {
                _clipboard = ClipboardService.GetText();
            }
            catch
            {
                // ignored
            }

            return _clipboard;
        }
    }
}