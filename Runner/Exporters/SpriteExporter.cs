﻿//
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

using Microsoft.Xna.Framework;
using PixelVision8.Engine;
using PixelVision8.Engine.Chips;
using PixelVision8.Runner.Parsers;


 namespace PixelVision8.Runner.Exporters
{
    public class SpriteExporter : IAbstractExporter
    {
        protected string fullFileName;
        protected IEngine engine;
        protected PixelDataExporter exporter;
        protected IImageExporter imageExporter;
        protected SpriteChip spriteChip;
        protected Color[] colors;


        public SpriteExporter(string fileName, IEngine engine, IImageExporter imageExporter)
        {
            fullFileName = fileName;
            this.engine = engine;
            this.imageExporter = imageExporter;
            
            spriteChip = engine.spriteChip;
            
            var colorMapChip = engine.GetChip(ColorMapParser.chipName, false) as ColorChip;

            colors = colorMapChip == null ? engine.colorChip.colors : colorMapChip.colors;

        }

        
        // TODO this should be a step in the exporter
        public virtual void ConfigurePixelData()
        {
            
            var width = spriteChip.textureWidth;
            var height = spriteChip.textureHeight;
            var pixelData = new int[width * height];
            
            spriteChip.texture.CopyPixels(ref pixelData, 0, 0, width, height);
            
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
        public virtual void CalculateSteps()
        {
            ConfigurePixelData();
            
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