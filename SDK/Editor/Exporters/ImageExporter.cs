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

// using Microsoft.Xna.Framework;
using System.IO;
using PixelVision8.Runner;
using PixelVision8.Runner.Exporters;

namespace PixelVision8.Editor
{
    public class ImageExporter : AbstractExporter
    {
        protected ColorData[] colors;
        protected int height;

        protected IImageExporter imageExporter;

        //        protected int loops;
        protected int width;

        public ImageExporter(string fileName, IImageExporter imageExporter, ColorData[] colors, int width, int height) :
            base(fileName)
        {
            this.imageExporter = imageExporter;

            this.colors = colors;
            this.width = width;
            this.height = height;
        }

        public override void CalculateSteps()
        {
            base.CalculateSteps();

            Steps.Add(WriteBytes);
        }

        protected virtual void WriteBytes()
        {
            if (colors.Length == width * height)
            {
                var stream = new MemoryStream();

                imageExporter.Write(width, height, stream, colors);

                Bytes = stream.ToArray();
            }

            CurrentStep++;
        }
    }
}