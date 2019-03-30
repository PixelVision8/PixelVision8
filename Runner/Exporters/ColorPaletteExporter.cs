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
using Microsoft.Xna.Framework;
using PixelVision8.Engine.Chips;

namespace PixelVision8.Runner.Exporters
{
    public class ColorPaletteExporter : IAbstractExporter
    {
        
        protected string fullFileName;
        protected PixelDataExporter exporter;
        protected ColorChip colorChip { get; set; }
        protected IImageExporter imageExporter;
        protected Color[] colors;
        protected int width;
        protected int height;
        protected int total;
        protected int[] pixels;

        public ColorPaletteExporter(string fileName, ColorChip colorChip, IImageExporter imageExporter)
        {
        
            fullFileName = fileName;

            this.colorChip = colorChip;

            this.imageExporter = imageExporter;
            
        }
        
        public int totalSteps => exporter.totalSteps;

        public bool completed => exporter.completed;

        public virtual void ConfigureColors()
        {
            colors = colorChip.colors;
            total = colors.Length;

            width = 8;
            height = (int) Math.Ceiling(total / (float) width);
            
        }

        public virtual void BuildPixelData()
        {
            var totalPixels = width * height;
            
            pixels = new int[totalPixels];
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
        }

        public void CalculateSteps()
        {
            // Force the color chip to not replace empty colors with background value
            colorChip.debugMode = true;

            ConfigureColors();
            
            // Reset color chip value
            colorChip.debugMode = false;

            BuildPixelData();

            // Create Pixel Data Exporter
            exporter = new PixelDataExporter(fullFileName, pixels, width, height, colors, imageExporter);
            
            // calculate steps for exporter
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