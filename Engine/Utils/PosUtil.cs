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
// 

namespace PixelVisionSDK.Engine.Utils
{

    /// <summary>
    ///     This utility offers helper methods for quickly calculating a position in
    ///     a 1D array as if it was a 2D array.
    /// </summary>
    public class PosUtil
    {

        /// <summary>
        ///     This calculates the <paramref name="index" /> of a 1D array based on
        ///     the x,y position and the <paramref name="width" /> of the array.
        /// </summary>
        /// <param name="x">The x position as an int. 0 is the far left.</param>
        /// <param name="y">The y poistion as an int. 0 is the top.</param>
        /// <param name="width">The width of the 1D array.</param>
        /// <param name="index">The returned value of the index.</param>
        public static void CalculateIndex(int x, int y, int width, out int index)
        {
            index = x + y * width;
        }

        /// <summary>
        ///     This calculates the position in the 1D array based on the index. It
        ///     returns an <paramref name="x" /> and <paramref name="y" /> position.
        /// </summary>
        /// <param name="index">The index in the 1D array.</param>
        /// <param name="width">The width of the 1D array.</param>
        /// <param name="x">
        ///     The returned x position in the array. 0 is the far left.
        /// </param>
        /// <param name="y">
        ///     The returned y position in the array. 0 is the top.
        /// </param>
        public static void CalculatePosition(int index, int width, out int x, out int y)
        {
            x = index % width;
            y = index / width;
        }

    }

}