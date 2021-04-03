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

using PixelVision8.Player;
using PixelVision8.Runner.Exporters;

namespace PixelVision8.Editor
{
    public class FontExporter : SpriteExporter
    {
        public FontExporter(string fileName, PixelVision engine, IImageExporter imageExporter) : base(fileName, engine,
            imageExporter, engine.FontChip)
        {
        }

        // TODO this should be a step in the exporter
        public override void ConfigurePixelData()
        {
            //            var spriteChip = engine.fontChip;

            var width = 96; //spriteChip.textureWidth;
            var height = 64; //spriteChip.textureHeight;


            var textureData = new PixelData(width, height);

            //            var pixelData = new int[width * height];

            // Go through all of the sprites in the font

            // TODO get font sprites

            var total = 96;

            var maxCol = width / Constants.SpriteSize;

            var tmpPixelData = new int[Constants.SpriteSize * Constants.SpriteSize];

            for (var i = 0; i < total; i++)
            {
                var pos = Utilities.CalculatePosition(i, maxCol);

                spriteChip.ReadSpriteAt(i, ref tmpPixelData);
                Utilities.SetPixels(tmpPixelData, pos.X * Constants.SpriteSize, pos.Y * Constants.SpriteSize, Constants.SpriteSize,
                    Constants.SpriteSize, textureData);
            }

            // var convertedColors = Utilities.ConvertColors(engine.ColorChip.hexColors, engine.ColorChip.maskColor, true);

            // var colors = !(engine.GetChip(ColorMapParser.chipName, false) is ColorChip colorMapChip)
            //     ? engine.ColorChip.colors
            //     : colorMapChip.colors;

            var imageExporter = new PNGWriter();

            // TODO use the colors from the sprite parser this class extends?

            exporter = new PixelDataExporter(fullFileName, textureData.Pixels, width, height, colors, imageExporter,
                engine.ColorChip.MaskColor);
        }
    }
}