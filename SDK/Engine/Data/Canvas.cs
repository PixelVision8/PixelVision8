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
using Microsoft.Xna.Framework;
using PixelVision8.Engine.Chips;
using PixelVision8.Engine.Utils;

namespace PixelVision8.Engine
{
    public struct CanvasDrawRequest
    {
        public string Action;
        public int X0;
        public int X1;
        public int Y0;
        public int Y1;
        public bool Fill;
        public bool DrawCentered;
        public Brush Stroke;
        public Brush Pattern;
        public int[] Ids;
        public bool FlipH;
        public bool FlipV;
        public int ColorOffset;
        public string Text;
        public string Font;
        public int Spacing;
        public TextureData TargetTexture;
    } 

    public class Brush : TextureData
    {
        public Brush(int width = 1, int height = 1) : base(width, height)
        {
        }
    }


    public class Canvas : TextureData, IDraw
    {
        private readonly GameChip gameChip;
        private readonly Brush pattern;
        private readonly Point spriteSize;
        private readonly Brush stroke;
        private readonly CanvasDrawRequest[] requestPool = new CanvasDrawRequest[1024];
        private readonly TextureData tmpLayer = new TextureData();
        private int currentRequest = -1;
        private bool canDraw;

        private Point linePattern = new Point(1, 0);
        public bool wrap = false;
        public Dictionary<string, Action<CanvasDrawRequest>> Actions;
        private TextureData currentTexture;

        // These are temporary values we use to help speed up calculations
        private int _x0;
        private int _y0;
        private int _x1;
        private int _y1;
        private int _w;
        private int _h;
        private Point _tl = Point.Zero;
        private Point _tr = Point.Zero;
        private Point _br = Point.Zero;
        private Point _bl = Point.Zero;
        private Point _center = Point.Zero;
        private int _total;
        private CanvasDrawRequest _request;

        public Canvas(int width, int height, GameChip gameChip = null) : base(width, height)
        {
            // Make the canvas the default drawing surface
            currentTexture = this;
            
            this.gameChip = gameChip;
            pattern = new Brush();
            pattern.SetPixel(0, 0, 0);

            stroke = new Brush();
            stroke.SetPixel(0, 0, 0);
            spriteSize = gameChip.SpriteSize();

            // Create a pool of draw requests
            for (int i = 0; i < requestPool.Length; i++)
            {
                requestPool[i] = new CanvasDrawRequest();
            }

            // TODO could we register external drawing calls to this?
            Actions = new Dictionary<string, Action<CanvasDrawRequest>>()
            {
                {"LinePattern", request => LinePatternAction(request)},
                {"SetStroke", request => SetStrokeAction(request)},
                {"SetPattern", request => SetPatternAction(request)},
                {"DrawLine", request => DrawLineAction(request)},
                // {"DrawCircle", request => DrawCircleAction(request)},
                {"DrawEllipse", request => DrawEllipseAction(request)},
                {"FloodFill", request => FloodFillAction(request)},
                {"DrawSprite", request => DrawSpriteAction(request)},
                {"DrawText", request => DrawTextAction(request)},
                {"ChangeTargetCanvas", request => ChangeTargetCanvasAction(request)},
                {"SaveTmpLayer", request => SaveTmpLayerAction(request)},
            };
            
        }

        public override void Resize(int width, int height)
        {
            base.Resize(width, height);

            // make sure the tmp layer is the same size as the canvas
            tmpLayer.Resize(width, height);
        }

        private void ChangeTargetCanvas(TextureData textureData, int? width = null, int? height = null)
        {
            var getRequest = NextRequest();

            if (getRequest == null)
                return;

            var newRequest = getRequest.Value;

            newRequest.Action = "ChangeTargetCanvas";
            newRequest.TargetTexture = textureData;
            newRequest.X0 = width ?? -1;
            newRequest.Y0 = height ?? -1;
            
            // Save the changes to the request
            requestPool[currentRequest] = newRequest;
        }

        private void ChangeTargetCanvasAction(CanvasDrawRequest drawRequest)
        {
            currentTexture = drawRequest.TargetTexture;
            
            if(drawRequest.X0 > 0 || drawRequest.Y0 > 0)
                currentTexture.Resize(drawRequest.X0, drawRequest.Y0);
        }

