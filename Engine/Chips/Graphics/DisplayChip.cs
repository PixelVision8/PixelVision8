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
using System.Collections.Generic;
using PixelVisionSDK.Utils;
using UnityEngine.WSA;

namespace PixelVisionSDK.Chips
{

    public class DisplayChip : AbstractChip, IDraw
    {

        protected readonly TextureData textureData = new TextureData(0, 0);
        protected int _height = 240;
        protected int _maxSpriteCount = 64;
        protected int _scrollX;
        protected int _scrollY;
        protected int _width = 256;
        protected ILayer[] backgroundLayer = new ILayer[0];
        protected bool clearFlag;
        protected bool copyScreenBuffer;
        protected int currentSprites;
        protected List<DrawRequest> drawRequestPool = new List<DrawRequest>();
        protected List<DrawRequest> drawRequests = new List<DrawRequest>();
        protected bool drawTilemapFlag;
        protected int maxLayer = 1;
        protected int minLayer = -1;
        protected int offscreenPaddingX = 8;
        protected int offscreenPaddingY = 8;
        protected int[] tmpBufferData;
        protected int[] tmpDisplayData = new int[0];
        protected int[] tmpTileData = new int[0];

        /// <summary>
        ///     The width of the area to sample from in the ScreenBufferChip. If
        ///     width of the view port is larger than the <see cref="TextureData" />
        ///     it will wrap.
        /// </summary>
        public int viewPortHeight = 240;

        /// <summary>
        ///     This represents the x position on the screen where the
        ///     ScreenBufferChip's view port should be rendered to on the display. 0
        ///     is the left of the screen.
        /// </summary>
        public int viewPortOffsetX;

        /// <summary>
        ///     This represents the y position on the screen where the
        ///     ScreenBufferChip's view port should be rendered to on the display. 0
        ///     is the top of the screen.
        /// </summary>
        public int viewPortOffsetY;

