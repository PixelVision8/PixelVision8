﻿//   
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
using Microsoft.Xna.Framework;
using PixelVision8.Engine.Chips;
using PixelVision8.Engine.Utils;

namespace PixelVision8.Engine
{
    
    public class Canvas : AbstractData, IDraw
    {
        
        private readonly GameChipLite gameChip;
        private PixelData stroke = new PixelData();
        private PixelData pattern = new PixelData();
        private readonly Point spriteSize;
        private readonly CanvasDrawRequest[] requestPool = new CanvasDrawRequest[1024];
        private PixelData defaultLayer = new PixelData();
        private PixelData tmpLayer = new PixelData();

        private int currentRequest = -1;
        private bool canDraw;

        private Point linePattern = new Point(1, 0);
        public bool wrap = false;
        public Dictionary<string, Action<CanvasDrawRequest>> Actions;
        private PixelData currentTexture;

        // These are temporary values we use to help speed up calculations
        private int _x0;
        private int _y0;
        private int _x1;
        private int _y1;
        private int _w;
        private int _h;
        private int _total;
        private CanvasDrawRequest _request;
        private int tmpX;
        private int tmpY;
        private int tmpW;
        private int tmpH;
        
        public int width => defaultLayer.Width;
        public int height => defaultLayer.Height;
        public int[] Pixels => defaultLayer.Pixels;

        public Canvas(int width, int height, GameChipLite gameChip = null)
        {
            
            Resize(width, height);
            
            // Make the canvas the default drawing surface
            currentTexture = defaultLayer;
            
            this.gameChip = gameChip;
            
            spriteSize = gameChip.SpriteSize();

            // Create a pool of draw requests
            for (int i = 0; i < requestPool.Length; i++)
            {
                requestPool[i] = new CanvasDrawRequest();
            }

            // TODO could we register external drawing calls to this?
            Actions = new Dictionary<string, Action<CanvasDrawRequest>>()
            {
                {"SetStroke", request => SetStrokeAction(request)},
                {"SetPattern", request => SetPatternAction(request)},
                {"DrawLine", request => DrawLineAction(request)},
                {"DrawEllipse", request => DrawEllipseAction(request)},
                {"DrawPixelData", request => DrawPixelDataAction(request)},
                {"FloodFill", request => FloodFillAction(request)},
                {"ChangeTargetCanvas", request => ChangeTargetCanvasAction(request)},
                {"SaveTmpLayer", request => SaveTmpLayerAction(request)},
            };
            
        }

        public  void Resize(int width, int height) => PixelDataUtil.Resize(defaultLayer, width, height);
  
        private void ChangeTargetCanvas(int width, int height)
        {
            var getRequest = NextRequest();

            if (getRequest == null)
                return;

            var newRequest = getRequest;

            newRequest.Action = "ChangeTargetCanvas";
            // newRequest.PixelData = textureData;
            newRequest.Bounds.Width = width;
            newRequest.Bounds.Height = height;
            
            // Save the changes to the request
            requestPool[currentRequest] = newRequest;
        }

        private void ChangeTargetCanvasAction(CanvasDrawRequest drawRequest)
        {
            currentTexture = tmpLayer;
            
            if(drawRequest.Bounds.Width !=  currentTexture.Width || drawRequest.Bounds.Height != currentTexture.Height)
                PixelDataUtil.Resize(currentTexture, drawRequest.Bounds.Width, drawRequest.Bounds.Height);
            else
                PixelDataUtil.Clear(currentTexture);
        }

        public void SetStroke(int color, int size = 1)
        {
            var getRequest = NextRequest();

            if (getRequest == null)
                return;

            var newRequest = getRequest;

            newRequest.Action = "SetStroke";

            if (newRequest.PixelData.Width != size || newRequest.PixelData.Height != size)
            {
                PixelDataUtil.Resize(newRequest.PixelData, size, size);
            }

            var newPixels = new int[size * size];
            for (int i = 0; i < newPixels.Length; i++)
            {
                newPixels[i] = color;
            }

            PixelDataUtil.SetPixels(newPixels, newRequest.PixelData);
            
            // Save the changes to the request
            requestPool[currentRequest] = newRequest;
        }
        
