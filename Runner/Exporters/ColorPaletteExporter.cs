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
using GameCreator.Exporters;
using PixelVisionSDK.Chips;

namespace PixelVisionRunner.Exporters
{
    public class ColorPaletteExporter : IAbstractExporter
    {
        
        private string fullFileName;
        private ITextureFactory textureFactory;
        private PixelDataExporter exporter;
        private ColorChip colorChip { get; set; }
        
        public ColorPaletteExporter(string fileName, ColorChip colorChip, ITextureFactory textureFactory)
        {
            fullFileName = fileName;

            this.colorChip = colorChip;
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
            // Force the color chip to not replace empty colors with background value
            colorChip.debugMode = true;
            
            var colors = colorChip.colors;
            var total = colors.Length;

            var width = 8;
            var height = (int) Math.Ceiling(total / (float) width);

            var totalPixels = width * height;
            
            var pixels = new int[totalPixels];
            for (int i = 0; i < totalPixels; i++)
            {
                if (i < total)
                {
                    pixels[i] = i;
                }
                else
                {
                    pixels[i] = -1;
                }
                
            }
            
            var imageExporter = new PNGWriter();

            
            exporter = new PixelDataExporter(fullFileName, pixels, width, height, colors, imageExporter);
            
            // Reset color chip value
            colorChip.debugMode = false;
            
            exporter.CalculateSteps();
        }

        public void NextStep()
        {
            exporter.NextStep();
        }

        public byte[] bytes
        {
            get { return exporter.bytes; }
            set { throw new NotImplementedException(); }
        }

        public string fileName
        {
            get { return exporter.fileName; }
        }
        
    }
}