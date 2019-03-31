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
    public class TilemapParser : SpriteParser
    {
        private readonly bool autoImport;

        private readonly TilemapChip tilemapChip;
//        private ITexture2D tileFlagTex;
//        private IColor clear;
//        
//        private int flag;
//        private int offset;

        public TilemapParser(IImageParser imageParser, byte[] tileFlagData, IEngineChips chips) :
            base(imageParser, chips)
        {
            tilemapChip = chips.tilemapChip;

            autoImport = tilemapChip.autoImport;

//            clear = new ColorData(0f){a = 0f};
            maskColor = ColorUtils.HexToColor(chips.colorChip.maskColor);
        }

        protected override void CalculateBounds()
        {
            // Calculate the texture's bounds
            base.CalculateBounds();

            columns = columns > tilemapChip.columns ? tilemapChip.columns : columns;

            rows = rows > tilemapChip.rows ? tilemapChip.rows : rows;

            // Recalculate total sprites
            totalSprites = columns * rows;
        }

        protected override void ProcessSpriteData()
        {
            var id = spriteChip.FindSprite(spriteData);

            if (id == -1 && autoImport)
            {
                id = spriteChip.NextEmptyID();
                spriteChip.UpdateSpriteAt(id, spriteData);
            }

            x = index % columns;
            y = index / columns;

            var tile = tilemapChip.GetTile(x, y);

            tile.spriteID = id;
//            tile.flag = flag;
//            tile.colorOffset = offset;
        }
    }
}