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

namespace PixelVision8.Runner.Chips.Sfxr
{
    internal static class Mathf
    {
        public static float Clamp(float input, float min, float max)
        {
            return input < min
                ? min
                : input > max
                    ? max
                    : input;
        }

        internal static float Pow(float n, float power)
        {
            return (float) Math.Pow(n, power);
        }

        internal static float Min(float v1, float v2)
        {
            return Math.Min(v1, v2);
        }

        internal static float Sin(float _vibratoPhase)
        {
            return (float) Math.Sin(_vibratoPhase);
        }

        internal static float Max(float v1, float v2)
        {
            return Math.Max(v1, v2);
        }
    }
}