        private void SetStrokeAction(CanvasDrawRequest request)
        {
            if (stroke.Width != request.PixelData.Width || pattern.Height != request.PixelData.Height)
                PixelDataUtil.Resize(stroke, request.PixelData.Width, request.PixelData.Height);

            PixelDataUtil.SetPixels(request.PixelData.Pixels, stroke);
       }
 
        public void SetPattern(int[] newPixels, int newWidth, int newHeight)
        {
            var getRequest = NextRequest();

            if (getRequest == null)
                return;

            var newRequest = getRequest;

            newRequest.Action = "SetPattern";

            if (newRequest.PixelData.Width != newWidth || newRequest.PixelData.Height != newHeight)
            {
                PixelDataUtil.Resize(newRequest.PixelData, newWidth, newHeight);
            }

            PixelDataUtil.SetPixels(newPixels, newRequest.PixelData);

            // Save the changes to the request
            requestPool[currentRequest] = newRequest;
        }

        public void SetPatternAction(CanvasDrawRequest request)
        {
            if (pattern.Width != request.PixelData.Width || pattern.Height != request.PixelData.Height)
                PixelDataUtil.Resize(pattern, request.PixelData.Width, request.PixelData.Height);

            PixelDataUtil.SetPixels(request.PixelData.Pixels, pattern);

        }

        private void SetStrokePixel(int x, int y)
        {
            canDraw = wrap || x >= 0 && x <= width - stroke.Width && y >= 0 && y <= height - stroke.Height;
            
            // TODO this should never be null           
            if (canDraw) 
                PixelDataUtil.SetPixels(currentTexture, x, y, stroke.Width, stroke.Height, stroke.Pixels);
            
        }

        private CanvasDrawRequest NextRequest()
        {
            // Test to see if there is another available request
            if (currentRequest + 1 >= requestPool.Length)
                return null;

            // Increase the request
            currentRequest++;

            // Invalidate the canvas so the request will be called during the draw cycle
            Invalidate();

            // Return the new request
            return requestPool[currentRequest];
        }

        public void DrawLine(int x0, int y0, int x1, int y1)
        {
            var getRequest = NextRequest();

            if (getRequest == null)
                return;

            var newRequest = getRequest;

            newRequest.Action = "DrawLine";

            newRequest.Bounds.X = x0;
            newRequest.Bounds.Y = y0;
            
            // Well be using the wid as the second point
            newRequest.Bounds.Width = x1;
            newRequest.Bounds.Height = y1;

            // Save the changes to the request
            requestPool[currentRequest] = newRequest;
        }

        int _counter = 0;
        int _sx;
        int _sy;
        
        private void DrawLineAction(CanvasDrawRequest drawRequest)
        {
            _x0 = drawRequest.Bounds.X;
            _y0 = drawRequest.Bounds.Y;
            _x1 = drawRequest.Bounds.Width;
            _y1 = drawRequest.Bounds.Height;

            _counter = 0;

            _dx = _x1 - _x0;
            // _sx;
            if (_dx < 0)
            {
                _dx = -_dx;
                _sx = -1;
            }
            else
            {
                _sx = 1;
            }

            _dy = _y1 - _y0;
            // _sy;
            if (_dy < 0)
            {
                _dy = -_dy;
                _sy = -1;
            }
            else
            {
                _sy = 1;
            }

            _err = (_dx > _dy ? _dx : -_dy) / 2;
            
            for (;;)
            {
                if (_counter % linePattern.X == linePattern.Y) SetStrokePixel(_x0, _y0);

                _counter++;
                if (_x0 == _x1 && _y0 == _y1) break;

                _e2 = _err;
                if (_e2 > -_dx)
                {
                    _err -= _dy;
                    _x0 += _sx;
                }

                if (_e2 < _dy)
                {
                    _err += _dx;
                    _y0 += _sy;
                }
            }
        }

