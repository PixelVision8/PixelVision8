//  
// Copyright (c) Jesse Freeman. All rights reserved.  
// 
// Licensed under the Microsoft Public License (MS-PL) License. 
// See LICENSE file in the project root for full license information. 
// 
// Contributors
// --------------------------------------------------------
// This is the official list of Pixel Vision 8 contributors:
//  
// Jesse Freeman - @JesseFreeman
// Christer Kaitila - @McFunkypants
// Pedro Medeiros - @saint11
// Shawn Rakowski - @shwany
// 

namespace PixelVisionRunner.Chips.Sfxr
{
    using System;

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
            return (float)Math.Pow(n, power);
        }

        internal static float Min(float v1, float v2)
        {
            return Math.Min(v1, v2);
        }

        internal static float Sin(float _vibratoPhase)
        {
            return (float)Math.Sin((double)_vibratoPhase);
        }

        internal static float Max(float v1, float v2)
        {
            return Math.Max(v1, v2);
        }
    }
}
