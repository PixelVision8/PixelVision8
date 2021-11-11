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

using System;
using System.Collections.Generic;

namespace PixelVision8.Player
{
    /// <summary>
    ///     Wrapper for texture data that includes Hex color data to rebuild colors when
    ///     exporting and cutting up sprites. 
    /// </summary>
    public struct ImageData
    {
        public PixelData<int> PixelData { get; }
        public int Width => PixelData.Width;
        public int Height => PixelData.Height;
        
        public string[] Colors;

        public bool Remapping;
        private int currentPixel;
        private int remapPixelBatch;
        
        private List<int> colorMap;
        
        public ImageData(int width, int height, int[] pixels = null, string[] colors = null)
        {
            // Create sprite friendly grid
            PixelData = new PixelData<int>(width, height);
            
            // Clear the pixel data with the default transparent color
            Utilities.Fill(PixelData, Constants.EmptyPixel);

            // See if there are any pixels
            if (pixels != null)
            {
                // Copy pixels over to PixelData at default si  ze
                Utilities.SetPixels(pixels, 0, 0, width, height, PixelData);
            }

            // Save a color lookup table
            Colors = colors ?? Constants.DefaultColors.Split(',');

            Remapping = false;
            currentPixel = 0;
            remapPixelBatch = 0;
            colorMap = null;

        }

        public int[] GetPixels(int x, int y, int width, int height) => Utilities.GetPixels(PixelData, x, y, width, height);
        
        public int[] GetPixels() => Utilities.GetPixels(PixelData);

        public void Resize(int newWidth, int newHeight) => Utilities.Resize(PixelData, newWidth, newHeight);

        public void Clear(int color = -1) => Utilities.Fill(PixelData, color);

        public void RemapColors(string[] colors, bool steps = false)
        {
            
            Remapping = true;


            colorMap = new List<int>();

            // Create color map
            for (int i = 0; i < Colors.Length; i++)
            {
                colorMap.Add(Array.IndexOf(colors, Colors[i]));
            }

            currentPixel = 0;
            remapPixelBatch = Math.Min(1000, PixelData.Total);

            if(steps == false)
            {
                while(Remapping)
                {
                    ContinueColorRemap();
                }
            }

            // Replace colors
            Array.Resize(ref Colors, colors.Length);
            Array.Copy(colors, Colors, colors.Length);
        }

        public void ContinueColorRemap()
        {
            if(Remapping == false)
                return;

            // Store pixel we are working with 
            int pixel;

            for (int i = 0; i < remapPixelBatch; i++)
            {

                pixel = PixelData[currentPixel];

                if(pixel > Constants.EmptyPixel && pixel < colorMap.Count)
                {
                    if(colorMap[pixel] > -1)
                        PixelData[currentPixel] = colorMap[pixel];
                }

                currentPixel ++;

                if(currentPixel >= PixelData.Total)
                {
                    Remapping = false;
                    
                    return;
                }
               
            }

        }

    }
}