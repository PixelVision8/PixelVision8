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

namespace PixelVisionSDK
{

    /// <summary>
    ///     The Rect is a basic rectangle class used for determining
    ///     bounding box collision based on x, y , width and hight values.
    /// </summary>
    public class Rect
    {

        /// <summary>
        ///     Height of the Rect.
        /// </summary>
        public int height;

        /// <summary>
        ///     Width of the Rect.
        /// </summary>
        public int width;

        /// <summary>
        ///     X position of the Rect.
        /// </summary>
        public int x;

        /// <summary>
        ///     Y position of the Rect.
        /// </summary>
        public int y;

        /// <summary>
        ///     The Rect constructor takes valuues for the x, y, width and height
        ///     of the new instance. The default values for each is 0.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public Rect(int x = 0, int y = 0, int width = 0, int height = 0)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        /// <summary>
        /// </summary>
        /// <param name="targetA"></param>
        /// <param name="targetB"></param>
        public void Intersect(Rect targetA, ref Rect targetB)
        {
            //var rect = new Rect();
            var tmpX = Math.Max(x, targetA.x);
            var num1 = Math.Min(x + width, targetA.x + targetA.width);
            var tmpY = Math.Max(y, targetA.y);
            var num2 = Math.Min(y + height, targetA.y + targetA.height);
            if (num1 >= tmpX && num2 >= tmpY)
            {
                targetB.x = tmpX;
                targetB.y = tmpY;
                targetB.width = num1 - tmpX;
                targetB.height = num2 - tmpY;
            }
            else
            {
                targetB.x = 0;
                targetB.y = 0;
                targetB.width = 0;
                targetB.height = 0;
            }
        }

        /// <summary>
        ///     Detects if two rectangles intersetcs. Returns true
        ///     if there is a collision.
        /// </summary>
        /// <param name="a">Rectangle A</param>
        /// <param name="target">Rectangle B</param>
        /// <returns>
        ///     Returns true if there is an intersection or
        ///     false if there is none.
        /// </returns>
        public bool IntersectsWith(Rect target)
        {
            if (target.x < x + width && x < target.x + target.width && target.y < y + height)
                return y < target.y + target.height;

            return false;
        }

    }

}