        private Rectangle _tmpRect = Rectangle.Empty;

        public void DrawSquare(int x0, int y0, int x1, int y1, bool fill = false)
        {
            // Make sure we are using positive numbers for the rectangle
            _x0 = Math.Min(x0, x1);
            _y0 = Math.Min(y0, y1);
            _x1 = Math.Max(x0, x1);
            _y1 = Math.Max(y0, y1);
            
            _w = _x1 - _x0;
            _h = _y1 - _y0;
            
            DrawRectangle(_x0, _y0, _w, _h, fill);
        }
        

        public void DrawRectangle(int x, int y, int width, int height, bool fill = false)
        {

            // Store values in a temp rect
            _tmpRect.X = x;
            _tmpRect.Y = y;
            
            // Adjust dimensions to account for the stroke
            _tmpRect.Width = width - (stroke.Width * 2);
            _tmpRect.Height = height - (stroke.Height * 2);
            
            if (fill)
            {
                ChangeTargetCanvas( width, height);
                
                // Reset the tmp Rect to 0,0 when drawing into the new layer
                _tmpRect.X = 0;
                _tmpRect.Y = 0;
                
            }

            // Top
            DrawLine(_tmpRect.Left, _tmpRect.Top, _tmpRect.Right, _tmpRect.Top);

            // Left
            DrawLine(_tmpRect.Left, _tmpRect.Top, _tmpRect.Left, _tmpRect.Bottom);

            // Right
            DrawLine(_tmpRect.Right, _tmpRect.Top, _tmpRect.Right, _tmpRect.Bottom);

            // Bottom
            DrawLine(_tmpRect.Left, _tmpRect.Bottom, _tmpRect.Right, _tmpRect.Bottom);

            // Check again to see if we need to fill the rectangle
            if (fill)
            {
                // Make sure there are enough pixels to fill
                if (width > stroke.Width && height > stroke.Height)
                {
                    // Trigger a flood fill
                    FloodFill(_tmpRect.Center.X, _tmpRect.Center.Y);

                    // Copy pixels data to the main drawing surface
                    SaveTmpLayer(x, y, width, height);
                    
                    // Change back to default drawing surface
                    // ChangeTargetCanvas(defaultLayer);
                }
            }
        }


        private void SaveTmpLayer(int x, int y, int blockWidth, int blockHeight)
        {
            var getRequest = NextRequest();

            if (getRequest == null)
                return;

            var newRequest = getRequest;
            
            newRequest.Action = "SaveTmpLayer";
            newRequest.x = x;
            newRequest.y = y;
            newRequest.Bounds.X = 0;
            newRequest.Bounds.Y = 0;
            newRequest.Bounds.Width = blockWidth;
            newRequest.Bounds.Height = blockHeight;
            
            // Save the changes to the request
            requestPool[currentRequest] = newRequest;
        }

        // private Rectangle _tmpRect = Rectangle.Empty;
        
        private void SaveTmpLayerAction(CanvasDrawRequest request)
        {
            PixelDataUtil.MergePixels(tmpLayer, request.Bounds.X, request.Bounds.Y, request.Bounds.Width, request.Bounds.Height, defaultLayer, request.x, request.y);
            
            currentTexture = defaultLayer;
        }
        
