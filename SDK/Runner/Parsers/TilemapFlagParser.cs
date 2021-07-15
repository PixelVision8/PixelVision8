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
    public class TilemapFlagParser : TilemapParser
    {

        public TilemapFlagParser(string sourceFile, IImageParser parser, ColorChip colorChip, SpriteChip spriteChip,
            TilemapChip tilemapChip, GameChip gameChip) :
            base(sourceFile, parser, colorChip, spriteChip, tilemapChip, gameChip)
        {
            
        }

        protected override void ProcessSpriteData()
        {
            var id = spriteChip.FindSprite(spriteData);

            x = index % Columns;
            y = index / Columns;

            var tile = tilemapChip.GetTile(x, y);
            tile.Flag = id;
        }

    }

    public partial class Loader
    {
        [FileParser(".flags.png", FileFlags.Tilemap)]
        public void ParseTilemapFlagImage(string file, PixelVision engine)
        {
            
            var tmpColorChip = new ColorChip();

            AddParser(new TilemapFlagParser(file, _imageParser, tmpColorChip, flagSpriteChip, engine.TilemapChip, engine.GameChip));
            
        }
    }
}