        public void LinePattern(int x, int y)
        {
            var getRequest = NextRequest();

            if (getRequest == null)
                return;

            var newRequest = getRequest.Value;

            newRequest.Action = "LinePattern";
            newRequest.X0 = x;
            newRequest.Y0 = y;
            
            // Save the changes to the request
            requestPool[currentRequest] = newRequest;
        }

        private void LinePatternAction(CanvasDrawRequest request)
        {
            linePattern.X = request.X0;
            linePattern.Y = request.Y0;
        }

        public void SetStroke(int color, int size = 1)
        {
            var getRequest = NextRequest();

            if (getRequest == null)
                return;

            var newRequest = getRequest.Value;

            newRequest.Action = "SetStroke";

            if (newRequest.Stroke == null)
            {
                newRequest.Stroke = new Brush(size, size);
            }
            else if (newRequest.Stroke.width != size || newRequest.Stroke.height != size)
            {
                newRequest.Stroke.Resize(size, size);
            }

            var newPixels = new int[size * size];
            for (int i = 0; i < newPixels.Length; i++)
            {
                newPixels[i] = color;
            }

            newRequest.Stroke.SetPixels(newPixels);
            
            // Save the changes to the request
            requestPool[currentRequest] = newRequest;
        }
        
        // public void SetStroke(int[] newPixels, int newWidth, int newHeight)
        // {
        //     var getRequest = NextRequest();
        //
        //     if (getRequest == null)
        //         return;
        //
        //     var newRequest = getRequest.Value;
        //
        //     newRequest.Action = "SetStroke";
        //
        //     if (newRequest.Stroke == null)
        //     {
        //         newRequest.Stroke = new Brush(newWidth, newHeight);
        //     }
        //     else if (newRequest.Stroke.width != newWidth || newRequest.Stroke.height != newHeight)
        //     {
        //         newRequest.Stroke.Resize(newWidth, newHeight);
        //     }
        //
        //     newRequest.Stroke.SetPixels(newPixels);
        //     
        //     // Save the changes to the request
        //     requestPool[currentRequest] = newRequest;
        // }

        private void SetStrokeAction(CanvasDrawRequest request)
        {
            if (stroke.width != request.Stroke.width || pattern.height != request.Stroke.height)
                stroke.Resize(request.Stroke.width, request.Stroke.height);

            stroke.SetPixels(request.Stroke.Pixels);
        }

        public void SetPattern(int[] newPixels, int newWidth, int newHeight)
        {
            var getRequest = NextRequest();

            if (getRequest == null)
                return;

            var newRequest = getRequest.Value;

            newRequest.Action = "SetPattern";

            if (newRequest.Pattern == null)
            {
                newRequest.Pattern = new Brush(newWidth, newHeight);
            }
            else if (newRequest.Pattern.width != newWidth || newRequest.Pattern.height != newHeight)
            {
                newRequest.Pattern.Resize(newWidth, newHeight);
            }

            newRequest.Pattern.SetPixels(newPixels);
            
            // Save the changes to the request
            requestPool[currentRequest] = newRequest;
        }

        public void SetPatternAction(CanvasDrawRequest request)
        {
            if (pattern.width != request.Pattern.width || pattern.height != request.Pattern.height)
                pattern.Resize(request.Pattern.width, request.Pattern.height);

            pattern.SetPixels(request.Pattern.GetPixels());
        }

        private void SetStrokePixel(int x, int y)
        {
            canDraw = wrap || x >= 0 && x <= width - stroke.width && y >= 0 && y <= height - stroke.height;
            
            // TODO this should never be null           
            if (canDraw) 
                currentTexture.SetPixels(x, y, stroke.width, stroke.height, stroke.Pixels);
            
        }

        private CanvasDrawRequest? NextRequest()
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

            var newRequest = getRequest.Value;

            newRequest.Action = "DrawLine";

            newRequest.X0 = x0;
            newRequest.Y0 = y0;
            newRequest.X1 = x1;
            newRequest.Y1 = y1;

            // Save the changes to the request
            requestPool[currentRequest] = newRequest;
        }

        int _counter = 0;
        // int _dx;
        int _sx;
        int _sy;
        
