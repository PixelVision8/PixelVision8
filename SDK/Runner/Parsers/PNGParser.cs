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
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;

namespace PixelVision8.Runner
{
    public class PNGParser : IImageParser
    {
        public GraphicsDevice GraphicsDevice;

        public string FileName { get; set; } = "untitled";

        public PNGParser(GraphicsDevice graphicsDevice)
        {
            GraphicsDevice = graphicsDevice;
        }

        // public string MaskHex { get; private set; } = "#FF00FF";
        public int Width { get; private set; }
        public int Height { get; private set; }
        public ColorData[] ColorPixels { get; private set; }
        public List<ColorData> ColorPalette { get; private set; }

        public void ReadStream(string sourcePath, string maskHex)
        {
            // TODO not using mask color here
            FileName = Path.GetFileName(sourcePath);

            // Load the png file into a Texture 2D
            var t2D = Texture2D.FromFile(GraphicsDevice, sourcePath);

            // Set the width and height
            Width = t2D.Width;
            Height = t2D.Height;

            // Calculate the total pixels
            var totalPixels = t2D.Width * t2D.Height;

            // Get the color pixels from the texture 2D
            ColorPixels = new ColorData[totalPixels];
            t2D.GetData(ColorPixels);

            // Create a palette made up of unique colors
            ColorPalette = new List<ColorData>();
            for (int i = 0; i < totalPixels; i++)
            {
                if (ColorPalette.IndexOf(ColorPixels[i]) == -1)
                    ColorPalette.Add(ColorPixels[i]);
            }

            // }
        }
    }
}