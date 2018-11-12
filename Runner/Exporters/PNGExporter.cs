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

using System.IO;


namespace PixelVisionRunner.Exporters
{
    public class PNGExporter : AbstractExporter
    {

        protected  IColor[] colors;
        protected  int height;
        protected  int width;
        protected int loops;
        protected IImageExporter imageExporter;
        
        public PNGExporter(string fileName, IImageExporter imageExporter, IColor[] colors = null) : base(fileName)
        {

            this.imageExporter = imageExporter;
            
            this.colors = colors;

        }

        public override void CalculateSteps()
        {
            base.CalculateSteps();

            steps.Add(WriteBytes);
        }

        protected virtual void WriteBytes()
        {

            if (colors.Length != width * height)
            {
                // TODO should we throw an error? The color data doesn't match the size
                return;
            }

            var stream = new MemoryStream();

            imageExporter.Write(width, height, stream, colors);
            
            bytes = stream.ToArray();
            
            currentStep++;
        }

    }
}