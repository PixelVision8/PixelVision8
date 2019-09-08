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

using Microsoft.Xna.Framework;
using PixelVision8.Engine;
using PixelVision8.Engine.Chips;
using PixelVision8.Engine.Utils;
using PixelVision8.Runner.Parsers;

namespace PixelVision8.Runner.Exporters
{
    public class FlagColorExporter : IAbstractExporter
    {
//        private ITextureFactory textureFactory;
        private PixelDataExporter exporter;
        private readonly ColorChip flagColorChip;

//        public static Color ConvertFlagColor(int flagValue, int totalFlags)
//        {
//            var colorValue = CalcualteFlagColor(flagValue, totalFlags);
//                
//            return new ColorData(colorValue, colorValue, colorValue);
//        }
//
//        public static float CalcualteFlagColor(int flagValue, int totalFlags)
//        {
//            return (flagValue * totalFlags) / 256f;
//        }

        private readonly string fullFileName;
        private readonly GameChip gameChip;
        private readonly Point tileSize;
        private readonly int totalFlags;

        public FlagColorExporter(string fileName, IEngineChips engineChips)
        {
            fullFileName = fileName;

//            tilemapChip = engineChips.tilemapChip;

            totalFlags = engineChips.tilemapChip.totalFlags;

            gameChip = engineChips.gameChip;

            flagColorChip = engineChips.GetChip(FlagColorParser.flagColorChipName, false) as ColorChip;

            tileSize = gameChip.SpriteSize();

//            this.textureFactory = textureFactory;
        }

        public int currentStep => exporter.currentStep;

        public int totalSteps => exporter.totalSteps;

        public bool completed => exporter.completed;

        public void CalculateSteps()
        {
            var cols = 16;
            var rows = MathUtil.CeilToInt(totalFlags / (float) cols);

            var w = cols * tileSize.X;
            var h = rows * tileSize.Y;

            var canvas = new TextureData(w, h);
            canvas.Clear();

            var totalPixels = tileSize.X * tileSize.Y;

            var brush = new int[totalPixels];

            var colors = new Color[totalFlags];

            var flagColors = flagColorChip.colors;

            for (var i = 0; i < totalFlags; i++)
            {
                colors[i] = flagColors[i];

                var pos = gameChip.CalculatePosition(i, w);

                pos.X *= tileSize.X;
                pos.Y *= tileSize.Y;
                // Update the brush
                for (var j = 0; j < totalPixels; j++) brush[j] = i;

                canvas.SetPixels(pos.X, pos.Y, tileSize.X, tileSize.Y, brush);
            }

            var imageExporter = new PNGWriter();

            exporter = new PixelDataExporter(fullFileName, canvas.pixels, w, h, colors, imageExporter);

            exporter.CalculateSteps();
        }

        public void NextStep()
        {
            exporter.NextStep();
        }

        public byte[] bytes => exporter.bytes;

        public string fileName => exporter.fileName;
    }
}