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
using System.Linq;

namespace PixelVision8.Player
{

    public struct TilemapData
    {

        private int _columns;
        private int _rows;
        private int _total;

        public PixelData<int> SpriteIds;
        public PixelData<int> Flags;
        public PixelData<int> ColorOffset;
        
        public string Name;
        public int Columns => _columns;
        public int Rows => _rows;

        public int Total => Columns * Rows;

        public TilemapData(string name, int columns = 32, int rows = 30)
        {
            Name = name;
            _columns = columns;
            _rows = rows;
            _total = _columns * _rows;

            SpriteIds = new PixelData<int>(_columns, _rows, Enumerable.Repeat(-1, _total).ToArray());
            Flags = new PixelData<int>(_columns, _rows, Enumerable.Repeat(-1, _total).ToArray());
            ColorOffset = new PixelData<int>(_columns, _rows);
            
        }

    }
    
    public partial class GameChip
    {

        #region Tilemaps Logic

        private Dictionary<string, TilemapData> _tilemaps = new Dictionary<string, TilemapData>();

        private string _tilemapName;

        public string TilemapName(string name = null)
        {
            
            if(name != null || name != string.Empty)
            {
                _tilemapName = name;
            }

            return _tilemapName;

        }

        public bool TilemapExists(string name)
        {
            return _tilemaps.ContainsKey(name);
        }

        public TilemapData ReadTilemap(string name)
        {
            return _tilemaps[name];
        }

        public bool LoadTilemap(string name)
        {
            
            _tilemapName = name;

            ClearTilemap();

            TilemapName(name);

            if(TilemapExists(name))
            {

                var tilemap = _tilemaps[name];

                if(tilemap.Total == TilemapChip.Total)
                {
                    for (int i = 0; i < tilemap.Total; i++)
                    {

                        var tile = TilemapChip.tiles[i];
                        
                        tile.SpriteId = tilemap.SpriteIds[i];
                        tile.Flag = tilemap.Flags[i];
                        tile.ColorOffset = tilemap.ColorOffset[i];

                    }

                    return true;
                }
                
            }

            return false;
        }

        public void ClearTilemap()
        {

            TilemapName("untitled");

            TilemapChip.Clear();
        }

        public void SaveTilemap(string name = "")
        {
            
            // give the tilemap a name
            if(name == "")
                name = _tilemapName;
            
            // Create map if one does not exist
            if(!TilemapExists(name))
            {
                _tilemaps[name] = new TilemapData(name, TilemapChip.Columns, TilemapChip.Rows);
            }

            var tilemap = _tilemaps[name];

            for (int i = 0; i < TilemapChip.Total; i++)
            {
                
                var tile = TilemapChip.tiles[i];

                tilemap.SpriteIds[i] = tile.SpriteId;
                tilemap.Flags[i] = tile.Flag;
                tilemap.ColorOffset[i] = tile.ColorOffset;

            }

        }
        
        #endregion
    }
}