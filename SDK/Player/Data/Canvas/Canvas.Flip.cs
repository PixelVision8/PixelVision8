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

namespace PixelVision8.Player
{

    public sealed partial class Canvas
    {

        public void Flip(bool horizontal = false, bool vertical = false, int x =0, int y= 0, int width = 0, int height = 0)
        {


            if(width <= 0 || width > Width)
                width = Width;
            

            if(height <= 0 || height > Height)
                height = Height;
            

            x = Math.Max(0, Math.Min(width, x));
            y = Math.Max(0, Math.Min(height, y));


            var data = GetPixels(x, y, width, height);

            Utilities.FlipPixelData(ref data, width, height, horizontal, vertical);

            SetPixels(x, y, width, height, data);
        
        }

    }

}