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

using System;

namespace PixelVisionSDK.Utils
{

    public static class MathUtil
    {

        public static readonly Random random = new Random();

        /// <summary>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="val"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0)
                return min;
            if (val.CompareTo(max) > 0)
                return max;

            return val;
        }

//        /// <summary>
//        ///     Repeats a value based on a max number.
//        /// </summary>
//        /// <param name="val"></param>
//        /// <param name="max"></param>
//        /// <returns></returns>
//        public static int Repeat(int val, int max)
//        {
//            return (int) (val - Math.Floor(val / (float) max) * max);
//        }
//
//        /// <summary>
//        ///     Returns a random int between a min and max range.
//        /// </summary>
//        /// <param name="min"></param>
//        /// <param name="max"></param>
//        /// <returns></returns>
//        public static int RandomRange(int min, int max)
//        {
//            return random.Next(min, max);
//        }

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

        /// <summary>
        ///     Returns Round value as an int.
        /// </summary>
        /// <param name="value"></param>
        public static int RoundToInt(float value)
        {
            return (int) Math.Round(value);
        }

        public static int RandomRange(int min, int max)
        {
            return random.Next(min, max);
        }
        
        public static float RandomValue()
        {
            return random.Next(0, 100)/100f;
        }
    }

}