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
        private readonly bool autoImport;

        private readonly TilemapChip tilemapChip;
        private bool autoResize;
        private int Columns;
        private int Rows;

        public TilemapParser(string sourceFile, IImageParser parser, ColorChip colorChip, SpriteChip spriteChip,
            TilemapChip tilemapChip, bool autoResize = false) :
            base(sourceFile, parser, colorChip, spriteChip)
        {
            this.tilemapChip = tilemapChip;

            autoImport = tilemapChip.autoImport;
            this.autoResize = autoResize;
        }

        public override void CutOutSprites()
        {
            // TODO the image should be the right size from the beginning

            Columns = ImageData.Width / Constants.SpriteSize;
            Rows = ImageData.Height / Constants.SpriteSize;
            // int TotalSprites = Columns * Rows;
            
            if (autoResize)
                tilemapChip.Resize(Columns, Rows);

            // if(autoResize)
            // 
            var tmpColumns = Columns > tilemapChip.Columns ? tilemapChip.Columns : Columns;
            var tmpRows = Rows > tilemapChip.Rows ? tilemapChip.Rows : Rows;

            // Make sure the tilemap matches the image size
            // tilemapChip.Resize(image.Columns, image.Rows);

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
            
            if (id == -1 && autoImport)
            {
                id = spriteChip.NextEmptyId();
                Console.WriteLine("Add new tile {0}", id);

                spriteChip.UpdateSpriteAt(id, spriteData);
            }

            x = index % Columns;
            y = index / Columns;

            var tile = tilemapChip.GetTile(x, y);

            tile.SpriteId = id;
        }
    }

    public partial class Loader
    {
        [FileParser("tilemap.png", FileFlags.Tilemap)]
        public void ParseTilemapImage(string file, PixelVision engine)
        {
            Console.WriteLine("Add Tilemap Parser");
            AddParser(new TilemapParser(file, _imageParser, engine.ColorChip, engine.SpriteChip, engine.TilemapChip,
                true));
        }
    }
}