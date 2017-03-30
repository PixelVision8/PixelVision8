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

namespace PixelVisionSDK.Chips
{

    public class DisplayChip : AbstractChip, IDraw
    {

        protected int _height = 240;
        protected int _maxSpriteCount = 64;
        protected int _scrollX;
        protected int _scrollY;
        protected int _width = 256;
        protected bool _clearFlag;

        public bool autoClear;

        protected bool clearFlag
        {
            get { return autoClear || _clearFlag; }
            set { _clearFlag = value; }
        }

        protected bool copyScreenBuffer;
        protected int currentSprites;
        protected List<DrawRequest> drawRequestPool = new List<DrawRequest>();
        protected List<DrawRequest> drawRequests = new List<DrawRequest>();
        protected bool drawTilemapFlag;
        protected DrawRequest tilemapDrawRequest;

        protected int _overscanX;
        protected int _overscanY;

        public int overscaneX
        {
            get { return _overscanX * 8; }
            protected set { _overscanX = value; }
        }

        public int overscaneY
        {
            get { return _overscanY * 8; }
            protected set { _overscanY = value; }
        }

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
        public bool wrapMode { get; set; }

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

        protected TileMapChip tilemapChip
        {
            get { return engine.tileMapChip; }
        }

        protected int backgroundColor
        {
            get { return engine.colorChip != null ? engine.colorChip.backgroundColor : -1; }
        }

        public void DrawTilemap(int x, int y, int columns, int rows)
        {
            // Set the draw flag to true
            drawTilemapFlag = true;

            if (tilemapDrawRequest == null)
                tilemapDrawRequest = new DrawRequest();

            // Get map's tile size
            var tileWidth = tilemapChip.tileWidth;
            var tileHeight = tilemapChip.tileHeight;

            // Convert tile width and height to pixel width and height
            tilemapDrawRequest.width = columns * tileWidth;
            tilemapDrawRequest.height = rows * tileHeight;

            // save the starting x,y position to render the map on the screen
            tilemapDrawRequest.x = x;
            tilemapDrawRequest.y = y;

        }

        public int[] displayPixels = new int[0];
        public int[] displayMask = new int[0];
        //private int[] cachedTilemapPixels = new int[0];
        
        protected int clLeft = -1;
        protected int clTop = -1;
        protected int clRight = -1;
        protected int clBottom = -1;

