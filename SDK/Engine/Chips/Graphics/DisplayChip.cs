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

namespace PixelVision8.Engine.Chips
{
    public class DisplayChip : AbstractChip, IDraw
    {
        protected int _height = 240;

        protected int _width = 256;

        private Color[] cachedColors;

        private DrawRequest draw;
        protected List<DrawRequest>[] drawRequestLayers = new List<DrawRequest>[0];
        protected Stack<int[]> drawRequestPixelDataPool = new Stack<int[]>();
        public Color[] pixels = new Color[0];
        public int totalPixels;
        public int overscanX { get; set; }
        public int overscanY { get; set; }

        public int layers
        {
            get => drawRequestLayers.Length;
            set
            {
                Array.Resize(ref drawRequestLayers, value);
                for (var i = value - 1; i > -1; i--)
                {
                    var requests = drawRequestLayers[i];
                    if (requests == null)
                        drawRequestLayers[i] = new List<DrawRequest>();
                    else
                        requests.Clear();
                }
            }
        }

        public int overscanXPixels => overscanX * engine.spriteChip.width;

        public int overscanYPixels => overscanY * engine.spriteChip.height;

//        public bool displayMaskColor;

        /// <summary>
        ///     This returns the visble areas sprites should be displayed on. Note that x and y may be negative if overscan is set
        ///     since the screen wraps.
        /// </summary>
        public Rectangle visibleBounds => new Rectangle(-overscanXPixels, -overscanYPixels, width - overscanXPixels,
            height - overscanYPixels);

        /// <summary>
        ///     Returns the display's <see cref="width" />
        /// </summary>
        public int width => _width;

        /// <summary>
        ///     Returns the display's <see cref="height" />
        /// </summary>
        public int height => _height;

        /// <summary>
        /// </summary>
        public void Draw()
        {
            cachedColors = engine.colorChip.colors;

            // Loop through all draw requests
            for (var layer = 0; layer < drawRequestLayers.Length; layer++)
            {
                // TODO need to add back in support for turning layers on and off

                var drawRequests = drawRequestLayers[layer];
                var totalDR = drawRequests.Count;
                for (var i = 0; i < totalDR; i++)
                {
                    var draw = drawRequests[i];

                    CopyDrawRequest(draw.isRectangle ? null : draw.pixelData, draw.x, draw.y, draw.width, draw.height,
                        draw.flipH, draw.flipV, draw.colorOffset);
                }
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
        public void NewDrawCall(int[] pixelData, int x, int y, int width, int height, int layer = 0, bool flipH = false,
            bool flipV = false, int colorOffset = 0)
        {
            if (layer >= layers)
            {
                // This can happen as the old system wasn't very strict.
                // TODO: Handle "out of bounds" layer accesses properly!
                var sizeOld = layers;
                Array.Resize(ref drawRequestLayers, layer + 1);
                for (var i = layers - 1; i >= sizeOld; i--) drawRequestLayers[i] = new List<DrawRequest>();
            }

            draw = NextDrawRequest();
            draw.x = x;
            draw.y = y;
            draw.width = width;
            draw.height = height;
            draw.pixelData = pixelData;
            draw.flipH = flipH;
            draw.flipV = flipV;
            draw.colorOffset = colorOffset;
            drawRequestLayers[layer].Add(draw);
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

            Array.Resize(ref pixels, totalPixels);
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

            // By default set the total layers to the DrawModes minus Tilemap Cache which isn't used for rendering
            layers = Enum.GetNames(typeof(DrawMode)).Length - 1;
        }

        public override void Deactivate()
        {
            base.Deactivate();
            engine.displayChip = null;
        }

        public void ResetDrawCalls()
        {
            // Reset all draw requests
            for (var layer = drawRequestLayers.Length - 1; layer > -1; layer--)
            {
                var drawRequests = drawRequestLayers[layer];

                for (var i = drawRequests.Count - 1; i > -1; i--)
                {
                    var request = drawRequests[i];
                    drawRequestPixelDataPool.Push(request.pixelData);
                }

                drawRequests.Clear();
            }
        }

        public DrawRequest NextDrawRequest()
        {
            var request = new DrawRequest();

            if (drawRequestPixelDataPool.Count > 0)
                request.pixelData = drawRequestPixelDataPool.Pop();
            else
                request.pixelData = new int[0];

            return request;
        }

        public void CopyDrawRequest(int[] pixelData, int x, int y, int width, int height, bool flipH = false,
            bool flipV = false, int colorOffset = 0)
        {
            int total;
            int srcX;
            int srcY;
            int colorID;
            int i;
            int index;

            total = width * height;


            for (i = 0; i < total; i++)
            {
                colorID = pixelData?[i] ?? 0;

                if (colorID > -1)
                {
                    if (colorOffset > 0)
                        colorID += colorOffset;

                    srcX = i % width;
                    srcY = i / width;

                    if (flipH)
                        srcX = width - 1 - srcX;
                    if (flipV)
                        srcY = height - 1 - srcY;

                    srcX += x;
                    srcY += y;

                    // Make sure x & y are wrapped around the display
                    // Note: + size and the second modulo operation are required to get wrapped values between 0 and +size
                    var size = _height;
                    srcY = (srcY % size + size) % size;
                    size = _width;
                    srcX = (srcX % size + size) % size;
                    // size is still == _width from the previous operation - let's reuse the local

                    // Find the index
                    index = srcX + size * srcY;

                    // Set the pixel
                    pixels[index] = cachedColors[colorID];
                }
            }
        }

        public Color[] VisiblePixels()
        {
            var pixels = engine.displayChip.pixels;

            var displaySize = engine.gameChip.Display();

            var visibleWidth = displaySize.X;
            var visibleHeight = displaySize.Y;
            var width = engine.displayChip.width;

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
                    newPixels[index] = pixels[i];
                    index++;
                }
            }

            return newPixels;
        }

    }
}