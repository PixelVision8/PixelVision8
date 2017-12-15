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
using System.Linq;
using PixelVisionSDK.Utils;
using UnityEngine;

namespace PixelVisionSDK.Chips
{

    public class DisplayChip : AbstractChip, IDraw
    {

        protected int _height = 240;
        protected int _maxSpriteCount = 64;
        protected int _overscanX;
        protected int _overscanY;
        protected int _scrollX;
        protected int _scrollY;
        protected int _width = 256;
        protected TextureData uiLayer = new TextureData(0, 0);
        private bool clearUIFlag;
        public int[] displayPixels = new int[0];
        protected List<DrawRequest> drawRequestPool = new List<DrawRequest>();
        protected List<DrawRequest> drawRequests = new List<DrawRequest>();
        private int totalPixels;
        protected bool clearFlag { get; set; }

        public int overscanX
        {
            get { return _overscanX; }
            set { _overscanX = value; }
        }

        public int overscanY
        {
            get { return _overscanY; }
            set { _overscanY = value; }
        }

        public int overscanXPixels
        {
            get { return _overscanX * engine.spriteChip.width; }
        }

        public int overscanYPixels
        {
            get { return _overscanY * engine.spriteChip.height; }
        }

        public Rect visibleBounds
        {
            get
            {
                return new Rect(-overscanXPixels, -overscanYPixels, width - overscanXPixels, height - overscanYPixels);
            }
        }

        /// <summary>
        ///     This value is used for horizontally scrolling the ScreenBufferChip.
        ///     The <see cref="scrollX" /> field represents starting x position of
        ///     the <see cref="TextureData" /> to sample from. 0 is the left of the
        ///     screen;
        /// </summary>
        public int scrollX
        {
            get { return _scrollX; }
            set { _scrollX = value; }
        }

        /// <summary>
        ///     This value is used for vertically scrolling the ScreenBufferChip.
        ///     The <see cref="scrollY" /> field represents starting y position of
        ///     the <see cref="TextureData" /> to sample from. 0 is the top of the
        ///     screen;
        /// </summary>
        public int scrollY
        {
            get { return _scrollY; }
            set { _scrollY = value; }
        }

        /// <summary>
        ///     Sets the total number of sprite draw calls for the display.
        /// </summary>
        public int maxSpriteCount
        {
            get { return _maxSpriteCount; }
            set { _maxSpriteCount = value; }
        }

        /// <summary>
        ///     This toggles wrap mode on the display. If pixel data is draw past
        ///     the end of the display it will appear on the opposite side. There is
        ///     a slight performance hit for this.
        /// </summary>
        //public bool wrapMode { get; set; }
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

        protected TilemapChip tilemapChip
        {
            get { return engine.tilemapChip; }
        }

        /// <summary>
        /// </summary>
        public void Draw()
        {

            // Create draw call for the UI layer
            NewDrawCall(uiLayer.pixels, 0, 0, width, height, false, false, false, DrawMode.UI);
            
            // Go through and clear the background
            if (clearFlag)
            {
                for (int i = 0; i < totalPixels; i++)
                {
                    displayPixels[i] = -1;
                }
            }
            
            // Sort draw requests by their draw mode
            DrawRequest[] sorted = drawRequests.OrderBy(c => (int)c.drawMode).ToArray();
            
            // Loop through all draw requests
            var totalDR = sorted.Length;

            for (var i = 0; i < totalDR; i++)
            {
                var draw = sorted[i];

                CopyDrawRequest(ref displayPixels, draw.pixelData, draw.x, draw.y, draw.width, draw.height, _width, draw.colorOffset);

            }

            // Reset clear flag
            clearFlag = false;
            clearUIFlag = false;
            
            // Reset Draw Requests after they have been processed
            ResetDrawCalls();
            
        }

        private int[] tmpTilemapCache = new int[0];
        
        public void DrawTilemap(int x = 0, int y = 0, int columns = 0, int rows = 0)
        {
 
            if (tilemapChip.invalid)
            {
                tilemapChip.RebuildCache(tilemapChip.cachedTileMap);
            }
            
            var width = columns == 0 ? _width : columns * tilemapChip.tileWidth;

            if ((width + x) > _width)
            {
                width = _width - x;
            }
            
            var height = rows == 0 ? _height : rows * tilemapChip.tileHeight;
            
            if ((height + y) > _height)
            {
                height = _height - y;
            }
            
            // Flip the y scroll value
            var sY = tilemapChip.realHeight - height - scrollY;
            
            
            tilemapChip.cachedTileMap.GetPixels(scrollX, sY, width, height, ref tmpTilemapCache);

            y = _height - height - y;
            
            NewDrawCall(tmpTilemapCache, x, y, width, height, false, false, false, DrawMode.TilemapCache);
        }

        /// <summary>
        ///     This clears the display. It will write a background color from the
        ///     <see cref="ScreenBufferChip" /> into the internal
        ///     screenBufferData or us 0 if no <see cref="ScreenBufferChip" /> is
        ///     found.
        /// </summary>
        /// <summary>
        ///     This triggers the renderer to clear an area of the display.
        /// </summary>
        public void Clear()
        {
            clearFlag = true;
        }

        public void ClearUILayer()
        {
            // TODO this is immediate and needs to be part of the draw call stack
            uiLayer.Clear();
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
        /// <param name="layerOrder"></param>
        /// <param name="masked"></param>
        /// <param name="colorOffset"></param>
        public void NewDrawCall(int[] pixelData, int x, int y, int width, int height, bool flipH, bool flipV, bool flipY, DrawMode drawMode = DrawMode.Sprite, bool masked = false, int colorOffset = 0)
        {

            // flip y coordinate space
            if (flipY)
                y = _height - engine.spriteChip.height - y;

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
                draw.drawMode = drawMode;
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
            Clear();
            
            // Resize UI Layer
            uiLayer.Resize(_width, _height);
            uiLayer.Clear();
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

            scrollX = 0;
            scrollY = 0;
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

        public void DrawToUI(int[] pixelData, int x, int y, int width, int height, bool flipH = false, bool flipV = false, int colorOffset = 0)
        {
            
            // Update pixel data
            
            y = _height - height - y;

            if (flipH || flipV)
                SpriteChipUtil.FlipSpriteData(ref pixelData, width, height, flipH, flipV);

            CopyDrawRequest(ref uiLayer.pixels, pixelData, x, y, width, height, _width, colorOffset);

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