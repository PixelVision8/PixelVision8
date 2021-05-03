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
using System.Collections.Generic;

namespace PixelVision8.Player
{
    public sealed partial class Canvas
    {
        public void FloodFill(int x, int y)
        {
            var getRequest = NextRequest();

            if (getRequest == null)
                return;

            var newRequest = getRequest;

            newRequest.Action = FloodFillAction;
            newRequest.Bounds.X = x;
            newRequest.Bounds.Y = y;

            // Save the changes to the request
            requestPool[currentRequest] = newRequest;
        }

        /// <summary>
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        [DrawAction]
        public void FloodFillAction(CanvasDrawRequest request)
        {
            // if (request.Bounds.X < 0 || request.Bounds.Y < 0 || request.Bounds.X > Width ||
            //     request.Bounds.Y > Height) return;

            // var targetColor = GetPixel(currentTexture, request.Bounds.X, request.Bounds.Y);
            // var replacementColor = 0;
            
            // var pt = new Point(request.Bounds.X, request.Bounds.Y);

            // Queue<Point> q = new Queue<Point>();
            // q.Enqueue(pt);
            // while (q.Count > 0)
            // {
            //     Point n = q.Dequeue();
            //     if (!ColorMatch(GetPixel(currentTexture, n.X, n.Y),targetColor))
            //         continue;
            //     Point w = n, e = new Point(n.X + 1, n.Y);
            //     while ((w.X >= 0) && ColorMatch(GetPixel(currentTexture,w.X, w.Y),targetColor))
            //     {
            //         SetPatternPixel(currentTexture, w.X, w.Y, replacementColor);
            //         if ((w.Y > 0) && ColorMatch(GetPixel(currentTexture,w.X, w.Y - 1),targetColor))
            //             q.Enqueue(new Point(w.X, w.Y - 1));
            //         if ((w.Y < currentTexture.Height - 1) && ColorMatch(GetPixel(currentTexture,w.X, w.Y + 1),targetColor))
            //             q.Enqueue(new Point(w.X, w.Y + 1));
            //         w.X--;
            //     }
            //     while ((e.X <= currentTexture.Width - 1) && ColorMatch(GetPixel(currentTexture,e.X, e.Y),targetColor))
            //     {
            //         SetPatternPixel(currentTexture, e.X, e.Y, replacementColor);
            //         if ((e.Y > 0) && ColorMatch(GetPixel(currentTexture,e.X, e.Y - 1), targetColor))
            //             q.Enqueue(new Point(e.X, e.Y - 1));
            //         if ((e.Y < currentTexture.Height - 1) && ColorMatch(GetPixel(currentTexture,e.X, e.Y + 1), targetColor))
            //             q.Enqueue(new Point(e.X, e.Y + 1));
            //         e.X++;
            //     }
            // }

            // Get the color at the point where we are trying to fill and use that to match all the color inside the shape
            var targetColor = GetPixel(currentTexture, request.Bounds.X, request.Bounds.Y);

            var pixels = new Stack<Point>();

            pixels.Push(new Point(request.Bounds.X, request.Bounds.Y));

            while (pixels.Count != 0)
            {
                var temp = pixels.Pop();
                var y1 = temp.Y;
                while (y1 >= 0 && GetPixel(currentTexture, temp.X, y1) == targetColor) y1--;

                y1++;
                var spanLeft = false;
                var spanRight = false;
                while (y1 < Height && GetPixel(currentTexture, temp.X, y1) == targetColor)
                {
                    SetPixel(currentTexture, temp.X, y1, GetPixel(pattern, temp.X, y1));

                    if (!spanLeft && temp.X > 0 && GetPixel(currentTexture, temp.X - 1, y1) == targetColor)
                    {
                        if (GetPixel(currentTexture, temp.X - 1, y1) != GetPixel(pattern, temp.X, y1))
                            pixels.Push(new Point(temp.X - 1, y1));

                        spanLeft = true;
                    }
                    else if (spanLeft && temp.X - 1 == 0 && GetPixel(currentTexture, temp.X - 1, y1) != targetColor)
                    {
                        spanLeft = false;
                    }

                    if (!spanRight && temp.X < Width - 1 && GetPixel(currentTexture, temp.X + 1, y1) == targetColor)
                    {
                        if (GetPixel(currentTexture, temp.X + 1, y1) != GetPixel(pattern, temp.X, y1))
                            pixels.Push(new Point(temp.X + 1, y1));

                        spanRight = true;
                    }
                    else if (spanRight && temp.X < Width - 1 && GetPixel(currentTexture, temp.X + 1, y1) != targetColor)
                    {
                        spanRight = false;
                    }

                    y1++;
                }
            }
        }

        private void SetPatternPixel(PixelData currentTexture, int x, int y, int replacementColor)
        {
            // Console.WriteLine("{0},{1}", x, y);
            replacementColor = GetPixel(pattern, x % pattern.Width, y % pattern.Height);
            
            SetPixel(currentTexture, x, y, replacementColor);
        }

        private static bool ColorMatch(int a, int b)
        {
            return a == b;
        }
    }
}