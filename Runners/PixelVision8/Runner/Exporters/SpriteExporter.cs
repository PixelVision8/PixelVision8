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
using PixelVision8.Runner.Parsers;

namespace PixelVision8.Runner.Exporters
{
    public class SpriteExporter : IAbstractExporter
    {
        protected Color[] colors;
        protected IEngine engine;
        protected PixelDataExporter exporter;
        protected string fullFileName;
        protected IImageExporter imageExporter;
        protected SpriteChip spriteChip;


        public SpriteExporter(string fileName, IEngine engine, IImageExporter imageExporter, SpriteChip spriteChip = null)
        {
            fullFileName = fileName;
            this.engine = engine;
            this.imageExporter = imageExporter;

            this.spriteChip = spriteChip ?? engine.spriteChip;

            var colorMapChip = engine.GetChip(ColorMapParser.chipName, false) as ColorChip;

            colors = colorMapChip == null ? engine.colorChip.colors : colorMapChip.colors;
        }

        public int totalSteps => exporter.totalSteps;

        public bool completed => exporter.completed;

        public virtual void CalculateSteps()
        {
            ConfigurePixelData();

            exporter.CalculateSteps();
        }

        public void NextStep()
        {
            exporter.NextStep();
        }

        public byte[] bytes => exporter.bytes;

        public string fileName => exporter.fileName;


        // TODO this should be a step in the exporter
        public virtual void ConfigurePixelData()
        {
            var width = spriteChip.textureWidth;
            var height = spriteChip.textureHeight;
            var pixelData = new int[width * height];

            spriteChip.texture.CopyPixels(ref pixelData, 0, 0, width, height);

            exporter = new PixelDataExporter(fullFileName, pixelData, width, height, colors, imageExporter);
        }
    }
}