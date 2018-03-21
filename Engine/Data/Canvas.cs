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
using System.Collections.Generic;
using PixelVisionSDK.Chips;
using PixelVisionSDK.Utils;

namespace PixelVisionSDK
{
    
    public class Canvas : TextureData
    {
        private Pattern stroke;
        private Pattern pattern;
        private IGameChip gameChip;
        private Vector spriteSize;
        private bool drawCentered;
        private Vector linePattern = new Vector(1);
        public bool wrap = false;
        
        public void LinePattern(int a, int b)
        {
            linePattern.x = a;
            linePattern.y = b;
        }
        
        public bool DrawCentered(bool? newValue = null)
        {
            if (newValue.HasValue)
            {
                drawCentered = newValue.Value;
            }

            return drawCentered;
        }
        
        public void SetStroke(int[] pixels, int width, int height)
        {
//            var total = width * height;

            if (stroke.width != width || pattern.height != height)
            {
                stroke.Resize(width, height);
            }
            
            stroke.SetPixels(pixels);
        }
        
        public void SetPattern(int[] pixels, int width, int height)
        {
//            var total = width * height;

            if (pattern.width != width || pattern.height != height)
            {
                pattern.Resize(width, height);
            }
            
            pattern.SetPixels(pixels);
        }

        public Canvas(int width, int height, IGameChip gameChip = null) : base(width, height)
        {
            this.gameChip = gameChip;
            pattern = new Pattern(1, 1);
            pattern.SetPixel(0,0, 0);
            
            stroke = new Pattern(1, 1);
            stroke.SetPixel(0,0, 0);

            spriteSize = gameChip.SpriteSize();
            
        }
    
        /// <summary>
        ///     Fast blit to the display through the draw request API
        /// </summary>
        /// <param name="drawMode"></param>
        public void DrawPixels(int x = 0, int y = 0, DrawMode drawMode = DrawMode.TilemapCache)
        {
            // This only works when the canvas has a reference to the gameChip
            if (gameChip == null)
                return;
            
            gameChip.DrawPixels(GetPixels(), x, y, width, height, drawMode);
        }

        private bool canDraw = false;
        
