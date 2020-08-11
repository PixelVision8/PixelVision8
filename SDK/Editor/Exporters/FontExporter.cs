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

using PixelVision8.Engine;
using PixelVision8.Engine.Chips;
using PixelVision8.Runner.Parsers;

namespace PixelVision8.Runner.Exporters
{
    public class FontExporter : SpriteExporter
    {
        public FontExporter(string fileName, IEngine engine, IImageExporter imageExporter) : base(fileName, engine,
            imageExporter, engine.fontChip)
        {
        }

        // TODO this should be a step in the exporter
        public override void ConfigurePixelData()
        {
//            var spriteChip = engine.fontChip;

            var width = 96; //spriteChip.textureWidth;
            var height = 64; //spriteChip.textureHeight;


            var textureData = new TextureData(width, height);

//            var pixelData = new int[width * height];

            // Go through all of the sprites in the font

            // TODO get font sprites

            var total = 96;

            var maxCol = width / spriteChip.width;

            var tmpPixelData = new int[spriteChip.width * spriteChip.height];

            for (var i = 0; i < total; i++)
            {
                var pos = engine.gameChip.CalculatePosition(i, maxCol);

                spriteChip.ReadSpriteAt(i, tmpPixelData);

                textureData.SetPixels(pos.X * spriteChip.width, pos.Y * spriteChip.height, spriteChip.width,
                    spriteChip.height, tmpPixelData);
            }

            var colors = !(engine.GetChip(ColorMapParser.chipName, false) is ColorChip colorMapChip)
                ? engine.colorChip.colors
                : colorMapChip.colors;

            var imageExporter = new PNGWriter();

            exporter = new PixelDataExporter(fullFileName, textureData.pixels, width, height, colors, imageExporter,
                engine.colorChip.maskColor);
        }
    }
}