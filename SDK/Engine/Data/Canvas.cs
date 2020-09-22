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
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using PixelVision8.Engine.Chips;
using PixelVision8.Engine.Utils;

namespace PixelVision8.Engine
{
    public class Canvas : TextureData
    {
        private readonly GameChip gameChip;
        private readonly TextureData pattern;
        private readonly Point spriteSize;
        private readonly TextureData stroke;

        private readonly int[] tmpPixelData = new int[0];
        private bool canDraw;
        private bool drawCentered;
        private Point linePattern = new Point(1, 0);

        protected int[] tmpIDs = new int[0];
        public bool wrap = false;

        public Canvas(int width, int height, GameChip gameChip = null) : base(width, height)
        {
            this.gameChip = gameChip;
            pattern = new TextureData();
            pattern.SetPixel(0, 0, 0);

            stroke = new TextureData();
            stroke.SetPixel(0, 0, 0);

            spriteSize = gameChip.SpriteSize();
        }

        public void LinePattern(int x, int y)
        {
            linePattern.X = x;
            linePattern.Y = y;
        }

        public bool DrawCentered(bool? newValue = null)
        {
            if (newValue.HasValue) drawCentered = newValue.Value;

            return drawCentered;
        }

        public void SetStroke(int[] pixels, int width, int height)
        {
            //            var total = width * height;

            if (stroke.width != width || pattern.height != height) stroke.Resize(width, height);

            stroke.SetPixels(pixels);
        }

        public void SetPattern(int[] pixels, int width, int height)
        {
            //            var total = width * height;

            if (pattern.width != width || pattern.height != height) pattern.Resize(width, height);

            pattern.SetPixels(pixels);
        }

        


        /// <summary>
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void SetStrokePixel(int x, int y)
        {
            canDraw = wrap || x >= 0 && x <= _width - stroke.width && y >= 0 && y <= _height - stroke.height;
            //            
            if (canDraw) SetPixels(x, y, stroke.width, stroke.height, stroke.pixels);
        }

