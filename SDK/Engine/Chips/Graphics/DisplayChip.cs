﻿﻿//   
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
        private PixelData Display = new PixelData();
        // protected List<DrawRequestPixelData>[] DrawRequestPixelDataLayers = new List<DrawRequestPixelData>[0];
        // protected Stack<int[]> DrawRequestPixelDataPool = new Stack<int[]>();
        public int[] Pixels => Display.Pixels;

        public int TotalPixels => Display.TotalPixels;
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
        public int Width => Display.Width;

        /// <summary>
        ///     Returns the display's <see cref="Height" />
        /// </summary>
        public int Height => Display.Height;


        private List<DrawRequestPixelData> _drawRequests;
        // private int _totalDR;
        // private int _layer;
        private int _i;
        private DrawRequestPixelData _drawRequest;
        private int drawCallCounter = -1;
        private bool _clearFlag = true;
        private int clearColor = -1;
        
        /// <summary>
        /// </summary>
        public void Draw()
        {
            drawCallCounter = DrawCalls.Count;

            if (_clearFlag)
            {

                // TODO need to use Clear instead of a loop after re-indexing colors correctly
                // Array.Clear(Pixels, 0, TotalPixels);
                
                // Loop through all of the display pixels
                for (_i = TotalPixels-1 ; _i > -1; _i--)
                {
                    // We always set the clear color to -1 since the display target will automatically convert this into the background color
                    Pixels[_i] = clearColor;
                }

                // Reset the clear flag for the next frame
                _clearFlag = false;
            }
            
            // Sort sprite draw calls
            DrawCalls.Sort((x, y) => x.Priority.CompareTo(y.Priority));

            for (_i = 0; _i < drawCallCounter; _i++)
            {
                _drawRequest = DrawCalls[_i];
                
                PixelDataUtil.MergePixels(_drawRequest.PixelData, _drawRequest.SampleRect.X, _drawRequest.SampleRect.Y, _drawRequest.SampleRect.Width, _drawRequest.SampleRect.Height, Display, _drawRequest.x, _drawRequest.y, _drawRequest.FlipH, _drawRequest.FlipV, _drawRequest.ColorOffset);
                
            }
           
            // Reset Draw Requests after they have been processed
            ResetDrawCalls();

        }
        
        public void Clear(int color = -1)
        {
            clearColor = color;
            
            _clearFlag = true;
        }

        public void NewDrawCall(int[] pixels, int destX, int destY, int blockWidth, int blockHeight, byte layer = 0,
            bool flipH = false,
            bool flipV = false, int colorOffset = 0, int srcX = 0, int srcY = 0)
        {

            NextDrawRequest(destX, destY, layer, flipH, flipV, colorOffset);
                
            if (nextDrawRequest)
            {
                _drawRequest.SampleFrom(pixels, srcX, srcY, blockWidth, blockHeight);
                DrawCalls.Add(_drawRequest);
            }
            
        }
        

        /// <summary>
        ///     Creates a new draw by copying the supplied pixel data over
        ///     to the Display's TextureData.
        /// </summary>
        /// <param name="pixelData"></param>
        /// <param name="destX"></param>
        /// <param name="destY"></param>
        /// <param name="blockWidth"></param>
        /// <param name="blockHeight"></param>
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
        public void NewDrawCall(IDisplay src, int destX, int destY, int blockWidth, int blockHeight, byte layer = 0, bool flipH = false,
            bool flipV = false, int colorOffset = 0, int srcX = 0, int srcY = 0)
        {
            NextDrawRequest(destX, destY, layer, flipH, flipV, colorOffset);
            
            if (nextDrawRequest)
            {
                // _drawRequest = request.Value;
                _drawRequest.SampleFrom(src, srcX, srcY, blockWidth, blockHeight);
                DrawCalls.Add(_drawRequest);
                 
            }
            
        }
 
        /// <summary>
        ///     Changes the resolution of the display.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void ResetResolution(int width, int height) => Display.Resize(width, height);

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

        private bool nextDrawRequest;
        
        public void NextDrawRequest(int destX, int destY, byte layer = 0, bool flipH = false, bool flipV = false, int colorOffset = 0)
        {
            drawRequestCounter++;

            nextDrawRequest = drawRequestCounter < _maxDrawRequests;
            
            if (nextDrawRequest == false)
                return;
            
            _drawRequest = DrawRequestPool[drawRequestCounter];
            
            _drawRequest.x = destX;
            _drawRequest.y = destY;
            _drawRequest.Priority = layer;
            _drawRequest.FlipH = flipH;
            _drawRequest.FlipV = flipV;
            _drawRequest.ColorOffset = colorOffset;
            
        }

        // public Color[] VisiblePixels()
        // {
        //
        //     // TODO there might be a better way to do this like grabbing the pixel data from somewhere else?
        //     var pixels = engine.DisplayChip.Pixels;
        //
        //     var cachedColors = ColorUtils.ConvertColors(engine.ColorChip.hexColors, engine.ColorChip.maskColor, engine.ColorChip.debugMode, engine.ColorChip.backgroundColor);
        //
        //     // var cachedColors = engine.ColorChip.colors;
        //     var displaySize = engine.GameChip.Display();
        //
        //     var visibleWidth = displaySize.X;
        //     var visibleHeight = displaySize.Y;
        //     var width = engine.DisplayChip.Width;
        //
        //     // Need to crop the image
        //     var newPixels = new Color[visibleWidth * visibleHeight];
        //
        //     var totalPixels = pixels.Length;
        //     var newTotalPixels = newPixels.Length;
        //
        //     var index = 0;
        //
        //     for (var i = 0; i < totalPixels; i++)
        //     {
        //         var col = i % width;
        //         if (col < visibleWidth && index < newTotalPixels)
        //         {
        //             newPixels[index] = cachedColors[pixels[i]];
        //             index++;
        //         }
        //     }
        //
        //     return newPixels;
        // }

        
    }
}