        public void DrawEllipse(int x, int y, int width, int height, bool fill = false)
        {
            
            if(fill)
                ChangeTargetCanvas( width, height);
            
            var getRequest = NextRequest();

            if (getRequest == null)
                return;

            var newRequest = getRequest;

            newRequest.Bounds.X = x;
            newRequest.Bounds.Y = y;
            newRequest.Bounds.Width = width - (stroke.Width * 2);
            newRequest.Bounds.Height = height - (stroke.Height * 2);
            
            newRequest.Fill = fill;

            newRequest.Action = "DrawEllipse";
            
            // Save the changes to the request
            requestPool[currentRequest] = newRequest;
            
            if(fill)
                SaveTmpLayer(x, y, width, height);
            
            // Change back to default drawing surface
            // ChangeTargetCanvas(defaultLayer);
        }

        private long _a;
        private long _b;
        private long _b1;
        private double _dx;
        private double _dy;
        private double _err;
        private double _e2;
        
        public void DrawEllipseAction(CanvasDrawRequest request)
        {
            
            // Save the x and y values to calculate below
            _x0 = request.Bounds.Left;
            _y0 = request.Bounds.Top;
            _x1 = request.Bounds.Right;
            _y1 = request.Bounds.Bottom;
            
            if (request.Fill)
            {
                _x0 = 0;
                _y0 = 0;
                _x1 = request.Bounds.Width;
                _y1 = request.Bounds.Height;
            }

            // Adjust for border
            _y0 += stroke.Height;
            _x1 -= stroke.Width;
            _y1 -= stroke.Height;
            
            /* rectangular parameter enclosing the ellipse */
            _a = Math.Abs(_x1 - _x0);
            _b = Math.Abs(_y1 - _y0);
            _b1 = _b & 1; /* diameter */
            _dx = 4 * (1.0 - _a) * _b * _b;
            _dy = 4 * (_b1 + 1) * _a * _a; /* error increment */
            _err = _dx + _dy + _b1 * _a * _a;

            if (_x0 > _x1)
            {
                _x0 = _x1;
                _x1 += (int) (_a);
            } /* if called with swapped points */

            if (_y0 > _y1)
                _y0 = _y1; /* .. exchange them */

            _y0 += (int) ((_b + 1) / 2);
            _y1 = (int) (_y0 - _b1); /* starting pixel */
            _a = 8 * _a * _a;
            _b1 = 8 * _b * _b;
            do
            {
                SetStrokePixel(_x1, _y0); /* I. Quadrant */
                SetStrokePixel(_x0, _y0); /* II. Quadrant */
                SetStrokePixel(_x0, _y1); /* III. Quadrant */
                SetStrokePixel(_x1, _y1); /* IV. Quadrant */
                _e2 = 2 * _err;
                if (_e2 <= _dy)
                {
                    _y0++;
                    _y1--;
                    _err += _dy += _a;
                } /* y step */

                if (_e2 >= _dx || 2 * _err > _dy)
                {
                    _x0++;
                    _x1--;
                    _err += _dx += _b1;
                } /* x */
            } while (_x0 <= _x1);

            while (_y0 - _y1 <= _b)
            {
                /* to early stop of flat ellipses a=1 */
                SetStrokePixel(_x0 - 1, _y0); /* -> finish tip of ellipse */
                SetStrokePixel(_x1 + 1, _y0++);
                SetStrokePixel(_x0 - 1, _y1);
                SetStrokePixel(_x1 + 1, _y1--);
            }

            if (request.Fill)
            {

                // Save the center X & Y position before we save it back
                _x1 = request.Bounds.Center.X;
                _y1 = request.Bounds.Center.Y;
                
                if (request.Bounds.Width > stroke.Width && request.Bounds.Height > stroke.Height)
                {
                    request.Bounds.X = _x1;
                    request.Bounds.Y = _y1;
                    
                    FloodFillAction(request);

                }
                
            }
        }
        
        // TODO need to draw pixel data to the display and route the sprite and text through it

