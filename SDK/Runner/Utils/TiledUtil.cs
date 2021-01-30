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

namespace PixelVision8.Runner
{
    public class TiledUtil
    {
        public static uint CreateGID(int id, bool flipH, bool flipV)
        {
            var gid = (uint) id;

            if (flipH) gid |= 1U << 31;

            if (flipV) gid |= 1U << 30;

            return gid;
        }

        public static void ReadGID(uint gid, out int id, out bool flipH, out bool flipV)
        {
            // Starts with 0, 31 in an int

            // Create mask by subtracting the bits you don't want

            var idMask = (1 << 30) - 1;

            id = (int) (gid & idMask);

            var hMask = 1 << 31;

            flipH = (hMask & gid) != 0;

            var vMask = 1 << 30;

            flipV = (vMask & gid) != 0;
        }
    }
}