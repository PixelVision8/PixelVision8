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

        public string FilePath;

        public PNGParser(string filePath, GraphicsDevice graphicsDevice, string maskHex = "#FF00FF")
        {
            FilePath = filePath;
            GraphicsDevice = graphicsDevice;
        }

        public string MaskHex => "#FF00FF";
        public int width { get; private set; }
        public int height { get; private set; }
        public Color[] colorPixels { get; private set; }
        public List<Color> colorPalette { get; private set; }

        public void ReadStream()
        {
            if (!string.IsNullOrEmpty(FilePath))
            {
                // Load the png file into a Texture 2D
                var t2D = Texture2D.FromFile(GraphicsDevice, FilePath);

                // Set the width and height
                width = t2D.Width;
                height = t2D.Height;

                // Calculate the total pixels
                var totalPixels = t2D.Width * t2D.Height;

                // Get the color pixels from the texture 2D
                colorPixels = new Color[totalPixels];
                t2D.GetData(colorPixels);

                // Create a palette made up of unique colors
                colorPalette = new List<Color>();
                for (int i = 0; i < totalPixels; i++)
                {
                    if (colorPalette.IndexOf(colorPixels[i]) == -1)
                        colorPalette.Add(colorPixels[i]);
                }

            }

        }

        public string FileName
        {
            get => Path.GetFileName(FilePath);
            set
            {
                // Do nothing
            }
        }
    }
}