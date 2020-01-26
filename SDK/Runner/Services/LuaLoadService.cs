using PixelVision8.Engine;
using PixelVision8.Runner.Parsers;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PixelVision8.Runner.Services
{
    public class LuaLoadService : LoadService
    {
        public override void ParseExtraFileTypes(Dictionary<string, byte[]> files, IEngine engine, SaveFlags saveFlags)
        {

            // Step 2 (optional). Load up the Lua script
            if ((saveFlags & SaveFlags.Code) == SaveFlags.Code)
            {
                //var scriptExtension = ".lua";

                var paths = files.Keys.Where(s => textExtensions.Any(x => s.EndsWith(x))).ToList();

                foreach (var fileName in paths)
                {
                    parser = LoadScript(fileName, files[fileName]);
                    AddParser(parser);
                }
            }
        }

        private ScriptParser LoadScript(string fileName, byte[] data)
        {
            var script = Encoding.UTF8.GetString(data);
            var scriptParser = new ScriptParser(fileName, script, targetEngine.GameChip);

            return scriptParser;
        }
    }
    
    
}
