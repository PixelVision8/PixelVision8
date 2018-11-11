//
// Copyright (c) Jesse Freeman. All rights reserved.  
//
// Licensed under the Microsoft Public License (MS-PL) License. 
// See LICENSE file in the project root for full license information. 
//
// Contributors
// --------------------------------------------------------
// This is the official list of Pixel Vision 8 contributors:
//
// Jesse Freeman - @JesseFreeman
// Christer Kaitila - @McFunkypants
// Pedro Medeiros - @saint11
// Shawn Rakowski - @shwany

using System;
using System.Linq;
using PixelVisionRunner.Parsers;
using PixelVisionSDK;
using PixelVisionSDK.Chips;

namespace PixelVisionRunner.Exporters
{
    public class TilemapExporter : PNGExporter
    {
        
        protected int currentTile;
        protected GameChip gameChip;
        
        protected int maxTilesPerLoop = 100;
        protected Vector spriteSize;
        protected TilemapChip tilemapChip;
        protected int totalTiles;
        protected int totalPixels;
        protected IEngine engine;
        
        public TilemapExporter(string fileName, IEngine engine, ITextureFactory textureFactory) : base(fileName, textureFactory)
        {
            this.engine = engine;
        }

        public override void CalculateSteps()
        {
            tilemapChip = engine.tilemapChip;
            totalTiles = tilemapChip.total;
            gameChip = engine.gameChip;
            
            var colorMapChip = engine.chipManager.GetChip(ColorMapParser.chipName, false) as ColorChip;

            colors = colorMapChip == null ? engine.colorChip.colors : colorMapChip.colors;
            
            var spriteChip = engine.spriteChip;
            
            width = spriteChip.width * tilemapChip.columns;
            height =  spriteChip.height * tilemapChip.rows;
            
            spriteSize = gameChip.SpriteSize();
            
            var size = gameChip.SpriteSize();
            totalPixels = size.x * size.y;
            
            loops = (int) Math.Ceiling((float) totalTiles / maxTilesPerLoop);
            
            base.CalculateSteps();
        }

        protected override void ProcessPixelData()
        {
            // TODO i may need to be reset to 0 on each loop. Double check this
            for (var i = 0; i < maxTilesPerLoop; i++)
            {
                var pos = gameChip.CalculatePosition(currentTile, tilemapChip.columns);
                
                var tileData = gameChip.Tile(pos.x, pos.y);
                var spriteData = GetPixelData(tileData);
                
                tmpTextureData.SetPixels(pos.x * spriteSize.x, pos.y * spriteSize.y, spriteSize.x, spriteSize.y, spriteData);
                

                currentTile++;

                if (currentTile >= totalTiles)
                    break;

                // TODO go through the tiles and copy them over to the TextureData
            }

            base.ProcessPixelData();
        }
        
        public virtual int[] GetPixelData(TileData tileData)
        {
            if (tileData.spriteID == -1)
            {
                return Enumerable.Repeat(-1, totalPixels).ToArray();
            }
            
            return gameChip.Sprite(tileData.spriteID);
            
        }

    }
}