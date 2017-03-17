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
using System.Diagnostics;
using System.Linq;
using PixelVisionSDK.Utils;

namespace PixelVisionSDK.Chips
{
    public class DisplayChip : AbstractChip, IDraw
    {
        protected readonly TextureData textureData = new TextureData(0, 0);
        protected int _height = 240;
        protected int _maxSpriteCount = 64;
        protected int _width = 256;
        protected int currentSprites;
        protected int[] tmpBufferData;
        protected int offscreenPaddingX = 8;
        protected int offscreenPaddingY = 8;
        protected int minLayer = -1;
        protected int maxLayer = 1;

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
        public int[] tmpDisplayData = new int[0];

        /// <summary>
        ///     A flag to define if the display auto clears after each frame.
        /// </summary>
//        public bool autoClear { get; set; }

        //int[] tmpPixels = new int[0];

        /// <summary>
        ///     Returns the raw pixel data that represents what the display should look
        ///     like. Use this data to render to the display.
        /// </summary>
        public int[] displayPixelData
        {
            get
            {
                //textureData.GetPixels(0, 0, _width, _height, tmpPixels);
//                var total = _width * _height;
//                if (tmpBufferData.Length != total)
//                {
//                    Array.Resize(ref tmpBufferData, total);
//                    UnityEngine.Debug.Log("Resize tmpPixels");        
//                }
                //var copy = new int[total];

                textureData.GetPixels(0, 0, width, height, tmpDisplayData);
                return tmpDisplayData;//textureData.GetPixels();
            }
        }


        protected bool clearFlag;

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

        protected bool copyScreenBuffer;

        protected ScreenBufferChip bufferChip
        {
            get { return engine.screenBufferChip; }
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
        public void NewDrawCall(int[] pixelData, int x, int y, int width, int height, bool flipH, bool flipV, bool flipY,
            int layerOrder = 0,
            bool masked = false)
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
        }

        public override void Deactivate()
        {
            base.Deactivate();
            engine.displayChip = null;
        }

        protected List<DrawRequest> drawRequests = new List<DrawRequest>();
        protected List<DrawRequest> drawRequestPool = new List<DrawRequest>();

        /// <summary>
        /// </summary>
        public void Draw()
        {
            // At the beginning of the draw call, see if the texture should be cleared
            if (clearFlag)
            {
                //UnityEngine.Debug.Log("Clear");

                textureData.Clear(engine.screenBufferChip != null ? engine.screenBufferChip.backgroundColor : 0);
                clearFlag = false;
            }

            // TODO need to render sprites under background

            if (copyScreenBuffer)
            {

                if (bufferChip.invalid)
                {
                    bufferChip.ReadScreenData(width, height, ref tmpBufferData);

                    bufferChip.ResetInvalidation();
                }

                textureData.MergePixels(0, 0, width, height, tmpBufferData, false, bufferChip.backgroundColor);

                copyScreenBuffer = false;
            }

            //var draws = drawRequests; // TODO need to use linq to find the order

            // TODO render sprites above the background
            var total = drawRequests.Count;
            for (int i = 0; i < total; i++)
            {
                var draw = drawRequests[i];
                draw.MergePixelData(textureData);
            }

            ResetDrawCalls();
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
    
    public class DrawRequest
    {
        public int order;
        
        protected int[] _pixelData = new int[0];

        public int[] pixelData
        {
            get { return _pixelData; }
            set
            {
                var totalPixels = value.Length;

                if (_pixelData.Length != totalPixels)
                {
                    Array.Resize(ref _pixelData, totalPixels);
                }

                Array.Copy(value, _pixelData, totalPixels);
            }
        }

        public int x;
        public int y;
        public int width;
        public int height;
        public bool masked = true;
        public int transparent = -1;

        public virtual void MergePixelData(TextureData target)
        {
            target.MergePixels(x, y, width, height, pixelData, masked, transparent);
        }
    }

}