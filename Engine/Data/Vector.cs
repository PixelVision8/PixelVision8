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

namespace PixelVisionSDK
{

    /// <summary>
    ///     The Vector represents an int x and int y value for position.
    /// </summary>
    public class Vector
    {

        /// <summary>
        ///     X value of the Vector
        /// </summary>
        public int x;

        /// <summary>
        ///     Y value of the vector
        /// </summary>
        public int y;

        /// <summary>
        ///     Create a new Vector instance by supplying an int x and int y
        ///     value. The default values are 0.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public Vector(int x = 0, int y = 0)
        {
            this.x = x;
            this.y = y;
        }

    }

}