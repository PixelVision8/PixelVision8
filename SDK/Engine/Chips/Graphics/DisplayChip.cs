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
        private Color[] cachedColors;

        private DrawRequest draw;
        protected List<DrawRequest>[] DrawRequestLayers = new List<DrawRequest>[0];
        protected Stack<int[]> DrawRequestPixelDataPool = new Stack<int[]>();
        public Color[] Pixels = new Color[0];
        public int TotalPixels;
        public int OverscanX { get; set; }
        public int OverscanY { get; set; }

        public int layers
        {
            get => DrawRequestLayers.Length;
            set
            {
                Array.Resize(ref DrawRequestLayers, value);
                for (var i = value - 1; i > -1; i--)
                {
                    var requests = DrawRequestLayers[i];
                    if (requests == null)
                        DrawRequestLayers[i] = new List<DrawRequest>();
                    else
                        requests.Clear();
                }
            }
        }

        public int OverscanXPixels => OverscanX * engine.SpriteChip.width;

        public int OverscanYPixels => OverscanY * engine.SpriteChip.height;

        //        public bool displayMaskColor;

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


        private List<DrawRequest> _drawRequests;
        private int _totalDR;
        private int _layer;
        private int _i;
        private DrawRequest _drawRequest;

        /// <summary>
        /// </summary>
        public void Draw()
        {
            cachedColors = engine.ColorChip.colors;

            // Loop through all draw requests
            for (_layer = 0; _layer < DrawRequestLayers.Length; _layer++)
            {
                // TODO need to add back in support for turning layers on and off

                _drawRequests = DrawRequestLayers[_layer];
                _totalDR = _drawRequests.Count;
                for (_i = 0; _i < _totalDR; _i++)
                {
                    _drawRequest = _drawRequests[_i];

                    CopyDrawRequest(_drawRequest.isRectangle ? null : _drawRequest.pixelData, _drawRequest.x, _drawRequest.y, _drawRequest.width, _drawRequest.height,
                        _drawRequest.flipH, _drawRequest.flipV, _drawRequest.colorOffset);
                }
            }

            // Reset Draw Requests after they have been processed
            ResetDrawCalls();
        }

        private int _oldSize;

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
                _oldSize = layers;
                Array.Resize(ref DrawRequestLayers, layer + 1);
                for (_i = layers - 1; _i >= _oldSize; _i--) DrawRequestLayers[_i] = new List<DrawRequest>();
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
            DrawRequestLayers[layer].Add(draw);
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
            layers = Enum.GetNames(typeof(DrawMode)).Length - 1;
        }

        public override void Deactivate()
        {
            base.Deactivate();
            engine.DisplayChip = null;
        }

        public void ResetDrawCalls()
        {
            // Reset all draw requests
            for (var layer = DrawRequestLayers.Length - 1; layer > -1; layer--)
            {
                var drawRequests = DrawRequestLayers[layer];

                for (var i = drawRequests.Count - 1; i > -1; i--)
                {
                    var request = drawRequests[i];
                    DrawRequestPixelDataPool.Push(request.pixelData);
                }

                drawRequests.Clear();
            }
        }

        public DrawRequest NextDrawRequest()
        {
            var request = new DrawRequest();

            if (DrawRequestPixelDataPool.Count > 0)
                request.pixelData = DrawRequestPixelDataPool.Pop();
            else
                request.pixelData = new int[0];

            return request;
        }

        int _total;
        int _srcX;
        int _srcY;
        int _colorID;
        int i1;
        int _index;

        public void CopyDrawRequest(int[] pixelData, int x, int y, int width, int height, bool flipH = false,
            bool flipV = false, int colorOffset = 0)
        {
            // int total;
            // int srcX;
            // int srcY;
            // int colorID;
            // // int i;
            // int index;

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
                    var size = this.Height;
                    _srcY = (_srcY % size + size) % size;
                    size = this.Width;
                    _srcX = (_srcX % size + size) % size;
                    // size is still == _width from the previous operation - let's reuse the local

                    // Find the index
                    _index = _srcX + size * _srcY;

                    // Set the pixel
                    Pixels[_index] = cachedColors[_colorID];
                }
            }
        }

        public Color[] VisiblePixels()
        {
            var pixels = engine.DisplayChip.Pixels;

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
                    newPixels[index] = pixels[i];
                    index++;
                }
            }

            return newPixels;
        }

    }
}