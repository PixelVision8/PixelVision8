﻿//   
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
using System.Linq;
using PixelVisionSDK.Utils;

namespace PixelVisionSDK.Chips
{

    public class DisplayChip : AbstractChip, IDraw
    {
        protected int _width = 256;
        protected int _height = 240;
        public int overscanX { get; set; }
        public int overscanY { get; set; }
        
        public int[] displayPixels = new int[0];
        protected List<DrawRequest> drawRequestPool = new List<DrawRequest>();
        protected List<DrawRequest> drawRequests = new List<DrawRequest>();
        private int totalPixels;

        public int layers = 1;

        public int overscanXPixels
        {
            get { return overscanX * engine.spriteChip.width; }
        }

        public int overscanYPixels
        {
            get { return overscanY * engine.spriteChip.height; }
        }

        public Rect visibleBounds
        {
            get
            {
                return new Rect(-overscanXPixels, -overscanYPixels, width - overscanXPixels, height - overscanYPixels);
            }
        }

        /// <summary>
        ///     Returns the display's <see cref="width" />
        /// </summary>
        public int width
        {
            get { return _width; }
        }

        /// <summary>
        ///     Returns the display's <see cref="height" />
        /// </summary>
        public int height
        {
            get { return _height; }
        }

        /// <summary>
        /// </summary>
        public void Draw()
        {

            // Sort draw requests by their draw mode
            DrawRequest[] sorted = drawRequests.OrderBy(c => c.layer).ToArray();
            
            // Loop through all draw requests
            var totalDR = sorted.Length;

            for (var i = 0; i < totalDR; i++)
            {
                var draw = sorted[i];

                CopyDrawRequest(ref displayPixels, draw.pixelData, draw.x, draw.y, draw.width, draw.height, _width, draw.colorOffset);

            }

            // Reset Draw Requests after they have been processed
            ResetDrawCalls();
            
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
        /// <param name="flipH"></param>
        /// <param name="flipV"></param>
        /// <param name="flipY"></param>
        /// <param name="layer"></param>
        /// <param name="colorOffset"></param>
        /// <param name="layerOrder"></param>
        public void NewDrawCall(int[] pixelData, int x, int y, int width, int height, bool flipH, bool flipV, bool flipY, int layer = 0, int colorOffset = 0)
        {

            // flip y coordinate space
            if (flipY)
                y = _height - height - y;

            if (pixelData != null)
            {
                if (flipH || flipV)
                    SpriteChipUtil.FlipSpriteData(ref pixelData, width, height, flipH, flipV);

                var draw = NextDrawRequest();
                draw.x = x;
                draw.y = y;
                draw.width = width;
                draw.height = height;
                draw.pixelData = pixelData;
                draw.layer = layer;
                draw.colorOffset = colorOffset;
                drawRequests.Add(draw);

            }
        }

        /// <summary>
        ///     Changes the resolution of the display.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public void ResetResolution(int width, int height)
        {
            _width = width;
            _height = height;
            
            totalPixels = _width * _height;

            Array.Resize(ref displayPixels, totalPixels);

            for (int i = 0; i < totalPixels; i++)
            {
                displayPixels[i] = -1;
            }
//            Clear();
            
            // Resize UI Layer
//            uiLayer.Resize(_width, _height);
//            uiLayer.Clear();
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
            engine.displayChip = this;

            ResetResolution(256, 240);

        }

        public override void Deactivate()
        {
            base.Deactivate();
            engine.displayChip = null;
        }

        public void ResetDrawCalls()
        {
            // Reset all draw requests and pools
            while (drawRequests.Count > 0)
            {
                var request = drawRequests[0];

                drawRequests.Remove(request);

                drawRequestPool.Add(request);
            }
        }

        public DrawRequest NextDrawRequest()
        {
            DrawRequest request;

            if (drawRequestPool.Count > 0)
            {
                request = drawRequestPool[0];
                drawRequestPool.Remove(request);
            }
            else
            {
                request = new DrawRequest();
            }

            return request;
        }

        public void CopyDrawRequest(ref int[] destPixelData, int[] pixelData, int x, int y, int width, int height, int destWidth, int colorOffset = 0)
        {
            var total = width * height;
            int srcX = x;
            int srcY = y;

            var tmpWidth = x + width;
            int destIndex;
            var colorID = -1;
            var totalPixels = destPixelData.Length;

            for (var i = 0; i < total; i++)
            {
                
                colorID = pixelData[i];

                if (colorID > -1)
                {
                    if (colorOffset > 0)
                        colorID += colorOffset;

                    
                    destIndex = srcX + srcY * destWidth;

                    if (destIndex < totalPixels && destIndex > -1)
                    {
                    destPixelData[destIndex] = colorID;

                    }
                }
                
                srcX ++;
                if (srcX >= tmpWidth)
                {
                    srcX = x;
                    srcY++;
                }

            }
        }
        
    }

}