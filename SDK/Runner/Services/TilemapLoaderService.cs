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

using PixelVision8.Player;
using PixelVision8.Runner;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

namespace PixelVision8.Runner
{
    public class TilemapLoaderService : AbstractService
    {

        private Dictionary<string, string> _tilemapPaths = new Dictionary<string, string>();
        private IImageParser _imageParser;
        private readonly IFileLoader _fileLoadHelper;

        public TilemapLoaderService(IFileLoader fileLoadHelper, IImageParser imageParser)
        {
            // imageParser = new PNGFileReader(fileLoadHelper);

            _imageParser = imageParser;

            _fileLoadHelper = fileLoadHelper;
        }

        public void Clear() => _tilemapPaths.Clear();
        
        public void Add(string name, string path)
        {

        }

        public void Load(string name, PixelVision engine)
        {

        }

        public void Unload(PixelVision engine)
        {
            
        }

    }

}

// TODO need to add a loader that pushes tilemap file paths into the loader


namespace PixelVision8.Player
{

    public partial class PixelVision
    {

        public TilemapLoaderService TilemapService;

        public void AddTilemap()
        {
            TilemapService = ServiceLocator.GetService(typeof(TilemapLoaderService).FullName) as TilemapLoaderService;
        }


    }

    public partial class GameChip
    {
        private TilemapLoaderService _tilemapService;

        public TilemapLoaderService TilemapService
        {
            get{
                if(_tilemapService == null)
                    _tilemapService = Player.GetService(typeof(TilemapLoaderService).FullName) as TilemapLoaderService;


                return _tilemapService;
                
            }
        }

        public void LoadTilemap(string name)
        {
            // Player.GetService()
        }

        public void UnloadTilemap()
        {

        }

    }

    public partial class LuaGameChip
    {
        [LuaGameChipAPI]
        public void RegisterTilemapLoadService()
        {

            Console.WriteLine("Register Tilemap Lua Service");
            // Register PV8's rect type
            // UserData.RegisterType<Canvas>();
            // LuaScript.Globals["NewCanvas"] =
            //     new Func<int, int, Canvas>(NewCanvas);
        }
    }
    
}