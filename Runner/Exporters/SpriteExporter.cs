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
using PixelVisionRunner.Parsers;
using PixelVisionSDK;
using PixelVisionSDK.Chips;
using PixelVisionSDK.Utils;

namespace PixelVisionRunner.Exporters
{
    public class SpriteExporter : IAbstractExporter
    {
        private string fullFileName;
        private IEngine engine;
        private ITextureFactory textureFactory;
        private PixelDataExporter exporter;
        
        public SpriteExporter(string fileName, IEngine engine, ITextureFactory textureFactory)
        {
            fullFileName = fileName;
            this.engine = engine;
            this.textureFactory = textureFactory;
            
            ConfigurePixelData();
            
            CalculateSteps();
        }
        
        // TODO this should be a step in the exporter
        public void ConfigurePixelData()
        {
            var spriteChip = engine.spriteChip;

            var totalSprite = spriteChip.totalSprites - 1;
            
            var width = spriteChip.textureWidth;
            var height = spriteChip.textureHeight;
            var sWidth = spriteChip.width;
            var sHeight = spriteChip.height;
            
            var emptyCount = 0;
            var tmpData = new int[sWidth * sHeight];
            var cols = (int)Math.Floor((float) width / sWidth);
            
            for (int i = totalSprite; i > -1; i--)
            {
                spriteChip.ReadSpriteAt(i, tmpData);
                
                if (spriteChip.IsEmpty(tmpData))
                {
                    emptyCount++;
                }
                
                if (i % cols == 0)
                {
                    if (emptyCount == cols)
                    {
                        height -= sHeight;
                    }

                    emptyCount = 0;
                }
            }
            
            var pixelData = new int[width * height];
            
            spriteChip.texture.CopyPixels(ref pixelData, 0, 0, width, height);
            
            if(textureFactory.flip)
                SpriteChipUtil.FlipSpriteData(ref pixelData, width, height, false, true);
            
            var colorMapChip = engine.chipManager.GetChip(ColorMapParser.chipName, false) as ColorChip;

            var colors = colorMapChip == null ? engine.colorChip.colors : colorMapChip.colors;
            
            exporter = new PixelDataExporter(fullFileName, pixelData, width, height, colors, textureFactory);
            
        }
        
//        public int currentStep
//        {
//            get { return exporter.currentStep; }
//        }

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