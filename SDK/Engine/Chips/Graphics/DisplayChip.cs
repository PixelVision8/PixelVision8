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
using PixelVision8.Engine.Utils;

namespace PixelVision8.Engine.Chips
{
    public class DisplayChip : AbstractChip, IDraw
    {
        // private Color[] cachedColors;

        private DrawRequestPixelData draw;
        // protected List<DrawRequestPixelData>[] DrawRequestPixelDataLayers = new List<DrawRequestPixelData>[0];
        // protected Stack<int[]> DrawRequestPixelDataPool = new Stack<int[]>();
        public int[] Pixels = new int[0];
        public int TotalPixels;
        public int OverscanX { get; set; }
        public int OverscanY { get; set; }
        
        
        private List<DrawRequestPixelData> DrawCalls = new List<DrawRequestPixelData>();
        private DrawRequestPixelData[] DrawRequestPool = new DrawRequestPixelData[0];
        private int drawRequestCounter = -1;
        
        public int MaxDrawRequests
        {
            set
            {
                if(value == 0)
                    return;
                
                _maxDrawRequests = value;
                if (DrawRequestPool.Length != _maxDrawRequests)
                {
                    Array.Resize(ref DrawRequestPool, _maxDrawRequests);
                    DrawCalls.Capacity = _maxDrawRequests;
                }
            }
        }

        // This should be part of the chip's data
        private int _maxDrawRequests = 1024;
        
        int _total;
        int _srcX;
        int _srcY;
        int _colorID;
        int i1;
        int _index;

        private int[] emptyPixels = new int[0];

        public int OverscanXPixels => OverscanX * engine.SpriteChip.width;

        public int OverscanYPixels => OverscanY * engine.SpriteChip.height;

        /// <summary>
        ///     This returns the visble areas sprites should be displayed on. Note that x and y may be negative if overscan is set
        ///     since the screen wraps.
        /// </summary>
        public Rectangle VisibleBounds => new Rectangle(-OverscanXPixels, -OverscanYPixels, Width - OverscanXPixels,
            Height - OverscanYPixels);

        /// <summary>
        ///     Returns the display's <see cref="Width" />
        /// </summary>
        public int Width { get; protected set; } = 256;

        /// <summary>
        ///     Returns the display's <see cref="Height" />
        /// </summary>
        public int Height { get; protected set; } = 240;


        private List<DrawRequestPixelData> _drawRequests;
        private int _totalDR;
        private int _layer;
        private int _i;
        private DrawRequestPixelData _drawRequest;
        private int drawCallCounter = -1;
        private bool _clearFlag = true;

        /// <summary>
        /// </summary>
        public void Draw()
        {
            drawCallCounter = DrawCalls.Count;

            if (_clearFlag)
            {

                // Loop through all of the display pixels
                for (_i = 0; _i < TotalPixels; _i++)
                {
                    // We always set the clear color to -1 since the display target will automatically convert this into the background color
                    Pixels[_i] = -1;
                }

                // Reset the clear flag for the next frame
                _clearFlag = false;
            }
            
            // Sort sprite draw calls
            DrawCalls.Sort((x, y) => x.Priority.CompareTo(y.Priority));

            // if(drawRequestCounter  < 0)
            //     return;
            
            for (int i = 0; i < drawCallCounter; i++)
            {
                _drawRequest = DrawCalls[i];
                CopyDrawRequestPixelData(_drawRequest.isRectangle ? null : _drawRequest.PixelData.Pixels, _drawRequest.x, _drawRequest.y, _drawRequest.width, _drawRequest.height,
                _drawRequest.flipH, _drawRequest.flipV, _drawRequest.colorOffset);
            }
           
            // Reset Draw Requests after they have been processed
            ResetDrawCalls();

        }
        
        public void Clear()
        {
            _clearFlag = true;
        }

        /// <summary>
        ///     Creates a new draw by copying the supplied pixel data over
        ///     to the Display's TextureData.
        /// </summary>
        /// <param name="pixelData"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="layer"></param>
        /// <param name="flipH">
        ///     This is an optional argument which accepts a bool. The default value is set to false but passing in true flips
        ///     the pixel data horizontally.
        /// </param>
        /// <param name="flipV">
        ///     This is an optional argument which accepts a bool. The default value is set to false but passing in true flips
        ///     the pixel data vertically.
        /// </param>
        /// <param name="colorOffset"></param>
        /// <param name="layerOrder"></param>
        public void NewDrawCall(int[] pixelData, int x, int y, int width, int height, byte layer = 0, bool flipH = false,
            bool flipV = false, int colorOffset = 0)
        {
            
            // Exit if we are drawing to a layer that doesn't exist
            // if (layer >= layers)
            //     return;

            if (pixelData == null)
            {
                var total = width * height;
                
                if(emptyPixels.Length != total)
                    Array.Resize(ref emptyPixels, total);

                pixelData = emptyPixels;

            }

            var request = NextDrawRequest();

            if (request.HasValue)
            {
                draw = request.Value;
                draw.x = x;
                draw.y = y;
                draw.SetPixels(pixelData, width, height);
                draw.Priority = layer;
                draw.flipH = flipH;
                draw.flipV = flipV;
                draw.colorOffset = colorOffset;
                
                DrawCalls.Add(draw);
                // DrawRequestPixelDataLayers[layer].Add(draw);
            }
            
        }
 