        private void DrawLineAction(CanvasDrawRequest drawRequest)
        {
            _x0 = drawRequest.X0;
            _y0 = drawRequest.Y0;
            _x1 = drawRequest.X1;
            _y1 = drawRequest.Y1;

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

        public void DrawSquare(int x0, int y0, int x1, int y1, bool fill = false, bool drawCentered = false)
        {
            
            // Save the x and y values to calculate below
            _x0 = Math.Min(x0, x1);
            _y0 = Math.Min(y0, y1);
            _x1 = Math.Max(x0, x1);
            _y1 = Math.Max(y0, y1);

            // Calculate the width and height
            _w = _x1 - _x0;
            _h = _y1 - _y0;
            
            if (fill)
            {
                ChangeTargetCanvas(tmpLayer, _w, _h);
                _x0 = 0;
                _y0 = 0;
                _x1 = _w;
                _y1 = _h;
            }

            // Calculate the top left
            _tl.X = _x0;
            _tl.Y = _y0;

            // Calculate the top right
            _tr.X = _x0 + _w - (stroke.width * 2);
            _tr.Y = _y0;

            // Calculate the bottom right
            _br.X = _x0 + _w - (stroke.width * 2);
            _br.Y = _y0 + _h - (stroke.height * 2);

            // Calculate the bottom left
            _bl.X = _x0;
            _bl.Y = _y0 + _h - (stroke.height * 2);

            // Determine if the box  should be drawn from the center
            if (drawCentered)
            {
                // Adjust values based on center position
                _tl.X -= _w;
                _tl.Y -= _h;
                _tr.Y -= _h;
                _bl.X -= _w;
                _center.X = _tl.X + _w;
                _center.Y = _tl.Y + _h;
            }
            else
            {
                // Calculate the center of the rectangle
                _center.X = _tl.X + _w / 2;
                _center.Y = _tl.Y + _h / 2;
            }

            // Top
            DrawLine(_tl.X, _tl.Y, _tr.X, _tr.Y);

            // Left
            DrawLine(_tl.X, _tl.Y, _bl.X, _bl.Y);

            // Right
            DrawLine(_tr.X, _tr.Y, _br.X, _br.Y);

            // Bottom
            DrawLine(_bl.X, _bl.Y, _br.X, _br.Y);

            // Check again to see if we need to fill the rectangle
            if (fill)
            {
                // Make sure there are enough pixels to fill
                if (Math.Abs(_w) > stroke.width && Math.Abs(_h) > stroke.height)
                {
                    // Trigger a flood fill
                    FloodFill(_center.X, _center.Y);

                    // Copy pixels data to the main drawing surface
                    SaveTmpLayer(x0, y0, _w, _h);
                    
                    // Change back to default drawing surface
                    ChangeTargetCanvas(this);
                }
            }
        }


        private void SaveTmpLayer(int x, int y, int blockWidth, int blockHeight)
        {
            var getRequest = NextRequest();

            if (getRequest == null)
                return;

            var newRequest = getRequest.Value;
            
            newRequest.Action = "SaveTmpLayer";
            newRequest.X0 = x;
            newRequest.Y0 = y;
            newRequest.X1 = blockWidth;
            newRequest.Y1 = blockHeight;
            
            // Save the changes to the request
            requestPool[currentRequest] = newRequest;
        }

        private void SaveTmpLayerAction(CanvasDrawRequest request)
        {
            // Copy pixels data to the main drawing surface
            MergePixels(request.X0, request.Y0, request.X1, request.Y1, tmpLayer.GetPixels());
        }
        
        public void DrawEllipse(int x0, int y0, int x1, int y1, bool fill = false, bool drawCentered = false)
        {
            _w = x1 - x0;
            _h = y1 - y0;
            
            if(fill)
                ChangeTargetCanvas(tmpLayer, _w, _h);
            
            var getRequest = NextRequest();

            if (getRequest == null)
                return;

            var newRequest = getRequest.Value;
            
            newRequest.X0 = x0;
            newRequest.Y0 = y0;
            newRequest.X1 = x1;
            newRequest.Y1 = y1;
            newRequest.Fill = fill;
            newRequest.DrawCentered = drawCentered;

            newRequest.Action = "DrawEllipse";
            
            // Save the changes to the request
            requestPool[currentRequest] = newRequest;
            
            SaveTmpLayer(x0, y0, _w, _h);
            
            // Change back to default drawing surface
            ChangeTargetCanvas(this);
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
            // TODO need to offset this by the stroke
            
            _w = Math.Abs(request.X1 - request.X0);
            _h = Math.Abs(request.Y1 - request.Y0);
            
            // Save the x and y values to calculate below
            _x0 = Math.Min(request.X0, request.X1);
            _y0 = Math.Min(request.Y0, request.Y1);
            _x1 = Math.Max(request.X0, request.X1);
            _y1 = Math.Max(request.Y0, request.Y1);
            
            if (request.Fill)
            {
                _x0 = 0;
                _y0 = 0;
                _x1 = _w;
                _y1 = _h;
            }

            // Adjust for border
            _y0 += stroke.height;
            _x1 -= stroke.width;
            _y1 -= stroke.height;
            
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
            // y1 = y0–b1; /* starting pixel */
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

                // var centerX = request.X0 + w / 2;
                // var centerY = request.Y0 + h / 2;

                _x1 = request.X0;
                _y1 = request.Y0;
                
                if (Math.Abs(_w) > stroke.width && Math.Abs(_h) > stroke.height)
                {
                    request.X0 = _w / 2;
                    request.Y0 = _h / 2;
                    
                    FloodFillAction(request);

                }
                
                // MergePixels(_x1, _y1, _w, _h, currentTexture.GetPixels());

                // currentTexture = this;
            }
        }