        /// <summary>
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void SetStrokePixel(int x, int y)
        {
            canDraw = wrap || x >= 0 && x <= width - stroke.width && y >= 0 && y <= height - stroke.height;
//            
            if(canDraw)
                SetPixels(x, y, stroke.width, stroke.height, stroke.GetPixels());
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
            
            int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int err = (dx > dy ? dx : -dy) / 2, e2;
            for(;;) {
                if (counter % linePattern.x == linePattern.y)
                {
                    SetStrokePixel(x0, y0);
                }

                counter++;
                if (x0 == x1 && y0 == y1) break;
                e2 = err;
                if (e2 > -dx) { err -= dy; x0 += sx; }
                if (e2 < dy) { err += dx; y0 += sy; }
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

            var tl = new Vector(x0, y0);
            var tr = new Vector(x0 + w, y0);
            var br = new Vector(x0 + w, y0 + h);
            var bl = new Vector(x0, y0 + h);
            var center = new Vector();
            
            if (drawCentered)
            {
                tl.x -= w;
                tl.y -= h;
                tr.y -= h;
                bl.x -= w;
                center.x = tl.x + w;
                center.y = tl.y + h;
                
            }
            else
            {
                center.x = tl.x + w / 2;
                center.y = tl.y + h / 2;
            }
            
            // Top
            DrawLine(tl.x, tl.y, tr.x , tr.y);

            // Left
            DrawLine(tl.x, tl.y, bl.x, bl.y);

            // Right
            DrawLine(tr.x, tr.y, br.x, br.y);

            // Bottom
            DrawLine(bl.x, bl.y, br.x, br.y);
 
            if (fill)
            {

                // TODO this needs to take into account the thickness of the border
                if (Math.Abs(w) > stroke.width && Math.Abs(h) > stroke.height)
                {
                    // Fill the square
                    FloodFill(center.x, center.y);

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
        public void DrawCircle(int x0, int y0, int x1, int y1, bool fill = false)
        {
            
            // Create a box to draw the circle inside of
            
            var w = x1 - x0;
            var h = y1 - y0;

            var tl = new Vector(x0, y0);
            var tr = new Vector(x0 + w, y0);
            var br = new Vector(x0 + w, y0 + h);
            var bl = new Vector(x0, y0 + h);
            
            var center = new Vector();
            var radius = (int)Math.Sqrt((w * w) + (h * h));
            if (drawCentered)
            {
                tl.x -= w;
                tl.y -= h;
                tr.y -= h;
                bl.x -= w;

                center.x = tl.x + w;
                center.y = tl.y + h;
                
            }
            else
            {
                center.x = tl.x + w / 2;
                center.y = tl.y + h / 2;
                radius = radius / 2;
            }
            
//            SetStrokePixel(center.x, center.y);
//            
//            SetStrokePixel(tl.x, tl.y);

            x0 = center.x;
            y0 = center.y;
            
            SetStrokePixel(tr.x, tr.y);
            SetStrokePixel(br.x, br.y);
            SetStrokePixel(bl.x, bl.y);

            int d = (5 - radius * 4) / 4;
            int x = 0;
            int y = radius;

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
            {
                if(radius > 4)
                    FloodFill(center.x, center.y);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="colorOffset"></param>
        public void DrawSprite(int id, int x, int y, int colorOffset = 0)
        {
            // This only works when the canvas has a reference to the gameChip
            if (gameChip == null)
                return;
            
            var pixelData = gameChip.Sprite(id);

            if (colorOffset > 0)
            {
                var total = pixelData.Length;

                for (int i = 0; i < total; i++)
                {
                    pixelData[i] = pixelData[i] + colorOffset;
                }
            }
            
            // Canvas is reversed, so flip the pixel data
            SpriteChipUtil.FlipSpriteData(ref pixelData, spriteSize.x, spriteSize.y);
            
            SetPixels(x, y, spriteSize.x, spriteSize.y, pixelData);
        }
        
        /// <summary>
        /// 
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
            if (gameChip == null)
                return;
            
            var ids = gameChip.ConvertTextToSprites(text, font);
            var total = ids.Length;
            var nextX = x;
            var nextY = y;
            
            for (var i = 0; i < total; i++)
            {

                DrawSprite(ids[i], nextX, nextY, colorOffset);
                nextX += spriteSize.x + spacing;

            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void FloodFill(int x, int y)
        {

            if (x < 0 || y < 0 || x > width || y > height)
                return;
            
            // Get the color at the point where we are trying to fill and use that to match all the color inside the shape
            var targetColor = GetPixel(x, y);

            Stack<Vector> pixels = new Stack<Vector>();
             
            pixels.Push(new Vector(x, y));
            
            while (pixels.Count != 0)
            {
                Vector temp = pixels.Pop();
                int y1 = temp.y;
                while (y1 >= 0 && GetPixel(temp.x, y1) == targetColor)
                {
                    y1--;
                }
                y1++;
                bool spanLeft = false;
                bool spanRight = false;
                while (y1 < height && GetPixel(temp.x, y1) == targetColor)
                {
                   
                    SetPixel(temp.x, y1, pattern.GetPixel(temp.x, y1));
 
                    if (!spanLeft && temp.x > 0 && GetPixel(temp.x - 1, y1) == targetColor)
                    {
                        pixels.Push(new Vector(temp.x - 1, y1));
                        spanLeft = true;
                    }
                    else if(spanLeft && temp.x - 1 == 0 && GetPixel(temp.x - 1, y1) != targetColor)
                    {
                        spanLeft = false;
                    }
                    if (!spanRight && temp.x < width - 1 && GetPixel(temp.x + 1, y1) == targetColor)
                    {
                        pixels.Push(new Vector(temp.x + 1, y1));
                        spanRight = true;
                    }
                    else if (spanRight && temp.x < width - 1 && GetPixel(temp.x + 1, y1) != targetColor)
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
        public void Merge(Canvas canvas, int colorOffset = 0, bool ignoreTransparent = false)
        {
            MergePixels(0, 0, canvas.width, canvas.height, canvas.GetPixels(), colorOffset, ignoreTransparent);
        }

    }
}