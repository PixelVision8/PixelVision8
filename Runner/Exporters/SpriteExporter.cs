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

namespace PixelVisionRunner.Exporters
{
    public class SpriteExporter : IAbstractExporter
    {
        protected string fullFileName;
        protected IEngine engine;
        protected PixelDataExporter exporter;
        
        public SpriteExporter(string fileName, IEngine engine)
        {
            fullFileName = fileName;
            this.engine = engine;
            
            ConfigurePixelData();
            
            CalculateSteps();
        }
        
        // TODO this should be a step in the exporter
        public virtual void ConfigurePixelData()
        {
            var spriteChip = engine.spriteChip;

            var width = spriteChip.textureWidth;
            var height = spriteChip.textureHeight;
            
            var pixelData = new int[width * height];
            
            spriteChip.texture.CopyPixels(ref pixelData, 0, 0, width, height);
            
            var colorMapChip = engine.chipManager.GetChip(ColorMapParser.chipName, false) as ColorChip;

            var colors = colorMapChip == null ? engine.colorChip.colors : colorMapChip.colors;
            
            var imageExporter = new PNGWriter();
            
            exporter = new PixelDataExporter(fullFileName, pixelData, width, height, colors, imageExporter);
            
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