        public void DrawSprite(int id, int x, int y, bool flipH = false, bool flipV = false, int colorOffset = 0)
        {
            var getRequest = NextRequest();

            if (getRequest == null)
                return;

            var newRequest = getRequest;

            newRequest.Action = "DrawPixelData";

            // We need at least 1 pixel to save the sprite ID
            if (newRequest.PixelData.Width != spriteSize.X || newRequest.PixelData.Height != spriteSize.Y)
            {
                PixelDataUtil.Resize(newRequest.PixelData, spriteSize.X,spriteSize.X);
            }

            // Copy over the pixel
            PixelDataUtil.SetPixels( gameChip.Sprite(id), newRequest.PixelData);

            newRequest.x = x;
            newRequest.y = y;
            newRequest.Bounds.X = 0;
            newRequest.Bounds.Y = 0;
            newRequest.Bounds.Width = spriteSize.X;
            newRequest.Bounds.Height = spriteSize.Y;
            newRequest.FlipH = flipH;
            newRequest.FlipV = flipV;
            newRequest.ColorOffset = colorOffset;
            
            // Save the changes to the request
            requestPool[currentRequest] = newRequest;
        }

        public void DrawPixelDataAction(CanvasDrawRequest request)
        {
            
            // if (request.Bounds.Width * request.Bounds.Height != request.PixelData.TotalPixels)
            //     return;
            // Debug.WriteLine("Bounds " + request.Bounds + " "+ request.PixelData.TotalPixels);
            PixelDataUtil.MergePixels(request.PixelData, request.Bounds.X, request.Bounds.Y, request.Bounds.Width, request.Bounds.Height, defaultLayer, request.x, request.y, request.FlipH, request.FlipV, request.ColorOffset);
        }

        public void DrawSprites(int[] ids, int x, int y, int width, bool flipH = false, bool flipV = false,
            int colorOffset = 0)
        {
            _total = ids.Length;

            var startX = x;
            var startY = y;

            var paddingW = spriteSize.X;
            var paddingH = spriteSize.Y;

            // TODO need to offset the bounds based on the scroll position before testing against it

            for (var i = 0; i < _total; i++)
            {
                // Set the sprite id
                var id = ids[i];

                // TODO should also test that the sprite is not greater than the total sprites (from a cached value)
                // Test to see if the sprite is within range
                if (id > -1)
                {
                    x = MathUtil.FloorToInt(i % width) * paddingW + startX;
                    y = MathUtil.FloorToInt(i / width) * paddingH + startY;
                    
                    DrawSprite(id, x, y, flipH, flipV, colorOffset);
                }
            }
        }

        public void DrawText(string text, int x, int y, string font = "default", int colorOffset = 0, int spacing = 0)
        {
            
            // This only works when the canvas has a reference to the gameChip
            if (gameChip == null) return;

            var total = text.Length;
            var nextX = x;

            for (var i = 0; i < total; i++)
            {
                var getRequest = NextRequest();

                if (getRequest == null)
                    return;

                var newRequest = getRequest;

                newRequest.Action = "DrawPixelData";

                // We need at least 1 pixel to save the sprite ID
                if (newRequest.PixelData.Width != spriteSize.X || newRequest.PixelData.Height != spriteSize.Y)
                {
                    PixelDataUtil.Resize(newRequest.PixelData, spriteSize.X,spriteSize.X);
                }

                // Copy over the pixel
                PixelDataUtil.SetPixels( gameChip.FontChar(text[i], font), newRequest.PixelData);

                newRequest.x = nextX;
                newRequest.y = y;
                newRequest.Bounds.X = 0;
                newRequest.Bounds.Y = 0;
                newRequest.Bounds.Width = spriteSize.X;
                newRequest.Bounds.Height = spriteSize.Y;
                newRequest.FlipH = false;
                newRequest.FlipV = false;
                newRequest.ColorOffset = colorOffset;
            
                // Save the changes to the request
                requestPool[currentRequest] = newRequest;
                
                nextX += spriteSize.X + spacing;
            }
            
        }
        