        /// <summary>
        ///     The height of the area to sample from in the ScreenBufferChip. If
        ///     width of the view port is larger than the <see cref="TextureData" />
        ///     it will wrap.
        /// </summary>
        public int viewPortWidth = 256;


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
        public bool wrapMode
        {
            get { return textureData.wrapMode; }
            set { textureData.wrapMode = value; }
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
        public bool paused { get; set; }

        /// <summary>
        ///     Returns the raw pixel data that represents what the display should look
        ///     like. Use this data to render to the display.
        /// </summary>
        public int[] displayPixelData
        {
            get
            {
                textureData.GetPixels(0, 0, width, height, ref tmpDisplayData);
                return tmpDisplayData; //textureData.GetPixels();
            }
        }

        protected ScreenBufferChip bufferChip
        {
            get { return engine.screenBufferChip; }
        }

        protected TileMapChip tilemapChip
        {
            get { return engine.tileMapChip; }
        }

        protected int backgroundColor
        {
            get { return engine.colorChip != null ? engine.colorChip.backgroundColor : -1; }
        }

        /// <summary>
        /// </summary>
        public void Draw()
        {
            // TODO need to render sprites under background

            // At the beginning of the draw call, see if the texture should be cleared
            if (clearFlag)
            {
                if (clearDrawRequest != null)
                {
                    var totalPixels = width * height;
                    var pixels = new int[totalPixels];

                    var color = clearDrawRequest.transparent;

                    for (int i = 0; i < totalPixels; i++)
                    {
                        pixels[i] = color;
                    }

                    clearDrawRequest.pixelData = pixels;

                    textureData.SetPixels(clearDrawRequest.x, clearDrawRequest.y, clearDrawRequest.width, clearDrawRequest.height, pixels);
                }
                else
                {
                    textureData.Clear(backgroundColor);
                }
                
                clearFlag = false;
            }

            if (copyScreenBuffer)
            {

                // Update the scroll position
                bufferChip.scrollX = scrollX;
                bufferChip.scrollY = scrollY;

                // Test to see if the buffer chip has been invalidated
                if (bufferChip.invalid)
                {

                    // Copy out the new pixel data
                    bufferChip.ReadPixelData(width, height, ref tmpBufferData);

                    // Reset the invalidation since we got the latest pixel data
                    bufferChip.ResetValidation();
                }

                // Copy all the buffer pixel data over into the display's texture data.
                textureData.MergePixels(0, 0, width, height, tmpBufferData, false, backgroundColor);


                // Reset the copy buffer flag
                copyScreenBuffer = false;
            }

            


            if (drawTilemapFlag)
            {

                tilemapChip.scrollX = scrollX;
                tilemapChip.scrollY = scrollY;

                var sC = tilemapDrawRequest.x;
                var sR = tilemapDrawRequest.y;
                var tmpWidth = tilemapDrawRequest.width;
                var tmpHeight = tilemapDrawRequest.height;

                if (tilemapChip.invalid)
                {
                    tilemapChip.ReadPixelData(tmpWidth, tmpHeight, ref tmpTileData, sC, sR);


                    tilemapChip.ResetValidation();
                }

                textureData.MergePixels(tilemapDrawRequest.offsetX, tilemapDrawRequest.offsetY, tmpWidth, tmpHeight, tmpTileData);

                drawTilemapFlag = false;
            }

            

            //var draws = drawRequests; // TODO need to use linq to find the order

            // TODO render sprites above the background
            var total = drawRequests.Count;
            for (var i = 0; i < total; i++)
            {
                var draw = drawRequests[i];
                draw.MergePixelData(textureData);
            }

            ResetDrawCalls();
        }

        protected DrawRequest clearDrawRequest;

        /// <summary>
        ///     This clears the display. It will write a background color from the
        ///     <see cref="ScreenBufferChip" /> into the internal
        ///     screenBufferData or us 0 if no <see cref="ScreenBufferChip" /> is
        ///     found.
        /// </summary>
        public void Clear()
        {
            clearFlag = true;

            // TODO this is a strange dependency I need to look into. Throws an error on startup
            //textureData.Clear(engine.screenBufferChip != null ? engine.screenBufferChip.backgroundColor : 0);
        }

        public void ClearArea(int x, int y, int width, int height, int color = -1)
        {
            clearFlag = true;

            // Create new clear draw request instance
            if (clearDrawRequest == null)
                clearDrawRequest = new DrawRequest();

            // Configure the clear draw request
            clearDrawRequest.x = x;
            clearDrawRequest.y = _height - height - y;
            clearDrawRequest.width = width;
            clearDrawRequest.height = height;
            clearDrawRequest.transparent = color == -1 ? backgroundColor : color;

        }

        public void CopyScreenBlockBuffer()
        {

            // If there is no buffer chip then ignore the request
            if (bufferChip == null)
                return;

            // Set the copy buffer flag
            copyScreenBuffer = true;

        }

        /// <summary>
        ///     Resets the display chip and clears all of the pixel data.
        /// </summary>
        public override void Reset()
        {
            Clear();
        }

        /// <summary>
        ///     Returns a bool if the Display has enough draw calls left to
        ///     render a sprite.
        /// </summary>
        /// <returns></returns>
        public bool CanDraw()
        {
            return currentSprites < maxSpriteCount;
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
        public void NewDrawCall(int[] pixelData, int x, int y, int width, int height, bool flipH, bool flipV, bool flipY, int layerOrder = 0, bool masked = false)
        {
            var drawCalls = width / engine.spriteChip.width * (height / engine.spriteChip.height);

            //currentSprites += drawCalls;

            if (currentSprites + drawCalls > maxSpriteCount)
                return;

            currentSprites += drawCalls;

            //TODO need to add in layer merge logic, -1 is behind, 0 is normal, 1 is above

            //layerOrder = layerOrder.Clamp(-1, 1);

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
                drawRequests.Add(draw);

                //texturedata.MergePixels(x, y, width, height, pixelData);
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

            // Resize data structures
            textureData.Resize(_width, _height);

            tmpBufferData = new int[_width * _height];
            tmpDisplayData = new int[_width * _height];
        }

        protected DrawRequest tilemapDrawRequest;

        public void DrawTilemap(int startCol, int startRow, int columns, int rows, int offsetX = 0, int offsetY = 0)
        {
            drawTilemapFlag = true;

            if(tilemapDrawRequest == null)
                tilemapDrawRequest = new DrawRequest();

            var tileWidth = tilemapChip.tileWidth;
            var tileHeight = tilemapChip.tileHeight;

            tilemapDrawRequest.x = startCol * tileWidth;
            tilemapDrawRequest.y = startRow * tileHeight;
            tilemapDrawRequest.width = columns * tileWidth;
            tilemapDrawRequest.height = rows * tileHeight;
            tilemapDrawRequest.offsetX = offsetX;
            tilemapDrawRequest.offsetY = _height - tilemapDrawRequest.height - offsetY;

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

            // Get the target raw image from the engine
            //target = engine.renderTarget;

            // TODO Need to set the display from the engine
            //maxSpriteCount = 64;
            //autoClear = false;
            wrapMode = true;
            ResetResolution(256, 240);

            viewPortOffsetX = 0;
            viewPortOffsetY = 0;
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
            currentSprites = 0;

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

    }
    
}