        /// <summary>
        /// </summary>
        public void Draw()
        {
            int displayX, displayY, mapX, mapY, mapPixelIndex;
            int colorID = -1;

            int mapWidth = tilemapChip.realWidth;
            int mapHeight = tilemapChip.realHeight;
            int width = _width;
            int tileColor;

            // Get a reference to the complete tilemap's cached pixel data;
            var cachedTilemap = tilemapChip.cachedTilemapPixels;

            var totalMapPixels = cachedTilemap.Length;

            // Get the current clear flag value
            var clearViewport = clearFlag;

            // Set up the clear boundaries
            var clLeft = -1;
            var clTop = -1;
            var clRight = -1;
            var clBottom = -1;

            // If Clear view port is true, resize the bounds to the clear draw request
            if (clearViewport)
            {
                clLeft = clearDrawRequest.x.Clamp(0, _width);
                clTop = clearDrawRequest.y.Clamp(0, _width);
                clRight = (clLeft + clearDrawRequest.width).Clamp(0, _width);
                clBottom = (clTop + clearDrawRequest.height).Clamp(0, _width);
                //Debug.Log("Clear Bounds clLeft " + clLeft + " clTop " + clTop + " clRight " + clRight + " clBottom " + clBottom);
            }

            // Flag to tell if we should draw the tilemap
            var tilemapViewport = false;

            // Set up the map position
            var tmLeft = -1;
            var tmTop = -1;
            var tmRight = -1;
            var tmBottom = -1;

            // If tilemap draw request exists, configure the coordinates
            if (tilemapDrawRequest != null)
            {
                tmLeft = tilemapDrawRequest.x.Clamp(0, _width);
                tmTop = tilemapDrawRequest.y.Clamp(0, _width);
                tmRight = (tmLeft + tilemapDrawRequest.width).Clamp(0, _width);
                tmBottom = (tmTop + tilemapDrawRequest.height).Clamp(0, _height);
            }
            
            // A flag to determine if we need to draw the pixel or not
            bool setPixel;

            // Get a local reference to the total number pixels in the display
            var total = totalPixels;

            // Setup the display mask
            if (displayMask.Length != total)
            {
                Array.Resize(ref displayMask, total);
            }

            // Create floats for these values since we use it to repeate later on
            var mwF = (float) mapWidth;
            var mhF = (float) mapHeight;

            // Loop through each of the pixels in the display
            for (int i = 0; i < total; i++)
            {
               
                // Calculate current display x,y position
                displayX = (i % width); // TODO if we don't repeat this it draws matching pixels off by 1 on Y axis?
                displayY = (i / width);

                // Calculate map position
                mapX = (displayX - tmLeft) + scrollX;
                mapY = displayY + tmTop + (mapHeight - height) - scrollY;
                
                // Flip Y for display to draw clear correctly
                displayY = height - displayY - 1;

                // Calculate if x,y is within the clear boundaries
                clearViewport = (displayX >= clLeft && displayX <= clRight && displayY >= clTop && displayY <= clBottom);
                tilemapViewport = (displayX >= tmLeft && displayX <= tmRight && displayY >= tmTop && displayY <= tmBottom);
                
                // Check to see if we need to clear the display
                if (clearViewport)
                {
                    colorID = -1;
                    setPixel = true;
                }
                else
                {
                    setPixel = false;
                }

                // Check to see if we need to draw the tilemap
                if (tilemapViewport)
                {
                    // Wrap the map's x,y position
                    mapX = (int)(mapX - Math.Floor(mapX / mwF) * mwF);
                    mapY = (int)(mapY - Math.Floor(mapY / mhF) * mhF);

                    // Calculate the map pixel index
                    mapPixelIndex = mapX + mapY * mapWidth;
                    
                    // Find the color for the tile's pixel
                    tileColor = cachedTilemap[mapPixelIndex];
                    
                    // If there is a pixel color, set it to the colorID and flag to draw the pixel
                    if (tileColor > -1)
                    {
                        colorID = tileColor;
                        setPixel = true;
                    }
                    
                }

                // If there is a pixel to draw, set it
                if (setPixel)
                {
                    displayPixels[i] = colorID;
                }

                // Always set the color to the mask. -1 will be empty pixels, everything else is ignored
                displayMask[i] = colorID;

            }

            // Loop through all draw requests
            var totalDR = drawRequests.Count;

            for (var i = 0; i < totalDR; i++)
            {
                var draw = drawRequests[i];
                draw.DrawPixels(ref displayPixels, width, height, wrapMode, displayMask);
            }

            // Reset clear flag
            clearFlag = false;

            // Reset Draw Requests after they have been processed
            ResetDrawCalls();
        }

        protected DrawRequest clearDrawRequest;

        /// <summary>
        ///     This clears the display. It will write a background color from the
        ///     <see cref="ScreenBufferChip" /> into the internal
        ///     screenBufferData or us 0 if no <see cref="ScreenBufferChip" /> is
        ///     found.
        /// </summary>
        //        public void Clear()
        //        {
        //            clearFlag = true;
        //
        //            // TODO this is a strange dependency I need to look into. Throws an error on startup
        //            //textureData.Clear(engine.screenBufferChip != null ? engine.screenBufferChip.backgroundColor : 0);
        //        }

        /// <summary>
        ///     This triggers the renderer to clear an area of the display.
        /// </summary>
        public void ClearArea(int x, int y, int blockWidth, int blockHeight, int color = -1)
        {

            // Create new clear draw request instance
            if (clearDrawRequest == null)
                clearDrawRequest = new DrawRequest();

            // Configure the clear draw request
            clearDrawRequest.x = x;
            clearDrawRequest.y = y;
            clearDrawRequest.width = blockWidth;
            clearDrawRequest.height = blockHeight;
            clearDrawRequest.transparent = color;

            clearFlag = true;

            //Debug.Log("Clear Request " + clearDrawRequest.x + " "+ clearDrawRequest.y + " "+ clearDrawRequest.width + " "+ clearDrawRequest.height + " " + clearFlag);
        }

        /// <summary>
        ///     Resets the display chip and calls clear for the next render pass.
        /// </summary>
        public override void Reset()
        {
            ClearArea(0, 0, width, height);
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
        /// <param name="colorOffset"></param>
        public void NewDrawCall(int[] pixelData, int x, int y, int width, int height, bool flipH, bool flipV, bool flipY, int layerOrder = 0, bool masked = false, int colorOffset = 0)
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
                draw.order = layerOrder;
                draw.colorOffset = colorOffset;
                drawRequests.Add(draw);
                

                //texturedata.MergePixels(x, y, width, height, pixelData);
            }
        }

        private int totalPixels = 0;

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
            Array.Resize(ref displayMask, totalPixels);

            // Clear display mask
            for (int i = 0; i < totalPixels; i++)
            {
                displayMask[i] = -1;
            }

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