        public void DrawSprite(int id, int x, int y, bool flipH = false, bool flipV = false, int colorOffset = 0)
        {
            var getRequest = NextRequest();

            if (getRequest == null)
                return;

            var newRequest = getRequest.Value;

            newRequest.Action = "DrawSprite";

            if (newRequest.Ids == null)
            {
                newRequest.Ids = new int[1];
            }
            else if (newRequest.Ids.Length < 1)
            {
                Array.Resize(ref newRequest.Ids, 1);
            }

            newRequest.Ids[0] = id;
            newRequest.X0 = x;
            newRequest.Y0 = y;
            newRequest.FlipH = flipH;
            newRequest.FlipV = flipV;
            newRequest.ColorOffset = colorOffset;
            
            // Save the changes to the request
            requestPool[currentRequest] = newRequest;
        }

        /// <summary>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="colorOffset"></param>
        public void DrawSpriteAction(CanvasDrawRequest request)
        {
            // This only works when the canvas has a reference to the gameChip
            if (gameChip == null) return;

            MergePixels(request.X0, request.Y0, spriteSize.X, spriteSize.Y, gameChip.Sprite(request.Ids[0]),
                request.FlipH, request.FlipV, request.ColorOffset);
            
            Invalidate();
        }


        public void DrawSprites(int[] ids, int x, int y, int width, bool flipH = false, bool flipV = false,
            int colorOffset = 0)
        {
            _total = ids.Length;

            // TODO added this so C# code isn't corrupted, need to check performance impact
            // if (tmpIDs.Length != total) Array.Resize(ref tmpIDs, total);
            //
            // Array.Copy(ids, tmpIDs, total);

            var height = MathUtil.CeilToInt(_total / width);

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
                    //
                    //                    var render = true;

                    // Check to see if we need to test the bounds

                    DrawSprite(id, x, y, flipH, flipV, colorOffset);
                }
            }
        }

        public void DrawText(string text, int x, int y, string font = "default", int colorOffset = 0, int spacing = 0)
        {
            var getRequest = NextRequest();

            if (getRequest == null)
                return;

            var newRequest = getRequest.Value;

            newRequest.Action = "DrawText";

            if (newRequest.Ids == null)
            {
                newRequest.Ids = new int[1];
            }
            else if (newRequest.Ids.Length < 1)
            {
                Array.Resize(ref newRequest.Ids, 1);
            }

            newRequest.Text = text;
            newRequest.X0 = x;
            newRequest.Y0 = y;
            newRequest.Font = font;
            newRequest.Spacing = spacing;
            newRequest.ColorOffset = colorOffset;
            
            // Save the changes to the request
            requestPool[currentRequest] = newRequest;
        }

        /// <summary>
        /// </summary>
        /// <param name="text"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="font"></param>
        /// <param name="colorOffset"></param>
        /// <param name="spacing"></param>
        public void DrawTextAction(CanvasDrawRequest request)
        {
            // This only works when the canvas has a reference to the gameChip
            if (gameChip == null) return;

            //            var ids = gameChip.ConvertTextToSprites(text, font);
            var total = request.Text.Length;
            var nextX = request.X0;
            var nextY = request.Y0;

            for (var i = 0; i < total; i++)
            {
                MergePixels(nextX, nextY, spriteSize.X, spriteSize.Y,
                    ((GameChip) gameChip).CharacterToPixelData(request.Text[i], request.Font), false, false,
                    request.ColorOffset);


                //                DrawSprite(ids[i], nextX, nextY, false, false, colorOffset);
                nextX += spriteSize.X + request.Spacing;
            }
        }

        public void FloodFill(int x, int y)
        {
            var getRequest = NextRequest();

            if (getRequest == null)
                return;

            var newRequest = getRequest.Value;

            newRequest.Action = "FloodFill";
            newRequest.X0 = x;
            newRequest.Y0 = y;
            
            // Save the changes to the request
            requestPool[currentRequest] = newRequest;
        }

        /// <summary>
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void FloodFillAction(CanvasDrawRequest request)
        {
            if (request.X0 < 0 || request.Y0 < 0 || request.X0 > width || request.Y0 > height) return;

            // Get the color at the point where we are trying to fill and use that to match all the color inside the shape
            var targetColor = currentTexture.GetPixel(request.X0, request.Y0);

            var pixels = new Stack<Point>();

            pixels.Push(new Point(request.X0, request.Y0));

            while (pixels.Count != 0)
            {
                var temp = pixels.Pop();
                var y1 = temp.Y;
                while (y1 >= 0 && currentTexture.GetPixel(temp.X, y1) == targetColor) y1--;

                y1++;
                var spanLeft = false;
                var spanRight = false;
                while (y1 < height && currentTexture.GetPixel(temp.X, y1) == targetColor)
                {
                    currentTexture.SetPixel(temp.X, y1, pattern.GetPixel(temp.X, y1));

                    if (!spanLeft && temp.X > 0 && currentTexture.GetPixel(temp.X - 1, y1) == targetColor)
                    {
                        if (currentTexture.GetPixel(temp.X - 1, y1) != pattern.GetPixel(temp.X, y1))
                            pixels.Push(new Point(temp.X - 1, y1));

                        spanLeft = true;
                    }
                    else if (spanLeft && temp.X - 1 == 0 && currentTexture.GetPixel(temp.X - 1, y1) != targetColor)
                    {
                        spanLeft = false;
                    }

                    if (!spanRight && temp.X < width - 1 && currentTexture.GetPixel(temp.X + 1, y1) == targetColor)
                    {
                        if (currentTexture.GetPixel(temp.X + 1, y1) != pattern.GetPixel(temp.X, y1))
                            pixels.Push(new Point(temp.X + 1, y1));

                        spanRight = true;
                    }
                    else if (spanRight && temp.X < width - 1 && currentTexture.GetPixel(temp.X + 1, y1) != targetColor)
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
            MergePixels(0, 0, canvas.width, canvas.height, canvas.Pixels, false, false, colorOffset);
        }

        public int ReadPixelAt(int x, int y)
        {
            // Calculate the index
            var index = x + y * width;

            if (index >= Pixels.Length) return -1;

            return Pixels[index];
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
                BlockSave(pixels, blockWidth, blockHeight, this.Pixels, x, y, width, height);
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
            int maskColor = -1, int maskColorID = -1, int colorOffset = 0, Rectangle? viewport = null,
            int[] isolateColors = null)
        {
            // Flatten the canvas
            Draw();

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

        public override int[] GetPixels()
        {
            if(invalid)
                Draw();
            
            return base.GetPixels();
            
        }

        public override int[] GetPixels(int x, int y, int blockWidth, int blockHeight)
        {
            if(invalid)
                Draw();
            
            return base.GetPixels(x, y, blockWidth, blockHeight);
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

        public TextureData texture => this;
        public int textureWidth => this.width;
        public int textureHeight => this.height;
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