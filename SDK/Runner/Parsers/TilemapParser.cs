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

using PixelVision8.Engine.Chips;
using PixelVision8.Engine.Utils;

namespace PixelVision8.Runner.Parsers
{
    public class TilemapParser : SpriteImageParser
    {
        private readonly bool autoImport;

        private readonly TilemapChip tilemapChip;
        private bool autoResize;

        public TilemapParser(IImageParser parser, ColorChip colorChip, SpriteChip spriteChip, TilemapChip tilemapChip, bool autoResize = false) :
            base(parser, colorChip, spriteChip)
        {
            this.tilemapChip = tilemapChip;

            autoImport = tilemapChip.autoImport;
            this.autoResize = autoResize;
        }

        public override void CutOutSprites()
        {

            // TODO the image should be the right size from the beginning

            if (autoResize)
                tilemapChip.Resize(image.Columns, image.Rows);

            // if(autoResize)
            // 
            var tmpColumns = image.Columns > tilemapChip.columns ? tilemapChip.columns : image.Columns;
            var tmpRows = image.Rows > tilemapChip.rows ? tilemapChip.rows : image.Rows;

            // Make sure the tilemap matches the image size
            // tilemapChip.Resize(image.Columns, image.Rows);

            for (var i = 0; i < totalSprites; i++)
            {

                var pos = MathUtil.CalculatePosition(i, image.Columns);

                if (pos.X < tmpColumns && pos.Y < tmpRows)
                {
                    // Convert sprite to color index
                    ConvertColorsToIndexes(cps);

                    ProcessSpriteData();
                }

                index++;

            }

            if (tmpColumns < image.Columns || tmpRows < image.Rows)
            {
                image.Resize(tmpColumns * tmpColumns * spriteChip.width, tmpRows * spriteChip.height);
            }

            StepCompleted();
        }

        protected override void ProcessSpriteData()
        {
            var id = spriteChip.FindSprite(spriteData);

            if (id == -1 && autoImport)
            {
                id = spriteChip.NextEmptyID();
                spriteChip.UpdateSpriteAt(id, spriteData);
            }

            x = index % image.Columns;
            y = index / image.Columns;

            var tile = tilemapChip.GetTile(x, y);

            tile.SpriteId = id;

        }
    }
}