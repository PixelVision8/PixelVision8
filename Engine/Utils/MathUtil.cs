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

using System;

namespace PixelVisionSDK.Utils
{

    public static class MathUtil
    {

        private static readonly Random random = new Random();

        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            if (val.CompareTo(max) > 0) return max;

            return val;
        }

        public static int Repeat(int val, int max)
        {
            if (val < 0F)
            {
                return max + val % max;
            }

            return val % max;
        }

        public static int FloorToInt(int a)
        {
            return (int) Math.Floor((double) a);
        }

        public static int CeilToInt(int a)
        {
            return (int) Math.Ceiling((double) a);
        }

        public static int RandomRange(int min, int max)
        {
            return random.Next(min, max);
        }

        public static int RoundToInt(float f)
        {
            return (int) Math.Round(f);
        }

    }

}