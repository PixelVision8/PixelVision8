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
using System;

namespace PixelVision8.Engine.Utils
{
    public static class MathUtil
    {
        public static readonly Random Random = new Random();

        /// <summary>
        ///     Returns Ceil value as an int.
        /// </summary>
        /// <param name="value"></param>
        public static int CeilToInt(float value)
        {
            return (int)Math.Ceiling(value);
        }

        /// <summary>
        ///     Returns Floor value as an int.
        /// </summary>
        /// <param name="value"></param>
        public static int FloorToInt(float value)
        {
            return (int)Math.Floor(value);
        }

        /// <summary>
        ///     Converts an X and Y position into an index. This is useful for finding positions in 1D
        ///     arrays that represent 2D data.
        /// </summary>
        /// <param name="x">
        ///     The x position.
        /// </param>
        /// <param name="y">
        ///     The y position.
        /// </param>
        /// <param name="width">
        ///     The width of the data if it was represented as a 2D array.
        /// </param>
        /// <returns>
        ///     Returns an int value representing the X and Y position in a 1D array.
        /// </returns>
        public static int CalculateIndex(int x, int y, int width)
        {
            return x + y * width;
        }

        /// <summary>
        ///     Repeats a value based on the max. When the value is greater than the max, it starts
        ///     over at 0 plus the remaining value.
        /// </summary>
        /// <param name="val">
        ///     The value to repeat.
        /// </param>
        /// <param name="max">
        ///     The maximum the value can be.
        /// </param>
        /// <returns>
        ///     Returns an int that is never less than 0 or greater than the max.
        /// </returns>
        public static int Repeat(int val, int max)
        {
            return (int)(val - Math.Floor(val / (float)max) * max);
        }

        /// <summary>
        ///     Converts an index into an X and Y position to help when working with 1D arrays that
        ///     represent 2D data.
        /// </summary>
        /// <param name="index">
        ///     The position of the 1D array.
        /// </param>
        /// <param name="width">
        ///     The width of the data if it was a 2D array.
        /// </param>
        /// <returns>
        ///     Returns a vector representing the X and Y position of an index in a 1D array.
        /// </returns>
        public static Point CalculatePosition(int index, int width)
        {
            return new Point(index % width, index / width);
        }

        /// <summary>
        ///     Converts an index into an X and Y position to help when working with 1D arrays that
        ///     represent 2D data.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="index">
        ///     The position of the 1D array.
        /// </param>
        /// <param name="width">
        ///     The width of the data if it was a 2D array.
        /// </param>
        /// <returns>
        ///     Returns a vector representing the X and Y position of an index in a 1D array.
        /// </returns>
        public static void CalculatePosition(ref Point point, int index, int width)
        {
            point.X = index % width;
            point.Y = index / width;
        }

        public static int Clamp(int value, int min, int max)
        {
            value = value > max ? max : value;
            value = value < min ? min : value;
            return value;
        }

        // from https://stackoverflow.com/questions/16365870/distance-between-two-points-without-using-the-square-root
        public static int Sqrt(int x){
            int s, t;

            s = 1;  t = x;
            while (s < t) {
                s <<= 1;
                t >>= 1;
            }//decide the value of the first tentative

            do {
                t = s;
                s = (x / s + s) >> 1;//x1=(N / x0 + x0)/2 : recurrence formula
            } while (s < t);

            return t;
        }


    }
}