        public void FloodFill(int x, int y)
        {
            var getRequest = NextRequest();

            if (getRequest == null)
                return;

            var newRequest = getRequest;

            newRequest.Action = "FloodFill";
            newRequest.Bounds.X = x;
            newRequest.Bounds.Y = y;
            
            // Save the changes to the request
            requestPool[currentRequest] = newRequest;
        }

        /// <summary>
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void FloodFillAction(CanvasDrawRequest request)
        {
            if (request.Bounds.X < 0 || request.Bounds.Y < 0 || request.Bounds.X > width || request.Bounds.Y > height) return;

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
                while (y1 < height && GetPixel(currentTexture, temp.X, y1) == targetColor)
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

                    if (!spanRight && temp.X < width - 1 && GetPixel(currentTexture, temp.X + 1, y1) == targetColor)
                    {
                        if (GetPixel(currentTexture, temp.X + 1, y1) != GetPixel(pattern, temp.X, y1))
                            pixels.Push(new Point(temp.X + 1, y1));

                        spanRight = true;
                    }
                    else if (spanRight && temp.X < width - 1 && GetPixel(currentTexture, temp.X + 1, y1) != targetColor)
                    {
                        spanRight = false;
                    }

                    y1++;
                }
            }
        }
        private static int _size;

        public static int GetPixel(PixelData pixelData, int x, int y)
        {
            return -1;
            
            _size = pixelData.Height;
            y = (y % _size + _size) % _size;
            _size = pixelData.Width;
            x = (x % _size + _size) % _size;
                // size is still == _width from the previous operation - let's reuse the local
            if (x + pixelData.Width * y > pixelData.TotalPixels)
                    return -1;
                
            return pixelData[x + pixelData.Width * y];
        }
        //
        public static void SetPixel(PixelData pixelData, int x, int y, int color)
        {
            return;
            
            // Note: + size and the second modulo operation are required to get wrapped values between 0 and +size
            _size = pixelData.Height;
            y = (y % _size + _size) % _size;
            _size = pixelData.Width;
            x = (x % _size + _size) % _size;

            if (x + pixelData.Width * y > pixelData.TotalPixels)
                return;
                    
            pixelData[x + pixelData.Height * y] = color;
        }

        /// <summary>
        ///     Allows you to merge the pixel data of another canvas into this one without compleatly overwritting it.
        /// </summary>
        /// <param name="canvas"></param>
        public void MergeCanvas(Canvas canvas, int colorOffset = 0, bool ignoreTransparent = false) => MergePixels(0, 0, canvas.width, canvas.height, canvas.GetPixels(), false, false, colorOffset, ignoreTransparent);

        private PixelData _tmpPixelData = new PixelData();
        
        public virtual void MergePixels(int x, int y, int blockWidth, int blockHeight, int[] pixels,
            bool flipH = false, bool flipV = false, int colorOffset = 0, bool ignoreTransparent = true)
        {

            // Flatten the canvas
            Draw();
         
            if(_tmpPixelData.Width != blockWidth || _tmpPixelData.Height != blockHeight)
                _tmpPixelData.Resize(blockWidth, blockHeight);

            _tmpPixelData.Pixels = pixels;

            // _tmpRect.Width = blockWidth;
            // _tmpRect.Height = blockHeight;
            
            PixelDataUtil.MergePixels(_tmpPixelData, 0, 0, blockWidth, blockHeight, defaultLayer, x, y, flipH, flipV, colorOffset, ignoreTransparent);
        }

        public int ReadPixelAt(int x, int y)
        {
            // Calculate the index
            var index = x + y * width;

            if (index >= defaultLayer.TotalPixels) return -1;
            
            // Flatten the canvas
            Draw();

            return defaultLayer[index];
        }

        public int[] SamplePixels(int x, int y, int width, int height)
        {
            // TODO this should be optimized if we are going to us it moving forward
            var totalPixels = width * height;
            var tmpPixels = new int[totalPixels];

            CopyPixels(ref tmpPixels, x, y, width, height);

            return tmpPixels;
        }

        public void CopyPixels(ref int[] data, int x, int y, int blockWidth, int blockHeight)
        {
            // Flatten the canvas
            Draw();
            
            PixelDataUtil.CopyPixels(ref data, defaultLayer, x, y, blockWidth, blockHeight);
        }

        public  void SetPixels(int x, int y, int blockWidth, int blockHeight, int[] pixels)
        {
            // Flatten the canvas
            Draw();
            
            if (wrap == false)
            {
                BlockSave(pixels, blockWidth, blockHeight, defaultLayer.Pixels, x, y, width, height);
                return;
            }

            PixelDataUtil.SetPixels(defaultLayer, x, y, blockWidth, blockHeight, pixels);
            
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
            int maskColor = -1, int maskColorID = -1, int colorOffset = 0, Rectangle? viewport = null,
            int[] isolateColors = null)
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
            var newWidth = (int) (tmpW * scale);
            var newHeight = (int) (tmpH * scale);

            var destPixels = scale > 1 ? ResizePixels(srcPixels, tmpW, tmpH, newWidth, newHeight) : srcPixels;

            gameChip.DrawPixels(destPixels, x, y, newWidth, newHeight, false, false, drawMode, colorOffset);
        }

        public  int[] GetPixels()
        {
            // Flatten the canvas
            Draw();
            
            return PixelDataUtil.GetPixels(defaultLayer);
            
        }

        public virtual void Clear(int colorRef = -1, int x = 0, int y = 0, int? width = null, int? height = null)
        {
            if (width.HasValue || height.HasValue)
            {
                var tmpWidth = width ?? 1;
                var tmpHeight = height ?? 1;

                var tmpPixels = new int[tmpWidth * tmpHeight];
                
                for (int i = 0; i < tmpPixels.Length; i++)
                {
                    tmpPixels[i] = colorRef;
                }
                
                SetPixels(x, y, tmpWidth, tmpHeight, tmpPixels);
            }
            else
            {
                PixelDataUtil.Clear(defaultLayer, colorRef);
                ResetValidation();
            }
            
        }
        
        public  int[] GetPixels(int x, int y, int blockWidth, int blockHeight)
        {
            // Flatten the canvas
            Draw();
            
            return PixelDataUtil.GetPixels(defaultLayer, x, y, blockWidth, blockHeight);
        }
        
        public virtual void SetPixels(int[] pixels)
        {
            // Flatten the canvas
            Draw();
            
            PixelDataUtil.SetPixels(pixels, defaultLayer);

            // Invalidate();
        }

        // Reference https://tech-algorithm.com/articles/nearest-neighbor-image-scaling/
        public int[] ResizePixels(int[] pixels, int w1, int h1, int w2, int h2)
        {
            int[] temp = new int[w2 * h2];
            // EDIT: added +1 to account for an early rounding problem
            int xRatio = (w1 << 16) / w2 + 1;
            int yRatio = (h1 << 16) / h2 + 1;
            int x2, y2;
            for (int i = 0; i < h2; i++)
            {
                for (int j = 0; j < w2; j++)
                {
                    x2 = ((j * xRatio) >> 16);
                    y2 = ((i * yRatio) >> 16);
                    temp[(i * w2) + j] = pixels[(y2 * w1) + x2];
                }
            }

            return temp;
        }

        public void Draw()
        {
            if (invalid == false)
                return;

            // Calculate the total requests based on the current request number
            _total = currentRequest + 1;

            // Loop through all off the requests
            for (int i = 0; i < _total; i++)
            {
                // Get the next request
                _request = requestPool[i];

                // Check to see if the action exists
                if (Actions.ContainsKey(_request.Action))
                {
                    // Pass the request into the action
                    Actions[_request.Action](_request);
                }
            }

            // Reset the request
            currentRequest = -1;

            ResetValidation();
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