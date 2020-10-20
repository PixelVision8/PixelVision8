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
using Microsoft.Xna.Framework;
using PixelVision8.Engine.Chips;
using PixelVision8.Engine.Utils;

namespace PixelVision8.Engine
{
    public class CanvasDrawRequest
    {
        public string Action;
        public Rectangle Bounds = Rectangle.Empty;
        public PixelData PixelData = new PixelData();
        public bool Fill;
        public bool FlipH;
        public bool FlipV;
        public int ColorOffset;
        
        
        
        
        // Depricate
        public int X0;
        public int X1;
        public int Y0;
        public int Y1;
        
        // public string Text;
        // public string Font;
        // public int Spacing;
        // public PixelData Stroke = new PixelData();
        // public PixelData Pattern = new PixelData();
        // public PixelData TargetTexture;
    }
}