        /// <summary>
        /// </summary>
        /// <param name="x0"></param>
        /// <param name="y0"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        public void DrawLine(int x0, int y0, int x1, int y1)
        {
            var counter = 0;

            var dx = x1 - x0;
            int sx;
            if (dx < 0)
            {
                dx = -dx;
                sx = -1;
            }
            else
            {
                sx = 1;
            }

            var dy = y1 - y0;
            int sy;
            if (dy < 0)
            {
                dy = -dy;
                sy = -1;
            }
            else
            {
                sy = 1;
            }

            int err = (dx > dy ? dx : -dy) / 2, e2;
            for (;;)
            {
                if (counter % linePattern.X == linePattern.Y) SetStrokePixel(x0, y0);

                counter++;
                if (x0 == x1 && y0 == y1) break;

                e2 = err;
                if (e2 > -dx)
                {
                    err -= dy;
                    x0 += sx;
                }

                if (e2 < dy)
                {
                    err += dx;
                    y0 += sy;
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="x0"></param>
        /// <param name="y0"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="fill"></param>
        public void DrawSquare(int x0, int y0, int x1, int y1, bool fill = false)
        {
            // TODO if fixedSize is set, make the width and heght the same based on the x,y distance
            var w = x1 - x0;
            var h = y1 - y0;

            var tl = new Point(x0, y0);
            var tr = new Point(x0 + w, y0);
            var br = new Point(x0 + w, y0 + h);
            var bl = new Point(x0, y0 + h);
            var center = new Point();

            if (drawCentered)
            {
                tl.X -= w;
                tl.Y -= h;
                tr.Y -= h;
                bl.X -= w;
                center.X = tl.X + w;
                center.Y = tl.Y + h;
            }
            else
            {
                center.X = tl.X + w / 2;
                center.Y = tl.Y + h / 2;
            }

            // Top
            DrawLine(tl.X, tl.Y, tr.X, tr.Y);

            // Left
            DrawLine(tl.X, tl.Y, bl.X, bl.Y);

            // Right
            DrawLine(tr.X, tr.Y, br.X, br.Y);

            // Bottom
            DrawLine(bl.X, bl.Y, br.X, br.Y);

            if (fill)
                if (Math.Abs(w) > stroke.width && Math.Abs(h) > stroke.height)
                    FloodFill(center.X, center.Y);
        }

        /// <summary>
        /// </summary>
        /// <param name="x0"></param>
        /// <param name="y0"></param>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        /// <param name="fill"></param>
        public void DrawCircle(int x0, int y0, int x1, int y1, bool fill = false)
        {
            // Create a box to draw the circle inside of

            var w = x1 - x0;
            var h = y1 - y0;

            var tl = new Point(x0, y0);
            var tr = new Point(x0 + w, y0);
            var br = new Point(x0 + w, y0 + h);
            var bl = new Point(x0, y0 + h);

            var center = new Point();
            var radius = (int) Math.Sqrt(w * w + h * h);
            if (drawCentered)
            {
                tl.X -= w;
                tl.Y -= h;
                tr.Y -= h;
                bl.X -= w;

                center.X = tl.X + w;
                center.Y = tl.Y + h;
            }
            else
            {
                center.X = tl.X + w / 2;
                center.Y = tl.Y + h / 2;
                radius = radius / 2;
            }

            //            SetStrokePixel(center.x, center.y);
            //            
            //            SetStrokePixel(tl.x, tl.y);

            x0 = center.X;
            y0 = center.Y;

            SetStrokePixel(tr.X, tr.Y);
            SetStrokePixel(br.X, br.Y);
            SetStrokePixel(bl.X, bl.Y);

            var d = (5 - radius * 4) / 4;
            var x = 0;
            var y = radius;

            do
            {
                // 1 O'Clock
                SetStrokePixel(x0 + x, y0 - y);
                //                 3 O'Clock
                SetStrokePixel(x0 + y, y0 - x);

                // 4 O' Clock
                SetStrokePixel(x0 + y, y0 + x);

                // 5 O'Clock
                SetStrokePixel(x0 + x, y0 + y);

                // 7 O'Clock
                SetStrokePixel(x0 - x, y0 + y);

                // 8 O'Clock
                SetStrokePixel(x0 - y, y0 + x);

                // 10 O'Clock
                SetStrokePixel(x0 - y, y0 - x);

                // 11 O'Clock
                SetStrokePixel(x0 - x, y0 - y);

                if (d < 0)
                {
                    d += 2 * x + 1;
                }
                else
                {
                    d += 2 * (x - y) + 1;
                    y--;
                }

                x++;
            } while (x <= y);

            if (fill)
                if (radius > 4)
                    FloodFill(center.X, center.Y);
        }

        // public void DrawEllipse_old(int x0, int y0, int x1, int y1, bool fill = false)
        // {
        //     // Create a box to draw the circle inside of
        //
        //     var w = x1 - x0;
        //     var h = y1 - y0;
        //
        //     var tl = new Point(x0, y0);
        //     var tr = new Point(x0 + w, y0);
        //     // var br = new Point(x0 + w, y0 + h);
        //     var bl = new Point(x0, y0 + h);
        //
        //     var center = new Point();
        //     // var radius = (int) Math.Sqrt(w * w + h * h);
        //     if (drawCentered)
        //     {
        //         tl.X -= w;
        //         tl.Y -= h;
        //         tr.Y -= h;
        //         bl.X -= w;
        //
        //         center.X = tl.X + w;
        //         center.Y = tl.Y + h;
        //     }
        //     else
        //     {
        //         center.X = tl.X + w / 2;
        //         center.Y = tl.Y + h / 2;
        //         // radius = radius / 2;
        //         w = w / 2;
        //         h = h / 2;
        //     }
        //
        //     if (y1 < y0)
        //     {
        //         w *= -1;
        //         h *= -1;
        //     }
        //
        //     var xc = center.X;
        //     var yc = center.Y;
        //     var rx = w;
        //     var ry = h;
        //
        //     var x = 0;
        //     var y = ry;
        //     var p = ry * ry - rx * rx * ry + rx * rx / 4;
        //
        //     while (2 * x * ry * ry < 2 * y * rx * rx)
        //     {
        //         SetStrokePixel(xc + x, yc - y);
        //         SetStrokePixel(xc - x, yc + y);
        //         SetStrokePixel(xc + x, yc + y);
        //         SetStrokePixel(xc - x, yc - y);
        //
        //         if (p < 0)
        //         {
        //             x ++;
        //             p = p + 2 * ry * ry * x + ry * ry;
        //         }
        //         else
        //         {
        //             x ++;
        //             y --;
        //             p = p + 2 * ry * ry * x + ry * ry - 2 * rx * rx * y;
        //         }
        //     }
        //
        //     p = (int) ((x + 0.5) * (x + 0.5) * ry * ry + (y - 1) * (y - 1) * rx * rx - rx * rx * ry * ry);
        //
        //     while (y >= 0)
        //     {
        //         SetStrokePixel(xc + x, yc - y);
        //         SetStrokePixel(xc - x, yc + y);
        //         SetStrokePixel(xc + x, yc + y);
        //         SetStrokePixel(xc - x, yc - y);
        //
        //         if (p > 0)
        //         {
        //             y --;
        //             p = p - 2 * rx * rx * y + rx * rx;
        //         }
        //         else
        //         {
        //             y --;
        //             x ++;
        //             p = p + 2 * ry * ry * x - 2 * rx * rx * y - rx * rx;
        //         }
        //     }
        //
        //     if (fill)
        //         if (Math.Abs(w) > stroke.width && Math.Abs(h) > stroke.height)
        //             FloodFill(center.X, center.Y);
        // }

        public void DrawEllipse(int x0, int y0, int x1, int y1, bool fill = false)
        { /* rectangular parameter enclosing the ellipse */
            long a = Math.Abs(x1 - x0), b = Math.Abs(y1 - y0), b1 = b & 1; /* diameter */
            double dx = 4 * (1.0 - a) * b * b, dy = 4 * (b1 + 1) * a * a; /* error increment */
            double err = dx + dy + b1 * a * a, e2; /* error of 1.step */
            
            if (x0 > x1)
            {
                x0 = x1; 
                x1 += (int)(a);
            } /* if called with swapped points */

            if (y0 > y1)
                y0 = y1; /* .. exchange them */

            y0 += (int)((b + 1) / 2);
            // y1 = y0–b1; /* starting pixel */
            y1 = (int) (y0 - b1); /* starting pixel */
            a = 8 * a * a; b1 = 8 * b * b;
            do
            {
                SetStrokePixel(x1, y0); /* I. Quadrant */
                SetStrokePixel(x0, y0); /* II. Quadrant */
                SetStrokePixel(x0, y1); /* III. Quadrant */
                SetStrokePixel(x1, y1); /* IV. Quadrant */
                e2 = 2 * err;
                if (e2 <= dy) { y0++; y1--; err += dy += a; } /* y step */
                if (e2 >= dx || 2 * err > dy) { x0++; x1--; err += dx += b1; } /* x */
            } while (x0 <= x1);
            while (y0 - y1 <= b)
            { /* to early stop of flat ellipses a=1 */
                SetStrokePixel(x0 - 1, y0); /* -> finish tip of ellipse */
                SetStrokePixel(x1 + 1, y0++);
                SetStrokePixel(x0 - 1, y1);
                SetStrokePixel(x1 + 1, y1--);
            }

            if (fill)
            {
                var w = x1 - x0;
                var h = y1 - y0;
                
                var centerX = x0 + w / 2;
                var centerY = y0 + h / 2;

                if (Math.Abs(w) > stroke.width && Math.Abs(h) > stroke.height)
                    FloodFill(centerX, centerY);
            }
            
        }

        /// <summary>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="colorOffset"></param>
        public void DrawSprite(int id, int x, int y, bool flipH = false, bool flipV = false, int colorOffset = 0)
        {
            // This only works when the canvas has a reference to the gameChip
            if (gameChip == null) return;

            // TODO need to add flip to DrawSprite method
            MergePixels(x, y, spriteSize.X, spriteSize.Y, gameChip.Sprite(id), flipH, flipV, colorOffset);
        }

        public void DrawSprites(int[] ids, int x, int y, int width, bool flipH = false, bool flipV = false,
            int colorOffset = 0)
        {
            var total = ids.Length;

            // TODO added this so C# code isn't corrupted, need to check performance impact
            if (tmpIDs.Length != total) Array.Resize(ref tmpIDs, total);

            Array.Copy(ids, tmpIDs, total);

            var height = MathUtil.CeilToInt(total / width);

            var startX = x;
            var startY = y;

            var paddingW = spriteSize.X;
            var paddingH = spriteSize.Y;

            // TODO need to offset the bounds based on the scroll position before testing against it

            for (var i = 0; i < total; i++)
            {
                // Set the sprite id
                var id = tmpIDs[i];

                // TODO should also test that the sprite is not greater than the total sprites (from a cached value)
                // Test to see if the sprite is within range
                if (id > -1)
                {
                    x = MathUtil.FloorToInt(i % width) * paddingW + startX;
                    y = MathUtil.FloorToInt(i / width) * paddingH + startY;
                    //
                    //                    var render = true;

                    // Check to see if we need to test the bounds

                    DrawSprite(id, x, y, flipH, flipV, colorOffset);
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="text"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="font"></param>
        /// <param name="colorOffset"></param>
        /// <param name="spacing"></param>
        public void DrawText(string text, int x, int y, string font = "default", int colorOffset = 0, int spacing = 0)
        {
            // This only works when the canvas has a reference to the gameChip
            if (gameChip == null) return;

            //            var ids = gameChip.ConvertTextToSprites(text, font);
            var total = text.Length;
            var nextX = x;
            var nextY = y;


            for (var i = 0; i < total; i++)
            {
                MergePixels(nextX, nextY, spriteSize.X, spriteSize.Y,
                    gameChip.CharacterToPixelData(text[i], font), false, false, colorOffset);


                //                DrawSprite(ids[i], nextX, nextY, false, false, colorOffset);
                nextX += spriteSize.X + spacing;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void FloodFill(int x, int y)
        {
            if (x < 0 || y < 0 || x > _width || y > _height) return;

            // Get the color at the point where we are trying to fill and use that to match all the color inside the shape
            var targetColor = GetPixel(x, y);

            var pixels = new Stack<Point>();

            pixels.Push(new Point(x, y));

            while (pixels.Count != 0)
            {
                var temp = pixels.Pop();
                var y1 = temp.Y;
                while (y1 >= 0 && GetPixel(temp.X, y1) == targetColor) y1--;

                y1++;
                var spanLeft = false;
                var spanRight = false;
                while (y1 < _height && GetPixel(temp.X, y1) == targetColor)
                {
                    SetPixel(temp.X, y1, pattern.GetPixel(temp.X, y1));

                    if (!spanLeft && temp.X > 0 && GetPixel(temp.X - 1, y1) == targetColor)
                    {
                        if (GetPixel(temp.X - 1, y1) != pattern.GetPixel(temp.X, y1))
                            pixels.Push(new Point(temp.X - 1, y1));

                        spanLeft = true;
                    }
                    else if (spanLeft && temp.X - 1 == 0 && GetPixel(temp.X - 1, y1) != targetColor)
                    {
                        spanLeft = false;
                    }

                    if (!spanRight && temp.X < _width - 1 && GetPixel(temp.X + 1, y1) == targetColor)
                    {
                        if (GetPixel(temp.X + 1, y1) != pattern.GetPixel(temp.X, y1))
                            pixels.Push(new Point(temp.X + 1, y1));

                        spanRight = true;
                    }
                    else if (spanRight && temp.X < _width - 1 && GetPixel(temp.X + 1, y1) != targetColor)
                    {
                        spanRight = false;
                    }

                    y1++;
                }
            }
        }

        /// <summary>
        ///     Allows you to merge the pixel data of another canvas into this one without compleatly overwritting it.
        /// </summary>
        /// <param name="canvas"></param>
        public void MergeCanvas(Canvas canvas, int colorOffset = 0, bool ignoreTransparent = false)
        {
            MergePixels(0, 0, canvas.width, canvas.height, canvas.pixels, false, false, colorOffset);
        }

        public int ReadPixelAt(int x, int y)
        {
            // Calculate the index
            var index = x + y * _width;

            if (index >= pixels.Length) return -1;

            return pixels[index];
        }

        public int[] SamplePixels(int x, int y, int width, int height)
        {
            // TODO this should be optimized if we are going to us it moving forward
            var totalPixels = width * height;
            var pixelData = new int[totalPixels];

            CopyPixels(ref pixelData, x, y, width, height);

            return pixelData;
        }

        public override void SetPixels(int x, int y, int blockWidth, int blockHeight, int[] pixels)
        {

            if (wrap == false)
            {

                BlockSave(pixels, blockWidth, blockHeight, this.pixels, x, y, width, height);
                return;
            }

            base.SetPixels(x, y, blockWidth, blockHeight, pixels);
        }

        void BlockSave(int[] src, int srcW, int srcH, int[] dest, int destX, int destY, int destW, int destH)
        {
        
            var srcX = 0;
            var srcY = 0;
            var srcLength = srcW;
        
            // Adjust X
            if (destX < 0)
            {
                srcX = -destX;
        
                srcW -= srcX;
                 
                // destW += destX; 
                destX = 0;
            }
        
            if (destX + srcW > destW)
                srcW -= ((destX + srcW) - destW);
        
            if (srcW <= 0) return;
        
            // Adjust Y
            if (destY < 0)
            {
                srcY = -destY;
        
                srcH -= srcY;
        
                // destW += destX; 
                destY = 0;
            }
        
            if (destY + srcH > destH)
                srcH -= ((destY + srcH) - destH);
        
            if (srcH <= 0) return;
        
            var row = 0;
            var startCol = 0;
            // var endCol = 0;
            var destCol = 0;
        
            for (row = 0; row < srcH; row++)
            {
                startCol = srcX + (row + srcY) * srcLength;
                destCol = destX + (row + destY) * destW;
        
                Array.Copy(src, startCol, dest, destCol, srcW);
            }
        
        }

        private int tmpX = 0;
        private int tmpY = 0;
        private int tmpW = 0;
        private int tmpH = 0;

        /// <summary>
        ///     Fast blit to the display through the draw request API
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="drawMode"></param>
        /// <param name="scale"></param>
        /// <param name="maskColor"></param>
        /// <param name="maskColorID"></param>
        /// <param name="viewport"></param>
        public void DrawPixels(int x = 0, int y = 0, DrawMode drawMode = DrawMode.TilemapCache, float scale = 1f,
            int maskColor = -1, int maskColorID = -1, int colorOffset = 0, Rectangle? viewport = null, int[] isolateColors = null)
        {
            // This only works when the canvas has a reference to the gameChip
            if (gameChip == null) return;

            if (viewport.HasValue)
            {
                tmpX = viewport.Value.X;
                tmpY = viewport.Value.Y;
                tmpW = viewport.Value.Width;
                tmpH = viewport.Value.Height;
            }
            else
            {
                tmpX = 0;
                tmpY = 0;
                tmpW = width;
                tmpH = height;
            }

            var srcPixels = GetPixels(tmpX, tmpY, tmpW, tmpH);

            // Loop through and replace mask colors
            for (int i = 0; i < srcPixels.Length; i++)
            {

                // Check to see if colors should be isolated
                if (isolateColors != null && Array.IndexOf(isolateColors, srcPixels[i]) == -1)
                {
                    srcPixels[i] = -1;
                }

                // Replace any mask color with the supplied mask color
                if (srcPixels[i] == maskColor)
                {
                    srcPixels[i] = maskColorID;
                }
            }

            // Covert the width and height into ints based on scale
            var newWidth = (int)(tmpW * scale);
            var newHeight = (int)(tmpH * scale);

            var destPixels = scale > 1 ? ResizePixels(srcPixels, tmpW, tmpH, newWidth, newHeight) : srcPixels;

            gameChip.DrawPixels(destPixels, x, y, newWidth, newHeight, false, false, drawMode, colorOffset);

        }

        // Reference https://tech-algorithm.com/articles/nearest-neighbor-image-scaling/
        public int[] ResizePixels(int[] pixels, int w1, int h1, int w2, int h2)
        {
            int[] temp = new int[w2 * h2];
            // EDIT: added +1 to account for an early rounding problem
            int x_ratio = (int)((w1 << 16) / w2) + 1;
            int y_ratio = (int)((h1 << 16) / h2) + 1;
            //int x_ratio = (int)((w1<<16)/w2) ;
            //int y_ratio = (int)((h1<<16)/h2) ;
            //int y_ratio = (int)((h1<<16)/h2) ;
            int x2, y2;
            for (int i = 0; i < h2; i++)
            {
                for (int j = 0; j < w2; j++)
                {
                    x2 = ((j * x_ratio) >> 16);
                    y2 = ((i * y_ratio) >> 16);
                    temp[(i * w2) + j] = pixels[(y2 * w1) + x2];
                }
            }
            return temp;
        }
    }

    // Performance improvement
    // for (int i=0;i<h2;i++)
    // {
    // int* t = temp + i * w2;
    // y2 = ((i* y_ratio)>>16);
    // int* p = pixels + y2 * w1;
    // int rat = 0;
    //     for (int j=0;j<w2;j++)
    // {
    //     x2 = (rat>>16);
    //     * t++ = p[x2];
    //     rat += x_ratio;
    // }
    // }
}