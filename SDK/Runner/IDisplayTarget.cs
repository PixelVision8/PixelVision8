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
using PixelVision8.Engine.Chips;

namespace PixelVision8.Runner
{
    public interface IDisplayTarget
    {
        void ResetResolution(int gameWidth, int gameHeight, int overScanX = 0, int overScanY = 0);
        void RebuildColorPalette(ColorChip colorChip);
        void Render(int[] pixels, int defaultColor = 0);
        int monitorScale { get; set; }
        bool fullscreen { get; set; }
        bool stretchScreen { get; set; }
        bool cropScreen { get; set; }
        Vector2 Scale { get; }
    }
}