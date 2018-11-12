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

using GameCreator.Exporters;
using PixelVisionRunner.Parsers;
using PixelVisionSDK;
using PixelVisionSDK.Chips;
using PixelVisionSDK.Utils;

namespace PixelVisionRunner.Exporters
{
    public class FlagColorExporter : IAbstractExporter
    {
        
//        public static IColor ConvertFlagColor(int flagValue, int totalFlags)
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
        
        private string fullFileName;
        private ITextureFactory textureFactory;
        private PixelDataExporter exporter;
        private Vector tileSize;
        private int totalFlags;
        private GameChip gameChip;
        private ColorChip flagColorChip;
        
        public FlagColorExporter(string fileName, IEngineChips engineChips, ITextureFactory textureFactory)
        {
            fullFileName = fileName;

//            tilemapChip = engineChips.tilemapChip;

            totalFlags = engineChips.tilemapChip.totalFlags;

            gameChip = engineChips.gameChip;

            flagColorChip = engineChips.chipManager.GetChip(FlagColorParser.flagColorChipName, false) as ColorChip;
            
            tileSize = gameChip.SpriteSize();
            
            this.textureFactory = textureFactory;
        
        }
        
        public int currentStep
        {
            get { return exporter.currentStep; }
        }

        public int totalSteps
        {
            get { return exporter.totalSteps; }

        }

        public bool completed
        {
            get { return exporter.completed; }

        }
        public void CalculateSteps()
        {
            
            var cols = 16;
            var rows = MathUtil.CeilToInt(totalFlags / (float) cols);

            var w = cols * tileSize.x;
            var h = rows * tileSize.y;

            var canvas = new Pattern(w, h);
            canvas.Clear();

            var totalPixels = tileSize.x * tileSize.y;
            
            var brush = new int[totalPixels];

            IColor[] colors = new IColor[totalFlags];

            var flagColors = flagColorChip.colors;
            
            for (int i = 0; i < totalFlags; i++)
            {
                colors[i] = flagColors[i];
                
                var pos = gameChip.CalculatePosition(i, w);

                pos.x *= tileSize.x;
                pos.y *= tileSize.y;
                // Update the brush
                for (int j = 0; j < totalPixels; j++)
                {
                    brush[j] = i;
                }
                
                canvas.SetPixels(pos.x, pos.y, tileSize.x, tileSize.y, brush);

            }
            var imageExporter = new PNGWriter();

            exporter = new PixelDataExporter(fullFileName, canvas.pixels, w, h, colors, imageExporter);
            
            exporter.CalculateSteps();
        }

        public void NextStep()
        {
            exporter.NextStep();
        }

        public byte[] bytes
        {
            get { return exporter.bytes; }
        }

        public string fileName
        {
            get { return exporter.fileName; }
        }
        
        

        
    }
}