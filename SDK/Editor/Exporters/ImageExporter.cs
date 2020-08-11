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

using System.IO;
using Microsoft.Xna.Framework;

namespace PixelVision8.Runner.Exporters
{
    public class ImageExporter : AbstractExporter
    {
        protected Color[] colors;
        protected int height;

        protected IImageExporter imageExporter;

//        protected int loops;
        protected int width;

        public ImageExporter(string fileName, IImageExporter imageExporter, Color[] colors, int width, int height) :
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

            steps.Add(WriteBytes);
        }

        protected virtual void WriteBytes()
        {
            if (colors.Length == width * height)
            {
                var stream = new MemoryStream();

                imageExporter.Write(width, height, stream, colors);

                bytes = stream.ToArray();
            }

            currentStep++;
        }
    }
}