        /// <summary>
        ///     Changes the resolution of the display.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void ResetResolution(int width, int height)
        {
            Width = width;
            Height = height;

            TotalPixels = Width * Height;

            Array.Resize(ref Pixels, TotalPixels);
        }

        /// <summary>
        ///     This configures the DisplayChip. It registers itself as the default
        ///     <see cref="DisplayChip" /> for the engine, gets a reference to the
        ///     engine's renderTarget, sets <see cref="autoClear" /> and
        ///     <see cref="wrapMode" /> to true and
        ///     finally resets the resolution to its default value
        ///     of 256 x 240.
        /// </summary>
        public override void Configure()
        {
            //Debug.Log("Pixel Data Renderer: Configure ");
            engine.DisplayChip = this;

            ResetResolution(256, 240);

            // By default set the total layers to the DrawModes minus Tilemap Cache which isn't used for rendering
            // layers = Enum.GetNames(typeof(DrawMode)).Length - 1;

            MaxDrawRequests = 1024;

            // TODO should the display have the sprite limit and the game chip looks there first

            // SpriteDrawRequestPool = new Stack<DrawRequest>();
            //
            // var maxCalls = SpriteChip.maxSpriteCount > 0 ? SpriteChip.maxSpriteCount : maxDrawRequests;
            //
            // for (int i = 0; i < maxCalls; i++)
            // {
            //     SpriteDrawRequestPool.Push(new DrawRequest());
            // }


        }

        public override void Deactivate()
        {
            base.Deactivate();
            engine.DisplayChip = null;
        }

        public void ResetDrawCalls()
        {

            drawRequestCounter = -1;
            DrawCalls.Clear();
        }

        public DrawRequestPixelData? NextDrawRequest()
        {
            drawRequestCounter++;

            if (drawRequestCounter >= _maxDrawRequests)
            {
                // drawRequestCounter = _maxDrawRequests;
                return null;
            }

            return DrawRequestPool[drawRequestCounter];
        }

        public void CopyDrawRequestPixelData(int[] pixelData, int x, int y, int width, int height, bool flipH = false,
            bool flipV = false, int colorOffset = 0)
        {
            
            var tmpWidth = this.Width;
            var tmpHeight = this.Height;

            _total = width * height;

            for (i1 = 0; i1 < _total; i1++)
            {
                _colorID = pixelData?[i1] ?? 0;

                if (_colorID > -1)
                {
                    if (colorOffset > 0) _colorID += colorOffset;

                    _srcX = i1 % width;
                    _srcY = i1 / width;

                    if (flipH) _srcX = width - 1 - _srcX;

                    if (flipV) _srcY = height - 1 - _srcY;

                    _srcX += x;
                    _srcY += y;

                    // Make sure x & y are wrapped around the display
                    // Note: + size and the second modulo operation are required to get wrapped values between 0 and +size
                    _srcY = (_srcY % tmpHeight + tmpHeight) % tmpHeight;
                    _srcX = (_srcX % tmpWidth + tmpWidth) % tmpWidth;
                    // size is still == _width from the previous operation - let's reuse the local

                    // Find the index
                    _index = _srcX + tmpWidth * _srcY;

                    // Set the pixel
                    Pixels[_index] = _colorID;//cachedColors[_colorID];
                }
            }
        }

        public Color[] VisiblePixels()
        {

            // TODO there might be a better way to do this like grabbing the pixel data from somewhere else?
            var pixels = engine.DisplayChip.Pixels;

            var cachedColors = ColorUtils.ConvertColors(engine.ColorChip.hexColors, engine.ColorChip.maskColor, engine.ColorChip.debugMode, engine.ColorChip.backgroundColor);

            // var cachedColors = engine.ColorChip.colors;
            var displaySize = engine.GameChip.Display();

            var visibleWidth = displaySize.X;
            var visibleHeight = displaySize.Y;
            var width = engine.DisplayChip.Width;

            // Need to crop the image
            var newPixels = new Color[visibleWidth * visibleHeight];

            var totalPixels = pixels.Length;
            var newTotalPixels = newPixels.Length;

            var index = 0;

            for (var i = 0; i < totalPixels; i++)
            {
                var col = i % width;
                if (col < visibleWidth && index < newTotalPixels)
                {
                    newPixels[index] = cachedColors[pixels[i]];
                    index++;
                }
            }

            return newPixels;
        }

        
    }
}