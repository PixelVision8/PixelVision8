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
using PixelVision8.Player;

namespace PixelVision8.Runner
{
    public class TilemapParser : SpriteImageParser
    {
        // protected readonly bool autoImport = true;

        protected readonly TilemapChip tilemapChip;
        protected readonly GameChip gameChip;
        
        protected int Columns;
        protected int Rows;
    
        // protected TilemapData tilemapData;

        public TilemapParser(string sourceFile, IImageParser parser, ColorChip colorChip, SpriteChip spriteChip,
            TilemapChip tilemapChip, GameChip gameChip) :
            base(sourceFile, parser, colorChip, spriteChip)
        {
            this.tilemapChip = tilemapChip;
            this.gameChip = gameChip;
        }

        public override void CalculateSteps()
        {
            base.CalculateSteps();

            Steps.Add(SaveTilemap);
        }

        public override void CutOutSprites()
        {
            // TODO the image should be the right size from the beginning

            Columns = ImageData.Width / Constants.SpriteSize;
            Rows = ImageData.Height / Constants.SpriteSize;
            
            var tmpColumns = Columns > tilemapChip.Columns ? tilemapChip.Columns : Columns;
            var tmpRows = Rows > tilemapChip.Rows ? tilemapChip.Rows : Rows;

            gameChip.LoadTilemap(Parser.FileName.Split('.')[0]);

            for (var i = 0; i < totalSprites; i++)
            {
                var pos = Utilities.CalculatePosition(i, Columns);

                if (pos.X < tmpColumns && pos.Y < tmpRows)
                {
                    // Convert sprite to color index
                    ConvertColorsToIndexes(cps);

                    ProcessSpriteData();
                }

                index++;
            }

            if (tmpColumns < Columns || tmpRows < Rows)
            {
                ImageData.Resize(tmpColumns * tmpColumns * Constants.SpriteSize, tmpRows * Constants.SpriteSize);
            }

            StepCompleted();
        }

        protected override void ProcessSpriteData()
        {
            var id = spriteChip.FindSprite(spriteData);
            
            if (id == -1 && Utilities.IsEmpty(spriteData, Constants.EmptyPixel) == false)
            {
                id = spriteChip.NextEmptyId();
                
                spriteChip.UpdateSpriteAt(id, spriteData);
            }

            x = index % Columns;
            y = index / Columns;

            var tile = tilemapChip.GetTile(x, y);

            tile.SpriteId = id;

        }

        protected void SaveTilemap()
        {
            
            gameChip.SaveTilemap();

            gameChip.ClearTilemap();

            StepCompleted();
        }
    }

    public partial class Loader
    {
        [FileParser("tilemap.png", FileFlags.Tilemap)]
        public void ParseTilemapImage(string file, PixelVision engine)
        {
            AddParser(new TilemapParser(file, _imageParser, engine.ColorChip, engine.SpriteChip, engine.TilemapChip, engine.GameChip));
        }
    }
}