﻿//   
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
using Microsoft.Xna.Framework;
using PixelVision8.Engine.Chips;

namespace PixelVision8.Runner.Exporters
{
    public class ColorPaletteExporter : IAbstractExporter
    {
        protected Color[] colors;
        protected PixelDataExporter exporter;

        protected string fullFileName;
        protected int height;
        protected IImageExporter imageExporter;
        protected int[] pixels;
        protected int total;
        protected int width;

        public ColorPaletteExporter(string fileName, ColorChip colorChip, IImageExporter imageExporter)
        {
            fullFileName = fileName;

            this.colorChip = colorChip;

            this.imageExporter = imageExporter;
        }

        protected ColorChip colorChip { get; set; }

        public int totalSteps => exporter.totalSteps;

        public bool completed => exporter.completed;

        public void CalculateSteps()
        {
            var currentDebugMode = colorChip.debugMode;

            // Force the color chip to not replace empty colors with background value
            colorChip.debugMode = true;

            ConfigureColors();

            // Restore the color chip debug value
            colorChip.debugMode = currentDebugMode;

            BuildPixelData();

            // Create Pixel Data Exporter
            exporter = new PixelDataExporter(fullFileName, pixels, width, height, colors, imageExporter,
                colorChip.maskColor);

            // calculate steps for exporter
            exporter.CalculateSteps();
        }

        public void NextStep()
        {
            exporter.NextStep();
        }

        public byte[] bytes => exporter.bytes;

        public string fileName => exporter.fileName;

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
            for (var i = 0; i < totalPixels; i++)
                if (i < total)
                    pixels[i] = i;
                else
                    pixels[i] = -1;
        }
    }
}