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

namespace PixelVision8.Engine.Utils
{
    public static class MathUtil
    {
        public static readonly Random random = new Random();

        /// <summary>
        ///     Returns Ceil value as an int.
        /// </summary>
        /// <param name="value"></param>
        public static int CeilToInt(float value)
        {
            return (int) Math.Ceiling(value);
        }

        /// <summary>
        ///     Returns Floor value as an int.
        /// </summary>
        /// <param name="value"></param>
        public static int FloorToInt(float value)
        {
            return (int) Math.Floor(value);
        }

    }
}